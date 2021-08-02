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

namespace WpfApplication1
{
    public class mTreeViewItem
    {
        public mList<object> Values = new mList<object>();
        public TreeViewItem trvItem = new TreeViewItem();
        public mDataRow Parent;
        public mTreeViewItem() 
        {
            Values.OnAdd += new mListChangedEventHandler(Values_OnChange);
            Values.OnMove += new mListChangedEventHandler(Values_OnChange);
            Values.OnRemove += new mListChangedEventHandler(Values_OnChange);
        }

        private void Values_OnChange(object sender, object item) 
        {
            trvItem.Header = Values[0];
        }
    }
}
