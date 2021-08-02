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
using System.Data;
using System.Data.Odbc;

namespace Frank___ODBC_Client
{
    /// <summary>
    /// Interaktionslogik für MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            ExecuteSql();
        }

        DataSet ds = new DataSet();

        public void ExecuteSql() 
        {
            this.Title = "Loading...";
            string strSQL = "select Item.\"No.\", Item.Description, Item.\"Evers Nr System\" from Item inner join \"Item Ledger Entry\" on Item.\"No.\" = \"Item Ledger Entry\".\"Item No.\" where \"Item Ledger Entry\".\"Posting Date\" > '2010-01-01' and (\"Item Ledger Entry\".\"Entry Type\" = 0 or \"Item Ledger Entry\".\"Entry Type\" = 1 or \"Item Ledger Entry\".\"Entry Type\" = 5) Group By Item.\"No.\", Item.Description, Item.\"Evers Nr System\"";
            OdbcConnection connection = new OdbcConnection("DSN=Navision Frank-Backup; Asynchronous Processing=true");
            OdbcCommand cmd = new OdbcCommand(strSQL, connection);
            connection.Open();
            AsyncCallback callback = new AsyncCallback(CallbackMethod);
        }

        static void CallbackMethod(IAsyncResult result) { }
    }
}
