/*
 * Created by: Alexey V. Vorochsak
 * Created: 2013.06.10
 * Copyright © JSC NAT Kazakhstan 2013
 */

using Nat.Tools;
using Nat.Web.Controls.ExtNet.Properties;

namespace Nat.Web.Controls.ExtNet.Generation.Filter
{
    using System;
    using System.ComponentModel;
    using System.Text;
    using System.Web.UI.WebControls;
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;
    using System.Web.UI;
    using Ext.Net;
    using GenerationClasses;
    using GenerationClasses.BaseJournal;

    using Nat.Web.Tools.ExtNet.ExtNetConfig;

    using Tools.Initialization;

    public class FilterControlBase
    {
        private StringBuilder _itemBuilder = new StringBuilder();
        private StringBuilder _clientRepositoryBuilder;
        protected string ClientRegistratorToRepository { get; private set; }

        protected string ClientRepositoryJsMember
        {
            get
            {
                var member = _clientRepositoryBuilder.ToString();
                _clientRepositoryBuilder.Clear();
                return member;
            }

            set
            {
                if (_clientRepositoryBuilder == null)
                {
                    _clientRepositoryBuilder = new StringBuilder();
                    _clientRepositoryBuilder.Append(FilterJs.InitialPartRepositoryMember);
                    ClientRegistratorToRepository = string.Empty;
                }

                _clientRepositoryBuilder.Append(value);
            }
        }

        protected FieldContainer GetFieldContainerFilter(
            FilterHtmlGenerator.Filter filterData,
            bool isAdditionalFieldSet,
            bool isHideParentFieldSet,
            bool isRangeControlId = false)
        {
            var storeFilter = GetStoreFilter(filterData);
            var selectBoxFilter = GetSelectBox(filterData, storeFilter, isRangeControlId, isAdditionalFieldSet, isHideParentFieldSet);
            return GetFieldContainer(filterData, selectBoxFilter);
        }

        #region Методы реализации базовой структуры фильтра

        private Store GetStoreFilter(FilterHtmlGenerator.Filter filterData)
        {
            var store = new Store(StandartConfigStoreFilter(filterData))
                { 
                    ID = RegistrationControlToRepository(UniquePrefixes.StoreFilterOperations, filterData), 
                    ClientIDMode = ClientIDMode.Static,
                };
            store.Model.Add(GetModelStoreFilter());
            return store;
        }

        private SelectBox GetSelectBox(
            FilterHtmlGenerator.Filter filterData,
            Store storeFilter,
            bool isRangeControlId,
            bool isAdditionalFieldSet,
            bool isHideParentFieldSet)
        {
            var selectBox = new SelectBox(StandartConfigSelectBoxFilter())
                {
                    ID = RegistrationControlToRepository(UniquePrefixes.SelectBoxFilterOperations, filterData, isAdditionalFieldSet, isHideParentFieldSet),
                    ClientIDMode = ClientIDMode.Static,
                };
            if (filterData.Type == FilterHtmlGenerator.FilterType.Boolean)
            {
                selectBox.Triggers.Add(new FieldTrigger()
                {
                    Icon = TriggerIcon.Clear,
                    Qtip = Resources.SRemoveSelected
                });
                selectBox.Listeners.TriggerClick.Handler = @"
                            this.clearValue();";
            }
            selectBox.Store.Add(storeFilter);
            SetListenersSelectBox(filterData, isRangeControlId, selectBox);
            return selectBox;
        }

        private FieldContainer GetFieldContainer(
            FilterHtmlGenerator.Filter filterData,
            SelectBox selectBox)
        {
            var fieldContainer = new FieldContainer(StandartConfigFilterFieldContainer(filterData))
                {
                    ID = RegistrationControlToRepository(UniquePrefixes.FieldContainerCommon, filterData),
                    ClientIDMode = ClientIDMode.Static,
                };
            fieldContainer.Items.Add(selectBox);
            return fieldContainer;
        }

        private void SetListenersSelectBox(
            FilterHtmlGenerator.Filter filterData,
            bool isRangeControlId,
            SelectBox selectBox)
        {
            if (isRangeControlId)
            {
                selectBox.Listeners.Change.Handler = FilterJs.JsCallToggleVisibleSecondRangeControl;
            }
        }

        private static Model GetModelStoreFilter()
        {
            return GetModel(StandartModelFieldsFilter());
        }

        #endregion

        #region Стандартная конфигурация контролов фильтра

