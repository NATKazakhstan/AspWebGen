/*
* Created by: Sergey V. Shpakovskiy
* Created: 2013.03.20
* Copyright © JSC NAT Kazakhstan 2013
*/
namespace Nat.Web.Controls.ExtNet.SelectValues
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Linq;
    using System.Web.UI;
    using System.Web.UI.WebControls;

    using Ext.Net;

    using Nat.Web.Tools.ExtNet;

    [Meta]
    [ControlValuePropertyAttribute("Values")]
    public abstract class BaseMultipleSelect : FieldSet
    {
        private Unit? gridHeight;
        private string onChangedValuesFn;
        private string namingContainerIDOnInit;

        #region Public Properties

        [Description("AllowBlank.")]
        [Meta]
        [Category("MultipleSelect")]
        [DefaultValue(true)]
        public virtual bool AllowBlank
        {
            get { return State.Get(ScriptFieldConstants.AllowBlank, true); }
            set { State.Set(ScriptFieldConstants.AllowBlank, value); }
        }

        [Description("ReadOnly.")]
        [Meta]
        [Category("MultipleSelect")]
        [DefaultValue(false)]
        public virtual bool ReadOnly
        {
            get { return State.Get(ScriptFieldConstants.ReadOnly, false); }
            set { State.Set(ScriptFieldConstants.ReadOnly, value); }
        }

        public override ConfigOptionsCollection ConfigOptions
        {
            get
            {
                ConfigOption configOption;
                var options = base.ConfigOptions;
                options.Add(ScriptFieldConstants.AllowBlank, new ConfigOption(ScriptFieldConstants.AllowBlank, null, null, AllowBlank));
                options.Add(ScriptFieldConstants.ReadOnly, new ConfigOption(ScriptFieldConstants.ReadOnly, null, null, ReadOnly));

                var scriptFunction = GetValueJavaScriptFunction();
                if (!string.IsNullOrEmpty(scriptFunction))
                {
                    configOption = new ConfigOption(
                        ScriptFunctionConstant.GetValue,
                        new SerializationOptions(JsonMode.Raw),
                        null,
                        scriptFunction);
                    options.Add(ScriptFunctionConstant.GetValue, configOption);
                }

                configOption = new ConfigOption(
                    ScriptFunctionConstant.OnChangedValues,
                    new SerializationOptions(JsonMode.Raw),
                    null,
                    string.IsNullOrEmpty(OnChangedValuesFn) ? "null" : OnChangedValuesFn);
                options.Add(ScriptFunctionConstant.OnChangedValues, configOption);

                return options;
            }
        }

        public string DataSelectedKeyField { get; set; }

        public string DataValueField { get; set; }

        public string DataTextField { get; set; }

        public string DataTextFormatString { get; set; }

        public string DataSourceID { get; set; }

        public IDataSource DataSource { get; set; }

        /// <summary>
        /// Имя функции вызываемой, при смене выбранных значений. Передается коллекция выбранных ключей.
        /// </summary>
        public string OnChangedValuesFn
        {
            get
            {
                return onChangedValuesFn;
            }

            set
            {
                onChangedValuesFn = value;
                SetOnChangedValuesFn();
            }
        }

        public Unit? GridHeight
        {
            get
            {
                return gridHeight;
            }

            set
            {
                if (GridPanel != null)
                    GridPanel.Height = value ?? GetDefaultGridHeight();
                gridHeight = value;
            }
        }

        protected virtual Unit GetDefaultGridHeight()
        {
            return new Unit(150, UnitType.Pixel);
        }

        [Localizable(true)]
        public string FieldLabel { get; set; }

        public GridPanel GridPanel { get; protected set; }

        [Description("The data store to use for lookup.")]
        [IDReferenceProperty(typeof(Store))]
        [Meta]
        [Category("MultipleSelect")]
        [DefaultValue("")]
        public virtual string MembersStoreID
        {
            get { return State.Get("MembersStoreID", string.Empty); }
            set { State.Set("MembersStoreID", value); }
        }

        public virtual IEnumerable<string> SelectedValues
        {
            get
            {
                var rowModel = (RowSelectionModel)GridPanel.GetSelectionModel();
                return rowModel.SelectedRows.Select(r => r.RecordID).ToList();
            }

            set
            {
                var rowModel = (RowSelectionModel)GridPanel.GetSelectionModel();
                rowModel.ClearSelection();
                rowModel.Select(value);
            }
        }

        public string Values
        {
            get { return string.Join(",", SelectedValues); }
        }

        [Description("The data store to use for grid.")]
        [IDReferenceProperty(typeof(Store))]
        [Meta]
        [Category("MultipleSelect")]
        [DefaultValue("")]
        public virtual string ValuesStoreID
        {
            get { return State.Get("ValuesStoreID", string.Empty); }
            set { State.Set("ValuesStoreID", value); }
        }

        #endregion

        #region Public Methods and Operators

        protected override void OnInit(EventArgs e)
        {
            Width = new Unit(Width.Value - 13, UnitType.Pixel);
            Title = "<b>" + FieldLabel + "</b>";
            base.OnInit(e);
            namingContainerIDOnInit = NamingContainer.ID;
        }

        protected abstract List<Dictionary<string, object>> GetDataForDelete();

        protected abstract List<Dictionary<string, object>> GetDataForInsert();

        /// <summary>
        /// Функция для получения массива выбранных значений на клиенте.
        /// </summary>
        /// <returns>Возвращает объявление функции.</returns>
        protected abstract string GetValueJavaScriptFunction();

        protected abstract void SetOnChangedValuesFn();

        public abstract void SetFilters(ExtNetBrowseFilterParameters filters);

        public virtual void DeleteValues(IDictionary parentValues)
        {
            var items = GetDataForDelete();
            if (items == null || items.Count == 0)
                return;

            var dataSourceView = GetDataSourceView();
            var keys = new Dictionary<string, object>();

            // todo: подумать что если прийдет id который пользователю не доступен
            foreach (var item in items)
            {
                if (item.ContainsKey(DataValueField))
                    parentValues[DataValueField] = item[DataValueField].ToString();
                keys[DataSelectedKeyField] = item[DataSelectedKeyField].ToString();
                dataSourceView.Delete(keys, parentValues, OnDeleted);
            }
        }

        public virtual void InsertValues(IDictionary parentValues)
        {
            var items = GetDataForInsert();
            if (items == null || items.Count == 0)
                return;

            var dataSourceView = GetDataSourceView();

            foreach (var item in items)
            {
                parentValues[DataValueField] = item[DataValueField].ToString();
                dataSourceView.Insert(parentValues, OnInserted);
            }
        }

        public void UpdateItems(IDictionary parentValues)
        {
            DeleteValues(parentValues);
            InsertValues(parentValues);
        }

        #endregion

        #region Methods

        protected virtual bool OnDeleted(int affectedRecords, Exception exception)
        {
            return false;
        }
        
        protected virtual bool OnInserted(int affectedrecords, Exception exception)
        {
            return false;
        }
        
        private DataSourceView GetDataSourceView()
        {
            var dataSource = DataSource ?? (IDataSource)Parent.FindControl(DataSourceID);
            if (dataSource == null && namingContainerIDOnInit != NamingContainer.ID)
            {
                var container = NamingContainer.FindControl(namingContainerIDOnInit);
                if (container != null)
                    dataSource = (IDataSource)container.FindControl(DataSourceID);
            }
            if (dataSource == null)
                throw new Exception("DataSource not found");

            var dataSourceView = dataSource.GetView("default");
            if (dataSourceView == null)
                throw new Exception("DataSourceView not found");

            return dataSourceView;
        }

        #endregion
    }
}