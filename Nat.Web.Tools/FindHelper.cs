using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Text.RegularExpressions;
using Nat.Web.Tools.Properties;

namespace Nat.Web.Tools
{
    public static class FindHelper
    {
        private static readonly Regex sortItems =
            new Regex(@"((?<item>[\w\[\] ]+)\s+(ASC|DESC))|(?<item>[\w\[\] ]+)",
                      RegexOptions.Compiled | RegexOptions.IgnoreCase);

        public static List<string> GetSortItems(string sort)
        {
            List<string> list = new List<string>();
            MatchCollection matches = sortItems.Matches(sort);
            foreach (Match match in matches)
            {
                if (match.Success)
                {
                    string value = match.Groups["item"].Value;
                    while (value[0] == '[' && value[value.Length - 1] == ']')
                        value = value.Substring(1, value.Length - 2);
                    list.Add(value.ToLower());
                }
            }
            return list;
        }

        public static int Find(DataRow row, DataView view, int index)
        {
            if(index > -1 && index < view.Count && view[index].Row == row)
                return index;
            return Find(row, view);
        }

        public static int Find(DataRow row, DataView view)
        {
            if (row == null) return -1;
            List<string> items = GetSortItems(view.Sort);
            bool addedItem = false;
            DataView findInView = view;
            if (view.Table.PrimaryKey.Length > 0)
            {
                string sort = view.Sort;
                for (int i = 0; i < view.Table.PrimaryKey.Length; i++)
                {
                    string column = view.Table.PrimaryKey[i].ColumnName.ToLower();
                    if (!items.Contains(column))
                    {
                        if (!string.IsNullOrEmpty(sort)) sort += ",";
                        sort += string.Format("[{0}]", column);
                        items.Add(column);
                        addedItem = true;
                    }
                }
                if(addedItem)
                    //findInView = new DataView(view.Table, view.RowFilter, sort, DataViewRowState.CurrentRows);
                	findInView.Sort = sort;
            }
            if (items.Count == 0)
                throw new NotSupportedException(
                    string.Format("Не поддерживается поиск по таблице '{0}' без сортировки по умолчанию", view.Table));
            object[] values = new object[items.Count];
            for (int i = 0; i < items.Count; i++)
                values[i] = row[items[i]];
            return findInView.Find(values);
        }

        public static DataRow FindRow(IDictionary findKeys, DataTable table)
        {
            DataRow row = FindRowByPrimaryKey(findKeys, table);
            if (row == null)
            {
                DataRowView rowView = FindRow(findKeys, new DataView(table));
                if (rowView != null) row = rowView.Row;
            }
            return row;
        }

        public static DataRowView FindRow(IDictionary findKeys, DataView view)
        {
            string sort = "";
            foreach (DictionaryEntry entry in findKeys)
                sort += entry.Key + ", ";
            if (sort.Length > 0) sort = sort.Substring(0, sort.Length - 2);
            view.Sort = sort;
            int index = view.Find(GetValues(findKeys, findKeys));
            if (index > -1) return view[index];
            return null;
        }

        private static object[] GetValues(IDictionary keys, IDictionary value)
        {
            object[] values = new object[keys.Count];
            int i = 0;
            foreach (DictionaryEntry entry in keys)
            {
                if (value.Contains(entry.Key))
                    values[i++] = value[entry.Key];
            }
            return values;
        }

        public static DataRow FindRowByPrimaryKey(IDictionary dictionary2, DataTable table)
        {
            if (table.PrimaryKey.Length == dictionary2.Count && dictionary2.Count > 0)
            {
                object[] values = new object[dictionary2.Count];
                int i = 0;
                foreach (DataColumn column in table.PrimaryKey)
                {
                    if (dictionary2.Contains(column.ColumnName))
                        values[i++] = dictionary2[column.ColumnName];
                    else i = -1;
                }
                if (i > -1) return table.Rows.Find(values);
            }
            return null;
        }

        public static string GetContentFieldName(string nameRu, string nameKz)
        {
            if (CultureInfo.CurrentUICulture.Name.Equals("kk-kz", StringComparison.OrdinalIgnoreCase))
                return nameKz;
            return nameRu;
        }

        public static string GetStringValue(DataTable table, object value)
        {
            return GetStringValue(table, value, "nameRu", "nameKz");
        }

        public static string GetStringOfBoolValue(object value)
        {
            if (value == null || value == DBNull.Value) return "";
            bool b = Convert.ToBoolean(value);
            return b ? Resources.SYes : Resources.SNo;
        }

        public static string GetStringValue(DataTable table, object value, string columnNameRu, string columnNameKz)
        {
            DataRow row = table.Rows.Find(value);
            if (row == null) return null;
            string columnName = GetContentFieldName(columnNameRu, columnNameKz);
            if (row[columnName] == DBNull.Value) return null;
            return row[columnName].ToString();
        }

    }
}