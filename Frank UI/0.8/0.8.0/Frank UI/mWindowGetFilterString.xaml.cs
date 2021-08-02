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
        Dictionary<string, SwitchState> newFilters = new Dictionary<string, SwitchState>();

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
        
        public Dictionary<string, SwitchState> ShowDialog(string Title, Dictionary<string, SwitchState> filters, params string[] values)
        {
            this.Title = Title;          
            AddCmbxItems(values);
            foreach (string str in filters.Keys)
            {
                foreach (mComboBoxCheckableItem cbi in cmbx.Items)
                {
                    if (cbi.Text == str)
                        cbi.SwitchState = filters[str];
                }
            }
            base.ShowDialog();
            return newFilters;
        }
        public Dictionary<string, SwitchState> ShowDialog(string Title, Dictionary<string, SwitchState> filters, params object[] values)
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
                mComboBoxCheckableItem itm = new mComboBoxCheckableItem(str, SwitchState.Neutral, newFilters);
                itm.SwitchStateChanged += Itm_SwitchStateChanged;
                cmbx.Items.Add(itm);
            }
        }

        private void Itm_SwitchStateChanged(object sender, SwitchState state)
        {
            cmbx.Text = "";
            foreach (string str in newFilters.Keys)
            {
                if (newFilters[str] == SwitchState.Include)
                    cmbx.Text += "'" + str + "' | ";
                else if (newFilters[str] == SwitchState.Exclude)
                    cmbx.Text += "<>'" + str + "' | ";
            }
            if (cmbx.Text.EndsWith(" | "))
                cmbx.Text = cmbx.Text.Remove(cmbx.Text.Length - 3, 3);
            if (newFilters.Count > 0)
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
        ThreeStepSwitch tss = new ThreeStepSwitch();
        Dictionary<string, SwitchState> ContainingDict = new Dictionary<string, SwitchState>();

        public event SwitchStateChangedEventHandler SwitchStateChanged;

        public SwitchState SwitchState
        {
            get { return tss.SwitchState; }
            set
            {
                tss.SwitchState = value;
                if (value != SwitchState.Neutral && ContainingDict != null)
                {
                    if (ContainingDict.ContainsKey(Text))
                        ContainingDict.Remove(Text);
                    ContainingDict.Add(Text, value);
                }
                else if (ContainingDict != null && ContainingDict.ContainsKey(Text))
                    ContainingDict.Remove(Text);
            }
        }

        public string Text
        {
            get { return tbl.Text; }
            set
            {
                if (ContainingDict != null && ContainingDict.ContainsKey(Text))
                    ContainingDict.Remove(Text);
                tbl.Text = value;
                if (ContainingDict != null && SwitchState != SwitchState.Neutral)
                    ContainingDict.Add(Text, SwitchState);
            }
        }

        public mComboBoxCheckableItem() : base()
        {
            this.Orientation = Orientation.Horizontal;
            tbl.Margin = new Thickness(5, 0, 0, 0);
            Height = 30;
            tbl.VerticalAlignment = VerticalAlignment.Center;

            tss.SwitchStateChanged += Tss_SwitchStateChanged;

            this.Children.Add(tss);
            this.Children.Add(tbl);
        }

        private void Tss_SwitchStateChanged(object sender, SwitchState state)
        {
            SwitchState = state;
            if (SwitchStateChanged != null)
                SwitchStateChanged(this, state);
        }

        public mComboBoxCheckableItem(string Text, SwitchState State) : this()
        {
            this.Text = Text;
            this.SwitchState = State;
        }

        public mComboBoxCheckableItem(string Text, SwitchState State, Dictionary<string, SwitchState> ContainingDict) : this(Text, State)
        {
            this.ContainingDict = ContainingDict;
        }
    }
}
