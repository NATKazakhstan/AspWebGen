using System;
using System.ComponentModel;
namespace Nat.Web.Controls.GenerationClasses.BaseJournal
{
    public class BaseJournalUpdateValueEventArgs : CancelEventArgs
    {
        public BaseJournalUpdateValueEventArgs(object newValue, RenderContext renderContext)
        {
            NewValue = newValue;
            RenderContext = renderContext;
        }

        public BaseJournalUpdateValueEventArgs(bool cancel, object newValue, RenderContext renderContext) : base(cancel)
        {
            NewValue = newValue;
            RenderContext = renderContext;
        }

        public object NewValue { get; set; }
        public RenderContext RenderContext { get; set; }
        public bool Updated { get; set; }
    }
}