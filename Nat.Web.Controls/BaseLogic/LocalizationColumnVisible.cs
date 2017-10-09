/*
* Created by: Sergey V. Shpakovskiy
* Created: 2012.09.03
* Copyright © JSC NAT Kazakhstan 2012
*/

namespace Nat.Web.Controls.BaseLogic
{
    using Nat.Web.Controls.GenerationClasses;
    using Nat.Web.Tools;

    public class LocalizationColumnVisible : BaseJournalLogic
    {
        public override void InitColumns(BaseGridColumns gridColumns)
        {
            foreach (var column in gridColumns.Columns)
                if (column.ColumnName.EndsWith("Ru") || column.ColumnName.StartsWith("nameRu"))
                    if (LocalizationHelper.IsCultureKZ)
                    {
                        column.Visible = false;
                        column.ExportVisible = false;
                    }
                    else
                    {
                        column.Header = RemoveStringWithinBrackets(column.Header).Trim();
                    }
                else if (column.ColumnName.EndsWith("Kz") || column.ColumnName.StartsWith("nameKz"))
                    if (!LocalizationHelper.IsCultureKZ)
                    {
                        column.Visible = false;
                        column.ExportVisible = false;
                    }
                    else
                    {
                        column.Header = RemoveStringWithinBrackets(column.Header).Trim();
                    }
        }

        private static string RemoveStringWithinBrackets(string value)
        {
            if (string.IsNullOrEmpty(value)) return value;
            var count = 0;
            var index = value.IndexOf('(');
            var startIndex = index;
            if (index > -1) count++;
            while (count > 0 && index != -1)
            {
                var indexOpen = value.IndexOf('(', index + 1);
                var indexClose = value.IndexOf(')', index + 1);
                if (indexOpen > -1 && (indexOpen < indexClose || indexClose == -1))
                {
                    count++;
                    index = indexOpen;
                    continue;
                }
                if (indexClose > -1)
                {
                    count--;
                    if (count == 0)
                    {
                        value = value.Remove(startIndex, indexClose - startIndex + 1).Replace("  ", " ");
                        return RemoveStringWithinBrackets(value);
                    }
                    index = indexClose;
                    continue;
                }
                break;
            }
            if (count > 0)
                return value.Substring(0, startIndex).TrimEnd();
            return value;
        }
    }
}