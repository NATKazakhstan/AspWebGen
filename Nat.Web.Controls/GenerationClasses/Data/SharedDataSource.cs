using System;
using System.Collections;
using System.Web.UI;
using Nat.Web.Tools.Initialization;

namespace Nat.Web.Controls.GenerationClasses.Data
{
    public class SharedDataSource<TKey> : BaseDataSource<TKey> 
        where TKey : struct
    {
        private BaseDataSourceView<TKey> _baseView;

        #region IDataSource Members

        public override event EventHandler DataSourceChanged;

        public override DataSourceView GetView(string viewName)
        {
            CreateInstanceOfDataSourceView(viewName);
            return BaseView;
        }

        private void CreateInstanceOfDataSourceView(string viewName)
        {
            if ("Default".Equals(viewName, StringComparison.OrdinalIgnoreCase) || string.IsNullOrEmpty(viewName))
                viewName = DataSourceName;
            if (!DatasourcesSectionCollection.DataSources.ContainsKey(viewName))
            {
                throw new Exception(string.Format("Configuration section 'Nat.Initializer/Initializer/datasources' not contains datasource name '{0}' ({1})", viewName, ID));
            }

            var instance = Activator.CreateInstance(DatasourcesSectionCollection.DataSources[viewName], (IDataSource)this, viewName);
            _baseView = (BaseDataSourceView<TKey>) instance;
            _baseView.SelectForAddType = SelectForAddType;
            _baseView.HideRecordCanNotSelected = HideRecordCanNotSelected;
        }

        public override ICollection GetViewNames()
        {
            return DatasourcesSectionCollection.DataSources.Keys;
        }

        #endregion

        public string DataSourceName { get; set; }

        public override BaseDataSourceView<TKey> BaseView
        {
            get 
            {
                if (_baseView == null)
                    CreateInstanceOfDataSourceView(DataSourceName);
                return _baseView;
            }
        }
    }
}