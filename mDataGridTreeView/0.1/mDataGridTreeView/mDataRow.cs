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
using System.ComponentModel;
using System.Data;

namespace WpfApplication1
{
    public class mDataRow : DataRow
    {
        internal TreeView trv = new TreeView();
        mTreeViewItem item = new mTreeViewItem();
        public mTreeViewItem Item 
        {
            get 
            {
                return trv.Items[0] as mTreeViewItem;
            }
            set 
            {
                trv.Items[0] = value;
            }
        }

        public mDataRow(DataRowBuilder drb) : base(drb)
        {
            trv.Items.Add(item);
            DataGrid dg = new DataGrid();
        }

    }
}
