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
using System.Web;

namespace Nat.Web.Controls.Trace
{
    public static class TraceContextExt
    {
        public static TraceContextExtInstance TraceExt()
        {
            return HttpContext.Current.TraceExt();
        }

        public static TraceContextExtInstance TraceExt(this HttpContext context)
        {
            return (TraceContextExtInstance) (context.Items["TraceContextExtInstance"] ??
                                              (context.Items["TraceContextExtInstance"] = new TraceContextExtInstance()));
        }

        public static void WriteExt(this TraceContext trace, string message)
        {
            WriteExt(trace, null, message, null);
        }

        public static void WriteExt(this TraceContext trace, string category, string message)
        {
            WriteExt(trace, category, message, null);
        }

        public static void WriteExt(this TraceContext trace, string category, string message, Exception errorInfo)
        {
            if (trace != null)
                trace.Write(category, message, errorInfo);
            TraceExt().Write(category, message, errorInfo);
        }
        
        public static void WarnExt(this TraceContext trace, string message)
        {
            WarnExt(trace, null, message, null);
        }

        public static void WarnExt(this TraceContext trace, string category, string message)
        {
            WarnExt(trace, category, message, null);
        }

        public static void WarnExt(this TraceContext trace, string category, string message, Exception errorInfo)
        {
            if (trace != null)
                trace.Warn(category, message, errorInfo);
            TraceExt().Warn(category, message, errorInfo);
        }
    }
}