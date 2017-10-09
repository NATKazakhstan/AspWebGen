namespace Nat.Web.Tools.Threads
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;

    using Nat.Web.Tools.Security;

    public class ProgressManager
    {
        private volatile int progressIndex = -1;

        private volatile int stepCount = 0;

        private readonly object lockActions = new object();

        public ProgressManager()
        {
            Errors = new List<string>();
        }

        public DateTime? StartTime { get; set; }

        public DateTime? FinishTime { get; set; }

        public List<string> Errors { get; private set; }

        public ILogMonitor LogMonitor { get; set; }

        public int Percent
        {
            get
            {
                if (!InProgress) return -1;
                return progressIndex * 100 / stepCount;
            }
        }

        public bool InProgress
        {
            get { return stepCount != 0 && progressIndex > -1; }
        }

        public void ExecuteActions(params ProgressManagerAction[] actions)
        {
            lock (lockActions)
            {
                var processActions = actions.ToList();
                ClearState();
                StartTime = DateTime.Now;
                stepCount = actions.Sum(r => r.StepCount);
                var disposeList = new List<IDisposable>();
                int startProgressIndex = 0;
                
                // ReSharper disable once AccessToModifiedClosure
                Action<int> stepProgress = delegate(int i) { progressIndex = startProgressIndex + i; };
                try
                {
                    foreach (var action in processActions)
                    {
                        var args = new ProgressManagerActionArgs();
                        if (action.StepCount > 1)
                        {
                            startProgressIndex = progressIndex;
                            action.StepProgress = stepProgress;
                            action.Handler(args);
                            progressIndex = startProgressIndex + action.StepCount;
                        }
                        else
                        {
                            action.Handler(args);
                            progressIndex ++;
                        }

                        if (args.DisposableObject != null)
                            disposeList.Add(args.DisposableObject);

                        if (args.Errors != null && args.Errors.Count > 0)
                        {
                            Errors.AddRange(args.Errors);
                            break;
                        }
                    }
                }
                catch (Exception e)
                {
                    Errors.Add(DateTime.Now.ToString("HH:mm: ") + e);
                }
                finally
                {
                    foreach (var disposable in disposeList.AsEnumerable().Reverse())
                        disposable.Dispose();
                    progressIndex = -1;
                    FinishTime = DateTime.Now;
                }
            }

            if (LogMonitor != null && Errors.Count > 0)
            {
                LogMonitor.Log(
                    new BaseLogMessageEntry
                        {
                            DateTime = DateTime.Now,
                            Message = string.Join("; ", Errors.ToArray()),
                            MessageCodeAsLong = LogConstants.SystemErrorInApp,
                        });
            }
        }

        private void ClearState()
        {
            Errors.Clear();
            progressIndex = 0;
            StartTime = null;
            FinishTime = null;
        }

        public void RunAsync(params ProgressManagerAction[] actions)
        {
            ClearState();
            Errors.Clear();
            progressIndex = 0;
            ThreadPool.QueueUserWorkItem(state => ExecuteActions((ProgressManagerAction[])state), actions);
        }
    }
}