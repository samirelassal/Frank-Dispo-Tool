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
    public enum Status
    {
        Red,
        Yellow,
        Green,
        Error
    }


    class mTreeViewItem : TreeViewItem
    {
        private string _caption;
        public string Caption 
        {
            get 
            {
                return _caption;
            }
        }

        private Status _status;
        public Status status 
        {
            get 
            {
                return _status;
            }
        }

        StackPanel pnl = new StackPanel();

        public mTreeViewItem(string quantity, string caption, string status) : base()
        {
            _caption = caption;
            pnl.Orientation = Orientation.Horizontal;

            Rectangle rct = new Rectangle();
            rct.Width = rct.Height = 10;
            switch (status.Trim().ToLower()) 
            {
                case "red": rct.Fill = new SolidColorBrush(Colors.Red); _status = Status.Red; break;
                case "yellow": rct.Fill = new SolidColorBrush(Colors.Yellow); _status = Status.Yellow; break;
                case "Green": rct.Fill = new SolidColorBrush(Colors.Green); _status = Status.Green; break;
                case "Error": rct.Fill = new SolidColorBrush(Colors.Gray); _status = Status.Error; break;
            }
            pnl.Children.Add(rct);

            Label lbl = new Label();
            if (quantity == null)
                lbl.Content = caption;
            else
                lbl.Content = quantity + " x " + caption;
            pnl.Children.Add(lbl);

            this.Header = pnl;
        }

        public List<mTreeViewItem> FindExisting(ItemCollection collection) 
        {
            List<mTreeViewItem> result = new List<mTreeViewItem>();
            foreach (mTreeViewItem itm in collection) 
            {
                if (itm.Caption.ToString().Trim() == this.Caption.ToString().Trim())
                {
                    result.Add(itm);
                }
            }
            return result;
        }

        public void Highlight() 
        {
            //DDEEFF
            Color c = new Color();
            c.R = 0xDD;
            c.G = 0xEE;
            c.B = 0xFF;
            c.A = 0xFF;
            pnl.Background = new SolidColorBrush(c);
        }
        public void Reset()
        {
            if (this.IsSelected)
                pnl.Background = System.Windows.SystemColors.HighlightBrush;
            else
                pnl.Background = new SolidColorBrush(Colors.White);
        }

        /// <summary>
        /// Delivers true if this Item or one of the subitems has the specified caption
        /// </summary>
        /// <param name="caption"></param>
        /// <returns></returns>
        public bool Contains(string caption) 
        {
            if (Caption == caption)
                return true;
            foreach (mTreeViewItem itm in this.Items) 
            {
                 if (rekursiveContains(itm, caption))
                     return true;
            }
            return false;
        }

        private bool rekursiveContains(mTreeViewItem Item, string caption) 
        {
            if (Item.Caption == caption)
                return true;
            foreach (mTreeViewItem itm in Item.Items)
            {
                if (rekursiveContains(itm, caption))
                    return true;
            }
            return false;
        }

    }
}
