using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Web;
using System.Web.UI;

namespace Nat.Web.Controls.GenerationClasses.BaseJournal
{
    [ParseChildren(true)]
    public class BaseJournalCustomColumn
    {
        [PersistenceMode(PersistenceMode.Attribute)]
        public string ColumnName { get; set; }

        [PersistenceMode(PersistenceMode.InnerProperty)]
        [TemplateContainer(typeof(BaseJournalHeaderCustomTemplateColumn))]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
        public virtual ITemplate Template { get; set; }

        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public ITemplate InternalTemplate { get; set; }
    }
}
