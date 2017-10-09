using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Nat.Web.Controls.GenerationClasses
{
    public class ContextEventArgs<TKey, TEdit, TJournal> : EventArgs
        where TKey : struct
        where TEdit : AbstractUserControl<TKey>
        where TJournal : AbstractUserControl<TKey>
    {
        public ContextEventArgs(object context)
        {
            Edit = (TEdit)context;
            Journal = (TJournal)context;
            if (Edit != null)
                SelectedValue = Edit.SelectedValueKey;
            else if (Journal != null)
                SelectedValue = Journal.SelectedValueKey;
        }

        public TEdit Edit { get; private set; }
        public TJournal Journal { get; private set; }
        public TKey? SelectedValue { get; private set; }
    }
}
