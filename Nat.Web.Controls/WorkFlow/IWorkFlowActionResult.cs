namespace Nat.Web.Tools.WorkFlow
{
    public interface IWorkFlowActionResult
    {
        bool Successfull { get; }

        string ResultMessage { get; }

        bool ActionExecuted { get; }
    }
}