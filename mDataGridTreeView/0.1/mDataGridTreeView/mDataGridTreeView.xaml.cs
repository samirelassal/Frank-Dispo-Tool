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
    /// Interaktionslogik für mDataGridTreeVieww.xaml
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

        DataTable data = new DataTable();
        

        public mDataGridTreeView()
        {
            InitializeComponent();
            //Grid g = new Grid();
            //g.Height = 300;
            //g.Width = 50;
            //g.Background = new SolidColorBrush(Colors.Red);
            //List<Model> models = new List<Model>();
            //Model model = new Model();
            //model.Key = g;
            //models.Add(model);
            //grd.ItemsSource = models;
        }

        public void Add(TreeViewItem item, params object[] values)
        {
            UpdateDataColumns();
            //DataRow dr = data.NewRow();
            //TreeView trv = new TreeView();
            //trv.Items.Add(item);
            //dr[0] = trv;
            //for (int i = 1; i < data.Columns.Count; i++)
            //{
            //    try
            //    {
            //        dr[i] = values[i - 1];
            //    }
            //    catch { }
            //}
            //data.Rows.Add(dr);
            //grd.ItemsSource = data.Rows;
            Dictionary<string, object> dict = new Dictionary<string, object> {{"Value", item }};
            grd.ItemsSource = dict;
        }

        private void UpdateDataColumns() 
        {
            foreach (mDataGridColumn clm in Columns) 
            {
                try
                {
                    data.Columns.Add(clm.ReferenceKey);
                }
                catch { }
            }
        }

        public void SortDataGrid(DataGrid dataGrid, int columnIndex = 0, ListSortDirection sortDirection = ListSortDirection.Ascending)
        {
            var column = dataGrid.Columns[columnIndex];

            // Clear current sort descriptions
            dataGrid.Items.SortDescriptions.Clear();

            // Add the new sort description
            dataGrid.Items.SortDescriptions.Add(new SortDescription(column.SortMemberPath, sortDirection));

            // Apply sort
            foreach (var col in dataGrid.Columns)
            {
                col.SortDirection = null;
            }
            column.SortDirection = sortDirection;

            // Refresh items to display sort
            dataGrid.Items.Refresh();
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            UpdateDataColumns();
        }
    }
    public class Model
    {
        public object Key { get; set; }
    }

}
