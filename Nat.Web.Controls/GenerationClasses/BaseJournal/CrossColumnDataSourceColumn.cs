using System;
using System.Web.UI;

namespace Nat.Web.Controls.GenerationClasses.BaseJournal
{
    public abstract class CrossColumnDataSourceColumn<TRow> : BaseColumn
        where TRow : class
    {
        public override int GetRowsCount(RenderContext context)
        {
            if (context.GroupValues != null) return base.GetRowsCount(context);
            if (BaseJournalCrossTable != null && BaseJournalCrossTable.CanReturnInfo)
                return BaseJournalCrossTable.GetRowsCount(context);
            return 1;
        }

        public override int GetRowsCount(object row, object item, object journalRow, BaseJournalControl journal, string crossColumnId, object[] groupValues)
        {
            if (groupValues != null)
                return GetRowsCount(journal, groupValues, crossColumnId);
            return 1;
        }
    }

    public abstract class CrossColumnDataSourceColumn<THeaderKey, TRow, THeaderTable> : CrossColumnDataSourceColumn<TRow>
        where TRow : BaseRow
        where THeaderTable : class, ICrossTable<THeaderKey>
    {
        public VisibleCrossColumn<THeaderTable> VisibleHandler { get; set; }
        public abstract object GetItem(BaseJournalRow<TRow> journalRow, THeaderKey key, int index);
        public abstract int CountOfData(BaseJournalRow<TRow> journalRow, THeaderKey key);

        public override object GetCrossItem(object journalRow, object key, int index)
        {
            return GetItem((BaseJournalRow<TRow>)journalRow, (THeaderKey)key, index);
        }

        public override int CountOfCrossData(object journalRow, object key)
        {
            return CountOfData((BaseJournalRow<TRow>) journalRow, (THeaderKey) key);
        }

        public override bool GetVisible(RenderContext context)
        {
            if (VisibleHandler != null) return VisibleHandler(context.GetDataRow<THeaderTable>()) && base.GetVisible(context);
            return base.GetVisible(context);
        }
    }
    
    public abstract class CrossColumnDataSourceColumn<THeaderKey, TRow, THeaderTable, TItem> : CrossColumnDataSourceColumn<THeaderKey, TRow, THeaderTable>
        where TRow : BaseRow
        where THeaderTable : class, ICrossTable<THeaderKey>
        where TItem : class
    {
        public GetContent<TRow, TItem> ColumnContentHandler { get; set; }
        public GetValue<TRow, TItem> ColumnValueHandler { get; set; }
        public GetName<TRow, TItem> ColumnNameHandler { get; set; }
        public GetName<TRow, TItem> CustomHyperLinkParametersHandler { get; set; }
        public GetRowNumber<TRow, TItem> GetRowNumberHandler { get; set; }

        public override object GetValue(object row, object item, string crossColumnId, object[] groupValues)
        {
            if (groupValues != null)
                return GetGroupValue(crossColumnId, groupValues);
            if (ColumnValueHandler == null || item == null) return null;
            return ColumnValueHandler((TRow)row, (TItem)item);
        }

        public override string GetName(object row, object item, string crossColumnId, object[] groupValues)
        {
            if (groupValues != null)
                return GetGroupName(crossColumnId, groupValues);
            if (ColumnNameHandler == null || item == null) return null;
            return ColumnNameHandler((TRow)row, (TItem)item);
        }

        public override void GetContent(object row, object item, HtmlTextWriter writer, string crossColumnId, object[] groupValues)
        {
            if (groupValues != null)
            {
                GetContent(writer, crossColumnId, groupValues);
                return;
            }
            if (ColumnContentHandler == null || item == null) return;
            ColumnContentHandler((TRow)row, (TItem)item, writer);
        }

        public override object GetItem(BaseJournalRow<TRow> journalRow, THeaderKey key, int index)
        {
            throw new NotImplementedException();
        }

        public override int CountOfData(BaseJournalRow<TRow> journalRow, THeaderKey key)
        {
            throw new NotImplementedException();
        }

        public override string GetCustomHyperLinkParameters(object row, object item, string crossColumnId, object[] groupValues)
        {
            if (CustomHyperLinkParametersHandler == null || item == null) return null;
            return CustomHyperLinkParametersHandler((TRow)row, (TItem)item);
        }
        
        /*
        public override object GetCrossItem(RenderContext context)
        {
            if (BaseJournalCrossTable != null && BaseJournalCrossTable.CanReturnInfo)
                return BaseJournalCrossTable.GetDataItem(context);
            return base.GetCrossItem(context);
        }*/

        public override string GetRowNumber(RenderContext context)
        {
            if (GetRowNumberHandler != null)
                return GetRowNumberHandler((BaseJournalRow<TRow>) context.JournalRow, context.Journal,
                                           context.GetDataRow<TRow>(), context.GetCrossDataItemRow<TItem>());
            return base.GetRowNumber(context);
        }
    }
}
