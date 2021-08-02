using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using System.Data.Odbc;
using System.Diagnostics;
using System.Web;
using PdfSharp.Pdf;
using MigraDoc;
using MigraDoc.DocumentObjectModel;
using MigraDoc.DocumentObjectModel.Shapes;
using MigraDoc.DocumentObjectModel.Tables;
using MigraDoc.Rendering;
using System.IO;
using Helper_Library;

namespace Frank_SO_Demand_Report
{
    public static class Extensions
    {
        public static List<DataRow> findRows(this DataTable table, DataColumn dc, object value)
        {
            List<DataRow> result = new List<DataRow>();
            foreach (DataRow dr in table.Rows)
            {
                if (value.Equals(dr[dc]))
                    result.Add(dr);
            }
            return result;
        }

        public static List<DataRow> findRows(this DataTable table, string columnname, object value)
        {
            DataColumn dc = table.Columns[columnname];
            List<DataRow> result = new List<DataRow>();
            foreach (DataRow dr in table.Rows)
            {
                if (value.Equals(dr[dc]))
                    result.Add(dr);
            }
            return result;
        }

        public static void ImportRows(this DataTable table, DataTable dt)
        {
            foreach (DataRow dr in dt.Rows)
            {
                try
                {
                    table.ImportRow(dr);
                }
                catch (ConstraintException) { };
            }
        }
    }

    public class Report
    {
        enum Status 
        {
            Red,
            Yellow,
            Green,
            Error
        }

        #region Data
        OdbcConnection connection = new OdbcConnection("DSN=Navision Frank-Backup");
        OdbcDataAdapter adapter;
        //DataSet ds = new DataSet();

        //Die folgenden Tabellen enthalten ALLE Navision-Datensätze der entsprechenden Tabelle
        DataTable Item = new DataTable("Item");
        DataTable ProdBOMHeader = new DataTable("Production BOM Header");
        DataTable ProdBOMLine = new DataTable("Production BOM Line");
        DataTable SalesLine = new DataTable("Sales Line");
        DataTable SalesHeader = new DataTable("Sales Header");
        DataTable PurchaseLine = new DataTable("Purchase Line");
        DataTable ProdOrderLine = new DataTable("Prod. Order Line");
        DataTable RequisitionLine = new DataTable("Requisition Line");
        DataTable ProdOrderComponent = new DataTable("Prod. Order Component");
        DataTable ItemUnitOfMeasure = new DataTable("Item Unit of Measure");
        DataTable PurchasePrice = new DataTable("Purchase Price");

        DataTable Calculated_Item = new DataTable();
        DataTable Calculated_Sale = new DataTable();
        Parent_Child_Item Parent_Child_Item = new Parent_Child_Item();

        string strSQL;

        Document document;
        Table table;
        #endregion
        static string FileLocation = System.AppDomain.CurrentDomain.BaseDirectory;

