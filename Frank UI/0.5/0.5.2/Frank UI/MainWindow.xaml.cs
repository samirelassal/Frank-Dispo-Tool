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

namespace Frank_UI
{
    /// <summary>
    /// Interaktionslogik für MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        int port_broadcast = 15000;
        int port_communication = 8081;

        byte[] Calculated_Item_File;
        byte[] Calculated_Sale_File;
        byte[] Parent_Child_Item_File;

        static readonly string FileLocation = System.AppDomain.CurrentDomain.BaseDirectory;
        static readonly string Calculated_Item_Path = FileLocation + @"\calculated_item.xml";
        static readonly string Calculated_Sale_Path = FileLocation + @"\calculated_sale.xml";
        static readonly string Parent_Child_Item_Path = FileLocation + @"\parent_child_item.xml";

        DataTable Calculated_Item;
        DataTable Calculated_Sale;
        DataTable Parent_Child_Item;

        string SelectedItemNummer;


        public MainWindow()
        {
            InitializeComponent();
            GetData();
            OpenData();
            getSalesHeaders();
            updateStatusbar();
        }

        private void GetData()
        {
            #region Send broadcast message and recieve server IP
            UdpClient udp = new UdpClient();
            IPEndPoint ipendpoint = new IPEndPoint(IPAddress.Broadcast, port_broadcast);
            string message = "requesting server; frank ui";
            byte[] data = UTF8Encoding.UTF8.GetBytes(message);
            udp.Send(data, data.Length, ipendpoint);
            data = udp.Receive(ref ipendpoint);
            message = UTF8Encoding.UTF8.GetString(data);
            #endregion

            #region Connect to server and recieve data
            TcpClient tcp = new TcpClient();
            ipendpoint = new IPEndPoint(IPAddress.Parse(message), port_communication);            
            try
            {
                tcp.Connect(ipendpoint);
                NetworkStream stream = tcp.GetStream();
                data = UTF8Encoding.UTF8.GetBytes("requesting information");
                stream.Write(data, 0, data.Length);
                data = new byte[1024];
                stream.Read(data, 0, data.Length);
                message = UTF8Encoding.UTF8.GetString(data).Trim('\0');   
             
                int Calculated_Item_Size = int.Parse(message.Split('|')[0].Split(':')[1]);
                int Calculated_Sale_Size = int.Parse(message.Split('|')[1].Split(':')[1]);
                int Parent_Child_Item_Size = int.Parse(message.Split('|')[2].Split(':')[1]);
                
                Calculated_Item_File = new byte[Calculated_Item_Size];
                Calculated_Sale_File = new byte[Calculated_Sale_Size];
                Parent_Child_Item_File = new byte[Parent_Child_Item_Size];
                
                data = UTF8Encoding.UTF8.GetBytes("requesting Calculated_Item");
                stream.Write(data, 0, data.Length);
                stream.Read(Calculated_Item_File, 0, Calculated_Item_Size);
                
                data = UTF8Encoding.UTF8.GetBytes("requesting Calculated_Sale");
                stream.Write(data, 0, data.Length);
                stream.Read(Calculated_Sale_File, 0, Calculated_Sale_Size);

                data = UTF8Encoding.UTF8.GetBytes("requesting Parent_Child_Item");
                stream.Write(data, 0, data.Length);
                stream.Read(Parent_Child_Item_File, 0, Parent_Child_Item_Size);

                File.WriteAllBytes(Calculated_Item_Path, Calculated_Item_File);
                File.WriteAllBytes(Calculated_Sale_Path, Calculated_Sale_File);
                File.WriteAllBytes(Parent_Child_Item_Path, Parent_Child_Item_File);
                tcp.Close();
            }
            catch (Exception ex) 
            {
                MessageBox.Show(ex.GetType().ToString() + ": " + ex.Message, "Fehler", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            #endregion
        }

        private void OpenData()
        {
            Calculated_Item = new DataTable();
            Calculated_Sale = new DataTable();
            Parent_Child_Item = new DataTable();
            try
            {
                Calculated_Item.TableName = "Calculated_Item";
                Calculated_Sale.TableName = "Calculated_Sale";
                Parent_Child_Item.TableName = "Parent_Child_Item";
                Calculated_Item.ReadXml(Calculated_Item_Path);
                Calculated_Sale.ReadXml(Calculated_Sale_Path);
                Parent_Child_Item.ReadXml(Parent_Child_Item_Path);
            }
            catch (Exception ex) 
            {
                MessageBox.Show(ex.GetType().ToString() + ": " + ex.Message, "Fehler", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void getSalesHeaders() 
        {
            DataRow[] SalesHeaders = Parent_Child_Item.Select("[Parent No.] Like 'VB%'");

            foreach (DataRow dr in SalesHeaders)
            {
                if (!dgtv.KeyExists(dr["Parent No."].ToString()))
                {
                    List<fTreeViewItem> subitems = LoadRekursive(dr["Parent No."].ToString());
                    if (subitems.Count > 0)
                    {
                        DataRow sale = Calculated_Sale.Select("[No.] = '" + dr["Parent No."] + "'")[0];
                        string status = "green";
                        foreach (fTreeViewItem itm in subitems)
                        {
                            if (itm.Status == "yellow" && status != "red")
                                status = "yellow";
                            if (itm.Status == "red")
                                status = "red";
                        }
                        dgtv.Add(dr["Parent No."].ToString(), subitems.Cast<object>().ToList(), status, sale["customer"].ToString(), sale["Liefertermin nach KW"].ToString());

                    }
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

        private void updateStatusbar() 
        {
            lblNumberOfSales.Content = dgtv.NumberOfItems.ToString();
            lblNumberofItems.Content = Calculated_Item.Rows.Count.ToString();
        }

        private void trvItem_Expanded(object sender, RoutedEventArgs e)
        {
            mTreeViewItem trvItm = sender as mTreeViewItem;
            trvItm.Background = new SolidColorBrush(Colors.Transparent);
            HighlightRekursive(trvItm, SelectedItemNummer);
            e.Handled = true;

            #region Menu-Item Expand and Collapse
            mitmExpandAll.IsEnabled = false;
            foreach (mTreeViewItem itm in mTreeViewItem.mTreeViewItemCollection)
            {
                if (!itm.IsExpanded)
                {
                    mitmExpandAll.IsEnabled = true;
                    break;
                }
            }
            mitmCollapseAll.IsEnabled = true;
            #endregion
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

        private void SubtrvItem_Selected(object sender, RoutedEventArgs e)
        {
            fTreeViewItem item = sender as fTreeViewItem;
            #region Infomrations-Bereich
            DataRow calculated_item = Calculated_Item.Rows.Find(item.Nummer);

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
                    InSales insale = new InSales(dr["Required Quantity"].ToString(), dr["No."].ToString(), dr["Liefertermin nach KW"].ToString(), dr["Customer"].ToString(), dr["Parent Item No."] + " " + dr["Description"]);
                    insales.Add(insale);
                }
                dgrdInSales.ItemsSource = insales;
                e.Handled = true;
            }
            #endregion

            #region Highlighting
            string nummer = SelectedItemNummer = item.Nummer;
            foreach (mTreeViewItem itm in mTreeViewItem.mTreeViewItemCollection)
            {
                //Temporary Solution
                if (itm.Header != null)
                {
                    bool contains = itm.Header.ToString() == nummer;
                    foreach (fTreeViewItem subitm in itm.Items)
                    {
                        if (contains)
                            break;
                        contains = contains | subitm.Contains(nummer);
                    }
                    if (contains)
                    {
                        if (itm.IsExpanded)
                            HighlightRekursive(itm, nummer);
                        else
                            itm.Background = (SolidColorBrush)ResourceHelper.dict["Highlight"];
                    }
                    else
                        itm.Background = new SolidColorBrush(Colors.Transparent);
                }
            }
            #endregion
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

        private void clmDescription_FilterOptionsClick(object sender, RoutedEventArgs e)
        {
            WindowGetFilterString wdf = new WindowGetFilterString();
            string filter = wdf.ShowDialog();
            dgtv.UseFilter = filter != "";
            dgtv.FilterStrings.Remove("K0");
            dgtv.FilterStrings.Add("K0", filter);
            dgtv.Refresh();
        }

        private void clmCustomer_FilterOptionsClick(object sender, RoutedEventArgs e)
        {
            WindowGetFilterString wdf = new WindowGetFilterString();
            string filter = wdf.ShowDialog();
            dgtv.UseFilter = filter != "";
            dgtv.FilterStrings.Remove("K1");
            dgtv.FilterStrings.Add("K1", filter);
            dgtv.Refresh();
        }

        private void clmDate_FilterOptionsClick(object sender, RoutedEventArgs e)
        {
            WindowGetFilterString wdf = new WindowGetFilterString();
            string filter = wdf.ShowDialog();
            dgtv.UseFilter = filter != "";
            dgtv.FilterStrings.Remove("K2");
            dgtv.FilterStrings.Add("K2", filter);
            dgtv.Refresh();
        }
    }
}
