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

        string SelectedItemCaption 
        {
            get
            {
                try
                {
                    return (trv.SelectedItem as mTreeViewItem).Nummer;
                }
                catch { return ""; }
            }
        }


        public MainWindow()
        {
            InitializeComponent();
            //GetData();
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
                mTreeViewItem trvItem = new mTreeViewItem(null, dr["Parent No."].ToString(), "error");
                DataRow sale = Calculated_Sale.Select("[No.] = '" + dr["Parent No."] + "'")[0];
                trvItem.Columns.Add(new Column(sale["Liefertermin nach KW"].ToString()));
                List<mTreeViewItem> duplicate = trvItem.FindExisting(trv.Items);
                
                if (duplicate.Count == 0 && LoadRekursive(trvItem))
                {
                    trvItem.Expanded += new RoutedEventHandler(trvItem_Expanded);
                    trvItem.Collapsed += new RoutedEventHandler(trvItem_Collapsed);
                    trv.Items.Add(trvItem);
                }
            }
        }

        private void updateStatusbar() 
        {
            lblNumberOfSales.Content = trv.Items.Count.ToString();
            lblNumberofItems.Content = Calculated_Item.Rows.Count.ToString();
        }

        //If there are no subitems in item, result is false
        private bool LoadRekursive(mTreeViewItem item) 
        {
            bool result = false;
            DataRow[] Parent = Parent_Child_Item.Select("[Parent No.] = '" + item.Nummer + "'");
            foreach (DataRow dr in Parent)
            {
                DataRow calculated_item = Calculated_Item.Rows.Find(dr["Child No."]);
                mTreeViewItem subtrvItem = null;

                if (calculated_item != null)
                {
                    result = true;
                    subtrvItem = new mTreeViewItem(dr["Quantity"].ToString(), dr["Child No."].ToString(), dr["Status"].ToString());
                    subtrvItem.Expanded += new RoutedEventHandler(trvItem_Expanded);
                    subtrvItem.Collapsed += new RoutedEventHandler(trvItem_Collapsed);
                    item.Items.Add(subtrvItem);
                    LoadRekursive(subtrvItem);
                }
            }
            return result;
        }

        private void trvItem_Expanded(object sender, RoutedEventArgs e) 
        {
            mTreeViewItem trvItm = sender as mTreeViewItem;
            trvItm.Reset();
            foreach (mTreeViewItem subItm in trvItm.Items)
            {                
                if (subItm.Contains(SelectedItemCaption))
                    subItm.Highlight();
                else
                    subItm.Reset();
            }
            e.Handled = true;
        }

        private void trvItem_Collapsed(object sender, RoutedEventArgs e) 
        {
            mTreeViewItem item = (sender as mTreeViewItem);
            if (item.Contains(SelectedItemCaption))
                item.Highlight();
            else
                item.Reset();
            e.Handled = true;
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

        private void trv_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            mTreeViewItem item = e.NewValue as mTreeViewItem;

            #region Infomrations-Bereich
            lbxInItems.Items.Clear();           
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
                lblFehlendeMenge.Content = calculated_item["Missing Quantity"];
                lblFertigungsstelle.Content = calculated_item["Location"];

                switch (item.status)
                {
                    case Status.Red: tblErforderlicheAktion.Text = "Dieser Artikel ist nicht ausreichend Verfügbar"; break;
                    case Status.Yellow: tblErforderlicheAktion.Text = "Dieser Artikel ist ausreichend für die übergeordnete Bestellung verfügbar, jedoch wird er nach Abwicklung dieser Bestellung evtl. für andere Bestellungen fehlen."; break;
                    case Status.Green: tblErforderlicheAktion.Text = "Dieser Artikel ist ausreichend auf Lager. Es ist keine weitere Aktion erforderlich."; break;
                }

                DataRow[] ParentItems = Parent_Child_Item.Select("[Child No.] = '" + calculated_item["No."] + "'");
                foreach (DataRow dr in ParentItems)
                {
                    lbxInItems.Items.Add(dr["Quantity"] + " x in " + dr["Parent No."]);
                }

                DataRow[] ParentOrder = Calculated_Sale.Select("[Item No.] = '" + calculated_item["No."] + "'");
                List<InSales> insales = new List<InSales>();
                foreach (DataRow dr in ParentOrder)
                {
                    InSales insale = new InSales(dr["Required Quantity"].ToString(), dr["No."].ToString(), dr["Liefertermin nach KW"].ToString(), dr["Customer"].ToString(), dr["Parent Item No."] + " " + dr["Description"]);
                    insales.Add(insale);
                }
                dgrdInSales.ItemsSource = insales;
            }
            #endregion
            #region TreeView
            if (e.OldValue != null)
                (e.OldValue as mTreeViewItem).Reset();
            foreach (mTreeViewItem itm in trv.Items) 
            {
                if (itm.IsExpanded)
                {
                    itm.Reset();
                    HighlightRekursive(itm, SelectedItemCaption);
                }
                else
                {
                    if (itm.Contains(SelectedItemCaption))
                        itm.Highlight();
                    else
                        itm.Reset();
                }
            }
            item.Reset();
            #endregion
        }

        private void HighlightRekursive(mTreeViewItem item, string caption) 
        {
            foreach (mTreeViewItem itm in item.Items) 
            {
                if (itm.IsExpanded)
                {
                    itm.Reset();
                    HighlightRekursive(itm, caption);
                }
                else
                {
                    if (itm.Contains(caption))
                        itm.Highlight();
                    else
                        itm.Reset();
                }
            }
        }

        private void txtSearch_TextChanged(object sender, TextChangedEventArgs e)
        {
            string searchtext = txtSearch.Text.Trim();
            foreach (mTreeViewItem itm in trv.Items)
            {
                if (itm.IsExpanded)
                {
                    itm.Reset();
                    HighlightRekursive(itm, searchtext);
                }
                else
                {
                    if (itm.Contains(searchtext))
                        itm.Highlight();
                    else
                        itm.Reset();
                }
            }
        }
    }
}
