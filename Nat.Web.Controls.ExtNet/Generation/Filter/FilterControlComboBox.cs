/*
 * Created by: Alexey V. Vorochsak
 * Created: 2013.06.10
 * Copyright © JSC NAT Kazakhstan 2013
 */

namespace Nat.Web.Controls.ExtNet.Generation.Filter
{
    using System.Collections.Generic;
    using Ext.Net;
    using GenerationClasses;
    using Tools;
    using System.Text;
    using System.Web.Script.Serialization;
    using System.Web.UI;

    using Nat.Web.Tools.ExtNet;
    using Nat.Web.Tools.ExtNet.Extenders;
    using Nat.Web.Tools.ExtNet.ExtNetConfig;

    class FilterControlComboBox : FilterControlBase
    {
        internal FieldContainer GetControl(
            FilterHtmlGenerator.Filter filterData, 
            bool isAdditionalFieldSet, 
            bool isHideParentFieldSet,
            out string controlMemberJs)
        {
            PickerFieldListeners listeners;
            var selectBox = (ComboBoxBase) (filterData.Lookup 
                              ? GetComboBoxValues(filterData, out listeners)
                              : GetMultiComboBoxValues(filterData, out listeners));

            var listConfig = filterData.ComboBoxView as IListConfig;
            selectBox.InitializeListConfig(listConfig);
            SetSelectConfiguration(filterData, selectBox, listeners);

            var fieldContainer = GetFieldContainerFilter(filterData, isAdditionalFieldSet, isHideParentFieldSet);
            fieldContainer.Items.Add(selectBox);
            controlMemberJs = ClientRepositoryJsMember;
            return fieldContainer;
        }

        private AbstractComponent GetMultiComboBoxValues(FilterHtmlGenerator.Filter filterData, out PickerFieldListeners listeners)
        {
            var multiComboBox = new MultiCombo(StandartConfigMultiComboBoxItems()) 
                {
                    ID = RegistrationControlToRepository(UniquePrefixes.MultiBoxFilterValues, filterData),
                    ClientIDMode = ClientIDMode.Static,
                };

            multiComboBox.ListConfig.Listeners.ItemMouseLeave.Handler = string.Format("{0}.itemLeave = index; window.setTimeout(function() {{ if ({0}.itemLeave == index) {0}.collapse(); }}, 400);", multiComboBox.ClientID);
            multiComboBox.ListConfig.Listeners.ItemMouseEnter.Handler = string.Format("{0}.itemLeave = -1;", multiComboBox.ClientID);
            if (!filterData.IsMultipleSelect)
                multiComboBox.Listeners.BeforeSelect.Handler = "item.clearValue(); window.setTimeout(function() {item.collapse(); }, 100);";
            listeners = multiComboBox.Listeners;
            return multiComboBox;
        }

        private MultiCombo.Config StandartConfigMultiComboBoxItems()
        {
            return new MultiCombo.Config
                {
                    Editable = true,
                    SelectionMode = MultiSelectMode.Selection,
                    Flex = 1,
                    ValueField = "id",
                    DisplayField = "RowName",
                    EmptyText = Controls_ExtNet_Resources.SelectValue,
                    QueryMode = DataLoadMode.Local,
                    ListConfig = new BoundList
                        {
                            TrackOver = true,
                        },
                };
        }

        private void SetSelectConfiguration(
            FilterHtmlGenerator.Filter filterData, 
            ComboBoxBase selectBox, 
            PickerFieldListeners listeners)
        {
            var store = GetStore(filterData);
            selectBox.Store.Add(store);
            SetTriggersToSelectBox(selectBox.Triggers, filterData.Lookup);
            SetListeners(filterData, listeners);
            selectBox.Tag = new
                {
                    browseUrl = GetSerializeBrowseFilterParameters(filterData),
                    tableName = filterData.TableName,
                    header = filterData.Header
                };
        }

        #region Методы реализации

        private AbstractComponent GetComboBoxValues(
            FilterHtmlGenerator.Filter filterData, 
            out PickerFieldListeners listeners)
        {
            var comboBox = new ComboBox(StandartConfigComboBoxItems())
                {
                    ID = RegistrationControlToRepository(UniquePrefixes.ComboBoxFilterValues, filterData),
                    ClientIDMode = ClientIDMode.Static,
                };
            listeners = comboBox.Listeners;
            return comboBox;
        }

        private Store GetStore(FilterHtmlGenerator.Filter filterData)
        {
            var listConfig = filterData.ComboBoxView as IListConfig;
            var store = new Store
                {
                    ID = RegistrationControlToRepository(UniquePrefixes.StoreFilterValues, filterData),
                    ClientIDMode = ClientIDMode.Static,
                };
            store.Model.Add(GetModel(StandartModelFieldsItems(filterData)));
            store.InitializeListConfig(listConfig);

            if (filterData.Lookup)
            {
                store.Proxy.Add(GetProxyLookup(filterData));
                store.Listeners.BeforeLoad.Handler +=
                    string.Format(@"store.getProxy().extraParams.parameters = GetLookupFiltersByValues(App.{0}.tag.browseUrl);", 
                                  GetControlId(UniquePrefixes.ComboBoxFilterValues, filterData.FilterName));
            }
            else
            {
                store.DataSource = GetMultiComboBoxItems(filterData);
                store.AutoDataBind = true;
            }

            return store;
        }

