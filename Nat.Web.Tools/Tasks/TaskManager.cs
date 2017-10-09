namespace Nat.Web.Tools.Tasks
{
    using System;
    using System.Collections.Generic;
    using System.Data.Common;
    using System.Linq;
    using System.Threading;
    using System.Web.Compilation;
    using System.Web.UI;

    using Nat.Tools.Specific;
    using Nat.Web.Tools.Initialization;

    public class TaskManager : IExecuteManager
    {
        private static readonly Dictionary<string, object> LockRuns = new Dictionary<string, object>();

        public bool CheckPermit(Page page)
        {
            return true;
        }

        public void Execute(IDictionary<string, string> queryParameters, Page page)
        {
            const string TypeName = "typeName";

            if (queryParameters.ContainsKey(TypeName))
                ExecuteTask(queryParameters[TypeName], GetLogMonitor());
            else
                ExecuteTasks();

            page.Response.StatusCode = 204;
            page.Response.End();
        }

        public void ExecuteTasks()
        {
            using (var connection = CreateConnection())
            using (var db = new DBDataContext(connection))
            {
                var logMonitor = GetLogMonitor();

                foreach (var typeName in db.SYS_TaskStates.Select(r => r.TypeName))
                {
                    try
                    {
                        ExecuteTask(typeName, logMonitor);
                    }
                    catch (Exception e)
                    {
                        logMonitor.LogException(e);
                    }
                }
            }
        }

        private static ILogMonitor GetLogMonitor()
        {
            ILogMonitor logMonitor;
            try
            {
                var section = InitializerSection.GetSection();
                logMonitor = section.LogMonitor;
                logMonitor.Init();
            }
            catch (Exception)
            {
                var type = Type.GetType("Nat.Web.Controls.LogMonitor, Nat.Web.Controls, Version=1.0.0.0, Culture=neutral, PublicKeyToken=11c252a207597415");
                logMonitor = (ILogMonitor)Activator.CreateInstance(type);
            }
            return logMonitor;
        }

        public void ExecuteTask<TTask>(int countInIteration = 100)
            where TTask : ITask
        {
            ExecuteTask(typeof(TTask).AssemblyQualifiedName, null, countInIteration);
        }

        public void ExecuteTask(string taskTypeName, ILogMonitor logMonitor, int countInIteration = 100)
        {
            object lockRun;
            lock (LockRuns)
            {
                if (!LockRuns.ContainsKey(taskTypeName))
                    LockRuns[taskTypeName] = new object();
                lockRun = LockRuns[taskTypeName];
            }

            if (logMonitor == null)
                logMonitor = GetLogMonitor();

            var startTime = DateTime.Now;
            lock (lockRun)
            {
                var type = BuildManager.GetType(taskTypeName, true, true);
                using (var connection = CreateConnection())
                using (var db = new DBDataContext(connection))
                {
                    var task = (ITask)Activator.CreateInstance(type);
                    task.Initialize(SpecificInstances.DbFactory.CreateConnection(), 60, logMonitor);
                    var keyOfState = GetKeyOfState(taskTypeName, db);
                    var nextValue = keyOfState;
                    do
                    {
                        keyOfState = nextValue;
                        nextValue = task.Execute(keyOfState, countInIteration);
                        db.SYS_P_UpdateTaskStates(taskTypeName, nextValue, keyOfState);
                    }
                    while (nextValue > keyOfState && startTime.AddMinutes(5) > DateTime.Now);
                }
            }
        }

        private DbConnection CreateConnection()
        {
            return SpecificInstances.DbFactory.CreateConnection();
        }

        private long GetKeyOfState(string typeName, DBDataContext db)
        {
            return db.SYS_TaskStates.Where(r => r.TypeName == typeName).Select(r => r.KeyOfState).FirstOrDefault();
        }
    }
}