using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using System.Data.Odbc;
using System.Diagnostics;

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

        public static void ImportRows(this DataTable table, DataTable dt) 
        {
            foreach (DataRow dr in dt.Rows) 
            {
                table.ImportRow(dr);
            }
        }
    }

    class Program
    {
        static OdbcConnection connection = new OdbcConnection("DSN=Navision Frank-Backup");
        static OdbcDataAdapter adapter;
        static DataSet ds = new DataSet();
        static void Main(string[] args)
        {
            GetData();
        }

        private static void GetData() 
        {
            //Sales Header enthält alle aktuellen Bestellungen
            string strSQL = "select * from \"Sales Header\" where \"Sales Header\".\"Document Type\" = 1";
            adapter = new OdbcDataAdapter(strSQL, connection);
            adapter.Fill(ds, "Sales Header");
            Console.WriteLine("Reading 'Sales Header': Complete");

            //Sales Line enthält jede Zeile jeder Bestellung
            //Zuerst wird die gesamte Tabelle geladen
            strSQL = "select * from \"Sales Line\"";
            adapter = new OdbcDataAdapter(strSQL, connection);
            adapter.Fill(ds, "Sales Line");
            Console.WriteLine("Reading 'Sales Line': Complete");
            //Nun werden die Einträge gefiltert, es interessieren nur diejenigen, die mit einer aktuellen Bestellung verknüpft sind
            JoinFilter(ds.Tables["Sales Line"].Columns["Document No."], ds.Tables["Sales Header"].Columns["No."]);
            Console.WriteLine("Applying join-filter to 'Sales Line': Complete");

            //Item enthält zunächst alle Artikel
            Console.Write("Reading 'Item': ");
            strSQL = "select * from Item";
            adapter = new OdbcDataAdapter(strSQL, connection);
            adapter.Fill(ds, "Item");
            Console.WriteLine("Complete");
            //Damit die Tabelle Item für den späteren Gebrauch unverändert bleibt, wird die gefilterte Version in Item_Filtered abgespeichert
            DataTable Item_Filtered = ds.Tables["Item"].Clone();
            Item_Filtered.TableName = "Item_Filtered";
            ds.Tables.Add(Item_Filtered);
            Console.Write("Applying join-filter to 'Item': ");
            JoinFilter(ds.Tables["Item"].Columns["No."], ds.Tables["Sales Line"].Columns["No."], ds.Tables["Item_Filtered"]);
            Console.WriteLine("Complete");

            //Production BOM Header enthält alle Stücklisten-Köpfe. Diese sind 1:1 mit Item verknüpft
            Console.Write("Reading 'Production BOM Header': ");
            strSQL = "select * from \"Production BOM Header\"";
            adapter = new OdbcDataAdapter(strSQL, connection);
            adapter.Fill(ds, "Production BOM Header");
            Console.WriteLine("Complete");
            Console.Write("Applying join-filter to 'Production BOM Header': ");
            //Es werden nur die Stücklisten-Köpfe benötigt, die mit einem Artikel verknüpft sind
            JoinFilter(ds.Tables["Production BOM Header"].Columns["Item No."], ds.Tables["Item_Filtered"].Columns["No."]);
            Console.WriteLine("Complete");

            //Production BOM Line enthält jede Stücklisten-Zeile. Also eine Verknüpfung von Stückliste und Artikel (m:n)
            Console.Write("Reading 'Production BOM Line': ");
            strSQL = "select * from \"Production BOM Line\"";
            adapter = new OdbcDataAdapter(strSQL, connection);
            adapter.Fill(ds, "Production BOM Line");
            Console.WriteLine("Complete");
            Console.Write("Applying join-filter to 'Production BOM Line': ");
            JoinFilter(ds.Tables["Production BOM Line"].Columns["Production BOM No."], ds.Tables["Production BOM Header"].Columns["No."]);
            Console.WriteLine("Complete");

            //Entsprechend der Stücklisten-Zeilen werden die benötigten Artikel gefiltert
            JoinFilter(ds.Tables["Item"].Columns["No."], ds.Tables["Production BOM Line"].Columns["No."]);
            
            ds.Tables["Item"].Merge(ds.Tables["Item_Filtered"]);

            Console.WriteLine("\nDone!");
            Console.ReadLine();

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

        
    }
}
 