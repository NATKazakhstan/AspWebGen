using System;

namespace Nat.Web.Tools
{
    public class SessionWorkerArgs : EventArgs
    {
        private SessionWorker sessionWorker;

        public SessionWorker SessionWorker
        {
            get { return sessionWorker; }
            set { sessionWorker = value; }
        }
    }
}