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

namespace mUserControls
{
    /// <summary>
    /// Interaktionslogik für MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        public void TestData() 
        {
            StackPanel pnl = new StackPanel();
            pnl.Orientation = Orientation.Horizontal;

            Rectangle rect = new Rectangle();
            rect.Height = rect.Width = 10;
            rect.Fill = new SolidColorBrush(Colors.Green);
            pnl.Children.Add(rect);

            Label lbl = new Label();
            lbl.Content = "Samit der Große!";
            pnl.Children.Add(lbl);

            dgtv.Add(pnl, new List<object> { "Samir", "Samir II" }, "Test", "Test");

        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            TestData();
        }
    }
}
