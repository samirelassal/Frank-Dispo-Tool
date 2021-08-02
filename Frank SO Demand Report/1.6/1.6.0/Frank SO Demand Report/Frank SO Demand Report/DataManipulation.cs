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
    class DataManipulation
    {
        DataTable Parent_Child_Item;

        public DataManipulation(DataTable Parent_Child_Item) 
        {
            this.Parent_Child_Item = Parent_Child_Item;
        }

        public string GetStatusOfProductionItem(DataRow item) 
        {
            string status = "green";

            return status;
        }
    }
}
