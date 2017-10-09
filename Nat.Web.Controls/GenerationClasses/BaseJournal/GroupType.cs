using System;

namespace Nat.Web.Controls.GenerationClasses.BaseJournal
{
    [Flags]
    public enum GroupType
    {
        Top = 1,
        Total = 2,
        TopTotal = 3,
        Left = 4,
        LeftTotal = 6,
        InHeader = 8,
        None = 0,
    }
}