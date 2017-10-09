using System;
using System.Collections.Generic;
using System.Data.Linq;
using System.Linq;
using System.Web;

namespace Nat.Web.Controls.GenerationClasses.BaseJournal
{
    public interface ICrossTreeTable<TKey, THeaderTable> : ICrossTable<TKey>
        where TKey : struct
        where THeaderTable : class
    {
        TKey? refParent { get; }
        THeaderTable ParentObject { get; }
        EntitySet<THeaderTable> ChildObjects { get; }
    }
}
