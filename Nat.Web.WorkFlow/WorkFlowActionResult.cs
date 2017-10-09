using Nat.Web.Tools.WorkFlow;

namespace Nat.Web.WorkFlow
{
    public class WorkFlowActionResult : IWorkFlowActionResult
    {
        public bool Successfull { get; set; }
        public string ResultMessage { get; set; }
        public bool ActionExecuted { get; set; }
    }
}