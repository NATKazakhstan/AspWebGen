namespace Nat.Web.Controls.GenerationClasses.BaseJournal.ColumnInitializers
{
    using System;

    public class PercentColumnInitializer
    {
        public const decimal Percent100 = 100;

        private readonly BaseColumn fullCountColumn;

        private readonly BaseColumn partOfCountColumn;

        private readonly BaseColumn percentColumn;

        public PercentColumnInitializer(BaseColumn fullCountColumn, BaseColumn partOfCountColumn, BaseColumn percentColumn)
        {
            this.fullCountColumn = fullCountColumn;
            this.partOfCountColumn = partOfCountColumn;
            this.percentColumn = percentColumn;
        }

        public int RoundForDigits { get; set; } = -1;

        public decimal? ValueForDivideOnZero { get; set; }

        public void Initialize(string format = "{0:0.##}")
        {
            percentColumn.Format = format;
            percentColumn.ColumnType = ColumnType.Numeric;
            percentColumn.GetRowsCountHandler = GetRowsCount;
            percentColumn.GetValueHandler = GetPercentValue;
            percentColumn.GetNameHandler = GetPercentValueName;
            percentColumn.GetTotalValueHandler = GetPercentValue;
            percentColumn.GetTotalNameHandler = GetPercentValueName;
        }

        private int GetRowsCount(RenderContext context)
        {
            var count = this.fullCountColumn.GetRowsCount(context.OtherColumns[this.fullCountColumn.ColumnName]);
            return count == 0 ? 1 : count;
        }

        private string GetPercentValueName(RenderContext context)
        {
            return string.Format(this.percentColumn.Format, this.percentColumn.GetValue(context));
        }

        private object GetPercentValue(RenderContext context)
        {
            this.percentColumn.DependenceColumns(context, this.fullCountColumn, this.partOfCountColumn);
            
            var count = Convert.ToDecimal(this.fullCountColumn.GetValueByContext(context));
            if (count == 0) return ValueForDivideOnZero;

            var partOfData = Convert.ToDecimal(this.partOfCountColumn.GetValueByContext(context));

            if (RoundForDigits > 0)
            {
                count = Math.Round(count, RoundForDigits);
                partOfData = Math.Round(partOfData, RoundForDigits);
                return Math.Round(partOfData * Percent100 / count, RoundForDigits);
            }

            return partOfData * Percent100 / count;
        }
    }
}