namespace Nat.Web.Tools.Threads
{
    using System;
    using System.Collections.Generic;

    public class ProgressManagerActionArgs
    {
        public IDisposable DisposableObject { get; set; }
        public List<string> Errors { get; set; }
    }
}