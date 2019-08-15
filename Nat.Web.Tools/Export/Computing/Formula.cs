namespace Nat.Web.Tools.Export.Computing
{
    using System.Collections.Generic;
    using System.Text;

    public class Formula : FormulaItem
    {
        public Formula()
        {
            FormulaItems = new List<FormulaItem>();
        }

        public Formula(params FormulaItem[] formulaItems)
        {
            FormulaItems = new List<FormulaItem>(formulaItems);
        }

        public IList<FormulaItem> FormulaItems { get; }

        public string FunctionName { get; set; }

        public string ToString(int column, int row)
        {
            var sb = new StringBuilder();
            ToString(sb, column, row);
            return sb.ToString();
        }

        public override void ToString(StringBuilder sb, int column, int row)
        {
            if (string.IsNullOrEmpty(FunctionName))
            {
                foreach (var formulaItem in FormulaItems)
                    formulaItem.ToString(sb, column, row);
            }
            else
            {
                sb.Append(FunctionName).Append("(");
                for (var index = 0; index < FormulaItems.Count; index++)
                {
                    if (index > 0) sb.Append(",");
                    FormulaItems[index].ToString(sb, column, row);
                }
                sb.Append(")");
            }
        }
    }
}