using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Nat.Web.Controls.EnableController
{
    [Flags]
    public enum ControllerActiveType
    {
        None = 0,
        Enabled = 1,
        ReadOnly = 2,
        ValidationDisabled = 4,
        Hide = 8,
    }
}
