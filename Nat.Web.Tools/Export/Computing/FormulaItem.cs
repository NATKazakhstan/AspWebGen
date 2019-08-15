namespace Nat.Web.Tools.Export.Computing
{
    using System.Text;

    public class FormulaItem
    {
        protected FormulaItem()
        {
        }

        public FormulaItem(string s)
        {
            String = s;
        }

        public FormulaItem(FormulaCell formulaCell)
        {
            FormulaCell = formulaCell;
        }

        public FormulaItem(int? columnMove, int? rowMove)
        {
            FormulaCell = new FormulaCell(columnMove, rowMove);
        }
        
        public string String { get; }
        public FormulaCell FormulaCell { get; }

        public virtual void ToString(StringBuilder sb, int column, int row)
        {
            if (FormulaCell != null)
                FormulaCell.ToString(sb, column, row);
            else
                sb.Append(String);
        }

        public static implicit operator FormulaItem(string value)
        {
            return new FormulaItem(value);
        }

        public static implicit operator FormulaItem(int value)
        {
            return new FormulaItem(value, 0);
        }
    }
}