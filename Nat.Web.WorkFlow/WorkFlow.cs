using System.Collections.Generic;

namespace Nat.Web.WorkFlow
{
    public abstract class WorkFlow
    {
        public abstract void InitializeStatuses();
    }

    public abstract class WorkFlow<TStatus> : WorkFlow
    {
        public List<TStatus> Statuses { get; private set; }
        public Dictionary<TStatus, List<TStatus>> NextStatuses { get; private set; }

        public abstract void InitializeStatuses(List<TStatus> statuses);
        public abstract void InitializeNextStatuses(List<TStatus> statuses);

        public override void InitializeStatuses()
        {
            Statuses = new List<TStatus>();
            NextStatuses = new Dictionary<TStatus, List<TStatus>>();
            InitializeStatuses(Statuses);
            InitializeNextStatuses(Statuses);
        }

        protected void DeclareNextStatuses(TStatus status, params TStatus[] nextStatuses)
        {
            if (!NextStatuses.ContainsKey(status))
                NextStatuses[status] = new List<TStatus>();
            NextStatuses[status].AddRange(nextStatuses);
        }
    }
}