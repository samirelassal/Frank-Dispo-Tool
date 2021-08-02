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
using System.Threading.Tasks;
using System.Collections.ObjectModel;

namespace mUserControls
{
    public class mTreeViewItem : TreeViewItem
    {
        public readonly List<object> Values = new List<object>();    
    }
}
