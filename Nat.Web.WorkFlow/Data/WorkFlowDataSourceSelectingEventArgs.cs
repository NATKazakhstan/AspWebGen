namespace Nat.Web.WorkFlow.Data
{
    using System;

    using Nat.Web.Controls.GenerationClasses;

    public class WorkFlowDataSourceSelectingEventArgs : EventArgs
    {
        public BrowseFilterParameters BrowseFilterParameters { get; set; }
    }
}