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
    public partial class mWindowGetFilterString : Window
    {
        string Filter = "";

        public mWindowGetFilterString()
        {
            InitializeComponent();
        }

        //returns null if user canceled
        public string ShowDialog(string Title, string ColumnKey, mDataGridTreeView dgtv)
        {
            if (dgtv.FilterStrings.Rows.Find(ColumnKey) != null)
                txtFilter.Text = (string)dgtv.FilterStrings.Rows.Find(ColumnKey)["Filter"];
            this.Title = Title;
            txtFilter.Focus();
            txtFilter.CaretIndex = txtFilter.Text.Length;
            base.ShowDialog();
            return Filter;
        }
        public string ShowDialog(string Title, string filter)
        {
            txtFilter.Text = filter;
            this.Title = Title;
            txtFilter.Focus();
            txtFilter.CaretIndex = txtFilter.Text.Length;
            base.ShowDialog();
            return Filter;
        }

        private void btnOK_Click(object sender, RoutedEventArgs e)
        {
            ok();
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            cancel();
        }

        private void txtFilter_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            e.Handled = true;
            switch (e.Key)
            {
                case Key.Enter: ok(); break;
                case Key.Escape: cancel(); break;
                default: e.Handled = false; break;
            }
        }

        private void ok()
        {
            Filter = txtFilter.Text;
            this.Close();
        }
        private void cancel()
        {
            Filter = null;
            this.Close();
        }

        private void txtFilter_KeyDown(object sender, KeyEventArgs e)
        {

        }

        private void txtFilter_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (txtFilter.Text.Trim() != "")
                btnOK.IsEnabled = true;
        }
    }
}
