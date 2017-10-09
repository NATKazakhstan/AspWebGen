using System.ComponentModel;

namespace Nat.Web.Controls.GenerationClasses.BaseJournal
{
    public class BaseJournalValidateValueEventArgs : CancelEventArgs
    {
        public BaseJournalValidateValueEventArgs(object newValue, RenderContext renderContext)
        {
            NewValue = newValue;
            RenderContext = renderContext;
        }

        public BaseJournalValidateValueEventArgs(bool cancel, object newValue, RenderContext renderContext) : base(cancel)
        {
            NewValue = newValue;
            RenderContext = renderContext;
        }

        public object NewValue { get; set; }
        public RenderContext RenderContext { get; set; }
    }
}