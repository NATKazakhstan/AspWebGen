using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;

namespace SyncDbmlByScript
{
    [Serializable]
    public class SyncTable : BaseSync
    {
        XElement table;

        protected override bool Execute()
        {
            table = GetTable(TableName);
            if (table == null && !MustHave) return false;
            if (table == null)
                table = AddTable(TableName, TypeName, CollectionName);
            var tableType = GetTableType(TableName);
            ActiveObject = TableName;
            var primaryKeys = PrimaryKeys.Split(',');
            var columns = Columns.Split(',');
            foreach (var columnName in columns)
            {
                ActiveObject = TableName + "." + columnName;
                var column = EnsureExistsColumn(table, TableName + "." + columnName);
                if (column != null)
                    SetAttributeValue(column, "IsPrimaryKey", primaryKeys.Contains(columnName) ? "true" : null);
            }
            int skipedIndexes = 0;
            for (int i = 0; i < columns.Length; i++)
            {
                var column = GetColumn(table, columns[i]);
                var currentPos = (GetColumns(tableType).Select((e, index) => new { e, index }).Where(e => e.e == column).Select(e => (int?)e.index).FirstOrDefault() ?? -1) - skipedIndexes;
                if (currentPos > -1)
                {
                    if (currentPos != i)
                    {
                        Log(string.Format("Move Position of Column '{0}' from {1} to {2}", columns[i], currentPos, i));
                        XElement prevColumn = null;
                        column.Remove();
                        for (int j = i - 1; j >= 0 && prevColumn == null; j--)
                            prevColumn = GetColumn(table, columns[j]);
                        if (prevColumn != null)
                            prevColumn.AddAfterSelf(column);
                        else
                            tableType.AddFirst(column);
                    }
                }
                else
                    skipedIndexes++;
            }
            if (skipedIndexes == 0)
            {
                foreach (var column in GetColumns(tableType).ToArray())
                {
                    var attr = column.Attribute("Name");
                    var name = attr != null ? attr.Value : null;
                    if (!columns.Contains(name))
                    {
                        Log("Remove column '{0}'", name);
                        column.Remove();
                        //todo: нужно еще удалять связи по колонки GetAssociations(TableName, name);
                    }
                }
            }
            return true;
        }

        protected override bool Validate()
        {
            if (string.IsNullOrEmpty(TableName))
                Error("Property 'TableName' is not set");

            if (string.IsNullOrEmpty(TypeName))
                Error("Property 'TypeName' is not set");

            if (string.IsNullOrEmpty(CollectionName))
                Error("Property 'CollectionName' is not set");

            if (string.IsNullOrEmpty(Columns))
                Error("Property 'Columns' is not set");

            if (string.IsNullOrEmpty(PrimaryKeys))
                Error("Property 'PrimaryKeys' is not set");
            return true;
        }

        public string TableName { get; set; }
        public string TypeName { get; set; }
        public string CollectionName { get; set; }
        public string PrimaryKeys { get; set; }
        public string Columns { get; set; }

        public override string GetName()
        {
            if (!string.IsNullOrEmpty(TableName))
                return TableName;
            return base.GetName();
        }

        protected override XElement GetObject()
        {
            return table;
        }

        public override bool IsChangeTables(IDictionary<string, string> tables)
        {
            return tables.ContainsKey(TableName);
        }
    }
}
