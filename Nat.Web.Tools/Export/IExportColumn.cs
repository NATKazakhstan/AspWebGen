/*
* Created by: Sergey V. Shpakovskiy
* Created: 2012.08.29
* Copyright © JSC NAT Kazakhstan 2012
*/

namespace Nat.Web.Tools.Export
{
    using System.Collections.Generic;

    public interface IExportColumn
    {
        string Header { get; }

        string ColumnName { get; }

        bool IsVerticalHeaderText { get; }

        bool IsVerticalDataText { get; }
        
        string NullItemText { get; }

        bool Visible { get; }

        int ColSpan { get; }

        int RowSpan { get; }

        bool HasChild { get; }

        decimal Width { get; }

        bool IsNumericColumn { get; }

        string GetValue(object row);

        string GetHyperLink(object row);

        IEnumerable<IExportColumn> GetChilds();
    }
}