using System.Web.UI;

namespace Nat.Web.Controls.GenerationClasses.BaseJournal
{
    public class Column<TRow, TDataRow> : BaseColumn
        where TRow : BaseRow
        where TDataRow : class
    {
        public GetContent<TDataRow> ColumnContentHandler { get; set; }
        public GetValue<TDataRow> ColumnValueHandler { get; set; }
        public GetName<TDataRow> ColumnNameHandler { get; set; }
        public GetName<TDataRow> CustomHyperLinkParametersHandler { get; set; }
        public GetRowNumber<TRow> RowNumberHandler { get; set; }
        public GetRowsCount<TDataRow> ColumnRowsCountHandler { get; set; }

        public GetContentAggregate ColumnAggregateContentHandler { get; set; }
        public GetValueAggregate ColumnAggregateValueHandler { get; set; }
        public GetNameAggregate ColumnAggregateNameHandler { get; set; }
        public GetNameAggregate CustomAggregateHyperLinkParametersHandler { get; set; }
        public GetRowsCountAggregate ColumnAggregateRowsCountHandler { get; set; }

        public override object GetValue(object row, object item, string crossColumnId, object[] groupValues)
        {
            if (groupValues != null)
            {
                if (ColumnAggregateValueHandler != null)
                    return ColumnAggregateValueHandler(crossColumnId, groupValues);
                return GetGroupValue(crossColumnId, groupValues);
            }
            if (row == null) return null;
            if (ColumnValueHandler != null) return ColumnValueHandler((TDataRow)row);
            return null;
        }

        private bool _executeGetValue;

        public override object GetValue(RenderContext context)
        {
            if (context.GroupValues != null)
            {
                if (ColumnAggregateValueHandler != null)
                    return ColumnAggregateValueHandler(context.CrossColumnId, context.GroupValues);
                return GetGroupValue(context);
            }

            context.LoadData(BaseJournalCrossTable);

            var row = context.GetDataRow<TDataRow>();
            if (row == null) return null;
            if (GetValueHandler != null && !_executeGetValue)
            {
                _executeGetValue = true;
                try
                {
                    return GetValueHandler(context);
                }
                finally
                {
                    _executeGetValue = false;
                }
            }
            if (ColumnValueHandler != null) return ColumnValueHandler(row);
            return base.GetValue(context);
        }

        public override string GetName(object row, object item, string crossColumnId, object[] groupValues)
        {
            if (groupValues != null)
            {
                if (ColumnAggregateNameHandler != null)
                    return ColumnAggregateNameHandler(crossColumnId, groupValues);
                return GetGroupName(crossColumnId, groupValues);
            }
            if (row == null) return string.Empty;
            if (ColumnNameHandler != null) return ColumnNameHandler((TDataRow)row);
            return null;
        }

        private bool _executeGetName;

        public override string GetName(RenderContext context)
        {
            if (context.GroupValues != null)
            {
                if (ColumnAggregateNameHandler != null)
                    return ColumnAggregateNameHandler(context.CrossColumnId, context.GroupValues);
                return GetGroupName(context);
            }
            var row = context.GetDataRow<TDataRow>();
            if (row == null) return string.Empty;
            if (GetNameHandler != null && !_executeGetName)
            {
                _executeGetName = true;
                try
                {
                    return GetNameHandler(context);
                }
                finally
                {
                    _executeGetName = false;
                }
            }
            if (ColumnNameHandler != null) return ColumnNameHandler(row);
            return base.GetName(context);
        }

        public override void GetContent(object row, object item, HtmlTextWriter writer, string crossColumnId, object[] groupValues)
        {
            if (groupValues != null)
            {
                if (ColumnAggregateContentHandler != null)
                {
                    ColumnAggregateContentHandler(writer, crossColumnId, groupValues);
                    return;
                }
                GetContent(writer, crossColumnId, groupValues);
                return;
            }
            if (ColumnContentHandler != null)
                ColumnContentHandler((TDataRow)row, writer);
        }

        public override string GetCustomHyperLinkParameters(object row, object item, string crossColumnId, object[] groupValues)
        {
            if (groupValues != null && CustomAggregateHyperLinkParametersHandler != null)
                return CustomAggregateHyperLinkParametersHandler(crossColumnId, groupValues);
            if (CustomHyperLinkParametersHandler != null)
                return CustomHyperLinkParametersHandler((TDataRow)row);
            return null;
        }

        public override int GetRowsCount(object row, object item, object journalRow, BaseJournalControl journal, string crossColumnId, object[] groupValues)
        {
            if (groupValues != null)
            {
                if (ColumnAggregateRowsCountHandler != null)
                    return ColumnAggregateRowsCountHandler(journal, groupValues);
                return GetRowsCount(journal, groupValues, crossColumnId);
            }
            if (ColumnRowsCountHandler != null)
                return ColumnRowsCountHandler((TDataRow)row);
            return 1;
        }

        private bool _executeGetRowsCountHandler;

        public override int GetRowsCount(RenderContext context)
        {
            if (context.GroupValues != null)
            {
                if (ColumnAggregateRowsCountHandler != null)
                    return ColumnAggregateRowsCountHandler(context.Journal, context.GroupValues);
                return base.GetRowsCount(context);
            }

            context.LoadData(BaseJournalCrossTable);

            if (ColumnRowsCountHandler != null)
                return ColumnRowsCountHandler(context.GetDataRow<TDataRow>());

            int count;
            if (ExecuteGetRowsCount(context, out count))
                return count;

            return 1;
        }

        public override string GetRowNumber(RenderContext context)
        {
            if (RowNumberHandler != null)
                return RowNumberHandler((BaseJournalRow<TRow>)context.JournalRow, context.Journal, context.GetDataRow<TRow>());
            return base.GetRowNumber(context);
        }
    }

