using System.Collections.Generic;
using System.Data;

namespace Nat.Web.Tools
{
    public static class ClearTable
    {
        public static void Clear(DataSet dataSet, Dictionary<string, ClearType> listForClear)
        {
            if (dataSet == null) return;
            foreach (KeyValuePair<string, ClearType> pair in listForClear)
            {
                DataTable table = dataSet.Tables[pair.Key];
                switch (pair.Value)
                {
                    case ClearType.All:
                        table.Clear();
                        break;
                    case ClearType.ChildsNotExist:
                        for (int i = table.Rows.Count - 1; i > -1; i--)
                        {
                            DataRow row = table.Rows[i];
                            if (!ExistsChildRows(row, table.ChildRelations))
                                table.Rows.Remove(row);
                        }
                        break;
                    case ClearType.ParentNotExist:
                        for (int i = table.Rows.Count - 1; i > -1; i--)
                        {
                            DataRow row = table.Rows[i];
                            if (!ExistsParentRows(row, table.ParentRelations))
                                table.Rows.Remove(row);
                        }
                        break;
                    default:
                        break;
                }
            }
        }

        private static bool ExistsChildRows(DataRow row, DataRelationCollection relations)
        {
            DataRow[] rows;
            foreach (DataRelation dataRelation in relations)
            {
                rows = row.GetChildRows(dataRelation);
                if (rows != null && rows.Length > 0)
                    return true;
            }
            return false;
        }

        private static bool ExistsParentRows(DataRow row, DataRelationCollection relations)
        {
            DataRow[] rows;
            foreach (DataRelation dataRelation in relations)
            {
                rows = row.GetParentRows(dataRelation);
                if (rows != null && rows.Length > 0)
                    return true;
            }
            return false;
        }
    }
}