using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Nat.Web.Controls.GenerationClasses.Navigator;
using System.Data.Linq;

namespace Nat.Web.Controls.GenerationClasses.Data
{
    public interface IDataSourceViewCustomBehavior
    {
        DataContext DataContext { get; set; }
        bool SupportAllowAddRowGetSources { get; }
        List<IQueryable> AllowAddRowGetSources(BaseNavigatorValues valuesT);
        bool SupportGetParentCollection { get; }
        bool SupportGetNewParentCollection { get; }
        List<ParentDataSourceViewInfo> GetParentCollection();
        List<ParentDataSourceViewInfo> GetNewParentCollection();
    }
}
