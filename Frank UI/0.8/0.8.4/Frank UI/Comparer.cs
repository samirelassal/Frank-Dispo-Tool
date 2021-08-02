using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections;
using System.Data;

namespace Frank_UI
{
    class InSalesMengeNumericAscendingComparer : IComparer
    {
        int IComparer.Compare(object x, object y)
        {
            var line1 = (InSales)x;
            var line2 = (InSales)y;
            int i1 = int.Parse(line1.Menge);
            int i2 = int.Parse(line2.Menge);
            if (i1 > i2)
                return 1;
            if (i1 < i2)
                return -1;
            else
                return 0;
        }
    }
    class InSalesNumericNumericDescendingComparer : IComparer
    {
        int IComparer.Compare(object x, object y)
        {
            var line1 = (InSales)x;
            var line2 = (InSales)y;
            int i1 = int.Parse(line1.Menge);
            int i2 = int.Parse(line2.Menge);
            if (i1 < i2)
                return 1;
            if (i1 > i2)
                return -1;
            else
                return 0;
        }
    }

    class DictionaryLieferterminDateTimeAscendingComparer : IComparer
    {
        int IComparer.Compare(object x, object y)
        {
            var dict1 = (Dictionary<string, object>)x;
            var dict2 = (Dictionary<string, object>)y;
            DateTime i1 = DateTime.Parse(dict1["Liefertermin"].ToString());
            DateTime i2 = DateTime.Parse(dict2["Liefertermin"].ToString());
            if (i1 > i2)
                return 1;
            if (i1 < i2)
                return -1;
            else
                return 0;
        }
    }
    class DictionaryLieferterminDateTimeDescendingComparer : IComparer
    {
        int IComparer.Compare(object x, object y)
        {
            var dict1 = (Dictionary<string, object>)x;
            var dict2 = (Dictionary<string, object>)y;
            DateTime i1 = DateTime.Parse(dict1["Liefertermin"].ToString());
            DateTime i2 = DateTime.Parse(dict2["Liefertermin"].ToString());
            if (i1 < i2)
                return 1;
            if (i1 > i2)
                return -1;
            else
                return 0;
        }
    }

    class DataRowLieferterminDateTimeAscendingComparer : IComparer
    {
        int IComparer.Compare(object x, object y)
        {
            var dr1 = (DataRow)x;
            var dr2 = (DataRow)y;
            DateTime i1 = DateTime.Parse(dr1["Benötigt bis Liefertermin"].ToString());
            DateTime i2 = DateTime.Parse(dr2["Benötigt bis Liefertermin"].ToString());
            if (i1 > i2)
                return 1;
            if (i1 < i2)
                return -1;
            else
                return 0;
        }
    }
    class DataRowLieferterminDateTimeDescendingComparer : IComparer
    {
        int IComparer.Compare(object x, object y)
        {
            var dr1 = (DataRow)x;
            var dr2 = (DataRow)y;
            DateTime i1 = DateTime.Parse(dr1["Benötigt bis Liefertermin"].ToString());
            DateTime i2 = DateTime.Parse(dr2["Benötigt bis Liefertermin"].ToString());
            if (i1 < i2)
                return 1;
            if (i1 > i2)
                return -1;
            else
                return 0;
        }
    }
}
