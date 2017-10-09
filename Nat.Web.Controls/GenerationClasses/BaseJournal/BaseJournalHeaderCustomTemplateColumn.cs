using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;

namespace Nat.Web.Controls.GenerationClasses.BaseJournal
{
    public class BaseJournalHeaderCustomTemplateColumn : Control, INamingContainer
    {
        public BaseJournalHeaderCustomTemplateColumn()
        { }

        public BaseJournalHeaderCustomTemplateColumn(RenderContext renderContext)
        {
            RenderContext = renderContext;
        }

        public RenderContext RenderContext { get; set; }
    }

    public class BaseJournalHeaderCustomTemplateColumn<TRow> : BaseJournalHeaderCustomTemplateColumn
    {
        public BaseJournalHeaderCustomTemplateColumn()
        { }

        public BaseJournalHeaderCustomTemplateColumn(TRow row)
        {
            Row = row;            
        }

        public BaseJournalHeaderCustomTemplateColumn(TRow row, RenderContext renderContext)
            : base(renderContext)
        {
            Row = row;
        }

        public TRow Row { get; set; }
    }
}
