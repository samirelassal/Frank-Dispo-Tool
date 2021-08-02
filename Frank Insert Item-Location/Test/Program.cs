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
namespace Test
{
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
            strSQL = "select \"No.\", Location from Item where \"No.\" = 'AF0073'";
            adapter = new OdbcDataAdapter();
            adapter.SelectCommand = new OdbcCommand(strSQL, connection);
            cmdbuilder = new OdbcCommandBuilder(adapter);
            cmdbuilder.QuotePrefix = cmdbuilder.QuoteSuffix = "\"";
            adapter.Fill(Item);
            Item.PrimaryKey = new DataColumn[] { Item.Columns["No."] };
            var item = Item.Rows.Find("AF0073");
            item.BeginEdit();
            item["Location"] = "VE";
            item.EndEdit();
            adapter.UpdateCommand = cmdbuilder.GetUpdateCommand();
            adapter.Update(Item);
        }
    }
}
