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
    class mDataGridColumn : DataGridTemplateColumn
    {
        #region DependencyProperty
        public static readonly DependencyProperty EnableFilterProperty = DependencyProperty.Register("EnableFilter", typeof(bool), typeof(mDataGridColumn),
            new FrameworkPropertyMetadata(false, FrameworkPropertyMetadataOptions.AffectsRender, new PropertyChangedCallback(OnEnableFilterChanged)));

        public static readonly DependencyProperty CaptionProperty = DependencyProperty.Register("Caption", typeof(string), typeof(mDataGridColumn),
            new FrameworkPropertyMetadata("", FrameworkPropertyMetadataOptions.AffectsRender, new PropertyChangedCallback(OnCaptionChanged)));

        public static readonly DependencyProperty ReferenceKeyProperty = DependencyProperty.Register("ReferenceKey", typeof(string), typeof(mDataGridColumn),
            new FrameworkPropertyMetadata("Value", FrameworkPropertyMetadataOptions.None, new PropertyChangedCallback(OnReferenceKeyChanged)));

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

        public string ReferenceKey
        {
            get { return (string)GetValue(ReferenceKeyProperty); }
            set { SetValue(ReferenceKeyProperty, value); }
        }

        private static void OnEnableFilterChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            mDataGridColumn clm = (mDataGridColumn) d;
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
        private static void OnReferenceKeyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) 
        {
            dict["ContentKey"] = e.NewValue;
        }
        #endregion

        #region Public events

        public event RoutedEventHandler FilterOptionsClick;

        #endregion

        StackPanel pnl = new StackPanel();
        Label lbl = new Label();
        Button filteroptions = new Button();
        static ResourceDictionary dict = new ResourceDictionary();

        static mDataGridColumn() 
        {
            dict.Source = new Uri("/mDataGridTreeView;component/mResources.xaml", UriKind.RelativeOrAbsolute);
        }

        public mDataGridColumn()
        {
            this.CellTemplate = (DataTemplate) dict["ContentTemplate"];

            filteroptions.Foreground = SystemColors.ControlDarkBrush;
            filteroptions.Content = "...";
            filteroptions.Click += new RoutedEventHandler(filteroptions_Click);

            pnl.Orientation = Orientation.Horizontal;
            pnl.Children.Add(lbl);
            pnl.Children.Add(filteroptions);

            if (EnableFilter)
                filteroptions.Visibility = System.Windows.Visibility.Visible;
            else
                filteroptions.Visibility = System.Windows.Visibility.Collapsed;

            base.Header = pnl;
        }
        private void filteroptions_Click(object sender, RoutedEventArgs e) 
        {
            if (FilterOptionsClick != null)
                FilterOptionsClick(sender, e);
        }
    }
}
