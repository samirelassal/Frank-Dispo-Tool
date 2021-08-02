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

using System.Xml.XPath;

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

    class Program
    {
        #region Data
        static OdbcConnection connection = new OdbcConnection("DSN=Navision Frank-Backup");
        static OdbcDataAdapter adapter;
        static DataSet ds = new DataSet();

        //Die folgenden Tabellen enthalten ALLE Navision-Datensätze der entsprechenden Tabelle
        static DataTable Item = new DataTable("Item");
        static DataTable ProdBOMHeader = new DataTable("Production BOM Header");
        static DataTable ProdBOMLine = new DataTable("Production BOM Line");

        static DataTable Calculated_Item = new DataTable();
        static DataTable Calculated_Sales = new DataTable();

        static string strSQL;

        static Document document;
        static Table table;
        #endregion

        static void Main(string[] args)
        {
            Console.WriteLine("Welcome to the SO Demand Report Generator. Just relax and wait until this program is done.\n");
            getSalesData();
            setupCalculatedItem();
            getItemData(ds.Tables["Sales Line"].Columns["No."]);
            getPurchaseData();
            getProductionData();



            //Fill Calculated_Item
            for (int i = 0; i < Calculated_Item.Rows.Count; i++)
            {
                DataRow dr = Calculated_Item.Rows[i];
                DataRow item = Item.Rows.Find(dr["No."]);

                try
                {
                    if (double.Parse(dr["Required Quantity"].ToString()) < 0)
                    {
                        Calculated_Item.Rows.Remove(dr);
                        i--;
                    }
                    else
                    {
                        dr["Description"] = item["Description"];
                        //Einkauf = 0, Fertigungsauftrag = 1
                        dr["Replenishment System"] = item["Replenishment System"].ToString() == "0" ? "Einkauf" : "Fertigungsauftrag";
                        dr["Base Unit of Measure"] = item["Base Unit of Measure"];
                        dr["Sales Order"] = getQuantityInSalesOrder(item);
                        dr["Purchase Order"] = getQuantityInPurchaseOrder(item);
                        dr["Prod. Component"] = getQuantityInComponent(item);
                        dr["Prod. Order"] = getQuantityProdOrder(item);
                        dr["Qty. on Requisition"] = getQuantityInRequisition(item);

                        setStatus(dr);
                    }
                }
                catch { }
            }
            sortCalculatedItemByStatus();
            Console.Write("Generating Report: ");
            generateReport();
            Console.WriteLine("Done! You may now close this window and enjoy the report.");
            Console.ReadLine();

        }

        #region Load Data
        private static void getSalesData() 
        {
            Console.Write("Loading Sales Data: ");
            //Sales Header enthält alle aktuellen Bestellungen; Status = 1 => Freigegeben
            strSQL = "select * from \"Sales Header\" where \"Sales Header\".\"Document Type\" = 1 and Status = 1";
            adapter = new OdbcDataAdapter(strSQL, connection);
            adapter.Fill(ds, "Sales Header");

            ds.Tables["Sales Header"].PrimaryKey = new DataColumn[] { ds.Tables["Sales Header"].Columns["No."] };

            //Sales Line enthält jede Zeile jeder Bestellung
            //Zuerst wird die gesamte Tabelle geladen
            strSQL = "select * from \"Sales Line\"";
            adapter = new OdbcDataAdapter(strSQL, connection);
            adapter.Fill(ds, "Sales Line");
            //Nun werden die Einträge gefiltert, es interessieren nur diejenigen, die mit einer aktuellen Bestellung verknüpft sind
            JoinFilter(ds.Tables["Sales Line"].Columns["Document No."], ds.Tables["Sales Header"].Columns["No."]);
            Console.WriteLine("Done");
        }

        private static void getItemData(DataColumn parentcolumn) 
        {
            Console.Write("Loading Item Data: ");
            //Item enthält zunächst alle Artikel
            strSQL = "select * from Item";
            adapter = new OdbcDataAdapter(strSQL, connection);
            adapter.Fill(Item);
            
            //Production BOM Header enthält alle Stücklisten-Köpfe. Diese sind m:n mit Item verknüpft
            strSQL = "select * from \"Production BOM Header\" where \"No.\" LIKE 'SF%'";
            adapter = new OdbcDataAdapter(strSQL, connection);
            adapter.Fill(ProdBOMHeader);
            
            //Production BOM Line enthält jede Stücklisten-Zeile. Also eine Verknüpfung von Stückliste und Artikel (1:1)
            strSQL = "select * from \"Production BOM Line\"";
            adapter = new OdbcDataAdapter(strSQL, connection);
            adapter.Fill(ProdBOMLine);

            //Die im Folgenden definierten Tabellen in DataSet sind zunächst leer. Sie werden nur mit relevanten Datensätzen gefüllt
            if (ds.Tables["Item"] == null) 
            {
                Item.PrimaryKey = new DataColumn[] { Item.Columns["No."] };
                DataTable temp = Item.Clone();
                temp.TableName = "Item";
                ds.Tables.Add(temp);
            }

            if (ds.Tables["Production BOM Header"] == null)
            {
                ProdBOMHeader.PrimaryKey = new DataColumn[] { ProdBOMHeader.Columns["No."] };
                DataTable temp = ProdBOMHeader.Clone();
                temp.TableName = "Production BOM Header";
                ds.Tables.Add(temp);
            }

            if (ds.Tables["Production BOM Line"] == null)
            {
                DataTable temp = ProdBOMLine.Clone();
                temp.TableName = "Production BOM Line";
                ds.Tables.Add(temp);
            }

            foreach (DataRow dr in ds.Tables["Sales Line"].Rows)
            {
                #region Debug Control
                if (dr["Document No."].ToString() == "VB15971")
                    Debug.WriteLine("At VB15971");
                #endregion

                rekursiveItemData(dr, double.Parse(dr["Outstanding Quantity"].ToString()), dr);
            }
            Console.WriteLine("Done");
            
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="parentRow">Either a sales line or a production bom line that refers to the current item</param>
        /// <param name="RequiredQuantity">Required total quantity for current item (so far)</param>
        /// <param name="sales_line">current sales line, at the first execution of this method, parentRow and sales_line are equal</param>
        private static void rekursiveItemData(DataRow parentRow, double RequiredQuantity, DataRow sales_line)
        {
            //The following command works for Production BOM Line as well as for Sales Line
            string itemNo = parentRow["No."].ToString();
            if (itemNo == "")
                return;
            bool stop = false;

            #region Debug Control
            if (sales_line["Document No."].ToString() == "VB15971")
                Debug.WriteLine("At VB15971");
            if (itemNo == "AK6592")
            {
                Debug.WriteLine("At item AK6592");
            }

            if (stop)
                return;
            #endregion

            DataRow calculated_item = Calculated_Item.NewRow();
            calculated_item["No."] = itemNo;
            try
            {
                Calculated_Item.Rows.Add(calculated_item);
                DataRow item = Item.Rows.Find(calculated_item["No."]);
                calculated_item["No."] = item["No."];
                calculated_item["Inventory"] = item["Inventory"];
                calculated_item["Required Quantity"] = "-" + item["Inventory"];
                //RequiredQuantity -= double.Parse(item["Inventory"].ToString());
            }
            catch (ConstraintException) { }
            catch (NullReferenceException) { }
            calculated_item = Calculated_Item.Rows.Find(itemNo);

            try
            {
                calculated_item["Required Quantity"] = (double.Parse(calculated_item["Required Quantity"].ToString()) + RequiredQuantity).ToString();
                if (double.Parse(calculated_item["Required Quantity"].ToString()) <= 0)
                    return;
                else
                    calculated_item["Required Quantity"] = (double.Parse(calculated_item["Required Quantity"].ToString()) + double.Parse(calculated_item["Inventory"].ToString())).ToString();
                
            }
            catch (FormatException)
            {
                calculated_item["Required Quantity"] = RequiredQuantity.ToString();
                if (double.Parse(calculated_item["Required Quantity"].ToString()) <= 0)
                    return;
            }

            DataRow calculated_sale;

            DataRow sales_header = ds.Tables["Sales Header"].Rows.Find(sales_line["Document No."]);
            calculated_sale = Calculated_Sales.NewRow();
            calculated_sale["No."] = sales_line["Document No."];
            calculated_sale["Item No."] = itemNo;
            calculated_sale["Order Date"] = sales_header["Order Date"];
            calculated_sale["Customer"] = sales_header["Sell-to Customer No."] + " " + sales_header["Sell-to Customer Name"] + sales_header["Sell-to Customer Name 2"];
            calculated_sale["Line No."] = sales_line["Line No."];
            calculated_sale["Parent Item No."] = sales_line["No."];
            Calculated_Sales.Rows.Add(calculated_sale);

            #region Required Quanttity for current sales line
            double current_req_qty = 0;
            try
            {
                current_req_qty = double.Parse(calculated_sale["Required Quantity"].ToString());
            }
            catch (FormatException) { }
            double new_req_qty = current_req_qty + RequiredQuantity;
            calculated_sale["Required Quantity"] = new_req_qty.ToString();
            #endregion

            #region string prodBomNumber = Production BOM Number of current Item
            DataRow[] pbh = ProdBOMHeader.Select("[Item No.] = '" + itemNo + "'");
            if (pbh.Length == 0)
                return;
            string prodBomNumber = pbh[0]["No."].ToString();
            #endregion

            DataRow[] ProductionBOMLines = ProdBOMLine.Select("[Production BOM No.] = '" + prodBomNumber + "'");
            foreach (DataRow dr in ProductionBOMLines)
            {
                if (dr["No."].ToString() == "AK0356")
                    Debug.WriteLine("Before AK0356");
                rekursiveItemData(dr, RequiredQuantity * double.Parse(dr["Quantity"].ToString()), sales_line);
            }


        }

        private static void getPurchaseData() 
        {
            Console.Write("Loading Purchase Data: ");
            strSQL = "select * from \"Purchase Line\" where \"Document Type\" = 1";
            adapter = new OdbcDataAdapter(strSQL, connection);
            adapter.Fill(ds, "Purchase Line");
            Console.WriteLine("Done");
        }

        private static void getProductionData() 
        {
            Console.Write("Loading Production Data: ");
            //Fertigungsauftragszeilen
            strSQL = "select * from \"Prod. Order Line\" where Status >=1 AND Status <= 3";
            adapter = new OdbcDataAdapter(strSQL, connection);
            adapter.Fill(ds, "Prod. Order Line");

            //Anforderungszeilen
            strSQL = "select * from \"Requisition Line\" where Type = 2 AND \"Worksheet Template Name\" = 'BESTVORSCH' AND NOT \"Journal Batch Name\" = 'PLANUNG'";
            adapter = new OdbcDataAdapter(strSQL, connection);
            adapter.Fill(ds, "Requisition Line");

            //Menge in Komponente
            strSQL = "select * from \"Prod. Order Component\" where Status >=1 AND Status <= 3";
            adapter = new OdbcDataAdapter(strSQL, connection);
            adapter.Fill(ds, "Prod. Order Component");
            Console.WriteLine("Done");
        }

        /// <summary>
        /// This method goes through every row of the table containing child and finds out wether it relates to a record of the parent-table.
        /// One parent contains several children -> 1:m
        /// </summary>
        /// <param name="child"></param>
        /// <param name="parent"></param>
        private static void JoinFilter(DataColumn childcolumn, DataColumn parentcolumn) 
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
        private static void JoinFilter(DataColumn childcolumn, DataColumn parentcolumn, DataTable joinedTable)
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

        private static void setupCalculatedItem() 
        {
            //Calculated_Item is a table that describes the required quantity of each item
            Calculated_Item.Columns.Add("No.");
            Calculated_Item.Columns.Add("Description");
            Calculated_Item.Columns.Add("Status");
            //Beschaffungsmethode
            Calculated_Item.Columns.Add("Replenishment System");
            //Einheit
            Calculated_Item.Columns.Add("Base Unit of Measure");
            Calculated_Item.Columns.Add("Required Quantity");
            Calculated_Item.Columns.Add("Inventory");
            //Menge in Verkaufsauftrag
            Calculated_Item.Columns.Add("Sales Order");
            //Menge in Einkaufsbestellung
            Calculated_Item.Columns.Add("Purchase Order");
            //Menge in Fertigung
            Calculated_Item.Columns.Add("Prod. Order");
            //Menge in Komponente
            Calculated_Item.Columns.Add("Prod. Component");
            //Menge in Materialanforderung
            Calculated_Item.Columns.Add("Qty. on Requisition");
            Calculated_Item.PrimaryKey = new DataColumn[] { Calculated_Item.Columns["No."] };

            Calculated_Sales.Columns.Add("No.");
            Calculated_Sales.Columns.Add("Item No.");
            Calculated_Sales.Columns.Add("Order Date");
            Calculated_Sales.Columns.Add("Customer");
            Calculated_Sales.Columns.Add("Line No.");
            Calculated_Sales.Columns.Add("Parent Item No.");
            Calculated_Sales.Columns.Add("Required Quantity");
        }

        /// <summary>
        /// Gets the required quantity of the specified item
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        private static double getRequiredQuantity(DataRow item) 
        {
            double result = 0;
            DataRow[] ProdBOMLines = ds.Tables["Production BOM Line"].Select("[No.] = '" + item[item.Table.Columns["No."]] + "'");
            List<DataRow> SalesLines = ds.Tables["Sales Line"].findRows("No.", item[item.Table.Columns["No."]]);
            foreach (DataRow dr in ProdBOMLines) 
            {
                result += double.Parse(dr["Quantity"].ToString());
            }
            foreach (DataRow dr in SalesLines)
            {
                result += double.Parse(dr["Outstanding Quantity"].ToString());
            }
            return result;
        }

        /// <summary>
        /// Equivalent to Mege in Verkauf Auftrag
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        private static double getQuantityInSalesOrder(DataRow item) 
        {
            double result = 0;
            DataRow[] SalesLines = ds.Tables["Sales Line"].Select("[No.] = '" + item[item.Table.Columns["No."]] + "'");
            foreach (DataRow dr in SalesLines)
            {
                result += double.Parse(dr["Outstanding Quantity"].ToString());
            }
            return result;
        }

        /// <summary>
        /// Equivalent to Mege in Einkauf Bestellung
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        private static double getQuantityInPurchaseOrder(DataRow item) 
        {
            double result = 0;
            DataRow[] PurchaseLines = ds.Tables["Purchase Line"].Select("[No.] = '" + item[item.Table.Columns["No."]] + "'");
            foreach (DataRow dr in PurchaseLines)
            {
                result += double.Parse(dr["Quantity"].ToString());
            }
            return result;
        }

        /// <summary>
        /// Equivalent to Menge in Fertigungsauftrag
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        private static double getQuantityProdOrder(DataRow item) 
        {
            double result = 0;
            DataRow[] ProdOrderLines = ds.Tables["Prod. Order Line"].Select("[Item No.] = '" + item[item.Table.Columns["No."]] + "'");
            foreach (DataRow dr in ProdOrderLines)
            {
                result += double.Parse(dr["Quantity"].ToString());
            }
            return result;
        }

        /// <summary>
        /// Equivalent to Menge in Materialanforderung
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        private static double getQuantityInRequisition(DataRow item) 
        {
            double result = 0;
            DataRow[] RequisitionLines = ds.Tables["Requisition Line"].Select("[No.] = '" + item[item.Table.Columns["No."]] + "'");
            foreach (DataRow dr in RequisitionLines)
            {
                result += double.Parse(dr["Quantity"].ToString());
            }
            return result;
        }

        /// <summary>
        /// Equivalent to Menge in Komponente
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        private static double getQuantityInComponent(DataRow item)
        {
            double result = 0;
            DataRow[] ProdOrderComponent = ds.Tables["Prod. Order Component"].Select("[Item No.] = '" + item[item.Table.Columns["No."]] + "'");
            foreach (DataRow dr in ProdOrderComponent)
            {
                result += double.Parse(dr["Quantity"].ToString());
            }
            return result;
        }

        private static void setStatus(DataRow calculated_item) 
        {
            string replenishmentSystem = calculated_item["Replenishment System"].ToString();
            double RequiredQuantity = double.Parse(calculated_item["Required Quantity"].ToString());
            double Inventory = double.Parse(calculated_item["Inventory"].ToString());
            double PurchaseOrder = double.Parse(calculated_item["Purchase Order"].ToString());
            double Requisition = double.Parse(calculated_item["Qty. on Requisition"].ToString());

            if (replenishmentSystem == "Einkauf")
            {
                if (RequiredQuantity > Inventory + PurchaseOrder + Requisition)
                    calculated_item["Status"] = "Not in Stock";
                else
                    calculated_item["Status"] = "No Action Required";
            }

            //replenishmentSystem == "Fertigungsauftrag"
            else 
            {
                //Entsprechende Stückliste ermitteln
                object ProdBOMHeader;
                try
                {
                    ProdBOMHeader = ds.Tables["Production BOM Header"].Select("[Item No.] = '" + calculated_item["No."] + "'")[0]["No."];
                }
                catch 
                {
                    Debug.WriteLine("No Production BOM Header found for article " + calculated_item["No."]);
                    calculated_item["Status"] = "!!Error!!";
                    return;
                }
                //Stücklistenzeilen ermitteln
                DataRow[] ProdBOMLines = ds.Tables["Production BOM Line"].Select("[Production BOM No.] = '" + ProdBOMHeader + "'");
                //Summe der Unterartikel ermitteln
                foreach (DataRow pbline in ProdBOMLines)
                {
                    try
                    {
                        double inventory_of_subitem = double.Parse(ds.Tables["Item"].Rows.Find(pbline["No."])["Inventory"].ToString());
                        double required_qty_of_subitem = RequiredQuantity * double.Parse(pbline["Quantity"].ToString());
                        if (inventory_of_subitem < required_qty_of_subitem)
                        {
                            calculated_item["Status"] = "Not Able to Produce";
                            return;
                        }
                    }
                    catch (NullReferenceException) 
                    {
                        Debug.WriteLine("Production BOM Line is not linked to an item."); 
                    }
                }
                calculated_item["Status"] = "Able to Produce";
            }
        }

        private static void sortCalculatedItemByStatus() 
        {
            DataTable newCalculated_Item = Calculated_Item.Clone();
            string[] Status_array = new string[] {"Not in Stock", "Not Able to Produce", "Able to Produce", "No Action Required" };
            foreach (string status in Status_array) 
            {
                for (int i = 0; i < Calculated_Item.Rows.Count; i++)
                {
                    DataRow dr = Calculated_Item.Rows[i];
                    if (dr["Status"].ToString() == status)
                    {
                        newCalculated_Item.ImportRow(dr);
                        Calculated_Item.Rows.Remove(dr);
                        i--;
                    }
                }
            }
            Calculated_Item = newCalculated_Item;
        }

        #endregion

        #region Report

        private static void generateReport() 
        {
            document = new Document();
            document.DefaultPageSetup.Orientation = Orientation.Landscape;
            document.Info.Title = "Frank GmbH SO Demand Report";
            document.Info.Author = "Samir El-Assal jr.";
            DefineStyles();
            CreatePage();
            FillContent();
            PdfDocumentRenderer renderer = new PdfDocumentRenderer(true, PdfFontEmbedding.Always);
            renderer.Document = document;
            renderer.RenderDocument();
            string path = Environment.GetFolderPath(Environment.SpecialFolder.Desktop) + @"\hallowelt.pdf";
            renderer.PdfDocument.Save(path);
            Process.Start(path);
        }

        private static void DefineStyles() 
        {
            // Get the predefined style Normal.
            Style style = document.Styles["Normal"];
            // Because all styles are derived from Normal, the next line changes the 
            // font of the whole document. Or, more exactly, it changes the font of
            // all styles and paragraphs that do not redefine the font.
            style.Font.Name = "Verdana";

            style = document.Styles[StyleNames.Header];
            style.ParagraphFormat.AddTabStop("16cm", TabAlignment.Right);

            style = document.Styles[StyleNames.Footer];
            style.ParagraphFormat.AddTabStop("8cm", TabAlignment.Center);

            // Create a new style called Table based on style Normal
            style = document.Styles.AddStyle("Table", "Normal");
            style.Font.Name = "Verdana";
            style.Font.Name = "Times New Roman";
            style.Font.Size = 7;

            // Create a new style called Reference based on style Normal
            style = document.Styles.AddStyle("Reference", "Normal");
            style.ParagraphFormat.SpaceBefore = "5mm";
            style.ParagraphFormat.SpaceAfter = "5mm";
            style.ParagraphFormat.TabStops.AddTabStop("16cm", TabAlignment.Right);
        }
        private static void CreatePage() 
        {
            // Each MigraDoc document needs at least one section.
            Section section = document.AddSection();

            // Put a logo in the header
            Image image = section.Headers.Primary.AddImage("../../../Report/frank_logo.png");
            image.Height = "2.5cm";
            image.LockAspectRatio = true;
            image.RelativeVertical = RelativeVertical.Line;
            image.RelativeHorizontal = RelativeHorizontal.Margin;
            image.Top = ShapePosition.Top;
            image.Left = ShapePosition.Right;
            image.WrapFormat.Style = WrapStyle.Through;

            Paragraph paragraph = section.Footers.Primary.AddParagraph();


            // Add the print date field
            paragraph = section.AddParagraph();
            paragraph.Style = "Reference";
            paragraph.AddDateField("dd.MM.yyyy");

            // Create the item table
            table = section.AddTable();
            table.Style = "Table";

            // Before you can add a row, you must define the columns
            //Status
            Column column = table.AddColumn("1cm");
            column.Format.Alignment = ParagraphAlignment.Center;
            //Artikel
            column = table.AddColumn("4cm");
            column.Format.Alignment = ParagraphAlignment.Center;
            //Beschaffung
            column = table.AddColumn("2cm");
            column.Format.Alignment = ParagraphAlignment.Center;
            //Einheit
            column = table.AddColumn("2cm");
            column.Format.Alignment = ParagraphAlignment.Center;
            //Menge Benötigt
            column = table.AddColumn("1.5cm");
            column.Format.Alignment = ParagraphAlignment.Center;
            //Lagerbestand
            column = table.AddColumn("2cm");
            column.Format.Alignment = ParagraphAlignment.Center;
            //Im Auftrag
            column = table.AddColumn("1.5cm");
            column.Format.Alignment = ParagraphAlignment.Center;
            //Im Einkauf
            column = table.AddColumn("1.5cm");
            column.Format.Alignment = ParagraphAlignment.Center;
            //In Fertigung
            column = table.AddColumn("1.5cm");
            column.Format.Alignment = ParagraphAlignment.Center;
            //In Fertigung in Komponenten
            column = table.AddColumn("3cm");
            column.Format.Alignment = ParagraphAlignment.Center;
            //In Materialanforderungen
            column = table.AddColumn("3.5cm");
            column.Format.Alignment = ParagraphAlignment.Center;

            // Create the header of the table
            Row row = table.AddRow();

            row.HeadingFormat = true;
            row.Format.Alignment = ParagraphAlignment.Center;
            row.Format.Font.Bold = true;
            row.Shading.Color = Colors.LightGray;
            row.Cells[0].AddParagraph("Status");
            row.Cells[1].AddParagraph("Artikel");
            row.Cells[2].AddParagraph("Beschaffung");
            row.Cells[3].AddParagraph("Einheit");
            row.Cells[4].AddParagraph("Menge Benötigt");
            row.Cells[5].AddParagraph("Lagerbestand");
            row.Cells[6].AddParagraph("Im Auftrag");
            row.Cells[7].AddParagraph("Im Einkauf");
            row.Cells[8].AddParagraph("In Fertigung");
            row.Cells[9].AddParagraph("In Fertigung in Komponenten");
            row.Cells[10].AddParagraph("In Materialanforderungen");
        }
        private static void FillContent() 
        {
            int i;
            for (i = 0; i < Calculated_Item.Rows.Count; i++)
            {
                DataRow dr = Calculated_Item.Rows[i];
                Row row = table.AddRow();
                switch (dr["Status"].ToString()) 
                {
                    case "Not in Stock": row.Cells[0].Shading.Color = Colors.Red; break;
                    case "No Action Required": row.Cells[0].Shading.Color = Colors.Pink; break;
                    case "Not Able to Produce": row.Cells[0].Shading.Color = Colors.Yellow; break;
                    case "Able to Produce": row.Cells[0].Shading.Color = Colors.Blue; break;

                }
                row.Cells[0].AddParagraph(dr["Status"].ToString());
                row.Cells[1].AddParagraph(dr["No."].ToString() + " " + dr["Description"].ToString());
                row.Cells[2].AddParagraph(dr["Replenishment System"].ToString());
                row.Cells[3].AddParagraph(dr["Base Unit of Measure"].ToString());
                row.Cells[4].AddParagraph(dr["Required Quantity"].ToString());
                row.Cells[5].AddParagraph(dr["Inventory"].ToString());
                row.Cells[6].AddParagraph(dr["Sales Order"].ToString());
                row.Cells[7].AddParagraph(dr["Purchase Order"].ToString());
                row.Cells[8].AddParagraph(dr["Prod. Order"].ToString());
                row.Cells[9].AddParagraph(dr["Prod. Order"].ToString());
                row.Cells[10].AddParagraph(dr["Qty. on Requisition"].ToString());
                row.Borders.Color = Colors.Black;

                DataRow[] calculated_sale = Calculated_Sales.Select("[Item No.] = '" + dr["No."] + "'");
                foreach (DataRow cs in calculated_sale)
                {
                    Row row2 = table.AddRow();
                    row2.Cells[0].MergeRight = 2;
                    row2.Cells[0].AddParagraph("Document: " + cs["No."] + " Order Date: " + Convert.ToDateTime(cs["Order Date"]).ToString("dd.MM.yyyy"));
                    row2.Cells[3].MergeRight = 4;
                    row2.Cells[3].AddParagraph("Customer: " + cs["Customer"]);
                    row2.Cells[8].MergeRight = 2;
                    row2.Cells[8].AddParagraph("Line No.: " + cs["Line No."] + " Parent Item No.: " + cs["Parent Item No."] + " Required Qty.: " + cs["Required Quantity"]);
                }
            }
            Console.WriteLine("Number of items: {0}", i.ToString());
        }
        #endregion

    }
}
 