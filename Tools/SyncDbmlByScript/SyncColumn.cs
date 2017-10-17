/*
 * Created by: Sergey V. Shpakovskiy
 * Created: 4 θώνÿ 2009 γ.
 * Copyright © JSC New Age Technologies 2009
 */

using System;
using System.Xml.Linq;
using System.Collections.Generic;

namespace SyncDbmlByScript
{
    [Serializable]
    public class SyncColumn : BaseSync
    {
        XElement table;
        XElement column;

        protected override bool Execute()
        {//*
            ActiveObject = GetName();
            if (column == null)
                column = AddColumn(table);

            SetAttributeValue(column, "Name", ColumnName);
            if (ColumnName.IndexOfAny(new[] { '#', '*', '.' }) > -1)
                SetAttributeValue(column, "Member", ColumnName.Replace("#", "_").Replace(".", "_").Replace("*", "_"));
            else
                SetAttributeValue(column, "Member", null);
            if (!string.IsNullOrEmpty(DataType))
            {
                SetAttributeValue(column, "Type", DataType);
                if (DataType == "timestamp")
                    SetAttributeValue(column, "IsVersion", "true");
                else
                    SetAttributeValue(column, "IsVersion", null);

                if (DbType.Equals("varbinary(max)", StringComparison.OrdinalIgnoreCase))
                {
                    SetAttributeValue(column, "IsDelayLoaded", "true");
                    SetAttributeValue(column, "UpdateCheck", "Never");
                }

                if (DbType.Equals("xml", StringComparison.OrdinalIgnoreCase))
                {
                    SetAttributeValue(column, "UpdateCheck", "Never");
                }
            }
            if (!string.IsNullOrEmpty(DbType))
                SetAttributeValue(column, "DbType", DbType);
            SetAttributeValue(column, "CanBeNull", (!Mandatory).ToString().ToLower());
            if (ComputeInDataBase)
                SetAttributeValue(column, "IsDbGenerated", "true");
            else
                SetAttributeValue(column, "IsDbGenerated", null);
//            SetAttributeValue(column, "IsPrimaryKey", IsPrimaryKey.ToString());*/
            return true;
        }

        protected override bool Validate()
        {//*
            if(string.IsNullOrEmpty(ColumnName))
                Error("Property 'ColumnName' is not set");
            if (string.IsNullOrEmpty(TableName))
                Error("Property 'TableName' is not set");
            table = GetTable(TableName);
            if (table == null)
            {
                Error("Not found Table '{0}'", TableName);
                return false;
            }

            column = GetColumn(table, ColumnName);
            if (column == null && (string.IsNullOrEmpty(DataType) || string.IsNullOrEmpty(DbType)))
                Error("Can not add column, not setted DataType");//*/
            return true;
        }

        public string TableName { get; set; }
        public string ColumnName { get; set; }
        public string DataType { get; set; }
        public string DbType { get; set; }
        public bool Mandatory { get; set; }
        public bool ComputeInDataBase { get; set; }
//        public bool IsPrimaryKey { get; set; }

        public override string GetName()
        {
            if (!string.IsNullOrEmpty(TableName) && !string.IsNullOrEmpty(ColumnName))
                return TableName + "." + ColumnName;
            return base.GetName();
        }

        protected override XElement GetObject()
        {
            return column;
        }

        public override bool IsChangeTables(IDictionary<string, string> tables)
        {
            return tables.ContainsKey(TableName);
        }
    }
}