        private static FieldContainer.Config StandartConfigFilterFieldContainer(
            FilterHtmlGenerator.Filter filterData)
        {
            var fieldContainerConfig = new FieldContainer.Config
            {
                FieldLabel = filterData.Header,
                LabelAlign = LabelAlign.Right,
                LabelWidth = 146,
                Layout = "HBoxLayout",
                MarginSpec = "5 20 5 5",
                Flex = 1,
                LabelStyle = "white-space: nowrap !important; text-overflow: ellipsis; overflow: hidden;",
            };

            return fieldContainerConfig;
        }

        private static Store.Config StandartConfigStoreFilter(FilterHtmlGenerator.Filter filterData)
        {
            return new Store.Config
            {
                DataSource = GetOperationsFilter(filterData),
                AutoDataBind = true,
            };
        }

        private static SelectBox.Config StandartConfigSelectBoxFilter()
        {
            return new SelectBox.Config
            {
                Width = Unit.Pixel(150),
                MarginSpec = "0 5 0 0",
                ValueField = "Code",
                DisplayField = "Name",
                EmptyText = Controls_ExtNet_Resources.SelectValue,
            };
        }

        private static IEnumerable<ModelField> StandartModelFieldsFilter()
        {
            return new List<ModelField>
            {
                new ModelField("Code", ModelFieldType.String),
                new ModelField("Name", ModelFieldType.String),
            };
        }

        #endregion

        #region Наследуемые методы

        protected string RegistrationControlToRepository(
            UniquePrefixes prefix,
            FilterHtmlGenerator.Filter filterData,
            bool isAdditionalFieldSet = false,
            bool isHideParentFieldSet = false,
            bool isRangeControl = false)
        {
            var controlId = GetControlId(prefix, filterData.FilterName);
            PackingJsMember(prefix, filterData, controlId, isRangeControl, isAdditionalFieldSet, isHideParentFieldSet);
            return controlId;
        }

        public static string GetControlId(UniquePrefixes prefix, string filterName)
        {
            const string spaceSeparator = "_";
            var controlId = new StringBuilder().Append(prefix)
                                               .Append(spaceSeparator)
                                               .Append(filterName)
                                               .ToString();
            return controlId;
        }

        protected static object GetMultiComboBoxItems(FilterHtmlGenerator.Filter filterData)
        {
            var view = filterData.DataSource != null
                ? filterData.DataSource.GetView(filterData.TableName)
                : filterData.GetDataSourceView();

            if (view == null)
                return null;

            var dataSourceSelectArguments = new DataSourceSelectArguments { StartRowIndex = 0, MaximumRows = 100 };
            IEnumerable listValues = null;
            view.Select(dataSourceSelectArguments, r => listValues = r);
            return listValues.Cast<object>().ToArray();
        }

        private static object GetValue(object obj, string keyFieldName)
        {
            foreach (var property in keyFieldName.Split('.'))
            {
                if (obj == null)
                    return null;
                var properties = TypeDescriptor.GetProperties(obj);
                var descriptor = properties[property];

                if (descriptor == null)
                {
                    var message = string.Format("Не найдено свойство {0} у типа {1}", property, obj.GetType().FullName);
                    throw new ArgumentException(message, keyFieldName);
                }

                obj = descriptor.GetValue(obj);
            }

            return obj;
        }

        #endregion

        #region Методы формирования данных репозитория фильтра для передачи на клиент

        private void PackingJsMember(
            UniquePrefixes prefix,
            FilterHtmlGenerator.Filter filterData,
            string controlId,
            bool isRangeControl,
            bool isAdditionalFieldSet,
            bool isHideParentFieldSet)
        {
            var typePrefix = prefix.GetType();
            var fieledInfo = typePrefix.GetField(prefix.ToString());
            var filterAttributes = fieledInfo.GetCustomAttributes(false);

            foreach (var typeFilterAttribute in filterAttributes.Select(filterAttribute => filterAttribute.GetType()))
            {
                if (typeFilterAttribute == typeof(FilterTypeIdAttribute))
                {
                    AddItemToJsMember(JsMemberItemTypes.FilterTypeId, controlId);
                    AddItemToJsMember(JsMemberItemTypes.FilterName, filterData.FilterName);
                    AddItemToJsMember(JsMemberItemTypes.DefaultFilterType, filterData.DefaultFilterType == null ? null : filterData.DefaultFilterType.ToString());
                    AddItemToJsMember(JsMemberItemTypes.IsHiddenParentFieldSet, isAdditionalFieldSet.ToString());
                    AddItemToJsMember(JsMemberItemTypes.IsMain, filterData.MainGroup.ToString());
                    AddItemToJsMember(JsMemberItemTypes.RequiredFilter, filterData.RequiredFilter);
                    AddItemToJsMember(JsMemberItemTypes.DependedFilters, filterData.DependedFilters);
                    AddItemToJsHandler(JsMemberItemTypes.GetDependedVisible, filterData.DependedHandler, "values");
                    AddItemToJsMember(JsMemberItemTypes.IsHideParentGroupFieldSet, isHideParentFieldSet.ToString());
                }

                if (typeFilterAttribute == typeof(RangeFilterValueIdAttribute))
                {
                    AddItemToJsMember(JsMemberItemTypes.SecondFilterValueId, controlId);
                }

                if (typeFilterAttribute == typeof(FilterValueIdAttribute))
                {
                    AddItemToJsMember(JsMemberItemTypes.FirstFilterValueId, controlId);
                    
                    if (!isRangeControl) AddItemToJsMember(JsMemberItemTypes.SecondFilterValueId, string.Empty);
                }

                if (typeFilterAttribute == typeof(CommonContainerAttribute))
                {
                    SetFinalParthToJsMember();
                }
            }
        }

