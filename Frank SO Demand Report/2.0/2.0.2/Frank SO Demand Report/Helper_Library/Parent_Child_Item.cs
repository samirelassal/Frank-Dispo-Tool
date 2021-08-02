using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using System.Data.Odbc;
using System.Diagnostics;
using System.Web;
using System.IO;

namespace Helper_Library
{
    public class Parent_Child_Item
    {
        DataTable dtParent_Child_Item;

        public Parent_Child_Item() 
        {
            dtParent_Child_Item = new DataTable("Parent_Child_Item");
            dtParent_Child_Item.Columns.Add(new DataColumn("ID") { DataType = typeof(int), AutoIncrement = true, AutoIncrementSeed = 1, AutoIncrementStep = 1});
            dtParent_Child_Item.Columns.Add("No.");
            dtParent_Child_Item.Columns.Add("Unit of Measure");
            dtParent_Child_Item.Columns.Add(new DataColumn("Quantity Total") { DataType = typeof(double) });
            dtParent_Child_Item.Columns.Add(new DataColumn("Quantity in Position") { DataType = typeof(double) });
            dtParent_Child_Item.Columns.Add(new DataColumn("Available") { DataType = typeof(bool) });
            dtParent_Child_Item.Columns.Add("Priorität Buchstaben");
            dtParent_Child_Item.Columns.Add(new DataColumn("Priorität Zahl") { DataType = typeof(int) });
            dtParent_Child_Item.Columns.Add(new DataColumn("Parent ID") { DataType = typeof(int) });
            dtParent_Child_Item.PrimaryKey = new DataColumn[] { dtParent_Child_Item.Columns["ID"] };
            dtParent_Child_Item.Constraints.Add(new ForeignKeyConstraint(dtParent_Child_Item.Columns["ID"], dtParent_Child_Item.Columns["Parent ID"]));
        }

        public DataRow AddRow(params object[] values) 
        {
            DataRow newRow = dtParent_Child_Item.NewRow();
            for (int i = 0; i < dtParent_Child_Item.Columns.Count - 1 && i < values.Length; i++) 
            {
                newRow[i + 1] = values[i];
            }

            dtParent_Child_Item.Rows.Add(newRow);
            return newRow;
        }
        public DataRow AddChildRow(int ParentID, params object[] values) 
        {
            DataRow child = AddRow(values);
            if (String.IsNullOrEmpty(child["Parent ID"].ToString()))
                child["Parent ID"] = ParentID;
            else
                throw new InvalidOperationException("Too many values in parameter.");
            return child;
        }
        public DataRow AddChildRow(DataRow parent, params object[] values) 
        {
            int parentID = (int)parent["ID"];
            return AddChildRow(parentID, values);
        }

        public DataRow GetRow(int ID) 
        {
            return dtParent_Child_Item.Rows.Find(ID);
        }
        public DataRow[] GetChildren(int ID) 
        {
            return dtParent_Child_Item.Select("[Parent ID] = '" + ID.ToString() + "'");
        }
        public DataRow[] GetChildren(DataRow drParent)
        {
            int ID = (int)drParent["ID"];
            return GetChildren(ID);
        }
        public List<DataRow> GetAllParents(object key)
        {
            DataRow[] children = dtParent_Child_Item.Select("[No.] = '" + key + "'");
            List<DataRow> parents = new List<DataRow>();
            foreach (var child in children) 
            {
                parents.Add(GetRow((int)child["Parent ID"]));
            }
            return parents;
        }
        public DataRow GetFirstElement() 
        {
            return dtParent_Child_Item.Rows[0];
        }
        public DataRow GetRootParent(int ID) 
        {
            DataRow currentRow = GetRow(ID);
            if (String.IsNullOrEmpty(currentRow["Parent ID"].ToString()))
                return currentRow;
            else 
            {
                DataRow parentRow = GetRow((int)currentRow["Parent ID"]);
                return GetRootParent((int)parentRow["ID"]);
            }
        }
        public DataRow[] FindByNo(object no) 
        {
            return dtParent_Child_Item.Select("[No.] = '" + no + "'");
        }

        /// <summary>
        /// Gets the total required quantity of an item that it contained by the specified parent
        /// </summary>
        /// <param name="Quantity">The quantity, that the parent requires the item. E.g.: Item 1 -> 3 x Item 2 -> 2x3 x Item 3 (Parent) -> 2x3xQuantity x Item4</param>
        /// <param name="ParentID"></param>
        /// <returns></returns>
        public double GetTotalRequiredQuantity(double Quantity, int ParentID) 
        {
            DataRow Parent = GetRow(ParentID);
            if (String.IsNullOrEmpty(Parent["Quantity Total"].ToString()))
                return Quantity;
            else
            {
                return Quantity * (double)Parent["Quantity Total"];
            }
        }

        public bool SaveData(string Path) 
        {
            try
            {
                dtParent_Child_Item.WriteXml(Path, XmlWriteMode.WriteSchema);
                return true;
            }
            catch (Exception ex) 
            {
                Helper.LogFileWrite(ex.ToString());
            }
            return false;
        }
        public bool LoadData(string Path)
        {
            dtParent_Child_Item = new DataTable("Parent_Child_Item");
            try
            {
                dtParent_Child_Item.ReadXml(Path);
                return true;
            }
            catch (Exception ex)
            {
                Helper.LogFileWrite(ex.ToString());
            }
            return false;
        }


        public void SetAvailability(object No, double? Inventory) 
        {
            //specified object is a salesheader; the status of a salesheader depends on the status of it's child items
            if (Inventory == null)
            {
                DataRow[] currentRows = dtParent_Child_Item.Select("[No.] = '" + No + "'");
                foreach (DataRow currentRow in currentRows)
                {
                    bool Available = true;
                    DataRow[] childRows = GetChildren((int)currentRow["ID"]);
                    foreach (DataRow childRow in childRows)
                    {
                        Available = Available & (bool)childRow["Available"];
                        if (!Available)
                            break;
                    }
                }
            }
            //specified object is an item
            else 
            {
                DataRow[] items = dtParent_Child_Item.Select("[No.] = '" + No + "'");
                foreach (DataRow item in items)
                {
                    item["Available"] = (double)Inventory >= (double)item["Quantity Total"];
                }
            }

        }
    }
}
