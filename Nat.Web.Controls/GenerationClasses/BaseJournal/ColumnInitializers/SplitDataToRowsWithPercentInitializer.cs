namespace Nat.Web.Controls.GenerationClasses.BaseJournal.ColumnInitializers
{
    using System;
    using System.Linq;

    public class SplitDataToRowsWithPercentInitializer<TSource> : SplitDataToRowsInitializer<TSource>
        where TSource : class
    {
        private readonly BaseColumn column;

        private readonly Func<TSource, object> quantityHandler;

        private readonly Func<TSource, object> valueHandler;

        private const decimal Percent100 = 100;

        public SplitDataToRowsWithPercentInitializer(
            BaseColumn column,
            Func<TSource, object> quantity,
            Func<TSource, object> value)
            : base(column, new[] { quantity, value })
        {
            this.column = column;
            this.quantityHandler = quantity;
            this.valueHandler = value;
        }

        public override void Initialize()
        {
            GetTotalValues = new Func<RenderContext, object>[]
                                 {
                                     GetTotalQuantity,
                                     GetTotalPersons,
                                     GetTotalPercent,
                                 };
            base.Initialize();

            column.AggregateFormat = "{0:0.##}";
        }

        private object GetTotalQuantity(RenderContext context)
        {
            var originalRowIndex = context.RowIndex;
            context.RowIndex = 0;
            var value = context.Column.GetAggregateValue(context);
            context.RowIndex = originalRowIndex;
            return value;
        }

        private object GetTotalPersons(RenderContext context)
        {
            var originalRowIndex = context.RowIndex;
            context.RowIndex = 1;
            var value = context.Column.GetAggregateValue(context);
            context.RowIndex = originalRowIndex;
            return value;
        }

        private object GetTotalPercent(RenderContext context)
        {
            return this.GetPercent(Convert.ToDecimal(this.GetTotalQuantity(context)), Convert.ToDecimal(this.GetTotalPersons(context)));
        }

        protected override int GetRowsCount(RenderContext context)
        {
            return 3;
        }

        protected override object GetValue(RenderContext context)
        {
            if (context.RowIndex == 2)
            {
                column.GetValue(context);
                var row = context.GetDataRow<TSource>();
                var quantity = Convert.ToDecimal(quantityHandler(row));
                var value = Convert.ToDecimal(valueHandler(row));
                return this.GetPercent(quantity, value);
            }

            return base.GetValue(context);
        }

        protected virtual object GetPercent(decimal quantity, decimal value)
        {
            if (quantity == 0)
            {
                return null;
            }
            return value * Percent100 / quantity;
        }
    }

    public class SplitDataToRowsWithPercentAndOtherGroupInitializer<TSource> :
        SplitDataToRowsWithPercentInitializer<TSource>
        where TSource : class
    {
        private readonly BaseColumn quantityTotalColumn;

        private readonly BaseColumn countTotalColumn;

        public SplitDataToRowsWithPercentAndOtherGroupInitializer(
            BaseColumn column,
            BaseColumn quantityTotalColumn,
            BaseColumn countTotalColumn,
            Func<TSource, object> quantity,
            Func<TSource, object> value)
            : base(column, quantity, value)
        {
            this.quantityTotalColumn = quantityTotalColumn;
            this.countTotalColumn = countTotalColumn;
        }

        protected override object GetValue(RenderContext context)
        {
            if (context.Column.BaseCrossColumnDataSource != null
                && context.CrossDataItemKey.Count > 0 && context.CrossDataItemKey.First().Value.Value == null)
            {
                RenderContext colContext;
                var sum = context.Column.BaseCrossColumnDataSource.GetCrossColumnNames()
                    .Select(columnName => context.OtherColumns[columnName])
                    .Where(r => r.CrossDataItemKey.First().Value.Value != null)
                    .Sum(r => Convert.ToDecimal(r.Column.GetValueHandler(r)));

                if (context.RowIndex == 0)
                {
                    context.Column.DependenceColumns(context, this.quantityTotalColumn);
                    return Convert.ToDecimal(this.quantityTotalColumn.GetValueByContext(context)) - sum;
                }

                if (context.RowIndex == 1)
                {
                    context.Column.DependenceColumns(context, this.countTotalColumn);
                    colContext = context.OtherColumns[this.countTotalColumn.ColumnName];
                    colContext.RowIndex = 1;
                    return Convert.ToDecimal(this.countTotalColumn.GetValue(colContext)) - sum;
                }

                context.Column.DependenceColumns(context, this.quantityTotalColumn);
                colContext = context.OtherColumns[this.quantityTotalColumn.ColumnName];
                colContext.RowIndex = 0;
                var quantity = Convert.ToDecimal(this.quantityTotalColumn.GetValue(colContext)) - sum;
                colContext.RowIndex = 2;

                colContext = context.OtherColumns[this.countTotalColumn.ColumnName];
                context.Column.DependenceColumns(context, this.countTotalColumn);
                colContext.RowIndex = 1;
                var value = Convert.ToDecimal(this.countTotalColumn.GetValue(colContext)) - sum;
                colContext.RowIndex = 2;
                return this.GetPercent(quantity, value);
            }

            return base.GetValue(context);
        }
    }
}