        static void Main(string[] args)
        {
            Dictionary<string, string> arguments = new Dictionary<string, string>();
            #region Fill arguments dictionary
            for (int i = 0; i < args.Length; i += 2) 
            {
                if (args[i].StartsWith("-")) 
                {
                    arguments.Add(args[i].Remove(0, 1).Trim('"'), args[i + 1].Trim('"'));
                }
            }
            #endregion
            if (arguments.ContainsKey("filelocation"))
                FileLocation = arguments["filelocation"];
            Helper.LogFileWrite("\r\nStarted");
            Report report = new Report();
            report.Start("DSN=Navision Frank-Live", FileLocation);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="connection_string"></param>
        /// <param name="generate_report"></param>
        /// <param name="export_data_location">Describes the folder, in which the datatables are saved as xml files. If emtpy, no files will be created</param>
        public void Start(string connection_string, string export_data_location)
        {
            Stopwatch sw = new Stopwatch();
            sw.Start();
            
            connection = new OdbcConnection(connection_string);
            Console.WriteLine("Welcome to the SO Demand Report Generator. Just relax and wait until this program is done.\n");
            getSalesData();
            setupGeneratedTables();

            getPurchaseData();
            getProductionData();

            getItemData(SalesLine.Columns["No."]);
            FillCalculatedItem();
            SetParentChildItemAvailability();          
            sortCalculatedItem();
            
            if (export_data_location.Trim() != "")
            {
                Calculated_Item.TableName = "Calculated_Item";
                Calculated_Sale.TableName = "Calculated_Sale";
                try
                {
                    Calculated_Item.WriteXml(export_data_location + @"\calculated_item.xml", XmlWriteMode.WriteSchema);
                    Calculated_Sale.WriteXml(export_data_location + @"\calculated_sale.xml", XmlWriteMode.WriteSchema);
                    Parent_Child_Item.SaveData(export_data_location + @"\parent_child_item.xml");
                    SalesHeader.WriteXml(export_data_location + @"\salesheader.xml", XmlWriteMode.WriteSchema);
                }
                catch (Exception ex) 
                {
                    Helper.LogWrite(ConsoleColor.Red, export_data_location + "calculated_item.xml");
                    Helper.ErrorMessage(ex);
                    Helper.LogFileWrite("Error: {0}", ex.ToString());
                    Helper.LogFileWrite(false, export_data_location + "calculated_item.xml");
                }
            }
            sw.Stop();
            Helper.LogFileWrite("Data generated. Required time: {0}", sw.Elapsed.ToString());
        }
    
        #region Load Data
        private  void getSalesData()
        {
            Helper.LogFileWrite("Loading Sales Data...");
            //Sales Header enthält alle aktuellen Bestellungen; Status = 1 => Freigegeben
            strSQL = "select \"Document Type\", \"No.\", \"Sell-to Customer No.\", \"Sell-to Customer Name\", \"Sell-to Customer Name 2\",\"Liefertermin frühestens\"  from \"Sales Header\" where \"Sales Header\".\"Document Type\" = 1 and Status = 1";
            adapter = new OdbcDataAdapter(strSQL, connection);            
            adapter.FillSchema(SalesHeader, SchemaType.Mapped);
            SalesHeader.Columns["Liefertermin frühestens"].DataType = typeof(DateTime);            
            adapter.Fill(SalesHeader);
            SalesHeader.PrimaryKey = new DataColumn[] { SalesHeader.Columns["No."] };

            //Sales Line enthält jede Zeile jeder Bestellung
            //Zuerst wird die gesamte Tabelle geladen
            strSQL = "select \"Document No.\", \"No.\", \"Unit of Measure Code\", \"Outstanding Quantity\" as Quantity, \"Liefertermin frühestens\", \"Line No.\", \"Priorität\" from \"Sales Line\" where not  \"No.\" = ''";
            adapter = new OdbcDataAdapter(strSQL, connection);
            adapter.Fill(SalesLine);
            //Nun werden die Einträge gefiltert, es interessieren nur diejenigen, die mit einer aktuellen Bestellung verknüpft sind
            JoinFilter(SalesLine.Columns["Document No."], SalesHeader.Columns["No."]);
            Helper.LogFileWrite("Loading Sales Data: done");
        }

        private  void getItemData(DataColumn parentcolumn)
        {
            Helper.LogFileWrite("Loading Item Data...");
            //Item enthält zunächst alle Artikel
            strSQL = "select \"No.\", \"Description\", \"Description 2\", \"Replenishment System\", \"Base Unit of Measure\", \"Inventory\", \"Qty. on Sales Order\", \"Menge in Materialanforderungen\", \"Mitarbeiter\", \"Inventory Value Zero\", \"Minuten pro Stück\" from Item";
            adapter = new OdbcDataAdapter(strSQL, connection);
            adapter.FillSchema(Item, SchemaType.Mapped);
            Item.Columns["Minuten pro Stück"].DataType = typeof(double);
            Item.Columns["Inventory Value Zero"].DataType = typeof(bool);
            adapter.Fill(Item);

            //Production BOM Header enthält alle Stücklisten-Köpfe. Diese sind m:n mit Item verknüpft
            strSQL = "select \"No.\", \"Item No.\", \"Status\", \"Fertigungsstelle\", \"Recommended Prod. Quantity\" from \"Production BOM Header\" where \"No.\" LIKE 'SF%'";
            adapter = new OdbcDataAdapter(strSQL, connection);
            adapter.Fill(ProdBOMHeader);

            //Production BOM Line enthält jede Stücklisten-Zeile. Also eine Verknüpfung von Stückliste und Artikel (1:1)
            strSQL = "select \"Production BOM No.\", \"No.\", \"Unit of Measure Code\", \"Quantity\"  from \"Production BOM Line\" where not \"No.\" = ''";
            adapter = new OdbcDataAdapter(strSQL, connection);
            adapter.Fill(ProdBOMLine);

            strSQL = "select * from \"Item Unit of Measure\"";
            adapter = new OdbcDataAdapter(strSQL, connection);
            adapter.Fill(ItemUnitOfMeasure);

            Item.PrimaryKey = new DataColumn[] { Item.Columns["No."] };
            ProdBOMHeader.PrimaryKey = new DataColumn[] { ProdBOMHeader.Columns["No."] };

            foreach (DataRow dr in SalesHeader.Rows)
            {
                int ParentID = (int)Parent_Child_Item.AddRow(dr["No."])["ID"];
                rekursiveItemData(ParentID, SalesLine.Select("[Document No.] = '" + dr["No."] + "'"), null);
            }
            Helper.LogFileWrite("Loading Item Data: done");

        }

        private  void getPurchaseData()
        {
            Helper.LogFileWrite("Loading Purchase Data...");
            strSQL = "select * from \"Purchase Line\" where \"Document Type\" = 1";
            adapter = new OdbcDataAdapter(strSQL, connection);
            adapter.Fill(PurchaseLine);
            strSQL = "select \"Item No.\", \"Vendor No.\", \"Kreditor Name\", \"Vendor Item No.\", \"Ending Date\" from \"Purchase Price\" Order by \"Ending Date\" DESC";
            adapter = new OdbcDataAdapter(strSQL, connection);
            adapter.Fill(PurchasePrice);
            Helper.LogFileWrite("Loading Purchase Data: done");
        }

        private  void getProductionData()
        {
            Helper.LogFileWrite("Loading Production Data...");
            //Fertigungsauftragszeilen
            strSQL = "select * from \"Prod. Order Line\" where Status >=1 AND Status <= 3";
            adapter = new OdbcDataAdapter(strSQL, connection);
            adapter.Fill(ProdOrderLine);

            //Anforderungszeilen
            strSQL = "select * from \"Requisition Line\" where Type = 2 AND \"Worksheet Template Name\" = 'BESTVORSCH' AND NOT \"Journal Batch Name\" = 'PLANUNG'";
            adapter = new OdbcDataAdapter(strSQL, connection);
            adapter.Fill(RequisitionLine);

            //Menge in Komponente
            strSQL = "select * from \"Prod. Order Component\" where Status >=1 AND Status <= 3";
            adapter = new OdbcDataAdapter(strSQL, connection);
            adapter.Fill(ProdOrderComponent);
            Helper.LogFileWrite("Loading Production Data: done");
        }

        /// <summary>
        /// This method goes through every row of the table containing child and finds out wether it relates to a record of the parent-table.
        /// One parent contains several children -> 1:m
        /// </summary>
        /// <param name="child"></param>
        /// <param name="parent"></param>
        private  void JoinFilter(DataColumn childcolumn, DataColumn parentcolumn)
        {

            DataTable child = childcolumn.Table;
            DataTable parent = parentcolumn.Table;

            DataTable newchild = child.Clone();

            foreach (DataRow dr in child.Rows)
            {
                if (parent.findRows(parentcolumn, dr[childcolumn]).Count > 0)
                {
                    newchild.ImportRow(dr);
                }
            }
            child.Clear();

            child.ImportRows(newchild);
        }

        /// <summary>
        /// This method goes through every row of the table containing child and finds out wether it relates to a record of the parent-table.
        /// One parent contains several children -> 1:m
        /// </summary>
        /// <param name="child"></param>
        /// <param name="parent"></param>
        /// <param name="joinedTable">If this is set, the result will be saved in that table, so that the original child table remains unchanged</param>
        private  void JoinFilter(DataColumn childcolumn, DataColumn parentcolumn, DataTable joinedTable)
        {
            DataTable child = childcolumn.Table;
            DataTable parent = parentcolumn.Table;

            foreach (DataRow dr in child.Rows)
            {
                if (parent.findRows(parentcolumn, dr[childcolumn]).Count > 0)
                {
                    joinedTable.ImportRow(dr);
                }
            }
        }
        #endregion

        #region Analyse Data

        private void setupGeneratedTables()
        {
            //Calculated_Item is a table that describes the required quantity of each item
            Calculated_Item.Columns.Add("No.");
            Calculated_Item.Columns.Add("Description");
            //Beschaffungsmethode
            Calculated_Item.Columns.Add("Replenishment System");
            //Einheit
            Calculated_Item.Columns.Add("Base Unit of Measure");
            Calculated_Item.Columns.Add(new DataColumn("Required Quantity") {DataType = typeof(double) });
            Calculated_Item.Columns.Add(new DataColumn("Inventory") { DataType = typeof(double) });
            //Menge in Verkaufsauftrag
            Calculated_Item.Columns.Add(new DataColumn("Sales Order") { DataType = typeof(double) });
            //Menge in Einkaufsbestellung
            Calculated_Item.Columns.Add(new DataColumn("Purchase Order") { DataType = typeof(double) });
            //Menge in Fertigung
            Calculated_Item.Columns.Add(new DataColumn("Prod. Order") { DataType = typeof(double) });
            //Menge in Komponente
            Calculated_Item.Columns.Add(new DataColumn("Prod. Component") { DataType = typeof(double) });
            //Menge in Materialanforderung
            Calculated_Item.Columns.Add(new DataColumn("Qty. on Requisition") { DataType = typeof(double) });
            //Empfohlene Fertigungsmenge
            Calculated_Item.Columns.Add(new DataColumn("Recommended Prod. Quantity") { DataType = typeof(double) });
            //Fertigungsstelle
            Calculated_Item.Columns.Add("Location");
            //Mitarbeiter
            Calculated_Item.Columns.Add("Mitarbeiter");
            //Fehlende Menge
            Calculated_Item.Columns.Add(new DataColumn("Missing Quantity") { DataType = typeof(double) });
            //Bei Einkaufsartikel: Material Anfordern; Material bestellen; auf Materialeingang warten
            //Bei Fertigungsartikel: Auf Material warten; x Stück fertigen; in Fertigung
            Calculated_Item.Columns.Add("Action");
            //Beschreibt, wie oft ein Fertigungsartikel hergestellt werden kann
            Calculated_Item.Columns.Add(new DataColumn("Quantity able to Produce") { DataType = typeof(double) });
            Calculated_Item.Columns.Add("Nächste Priorität Buchstaben");
            Calculated_Item.Columns.Add(new DataColumn("Nächste Priorität Zahl") { DataType = typeof(int) });
            Calculated_Item.Columns.Add(new DataColumn("Menge bis Priorität") { DataType = typeof(double) });
            Calculated_Item.Columns.Add("Vendor No.");
            Calculated_Item.Columns.Add("Vendor Name");
            Calculated_Item.Columns.Add(new DataColumn("Minuten pro Stück") { DataType = typeof(double) });
            //ohne Lagerbewertung
            Calculated_Item.Columns.Add(new DataColumn("Inventory Value Zero") { DataType = typeof(bool) });
            Calculated_Item.PrimaryKey = new DataColumn[] { Calculated_Item.Columns["No."] };


            Calculated_Sale.Columns.Add("No.");
            Calculated_Sale.Columns.Add("Item No.");
            Calculated_Sale.Columns.Add("Nächste Priorität Buchstaben");
            Calculated_Sale.Columns.Add(new DataColumn("Nächste Priorität Zahl") { DataType = typeof(int) });
            Calculated_Sale.Columns.Add("Parent Item No.");
            Calculated_Sale.Columns.Add("Description");
            Calculated_Sale.Columns.Add(new DataColumn("Required Quantity") { DataType = typeof(double) });
        }


        /// <summary>
        /// This is the main method of this program. It runs through each sales- and production bom line, calculates the required quantity of each item and gets many other information.
        /// </summary>
        /// <param name="ParentID">Since this is a rekursive method running through each Sales Header and therefore through each Sales Line, this ParentID represents the ID in Parent_Child_Item of the parent element.</param>
        /// <param name="Children"></param>
        /// <param name="salesline">This is the current sales line. This parameter is not relevant for the rekursive process and is passed on to each child element.</param>
        private void rekursiveItemData(int ParentID, DataRow[] Children, DataRow _salesline)
        {
            DataRow PCI_Parent = Parent_Child_Item.GetRow(ParentID);
            DataRow PCI_SalesHeader = Parent_Child_Item.GetRootParent(ParentID);
            DataRow PCI_Current;
            DataRow salesline;
            for (int i = 0; i < Children.Length; i++)
            {
                //Child is either a salesline or a prodbomline
                DataRow Child = Children[i];

                if (_salesline == null)
                    salesline = Child;
                else
                    salesline = _salesline;

                #region Item Data under current Sales Header
                double PositionRequiredQuantity = Parent_Child_Item.GetTotalRequiredQuantity((double)Child["Quantity"], ParentID);                
                double unit_of_measure = 1;
                getUnitFactor(Child["No."], Child["Unit of Measure Code"], out unit_of_measure);
                PositionRequiredQuantity = PositionRequiredQuantity * unit_of_measure;                
                #endregion

                #region Item Data in general
                //The following command works for Production BOM Line as well as for Sales Line
                string itemNo = Child["No."].ToString();
                double itemInventory = -1;
                double itemRequiredQuantity = 0;
                double itemPurchaseOrder = 0;
                double itemRequisition = 0;
                string itemReplenishmentSystem = "";
                string prodBomNumber = "";
                DataRow itemprodBomHeader = null;
                #endregion

                if (itemNo != "")
                {
                    if (itemNo == "AK4062")
                        Console.Write("");
                    if (itemNo == "AK4062" && (PCI_SalesHeader["No."].ToString() == "VB16333" || PCI_SalesHeader["No."].ToString() == "VB16433"))
                        Console.Write("");

                    DataRow calculated_sale;

                    #region calculated_item
                    DataRow calculated_item = Calculated_Item.NewRow();
                    calculated_item["No."] = itemNo;

                    #region string prodBomNumber = Production BOM Number of current Item
                    try
                    {
                        DataRow[] pbh = ProdBOMHeader.Select("[Item No.] = '" + itemNo + "'");
                        if (pbh.Length > 0)
                        {
                            prodBomNumber = pbh[0]["No."].ToString();
                            itemprodBomHeader = ProdBOMHeader.Rows.Find(prodBomNumber);
                        }
                    }
                    catch (Exception ex) { Helper.LogFileWrite(ex.ToString()); }
                    #endregion
                    try
                    {
                        Calculated_Item.Rows.Add(calculated_item);
                        DataRow item = Item.Rows.Find(calculated_item["No."]);
                        //Einkauf = 0, Fertigungsauftrag = 1
                        calculated_item["Replenishment System"] = item["Replenishment System"].ToString() == "0" ? "Einkauf" : "Fertigungsauftrag";
                        calculated_item["Required Quantity"] = -1 * double.Parse(item["Inventory"].ToString());
                        calculated_item["Inventory"] = item["Inventory"];
                        calculated_item["Purchase Order"] = getQuantityInPurchaseOrder(item);
                        calculated_item["Qty. on Requisition"] = getQuantityInRequisition(item);
                        calculated_item["Base Unit of Measure"] = item["Base Unit of Measure"];
                        calculated_item["Mitarbeiter"] = item["Mitarbeiter"];
                        calculated_item["Inventory Value Zero"] = (bool)item["Inventory Value Zero"];
                        calculated_item["Minuten pro Stück"] = (double)item["Minuten pro Stück"];
                        if (calculated_item["Replenishment System"].ToString() == "Einkauf")
                        {
                            try
                            {
                                DataRow purchaseprice = getLatestVendor(itemNo);
                                calculated_item["Vendor No."] = purchaseprice["Vendor No."];
                                calculated_item["Vendor Name"] = purchaseprice["Kreditor Name"];
                            }
                            catch (IndexOutOfRangeException) { }
                            catch (Exception ex) { Helper.LogFileWrite(ex.ToString()); }
                        }
                        else
                        {
                            calculated_item["Location"] = itemprodBomHeader["Fertigungsstelle"];
                            double recprodqty = (double)itemprodBomHeader["Recommended Prod. Quantity"];
                            if (recprodqty > 0)
                                calculated_item["Recommended Prod. Quantity"] = recprodqty;
                        }
                    }
                    catch (ConstraintException)
                    {
                        calculated_item = Calculated_Item.Rows.Find(itemNo);
                    }
                    catch (NullReferenceException)
                    {
                        calculated_item = Calculated_Item.Rows.Find(itemNo);
                    }

                    double.TryParse(calculated_item["Inventory"].ToString(), out itemInventory);
                    double.TryParse(calculated_item["Required Quantity"].ToString(), out itemRequiredQuantity);
                    double.TryParse(calculated_item["Purchase Order"].ToString(), out itemPurchaseOrder);
                    double.TryParse(calculated_item["Qty. on Requisition"].ToString(), out itemRequisition);

                    itemReplenishmentSystem = calculated_item["Replenishment System"].ToString();
                    #endregion

                    #region calculated_sale
                    LettersAndDigits lad = SplitStringIntoLettersAndDigits(salesline["Priorität"].ToString());
                    var letters = lad.Letters;
                    var digits = lad.Digits;
                    DataRow[] calculated_sales = Calculated_Sale.Select("[No.] = '" + PCI_SalesHeader["No."] + "' and [Item No.] = '" + itemNo + "'");
                    if (calculated_sales.Length > 0)
                    {
                        calculated_sale = calculated_sales[0];
                        calculated_sale["Required Quantity"] = (double)(calculated_sale["Required Quantity"]) + PositionRequiredQuantity;
                        if ((int)calculated_sale["Nächste Priorität Zahl"] > digits)
                        {
                            calculated_sale["Nächste Priorität Buchstaben"] = letters;
                            calculated_sale["Nächste Priorität Zahl"] = digits;
                        }
                    }
                    else
                    {
                        try
                        {
                            DataRow sales_header = SalesHeader.Rows.Find(PCI_SalesHeader["No."]);
                            calculated_sale = Calculated_Sale.NewRow();
                            calculated_sale["No."] = PCI_SalesHeader["No."];
                            calculated_sale["Parent Item No."] = salesline["No."];
                            var parentItem = Item.Rows.Find(salesline["No."]);
                            calculated_sale["Description"] = parentItem["Description"] + "" + parentItem["Description 2"];
                            calculated_sale["Item No."] = itemNo;
                            calculated_sale["Required Quantity"] = PositionRequiredQuantity.ToString();
                            calculated_sale["Nächste Priorität Buchstaben"] = letters;
                            calculated_sale["Nächste Priorität Zahl"] = digits;
                            Calculated_Sale.Rows.Add(calculated_sale);
                        }
                        catch (NullReferenceException) { }
                    }
                    #endregion

                    #region Required Quantity for current item
                    try
                    {
                        bool lessthanzerobefore = false;
                        if (itemRequiredQuantity <= 0)
                            lessthanzerobefore = true;

                        calculated_item["Required Quantity"] = (itemRequiredQuantity += PositionRequiredQuantity).ToString();

                        if (lessthanzerobefore && itemRequiredQuantity > 0)
                            calculated_item["Required Quantity"] = (itemRequiredQuantity += itemInventory).ToString();
                    }
                    catch (FormatException)
                    {
                        calculated_item["Required Quantity"] = (itemRequiredQuantity = PositionRequiredQuantity).ToString();
                    }
                    #endregion

                    #region Parent_Child_Item
                    if (PositionRequiredQuantity > 0)
                    {
                        try
                        {
                            PCI_Current = Parent_Child_Item.AddChildRow(ParentID, itemNo, Child["Unit of Measure Code"], PositionRequiredQuantity, Child["Quantity"], (double)calculated_item["Inventory"] >= PositionRequiredQuantity, letters, digits);
                            if (itemprodBomHeader != null && /*(itemRequiredQuantity > 0) && */(itemReplenishmentSystem == "Fertigungsauftrag") && itemprodBomHeader["Status"].ToString() == "1")
                            {
                                DataRow[] ProductionBOMLines = ProdBOMLine.Select("[Production BOM No.] = '" + prodBomNumber + "'");
                                rekursiveItemData((int)PCI_Current["ID"], ProductionBOMLines, salesline);
                            }
                        }
                        catch (Exception ex)
                        {
                            Helper.LogFileWrite(ex.ToString());
                        }
                    }
                    #endregion
                }
            }
        }

        /// <summary>
        /// Equivalent to Mege in Verkauf Auftrag
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        private  double getQuantityInSalesOrder(DataRow item)
        {
            double result = 0;
            DataRow[] SalesLines = SalesLine.Select("[No.] = '" + item[item.Table.Columns["No."]] + "'");
            foreach (DataRow dr in SalesLines)
            {
                double unit_of_measure = 1;
                getUnitFactor(dr["No."], dr["Unit of Measure Code"], out unit_of_measure);
                result += double.Parse(dr["Quantity"].ToString()) * unit_of_measure;
            }
            return result;
        }

        /// <summary>
        /// Equivalent to Mege in Einkauf Bestellung
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        private  double getQuantityInPurchaseOrder(DataRow item)
        {
            if (item["No."].ToString() == "AK2612")
                Console.Write("");
            double result = 0;
            DataRow[] PurchaseLines = PurchaseLine.Select("[No.] = '" + item[item.Table.Columns["No."]] + "'");
            foreach (DataRow dr in PurchaseLines)
            {
                double unit_of_measure = 1;
                getUnitFactor(dr["No."], dr["Unit of Measure Code"], out unit_of_measure);
                result += double.Parse(dr["Outstanding Quantity"].ToString()) * unit_of_measure;
            }
            return result;
        }

        /// <summary>
        /// Equivalent to Menge in Fertigungsauftrag
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        private  double getQuantityProdOrder(DataRow item)
        {
            double result = 0;
            DataRow[] ProdOrderLines = ProdOrderLine.Select("[Item No.] = '" + item[item.Table.Columns["No."]] + "'");
            foreach (DataRow dr in ProdOrderLines)
            {
                double unit_of_measure = 1;
                getUnitFactor(dr["Item No."], dr["Unit of Measure Code"], out unit_of_measure);
                result += double.Parse(dr["Quantity"].ToString()) * unit_of_measure;
            }
            return result;
        }

        /// <summary>
        /// Equivalent to Menge in Materialanforderung
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        private  double getQuantityInRequisition(DataRow item)
        {
            double result = 0;
            DataRow[] RequisitionLines = RequisitionLine.Select("[No.] = '" + item[item.Table.Columns["No."]] + "'");
            foreach (DataRow dr in RequisitionLines)
            {
                double unit_of_measure = 1;
                getUnitFactor(dr["No."], dr["Unit of Measure Code"], out unit_of_measure);
                result += double.Parse(dr["Quantity"].ToString()) * unit_of_measure;
            }
            return result;
        }

        /// <summary>
        /// Equivalent to Menge in Komponente
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        private  double getQuantityInComponent(DataRow item)
        {
            double result = 0;
            DataRow[] prodordercomponent = ProdOrderComponent.Select("[Item No.] = '" + item[item.Table.Columns["No."]] + "'");
            foreach (DataRow dr in prodordercomponent)
            {
                double unit_of_measure = 1;
                getUnitFactor(dr["Item No."], dr["Unit of Measure Code"], out unit_of_measure);
                result += double.Parse(dr["Quantity"].ToString()) * unit_of_measure;
            }
            return result;
        }

        private DataRow getLatestVendor(object ItemNo) 
        {
            DataRow[] purchaseprices = PurchasePrice.Select("[Item No.] = '" + ItemNo + "'");
            if (purchaseprices.Length == 0)
                throw new IndexOutOfRangeException(String.Format("Item {0} was not found in Purchase Price table", ItemNo));
            return purchaseprices[0];
        }

        private void sortCalculatedItem()
        {
            DataView dv = Calculated_Item.DefaultView;
            dv.Sort = "No. ASC";

            DataTable newCalculated_Item = dv.ToTable();
            Calculated_Item.Rows.Clear();
            foreach (DataRow dr in newCalculated_Item.Rows)
                Calculated_Item.ImportRow(dr);
        }
        
        private void FillCalculatedItem() 
        {
            for (int i = 0; i < Calculated_Item.Rows.Count; i++)
            {
                DataRow calculated_item = Calculated_Item.Rows[i];
                DataRow item = Item.Rows.Find(calculated_item["No."]);
                if (calculated_item["No."].ToString() == "AK0835")
                    Console.Write("");

                try
                {
                    if ((double)calculated_item["Required Quantity"] <= 0)
                    {
                        Calculated_Item.Rows.Remove(calculated_item);
                        i--;
                    }
                    else
                    {
                        double inventory = (double)calculated_item["Inventory"];
                        double required_quantity = (double)calculated_item["Required Quantity"];
                        double sales_order = getQuantityInSalesOrder(item);
                        double purchase_order = (double)calculated_item["Purchase Order"];
                        double prod_component = getQuantityInComponent(item);
                        double prod_order = getQuantityProdOrder(item);
                        double qty_on_requisition = (double)calculated_item["Qty. on Requisition"];
                        double missing_quantity = required_quantity - inventory - purchase_order - prod_component - prod_order - qty_on_requisition;
                        double quantity_able_to_produce;
                        string replenishment_system = calculated_item["Replenishment System"].ToString();

                        calculated_item["Description"] = item["Description"];                        
                        calculated_item["Sales Order"] = sales_order;
                        calculated_item["Prod. Component"] = prod_component;
                        calculated_item["Prod. Order"] = prod_order;
                        calculated_item["Missing Quantity"] = missing_quantity;
                        if (double.TryParse(calculated_item["Quantity able to Produce"].ToString(), out quantity_able_to_produce)) 
                        {
                            if (missing_quantity < quantity_able_to_produce)
                                calculated_item["Quantity able to Produce"] = missing_quantity;
                        }

                        #region Aktion
                        if (replenishment_system == "Einkauf")
                        {
                            if (purchase_order >= required_quantity - inventory)
                                calculated_item["Action"] = "auf Materialeingang warten";
                            else
                            {
                                calculated_item["Action"] = "Material bestellen";
                                
                                if (qty_on_requisition < required_quantity - inventory)
                                    calculated_item["Action"] = "Material anfordern";
                            }
                        }
                        if (replenishment_system == "Fertigungsauftrag")
                        {
                            if (prod_order >= required_quantity - inventory)
                                calculated_item["Action"] = "in Fertigung";
                            else if (GetQuantityofProducable(calculated_item) < 1)
                                calculated_item["Action"] = "Auf Material warten";

                        }
                        #endregion

                        #region Priority
                        DataView dv_calculated_sales = new DataView(Calculated_Sale, "[Item No.] = '" + calculated_item["No."] + "'", "[Nächste Priorität Zahl] ASC", DataViewRowState.CurrentRows);
                        DataTable sorted_calculated_sale = dv_calculated_sales.ToTable();
                        double count = 0;
                        foreach (DataRow row in sorted_calculated_sale.Rows)
                        {
                            count += (double)row["Required Quantity"];
                            if (count > inventory)
                            {
                                calculated_item["Nächste Priorität Buchstaben"] = row["Nächste Priorität Buchstaben"];
                                calculated_item["Nächste Priorität Zahl"] = row["Nächste Priorität Zahl"];
                                calculated_item["Menge bis Priorität"] = count - inventory;
                                break;
                            }
                        }
                        #endregion
                    }
                }
                catch (Exception ex) { Helper.LogFileWrite("Exception in FillCalculatedItem():\r\n {0}", ex.ToString()); }
            }
        }

        private void SetParentChildItemAvailability() 
        {
            foreach (DataRow calculated_item in Calculated_Item.Rows) 
            {
                Parent_Child_Item.SetAvailability(calculated_item["No."], (double)calculated_item["Inventory"]);
            }

            foreach (DataRow salesheader in SalesHeader.Rows)
            {
                Parent_Child_Item.SetAvailability(salesheader["No."], null);
            }
        }

        /// <summary>
        /// Wie oft kann ein Artikel hergestellt werden?; sets Action automatically
        /// </summary>
        /// <param name="calculated_item"></param>
        /// <returns></returns>
        private double GetQuantityofProducable(DataRow calculated_item)
        {
            double required_quantity = double.Parse(calculated_item["Required Quantity"].ToString());
            double inventory = double.Parse(calculated_item["Inventory"].ToString());
            double missing_quantity;
            double producable = required_quantity;
            if (double.TryParse(calculated_item["Missing Quantity"].ToString(), out missing_quantity))
                producable = missing_quantity;

            string replenishment_system = calculated_item["Replenishment System"].ToString();
            DataRow[] pbh = ProdBOMHeader.Select("[Item No.] = '" + calculated_item["No."] + "'");

            if (replenishment_system == "Fertigungsauftrag" && pbh.Length > 0)
            {
                string prodBomNumber = pbh[0]["No."].ToString();
                DataRow[] prodbomlines = ProdBOMLine.Select("[Production BOM No.] = '" + prodBomNumber + "'");
                foreach (DataRow dr in prodbomlines)
                {
                    DataRow child = Calculated_Item.Rows.Find(dr["No."]);
                    if (child != null)
                    {
                        double child_inventory = double.Parse(child["Inventory"].ToString());
                        double child_required_quantity = double.Parse(dr["Quantity"].ToString()) * required_quantity;
                        string child_replenishment_sytem = child["Replenishment System"].ToString();

                        double unit_of_measure = 1;
                        getUnitFactor(dr["No."], dr["Unit of Measure Code"], out unit_of_measure);
                        child_required_quantity = child_required_quantity * unit_of_measure;

                        //Calculation for new producable
                        var temp = child_inventory * required_quantity / child_required_quantity;
                        if (temp < producable)
                            producable = temp;
                    }
                }
                calculated_item["Action"] = "Fertigen";
                if (calculated_item["Base Unit of Measure"].ToString() == "STK")
                    producable = Math.Floor(producable);
                //leave empty if zero
                if (producable > 0)
                    calculated_item["Quantity able to Produce"] = producable;
                return producable;
            }
            else
                return inventory;
        }

        //Ermittelt den benötigten Faktor der Einheit eines Artikels aus der Tabelle Item Unit of Measure
        private void getUnitFactor(object ItemNo, object Unit, out double Factor)
        {
            try
            {
                Factor = double.Parse(ItemUnitOfMeasure.Select("[Item No.] = '" + ItemNo + "' and Code = '" + Unit + "'")[0]["Qty. per Unit of Measure"].ToString());
            }
            catch (ConstraintException) { Factor = 1.0; }
            catch (IndexOutOfRangeException) { Factor = 1.0; }
        }

        struct LettersAndDigits
        {
            public string Letters;
            public int Digits;
            public LettersAndDigits(string letters, int digits) 
            {
                Letters = letters;
                Digits = digits;
            }
        }

        private LettersAndDigits SplitStringIntoLettersAndDigits(string str) 
        {
            if (!String.IsNullOrEmpty(str))
            {
                string Letters = "";
                string Digits = "";
                foreach (char c in str)
                {
                    if (Char.IsNumber(c))
                        Digits += c;
                    else
                        Letters += c;
                }
                if (String.IsNullOrEmpty(Digits))
                    return new LettersAndDigits(Letters, 0);
                return new LettersAndDigits(Letters, int.Parse(Digits));
            }
            else
                return new LettersAndDigits(String.Empty, 0);
        }

        #endregion
    }
}
