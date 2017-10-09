using System;
using System.Linq;
using System.Web.UI;

namespace Nat.Web.Controls.GenerationClasses.BaseJournal
{
    public abstract class CrossColumn : BaseColumn
    {
        public override bool IsCrossColumn
        {
            get { return true; }
        }
        
        public override object GetValue(object row, object item, string crossColumnId, object[] groupValues)
        {
            throw new NotImplementedException();
        }

        public override string GetName(object row, object item, string crossColumnId, object[] groupValues)
        {
            throw new NotImplementedException();
        }

        public override void GetContent(object row, object item, HtmlTextWriter writer, string crossColumnId, object[] groupValues)
        {
            throw new NotImplementedException();
        }

        public override string GetCustomHyperLinkParameters(object row, object item, string crossColumnId, object[] groupValues)
        {
            throw new NotImplementedException();
        }
        
        public override string GetRowNumber(RenderContext context)
        {
            throw new NotImplementedException();
        }

        public override void DetectIsCrossColumn(BaseJournalControl control, string crossColumnName)
        {
            BaseCrossColumnDataSource.DetectIsCrossColumn(control, crossColumnName);
        }
    }

    public class CrossColumn<TRow> : CrossColumn
        where TRow : BaseRow
    {
        private CrossColumnDataSource<TRow> _crossColumnDataSource;
        public CrossColumnDataSource<TRow> CrossColumnDataSource
        {
            get { return _crossColumnDataSource; }
            set
            {
                _crossColumnDataSource = value;
                BaseCrossColumnDataSource = value;
                if (_crossColumnDataSource != null)
                    _crossColumnDataSource.BaseColumnName = ColumnName;
            }
        }

        protected internal override int RowIndex
        {
            set
            {
                base.RowIndex = value;
                CrossColumnDataSource.RowIndex = value;
            }
        }
    }
    /*
     * возможно не понядобится
     * 
    public class CrossColumn<TRow, TItem> : CrossColumn<TRow>
        where TRow : class
        where TItem : class
    {
        public GetContent<TRow, TItem> ColumnContentHandler { get; set; }
        public GetValue<TRow, TItem> ColumnValueHandler { get; set; }
        public GetName<TRow, TItem> ColumnNameHandler { get; set; }
        public ShowAsTemplate<TRow, TItem> ShowAsTemplateHandler { get; set; }

        public override object GetValue(object row, object item)
        {
            if (ColumnValueHandler == null) return null;
            return ColumnValueHandler((TRow)row, (TItem)item);
        }

        public override string GetName(object row, object item)
        {
            if (ColumnNameHandler == null) return null;
            return ColumnNameHandler((TRow)row, (TItem)item);
        }

        public override void GetContent(object row, object item, HtmlTextWriter writer)
        {
            if (ColumnContentHandler == null) return;
            ColumnContentHandler((TRow)row, (TItem)item, writer);
        }

        public override bool ShowAsTemplate(object row, object item)
        {
            if (ShowAsTemplateHandler == null) return true;
            return ShowAsTemplateHandler((TRow)row, (TItem)item);
        }
    }*/
}