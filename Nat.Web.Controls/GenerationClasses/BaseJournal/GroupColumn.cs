using System;
namespace Nat.Web.Controls.GenerationClasses.BaseJournal
{
    [Serializable]
    public class GroupColumn
    {
        public string ColumnName;
        public GroupType GroupType;

        public override int GetHashCode()
        {
            return ColumnName.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            var g = obj as GroupColumn;
            if (g != null && ColumnName != null)
                return ColumnName.Equals(g.ColumnName);
            return base.Equals(obj);
        }

        public override string ToString()
        {
            return "$" + (int)GroupType + ColumnName;
        }

        public static implicit operator string (GroupColumn g)
        {
            return g.ColumnName;
        }

        public static implicit operator GroupColumn(string columnName)
        {
            if (columnName.StartsWith("$"))
                return new GroupColumn
                           {
                               ColumnName = columnName.Substring(2),
                               GroupType = (GroupType)Convert.ToInt32(columnName.Substring(1, 1)),
                           };
            return new GroupColumn {ColumnName = columnName,};
        }
    }
}