using System;
using System.Collections.Generic;
using Nat.Web.Controls;

namespace Nat.Web.ReportManager
{
    public interface IReportList
    {
        IEnumerable<Type> GetReportTypes();
        LogMessageType LogMessageType { get; }
    }
}