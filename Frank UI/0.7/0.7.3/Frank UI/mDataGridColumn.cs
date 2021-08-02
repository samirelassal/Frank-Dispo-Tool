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
using System.Collections.ObjectModel;


namespace Frank_UI
{

    class mDataGridColumn : DataGridTextColumn
    {
        public static int counter = 0;

        public Type DataType;

        #region DependencyProperty
        public static readonly DependencyProperty EnableFilterProperty = DependencyProperty.Register("EnableFilter", typeof(bool), typeof(mDataGridColumn),
            new FrameworkPropertyMetadata(false, FrameworkPropertyMetadataOptions.AffectsRender, new PropertyChangedCallback(OnEnableFilterChanged)));

        public static readonly DependencyProperty CaptionProperty = DependencyProperty.Register("Caption", typeof(string), typeof(mDataGridColumn),
            new FrameworkPropertyMetadata("", FrameworkPropertyMetadataOptions.AffectsRender, new PropertyChangedCallback(OnCaptionChanged)));

        public static readonly DependencyProperty CellTypeProperty = DependencyProperty.Register("CellType", typeof(string), typeof(mDataGridColumn),
            new FrameworkPropertyMetadata("Text", FrameworkPropertyMetadataOptions.AffectsRender, new PropertyChangedCallback(OnCellTypeChanged)));

        public bool EnableFilter
        {
            get { return (bool)GetValue(EnableFilterProperty); }
            set { SetValue(EnableFilterProperty, value); }
        }
        public string Caption
        {
            get { return (string)GetValue(CaptionProperty); }
            set { SetValue(CaptionProperty, value); }
        }

        public string CellType
        {
            get { return (string)GetValue(CellTypeProperty); }
            set { SetValue(CellTypeProperty, value); }
        }


        private static void OnEnableFilterChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            mDataGridColumn clm = (mDataGridColumn)d;
            bool newValue = (bool)e.NewValue;
            if (newValue)
                clm.filteroptions.Visibility = Visibility.Visible;
            else
                clm.filteroptions.Visibility = Visibility.Collapsed;

        }
        private static void OnCaptionChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            mDataGridColumn clm = (mDataGridColumn)d;
            string newValue = (string)e.NewValue;
            clm.lbl.Content = newValue;
        }
        private static void OnCellTypeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            mDataGridColumn clm = (mDataGridColumn)d;
            string newValue = (string)e.NewValue;
            switch (newValue)
            {
                case "TreeViewItemContent": clm.CellStyle = (Style)ResourceHelper.dict["TreeViewItemContentStyle"]; clm.SortMemberPath = "[Key]"; break;
                case "TreeViewItemText": clm.CellStyle = (Style)ResourceHelper.dict["TreeViewItemTextStyle"]; break;
                default: clm.CellStyle = (Style)ResourceHelper.dict["BaseStyle"]; counter++; break;
            }
        }

        #endregion

        #region Public events

        public event RoutedEventHandler FilterOptionsClick;
        public event RoutedEventHandler FilterDeletedClick;

        #endregion

        StackPanel pnl = new StackPanel();
        Label lbl = new Label();
        Button filteroptions = new Button();
        Button btnDeleteFilter = new Button();

        public bool DeleteFilterVisibility
        {
            get
            {
                return btnDeleteFilter.Visibility == Visibility.Visible;
            }
            set
            {
                if (value)
                    btnDeleteFilter.Visibility = Visibility.Visible;
                else
                    btnDeleteFilter.Visibility = Visibility.Collapsed;
            }
        }

        public mDataGridColumn()
        {
            DeleteFilterVisibility = false;
            if (this.CellStyle == null)
                this.CellStyle = (Style)ResourceHelper.dict["BaseStyle"];


            filteroptions.Foreground = SystemColors.ControlDarkBrush;
            filteroptions.Content = "...";
            filteroptions.Click += new RoutedEventHandler(filteroptions_Click);

            btnDeleteFilter.Style = (Style)ResourceHelper.dict["DeleteButton"];
            btnDeleteFilter.Click += BtnDeleteFilter_Click;

            pnl.Orientation = Orientation.Horizontal;
            pnl.Children.Add(lbl);
            pnl.Children.Add(filteroptions);
            pnl.Children.Add(btnDeleteFilter);

            if (EnableFilter)
                filteroptions.Visibility = System.Windows.Visibility.Visible;
            else
                filteroptions.Visibility = System.Windows.Visibility.Collapsed;

            base.Header = pnl;
        }

        private void BtnDeleteFilter_Click(object sender, RoutedEventArgs e)
        {
            DeleteFilterVisibility = false;
            if (FilterDeletedClick != null)
                FilterDeletedClick(sender, e);
        }

        private void filteroptions_Click(object sender, RoutedEventArgs e)
        {
            if (FilterOptionsClick != null)
                FilterOptionsClick(sender, e);
        }
    }
}
