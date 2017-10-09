using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Nat.Web.Controls.GenerationClasses.BaseJournal
{
    public abstract class BaseJournalFilterEventArgs : EventArgs
    {
        public abstract IEnumerable<BaseFilterParameter> GetFilterParameters();
        public CrossColumnDataSource CrossHeader { get; set; }
        public BaseJournalCrossTable CrossTable { get; set; }
    }

    public class BaseJournalFilterEventArgs<TTable, TFilterParameter> : BaseJournalFilterEventArgs
        where TTable : class
        where TFilterParameter : BaseFilterParameter<TTable>
    {
        private readonly List<TFilterParameter> _wheres = new List<TFilterParameter>();

        public List<TFilterParameter> Wheres
        {
            get { return _wheres; }
        }

        public override IEnumerable<BaseFilterParameter> GetFilterParameters()
        {
            return Wheres.Cast<BaseFilterParameter>();
        }
    }
}