    public class Column<TRow> : Column<TRow, TRow>
        where TRow : BaseRow
    {
    }

    public class Column<TRow, TCrossTable, TResultData> : Column<TRow, TResultData>
        where TRow : BaseRow
        where TCrossTable : class
        where TResultData : class, new()
    {
        public BaseJournalCrossTableManyData<TRow, TCrossTable, TResultData> CrossTable { get; set; }

        public TRow CurrentRow { get; protected set; }

        protected TResultData GetDataRow(TRow row)
        {
            CurrentRow = row;
            if (row == null) return null;
            var rowValues = CrossTable.CurrentValues[row];
            if (rowValues.Count > RowIndex)
                return rowValues[RowIndex];
            return null;
        }

        public override int GetRowsCount(object row, object item, object journalRow, BaseJournalControl journal, string crossColumnId, object[] groupValues)
        {
            if (groupValues != null) base.GetRowsCount(row, item, journalRow, journal, crossColumnId, groupValues);
            if (row == null) return 1;
            return CrossTable.CurrentValues[(TRow)row].Count;
        }

        public override int GetRowsCount(RenderContext context)
        {
            if (context.GroupValues != null)
                return base.GetRowsCount(context);
            var row = context.GetDataRow<TRow>();
            if (row == null) return 1;
            context.LoadData(CrossTable);
            return CrossTable.CurrentValues[row].Count;
        }

        public override object GetValue(object row, object item, string crossColumnId, object[] groupValues)
        {
            var dataRow = row as TResultData ?? GetDataRow((TRow) row);
            return base.GetValue(dataRow, item, crossColumnId, groupValues);
        }

        public override string GetName(object row, object item, string crossColumnId, object[] groupValues)
        {
            var dataRow = row as TResultData ?? GetDataRow((TRow)row);
            return base.GetName(dataRow, item, crossColumnId, groupValues);
        }

        public override void GetContent(object row, object item, HtmlTextWriter writer, string crossColumnId, object[] groupValues)
        {
            var dataRow = row as TResultData ?? GetDataRow((TRow)row);
            base.GetContent(dataRow, item, writer, crossColumnId, groupValues);
        }

        public override string GetCustomHyperLinkParameters(object row, object item, string crossColumnId, object[] groupValues)
        {
            var dataRow = row as TResultData ?? GetDataRow((TRow)row);
            return base.GetCustomHyperLinkParameters(dataRow, item, crossColumnId, groupValues);
        }

        public override object GetValue(RenderContext context)
        {
            if (context.GroupValues != null)
                return base.GetValue(context);
            context.LoadData(CrossTable);
            context.LoadData(BaseJournalCrossTable);
            var row = context.GetDataRow<TResultData>();
            if (row == null)
                context.CrossDataItemRow = row = GetDataRow(context.GetDataRow<TRow>());
            if (row == null) return null;
            return base.GetValue(context);
        }

        public override string GetName(RenderContext context)
        {
            if (context.GroupValues != null)
                return base.GetName(context);
            context.LoadData(CrossTable);
            context.LoadData(BaseJournalCrossTable);
            var row = context.GetDataRow<TResultData>();
            if (row == null)
                context.CrossDataItemRow = row = GetDataRow(context.GetDataRow<TRow>());
            if (row == null) return string.Empty;
            return base.GetName(context);
        }
    }
}