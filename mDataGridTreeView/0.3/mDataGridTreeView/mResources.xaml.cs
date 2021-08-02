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

namespace mUserControls
{
    public partial class mResources
    {
        public void treeviewitem_expanded(object sender, EventArgs e) 
        {
            TreeViewItem itm = sender as TreeViewItem;
            var Parent = (itm.Tag as mDataGridTreeView);
        }
    }
}
