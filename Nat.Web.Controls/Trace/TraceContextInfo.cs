// -----------------------------------------------
// <copyright company="JSC NAT Kazakhstan">
//   Copyright © JSC NAT Kazakhstan 2012
// </copyright>
// <author>
//   Sergey V. Shpakovskiy
// </author>
// <created>
//   2012.04.19
// </created>
// -----------------------------------------------

using System;

namespace Nat.Web.Controls.Trace
{
    public class TraceContextInfo
    {
        public string Category { get; set; }
        public string Message { get; set; }
        public Exception Exception { get; set; }
        public long LongFormLast { get; set; }
        public long LongFormFirst { get; set; }
        public bool IsWarning { get; set; }
    }
}