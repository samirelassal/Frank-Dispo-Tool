using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using System.Data.Odbc;
using System.Diagnostics;
using System.Web;
using System.Diagnostics;
using System.Runtime.ExceptionServices;

namespace Frank_Insert_Item_Location
{
    public static class Extensions 
    {
        public static void Remove(this DataRowCollection drc, List<DataRow> rows) 
        {
            foreach (DataRow dr in rows) 
            {
                drc.Remove(dr);
            }
        }

        public static void Remove(this DataRowCollection drc, DataRow[] rows)
        {
            foreach (DataRow dr in rows)
            {
                drc.Remove(dr);
            }
        }
    }

    class Program
    {
        static OdbcConnection connection = new OdbcConnection("DSN=Navision Frank-Live");
        static OdbcDataAdapter adapter;
        static OdbcCommandBuilder cmdbuilder;
        static string strSQL;

        static DataTable ProdOrder = new DataTable();
        static DataTable Item = new DataTable();

        static void Main(string[] args)
        {
            Connect();
            getLocation();
            InsertLocation();
        }

        private static void Connect() 
        {
            strSQL = "select \"No.\", \"Source No.\", Fertigungsstelle from \"Production Order\" where not Fertigungsstelle = '' and not \"Source No.\" = ''";
            adapter = new OdbcDataAdapter(strSQL, connection);
            adapter.Fill(ProdOrder);
            ProdOrder.PrimaryKey = new DataColumn[] { ProdOrder.Columns["No."] };

            strSQL = "select \"No.\", Location from Item where \"Replenishment System\" = 1";
            adapter = new OdbcDataAdapter();
            adapter.SelectCommand = new OdbcCommand(strSQL, connection);
            cmdbuilder = new OdbcCommandBuilder(adapter);
            cmdbuilder.QuotePrefix = cmdbuilder.QuoteSuffix = "\"";
            adapter.Fill(Item);
            Item.PrimaryKey = new DataColumn[] { Item.Columns["No."] };
        }

        private static void getLocation() 
        {
            Stopwatch sw = new Stopwatch();
            sw.Start();
            Console.WriteLine("In getLocation():");
            for (int i = 0; i < ProdOrder.Rows.Count; i++)
            {
                DataRow prodorder = ProdOrder.Rows[i];
                string itemNo = prodorder["Source No."].ToString();
                Console.Write("{0}: ", itemNo);
                DataRow[] ProdOrders = ProdOrder.Select("[Source No.] = '" + itemNo + "'");
                Dictionary<string, int> CountedLocations = CountValuesOnColumn(ProdOrder.Columns["Fertigungsstelle"], ProdOrders);
                string MaxLocation = getMaxLocation(CountedLocations);
                
                DataRow item = Item.Rows.Find(itemNo);
                if (item != null)
                {
                    item.BeginEdit();
                    item["Location"] = MaxLocation;
                    item.EndEdit();
                }
                ProdOrder.Rows.Remove(ProdOrders);
                i--;
                Console.WriteLine("{0}", MaxLocation);                
            }
            sw.Stop();
            Console.WriteLine("Time Elapsed: {0}ms", sw.ElapsedMilliseconds.ToString());
        }

        private static void InsertLocation()
        {
            try
            {
                adapter.UpdateCommand = cmdbuilder.GetUpdateCommand();
                adapter.Update(Item);
            }
            catch (Exception ex) 
            {
                Console.WriteLine("{0}: {1}", ex.GetType().ToString(), ex.Message);
            }
        }

        private static string getMaxLocation(Dictionary<string, int> CountedLocations) 
        {
            string result = "";
            int maxnumber = 0;
            foreach (string str in CountedLocations.Keys) 
            {
                if (CountedLocations[str] > maxnumber)
                    result = str;
            }
            return result;
        }

        private static Dictionary<string, int> CountValuesOnColumn(DataColumn dc) 
        {
            Dictionary<string, int> result = new Dictionary<string, int>();
            DataTable dt = dc.Table;
            foreach (DataRow dr in dt.Rows) 
            {
                try
                {
                    result.Add(dr[dc].ToString(), 1);
                }
                catch 
                {
                    result[dr[dc].ToString()]++;
                }
            }
            return result;
        }
        private static Dictionary<string, int> CountValuesOnColumn(DataColumn dc, DataRow[] rows)
        {
            Dictionary<string, int> result = new Dictionary<string, int>();
            foreach (DataRow dr in rows)
            {
                try
                {
                    result.Add(dr[dc].ToString(), 1);
                }
                catch
                {
                    result[dr[dc].ToString()]++;
                }
            }
            return result;
        }
        private static Dictionary<string, int> CountValuesOnColumn(DataColumn dc, List<DataRow> rows)
        {
            Dictionary<string, int> result = new Dictionary<string, int>();
            foreach (DataRow dr in rows)
            {
                try
                {
                    result.Add(dr[dc].ToString(), 1);
                }
                catch
                {
                    result[dr[dc].ToString()]++;
                }
            }
            return result;
        }


    }
}
