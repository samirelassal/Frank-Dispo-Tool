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
    public delegate void OnAddEventHandler(object sender, object item);

    public class mList<T> : List<T>
    {
        public event OnAddEventHandler OnAdd;

        public new void Add(T item)
        {
            if (null != OnAdd)
            {
                OnAdd(this, item);
            }
            base.Add(item);
        }

    }

    public  class mTreeViewItem : TreeViewItem
    {
        private static mList<mTreeViewItem> _mtreeViewItemCollection = new mList<mTreeViewItem>();
        public static mList<mTreeViewItem> mTreeViewItemCollection { get { return _mtreeViewItemCollection; } }

        bool _isHighlighted = false;
        public bool IsHighlighted
        {
            get { return _isHighlighted; }
            private set { _isHighlighted = value; }
        }

        #region Dependency properties
        public static readonly DependencyProperty IconProperty = DependencyProperty.Register("Icon", typeof(object), typeof(mTreeViewItem),
            new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.AffectsRender, new PropertyChangedCallback(OnIconChanged)));

        public object Icon
        {
            get { return GetValue(IconProperty); }
            set { SetValue(IconProperty, value); }
        }

        private static void OnIconChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) { }
        #endregion

        public mTreeViewItem() : base()
        {
            this.Style = (Style)ResourceHelper.dict["mTreeViewItemStyle"];
            mTreeViewItemCollection.Add(this);
        }

        public mTreeViewItem(bool IsParent) : base()
        {
            this.Style = (Style)ResourceHelper.dict["mTreeViewItemStyle"];
            if(IsParent)
                mTreeViewItemCollection.Add(this);
        }

        public void ExpandAll()
        {
            this.IsExpanded = true;
            foreach (mTreeViewItem itm in this.Items)
            {
                itm.ExpandAll();
            }
        }

        public void CollapseAll()
        {
            this.IsExpanded = false;
            foreach (mTreeViewItem itm in this.Items)
            {
                itm.CollapseAll();
            }
        }

        public void Highlight()
        {
            this.Background = (SolidColorBrush)ResourceHelper.dict["Highlight"];
            IsHighlighted = true;
        }

        public void UnHighlight()
        {
            this.Background = new SolidColorBrush(Colors.Transparent);
            IsHighlighted = false;
        }
    }
}
