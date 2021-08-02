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

namespace Frank_UI_Library
{
    /// <summary>
    /// Interaktionslogik für mDataGridTreeView.xaml
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

        public mDataGridTreeView()
        {
            InitializeComponent();
        }

        public void Add(object TreeViewItemHeader, List<object> TreeViewItemCollection, params object[] values)
        {
            Dictionary<string, object> dict = new Dictionary<string, object>();
            dict.Add("Key", TreeViewItemHeader);
            dict.Add("Items", TreeViewItemCollection);
            for (int i = 1; i < Columns.Count; i++)
            {
                mDataGridColumn clm = (mDataGridColumn)Columns[i];
                string BindingPath = "K" + i.ToString() + "";
                Binding binding = new Binding("[" + BindingPath + "]");
                clm.Binding = binding;
                dict.Add(BindingPath, values[i - 1]);
            }
            grd.Items.Add(dict);
        }

        private void grd_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
        }

        #region Das sind keine Event-Handler, sondern Event-Weiterleiter. Die Events werden von mResources zum Parent-Element dieses Objekt geleitet

        internal void TreeViewItemExpanded(object sender, RoutedEventArgs e)
        {
            TreeViewItem_Expanded(sender, e);
        }

        #endregion
    }
}
