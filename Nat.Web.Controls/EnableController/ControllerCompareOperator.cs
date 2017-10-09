using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Nat.Web.Controls.EnableController
{
    public enum ControllerCompareOperator
    {
        Equal,
        NotEqual,
        GreaterThan,
        GreaterThanEqual,
        LessThan,
        LessThanEqual,
        IsEmpty,
        IsNotEmpty,
        Or,
        And,
        IsEnabled,
        IsDisabled,
        IsHidden,
        IsShown,
        IsReadOnly,
        IsNotReadOnly,
    }
}
