namespace Nat.Web.Tools.Threads
{
    using System;
    using System.Collections.Generic;
    using System.Runtime.Serialization;

    using Nat.Web.Tools.Initialization;
    using Nat.Web.Tools.Security;

    public static class ProgressManagerPool
    {
        private static readonly Dictionary<string, ProgressManager> List = new Dictionary<string, ProgressManager>();
        private static readonly object ListLock = new object();

        public static bool ContainsProgressManager(string key)
        {
            lock (ListLock)
            {
                var contains = List.ContainsKey(key);
                if (contains)
                {
                    var progress = List[key];

                    // удаляем ProgressManager, если хранится дольше 20 минут после завершения работы.
                    if (!progress.InProgress && progress.FinishTime != null && progress.FinishTime.Value.AddMinutes(20) < DateTime.Now)
                    {
                        List.Remove(key);
                        contains = false;
                    }
                }

                return contains;
            }
        }

        public static ProgressManager GetProgressManager(string key)
        {
            lock (ListLock)
            {
                if (!ContainsProgressManager(key))
                    return null;
                return List[key];
            }
        }

        public static ProgressManager RunAsync(string key, params ProgressManagerAction[] actions)
        {
            ProgressManager progressManager;
            lock (ListLock)
            {
                if (List.ContainsKey(key) && List[key].InProgress)
                    throw new ProgressManagerExistsException("ProgressManager with key '" + key + "' exists");
                
                if (!List.ContainsKey(key))
                {
                    var manager = new ProgressManager();
                    manager.LogMonitor = InitializerSection.GetSection().LogMonitor;
                    manager.LogMonitor.Sid = User.GetSID();
                    manager.LogMonitor.Init();
                    List[key] = manager;
                }

                progressManager = List[key];
            }

            progressManager.RunAsync(actions);
            return progressManager;
        }
    }
}