        private void AddItemToJsHandler(JsMemberItemTypes itemType, string itemValue, string parameters)
        {
            if (string.IsNullOrEmpty(itemValue))
                return;

            _itemBuilder
                .Clear()
                .Append(", ")
                .Append(itemType)
                .Append(":");
            if (itemValue.Contains(" "))
            {
                _itemBuilder
                    .Append("function(")
                    .Append(parameters)
                    .Append(") { ")
                    .Append(itemValue)
                    .Append("}");
            }
            else
                _itemBuilder.Append(itemValue);

            ClientRepositoryJsMember = _itemBuilder.ToString();
        }

        private void SetFinalParthToJsMember()
        {
            ClientRepositoryJsMember = FilterJs.FinalPartJsMember;
        }

        private void AddItemToJsMember(JsMemberItemTypes itemType, string itemValue)
        {
            const string commaSeparator = ", ";
            ClientRepositoryJsMember = commaSeparator;
            ClientRepositoryJsMember = GetJsItem(itemType, itemValue);
        }

        private string GetJsItem(JsMemberItemTypes itemType, string itemValue)
        {
            const string colonSeparator = ":";
            const string quotesSeparator = "\"";

            return _itemBuilder.Clear()
                              .Append(itemType)
                              .Append(colonSeparator)
                              .Append(quotesSeparator)
                              .Append(itemValue)
                              .Append(quotesSeparator)
                              .ToString();
        }

        private void AddItemToJsMember(JsMemberItemTypes itemType, string[] itemValue)
        {
            const string commaSeparator = ", ";
            ClientRepositoryJsMember = commaSeparator;
            ClientRepositoryJsMember = GetJsItem(itemType, itemValue);
        }

        private string GetJsItem(JsMemberItemTypes itemType, string[] itemValue)
        {
            const string colonSeparator = ":";
            const string quotesSeparator = "\"";

            _itemBuilder.Clear()
                .Append(itemType)
                .Append(colonSeparator)
                .Append("[");
            if (itemValue != null)
            {
                foreach (var value in itemValue)
                {
                    _itemBuilder.Append(quotesSeparator)
                        .Append(value)
                        .Append(quotesSeparator);
                }
            }

            return _itemBuilder.Append("]").ToString();
        }

        private void AddItemToJsMember(JsMemberItemTypes itemType, bool itemValue)
        {
            const string commaSeparator = ", ";
            ClientRepositoryJsMember = commaSeparator;
            ClientRepositoryJsMember = GetJsItem(itemType, itemValue);
        }

        private string GetJsItem(JsMemberItemTypes itemType, bool itemValue)
        {
            const string colonSeparator = ":";

            return _itemBuilder.Clear()
                              .Append(itemType)
                              .Append(colonSeparator)
                              .Append(itemValue.ToString().ToLower())
                              .ToString();
        }

        #endregion

        #region Локальные методы

        private static IEnumerable ConvertToObjectsArray(IEnumerable<KeyValuePair<object, object>> list)
        {
            return list.Select(o => new[] { o.Key, o.Value }).ToArray();
        }

