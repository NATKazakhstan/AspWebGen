using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Nat.Web.Controls.GenerationClasses.Buttons
{
    public class BaseButtonEventArgs<TKey, TEdit, TJournal> : ContextEventArgs<TKey, TEdit, TJournal>
        where TKey : struct
        where TEdit : AbstractUserControl<TKey>
        where TJournal : AbstractUserControl<TKey>
    {
        public BaseButtonEventArgs(AdditionalButtons button, object context) : base(context)
        {
            Button = button;
        }

        public AdditionalButtons Button { get; private set; }
    }
}
