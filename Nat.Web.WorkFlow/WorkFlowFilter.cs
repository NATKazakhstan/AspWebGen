using System.Data.Linq;
using Nat.Web.Controls.GenerationClasses;

namespace Nat.Web.WorkFlow
{
    public delegate void WorkFlowFilterGetFiltersHandler<TTable, TDataContext>(BaseFilterEventArgs<TTable, TDataContext> args)
        where TTable : class
        where TDataContext : DataContext;

    public class WorkFlowFilter<TTable, TDataContext>
        where TTable : class
        where TDataContext : DataContext
    {
        public WorkFlowFilter()
        {
        }

        public WorkFlowFilter(WorkFlowFilterGetFiltersHandler<TTable, TDataContext> getFiltersHandler)
        {
            GetFiltersHandler = getFiltersHandler;
        }

        public WorkFlowFilterGetFiltersHandler<TTable, TDataContext> GetFiltersHandler { get; private set; }

        public virtual void GetFilters(BaseFilterEventArgs<TTable, TDataContext> args)
        {
            if (GetFiltersHandler != null) GetFiltersHandler(args);
        }
    }
}