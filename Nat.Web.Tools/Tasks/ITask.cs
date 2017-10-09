namespace Nat.Web.Tools.Tasks
{
    using System.Data.Common;

    public interface ITask
    {
        void Initialize(DbConnection connection, int commandTimeOut, ILogMonitor logMonitor);

        long Execute(long lastUpdateKey, int countInIteration);
    }
}