        private static IEnumerable GetOperationsFilter(FilterHtmlGenerator.Filter filterData)
        {
            IEnumerable<KeyValuePair<object, object>> list = new List<KeyValuePair<object, object>>();

            switch (filterData.Type)
            {
                case FilterHtmlGenerator.FilterType.Reference:
                {
                    list = filterData.FilterByStartsWithCode
                        ? InitializerSection.StaticFilterNamesResources
                            .ReferenceListWithTextFilterForCodeTypes
                        : InitializerSection.StaticFilterNamesResources.ReferencesFilterTypes;

                    var bfilter = filterData as BaseFilterParameter;
                    if (bfilter != null && bfilter.OTextValueExpressionRu == null)
                        list = list.Where(r => !((string) r.Key).EndsWith("ByRef")).ToList();

                    break;
                }
                case FilterHtmlGenerator.FilterType.Numeric:
                {
                    list = filterData.IsDateTime
                        ? InitializerSection.StaticFilterNamesResources.DatetimeFilterTypes
                        : InitializerSection.StaticFilterNamesResources.NumericFilterTypes;
                    break;
                }
                case FilterHtmlGenerator.FilterType.Boolean:
                {
                    var filterList = new List<KeyValuePair<object, object>>();
                    foreach (
                        var referenceFilterType in InitializerSection.StaticFilterNamesResources.ReferenceFilterTypes)
                    {
                        if (referenceFilterType.Key.Equals("Equals"))
                            filterList.Add(new KeyValuePair<object, object>("Equals", filterData.TrueBooleanText));
                        else if (referenceFilterType.Key.Equals("NotEquals"))
                            filterList.Add(new KeyValuePair<object, object>("NotEquals", filterData.FalseBooleanText));
                        else filterList.Add(referenceFilterType);
                    }

                    list = filterList;
                    break;
                }
                case FilterHtmlGenerator.FilterType.Text:
                {
                    list = InitializerSection.StaticFilterNamesResources.TextFilterTypes;
                    break;
                }
                case FilterHtmlGenerator.FilterType.FullTextSearch:
                {
                    var filterList = new List<KeyValuePair<object, object>>();
                    filterList.Add(new KeyValuePair<object, object>(DefaultFilters.TextFilter.Contains.ToString(),
                        InitializerSection.StaticFilterNamesResources.SContains));
                    list = filterList;
                    break;
                }
            }

            if (filterData.AllowedFilterTypes != null)
            {
                list = list.Where(r => filterData.AllowedFilterTypes.Contains(r.Key));
                var add = filterData.AllowedFilterTypes.Where(r => list.All(f => (string) f.Key != r));
                var newList = list.ToList();
                foreach (var item in add)
                {
                    var referenceType =
                        (DefaultFilters.ReferenceFilter) Enum.Parse(typeof(DefaultFilters.ReferenceFilter), item);
                    switch (referenceType)
                    {
                        case DefaultFilters.ReferenceFilter.MoreOrEqual:
                            newList.Add(new KeyValuePair<object, object>(item,
                                InitializerSection.StaticFilterNamesResources.SMore));

                            break;
                        case DefaultFilters.ReferenceFilter.LessOrEqual:
                            newList.Add(new KeyValuePair<object, object>(item,
                                InitializerSection.StaticFilterNamesResources.SLess));

                            break;
                        default:
                            throw new ArgumentOutOfRangeException();
                    }
                }

                list = newList;
            }

            return ConvertToObjectsArray(list);
        }

        #endregion

        #region Базовые наследуемые методы

        protected static Model GetModel(IEnumerable<ModelField> modelFields)
        {
            var model = new Model();
            foreach (var field in modelFields)
            {
                model.Fields.Add(field);
            }

            return model;
        }

        #endregion

        #region Перечисления для формирования уникальных имен контролов и регистрации их в клиентском репозитории

        public enum UniquePrefixes
        {
            FiltersPanel,

            [CommonContainer]
            FieldContainerCommon,

            [FilterTypeId]
            SelectBoxFilterOperations,
            StoreFilterOperations,

            BooleanGroup,
            BooleanMemberYes,
            BooleanMemberNo,

            [FilterValueId]
            BeginDateField,
            [RangeFilterValueId]
            EndDateField,

            [FilterValueId]
            BeginNumberField,
            [RangeFilterValueId]
            EndNumberField,

            [FilterValueId]
            MultiBoxFilterValues,
            [FilterValueId]
            ComboBoxFilterValues,
            StoreFilterValues,

            [FilterValueId]
            TextFieldFilterValues
        }

        private enum JsMemberItemTypes
        {
            FilterName,
            FilterTypeId,
            FirstFilterValueId,
            SecondFilterValueId,
            IsMain,
            IsHiddenParentFieldSet,
            IsHideParentGroupFieldSet,
            DefaultFilterType,
            RequiredFilter,
            DependedFilters,
            GetDependedVisible,
        }

        #endregion
    }

    #region Аттрибуты для формирования клиентского репозитория

    internal class FilterValueIdAttribute : Attribute { }

    internal class RangeFilterValueIdAttribute : Attribute { }

    internal class FilterTypeIdAttribute : Attribute { }

    internal class CommonContainerAttribute : Attribute { }

    #endregion
}
