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
using System.Windows.Media.Animation;

namespace Frank_UI
{
    /// <summary>
    /// Interaction logic for LoadingWindow.xaml
    /// </summary>
    public partial class LoadingWindow : Window
    {
        public LoadingWindow()
        {
            InitializeComponent();
            //DwmDropShadow.DropShadowToWindow(this);
            lblCopyright.Content = lblCopyright.Content.ToString().Replace("yyyy", DateTime.Now.Year.ToString());
            lblVersion.Content = lblVersion.Content.ToString().Replace("{version}", Frank_UI.Properties.Resources.Version);
        }
    }
}
