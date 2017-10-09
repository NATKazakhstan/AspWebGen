using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Nat.Web.Tools.Security
{
    public class BaseLogMessageEntry : ILogMessageEntry
    {
        public string Message { get; set; }
        public long MessageCodeAsLong { get; set; }
        public DateTime DateTime { get; set; }
        public string Sid { get; set; }
        public long? RefRVSProperties { get; set; }
    }
}