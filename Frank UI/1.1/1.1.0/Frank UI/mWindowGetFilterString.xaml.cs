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
        List<string> Filters = new List<string>();

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
        
        public List<string> ShowDialog(string Title, List<string> filters, params string[] values)
        {
            this.Title = Title;          
            AddCmbxItems(values);
            foreach (string str in filters)
            {
                foreach (mComboBoxCheckableItem cbi in cmbx.Items)
                {
                    if (cbi.Text == str)
                        cbi.IsCheched = true;
                }
            }
            base.ShowDialog();
            return Filters;
        }

        public List<string> ShowDialog(string Title, List<string> filters, params object[] values)
        {
            string[] array = Array.ConvertAll(values, item => item.ToString().Split('=')[1].Trim(' ', '{', '}'));
            return ShowDialog(Title, filters, array);
        }

        private void AddCmbxItems(object[] items)
        {
            txtFilter.Visibility = Visibility.Collapsed;
            cmbx.Visibility = Visibility.Visible;
            foreach (string str in items)
            {
                mComboBoxCheckableItem itm = new mComboBoxCheckableItem(str, false, Filters);
                itm.Checked += Item_Changed;
                itm.Unchecked += Item_Changed;
                cmbx.Items.Add(itm);
            }
        }

        private void Item_Changed(object sender, RoutedEventArgs e)
        {
            cmbx.Text = "";
            foreach (string str in Filters)
            {
                cmbx.Text += "'" + str + "' | ";
            }
            if (cmbx.Text.EndsWith(" | "))
                cmbx.Text = cmbx.Text.Remove(cmbx.Text.Length - 3, 3);
            if (Filters.Count > 0)
                btnOK.IsEnabled = true;
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
        TextBlock tbl = new TextBlock();
        CheckBox chk = new CheckBox();
        List<string> ContainingDict = new List<string>();

        public event RoutedEventHandler Checked;
        public event RoutedEventHandler Unchecked;

        public bool IsCheched
        {
            get { return chk.IsChecked.Value; }
            set
            {
                chk.IsChecked = value;
                if (value && ContainingDict != null)
                {
                    if (!ContainingDict.Contains(Text))
                        ContainingDict.Add(Text);
                }
                else if (ContainingDict != null && ContainingDict.Contains(Text))
                    ContainingDict.Remove(Text);
            }
        }

        public string Text
        {
            get { return tbl.Text; }
            set
            {
                if (ContainingDict != null && ContainingDict.Contains(Text))
                    ContainingDict.Remove(Text);
                tbl.Text = value;
                if (ContainingDict != null && IsCheched)
                    ContainingDict.Add(Text);
            }
        }

        public mComboBoxCheckableItem() : base()
        {
            this.Orientation = Orientation.Horizontal;
            tbl.Margin = new Thickness(5, 0, 0, 0);
            tbl.VerticalAlignment = VerticalAlignment.Center;
            chk.Checked += Chk_Checked;
            chk.Unchecked += Chk_Unchecked;

            this.Children.Add(chk);
            this.Children.Add(tbl);
        }

        private void Chk_Unchecked(object sender, RoutedEventArgs e)
        {
            ContainingDict.Remove(Text);
            if (Checked != null)
                Checked(sender, e);
        }

        private void Chk_Checked(object sender, RoutedEventArgs e)
        {
            ContainingDict.Add(Text);
            if (Unchecked != null)
                Unchecked(sender, e);
        }

        public mComboBoxCheckableItem(string Text, bool IsChecked) : this()
        {
            this.Text = Text;
            this.IsCheched = IsChecked;
        }

        public mComboBoxCheckableItem(string Text, bool IsChecked, List<string> ContainingDict) : this(Text, IsChecked)
        {
            this.ContainingDict = ContainingDict;
        }
    }
}
