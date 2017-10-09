namespace Nat.Web.Controls.GenerationClasses.BaseJournal.ColumnInitializers
{
    using System;

    public class SplitDataToRowsInitializer
    {
        private readonly BaseColumn column;

        private readonly Func<RenderContext, object>[] getValues;

        public SplitDataToRowsInitializer(BaseColumn column, params Func<RenderContext, object>[] getValues)
        {
            this.column = column;
            this.getValues = getValues;
        }

        public Func<RenderContext, object>[] GetTotalValues { get; set; }

        public Func<RenderContext, object, string>[] GetNamesHandlers { get; set; }

        public virtual void Initialize()
        {
            column.GetValueHandler = GetValue;
            column.GetNameHandler = GetName;
            if (GetTotalValues != null)
            {
                column.GetTotalValueHandler = GetTotalValue;
                column.GetTotalNameHandler = GetTotalName;
            }

            column.GetRowsCountHandler = GetRowsCount;
            column.AggregateGroupedByRowIndex = true;
        }

        protected virtual string GetTotalName(RenderContext context)
        {
            return string.Format(
                string.IsNullOrEmpty(this.column.AggregateFormat) ? "{0}" : this.column.AggregateFormat,
                this.GetTotalValue(context));
        }

        protected virtual object GetTotalValue(RenderContext context)
        {
            return GetTotalValues[context.RowIndex](context);
        }

        protected virtual int GetRowsCount(RenderContext context)
        {
            return getValues.Length;
        }

        protected virtual string GetName(RenderContext context)
        {
            var value = column.GetValue(context);
            if (GetNamesHandlers == null || GetNamesHandlers.Length <= context.RowIndex
                || GetNamesHandlers[context.RowIndex] == null)
            {
                return string.Format("{0:0.##}", value);
            }

            return GetNamesHandlers[context.RowIndex](context, value);
        }

        protected virtual object GetValue(RenderContext context)
        {
            column.GetValue(context);
            return getValues[context.RowIndex](context);
        }
    }

    public class SplitDataToRowsInitializer<TSource> : SplitDataToRowsInitializer
        where TSource : class
    {
        private readonly BaseColumn column;

        private readonly Func<TSource, object>[] getValues;

        public SplitDataToRowsInitializer(BaseColumn column, params Func<TSource, object>[] getValues)
            : base(column, new Func<RenderContext, object>[getValues.Length])
        {
            this.column = column;
            this.getValues = getValues;
        }

        protected override object GetValue(RenderContext context)
        {
            column.GetValue(context);
            var row = context.GetDataRow<TSource>();
            return getValues[context.RowIndex](row);
        }
    }
}