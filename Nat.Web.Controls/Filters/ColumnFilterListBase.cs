/*
 * Created by : Daniil Kovalev
 * Created    : 07.12.2007
 */

using System;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace Nat.Web.Controls.Filters
{
    public abstract class ColumnFilterListBase : DataBoundControl, INamingContainer
    {
        public abstract ColumnFilterStorageList ColumnFilterStorages { get; }
        public virtual Boolean ShowFullBriefViewButton { get { return false; } set { } }
        public virtual String PopupBehaviorParentNode { get { return null;} set { } }
        public abstract event EventHandler<ColumnFilterListCreatingEventArgs> ColumnFilterListCreating;
    }
}