        private void SetListeners(FilterHtmlGenerator.Filter filterData, PickerFieldListeners listeners)
        {
            listeners.Select.Handler = FilterJs.JsCallShowClearTrigger;
            listeners.Change.Handler = FilterJs.JsSetContextToolTipByChagneValueHandler + filterData.OnChangedValue;
            listeners.TriggerClick.Handler = FilterJs.JsCallSelectTriggerClick;

            listeners.BoxReady.Handler = FilterJs.JsCallClearTriggerHide;
        }

        private AbstractProxy GetProxyLookup(FilterHtmlGenerator.Filter filterData)
        {
            // todo: если метод будет расти, рефакторить его.

            const string url = "/AutoCompleteHandler.ashx";
            const string dataSourceType = AutoCompleteHandler.DataSourceType;
            const string suffixTypeName = "JournalDataSourceView";
            const string localizationParameterName = "isKz";
            const string jsonReaderRoot = "Data";
            const string jsonReaderTotalProperty = "Total";
            const string jsonReaderIdProperty = "jReader";

            const string pointSeparator = ".";
            const string commoSeparator = ", ";

            var proxyLookup = new AjaxProxy { Url = url };

            var sourceParameter = new Parameter
            {
                Name = dataSourceType,
                Value = new StringBuilder().Append(filterData.ProjectName)
                                           .Append(pointSeparator)
                                           .Append(filterData.TableName)
                                           .Append(suffixTypeName)
                                           .ToString()
            };

            var localizationParameter = new Parameter
                {
                    Name = localizationParameterName,
                    Value = LocalizationHelper.IsCultureKZ.ToString()
                };

            var reader = new JsonReader
                {
                    Root = jsonReaderRoot,
                    TotalProperty = jsonReaderTotalProperty,
                    IDProperty = jsonReaderIdProperty
                };

            proxyLookup.ActionMethods.Read = HttpMethod.POST;

            proxyLookup.Reader.Add(reader);
            proxyLookup.ExtraParams.Add(sourceParameter);
            proxyLookup.ExtraParams.Add(localizationParameter);

            if (filterData.ComboBoxView != null)
            {
                proxyLookup.ExtraParams.Add(
                    new Parameter
                        {
                            Name = AutoCompleteHandler.ComboBoxView,
                            Value = filterData.ComboBoxView.GetType().FullName,
                        });
            }

            return proxyLookup;
        }

        protected static string GetSerializeBrowseFilterParameters(FilterHtmlGenerator.Filter filterData)
        {
            string serializedData = null;
            if (filterData.BrowseFilterParameters != null)
            {
                var triplet = new Triplet(filterData.BrowseFilterParameters.ClientControlParameters,
                                          filterData.BrowseFilterParameters.ClientValueParameters,
                                          string.Empty);

                serializedData = new JavaScriptSerializer().Serialize(triplet);
            }

            return serializedData;
        }

        #endregion

        #region Стандартная конфигурация

        private ComboBox.Config StandartConfigComboBoxItems()
        {
            return new ComboBox.Config
            {
                Editable = true,
                Flex = 1,
                ValueField = "id",
                DisplayField = "RowName",
                EmptyText = Controls_ExtNet_Resources.SelectValue,
                ForceSelection = true,
                QueryParam = "prefixText",
                QueryMode = DataLoadMode.Remote,
                PageSize = 10,
                MinChars = 1,
                QueryCaching = false,
                Hidden = false,
                ReadOnly = false,
                TypeAhead = true,
                TriggerAction = new TriggerAction(),
                ListConfig = new BoundList
                {
                    LoadingText = Controls.Properties.Resources.SSearchLoading,
                    ItemTpl = new XTemplate
                    {
                        Html = "<div class=\"search-item\">{RowName}</div>",
                    }
                },
            };
        }

        private IEnumerable<ModelField> StandartModelFieldsItems(FilterHtmlGenerator.Filter filterData)
        {
            var modelFieldId = new ModelField("id", ModelFieldType.String)
                {
                    ServerMapping = string.IsNullOrEmpty(filterData.SelectKeyValueColumn)
                                        ? "Value"
                                        : "Item." + filterData.SelectKeyValueColumn
                };

            return new List<ModelField>
                {
                    modelFieldId,
                    new ModelField("RowName", ModelFieldType.String) { ServerMapping = "Name" }
                };
        }

        private void SetTriggersToSelectBox<T>(T triggerCollection, bool isLookup) where T : FieldTrigerCollection
        {
            triggerCollection.Add(GetTrigger(StandartConfigFieldTriggerClear()));
            if (isLookup) triggerCollection.Add(GetTrigger(StandartConfigFieldTriggerDictionaryView()));
        }

        private FieldTrigger GetTrigger(FieldTrigger.Config config)
        {
            return new FieldTrigger(config);
        }

        private FieldTrigger.Config StandartConfigFieldTriggerClear()
        {
            return new FieldTrigger.Config
            {
                Icon = TriggerIcon.Clear,
                Qtip = Controls_ExtNet_Resources.ClearFilter,
                Tag = "clear"
            };
        }

        private FieldTrigger.Config StandartConfigFieldTriggerDictionaryView()
        {
            return new FieldTrigger.Config
            {
                Icon = TriggerIcon.Search,
                Qtip = Controls_ExtNet_Resources.Dictionary,
                Tag = "open"
            };
        }

        #endregion
    }
}