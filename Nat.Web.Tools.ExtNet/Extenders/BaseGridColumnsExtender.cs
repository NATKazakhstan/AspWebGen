/*
* Created by: Sergey V. Shpakovskiy
* Created: 2012.09.26
* Copyright © JSC NAT Kazakhstan 2012
*/

namespace Nat.Web.Tools.ExtNet.Extenders
{
    using System.Collections.Generic;
    using System.Linq;
    using Nat.Web.Controls.GenerationClasses;

    public static class BaseGridColumnsExtender
    {
        public static IEnumerable<IGridColumn> GetExtNetGridColumns(this BaseGridColumns gridColumns)
        {
            return GridHtmlGenerator.GetColumnsHierarchy<GridColumn>(gridColumns.Columns).Cast<IGridColumn>();
        }
    }
}
