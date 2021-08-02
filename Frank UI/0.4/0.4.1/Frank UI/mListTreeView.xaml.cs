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
using System.ComponentModel;

namespace Frank_UI
{
    //public delegate void DictionaryAddEventHandler(object sender, object[] args);
    //public class mDictionary<TKey, TValue> : Dictionary<TKey, TValue> 
    //{
    //    public event DictionaryAddEventHandler OnAdd;

    //    public void Add(TKey key, TValue value)
    //    {
    //        if (OnAdd != null)
    //            OnAdd(this, new object[] { key, value });
    //        base.Add(key, value);
    //    }
    //}

    /// <summary>
    /// Interaktionslogik für mListTreeView.xaml
    /// </summary>
    public partial class mListTreeView : UserControl
    {
        public ItemCollection Items 
        {
            get 
            {
                return trv.Items;
            }
        }

        public object SelectedItem 
        {
            get 
            {
                return trv.SelectedItem;
            }
        }

        public event RoutedPropertyChangedEventHandler<object> SelectedItemChanged;

        public mListTreeView()
        {
            InitializeComponent();
            ((INotifyPropertyChanged)gclmNummer).PropertyChanged += new PropertyChangedEventHandler(gclmNummer_PropertyChanged);
            ((INotifyPropertyChanged)gclmLiefertermin).PropertyChanged += new PropertyChangedEventHandler(gclmLiefertermin_PropertyChanged);
        }

        private void gclmNummer_PropertyChanged(object sender, PropertyChangedEventArgs e) 
        {
            double offset_width = 33;

            if (e.PropertyName == "ActualWidth") 
            {
                if (gclmNummer.ActualWidth <= offset_width) 
                {
                    gclmNummer.Width = offset_width;
                    return;
                }
                foreach (mTreeViewItem itm in trv.Items)
                {
                    itm.Columns[0].Width = gclmNummer.ActualWidth - offset_width;
                    if (itm.IsExpanded)
                        ChangeWidthRekursive(23, 2, itm);
                }
            }
        }

        private void ChangeWidthRekursive(double offset_width, int level, mTreeViewItem itm) 
        {
            double newWidth = gclmNummer.ActualWidth - level * offset_width;
            foreach (mTreeViewItem subitem in itm.Items) 
            {
                if (newWidth > 0)
                {
                    subitem.Columns[0].Width = newWidth;
                    subitem.Visibility = System.Windows.Visibility.Visible;
                }
                else
                    subitem.Visibility = System.Windows.Visibility.Hidden;
                ChangeWidthRekursive(offset_width, level + 1, subitem);
            }
        }

        private void gclmLiefertermin_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "ActualWidth")
            {
                foreach (mTreeViewItem itm in trv.Items)
                {
                    if (itm.Columns.Count >= 2)
                    {
                        itm.Columns[1].Width = gclmLiefertermin.ActualWidth;
                        ChangeWidthRekursive(0, 2, itm);
                    }
                }
            }
        }

        private void trv_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            SelectedItemChanged(sender, e);
        }
    }
}
