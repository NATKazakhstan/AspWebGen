using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data.Linq;

namespace Nat.Web.Controls.GenerationClasses.Data
{
    public class BaseDataSourceViewCustomBehavior : IDataSourceViewCustomBehavior
    {
        #region IDataSourceViewCustomBehavior Members

        public DataContext DataContext { get; set; }

        public virtual List<IQueryable> AllowAddRowGetSources(Nat.Web.Controls.GenerationClasses.Navigator.BaseNavigatorValues valuesT)
        {
            throw new NotSupportedException();
        }

        public virtual bool SupportGetNewParentCollection
        {
            get { return false; }
        }

        public virtual List<ParentDataSourceViewInfo> GetParentCollection()
        {
            throw new NotSupportedException();
        }

        public virtual List<ParentDataSourceViewInfo> GetNewParentCollection()
        {
            throw new NotImplementedException();
        }

        public virtual bool SupportAllowAddRowGetSources
        {
            get { return false; }
        }

        public virtual bool SupportGetParentCollection
        {
            get { return false; }
        }

        #endregion
    }

    public class BaseDataSourceViewCustomBehavior<TDbDataContext> : BaseDataSourceViewCustomBehavior
        where TDbDataContext : DataContext
    {
        public TDbDataContext DB
        {
            get { return (TDbDataContext)base.DataContext; }
        }
    }
}
