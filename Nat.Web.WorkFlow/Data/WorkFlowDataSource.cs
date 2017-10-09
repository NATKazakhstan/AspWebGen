using System;
using System.Collections;
using System.Web.UI;

namespace Nat.Web.WorkFlow.Data
{
    public abstract class WorkFlowDataSource : Control, IDataSource
    {
        private WorkFlowDataSourceView _view;

        public DataSourceView GetView(string viewName)
        {
            return View;
        }

        protected WorkFlowDataSourceView View
        {
            get { return _view ?? (_view = CreateDataSourceView()); }
            set { _view = value; }
        }

        protected abstract WorkFlowDataSourceView CreateDataSourceView();

        public ICollection GetViewNames()
        {
            return new[] {"Default"};
        }

        public event EventHandler DataSourceChanged;

        public event EventHandler<WorkFlowDataSourceSelectingEventArgs> Selecting;

        internal protected virtual void OnSelecting(WorkFlowDataSourceSelectingEventArgs e)
        {
            var handler = Selecting;
            if (handler != null)
                handler(this, e);
        }
    }
}