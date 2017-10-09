using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Nat.Web.Controls.GenerationClasses.Buttons
{
    public class BaseButtonVisibleEventArgs<TKey, TEdit, TJournal> : ContextEventArgs<TKey, TEdit, TJournal>
        where TKey : struct
        where TEdit : AbstractUserControl<TKey>
        where TJournal : AbstractUserControl<TKey>
    {
        public BaseButtonVisibleEventArgs(object context) : base(context)
        {
            Visible = true;
        }

        public bool Visible { get; set; }
    }
}
