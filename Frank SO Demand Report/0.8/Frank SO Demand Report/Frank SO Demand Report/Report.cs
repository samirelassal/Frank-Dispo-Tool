﻿using System;
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
        
        DataTable Calculated_Item = new DataTable();
        DataTable Calculated_Sale = new DataTable();

        public DataTable CalculatedItem
        {
            get { return Calculated_Item; }
        }
        public DataTable CalculatedSale
        {
            get { return Calculated_Sale; }
        }

        string strSQL;

        Document document;
        Table table;
        #endregion

        public void Start()
        {
            Console.WriteLine("Welcome to the SO Demand Report Generator. Just relax and wait until this program is done.\n");
            getSalesData();
            setupCalculatedItem();
            getItemData(SalesLine.Columns["No."]);
            getPurchaseData();
            getProductionData();
            FillCalculatedItem();
            sortCalculatedItemByStatus();
            Console.Write("Generating Report: ");
            generateReport();
            Console.WriteLine("Done! You may now close this window and enjoy the report.");
            Console.ReadLine();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="connection_string"></param>
        /// <param name="generate_report"></param>
        /// <param name="export_data_location">Describes the folder, in which the datatables are saved as xml files. If emtpy, no files will be created</param>
        public void Start(string connection_string, bool generate_report, string export_data_location)
        {
            connection = new OdbcConnection(connection_string);
            Console.WriteLine("Welcome to the SO Demand Report Generator. Just relax and wait until this program is done.\n");
            getSalesData();
            setupCalculatedItem();
            getItemData(SalesLine.Columns["No."]);
            getPurchaseData();
            getProductionData();
            FillCalculatedItem();
            sortCalculatedItemByStatus();
            if (generate_report)
            {
                Console.Write("Generating Report: ");
                generateReport();
                Console.WriteLine("Done! You may now close this window and enjoy the report.");
                Console.ReadLine();
            }

            if (export_data_location.Trim() != "")
            {
                Calculated_Item.TableName = "Calculated_Item";
                Calculated_Sale.TableName = "Calculated_Sale";
                Calculated_Item.WriteXml(export_data_location + @"\calculated_item.xml", XmlWriteMode.WriteSchema);
                Calculated_Sale.WriteXml(export_data_location + @"\calculated_sale.xml", XmlWriteMode.WriteSchema);
            }
        }

        #region Load Data
        private  void getSalesData()
        {
            Console.Write("Loading Sales Data: ");
            //Sales Header enthält alle aktuellen Bestellungen; Status = 1 => Freigegeben
            strSQL = "select * from \"Sales Header\" where \"Sales Header\".\"Document Type\" = 1 and Status = 1";
            adapter = new OdbcDataAdapter(strSQL, connection);
            adapter.Fill(SalesHeader);

            SalesHeader.PrimaryKey = new DataColumn[] { SalesHeader.Columns["No."] };

            //Sales Line enthält jede Zeile jeder Bestellung
            //Zuerst wird die gesamte Tabelle geladen
            strSQL = "select * from \"Sales Line\"";
            adapter = new OdbcDataAdapter(strSQL, connection);
            adapter.Fill(SalesLine);
            //Nun werden die Einträge gefiltert, es interessieren nur diejenigen, die mit einer aktuellen Bestellung verknüpft sind
            JoinFilter(SalesLine.Columns["Document No."], SalesHeader.Columns["No."]);
            Console.WriteLine("Done");
        }

        private  void getItemData(DataColumn parentcolumn)
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
            strSQL = "select * from \"Production BOM Line\" where not \"No.\" = ''";
            adapter = new OdbcDataAdapter(strSQL, connection);
            adapter.Fill(ProdBOMLine);

            strSQL = "select * from \"Item Unit of Measure\"";
            adapter = new OdbcDataAdapter(strSQL, connection);
            adapter.Fill(ItemUnitOfMeasure);

            Item.PrimaryKey = new DataColumn[] { Item.Columns["No."] };
            ProdBOMHeader.PrimaryKey = new DataColumn[] { ProdBOMHeader.Columns["No."] };

            foreach (DataRow dr in SalesLine.Rows)
            {
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
        private  void rekursiveItemData(DataRow parentRow, double RequiredQuantity, DataRow sales_line)
        {
            //The following command works for Production BOM Line as well as for Sales Line
            string itemNo = parentRow["No."].ToString();
            if (itemNo == "")
                return;

            DataRow calculated_item = Calculated_Item.NewRow();
            calculated_item["No."] = itemNo;
            try
            {
                Calculated_Item.Rows.Add(calculated_item);
                DataRow item = Item.Rows.Find(calculated_item["No."]);
                calculated_item["No."] = item["No."];
                calculated_item["Inventory"] = item["Inventory"];
                calculated_item["Required Quantity"] = "-" + item["Inventory"];
            }
            catch (ConstraintException) { calculated_item = Calculated_Item.Rows.Find(itemNo); }
            catch (NullReferenceException) { calculated_item = Calculated_Item.Rows.Find(itemNo); }

            DataRow calculated_sale;

            try
            {
                DataRow sales_header = SalesHeader.Rows.Find(sales_line["Document No."]);
                calculated_sale = Calculated_Sale.NewRow();
                calculated_sale["No."] = sales_line["Document No."];
                calculated_sale["Item No."] = itemNo;
                calculated_sale["Liefertermin nach KW"] = sales_line["Liefertermin nach KW"];
                calculated_sale["Customer"] = sales_header["Sell-to Customer No."] + " " + sales_header["Sell-to Customer Name"] + sales_header["Sell-to Customer Name 2"];
                calculated_sale["Line No."] = sales_line["Line No."];
                calculated_sale["Parent Item No."] = sales_line["No."];
                calculated_sale["Required Quantity"] = RequiredQuantity.ToString();
                calculated_sale["Description"] = Item.Rows.Find(sales_line["No."])["Description"].ToString();
                Calculated_Sale.Rows.Add(calculated_sale);
            }
            catch (NullReferenceException) { }
            #region Required Quantity for current item
            try
            {
                bool smallerbefore = false;
                if (double.Parse(calculated_item["Required Quantity"].ToString()) <= 0)
                    smallerbefore = true;
                calculated_item["Required Quantity"] = (double.Parse(calculated_item["Required Quantity"].ToString()) + RequiredQuantity).ToString();
                if (double.Parse(calculated_item["Required Quantity"].ToString()) <= 0)
                    return;
                //if Required Quantity just became bigger than 0, add Inventory
                else if (smallerbefore)
                    calculated_item["Required Quantity"] = (double.Parse(calculated_item["Required Quantity"].ToString()) + double.Parse(calculated_item["Inventory"].ToString())).ToString();

            }
            catch (FormatException)
            {
                calculated_item["Required Quantity"] = RequiredQuantity.ToString();
                if (double.Parse(calculated_item["Required Quantity"].ToString()) <= 0)
                    return;
            }
            #endregion

            #region string prodBomNumber = Production BOM Number of current Item
            DataRow[] pbh = ProdBOMHeader.Select("[Item No.] = '" + itemNo + "'");
            if (pbh.Length == 0)
                return;
            string prodBomNumber = pbh[0]["No."].ToString();

            if (ProdBOMHeader.Rows.Find(prodBomNumber)["Status"].ToString() != "1" || Item.Rows.Find(itemNo)["Replenishment System"].ToString() != "1")
                return;
            #endregion



            DataRow[] ProductionBOMLines = ProdBOMLine.Select("[Production BOM No.] = '" + prodBomNumber + "'");

            foreach (DataRow dr in ProductionBOMLines)
            {
                if (dr["No."].ToString() == "AK2190")
                    Debug.WriteLine("");

                double unit_of_measure = 1;

                try
                {
                    unit_of_measure = getUnitFactor(dr["No."], dr["Unit of Measure Code"]);
                }
                catch (ConstraintException) { }
                catch (IndexOutOfRangeException) { }
                double prod_bom_line_qty = double.Parse(dr["Quantity"].ToString()) * unit_of_measure;
                rekursiveItemData(dr, RequiredQuantity * prod_bom_line_qty, sales_line);
            }

        }

        private  void getPurchaseData()
        {
            Console.Write("Loading Purchase Data: ");
            strSQL = "select * from \"Purchase Line\" where \"Document Type\" = 1";
            adapter = new OdbcDataAdapter(strSQL, connection);
            adapter.Fill(PurchaseLine);
            Console.WriteLine("Done");
        }

        private  void getProductionData()
        {
            Console.Write("Loading Production Data: ");
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
            Console.WriteLine("Done");
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

        private  void setupCalculatedItem()
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
            //Fertigungsstelle
            Calculated_Item.Columns.Add("Location");
            //Fehlende Menge
            Calculated_Item.Columns.Add("Missing Quantity");
            Calculated_Item.PrimaryKey = new DataColumn[] { Calculated_Item.Columns["No."] };

            Calculated_Sale.Columns.Add("No.");
            Calculated_Sale.Columns.Add("Item No.");
            Calculated_Sale.Columns.Add("Liefertermin nach KW");
            Calculated_Sale.Columns.Add("Customer");
            Calculated_Sale.Columns.Add("Line No.");
            Calculated_Sale.Columns.Add("Parent Item No.");
            Calculated_Sale.Columns.Add("Description");
            Calculated_Sale.Columns.Add("Required Quantity");
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
                result += double.Parse(dr["Outstanding Quantity"].ToString());
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
            double result = 0;
            DataRow[] PurchaseLines = PurchaseLine.Select("[No.] = '" + item[item.Table.Columns["No."]] + "'");
            foreach (DataRow dr in PurchaseLines)
            {
                result += double.Parse(dr["Outstanding Quantity"].ToString());
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
                result += double.Parse(dr["Quantity"].ToString());
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
                result += double.Parse(dr["Quantity"].ToString());
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
                result += double.Parse(dr["Quantity"].ToString());
            }
            return result;
        }

        private  void setStatus(DataRow calculated_item)
        {
            if (calculated_item["No."].ToString() == "AK1112")
                Debug.WriteLine("At AK1112");
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

            else if (replenishmentSystem == "Fertigungsauftrag")
            {
                //Entsprechende Stückliste ermitteln
                object prodbomheader;
                try
                {
                    prodbomheader = ProdBOMHeader.Select("[Item No.] = '" + calculated_item["No."] + "'")[0]["No."];
                }
                catch
                {
                    Debug.WriteLine("No Production BOM Header found for article " + calculated_item["No."]);
                    calculated_item["Status"] = "!!Error!!";
                    return;
                }
                //Stücklistenzeilen ermitteln
                DataRow[] ProdBOMLines = ProdBOMLine.Select("[Production BOM No.] = '" + prodbomheader + "'");
                //Summe der Unterartikel ermitteln
                foreach (DataRow pbline in ProdBOMLines)
                {
                    
                    try
                    {
                        DataRow subitem = Item.Rows.Find(pbline["No."]);
                        /*try
                        {
                            DataRow sub_calculated_item = Calculated_Item.Rows.Find(pbline["No."]);
                            if (double.Parse(sub_calculated_item["Required Quantity"].ToString()) > double.Parse(sub_calculated_item["Inventory"].ToString()))
                            {
                                calculated_item["Status"] = "Not Able to Produce";
                                return;
                            }
                        }
                        catch { }*/

                        double inventory_of_subitem = getUnitFactor(subitem["No."], subitem["Base Unit of Measure"]) * double.Parse(subitem["Inventory"].ToString());
                        double required_qty_of_subitem = getUnitFactor(pbline["No."], pbline["Unit of Measure Code"]) * RequiredQuantity * double.Parse(pbline["Quantity"].ToString());
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

        private  void sortCalculatedItemByStatus()
        {
            DataView dv = Calculated_Item.DefaultView;
            dv.Sort = "No. ASC";

            DataTable newCalculated_Item = dv.ToTable();
            Calculated_Item.Rows.Clear();
            string[] Status_array = new string[] { "Not in Stock", "Not Able to Produce", "Able to Produce", "No Action Required" };
            foreach (string status in Status_array)
            {
                for (int i = 0; i < newCalculated_Item.Rows.Count; i++)
                {
                    DataRow dr = newCalculated_Item.Rows[i];
                    if (dr["Status"].ToString() == status)
                    {
                        Calculated_Item.ImportRow(dr);
                    }
                }
            }
        }

        private void FillCalculatedItem() 
        {
            for (int i = 0; i < Calculated_Item.Rows.Count; i++)
            {
                DataRow dr = Calculated_Item.Rows[i];
                DataRow item = Item.Rows.Find(dr["No."]);

                try
                {
                    if (double.Parse(dr["Required Quantity"].ToString()) <= 0)
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
                        double inventory = double.Parse(dr["Inventory"].ToString());
                        double required_quantity = double.Parse(dr["Required Quantity"].ToString());
                        double sales_order = getQuantityInSalesOrder(item);
                        double purchase_order = getQuantityInPurchaseOrder(item);
                        double prod_component = getQuantityInComponent(item);
                        double prod_order = getQuantityProdOrder(item);
                        double qty_on_requisition = getQuantityInRequisition(item);
                        dr["Sales Order"] = sales_order;
                        dr["Purchase Order"] = purchase_order;
                        dr["Prod. Component"] = prod_component;
                        dr["Prod. Order"] = prod_order;
                        dr["Qty. on Requisition"] = qty_on_requisition;
                        dr["Missing Quantity"] = required_quantity - inventory - sales_order - purchase_order - prod_component - prod_order - qty_on_requisition;
                        dr["Location"] = item["Location"];
                        setStatus(dr);
                    }
                }
                catch { }
            }
        }

        //Ermittelt den benötigten Faktor der Einheit eines Artikels aus der Tabelle Item Unit of Measure
        private  double getUnitFactor(object ItemNo, object Unit)
        {
            return double.Parse(ItemUnitOfMeasure.Select("[Item No.] = '" + ItemNo + "' and Code = '" + Unit + "'")[0]["Qty. per Unit of Measure"].ToString());
        }



        #endregion

        #region Report

         Color NIS = new Color(254, 173, 168);
         Color NAR = new Color(222, 189, 255);
         Color NAP = new Color(221, 226, 117);
         Color ATP = new Color(99, 189, 255);
         Color Contrast = new Color(230, 230, 230);

        private  void generateReport()
        {
            document = new Document();
            document.DefaultPageSetup.Orientation = Orientation.Landscape;
            document.Info.Title = "Frank GmbH SO Demand Report";
            document.Info.Author = "Samir El-Assal jr.";
            DefineStyles();
            CreatePage();
            FillContent();
            //FillTestContent();
            PdfDocumentRenderer renderer = new PdfDocumentRenderer(true, PdfFontEmbedding.Always);
            renderer.Document = document;
            renderer.RenderDocument();
            string path = Environment.GetFolderPath(Environment.SpecialFolder.Desktop) + @"\Frank Report.pdf";
            renderer.PdfDocument.Save(path);
            Process.Start(path);
        }

        private  void DefineStyles()
        {
            // Get the predefined style Normal.
            Style style = document.Styles["Normal"];
            // Because all styles are derived from Normal, the next line changes the 
            // font of the whole document. Or, more exactly, it changes the font of
            // all styles and paragraphs that do not redefine the font.
            style.Font.Name = "Arial";

            style = document.Styles[StyleNames.Header];
            style.ParagraphFormat.AddTabStop("16cm", TabAlignment.Right);

            style = document.Styles[StyleNames.Footer];
            style.ParagraphFormat.AddTabStop("8cm", TabAlignment.Center);

            // Create a new style called Table based on style Normal
            style = document.Styles.AddStyle("Table", "Normal");
            style.Font.Name = "Arial";
            style.Font.Size = 7;

            // Create a new style called Reference based on style Normal
            style = document.Styles.AddStyle("Reference", "Normal");
            style.ParagraphFormat.SpaceBefore = "5mm";
            style.ParagraphFormat.SpaceAfter = "5mm";
            style.ParagraphFormat.TabStops.AddTabStop("16cm", TabAlignment.Right);

            document.DefaultPageSetup.TopMargin = MigraDoc.DocumentObjectModel.Unit.FromCentimeter(4.5);
            document.DefaultPageSetup.LeftMargin = MigraDoc.DocumentObjectModel.Unit.FromCentimeter(1.6);
        }
        private  void CreatePage()
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
            paragraph = section.Headers.Primary.AddParagraph();
            paragraph.Style = "Reference";
            paragraph.AddDateField("dd.MM.yyyy");

            // Create the item table
            table = section.AddTable();
            table.Style = "Table";

            // Before you can add a row, you must define the columns
            //Status
            Column column = table.AddColumn("2.5cm");
            column.Format.Alignment = ParagraphAlignment.Center;
            //Artikel
            column = table.AddColumn("4.2cm");
            column.Format.Alignment = ParagraphAlignment.Center;
            //Beschaffung
            column = table.AddColumn("2cm");
            column.Format.Alignment = ParagraphAlignment.Center;
            //Einheit
            column = table.AddColumn("1.2cm");
            column.Format.Alignment = ParagraphAlignment.Center;
            //Menge Benötigt
            column = table.AddColumn("1.4cm");
            column.Format.Alignment = ParagraphAlignment.Center;
            //Lagerbestand
            column = table.AddColumn("1.9cm");
            column.Format.Alignment = ParagraphAlignment.Center;
            //Im Auftrag
            column = table.AddColumn("1.1cm");
            column.Format.Alignment = ParagraphAlignment.Center;
            //Im Einkauf
            column = table.AddColumn("1.2cm");
            column.Format.Alignment = ParagraphAlignment.Center;
            //In Fertigung
            column = table.AddColumn("1.5cm");
            column.Format.Alignment = ParagraphAlignment.Center;
            //In Fertigung in Komponenten
            column = table.AddColumn("2.2cm");
            column.Format.Alignment = ParagraphAlignment.Center;
            //In Materialanforderungen
            column = table.AddColumn("3.2cm");
            column.Format.Alignment = ParagraphAlignment.Center;
            //Fertigungsstelle
            column = table.AddColumn("2.2cm");
            column.Format.Alignment = ParagraphAlignment.Center;
            //Fehlende Menge
            column = table.AddColumn("2cm");
            column.Format.Alignment = ParagraphAlignment.Center;

            // Create the header of the table
            //table.TopPadding = MigraDoc.DocumentObjectModel.Unit.FromCentimeter(2.5);
            Row row = table.AddRow();
            row.HeadingFormat = true;
            row.Format.Alignment = ParagraphAlignment.Center;
            row.Format.Font.Bold = true;
            row.Shading.Color = Colors.Gray;
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
            row.Cells[11].AddParagraph("Fertigungsstelle");
            row.Cells[12].AddParagraph("Fehlende Menge");
            row.VerticalAlignment = VerticalAlignment.Center;
        }

        private  void FillTestContent()
        {
            Row row = table.AddRow();
            row.Shading.Color = NIS;
            row.Cells[0].AddParagraph("Status");
            row.Cells[1].AddParagraph("No Description");
            row.Cells[2].AddParagraph("Replenishment System");
            row.Cells[3].AddParagraph("Base Unit of Measure");
            row.Cells[4].AddParagraph("Required Quantity");
            row.Cells[5].AddParagraph("Inventory");
            row.Cells[6].AddParagraph("Sales Order");
            row.Cells[7].AddParagraph("Purchase Order");
            row.Cells[8].AddParagraph("Prod. Order");
            row.Cells[9].AddParagraph("Prod. Order");
            row.Cells[10].AddParagraph("Qty. on Requisition");

            Row row2 = table.AddRow();
            row2.Format.Alignment = ParagraphAlignment.Left;
            row2.Cells[0].MergeRight = 2;
            row2.Cells[0].AddParagraph("No. Vom: Datum");
            row2.Cells[3].MergeRight = 4;
            row2.Cells[3].AddParagraph("Customer");
            row2.Cells[8].MergeRight = 2;
            row2.Cells[8].AddParagraph("Zeile: Line No. Parent Item No. Menge Benötigt: Required Quantity");

        }
        private  void FillContent()
        {
            int i;
            for (i = 0; i < Calculated_Item.Rows.Count; i++)
            {
                DataRow dr = Calculated_Item.Rows[i];
                Row row = table.AddRow();

                Color current_color = Colors.White;

                switch (dr["Status"].ToString())
                {
                    case "Not in Stock": row.Shading.Color = NIS; break;
                    case "No Action Required": row.Shading.Color = NAR; break;
                    case "Not Able to Produce": row.Shading.Color = NAP; break;
                    case "Able to Produce": row.Shading.Color = ATP; break;
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
                row.Cells[11].AddParagraph(dr["Location"].ToString());
                row.Cells[12].AddParagraph(dr["Missing Quantity"].ToString());

                DataRow[] calculated_sale = Calculated_Sale.Select("[Item No.] = '" + dr["No."] + "'");
                for (int x = 0; x < calculated_sale.Length; x++)
                {
                    DataRow cs = calculated_sale[x];
                    Row row2 = table.AddRow();
                    row2.Format.Alignment = ParagraphAlignment.Left;
                    row2.Cells[0].MergeRight = 1;
                    row2.Cells[0].AddParagraph(cs["No."] + " Bestätigter Liefertermin: " + cs["Liefertermin nach KW"].ToString());
                    row2.Cells[2].MergeRight = 5;
                    row2.Cells[2].AddParagraph(cs["Customer"].ToString());
                    row2.Cells[8].MergeRight = 3;
                    row2.Cells[8].AddParagraph(cs["Parent Item No."] + " " + cs["Description"] + " Menge Benötigt: " + cs["Required Quantity"]);
                    if (x % 2 == 1)
                    {
                        row2.Shading.Color = Contrast;
                    }
                }
            }
            Console.WriteLine("Number of items: {0}", i.ToString());
        }
        #endregion
    }
}
