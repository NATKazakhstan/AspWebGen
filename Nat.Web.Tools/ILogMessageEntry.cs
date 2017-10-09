using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Nat.Web.Tools
{
    public interface ILogMessageEntry
    {
        string Message { get; set; }
        long MessageCodeAsLong { get; set; }
        DateTime DateTime { get; set; }
        string Sid { get; set; }
        long? RefRVSProperties { get; set; }
    }
}
