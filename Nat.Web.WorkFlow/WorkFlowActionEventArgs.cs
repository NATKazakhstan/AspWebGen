using System;

namespace Nat.Web.WorkFlow
{
    using Nat.Web.Tools.WorkFlow;

    [Serializable]
    public class WorkFlowActionEventArgs : EventArgs
    {
        public bool Successfull { get; set; }

        public string ResultMessage { get; set; }

        public string[] Arguments { get; set; }

        public IWorkFlow WorkFlow { get; set; }
    }
}