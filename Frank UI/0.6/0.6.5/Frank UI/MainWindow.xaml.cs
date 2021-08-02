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

namespace Frank_UI
{
    [ValueConversion(typeof(object), typeof(object))]
    public class StringToColorConverter : IValueConverter
    {

        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            Rectangle rect = new Rectangle();
            rect.Height = rect.Width = 10;
            string strvalue = "";
            var type = value.GetType();

            if (type == typeof(string))
                strvalue = (string)value;
            if (type == typeof(Dictionary<string, object>))
            {
                var dict = (Dictionary<string, object>)value;
                if (dict.ContainsKey("Status"))
                    strvalue = dict["Status"].ToString();
            }
            switch (strvalue.Trim().ToLower())
            {
                case "red": rect.Fill = new SolidColorBrush(Colors.Red); break;
                case "yellow": rect.Fill = new SolidColorBrush(Colors.Yellow); break;
                case "green": rect.Fill = new SolidColorBrush(Colors.Green); break;
            }
            return rect;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    /// <summary>
    /// Interaktionslogik für MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        DataTable Calculated_Item;
        DataTable Calculated_Sale;
        DataTable Parent_Child_Item;
        DataTable SalesHeader;

        DataView Calculated_Item_View;

        string SelectedItemNummer;

        public MainWindow()
        {
            InitializeComponent();
            RefreshEverything();
        }

