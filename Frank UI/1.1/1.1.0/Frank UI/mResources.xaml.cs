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

namespace Frank_UI
{
    public partial class mResources
    {
        public void treeviewitem_expanded(object sender, RoutedEventArgs e)
        {
            TreeViewItem itm = sender as TreeViewItem;
            var Parent = (itm.Tag as mDataGridTreeView);
            Parent.TreeViewItemExpanded(sender, e);
        }
        public void treeviewitem_collapsed(object sender, RoutedEventArgs e)
        {
            TreeViewItem itm = sender as TreeViewItem;
            var Parent = (itm.Tag as mDataGridTreeView);
            Parent.TreeViewItemCollapsed(sender, e);
        }

        private void Grid_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            try
            {
                Grid itm = sender as Grid;
                var Parent = (itm.Tag as mTreeViewItem);
                var ParentParent = (Parent.Tag as mDataGridTreeView);
                ParentParent.MouseRightButtonDown(Parent, e);
            }
            catch (Exception) { }
        }
    }
}
