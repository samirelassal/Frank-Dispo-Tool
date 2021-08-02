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

    public delegate void OnAddEventHandler(object sender, object item);

    public class mList<T> : List<T>
    {
        public event OnAddEventHandler OnAdd;

        public void Add(T item)
        {
            if (null != OnAdd)
            {
                OnAdd(this, item);
            }
            base.Add(item);
        }

    }

    public class Column
    {
        Label lbl = new Label();
        public string Caption
        {
            set
            {
                lbl.Content = value;
            }

            get
            {
                return lbl.Content.ToString();
            }
        }
        public double Width
        {
            set
            {
                lbl.Width = value;
            }
            get
            {
                return lbl.Width;
            }
        }

        public Column() { }
        public Column(string caption)
        {
            Caption = caption;
        }

        public void AddToElement(Panel element)
        {
            element.Children.Add(lbl);
        }

    }

    public  class mTreeViewItem : TreeViewItem
    {
        string _nummer;
        public string Nummer 
        {
            get 
            {
                return _nummer;
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

        public mList<Column> Columns = new mList<Column>();

        public mTreeViewItem() : base() { }

        public mTreeViewItem(string quantity, string nummer, string status) : base()
        {
            Columns.OnAdd += new OnAddEventHandler(Columns_OnAdd);
            pnl.Orientation = Orientation.Horizontal;

            Rectangle rct = new Rectangle();
            rct.Width = rct.Height = 10;
            switch (status.Trim().ToLower()) 
            {
                case "red": rct.Fill = new SolidColorBrush(Colors.Red); _status = Status.Red; break;
                case "yellow": rct.Fill = new SolidColorBrush(Colors.Yellow); _status = Status.Yellow; break;
                case "green": rct.Fill = new SolidColorBrush(Colors.Green); _status = Status.Green; break;
                case "Error": rct.Fill = new SolidColorBrush(Colors.Gray); _status = Status.Error; break;
            }
            pnl.Children.Add(rct);


            _nummer = nummer;
            Label lbl = new Label();
            if (quantity == null)
                Columns.Add(new Column(Nummer));
            else
                Columns.Add(new Column(quantity + " x " + Nummer));

            this.Header = pnl;
        }

        public List<mTreeViewItem> FindExisting(ItemCollection collection) 
        {
            List<mTreeViewItem> result = new List<mTreeViewItem>();
            foreach (mTreeViewItem itm in collection) 
            {
                if (itm.Nummer.ToString().Trim() == this.Nummer.ToString().Trim())
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
            if (Nummer == caption)
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
            if (Item.Nummer == caption)
                return true;
            foreach (mTreeViewItem itm in Item.Items)
            {
                if (rekursiveContains(itm, caption))
                    return true;
            }
            return false;
        }

        private void Columns_OnAdd(object sender, object item) 
        {
            Column clm = item as Column;
            clm.AddToElement(pnl);

        }
    }
}
