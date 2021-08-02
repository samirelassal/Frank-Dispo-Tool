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
using System.ComponentModel;
using System.Data;
using System.Threading.Tasks;
using System.Collections.ObjectModel;

namespace WpfApplication1
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
        
        public mDataGridTreeView()
        {
            InitializeComponent();
        }

        public void Add(string TreeViewItemHeader, List<object> TreeViewItemCollection, params object[] values)
        {
            Dictionary<string, object> dict = new Dictionary<string, object>();
            dict.Add("Key", TreeViewItemHeader);
            dict.Add("Items", TreeViewItemCollection);
            for(int i = 0; i < Columns.Count; i++)
            {
                mDataGridColumn clm = (mDataGridColumn)Columns[i];
                string BindingPath = (clm.Binding as Binding).Path.Path.Trim('[', ']');
                if (BindingPath != "Key" && BindingPath != "Items") 
                {
                    dict.Add(BindingPath, values[i]);
                }
            }
            grd.Items.Add(dict);
        }
    }
}
