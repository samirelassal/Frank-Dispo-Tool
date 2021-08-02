﻿using System;
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

        List<Dictionary<string, object>> ItemCollection = new List<Dictionary<string, object>>();
        CollectionViewSource cvs;

        public int NumberOfItems
        {
            get
            {
                return ItemCollection.Count;
            }
        }

        public bool UseFilter = false;
        //Key is the column key, value is the filter string for current column
        public readonly Dictionary<string, string> FilterStrings = new Dictionary<string, string>();

        public mDataGridTreeView()
        {
            InitializeComponent();
            cvs = new CollectionViewSource();
            cvs.Source = ItemCollection;
            cvs.Filter += Cvs_Filter;
            grd.ItemsSource = cvs.View;
            
        }

        private void Cvs_Filter(object sender, FilterEventArgs e)
        {
            Dictionary<string, object> dict = e.Item as Dictionary<string, object>;
            if (!UseFilter)
                e.Accepted = true;
            else
            {
                foreach (string key in FilterStrings.Keys)
                {
                    if (FilterStrings[key] != null && FilterStrings[key] != "")
                    {
                        string filter = FilterStrings[key];
                        string[] filter_segments = filter.Split(new string[] { ".." }, StringSplitOptions.RemoveEmptyEntries);
                        if (filter_segments.Length == 2)
                        {
                            e.Accepted = String.Compare(filter_segments[0], dict[key].ToString(), true) <= 0 && String.Compare(dict[key].ToString(), filter_segments[1]) <= 0;
                        }
                        else if (filter.Contains(".."))
                        {
                            if (filter.StartsWith(".."))
                            {
                                e.Accepted = String.Compare(dict[key].ToString(), filter_segments[0], true) <= 0;
                            }
                            else
                            {
                                e.Accepted = String.Compare(filter_segments[0], dict[key].ToString(), true) <= 0;
                            }
                        }
                        else
                        {
                            e.Accepted = string.Compare(filter, dict[key].ToString(), true) == 0;
                        }
                    }
                }
            }
        }

        public void Refresh()
        {
            cvs.View.Refresh();
        }

        public void Add(object TreeViewItemHeader, List<object> TreeViewItemCollection, params object[] values)
        {
            Dictionary<string, object> dict = new Dictionary<string, object>();
            dict.Add("Key", TreeViewItemHeader);
            dict.Add("K0", TreeViewItemHeader);
            dict.Add("Items", TreeViewItemCollection);
            for (int i = 0; i < Columns.Count && i-1 < values.Length; i++)
            {
                mDataGridColumn clm = (mDataGridColumn)Columns[i];
                string BindingPath = "K" + i.ToString() + "";
                Binding binding = new Binding("[" + BindingPath + "]");
                clm.Binding = binding;
                if(i > 0)
                    dict.Add(BindingPath, values[i - 1]);
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
            TreeViewItem_Expanded(sender, e);
        }

        internal void TreeViewItemCollapsed(object sender, RoutedEventArgs e)
        {
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
    }
}
