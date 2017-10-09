namespace Nat.Web.Controls.GenerationClasses.BaseJournal.ColumnInitializers
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    public class JoinColumnInitializer
    {
        private readonly BaseColumn resultColumn;

        private readonly BaseColumn[] columns;

        public JoinColumnInitializer(BaseColumn resultColumn, params BaseColumn[] columns)
        {
            this.resultColumn = resultColumn;
            this.columns = columns;
            JoinSeparator = ",\r\n";
        }

        public Func<RenderContext, string, string> FormatNameHandler;

        public string JoinSeparator { get; set; }

        public void Initialize()
        {
            resultColumn.ColumnType = ColumnType.Other;
            resultColumn.GetRowsCountHandler = GetRowsCount;
            resultColumn.GetValueHandler = GetValue;
            resultColumn.GetNameHandler = GetName;
        }

        private string GetName(RenderContext context)
        {
            return (string)resultColumn.GetValue(context);
        }

        private object GetValue(RenderContext context)
        {
            resultColumn.DependenceColumns(context, columns);
            var result = GetJoinOf(context, JoinSeparator, columns);
            if (FormatNameHandler != null)
                result = FormatNameHandler(context, result);
            return result;
        }

        public static string GetJoinOf(RenderContext context, string joinSeparator, params BaseColumn[] columns)
        {
            var list = columns.SelectMany(c => GetJoinOf(context, c)).Where(r => !string.IsNullOrEmpty(r)).ToArray();
            if (list.Length == 0)
                return null;
            return list.Aggregate((r1, r2) => r1 + joinSeparator + r2);
        }

        private static IEnumerable<string> GetJoinOf(RenderContext context, BaseColumn c)
        {
            if (!c.IsCrossColumn)
                return new[] { c.GetNameByContext(context) };

            var crossColumnNames = c.BaseCrossColumnDataSource.GetCrossColumnNames().ToList();
            if (crossColumnNames.Count == 0)
                return new string[0];

            return crossColumnNames.Select(n => c.GetName(context.OtherColumns[n]));
        }

        public static string GetJoinOf(RenderContext context, string joinSeparator, int rowIndex, params BaseColumn[] columns)
        {
            var list = columns.SelectMany(c => GetJoinOf(context, rowIndex, c)).Where(r => !string.IsNullOrEmpty(r)).ToArray();
            if (list.Length == 0)
                return null;
            return list.Aggregate((r1, r2) => r1 + joinSeparator + r2);
        }

        private static IEnumerable<string> GetJoinOf(RenderContext context, int rowIndex, BaseColumn c)
        {
            if (!c.IsCrossColumn)
                return new[] { GetName(c, context.OtherColumns[c.ColumnName], rowIndex) };

            var crossColumnNames = c.BaseCrossColumnDataSource.GetCrossColumnNames().ToList();
            if (crossColumnNames.Count == 0)
                return new string[0];

            return crossColumnNames.Select(n => GetName(c, context.OtherColumns[n], rowIndex));
        }

        private static string GetName(BaseColumn column, RenderContext context, int rowIndex)
        {
            var currentRowIndex = context.RowIndex;
            context.RowIndex = rowIndex;
            var value = column.GetName(context);
            context.RowIndex = currentRowIndex;
            return value;
        }

        private int GetRowsCount(RenderContext context)
        {
            return columns.Select(c => GetRowsCount(context, c)).Max();
        }

        private static int GetRowsCount(RenderContext context, BaseColumn c)
        {
            if (!c.IsCrossColumn)
                return c.GetRowsCount(context.OtherColumns[c.ColumnName]);

            var crossColumnNames = c.BaseCrossColumnDataSource.GetCrossColumnNames().ToList();
            if (crossColumnNames.Count == 0)
                return 0;

            return crossColumnNames.Max(n => c.GetRowsCount(context.OtherColumns[n]));
        }
    }
}