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
using System.Collections.Generic;
using System.Diagnostics;
using System.Web.Configuration;

namespace Nat.Web.Controls.Trace
{
    public class TraceContextExtInstance
    {
        private Stopwatch watchLast;
        private Stopwatch watchFirst;

        public List<TraceContextInfo> TraceInfoExt { get; set; }

        private bool? isEnabled;

        public bool IsEnabled
        {
            get
            {
                if (isEnabled == null)
                {
                    var trace = WebConfigurationManager.AppSettings["NatTraceTimingReqeusts"];
                    isEnabled = !string.IsNullOrEmpty(trace);
                    if (isEnabled.Value)
                    {
                        TraceInfoExt = new List<TraceContextInfo>();
                        watchFirst = new Stopwatch();
                        watchFirst.Start();
                        watchLast = new Stopwatch();
                        watchLast.Start();
                    }
                }
                return isEnabled.Value;
            }
        }

        public void Write(string category, string message, Exception errorInfo)
        {
            if (IsEnabled)
            {
                TraceInfoExt.Add(new TraceContextInfo
                                     {
                                         Category = category,
                                         Exception = errorInfo,
                                         Message = message,
                                         LongFormFirst = watchFirst.ElapsedMilliseconds,
                                         LongFormLast = watchLast.ElapsedMilliseconds,
                                     });
                watchLast.Reset();
                watchLast.Start();
            }
        }

        public void Warn(string category, string message, Exception errorInfo)
        {
            if (IsEnabled)
            {
                TraceInfoExt.Add(new TraceContextInfo
                                     {
                                         Category = category,
                                         Exception = errorInfo,
                                         Message = message,
                                         IsWarning = true,
                                         LongFormFirst = watchFirst.ElapsedMilliseconds,
                                         LongFormLast = watchLast.ElapsedMilliseconds,
                                     });
                watchLast.Reset();
            }
        }
    }
}