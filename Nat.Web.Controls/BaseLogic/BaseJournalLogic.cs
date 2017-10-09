using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Nat.Web.Controls.GenerationClasses;

namespace Nat.Web.Controls.BaseLogic
{
    public abstract class BaseJournalLogic : IJournalLogic
    {
        public abstract void InitColumns(BaseGridColumns gridColumns);
    }
}
