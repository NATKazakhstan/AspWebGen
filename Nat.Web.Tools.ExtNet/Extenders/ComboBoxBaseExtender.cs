namespace Nat.Web.Tools.ExtNet.Extenders
{
    using Ext.Net;

    using Nat.Web.Controls.GenerationClasses;
    using Nat.Web.Tools.ExtNet.ExtNetConfig;

    public static class ComboBoxBaseExtender
    {
        public static void InitializeListConfig(this ComboBoxBase comboBox, IListConfig config)
        {
            if (config != null)
                comboBox.ListConfig = config.GetListConfig();
        }

        public static void InitializeListConfig(this Store store, IListConfig config)
        {
            if (config != null)
            {
                config.AddModelFields(store.Model.Primary.Fields);
                var ajaxProxy = store.Proxy.Primary as AjaxProxy;
                if (ajaxProxy != null)
                    ajaxProxy.ExtraParams[AutoCompleteHandler.ComboBoxView] = config.GetType().AssemblyQualifiedName;
            }
        }

        public static void InitializeListConfig<TKey>(this BaseDataSource<TKey> source, IListConfig config) 
            where TKey : struct
        {
            if (config != null)
                InitializeListConfig(source.BaseView, config);
        }

        public static void InitializeListConfig<TKey>(this BaseDataSourceView<TKey> view, IListConfig config)
            where TKey : struct
        {
            if (config != null)
            {
                foreach (var column in config.Columns)
                    view.SetLookupVisible(column.ServerMaping, true);
            }
        }

        public static void InitializeListConfig(this SelectParameters selectParameters, IListConfig config)
        {
        }

        public static void InitializeListConfig(this SelectColumnParameters selectColumnParameters, IListConfig config)
        {
        }
    }
}
