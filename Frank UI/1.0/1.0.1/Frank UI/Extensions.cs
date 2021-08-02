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
    public static class Extensions
    {
        public static void AddRange<T>(this ItemCollection collection, List<T> list)
        {
            foreach (T item in list)
            {
                collection.Add(item);
            }
        }

        public static bool Remove(this DataRowCollection drc, string primarykey)
        {
            bool succeeded = true;
            try
            {
                drc.Remove(drc.Find(primarykey));
            }
            catch (IndexOutOfRangeException) { succeeded = false; }
            return succeeded;
        }

        public static T FindVisualChild<T>(this DependencyObject depObj) where T : DependencyObject
        {
            if (depObj == null) return null;

            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(depObj); i++)
            {
                var child = VisualTreeHelper.GetChild(depObj, i);

                var result = (child as T) ?? FindVisualChild<T>(child);
                if (result != null) return result;
            }
            return null;
        }

        public static XmlNode Find(this XmlNode node, string name)
        {
            foreach (XmlNode child in node.ChildNodes)
            {
                if (child.Attributes["Name"] != null)
                {
                    if (child.Attributes["Name"].InnerText.Equals(name))
                        return child;
                }
                else if (child.Name.Equals(name))
                {
                    return child;
                }
            }
            return null;
        }

        public static XmlNode Find(this XmlNode node, string attribut, string value)
        {
            foreach (XmlNode child in node.ChildNodes)
            {
                if (child.Attributes[attribut] != null)
                {
                    if (child.Attributes[attribut].InnerText.Equals(value))
                        return child;
                }
            }
            return null;
        }
    }

    public delegate void OnListChangedEventHandler(object sender, object item);
    public class mList<T> : List<T>
    {
        public event OnListChangedEventHandler OnAdd;
        public event OnListChangedEventHandler OnRemove;

        public new void Add(T item)
        {
            base.Add(item);
            if (null != OnAdd)
            {
                OnAdd(this, item);
            }
        }
        public new void Remove(T item)
        {
            base.Remove(item);
            if (null != OnRemove)
            {
                OnRemove(this, item);
            }
            
        }

    }

}
