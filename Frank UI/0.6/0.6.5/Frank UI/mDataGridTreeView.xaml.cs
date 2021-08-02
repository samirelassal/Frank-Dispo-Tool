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

        private void Cvs_Filter(object sender, FilterEventArgs e)
        {
            Dictionary<string, object> dict = e.Item as Dictionary<string, object>;
            if (FilterStrings.Rows.Count == 0)
                e.Accepted = true;
            else
            {
                bool accept = true;
                foreach (DataRow dr in FilterStrings.Rows)
                {
                    if (dr["Filter"] != null && (string)dr["Filter"] != "")
                    {
                        string key = (string)dr["Key"];
                        string filter = (string)dr["Filter"];
                        Type datatype = (Type)dr["DataType"];

                        string[] filter_segments = filter.Split(new string[] { ".." }, StringSplitOptions.RemoveEmptyEntries);
                        if (filter_segments.Length == 2)
                        {
                            accept = accept & Compare(filter_segments[0], dict[key], datatype) <= 0 && Compare(dict[key], filter_segments[1], datatype) <= 0;
                        }
                        else if (filter.Contains(".."))
                        {
                            if (filter.StartsWith(".."))
                            {
                                accept = accept & Compare(dict[key], filter_segments[0], datatype) <= 0;
                            }
                            else
                            {
                                accept = accept & Compare(filter_segments[0], dict[key], datatype) <= 0;
                            }
                        }
                        else
                        {
                            accept = accept & Compare(filter, dict[key], datatype) == 0;
                        }
                    }
                    e.Accepted = accept;
                }
            }
        }

        private int Compare(object value1, object value2, Type datatype)
        {
            int result = 0;
            if (datatype == typeof(String))
            {
                result = String.Compare((string)value1, (string)value2, true);
            }
            else if (datatype == typeof(DateTime))
            {
                result = DateTime.Compare(DateTime.Parse((string)value1), (DateTime)value2);
            }
            else if (datatype == typeof(int))
            {
                bool smaller = int.Parse((string)value1) < (int)value2;
                bool bigger = int.Parse((string)value1) > (int)value2;
                if (smaller)
                    result = -1;
                else if (bigger)
                    result = 1;
                else
                    result = 0;
            }
            return result;
        }

        public void Refresh()
        {
            cvs.View.Refresh();
            grd.Items.Refresh();
        }

        //public void Add(object TreeViewItemHeader, List<object> TreeViewItemCollection, params object[] values)
        //{
        //    Dictionary<string, object> dict = new Dictionary<string, object>();
        //    dict.Add("Key", TreeViewItemHeader);
        //    dict.Add("K0", TreeViewItemHeader);
        //    dict.Add("Items", TreeViewItemCollection);
        //    for (int i = 0; i < Columns.Count && i - 1 < values.Length; i++)
        //    {
        //        try
        //        {
        //            mDataGridColumn clm = (mDataGridColumn)Columns[i];
        //            string BindingPath = "K" + i.ToString() + "";
        //            Binding binding = new Binding("[" + BindingPath + "]");
        //            clm.Binding = binding;
        //            if (i > 0)
        //                dict.Add(BindingPath, values[i - 1]);
        //        }
        //        catch (Exception ex) { }
        //    }
        //    dict.Add("Accepted", true);
        //    ItemCollection.Add(dict);
        //    cvs.View.Refresh();
        //}

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
            grd.ScrollIntoView(item);
        }
    }
}
