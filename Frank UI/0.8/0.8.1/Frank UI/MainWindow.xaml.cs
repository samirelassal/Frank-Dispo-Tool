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

    [ValueConversion(typeof(object), typeof(object))]
    public class BoolToCheckBox : IValueConverter
    {

        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            CheckBox chk = new CheckBox();
            chk.IsEnabled = false;

            Type type = value.GetType();
            if (type == typeof(bool))
                chk.IsChecked = (bool)value;
            return chk;
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
        DataTable Calculated_Requisition;
        DataTable SalesHeader;
        Parent_Child_Item Parent_Child_Item = new Parent_Child_Item();
        DataSet Final_Data = new DataSet("Final_Data");
        DataView Calculated_Item_View;
        string SelectedItemNummer;
        LoadingWindow lw;

        #region Initialize
        public MainWindow()
        {
            InitializeComponent();
            RefreshEverything();
            DispoClient.ServerNotificationReceived += DispoClient_ServerNotificationReceived;
        }        
        private void RefreshEverything()
        {
            try
            {
                Thread newWindowThread = new Thread(new ThreadStart(ThreadStartingPoint));
                newWindowThread.SetApartmentState(ApartmentState.STA);
                newWindowThread.IsBackground = true;
                newWindowThread.Start();

                dgtv.Clear();
                string result = "";
                DispoClient.ClientVersion = Frank_UI.Properties.Resources.Version;
                result = DispoClient.GetData();
                if (result == "")
                {
                    OpenData();
                    getSalesHeaders();
                    updateItemOnly();
                    updateStatusbar();
                }
                else
                {
                    CloseWindowSafe(lw);
                    MessageBox.Show(result, "Meldung", MessageBoxButton.OK, MessageBoxImage.Information, MessageBoxResult.OK, MessageBoxOptions.DefaultDesktopOnly);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString(), "Fehler", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            CloseWindowSafe(lw);
        }
        private void CloseWindowSafe(Window w)
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
        #endregion

        #region Data
        private void OpenData()
        {
            try
            {
                Final_Data.ReadXml(DispoClient.Final_Data_Path);
                Parent_Child_Item.LoadData(DispoClient.Parent_Child_Item_Path);
                Calculated_Item = Final_Data.Tables["Calculated_Item"];
                Calculated_Sale = Final_Data.Tables["Calculated_Sale"];
                Calculated_Requisition = Final_Data.Tables["Calculated_Requisition"];
                SalesHeader = Final_Data.Tables["Sales Header"];
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.GetType().ToString() + ": " + ex.Message, "Fehler", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// Fills thee mDataGridTreeView object with sales headers and items
        /// </summary>
        private void getSalesHeaders()
        {
            foreach (DataRow salesheader in SalesHeader.Rows)
            {
                DataRow PCI_SalesHeader = Parent_Child_Item.FindByNo(salesheader["No."].ToString())[0];
                List<fTreeViewItem> subitems = LoadRekursive((int)PCI_SalesHeader["ID"]);
                if (subitems.Count > 0)
                {
                    bool available = true;
                    foreach (fTreeViewItem itm in subitems)
                    {
                        available = available & itm.Available;
                    }
                    List<string> bindings = new List<string> { "Available", "Customer", "Liefertermin" };
                    List<object> values = new List<object> { available, salesheader["Sell-to Customer No."] + " " + salesheader["Sell-to Customer Name"] + " " + salesheader["Sell-to Customer Name 2"], salesheader["Liefertermin frühestens"] };
                    dgtv.Add(salesheader["No."].ToString(), subitems.Cast<object>().ToList(), bindings, values);
                }
            }
        }

        /// <summary>
        /// Loads all items that are linked to the elemt specified with the number. It can be either a salesheader->item relation or an item->production bom->item relation
        /// </summary>
        /// <param name="nummer">Specifies the sales header or the item</param>
        /// <returns></returns>
        private List<fTreeViewItem> LoadRekursive(int ParentID)
        {
            List<fTreeViewItem> collection = new List<fTreeViewItem>();
            DataRow[] PCI_children = Parent_Child_Item.GetChildren(ParentID);
            foreach (DataRow PCI_Child in PCI_children)
            {
                DataRow calculated_item = Calculated_Item.Rows.Find(PCI_Child["No."]);

                if (calculated_item != null)
                {
                    fTreeViewItem subtrvItem = new fTreeViewItem();
                    subtrvItem.Nummer = PCI_Child["No."].ToString();
                    subtrvItem.Beschreibung = calculated_item["Description"].ToString();
                    subtrvItem.Menge = PCI_Child["Quantity Total"].ToString();
                    subtrvItem.Available = (bool)PCI_Child["Available"];
                    subtrvItem.Fertigungsstelle = calculated_item["Location"].ToString();
                    subtrvItem.FehlendeMenge = calculated_item["Missing Quantity"].ToString();
                    subtrvItem.Beschaffungsmethode = calculated_item["Replenishment System"].ToString();
                    subtrvItem.Priorität = PCI_Child["Priorität Buchstaben"].ToString() + PCI_Child["Priorität Zahl"].ToString();
                    subtrvItem.Expanded += trvItem_Expanded;
                    subtrvItem.Collapsed += trvItem_Collapsed;
                    subtrvItem.Selected += SubtrvItem_Selected;
                    subtrvItem.Items.AddRange(LoadRekursive((int)PCI_Child["ID"]));
                    collection.Add(subtrvItem);
                }
            }
            return collection;
        }

        private void updateItemOnly()
        {
            Calculated_Item.CaseSensitive = false;
            Calculated_Item_View = Calculated_Item.DefaultView;
            dgr.ItemsSource = Calculated_Item_View;
        }
        #endregion

        #region Events
        private void DispoClient_ServerNotificationReceived(string message)
        {
            lblLastUpdate.Foreground = new SolidColorBrush(Colors.Red);
            if (!lblLastUpdate.Content.ToString().EndsWith(" Neue Daten verfügbar. Bitte aktualisieren!"))
                lblLastUpdate.Content += " Neue Daten verfügbar. Bitte aktualisieren!";
        }

        private void trvItem_Expanded(object sender, RoutedEventArgs e)
        {
            mTreeViewItem trvItm = sender as mTreeViewItem;
            //Make sure, Item_Expanded-event is not called for child-items as well
            e.Handled = true;
            if (trvItm.IsHighlighted)
            {
                //If background is not transparent, a subitem will me highlighted using the HighlightRekursive() method -> this item does not need to be highlighted anymore
                trvItm.UnHighlight();
                HighlightRekursive(trvItm, SelectedItemNummer);
            }
        }

        private void trvItem_Collapsed(object sender, RoutedEventArgs e)
        {
            Highlight();
            e.Handled = true;
        }

        private void mitmUpdateData_Click(object sender, RoutedEventArgs e)
        {
            RefreshEverything();
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
        }

        //Highlight selected items in mDataGridTreeViewItem
        private void dgrdInSales_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var insales = dgrdInSales.SelectedItems;
            dgtv.ScrollInto(Highlight());

            //foreach (mTreeViewItem item in mTreeViewItem.mTreeViewItemCollection)
            //{
            //    if (item.Header != null)
            //    {
            //        bool isSelected = false;
            //        string header = (item.Header as TextBlock).Text.Trim();
            //        foreach (InSales insale in insales)
            //        {
            //            if (header.Contains(insale.Nr))
            //                isSelected = true;
            //        }

            //        if (isSelected)
            //        {
            //            if (item.IsExpanded)
            //                HighlightRekursive(item, SelectedItemNummer);
            //            else
            //                item.Highlight();
            //            dgtv.ScrollInto(header);
            //        }
            //        else
            //        {
            //            if (item.IsExpanded)
            //                item.UnHighlightAll();
            //            else
            //                item.UnHighlight();
            //        }
            //    }
            //}
        }

        private void dgr_CopyingRowClipboardContent(object sender, DataGridRowClipboardEventArgs e)
        {

        }

        private void dgtv_TreeViewItem_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            var item = (mTreeViewItem)sender;
            if (item.IsExpanded)
                item.CollapseAll();
            else
                item.ExpandAll();
            e.Handled = true;
        }

        private void mtmManual_Click(object sender, RoutedEventArgs e)
        {
            Manual mn = new Manual();
            mn.Show();
        }

        private void mtmAbout_Click(object sender, RoutedEventArgs e)
        {
            About about = new About();
            about.Show();
        }
        #endregion

        #region UI
        private void updateStatusbar()
        {
            lblNumberOfSales.Content = dgtv.NumberOfItems.ToString();
            lblNumberofItems.Content = Calculated_Item.Rows.Count.ToString();
            if (DispoClient.serverInfo != null)
            {
                DateTime lastupdate = ((ServerInfo)DispoClient.serverInfo).LastUpdated;
                lblLastUpdate.Content = lastupdate.ToString();
                lblLastUpdate.Foreground = new SolidColorBrush(Colors.Black);
            }
        }

        private void UpdateInfoArea(DataRow calculated_item)
        {
            SelectedItemNummer = calculated_item["No."].ToString();
            if (calculated_item != null)
            {
                lblArtikelnr.Content = calculated_item["No."] + " - " + calculated_item["Description"];
                lblBeschaffung.Content = calculated_item["Replenishment System"];
                lblEinheit.Content = calculated_item["Base Unit of Measure"];
                lblMengeBenoetigt.Content = calculated_item["Required Quantity"];
                lblLagerbestand.Content = calculated_item["Inventory"];
                lblParentInventory.Content = calculated_item["Parent Inventory"];
                lblImAuftrag.Content = calculated_item["Sales Order"];
                lblImEinkauf.Content = calculated_item["Purchase Order"];
                lblInFertigung.Content = calculated_item["Prod. Order"];
                lblInFertigungInKomponente.Content = calculated_item["Prod. Component"];
                lblInMaterialanforderung.Content = calculated_item["Qty. on Requisition"];
                lblMissingQuantity.Content = calculated_item["Missing Quantity"];
                if (calculated_item["Action"].ToString() == "Fertigen")
                {
                    lblAction.Content = calculated_item["Quantity able to Produce"] + " x fertigen";
                }
                else
                    lblAction.Content = calculated_item["Action"];

                DataRow[] ParentOrder = Calculated_Sale.Select("[Item No.] = '" + calculated_item["No."] + "'");
                List<InSales> insales = new List<InSales>();
                foreach (DataRow dr in ParentOrder)
                {
                    DataRow salesheader = SalesHeader.Rows.Find(dr["No."]);
                    InSales insale = new InSales(dr["Required Quantity"].ToString(), dr["No."].ToString(), dr["Nächste Priorität Buchstaben"].ToString(), (int) dr["Nächste Priorität Zahl"], salesheader["Sell-to Customer No."] + " " + salesheader["Sell-to Customer Name"] + " " + salesheader["Sell-to Customer Name 2"], dr["Parent Item No."] + " " + dr["Description"]);
                    insales.Add(insale);
                }
                dgrdInSales.ItemsSource = insales;
                DataView cr_view = Calculated_Requisition.DefaultView;
                cr_view.RowFilter = "[No.] = '" + calculated_item["No."] + "'";
                dgrdRequisition.ItemsSource = cr_view;
            }
        }

        private void HighlightRekursive(mTreeViewItem item, string nummer)
        {
            foreach (fTreeViewItem itm in item.Items)
            {
                if (itm.IsExpanded)
                {
                    itm.UnHighlight();
                    HighlightRekursive(itm, nummer);
                }
                else
                {
                    if (itm.Contains(nummer))
                        itm.Highlight();
                    else
                        itm.UnHighlight();
                }
            }
        }
        private string Highlight(List<string> Salesnumbers, string Itemnumber)
        {
            string scrollinto = "";
            foreach (mTreeViewItem item in mTreeViewItem.mTreeViewItemCollection)
            {
                if (item.Header != null)
                {
                    bool isSelected = false;
                    string header = (item.Header as TextBlock).Text.Trim();
                    foreach (string salesnumber in Salesnumbers)
                    {
                        if (header.Contains(salesnumber))
                            isSelected = true;
                    }

                    if (item.IsExpanded)
                    {
                        item.UnHighlightAll();
                        if (isSelected)
                        {
                            HighlightRekursive(item, Itemnumber);
                            scrollinto = header;
                        }
                    }
                    else
                    {
                        if (isSelected)
                        {
                            item.Highlight();
                            scrollinto = header;
                        }
                        else
                            item.UnHighlight();
                    }
                }
            }
            return scrollinto;
        }
        private string Highlight()
        {
            List<string> Salesnumbers = new List<string>();
            foreach (InSales insale in dgrdInSales.SelectedItems)
                Salesnumbers.Add(insale.Nr);
            return Highlight(Salesnumbers, SelectedItemNummer);

        }
        #endregion

        #region selection changed
        private void SubtrvItem_Selected(object sender, RoutedEventArgs e)
        {
            fTreeViewItem item = sender as fTreeViewItem;
            SelectedItemNummer = item.Nummer;
            DataRow calculated_item = Calculated_Item.Rows.Find(item.Nummer);
            UpdateInfoArea(calculated_item);
            Highlight();
            e.Handled = true;
        }

        private void dgr_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.AddedItems.Count > 0)
            {
                DataRow calculated_item = (e.AddedItems[0] as DataRowView).Row;
                SelectedItemNummer = calculated_item["No."].ToString();
                UpdateInfoArea(calculated_item);
            }
        }        
        #endregion

        #region Filter
        #region Verkaufsaufträge
        private void clmDescription_FilterOptionsClick(object sender, RoutedEventArgs e)
        {
            try
            {
                mWindowGetFilterString wdf = new mWindowGetFilterString();
                string filter = wdf.ShowDialog("Beschreibung", "K0", dgtv);
                if (filter != null)
                {
                    dgtv.FilterStrings.Rows.Remove("K0");
                    dgtv.FilterStrings.Rows.Add("K0", filter, typeof(string));
                    dgtv.Refresh();
                    (dgtv.Columns[0] as mDataGridColumn).DeleteFilterVisibility = true;
                }
            }
            catch (Exception ex) { MessageBox.Show(ex.ToString(), "Fehler", MessageBoxButton.OK, MessageBoxImage.Error); }
        }
        private void clmDescription_FilterDeletedClick(object sender, RoutedEventArgs e)
        {
            dgtv.FilterStrings.Rows.Remove("K0");
            dgtv.Refresh();
        }

        private void clmCustomer_FilterOptionsClick(object sender, RoutedEventArgs e)
        {
            try
            {
                mWindowGetFilterString wdf = new mWindowGetFilterString();
                string filter = wdf.ShowDialog("Kunde", "Customer", dgtv);
                if (filter != null)
                {
                    dgtv.FilterStrings.Rows.Remove("Customer");
                    dgtv.FilterStrings.Rows.Add("Customer", filter, typeof(string));
                    dgtv.Refresh();
                    (dgtv.Columns[2] as mDataGridColumn).DeleteFilterVisibility = true;
                }
            }
            catch (Exception ex) { MessageBox.Show(ex.ToString(), "Fehler", MessageBoxButton.OK, MessageBoxImage.Error); }
        }
        private void clmCustomer_FilterDeletedClick(object sender, RoutedEventArgs e)
        {
            dgtv.FilterStrings.Rows.Remove("Customer");
            dgtv.Refresh();
        }

        private void clmDate_FilterOptionsClick(object sender, RoutedEventArgs e)
        {
            try
            {
                mWindowGetFilterString wdf = new mWindowGetFilterString();
                string filter = wdf.ShowDialog("Liefertermin", "Liefertermin", dgtv);
                if (filter != null)
                {
                    dgtv.FilterStrings.Rows.Remove("Liefertermin");
                    dgtv.FilterStrings.Rows.Add("Liefertermin", filter, typeof(DateTime));
                    dgtv.Refresh();
                    (dgtv.Columns[2] as mDataGridColumn).DeleteFilterVisibility = true;
                }
            }
            catch (Exception ex) { MessageBox.Show(ex.ToString(), "Fehler", MessageBoxButton.OK, MessageBoxImage.Error); }
        }
        private void clmDate_FilterDeletedClick(object sender, RoutedEventArgs e)
        {
            dgtv.FilterStrings.Rows.Remove("Liefertermin");
            dgtv.Refresh();
        }

        private void clmAvailability_FilterOptionsClick(object sender, RoutedEventArgs e)
        {
            try
            {
                mWindowGetFilterString wdf = new mWindowGetFilterString();
                string filter = wdf.ShowDialog("Verfügbarkeit", "Available", dgtv);
                if (filter != null)
                {
                    dgtv.FilterStrings.Rows.Remove("Available");
                    dgtv.FilterStrings.Rows.Add("Available", filter, typeof(DateTime));
                    dgtv.Refresh();
                    (dgtv.Columns[3] as mDataGridColumn).DeleteFilterVisibility = true;
                }
            }
            catch (Exception ex) { MessageBox.Show(ex.ToString(), "Fehler", MessageBoxButton.OK, MessageBoxImage.Error); }
        }
        private void clmAvailability_FilterDeletedClick(object sender, RoutedEventArgs e)
        {
            dgtv.FilterStrings.Rows.Remove("Available");
            dgtv.Refresh();
        }
        #endregion

        #region Artikel
        List<string> replenishment = new List<string>();
        private void dgrReplenishment_FilterOptionsClick(object sender, RoutedEventArgs e)
        {
            try
            {
                mWindowGetFilterString wdf = new mWindowGetFilterString();
                replenishment = wdf.ShowDialog("Beschaffungsmethode", replenishment, "Einkauf", "Fertigungsauftrag");

                if (replenishment != null)
                    Calculated_Item_View.RowFilter = ConcatFilters();
                if (replenishment != null && replenishment.Count > 0)
                    dgrReplenishment.DeleteFilterVisibility = true;
                else
                    dgrReplenishment.DeleteFilterVisibility = false;
            }
            catch (Exception ex) { MessageBox.Show(ex.ToString(), "Fehler", MessageBoxButton.OK, MessageBoxImage.Error); }
        }
        private void dgrReplenishment_DeleteFilterClick(object sender, RoutedEventArgs e)
        {
            try
            {
                replenishment.Clear();
                Calculated_Item_View.RowFilter = ConcatFilters();
            }
            catch (Exception ex) { MessageBox.Show(ex.ToString(), "Fehler", MessageBoxButton.OK, MessageBoxImage.Error); }
        }
        string itemno = "";
        private void dgrItemNo_FilterOptionsClick(object sender, RoutedEventArgs e)
        {
            try
            {
                mWindowGetFilterString wdf = new mWindowGetFilterString();
                itemno = wdf.ShowDialog("Artikelnummer", itemno);
                if (itemno != null)
                    Calculated_Item_View.RowFilter = ConcatFilters();
                if (itemno != null && itemno != "")
                    dgrItemNo.DeleteFilterVisibility = true;
                else
                    dgrItemNo.DeleteFilterVisibility = false;
            }
            catch (Exception ex) { MessageBox.Show(ex.ToString(), "Fehler", MessageBoxButton.OK, MessageBoxImage.Error); }
        }
        private void dgrItemNo_DeleteFilterClick(object sender, RoutedEventArgs e)
        {
            try
            {
                itemno = "";
                Calculated_Item_View.RowFilter = ConcatFilters();
            }
            catch (Exception ex) { MessageBox.Show(ex.ToString(), "Fehler", MessageBoxButton.OK, MessageBoxImage.Error); }
        }
        string description = "";
        private void dgrDescription_FilterOptionsClick(object sender, RoutedEventArgs e)
        {
            try
            {
                mWindowGetFilterString wdf = new mWindowGetFilterString();
                description = wdf.ShowDialog("Beschreibung", description);
                if (description != null)
                    Calculated_Item_View.RowFilter = ConcatFilters();
                if (description != null && description != "")
                    dgrDescription.DeleteFilterVisibility = true;
                else
                    dgrDescription.DeleteFilterVisibility = false;
            }
            catch (Exception ex) { MessageBox.Show(ex.ToString(), "Fehler", MessageBoxButton.OK, MessageBoxImage.Error); }
        }
        private void dgrDescription_DeleteFilterClick(object sender, RoutedEventArgs e)
        {
            try
            {
                description = "";
                Calculated_Item_View.RowFilter = ConcatFilters();
            }
            catch (Exception ex) { MessageBox.Show(ex.ToString(), "Fehler", MessageBoxButton.OK, MessageBoxImage.Error); }
        }
        string nextpriority = "";
        private void dgrNextPriority_FilterOptionsClick(object sender, RoutedEventArgs e)
        {
            try
            {
                mWindowGetFilterString wdf = new mWindowGetFilterString();
                nextpriority = wdf.ShowDialog("Nächste Priorität", nextpriority);
                if (nextpriority != null)
                    Calculated_Item_View.RowFilter = ConcatFilters();
                if (nextpriority != null && nextpriority != "")
                    dgrNextPriority.DeleteFilterVisibility = true;
                else
                    dgrNextPriority.DeleteFilterVisibility = false;
            }
            catch (Exception ex) { MessageBox.Show(ex.ToString(), "Fehler", MessageBoxButton.OK, MessageBoxImage.Error); }
        }
        private void dgrNextPriority_DeleteFilterClick(object sender, RoutedEventArgs e)
        {
            try
            {
                nextpriority = "";
                Calculated_Item_View.RowFilter = ConcatFilters();
            }
            catch (Exception ex) { MessageBox.Show(ex.ToString(), "Fehler", MessageBoxButton.OK, MessageBoxImage.Error); }
        }
        string mengebis = "";
        private void dgrMengeBis_FilterOptionsClick(object sender, RoutedEventArgs e)
        {
            try
            {
                mWindowGetFilterString wdf = new mWindowGetFilterString();
                mengebis = wdf.ShowDialog("Beschreibung", mengebis);
                if (mengebis != null)
                    Calculated_Item_View.RowFilter = ConcatFilters();
                if (mengebis != null && mengebis != "")
                    dgrMengeBis.DeleteFilterVisibility = true;
                else
                    dgrMengeBis.DeleteFilterVisibility = false;
            }
            catch (Exception ex) { MessageBox.Show(ex.ToString(), "Fehler", MessageBoxButton.OK, MessageBoxImage.Error); }
        }
        private void dgrMengeBis_DeleteFilterClick(object sender, RoutedEventArgs e)
        {
            try
            {
                mengebis = "";
                Calculated_Item_View.RowFilter = ConcatFilters();
            }
            catch (Exception ex) { MessageBox.Show(ex.ToString(), "Fehler", MessageBoxButton.OK, MessageBoxImage.Error); }
        }
        List<string> action = new List<string>();
        private void dgrAction_FilterOptionsClick(object sender, RoutedEventArgs e)
        {
            try
            {
                mWindowGetFilterString wdf = new mWindowGetFilterString();
                action = wdf.ShowDialog("Aktion", action, "auf Materialeingang warten", "Material bestellen", "Material anfordern", "in Fertigung", "auf Material warten", "Fertigen");
                if (action != null)
                    Calculated_Item_View.RowFilter = ConcatFilters();
                if (action != null && action.Count > 0)
                    dgrAction.DeleteFilterVisibility = true;
                else
                    dgrAction.DeleteFilterVisibility = false;
            }
            catch (Exception ex) { MessageBox.Show(ex.ToString(), "Fehler", MessageBoxButton.OK, MessageBoxImage.Error); }
        }
        private void dgrAction_DeleteFilterClick(object sender, RoutedEventArgs e)
        {
            try
            {
                action.Clear();
                Calculated_Item_View.RowFilter = ConcatFilters();
            }
            catch (Exception ex) { MessageBox.Show(ex.ToString(), "Fehler", MessageBoxButton.OK, MessageBoxImage.Error); }
        }

        List<string> location = new List<string>();
        private void dgrLocation_FilterOptionsClick(object sender, RoutedEventArgs e)
        {
            try
            {
                mWindowGetFilterString wdf = new mWindowGetFilterString();
                var result = from row in Calculated_Item.AsEnumerable()
                             group row by row.Field<object>("Location") into grp
                             select new
                             {
                                 Location = (string)grp.Key
                             };
                location = wdf.ShowDialog("Fertigungsstelle", location, result.ToArray<object>());
                if (location != null)
                    Calculated_Item_View.RowFilter = ConcatFilters();
                if (location != null && location.Count > 0)
                    dgrLocation.DeleteFilterVisibility = true;
                else
                    dgrLocation.DeleteFilterVisibility = false;
            }
            catch (Exception ex) { MessageBox.Show(ex.ToString(), "Fehler", MessageBoxButton.OK, MessageBoxImage.Error); }
        }
        private void dgrLocation_DeleteFilterClick(object sender, RoutedEventArgs e)
        {
            try
            {
                location.Clear();
                Calculated_Item_View.RowFilter = ConcatFilters();
            }
            catch (Exception ex) { MessageBox.Show(ex.ToString(), "Fehler", MessageBoxButton.OK, MessageBoxImage.Error); }
        }
        string vendorno = "";
        private void dgrVendorNo_FilterOptionsClick(object sender, RoutedEventArgs e)
        {
            try
            {
                mWindowGetFilterString wdf = new mWindowGetFilterString();
                vendorno = wdf.ShowDialog("Kreditor Nr.", vendorno);
                if (vendorno != null)
                    Calculated_Item_View.RowFilter = ConcatFilters();
                if (vendorno != null && vendorno != "")
                    dgrVendorNo.DeleteFilterVisibility = true;
                else
                    dgrVendorNo.DeleteFilterVisibility = false;
            }
            catch (Exception ex) { MessageBox.Show(ex.ToString(), "Fehler", MessageBoxButton.OK, MessageBoxImage.Error); }
        }
        private void dgrVendorNo_DeleteFilterClick(object sender, RoutedEventArgs e)
        {
            try
            {
                vendorno = "";
                Calculated_Item_View.RowFilter = ConcatFilters();
            }
            catch (Exception ex) { MessageBox.Show(ex.ToString(), "Fehler", MessageBoxButton.OK, MessageBoxImage.Error); }
        }
        string vendorname = "";
        private void dgrVendorName_FilterOptionsClick(object sender, RoutedEventArgs e)
        {
            try
            {
                mWindowGetFilterString wdf = new mWindowGetFilterString();
                vendorname = wdf.ShowDialog("Kreditor Name", vendorname);
                if (vendorname != null)
                    Calculated_Item_View.RowFilter = ConcatFilters();
                if (vendorname != null && vendorname != "")
                    dgrVendorName.DeleteFilterVisibility = true;
                else
                    dgrVendorName.DeleteFilterVisibility = false;
            }
            catch (Exception ex) { MessageBox.Show(ex.ToString(), "Fehler", MessageBoxButton.OK, MessageBoxImage.Error); }
        }
        private void dgrVendorName_DeleteFilterClick(object sender, RoutedEventArgs e)
        {
            try
            {
                vendorname = "";
                Calculated_Item_View.RowFilter = ConcatFilters();
            }
            catch (Exception ex) { MessageBox.Show(ex.ToString(), "Fehler", MessageBoxButton.OK, MessageBoxImage.Error); }
        }
        List<string> mitarbeiter = new List<string>();
        private void dgrMitarbeiter_FilterOptionsClick(object sender, RoutedEventArgs e)
        {
            try
            {
                mWindowGetFilterString wdf = new mWindowGetFilterString();
                var result = from row in Calculated_Item.AsEnumerable()
                             group row by row.Field<object>("Mitarbeiter") into grp
                             select new
                             {
                                 Mitarbeiter = (string)grp.Key
                             };
                
                mitarbeiter = wdf.ShowDialog("Mitarbeiter", mitarbeiter, result.ToArray<object>());
                foreach (var ti in result)
                { }
                if (mitarbeiter != null)
                    Calculated_Item_View.RowFilter = ConcatFilters();
                if (mitarbeiter != null && mitarbeiter.Count > 0)
                    dgrMitarbeiter.DeleteFilterVisibility = true;
                else
                    dgrMitarbeiter.DeleteFilterVisibility = false;
            }
            catch (Exception ex) { MessageBox.Show(ex.ToString(), "Fehler", MessageBoxButton.OK, MessageBoxImage.Error); }
        }
        private void dgrMitarbeiter_FilterDeletedClick(object sender, RoutedEventArgs e)
        {
            try
            {
                mitarbeiter.Clear();
                Calculated_Item_View.RowFilter = ConcatFilters();
            }
            catch (Exception ex) { MessageBox.Show(ex.ToString(), "Fehler", MessageBoxButton.OK, MessageBoxImage.Error); }
        }
        #endregion

        private void mitmResetFilters_Click(object sender, RoutedEventArgs e)
        {
            replenishment.Clear();
            action.Clear();
            mitarbeiter.Clear();
            location.Clear();
            itemno = description = nextpriority = mengebis = vendorno = vendorname = "";
            Calculated_Item_View.RowFilter = "";

            dgtv.FilterStrings.Clear();
            dgtv.Refresh();

            (dgtv.Columns[0] as mDataGridColumn).DeleteFilterVisibility = false;
            (dgtv.Columns[2] as mDataGridColumn).DeleteFilterVisibility = false;
            (dgtv.Columns[3] as mDataGridColumn).DeleteFilterVisibility = false;

            dgrReplenishment.DeleteFilterVisibility = false;
            dgrItemNo.DeleteFilterVisibility = false;
            dgrDescription.DeleteFilterVisibility = false;
            dgrNextPriority.DeleteFilterVisibility = false;
            dgrMengeBis.DeleteFilterVisibility = false;
            dgrAction.DeleteFilterVisibility = false;
            dgrLocation.DeleteFilterVisibility = false;
            dgrVendorNo.DeleteFilterVisibility = false;
            dgrVendorName.DeleteFilterVisibility = false;


        }

        private string ConcatFilters()
        {
            string result = "";
            try
            {
                if (replenishment != null && replenishment.Count > 0)
                    result += FilterBuilder("Replenishment System", replenishment);
                if (itemno != null && itemno != "")
                {
                    if (result != "")
                        result += " AND ";
                    result += FilterBuilder("No.", itemno);
                }
                if (nextpriority != null && nextpriority != "")
                {
                    if (result != "")
                        result += " AND ";
                    result += FilterBuilder("Nächste Priorität Zahl", nextpriority);
                }
                if (mengebis != null && mengebis != "")
                {
                    if (result != "")
                        result += " AND ";
                    result += FilterBuilder("Menge bis Priorität", mengebis);
                }
                if (action != null && action.Count > 0)
                {
                    if (result != "")
                        result += " AND ";
                    result += FilterBuilder("Action", action);
                }
                if (description != null && description != "")
                {
                    if (result != "")
                        result += " AND ";
                    result += FilterBuilder("Description", description);
                }
                if (location != null && location.Count > 0)
                {
                    if (result != "")
                        result += " AND ";
                    result += FilterBuilder("Location", location);
                }
                if (vendorno != null && vendorno != "")
                {
                    if (result != "")
                        result += " AND ";
                    result += FilterBuilder("Vendor No.", vendorno);
                }
                if (vendorname != null && vendorname != "")
                {
                    if (result != "")
                        result += " AND ";
                    result += FilterBuilder("Vendor Name", vendorname);
                }
                if (mitarbeiter != null && mitarbeiter.Count > 0)
                {
                    if (result != "")
                        result += " AND ";
                    result += FilterBuilder("Mitarbeiter", mitarbeiter);
                }
            }
            catch (Exception ex) { MessageBox.Show(ex.ToString(), "Fehler", MessageBoxButton.OK, MessageBoxImage.Error); }
            if (result.EndsWith("And "))
                result.Remove(result.Length - 4, 4);
            return result;
        }

        private string FilterBuilder(string columnname, string filter)
        {
            string result = "";
            try
            {                
                if (filter != "")
                {
                    if (filter.Contains('*'))
                    {
                        filter = filter.Replace('*', '%');
                        result = String.Format("[{0}] LIKE '{1}'", columnname, filter);
                    }
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
                        result = String.Format("[{0}] LIKE '%{1}%'", columnname, filter);
                    }
                }                
            }
            catch (Exception ex) { MessageBox.Show("Fehler in FilterBuilder: result = " + result + "\n" + ex.ToString(), "Fehler", MessageBoxButton.OK, MessageBoxImage.Error); }
            return result;
        }

        private string FilterBuilder(string columnname, List<string> filters)
        {
            string include = "";
            string exclude = "";
            try
            {
                foreach (string filter in filters)
                {
                    include += String.Format("[{0}] = '{1}' or ", columnname, filter);
                }
                if (include.EndsWith(" or "))
                    include = include.Remove(include.Length - 4, 4);
                if (exclude.EndsWith(" and "))
                    exclude = exclude.Remove(exclude.Length - 5, 5);
            }
            catch (Exception ex) { MessageBox.Show("Fehler in FilterBuilder: result = " + include + "\n" + ex.ToString(), "Fehler", MessageBoxButton.OK, MessageBoxImage.Error); }
            if (include != "" && exclude != "")
                return String.Format("({0} and {1})", include, exclude);
            else
                return "(" + include + exclude + ")";
        }
        #endregion
    }

    class InSales
    {
        public string Menge { get; set; }
        /// <summary>
        /// Sales Header number
        /// </summary>
        public string Nr { get; set; }
        public string Priorität
        {
            get { return PrioritätBuchstaben + PrioritätZahl.ToString(); }
        }
        public string PrioritätBuchstaben { get; set; }
        public int PrioritätZahl { get; set; }
        public string Kunde { get; set; }
        public string Artikel { get; set; }
        public InSales(string amount, string no, string prioritätBuchstaben, int prioritätZahl, string customer, string item)
        {
            Menge = amount;
            Nr = no;
            PrioritätBuchstaben = prioritätBuchstaben;
            PrioritätZahl = prioritätZahl; 
            Kunde = customer;
            Artikel = item;
        }
    }
}