        private void OpenData()
        {
            Calculated_Item = new DataTable();
            Calculated_Sale = new DataTable();
            Parent_Child_Item = new DataTable();
            SalesHeader = new DataTable();
            try
            {
                Calculated_Item.TableName = "Calculated_Item";
                Calculated_Sale.TableName = "Calculated_Sale";
                Parent_Child_Item.TableName = "Parent_Child_Item";
                SalesHeader.TableName = "Sales Header";
                Calculated_Item.ReadXml(DispoClient.Calculated_Item_Path);
                Calculated_Sale.ReadXml(DispoClient.Calculated_Sale_Path);
                Parent_Child_Item.ReadXml(DispoClient.Parent_Child_Item_Path);
                SalesHeader.ReadXml(DispoClient.SalesHeader_Path);
            }
            catch (Exception ex) 
            {
                MessageBox.Show(ex.GetType().ToString() + ": " + ex.Message, "Fehler", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void getSalesHeaders() 
        {
            foreach (DataRow salesheader in SalesHeader.Rows)
            {

                List<fTreeViewItem> subitems = LoadRekursive(salesheader["No."].ToString());
                if (subitems.Count > 0)
                {
                    string status = "green";
                    foreach (fTreeViewItem itm in subitems)
                    {
                        if (itm.Status == "yellow" && status != "red")
                            status = "yellow";
                        if (itm.Status == "red")
                            status = "red";
                    }
                    List<string> bindings = new List<string> { "Status", "Customer", "Liefertermin" };
                    List<object> values = new List<object> { status, salesheader["Sell-to Customer No."] + " " + salesheader["Sell-to Customer Name"] + " " + salesheader["Sell-to Customer Name 2"], salesheader["Liefertermin frühestens"] };
                    dgtv.Add(salesheader["No."].ToString(), subitems.Cast<object>().ToList(), bindings, values);
                }
            }
        }

        private List<fTreeViewItem> LoadRekursive(string nummer)
        {
            List<fTreeViewItem> collection = new List<fTreeViewItem>();
            DataRow[] Parent = Parent_Child_Item.Select("[Parent No.] = '" + nummer + "'");
            foreach (DataRow dr in Parent)
            {
                DataRow calculated_item = Calculated_Item.Rows.Find(dr["Child No."]);                

                if (calculated_item != null)
                {
                    fTreeViewItem subtrvItem = new fTreeViewItem();
                    subtrvItem.Nummer = dr["Child No."].ToString();
                    subtrvItem.Beschreibung = calculated_item["Description"].ToString();
                    subtrvItem.Menge = dr["Quantity"].ToString();
                    subtrvItem.Status = dr["Status"].ToString();
                    subtrvItem.Fertigungsstelle = calculated_item["Location"].ToString();
                    subtrvItem.FehlendeMenge = calculated_item["Missing Quantity"].ToString();
                    subtrvItem.Beschaffungsmethode = calculated_item["Replenishment System"].ToString();
                    subtrvItem.Expanded += trvItem_Expanded;
                    subtrvItem.Collapsed += trvItem_Collapsed;
                    subtrvItem.Selected += SubtrvItem_Selected;
                    subtrvItem.Items.AddRange(LoadRekursive(dr["Child No."].ToString()));
                    collection.Add(subtrvItem);
                }
            }
            return collection;
        }

        public struct Item
        {
            public string status { get; set; }
            public string replenishment { get; set; }
            public string no { get; set; }
            public string benoetigtliefertermin { get; set; }
            public string mengeliefertermin { get; set; }
        }

        private void updateItemOnly()
        {

            Calculated_Item_View = Calculated_Item.DefaultView;
            dgr.ItemsSource = Calculated_Item_View;
        }

        private void updateStatusbar()
        {
            lblNumberOfSales.Content = dgtv.NumberOfItems.ToString();
            lblNumberofItems.Content = Calculated_Item.Rows.Count.ToString();
            if (DispoClient.serverInfo != null)
            {
                DateTime lastupdate = ((ServerInfo)DispoClient.serverInfo).LastUpdated;
                lblLastUpdate.Content = lastupdate.ToString();
            }
        }

        private void trvItem_Expanded(object sender, RoutedEventArgs e)
        {
            mTreeViewItem trvItm = sender as mTreeViewItem;
            trvItm.Background = new SolidColorBrush(Colors.Transparent);
            HighlightRekursive(trvItm, SelectedItemNummer);
            e.Handled = true;

            if (trvItm.IsHighlighted)
            {
                trvItm.UnHighlight();
                HighlightRekursive(trvItm, SelectedItemNummer);
            }

            //#region Menu-Item Expand and Collapse
            //mitmExpandAll.IsEnabled = false;
            //foreach (mTreeViewItem itm in mTreeViewItem.mTreeViewItemCollection)
            //{
            //    if (!itm.IsExpanded)
            //    {
            //        mitmExpandAll.IsEnabled = true;
            //        break;
            //    }
            //}
            //mitmCollapseAll.IsEnabled = true;
            //#endregion
        }

        private void trvItem_Collapsed(object sender, RoutedEventArgs e)
        {
            mTreeViewItem item = sender as mTreeViewItem;
            bool contains = item.Header.ToString() == SelectedItemNummer;
            foreach (fTreeViewItem subitm in item.Items)
            {
                if (contains)
                    break;
                contains = contains | subitm.Contains(SelectedItemNummer);
            }
            if (contains)
                item.Background = (SolidColorBrush)ResourceHelper.dict["Highlight"];
            else
                item.Background = new SolidColorBrush(Colors.Transparent);

            e.Handled = true;

            #region Menu-Item Expand and Collapse
            mitmCollapseAll.IsEnabled = false;
            foreach (mTreeViewItem itm in mTreeViewItem.mTreeViewItemCollection)
            {
                if (itm.IsExpanded)
                {
                    mitmCollapseAll.IsEnabled = true;
                    break;
                }
            }
            mitmExpandAll.IsEnabled = true;
            #endregion
        }

        private void SubtrvItem_Selected(object sender, RoutedEventArgs e)
        {
            fTreeViewItem item = sender as fTreeViewItem;
            #region Infomrations-Bereich
            DataRow calculated_item = Calculated_Item.Rows.Find(item.Nummer);
            SelectedItemNummer = item.Nummer;
            if (calculated_item != null)
            {
                lblArtikelnr.Content = calculated_item["No."] + " - " + calculated_item["Description"];
                lblBeschaffung.Content = calculated_item["Replenishment System"];
                lblEinheit.Content = calculated_item["Base Unit of Measure"];
                lblMengeBenoetigt.Content = calculated_item["Required Quantity"];
                lblLagerbestand.Content = calculated_item["Inventory"];
                lblImAuftrag.Content = calculated_item["Sales Order"];
                lblImEinkauf.Content = calculated_item["Purchase Order"];
                lblInFertigung.Content = calculated_item["Prod. Order"];
                lblInFertigungInKomponente.Content = calculated_item["Prod. Component"];
                lblInMaterialanforderung.Content = calculated_item["Qty. on Requisition"];
                lblMissingQuantity.Content = calculated_item["Missing Quantity"];

                switch (item.Status)
                {
                    case "red": tblErforderlicheAktion.Text = "Dieser Artikel ist nicht ausreichend Verfügbar"; break;
                    case "yellow": tblErforderlicheAktion.Text = "Dieser Artikel ist ausreichend für die übergeordnete Bestellung verfügbar, jedoch wird er nach Abwicklung dieser Bestellung evtl. für andere Bestellungen fehlen."; break;
                    case "green": tblErforderlicheAktion.Text = "Dieser Artikel ist ausreichend auf Lager. Es ist keine weitere Aktion erforderlich."; break;
                }

                DataRow[] ParentOrder = Calculated_Sale.Select("[Item No.] = '" + calculated_item["No."] + "'");
                List<InSales> insales = new List<InSales>();
                foreach (DataRow dr in ParentOrder)
                {
                    DataRow salesheader = SalesHeader.Rows.Find(dr["No."]);
                    InSales insale = new InSales(dr["Required Quantity"].ToString(), dr["No."].ToString(), dr["Liefertermin frühestens"].ToString(), salesheader["Sell-to Customer No."] + " " + salesheader["Sell-to Customer Name"] + " " + salesheader["Sell-to Customer Name 2"], dr["Parent Item No."] + " " + dr["Description"]);
                    insales.Add(insale);
                }
                dgrdInSales.ItemsSource = insales;
                e.Handled = true;
            }
            #endregion

            //#region Highlighting
            //string nummer = SelectedItemNummer = item.Nummer;
            //foreach (mTreeViewItem itm in mTreeViewItem.mTreeViewItemCollection)
            //{
            //    //Temporary Solution
            //    if (itm.Header != null)
            //    {
            //        bool contains = itm.Header.ToString() == nummer;
            //        foreach (fTreeViewItem subitm in itm.Items)
            //        {
            //            if (contains)
            //                break;
            //            contains = contains | subitm.Contains(nummer);
            //        }
            //        if (contains)
            //        {
            //            if (itm.IsExpanded)
            //                HighlightRekursive(itm, nummer);
            //            else
            //                itm.Background = (SolidColorBrush)ResourceHelper.dict["Highlight"];
            //        }
            //        else
            //            itm.Background = new SolidColorBrush(Colors.Transparent);
            //    }
            //}
            //#endregion
        }

        private void HighlightRekursive(mTreeViewItem item, string nummer) 
        {
            foreach (fTreeViewItem itm in item.Items) 
            {
                if (itm.IsExpanded)
                {
                    itm.Reset();
                    HighlightRekursive(itm, nummer);
                }
                else
                {
                    if (itm.Contains(nummer))
                        itm.Highlight();
                    else
                        itm.Reset();
                }
            }
        }

        private void txtSearch_TextChanged(object sender, TextChangedEventArgs e)
        {
        }

        private void mitmExpandAll_Click(object sender, RoutedEventArgs e)
        {
            foreach (mTreeViewItem itm in mTreeViewItem.mTreeViewItemCollection)
            {
                itm.ExpandAll();
            }
        }

        private void mitmCollapseAll_Click(object sender, RoutedEventArgs e)
        {
            foreach (mTreeViewItem itm in mTreeViewItem.mTreeViewItemCollection)
            {
                itm.CollapseAll();
            }
        }
                
        private void mitmUpdateData_Click(object sender, RoutedEventArgs e)
        {
            RefreshEverything();
        }

        LoadingWindow lw;
        private void RefreshEverything()
        {
            try
            {
                Thread newWindowThread = new Thread(new ThreadStart(ThreadStartingPoint));
                newWindowThread.SetApartmentState(ApartmentState.STA);
                newWindowThread.IsBackground = true;
                newWindowThread.Start();

                dgtv.Clear();
                DispoClient.GetData();
                OpenData();
                getSalesHeaders();
                updateItemOnly();
                updateStatusbar();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString(), "Fehler", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            CloseWindowSafe(lw);
        }
        void CloseWindowSafe(Window w)
        {
            if (w.Dispatcher.CheckAccess())
                w.Close();
            else
                w.Dispatcher.Invoke(DispatcherPriority.Normal, new ThreadStart(w.Close));
        }
        private void ThreadStartingPoint()
        {
            try
            {
                lw = new LoadingWindow();
                lw.Show();
                System.Windows.Threading.Dispatcher.Run();
            }
            catch (ThreadAbortException)
            {
                lw.Close();
            }
        }

        private void dgr_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            DataRow calculated_item = (e.AddedItems[0] as DataRowView).Row;
            SelectedItemNummer = calculated_item["No."].ToString();
            if (calculated_item != null)
            {
                lblArtikelnr.Content = calculated_item["No."] + " - " + calculated_item["Description"];
                lblBeschaffung.Content = calculated_item["Replenishment System"];
                lblEinheit.Content = calculated_item["Base Unit of Measure"];
                lblMengeBenoetigt.Content = calculated_item["Required Quantity"];
                lblLagerbestand.Content = calculated_item["Inventory"];
                lblImAuftrag.Content = calculated_item["Sales Order"];
                lblImEinkauf.Content = calculated_item["Purchase Order"];
                lblInFertigung.Content = calculated_item["Prod. Order"];
                lblInFertigungInKomponente.Content = calculated_item["Prod. Component"];
                lblInMaterialanforderung.Content = calculated_item["Qty. on Requisition"];
                lblMissingQuantity.Content = calculated_item["Missing Quantity"];

                switch (calculated_item["Status"].ToString().Trim().ToLower())
                {
                    case "red": tblErforderlicheAktion.Text = "Dieser Artikel ist nicht ausreichend Verfügbar"; break;
                    case "yellow": tblErforderlicheAktion.Text = "Dieser Artikel ist ausreichend für die übergeordnete Bestellung verfügbar, jedoch wird er nach Abwicklung dieser Bestellung evtl. für andere Bestellungen fehlen."; break;
                    case "green":
                        if (calculated_item["Replenishment System"].ToString() == "Einkauf")
                            tblErforderlicheAktion.Text = "Dieser Artikel befindet sich nicht ausreichend auf Lager, er wurde jedoch bereits bestellt. Keine weitere Aktion erforderlich!";
                        else
                            tblErforderlicheAktion.Text = "Dieser Artikel befindet sich nicht ausreichend auf Lager, er befindet sich jedoch in Produktion. Keine weitere Aktion erforderlich!";
                        break;
                }

                DataRow[] ParentOrder = Calculated_Sale.Select("[Item No.] = '" + calculated_item["No."] + "'");
                List<InSales> insales = new List<InSales>();
                foreach (DataRow dr in ParentOrder)
                {
                    DataRow salesheader = SalesHeader.Rows.Find(dr["No."]);
                    InSales insale = new InSales(dr["Required Quantity"].ToString(), dr["No."].ToString(), dr["Liefertermin frühestens"].ToString(), salesheader["Sell-to Customer No."] + " " + salesheader["Sell-to Customer Name"] + " " + salesheader["Sell-to Customer Name 2"], dr["Parent Item No."] + " " + dr["Description"]);
                    insales.Add(insale);
                }
                dgrdInSales.ItemsSource = insales;
            }
        }

        private void dgr_Sorting(object sender, DataGridSortingEventArgs e)
        {
        }

        private void dgrdInSales_Sorting(object sender, DataGridSortingEventArgs e)
        {
            DataGridColumn clm = e.Column;
            if (clm.Header.ToString() == "Menge")
            {
                e.Handled = true;

                var direction = (clm.SortDirection != ListSortDirection.Ascending) ? ListSortDirection.Ascending : ListSortDirection.Descending;
                clm.SortDirection = direction;

                ListCollectionView lcv = (ListCollectionView)CollectionViewSource.GetDefaultView(dgrdInSales.ItemsSource);

                switch (clm.SortDirection)
                {
                    case ListSortDirection.Ascending: lcv.CustomSort = new InSalesMengeNumericAscendingComparer(); break;
                    case ListSortDirection.Descending: lcv.CustomSort = new InSalesNumericNumericDescendingComparer(); break;
                }
            }

            if (clm.Header.ToString() == "Liefertermin")
            {
                e.Handled = true;

                var direction = (clm.SortDirection != ListSortDirection.Ascending) ? ListSortDirection.Ascending : ListSortDirection.Descending;
                clm.SortDirection = direction;

                ListCollectionView lcv = (ListCollectionView)CollectionViewSource.GetDefaultView(dgrdInSales.ItemsSource);

                switch (clm.SortDirection)
                {
                    case ListSortDirection.Ascending: lcv.CustomSort = new InSalesLieferterminDateTimeAscendingComparer(); break;
                    case ListSortDirection.Descending: lcv.CustomSort = new InSalesLieferterminDateTimeDescendingComparer(); break;
                }
            }
        }

        private void dgrdInSales_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var insales = dgrdInSales.SelectedItems;

            foreach (mTreeViewItem item in mTreeViewItem.mTreeViewItemCollection)
            {
                bool isSelected = false;
                string header = (item.Header as TextBlock).Text.Trim();
                foreach (InSales insale in insales)
                {
                    if (header.Contains(insale.Nr))
                        isSelected = true;
                }

                if (isSelected)
                {
                    item.Highlight();
                    dgtv.ScrollInto(header);
                }
                else
                    item.UnHighlight();

            }
        }

        #region Filter
        private void clmDescription_FilterOptionsClick(object sender, RoutedEventArgs e)
        {
            mWindowGetFilterString wdf = new mWindowGetFilterString();
            string filter = wdf.ShowDialog("Beschreibung", "K0", dgtv);
            if (filter != null)
            {
                dgtv.FilterStrings.Rows.Remove("K0");
                dgtv.FilterStrings.Rows.Add("K0", filter, typeof(string));
                dgtv.Refresh();
            }
        }
        private void clmCustomer_FilterOptionsClick(object sender, RoutedEventArgs e)
        {
            mWindowGetFilterString wdf = new mWindowGetFilterString();
            string filter = wdf.ShowDialog("Kunde", "Customer", dgtv);
            if (filter != null)
            {
                dgtv.FilterStrings.Rows.Remove("Customer");
                dgtv.FilterStrings.Rows.Add("Customer", filter, typeof(string));
                dgtv.Refresh();
            }
        }
        private void clmDate_FilterOptionsClick(object sender, RoutedEventArgs e)
        {
            mWindowGetFilterString wdf = new mWindowGetFilterString();
            string filter = wdf.ShowDialog("Liefertermin", "Liefertermin", dgtv);
            if (filter != null)
            {
                dgtv.FilterStrings.Rows.Remove("Liefertermin");
                dgtv.FilterStrings.Rows.Add("Liefertermin", filter, typeof(DateTime));
                dgtv.Refresh();
            }
        }

        string replenishment = "";
        private void dgrReplenishment_FilterOptionsClick(object sender, RoutedEventArgs e)
        {
            mWindowGetFilterString wdf = new mWindowGetFilterString();
            replenishment = wdf.ShowDialog("Beschaffungsmethode", replenishment);
            if (replenishment != null)
                Calculated_Item_View.RowFilter = ConcatFilters();
        }
        string itemno = "";
        private void dgrItemNo_FilterOptionsClick(object sender, RoutedEventArgs e)
        {
            mWindowGetFilterString wdf = new mWindowGetFilterString();
            itemno = wdf.ShowDialog("Beschreibung", itemno);
            if (itemno != null)
                Calculated_Item_View.RowFilter = ConcatFilters();
        }
        string benoetigtbis = "";
        private void dgrBenoetigtBis_FilterOptionsClick(object sender, RoutedEventArgs e)
        {
            mWindowGetFilterString wdf = new mWindowGetFilterString();
            benoetigtbis = wdf.ShowDialog("Beschreibung", benoetigtbis);
            if (benoetigtbis != null)
                Calculated_Item_View.RowFilter = ConcatFilters();
        }
        string mengebis = "";
        private void dgrMengeBis_FilterOptionsClick(object sender, RoutedEventArgs e)
        {
            mWindowGetFilterString wdf = new mWindowGetFilterString();
            mengebis = wdf.ShowDialog("Beschreibung", mengebis);
            if (mengebis != null)
                Calculated_Item_View.RowFilter = ConcatFilters();
        }

        private void mitmResetFilters_Click(object sender, RoutedEventArgs e)
        {
            replenishment = itemno = benoetigtbis = mengebis = "";
            Calculated_Item_View.RowFilter = "";

            dgtv.FilterStrings.Clear();
            dgtv.Refresh();
        }

        private string ConcatFilters()
        {
            string result = "";
            if (replenishment != "")
                result += FilterBuilder("Replenishment System", replenishment);
            if (itemno != "")
            {
                if (result != "")
                    result += " AND ";
                result += FilterBuilder("No.", itemno);
            }
            if (benoetigtbis != "")
            {
                if (result != "")
                    result += " AND ";
                result += FilterBuilder("Erforderlich bis Liefertermin", benoetigtbis);
            }
            if (mengebis != "")
            {
                if (result != "")
                    result += " AND ";
                result += FilterBuilder("Menge bis Liefertermin", mengebis);
            }
            return result;
        }

        private string FilterBuilder(string columnname, string filter)
        {
            string result = "";
            if (filter != "")
            {
                string[] filter_segments = filter.Split(new string[] { ".." }, StringSplitOptions.RemoveEmptyEntries);
                if (filter_segments.Length == 2)
                {
                    result = String.Format("[{0}] >= '{1}' AND [{0}] <= '{2}'", columnname, filter_segments[0], filter_segments[1]);
                }
                else if (filter.Contains(".."))
                {
                    if (filter.StartsWith(".."))
                    {
                        result = String.Format("[{0}] <= '{1}'", columnname, filter_segments[0]);
                    }
                    else
                    {
                        result = String.Format("[{0}] >= '{1}'", columnname, filter_segments[0]);
                    }
                }
                else
                {
                    result = String.Format("[{0}] = '{1}'", columnname, filter_segments[0]);
                }
            }
            return result;

        }
        #endregion
    }
    class InSales
    {
        public string Menge { get; set; }
        public string Nr { get; set; }
        public string Liefertermin { get; set; }
        public string Kunde { get; set; }
        public string Artikel { get; set; }
        public InSales(string amount, string no, string date, string customer, string item)
        {
            Menge = amount;
            Nr = no;
            Liefertermin = date;
            Kunde = customer;
            Artikel = item;
        }
    }
}
