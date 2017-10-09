using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Web.UI.WebControls;
using Nat.Web.Controls;

namespace Nat.Web.Controls
{
    public interface IFilterControl
    {
        event EventHandler FilterApply;
        IQueryable GetWhereForParentFilter(string parent, IQueryable enumerable);
        IQueryable SetFilters(IQueryable enumerable);
    }
}
