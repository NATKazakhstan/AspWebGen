namespace Nat.Web.Controls.GenerationClasses.BaseJournal.ColumnInitializers
{
    using System;
    using System.Linq;

    public class TotalColumnInitializer
    {
        private readonly BaseColumn totalColumn;
        private readonly BaseColumn[] columns;

        private Func<decimal[], decimal> customComputeAggregate;
        private Func<decimal[], decimal> customTotalComputeAggregate;
        private Func<RenderContext, BaseColumn, decimal>[] getSumValues;

        private Func<RenderContext, BaseColumn, bool> whereForSum;

        public TotalColumnInitializer(BaseColumn totalColumn, params BaseColumn[] columns)
        {
            this.totalColumn = totalColumn;
            this.columns = columns;
        }

        public void Initialize(Func<RenderContext, BaseColumn, bool> where)
        {
            whereForSum = where;
            Initialize();
        }

        public void Initialize()
        {
            var format = columns[0].Format;
            if (string.IsNullOrEmpty(format))
                format = "{0}";
            totalColumn.Format = format;
            totalColumn.ColumnType = ColumnType.Numeric;
            totalColumn.GetRowsCountHandler = GetRowsCount;
            totalColumn.GetValueHandler = GetValue;
            totalColumn.GetNameHandler = GetName;
        }

        public void InitializeCustomAggregate(Func<decimal[], decimal> compute, Func<RenderContext, BaseColumn, decimal>[] getSumValues)
        {
            Initialize();
            customComputeAggregate = compute;
            totalColumn.GetValueHandler = GetCustomValueAggregate;
            this.getSumValues = getSumValues ?? new Func<RenderContext, BaseColumn, decimal>[columns.Length];
        }

        public void InitializeCustomAggregate(Func<decimal[], decimal> compute, Func<decimal[], decimal> totalCompute, Func<RenderContext, BaseColumn, decimal>[] getSumValues)
        {
            InitializeCustomAggregate(compute, getSumValues);
            customTotalComputeAggregate = totalCompute;
            totalColumn.GetTotalValueHandler = GetCustomTotalValueHandler;
            totalColumn.GetTotalNameHandler = context => string.Format(totalColumn.Format, GetCustomTotalValueHandler(context));
        }

        private object GetCustomTotalValueHandler(RenderContext context)
        {
            totalColumn.DependenceColumns(context, columns);
            var values = columns.Select((column, ind) => getSumValues[ind] != null ? getSumValues[ind](context, column) : GetSumOf(context, column)).ToArray();
            return customTotalComputeAggregate(values);
        }

        private string GetName(RenderContext context)
        {
            return string.Format(totalColumn.Format, totalColumn.GetValue(context));
        }

        private object GetValue(RenderContext context)
        {
            totalColumn.DependenceColumns(context, columns);
            return GetSumOf(context, whereForSum, columns);
        }

        private object GetCustomValueAggregate(RenderContext context)
        {
            totalColumn.DependenceColumns(context, columns);
            var values = columns.Select((column, ind) => getSumValues[ind] != null ? getSumValues[ind](context, column) : GetSumOf(context, column)).ToArray();
            return customComputeAggregate(values);
        }

        public static decimal GetSumOf(RenderContext context, params BaseColumn[] columns)
        {
            return GetSumOf(context, null, columns);
        }

        public static decimal GetSumOf(RenderContext context, Func<RenderContext, BaseColumn, bool> where, params BaseColumn[] columns)
        {
            return columns.Sum(c => GetSumOf(context, c, where));
        }

        private static decimal GetSumOf(RenderContext context, BaseColumn c, Func<RenderContext, BaseColumn, bool> where)
        {
            if (!c.IsCrossColumn)
            {
                if (where == null || where(context, c))
                    return Convert.ToDecimal(c.GetValueByContext(context));
                return 0;
            }

            var contexts = c.BaseCrossColumnDataSource.GetCrossColumnNames().Select(columnName => context.OtherColumns[columnName]);
            var crossColumnNames = where == null
                                       ? contexts.ToList()
                                       : contexts.Where(renderContext => where(renderContext, renderContext.Column)).ToList();

            if (crossColumnNames.Count == 0)
                return 0;

            return crossColumnNames.Sum(renderContext => Convert.ToDecimal(renderContext.Column.GetValue(renderContext)));
        }

        public static decimal GetSumOf(RenderContext context, int rowIndex, params BaseColumn[] columns)
        {
            return columns.Sum(c => GetSumOf(context, rowIndex, c));
        }

        private static decimal GetSumOf(RenderContext context, int rowIndex, BaseColumn c)
        {
            if (!c.IsCrossColumn) 
                return GetValue(c, context.OtherColumns[c.ColumnName], rowIndex);
            
            var crossColumnNames = c.BaseCrossColumnDataSource.GetCrossColumnNames().ToList();
            if (crossColumnNames.Count == 0)
                return 0;
            
            return crossColumnNames.Sum(n => GetValue(c, context.OtherColumns[n], rowIndex));
        }

        private static decimal GetValue(BaseColumn column, RenderContext context, int rowIndex)
        {
            var currentRowIndex = context.RowIndex;
            context.RowIndex = rowIndex;
            var value = column.GetValue(context);
            context.RowIndex = currentRowIndex;
            return Convert.ToDecimal(value);
        }

        private int GetRowsCount(RenderContext context)
        {
            return columns.Select(c => GetRowsCount(context, c, whereForSum)).Max();
        }

        private static int GetRowsCount(RenderContext context, BaseColumn c, Func<RenderContext, BaseColumn, bool> where)
        {
            if (!c.IsCrossColumn)
            {
                if (where == null || where(context, c))
                    return c.GetRowsCount(context.OtherColumns[c.ColumnName]);
                return 0;
            }

            var contexts = c.BaseCrossColumnDataSource.GetCrossColumnNames().Select(columnName => context.OtherColumns[columnName]);
            var crossColumnNames = where == null
                                       ? contexts.ToList()
                                       : contexts.Where(renderContext => where(renderContext, renderContext.Column)).ToList();

            if (crossColumnNames.Count == 0)
                return 0;

            return crossColumnNames.Max(renderContext => renderContext.Column.GetRowsCount(renderContext));
        }
    }
}