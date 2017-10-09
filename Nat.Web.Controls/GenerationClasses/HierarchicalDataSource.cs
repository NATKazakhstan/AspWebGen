/*
 * Created by: Sergey V. Shpakovskiy
 * Created: 6 октября 2008 г.
 * Copyright © JSC New Age Technologies 2008
 */

using System;
using System.Web.Compilation;
using System.Web.UI;

namespace Nat.Web.Controls
{
    public class HierarchicalDataSource : Control, IHierarchicalDataSource
    {
        private HierarchicalDataSourceView _view;

        public string TypeName { get; set; }
        public event EventHandler DataSourceChanged;

        public HierarchicalDataSourceView GetHierarchicalView(string viewPath)
        {
            if(_view == null)
            {
                var type = BuildManager.GetType(TypeName, true, false);
                var instance = Activator.CreateInstance(type);
                _view = (HierarchicalDataSourceView) instance;
            }
            return _view;
        }
    }
}