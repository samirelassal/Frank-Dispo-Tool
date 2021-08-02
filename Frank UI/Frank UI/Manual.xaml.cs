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
using System.Windows.Shapes;

namespace Frank_UI
{
    /// <summary>
    /// Interaction logic for Manual.xaml
    /// </summary>
    public partial class Manual : Window
    {
        public Manual()
        {
            InitializeComponent();
        }

        static readonly string Manuals = System.AppDomain.CurrentDomain.BaseDirectory + @"Manuals";
        static readonly string Filter_Man = Manuals + @"\filter.html";

        private void trvFilter_Selected(object sender, RoutedEventArgs e)
        {
            browser.Navigate(Filter_Man);
        }
    }
}
