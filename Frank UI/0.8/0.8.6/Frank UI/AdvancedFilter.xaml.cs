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
using System.Net;
using System.Net.Sockets;
using System.IO;
using System.Data;
using System.Xml;
using Client_Library;
using System.ComponentModel;
using System.Threading;
using System.Windows.Threading;
using System.Threading.Tasks;
using Helper_Library;
using System.Globalization;
namespace Frank_UI
{
    /// <summary>
    /// Interaction logic for AdvancedFilter.xaml
    /// </summary>
    public partial class AdvancedFilter : Window
    {
        DataTable Calculated_Item;
        DataTable Calculated_Sale;
        DataTable Calculated_Requisition;
        DataTable Calculated_Purchase;
        DataTable SalesHeader;

        public AdvancedFilter()
        {
            InitializeComponent();
        }

        public AdvancedFilter(DataTable Calculated_Item, DataTable Calculated_Sale, DataTable Calculated_Requisition, DataTable Calculated_Purchase, DataTable SalesHeader)
        {
            InitializeComponent();
            this.Calculated_Item = Calculated_Item;
            this.Calculated_Sale = Calculated_Sale;
            this.Calculated_Requisition = Calculated_Requisition;
            this.Calculated_Purchase = Calculated_Purchase;
            this.SalesHeader = SalesHeader;
        }

        private void btnGo_Click(object sender, RoutedEventArgs e)
        {
            try
            {

                DataView dv = null;
                switch ((string)((ComboBoxItem)cmbDataTable.SelectedItem).Content)
                {
                    case "Calculated_Item": dv = Calculated_Item.DefaultView; break;
                    case "Calculated_Sale": dv = Calculated_Sale.DefaultView; break;
                    case "Calculated_Requisition": dv = Calculated_Requisition.DefaultView; break;
                    case "Calculated_Purchase": dv = Calculated_Purchase.DefaultView; break;
                    case "SalesHeader": dv = SalesHeader.DefaultView; break;
                }
                if (dv != null)
                {
                    dv.RowFilter = txtFilterString.Text;
                    dgr.ItemsSource = dv;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
        }

        private void dgr_AutoGeneratingColumn(object sender, DataGridAutoGeneratingColumnEventArgs e)
        {
            if (e.PropertyName.Contains('/') || e.PropertyName.Contains('.') && e.Column is DataGridBoundColumn)
            {
                var col = e.Column as DataGridBoundColumn;
                col.Binding = new Binding(string.Format("[{0}]", e.PropertyName));
            }
        }
    }
}
