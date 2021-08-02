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


namespace Frank_UI_Library
{
    public static class ResourceHelper
    {
        public static ResourceDictionary dict = new ResourceDictionary();

        static ResourceHelper()
        {
            dict.Source = new Uri("/mDataGridTreeView;component/mResources.xaml", UriKind.RelativeOrAbsolute);
        }

        static public string FindNameFromResource(object resourceItem)
        {
            foreach (object key in dict.Keys)
            {
                if (dict[key] == resourceItem)
                {
                    return key.ToString();
                }
            }

            return null;
        }
    }
}
