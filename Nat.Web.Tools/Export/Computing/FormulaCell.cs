namespace Nat.Web.Tools.Export.Computing
{
    using System.Text;

    public class FormulaCell
    {
        public FormulaCell()
        {
        }

        public FormulaCell(int? columnMove, int? rowMove)
        {
            ColumnMove = columnMove;
            RowMove = rowMove;
        }

        public int? Column { get; set; }
        public int? ColumnMove { get; set; }
        public int? Row { get; set; }
        public int? RowMove { get; set; }

        public void ToString(StringBuilder sb, int column, int row)
        {
            if (Column != null)
                sb.Append(GetLaterByInt(Column.Value));
            else if (ColumnMove != null)
                sb.Append(GetLaterByInt(column + ColumnMove.Value));
            else
                throw new FormulaCellException("Не инициализирован Column и ColumnMove, один из них обязателен.");

            if (Row != null)
                sb.Append(Row.Value);
            else if (RowMove != null)
                sb.Append(row + RowMove.Value);
            else
                throw new FormulaCellException("Не инициализирован Row и RowMove, один из них обязателен.");
        }

        public string ToString(int column, int row)
        {
            var str = string.Empty;
            if (Column != null)
                str += GetLaterByInt(Column.Value);
            else if (ColumnMove != null)
                str += GetLaterByInt(column + ColumnMove.Value);
            else
                throw new FormulaCellException("Не инициализирован Column и ColumnMove, один из них обязателен.");

            if (Row != null)
                str += Row.Value;
            else if (RowMove != null)
                str += row + RowMove.Value;
            else
                throw new FormulaCellException("Не инициализирован Row и RowMove, один из них обязателен.");

            return str;
        }
        
        public static string GetLaterByInt(int value)
        {
            if (value == 0) return "A";
            string result = string.Empty;
            int mod = value % 26;
            result = (char)('A' + mod) + result;
            value /= 26;
            while (value > 0)
            {
                mod = value % 26 - 1;
                result = (char)('A' + mod) + result;
                value /= 26;
            }

            return result;
        }
    }
}