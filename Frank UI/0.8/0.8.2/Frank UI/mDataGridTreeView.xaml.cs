using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Data;

namespace Frank_UI
{
    /// <summary>
    /// Interaction logic for mDataGridTreeView.xaml
    /// </summary>
    public partial class mDataGridTreeView : UserControl
    {
        #region Dependency Properties

        private static readonly DependencyPropertyKey ColumnsPropertyKey = DependencyProperty.RegisterAttachedReadOnly("Columns", typeof(ObservableCollection<DataGridColumn>), typeof(DataGrid),
            new FrameworkPropertyMetadata(new ObservableCollection<DataGridColumn>(), FrameworkPropertyMetadataOptions.AffectsRender));
        public static readonly DependencyProperty ColumnsProperty = ColumnsPropertyKey.DependencyProperty;

        public ObservableCollection<DataGridColumn> Columns
        {
            get
            {
                return grd.Columns;
            }
        }
        #endregion

        public event RoutedEventHandler TreeViewItem_Expanded;
        public event RoutedEventHandler TreeViewItem_Collapsed;
        public event MouseButtonEventHandler TreeViewItem_MouseRightButtonDown;


        ObservableCollection<Dictionary<string, object>> ItemCollection = new ObservableCollection<Dictionary<string, object>>();
        CollectionViewSource cvs;

        public int NumberOfItems
        {
            get
            {
                return ItemCollection.Count;
            }
        }

        //Key is the column key, value is the filter string for current column
        //public readonly Dictionary<string, string> FilterStrings = new Dictionary<string, string>();
        public readonly DataTable FilterStrings = new DataTable();

        public mDataGridTreeView()
        {            
            InitializeComponent();
            cvs = new CollectionViewSource();
            cvs.Source = ItemCollection;
            cvs.Filter += Cvs_Filter;
            grd.ItemsSource = cvs.View;
            FilterStrings.Columns.Add("Key", typeof(String));
            FilterStrings.Columns.Add("Filter", typeof(String));
            FilterStrings.Columns.Add("DataType", typeof(Type));
            FilterStrings.PrimaryKey = new DataColumn[] { FilterStrings.Columns["Key"] };

        }

        //this event is called, when DataGrid is refreshed. It will be called for every single item in the DataGrid
        private void Cvs_Filter(object sender, FilterEventArgs e)
        {
            Dictionary<string, object> dgItem = e.Item as Dictionary<string, object>;
            bool acceptasterisc = false;
            bool acceptpoints = true;
            if (FilterStrings.Rows.Count == 0)
                e.Accepted = true;
            else
            {
                //run through every single filter and see if it applies on dgItem
                foreach (DataRow filterstring in FilterStrings.Rows)
                {
                    if (filterstring["Filter"] != null && (string)filterstring["Filter"] != "")
                    {
                        string key = (string)filterstring["Key"];
                        string filter = (string)filterstring["Filter"];
                        string[] filter_segments = filter.Split(new string[] { ".." }, StringSplitOptions.RemoveEmptyEntries);
                        Type datatype = (Type)filterstring["DataType"];

                        if (datatype == typeof(string))
                        {
                            string itemValue = (string)dgItem[key];

                            #region Handle *-character
                            if (filter.StartsWith("*"))
                                acceptasterisc = itemValue.EndsWith(filter.Trim('*'));
                            else if (filter.EndsWith("*"))
                                acceptasterisc = itemValue.StartsWith(filter.Trim('*'));
                            else if (filter.Contains("*"))
                            {
                                var seg1 = filter.Split('*')[0];
                                var seg2 = filter.Split('*')[1];
                                acceptasterisc = itemValue.StartsWith(seg1) && itemValue.EndsWith(seg2);
                            }
                            #endregion

                            #region Hanfle ..-characters
                            if (filter_segments.Length == 2)
                            {
                                acceptpoints = acceptpoints & String.Compare(filter_segments[0], itemValue) <= 0 && String.Compare(itemValue, filter_segments[1]) <= 0;
                            }
                            else if (filter.Contains(".."))
                            {
                                if (filter.StartsWith(".."))
                                {
                                    acceptpoints = acceptpoints & String.Compare(itemValue, filter_segments[0]) <= 0;
                                }
                                else
                                {
                                    acceptpoints = acceptpoints & String.Compare(filter_segments[0], itemValue) <= 0;
                                }
                            }
                            else
                            {
                                acceptpoints = acceptpoints & itemValue.ToLower().Contains(filter.ToLower());
                            }
                            #endregion
                        }
                        if (datatype == typeof(DateTime))
                        {
                            DateTime itemValue = (DateTime)dgItem[key];

                            #region Hanfle ..-characters
                            if (filter_segments.Length == 2)
                            {
                                acceptpoints = acceptpoints & DateTime.Compare(DateTime.Parse(filter_segments[0]), itemValue) <= 0 && DateTime.Compare(itemValue, DateTime.Parse(filter_segments[1])) <= 0;
                            }
                            else if (filter.Contains(".."))
                            {
                                if (filter.StartsWith(".."))
                                {
                                    acceptpoints = acceptpoints & DateTime.Compare(itemValue, DateTime.Parse(filter_segments[0])) <= 0;
                                }
                                else
                                {
                                    acceptpoints = acceptpoints & DateTime.Compare(DateTime.Parse(filter_segments[0]), itemValue) <= 0;
                                }
                            }
                            else
                            {
                                acceptpoints = acceptpoints & DateTime.Compare(DateTime.Parse(filter), itemValue) == 0;
                            }
                            #endregion
                        }
                        if (datatype == typeof(int))
                        {
                            int itemValue = (int)dgItem[key];

                            #region Hanfle ..-characters
                            if (filter_segments.Length == 2)
                            {
                                acceptpoints = acceptpoints & int.Parse(filter_segments[0]) < itemValue && itemValue < int.Parse(filter_segments[1]);
                            }
                            else if (filter.Contains(".."))
                            {
                                if (filter.StartsWith(".."))
                                {
                                    acceptpoints = acceptpoints & itemValue < int.Parse(filter_segments[0]);
                                }
                                else
                                {
                                    acceptpoints = acceptpoints & int.Parse(filter_segments[0]) < itemValue;
                                }
                            }
                            else
                            {
                                acceptpoints = acceptpoints & int.Parse(filter) == itemValue;
                            }
                            #endregion
                        }
                    }
                    e.Accepted = acceptasterisc | acceptpoints;
                }
            }
        }

