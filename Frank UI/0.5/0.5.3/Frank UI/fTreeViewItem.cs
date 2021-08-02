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

    class fTreeViewItem : mTreeViewItem
    {
        string _nummer = "";
        string _menge = "";
        string _status = "";
        string _beschaffungsmethode = "";
        string _beschreibung = "";
        string _fertigungsstelle = "";
        string _fehlendeMenge = "";

        Label lbl = new Label();
        StackPanel tooltip = new StackPanel();
        Label lbl_tooltip_header = new Label();
        TextBlock tbl_tooltip_body = new TextBlock();

        public string Nummer
        {
            get
            {
                return _nummer;
            }
            set
            {
                _nummer = value;
                GenerateHeader();
            }
        }
        public string Menge
        {
            get
            {
                return _menge;
            }
            set
            {
                _menge = value;
                GenerateHeader();
            }
        }
        public string Beschaffungsmethode
        {
            get
            {
                return _beschaffungsmethode;
            }
            set
            {
                _beschaffungsmethode = value;
                switch (value.Trim().ToLower())
                {
                    case "einkauf": lbl.Content = "e"; lbl_tooltip_header.Content = "Einkaufs-Artikel"; break;
                    case "fertigungsauftrag": lbl.Content = "f"; lbl_tooltip_header.Content = "Fertigungsauftrags-Artikel"; break;
                }
                GenerateHeader();
            }
        }
        public string Beschreibung
        {
            get
            {
                return _beschreibung;
            }
            set
            {
                _beschreibung = value;
                GenerateHeader();
            }
        }
        public string Fertigungsstelle
        {
            get
            {
                return _fertigungsstelle;
            }
            set
            {
                _fertigungsstelle = value;
                GenerateHeader();
            }
        }
        public string FehlendeMenge
        {
            get
            {
                return _fehlendeMenge;
            }
            set
            {
                _fehlendeMenge = value;
                GenerateHeader();
            }
        }
        public string Status
        {
            get
            {
                return _status;
            }
            set
            {
                _status = value.Trim().ToLower();
                switch (Status)
                {
                    case "red": lbl.Background = new SolidColorBrush(Colors.Red); break;
                    case "yellow": lbl.Background = new SolidColorBrush(Colors.Yellow); break;
                    case "green": lbl.Background = new SolidColorBrush(Colors.Green); break;
                    default: lbl.Background = new SolidColorBrush(Colors.Black); break;
                }
            }
        }

        public fTreeViewItem() : base()
        {
            lbl.Height = lbl.Width = 15;
            lbl.Padding = new Thickness(0);
            lbl.FontWeight = FontWeights.Bold;
            lbl.HorizontalContentAlignment = HorizontalAlignment.Center;
            lbl.VerticalContentAlignment = VerticalAlignment.Center;
            Icon = lbl;
            lbl_tooltip_header.FontWeight = FontWeights.Bold;
            tooltip.Children.Add(lbl_tooltip_header);
            tooltip.Children.Add(tbl_tooltip_body);
            lbl.ToolTip = tooltip;
            
        }
        public fTreeViewItem(string nummer, string beschreibung, string menge, string status, string beschaffungsmethode, string fertigungsstelle, string fehlendeMenge) : this()
        {
            Nummer = nummer;
            Beschreibung = beschreibung;
            Menge = menge;
            Status = status;
            Beschaffungsmethode = beschaffungsmethode;
            Fertigungsstelle = fertigungsstelle;
            FehlendeMenge = fehlendeMenge;
        }

        public void Highlight()
        {
            this.Background = (SolidColorBrush)ResourceHelper.dict["Highlight"];
        }
        public void Reset()
        {
            this.Background = new SolidColorBrush(Colors.Transparent);
        }

        public bool Contains(string nummer)
        {
            if (Nummer == nummer)
                return true;
            foreach (fTreeViewItem itm in this.Items)
            {
                if (rekursiveContains(itm, nummer))
                    return true;
            }
            return false;
        }

        private bool rekursiveContains(fTreeViewItem Item, string nummer)
        {
            if (Item.Nummer == nummer)
                return true;
            foreach (fTreeViewItem itm in Item.Items)
            {
                if (rekursiveContains(itm, nummer))
                    return true;
            }
            return false;
        }

        private void GenerateHeader()
        {
            string header = "";

            if (_menge != "")
                header += _menge + " x ";
            header += Nummer;
            if (_beschreibung != "")
                header += " - " + _beschreibung;
            if (_fertigungsstelle != "")
                header += " - Fertigungsstelle: " + _fertigungsstelle;
            if (_fehlendeMenge != "" && _fehlendeMenge[0] != '-')
                header += " - Fehlende Menge: " + _fehlendeMenge;

            this.Header = header;
        }
    }
}
