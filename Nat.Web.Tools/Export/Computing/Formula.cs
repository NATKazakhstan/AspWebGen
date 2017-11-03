namespace Nat.Web.Tools.Export.Computing
{
    using System.Collections.Generic;
    using System.Text;

    public class Formula
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

        public string ToString(int column, int row)
        {
            var sb = new StringBuilder();
            foreach (var formulaItem in FormulaItems)
                formulaItem.ToString(sb, column, row);
            return sb.ToString();
        }
    }
}