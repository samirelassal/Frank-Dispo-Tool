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

namespace Frank_UI
{
    public enum SwitchState
    {
        Include,
        Exclude,
        Neutral
    }

    public delegate void SwitchStateChangedEventHandler(object sender, SwitchState state);

    /// <summary>
    /// Interaction logic for ThreeStepSwitch.xaml
    /// </summary>
    public partial class ThreeStepSwitch : UserControl
    {
        bool DragSwitch = false;
        public event SwitchStateChangedEventHandler SwitchStateChanged;
        private SwitchState _SwitchState = SwitchState.Neutral;
        public SwitchState SwitchState
        {
            get
            {
                return _SwitchState;
            }
            set
            {
                if (_SwitchState != value)
                {
                    _SwitchState = value;
                    switch (value)
                    {
                        case SwitchState.Include:
                            this.Resources["grdPlusVisibility"] = Visibility.Visible;
                            this.Resources["grdMinusVisibility"] = Visibility.Collapsed;
                            Grid.SetColumn(btnSwitch, 2);
                            Grid.SetColumnSpan(btnSwitch, 1);
                            btnSwitch.Margin = new Thickness(0);
                            if (SwitchStateChanged != null)
                                SwitchStateChanged(this, SwitchState);
                            break;
                        case SwitchState.Exclude:
                            this.Resources["grdPlusVisibility"] = Visibility.Collapsed;
                            this.Resources["grdMinusVisibility"] = Visibility.Visible;
                            Grid.SetColumn(btnSwitch, 0);
                            Grid.SetColumnSpan(btnSwitch, 1);
                            btnSwitch.Margin = new Thickness(0);
                            if (SwitchStateChanged != null)
                                SwitchStateChanged(this, SwitchState);
                            break;
                        case SwitchState.Neutral:
                            this.Resources["grdPlusVisibility"] = Visibility.Collapsed;
                            this.Resources["grdMinusVisibility"] = Visibility.Collapsed;
                            Grid.SetColumn(btnSwitch, 1);
                            Grid.SetColumnSpan(btnSwitch, 1);
                            btnSwitch.Margin = new Thickness(0);
                            if (SwitchStateChanged != null)
                                SwitchStateChanged(this, SwitchState);
                            break;
                    }
                }
            }
        }

        public ThreeStepSwitch()
        {
            InitializeComponent();
        }

        private void bdInclude_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            SwitchState = SwitchState.Include;
        }

        private void bdExclude_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            SwitchState = SwitchState.Exclude;
        }

        private void bdNeutral_MouseDown(object sender, MouseButtonEventArgs e)
        {
            SwitchState = SwitchState.Neutral;
        }

        private void btnSwitch_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {


        }

        private void btnSwitch_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {

        }

        private void btnSwitch_MouseMove(object sender, MouseEventArgs e)
        {
            if (DragSwitch)
            {
                Grid.SetColumnSpan(btnSwitch, 3);
                Grid.SetColumn(btnSwitch, 0);
                double newLeft = e.GetPosition(this).X - 12.5;
                double newRight = 75 - newLeft + 12.5;
                if (newLeft >= 0 && newLeft <= 50)
                    btnSwitch.Margin = new Thickness(newLeft, 0, newRight, 0);
            }
        }

        private void btnSwitch_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            btnSwitch.CaptureMouse();
            DragSwitch = true;
        }

        private void btnSwitch_PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (DragSwitch)
            {
                double offset = 18;
                DragSwitch = false;
                btnSwitch.ReleaseMouseCapture();
                if (btnSwitch.Margin.Left <= offset)
                    SwitchState = SwitchState.Exclude;
                else if (btnSwitch.Margin.Left >= 50.0 - offset)
                    SwitchState = SwitchState.Include;
                else
                    SwitchState = SwitchState.Neutral;
            }
            //without this line, the combobox list will disappear as soon as this method ends. I don't know the exact reason for that.
            e.Handled = true;
        }
    }
}
