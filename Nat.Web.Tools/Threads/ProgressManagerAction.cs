namespace Nat.Web.Tools.Threads
{
    using System;

    public class ProgressManagerAction
    {
        public ProgressManagerAction(ProgressManagerActionHandler handler)
        {
            Handler = handler;
            StepCount = 1;
        }

        public ProgressManagerAction(int stepCount)
        {
            StepCount = stepCount;
        }

        public ProgressManagerActionHandler Handler { get; set; }
        public int StepCount { get; set; }
        public Action<int> StepProgress { get; set; }
    }
}