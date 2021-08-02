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
    /// Interaction logic for WindowDescriptionFilter.xaml
    /// </summary>
    public partial class WindowGetFilterString : Window
    {
        string Filter = "";

        public WindowGetFilterString()
        {
            InitializeComponent();
        }

        public new string ShowDialog()
        {
            base.ShowDialog();
            return Filter;
        }

        private void btnOK_Click(object sender, RoutedEventArgs e)
        {
            Filter = txtFilter.Text;
            this.Close();
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            Filter = "";
            this.Close();
        }
    }
}
