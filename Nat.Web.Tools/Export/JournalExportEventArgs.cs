/*
* Created by: Sergey V. Shpakovskiy
* Created: 2012.08.29
* Copyright © JSC NAT Kazakhstan 2012
*/

namespace Nat.Web.Tools.Export
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Text;

    using Nat.Web.Controls;
    using Nat.Web.Tools.Export.Formatting;

    public class JournalExportEventArgs
    {
        public string Header { get; set; }

        public List<string> FilterValues { get; set; }

        public ICollection Data { get; set; }

        public IAccessControl Control { get; set; }

        public IEnumerable<IExportColumn> Columns { get; set; }

        public string Format { get; set; }

        public ILogMonitor LogMonitor { get; set; }
        
        public bool CheckPermit { get; set; }

        public string FileNameExtention { get; set; }

        public Action<object> StartRenderRow { get; set; }

        public StringBuilder ViewJournalUrl { get; set; }

        public long ExportLog { get; set; }

        public List<ConditionalFormatting> ConditionalFormatting { get; set; }

        public int FixedColumnsCount { get; set; }

        public int FixedRowsCount { get; set; }
    }
}