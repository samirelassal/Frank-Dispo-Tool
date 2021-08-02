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
                    return (trv.SelectedItem as mTreeViewItem).Caption;
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
                List<mTreeViewItem> duplicate = trvItem.FindExisting(trv.Items);
                DataRow calculated_item = Calculated_Item.Rows.Find(dr["Child No."]);
                mTreeViewItem subtrvItem = null;

                if (calculated_item != null)
                {

                    subtrvItem = new mTreeViewItem(dr["Quantity"].ToString(), dr["Child No."].ToString(), calculated_item["Status"].ToString());
                    subtrvItem.Expanded += new RoutedEventHandler(trvItem_Expanded);
                    if (duplicate.Count == 0)
                    {
                        trvItem.Expanded += new RoutedEventHandler(trvItem_Expanded);
                        trv.Items.Add(trvItem);
                        trvItem.Items.Add(subtrvItem);
                    }
                    else
                    {
                        duplicate[0].Items.Add(subtrvItem);
                    }
                }
            }
        }

        private void trvItem_Expanded(object sender, RoutedEventArgs e) 
        {
            mTreeViewItem trvItm = sender as mTreeViewItem;
            trvItm.Reset();
            foreach (mTreeViewItem subItm in trvItm.Items)
            {
                #region Load Subitems on demand
                if (subItm.Items.Count == 0)
                {
                    DataRow[] items = Parent_Child_Item.Select("[Parent No.] = '" + subItm.Caption.ToString() + "'");
                    foreach (DataRow dr in items)
                    {
                        DataRow calculated_item = Calculated_Item.Rows.Find(dr["Child No."]);
                        if (calculated_item != null)
                        {
                            mTreeViewItem subsubItem = new mTreeViewItem(dr["Quantity"].ToString(), dr["Child No."].ToString(), Calculated_Item.Rows.Find(dr["Child No."])["Status"].ToString());
                            subsubItem.Expanded += new RoutedEventHandler(trvItem_Expanded);
                            subItm.Items.Add(subsubItem);
                        }
                    }
                }
                #endregion
                
                if (subItm.Contains(SelectedItemCaption))
                    subItm.Highlight();
                else
                    subItm.Reset();
            }
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
            DataRow calculated_item = Calculated_Item.Rows.Find(item.Caption);

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

                if (calculated_item["Replenishment System"].ToString() == "Einkauf")
                {
                    switch (item.status)
                    {
                        case Status.Red: tblErforderlicheAktion.Text = "Dieser Artikel ist nicht oft genug verfügbar. Er muss bestellt werden."; break;
                        case Status.Yellow: tblErforderlicheAktion.Text = "Dieser Artikel ist nicht oft genug verfügbar. Er wurde allerdings bereits bestellt. Es ist keine weitere Aktion erforderlich."; break;
                        case Status.Green: tblErforderlicheAktion.Text = "Dieser Artikel ist auf Lager. Es ist keine weitere Aktion erforderlich."; break;
                    }
                }
                else
                {
                    switch (item.status)
                    {
                        case Status.Red: tblErforderlicheAktion.Text = "Dieser Artikel kann nicht hergestellt werden."; break;
                        case Status.Yellow: tblErforderlicheAktion.Text = "Dieser Artikel kann produziert werden, jedoch wird dadurch die Produktion anderer Artikel behindert."; break;
                        case Status.Green: tblErforderlicheAktion.Text = "Dieser Artikel kann produziert werden."; break;
                    }
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
                if (itm.Contains(SelectedItemCaption))
                    itm.Highlight();
                else
                    itm.Reset();
            }
            item.Reset();
            #endregion
        }
    }
}
