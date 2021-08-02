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
using System.ComponentModel;

namespace Frank_UI
{
    /// <summary>
    /// Interaction logic for WindowDescriptionFilter.xaml
    /// </summary>
    public partial class mWindowGetFilterString : Window
    {
        string Filter = "";
        mList<string> Filters = new mList<string>();

        public mWindowGetFilterString()
        {
            InitializeComponent();
            Filters.OnAdd += Filters_OnChanged;
            Filters.OnRemove += Filters_OnChanged;
        }

        private void Filters_OnChanged(object sender, object item)
        {
            cmbx.Text = "";
            foreach (string str in Filters)
            {
                cmbx.Text += str + " | ";
            }
            if (cmbx.Text.EndsWith(" | "))
                cmbx.Text = cmbx.Text.Remove(cmbx.Text.Length - 3, 3);
            if (Filters.Count > 0)
                btnOK.IsEnabled = true;

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
        
        public mList<string> ShowDialog(string Title, mList<string> filters, params string[] values)
        {
            this.Title = Title;          
            AddCmbxItems(values);
            foreach (string str in filters)
            {
                foreach (mComboBoxCheckableItem cbi in cmbx.Items)
                {
                    if (cbi.Text == str)
                        cbi.IsChecked = true;
                }
            }
            base.ShowDialog();
            return Filters;
        }
        public mList<string> ShowDialog(string Title, mList<string> filters, params object[] values)
        {
            string[] array = Array.ConvertAll(values, item => item.ToString().Split('=')[1].Trim(' ', '{', '}'));
            return ShowDialog(Title, filters, array);
        }

        private void AddCmbxItems(string[] items)
        {
            txtFilter.Visibility = Visibility.Collapsed;
            cmbx.Visibility = Visibility.Visible;
            foreach (string str in items)
            {
                cmbx.Items.Add(new mComboBoxCheckableItem(str, false, Filters));
            }
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

        private void cmbx_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            e.Handled = true;
            cmbx.SelectedIndex = -1;
        }
    }

    public class mComboBoxCheckableItem : StackPanel
    {
        CheckBox chk = new CheckBox();
        TextBlock tbl = new TextBlock();
        mList<string> ContainingList;

        public bool IsChecked
        {
            get { return (bool)chk.IsChecked; }
            set
            {
                if (value)
                {
                    chk.IsChecked = true;
                    if (ContainingList != null && !ContainingList.Contains(Text))
                        ContainingList.Add(Text);
                }
                else
                {
                    chk.IsChecked = false;
                    if (ContainingList != null && ContainingList.Contains(Text))
                        ContainingList.Remove(Text);
                }
            }

        }

        public string Text
        {
            get { return tbl.Text; }
            set
            {
                if (ContainingList != null && ContainingList.Contains(Text))
                    ContainingList.Remove(Text);
                tbl.Text = value;
                if (ContainingList != null && IsChecked)
                    ContainingList.Add(Text);
            }
        }

        public mComboBoxCheckableItem() : base()
        {
            IsChecked = false;
            this.Orientation = Orientation.Horizontal;

            tbl.Margin = new Thickness(5, 0, 0, 0);
            chk.Checked += Chk_Checked;
            chk.Unchecked += Chk_Unchecked;

            this.Children.Add(chk);
            this.Children.Add(tbl);
        }

        private void Chk_Unchecked(object sender, RoutedEventArgs e)
        {
            if (ContainingList != null && ContainingList.Contains(Text))
                ContainingList.Remove(Text);
        }

        private void Chk_Checked(object sender, RoutedEventArgs e)
        {
            if (ContainingList != null && !ContainingList.Contains(Text))
                ContainingList.Add(Text);
        }

        public mComboBoxCheckableItem(string Text, bool IsChecked) : this()
        {
            this.Text = Text;
            this.IsChecked = IsChecked;
        }

        public mComboBoxCheckableItem(string Text, bool IsChecked, mList<string> ContainingList) : this()
        {
            this.Text = Text;
            this.IsChecked = IsChecked;
            this.ContainingList = ContainingList;
        }
    }
}
