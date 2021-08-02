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
using System.Net;
using System.Net.Sockets;
using System.IO;
using System.Data;
using System.Xml;

namespace Frank_UI
{
    public class StringToSolidColorBrushValueConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value == null)
                return null;
            string color = "";

            if (value is string)
                color = (string)value;
            else if (value is TextBlock)
                color = (value as TextBlock).Text;
            else if (value is Label)
                color = (value as Label).Content.ToString();
            else
            {
                Type type = value.GetType();
                throw new InvalidOperationException("Unsupported type [" + type.Name + "]");
            }

            switch (color)
            {
                case "red": return new SolidColorBrush(Colors.Red);
                case "yellow": return new SolidColorBrush(Colors.Yellow);
                case "green": return new SolidColorBrush(Colors.Green);
            }
            return null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
