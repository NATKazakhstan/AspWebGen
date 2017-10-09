using System.Web.UI;

namespace Nat.Web.Controls.GenerationClasses.BaseJournal
{
    public class AggregateColumn<TRow, TCrossTableKey, TCrossTable, TAggregateRow> : Column<TRow, TAggregateRow>
        where TRow : BaseRow
        where TCrossTableKey : struct
        where TCrossTable : class
        where TAggregateRow : class, new()
    {
        private BaseJournalCrossTable<TRow> _crossTable;

        public BaseJournalCrossTable<TRow, TCrossTableKey, TCrossTable, TAggregateRow> CrossTable
        {
            get { return _crossTable as BaseJournalCrossTable<TRow, TCrossTableKey, TCrossTable, TAggregateRow>; }
            set
            {
                BaseJournalCrossTable = value;
                _crossTable = value;
            }
        }

        public BaseJournalCrossTable<TRow> BaseCrossTable
        {
            get { return _crossTable; }
            set
            {
                BaseJournalCrossTable = value;
                _crossTable = value;
            }
        }

        public TRow CurrentRow { get; protected set; }

        protected virtual TAggregateRow GetAggregateRow(RenderContext context, TRow row)
        {
            CurrentRow = row;
            if (row == null) return null;
            if (CrossTable != null)
            {
                context.LoadData(CrossTable);
                if (CrossTable.RowValues.ContainsKey(row))
                    return CrossTable.RowValues[row];
                if (CrossTable.CurrentValuesRows != null
                    && CrossTable.CurrentValuesRows.ContainsKey(row)
                    && CrossTable.CurrentValuesRows[row].Count > RowIndex)
                {
                    var key = CrossTable.CurrentValuesRows[row][RowIndex];
                    return CrossTable.CurrentValues[row][key];
                }
                if (CrossTable.RowGroupedValues.ContainsKey(row))
                {
                    var rowValues = CrossTable.RowGroupedValues[row];
                    if (rowValues.Count > RowIndex)
                        return rowValues[RowIndex];
                }
            }
            else if (context != null)
            {
                context.LoadData(BaseCrossTable);
                return (TAggregateRow)BaseCrossTable.GetDataItem(context);
            }
            //Debug.Fail("RowIndex больше чем записей AggregateColumn");
            return new TAggregateRow();
        }

        public override int GetRowsCount(RenderContext context)
        {
            if (context.GroupValues != null)
                return base.GetRowsCount(context);
            
            int count;
            if (ExecuteGetRowsCount(context, out count))
                return count;

            var row = context.GetDataRow<TRow>();
            if (CrossTable != null)
            {
                context.LoadData(CrossTable);
                if (CrossTable.RowValues.ContainsKey(row)) return 1;
                if (CrossTable.CurrentValuesRows != null && CrossTable.CurrentValuesRows.ContainsKey(row))
                    return CrossTable.CurrentValuesRows[row].Count;
                return CrossTable.RowGroupedValues[row].Count;
            }

            if (IsCrossColumn)
                return BaseCrossTable.GetRowsCount(context);
            return BaseCrossTable.GetRowsCount(context);
        }

        public override object GetValue(object row, object item, string crossColumnId, object[] groupValues)
        {
            if (row == null && _getValueContext != null)
                row = _getValueContext.GetDataRow<TRow>();
            return base.GetValue(GetAggregateRow(_getValueContext, (TRow)row), item, crossColumnId, groupValues);
        }

        public override object GetValue(RenderContext context)
        {
            _getValueContext = context;
            try
            {
                if (context.AggregateDataRow == null)
                    context.AggregateDataRow = GetAggregateRow(context, context.GetDataRow<TRow>());
                return base.GetValue(context);
            }
            finally
            {
                _getValueContext = null;
            }
        }

        private RenderContext _getValueContext;

        public override object GetValue(RenderContext context, bool allowGetGroupValue)
        {
            _getValueContext = context;
            try
            {
                if (context.AggregateDataRow == null)
                    context.AggregateDataRow = GetAggregateRow(context, context.GetDataRow<TRow>());
                return base.GetValue(context, allowGetGroupValue);
            }
            finally
            {
                _getValueContext = null;
            }
        }

        private RenderContext _getNameContext;

        public override string GetName(object row, object item, string crossColumnId, object[] groupValues)
        {
            if (row == null && _getNameContext != null)
                row = _getNameContext.GetDataRow<TRow>();
            return base.GetName(GetAggregateRow(_getNameContext, (TRow)row), item, crossColumnId, groupValues);
        }

        public override string GetName(RenderContext context)
        {
            _getNameContext = context;
            try
            {
                if (context.AggregateDataRow == null)
                    context.AggregateDataRow = GetAggregateRow(context, context.GetDataRow<TRow>());
                return base.GetName(context);
            }
            finally
            {
                _getNameContext = null;
            }
        }

        public override void GetContent(object row, object item, HtmlTextWriter writer, string crossColumnId, object[] groupValues)
        {
            base.GetContent(GetAggregateRow(null, (TRow)row), item, writer, crossColumnId, groupValues);
        }

        public override string GetCustomHyperLinkParameters(object row, object item, string crossColumnId, object[] groupValues)
        {
            return base.GetCustomHyperLinkParameters(GetAggregateRow(null, (TRow)row), item, crossColumnId, groupValues);
        }

        public override bool ShowAsTemplate(RenderContext context)
        {
            if (context.AggregateDataRow == null)
                context.AggregateDataRow = GetAggregateRow(context, context.GetDataRow<TRow>());
            return base.ShowAsTemplate(context);
        }
    }
}
