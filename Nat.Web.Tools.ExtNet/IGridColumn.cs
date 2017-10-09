/*
* Created by: Sergey V. Shpakovskiy
* Created: 2012.09.26
* Copyright © JSC NAT Kazakhstan 2012
*/

namespace Nat.Web.Tools.ExtNet
{
    using System.Collections.Generic;
    using System.Web.UI.WebControls;

    using Ext.Net;

    public interface IGridColumn
    {
        string ColumnName { get; set; }

        string FilterColumnMapping { get; set; }
        
        string Header { get; set; }

        string ServerMapping { get; set; }

        bool HasFilter { get; }

        bool ShowInGrid { get; }

        Unit Width { get; }

        ModelFieldType ModelFieldType { get; set; }

        bool HasChildren { get; }

        /// <summary>
        /// Колонка в которой рисуется дерево. Для древовидных журналов.
        /// </summary>
        bool IsTreeColumn { get; set; }

        bool ChartColumn { get; set; }

        IEnumerable<IGridColumn> Children { get; set; }

        ColumnBase CreateColumn();

        GridFilter CreateFilter();

        IEnumerable<ModelField> CreateModelFields();
    }
}