        public void Refresh()
        {
            cvs.View.Refresh();
            grd.Items.Refresh();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="TreeViewItemHeader"></param>
        /// <param name="TreeViewItemCollection"></param>
        /// <param name="values">Key represents the binding path, value represents the value</param>
        public void Add(object TreeViewItemHeader, List<object> TreeViewItemCollection, List<string> bindings, List<object> values)
        {
            Dictionary<string, object> dict = new Dictionary<string, object>();
            dict.Add("Key", TreeViewItemHeader);
            dict.Add("K0", TreeViewItemHeader);
            dict.Add("Items", TreeViewItemCollection);

            mDataGridColumn clmkey = (mDataGridColumn)Columns[0];
            clmkey.Binding = new Binding("[K0]");

            for (int i = 0; i < Columns.Count && i < bindings.Count && i < values.Count; i++)
            {
                try
                {
                    mDataGridColumn clm = (mDataGridColumn)Columns[i + 1];
                    clm.DataType = values[i].GetType();
                    Binding binding = new Binding("[" + bindings[i] + "]");
                    if (clm.DataType == typeof(DateTime))
                        binding.StringFormat = "dd.MM.yyyy";
                    clm.Binding = binding;
                }
                catch (InvalidCastException) { }
                dict.Add(bindings[i], values[i]);
            }
            dict.Add("Accepted", true);
            ItemCollection.Add(dict);
            cvs.View.Refresh();
        }

        private void grd_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
        }

        #region Das sind keine Event-Handler, sondern Event-Weiterleiter. Die Events werden von mResources zum Parent-Element dieses Objekt geleitet

        internal void TreeViewItemExpanded(object sender, RoutedEventArgs e)
        {
            if (TreeViewItem_Expanded != null)
                TreeViewItem_Expanded(sender, e);
        }

        internal void TreeViewItemCollapsed(object sender, RoutedEventArgs e)
        {
            if (TreeViewItem_Collapsed != null)
                TreeViewItem_Collapsed(sender, e);
        }

        internal new void MouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (TreeViewItem_MouseRightButtonDown != null)
                TreeViewItem_MouseRightButtonDown(sender, e);
        }
        #endregion

        public bool KeyExists(object key)
        {
            foreach (Dictionary<string, object> dict in grd.Items)
            {
                if (dict["Key"].ToString() == key.ToString())
                    return true;
            }
            return false;
        }

        public void Clear()
        {
            ItemCollection.Clear();
            Refresh();
        }

        private void grd_PreviewMouseWheel(object sender, MouseWheelEventArgs e)
        {
            ScrollViewer sv = GetVisualChild<ScrollViewer>(grd);
            sv.ScrollToVerticalOffset(sv.VerticalOffset - e.Delta/4);
            e.Handled = true;
        }

        private static T GetVisualChild<T>(DependencyObject parent) where T : Visual
        {
            T child = default(T);

            int numVisuals = VisualTreeHelper.GetChildrenCount(parent);
            for (int i = 0; i < numVisuals; i++)
            {
                Visual v = (Visual)VisualTreeHelper.GetChild(parent, i);
                child = v as T;
                if (child == null)
                {
                    child = GetVisualChild<T>(v);
                }
                if (child != null)
                {
                    break;
                }
            }
            return child;
        }

        private void grd_Sorting(object sender, DataGridSortingEventArgs e)
        {
            if (e.Column.GetType() == typeof(mDataGridColumn))
            {
                mDataGridColumn clm = (mDataGridColumn)e.Column;
                
                if (clm.Caption == "Liefertermin frühestens")
                {
                    e.Handled = true;

                    var direction = (clm.SortDirection != ListSortDirection.Ascending) ? ListSortDirection.Ascending : ListSortDirection.Descending;
                    clm.SortDirection = direction;


                    ListCollectionView lcv = (ListCollectionView)cvs.View;

                    switch (clm.SortDirection)
                    {
                        case ListSortDirection.Ascending: lcv.CustomSort = new DictionaryLieferterminDateTimeAscendingComparer(); break;
                        case ListSortDirection.Descending: lcv.CustomSort = new DictionaryLieferterminDateTimeDescendingComparer(); break;
                    }
                }
            }
        }

        public void ScrollInto(object key)
        {
            object item = null;
            foreach (Dictionary<string, object> dict in grd.Items)
            {
                if (dict["Key"].Equals(key))
                {
                    item = dict;
                    break;
                }
            }
            if (item != null)
                grd.ScrollIntoView(item);
        }
    }
}
