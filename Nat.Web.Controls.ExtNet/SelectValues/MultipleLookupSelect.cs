/*
* Created by: Sergey V. Shpakovskiy
* Created: 2013.03.20
* Copyright © JSC NAT Kazakhstan 2013
*/

using System.Linq;

namespace Nat.Web.Controls.ExtNet.SelectValues
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Text;
    using System.Web;
    using System.Web.Compilation;
    using System.Web.Script.Serialization;
    using System.Web.UI;
    using System.Web.UI.WebControls;

    using Ext.Net;

    using Nat.Web.Controls.GenerationClasses;
    using Nat.Web.Controls.GenerationClasses.Data;
    using Nat.Web.Tools;
    using Nat.Web.Tools.ExtNet;
    using Nat.Web.Tools.ExtNet.Extenders;
    using Nat.Web.Tools.ExtNet.ExtNetConfig;

    using Newtonsoft.Json;

    using GridView = Ext.Net.GridView;
    using Label = Ext.Net.Label;

    [Meta]
    public class MultipleLookupSelect : BaseMultipleSelect
    {
        #region Private Properties

        private Container _containerLookupBox;

        #endregion

        #region Public Properties

        public LookupBox LookupBox { get; protected set; }

        public CommandColumn DeleteCommand { get; set; }

        public Label LabelErrorText { get; protected set; }

        public Hidden HiddenDeletedValues { get; protected set; }

        protected Hidden HiddenFilters { get; set; }

        public Hidden HiddenInsertedValues { get; protected set; }

        public bool AllowDublicateSelect { get; set; }
        public Label BeforeElementText { get; protected set; }
        protected override void SetOnChangedValuesFn()
        {
        }
        
        public override void SetFilters(ExtNetBrowseFilterParameters filters)
        {
            if (!AllowDublicateSelect)
                filters.AddControlParamerter("id.NotEqualsCollection", this);

            var jss = new JavaScriptSerializer();
            HiddenFilters.SetValue(jss.Serialize(new Triplet(filters.ClientControlParameters, filters.ClientValueParameters, null)));
        }

        public override bool ReadOnly
        {
            get
            {
                return base.ReadOnly;
            }

            set
            {
                base.ReadOnly = value;
                if (LookupBox != null)
                {                    
                    LookupBox.ReadOnly = ReadOnly;
                    LookupBox.Hidden = ReadOnly;
                    _containerLookupBox.Hidden = ReadOnly;
                }

                if (DeleteCommand != null)
                    DeleteCommand.Hidden = ReadOnly;
                // todo: изменить скрипт по удалению или скрыть кнопку
            }
        }

        public string ComboboxView { get; set; }

        private IListConfig ComboboxViewListConfig { get; set; }

        #endregion

        #region Methods

        /// <summary>
        /// Добавление записей в список.
        /// </summary>
        /// <param name="id">Идентификатор записи справочника.</param>
        /// <param name="name">Наименование записи справочника.</param>
        public void AddRow(long id, string name)
        {
            GridPanel.GetStore().Listeners.Load.Handler += string.Format(@"
                if (window.added{5})
                    return;
                var defaultData = {{RowName: '{3}', Value: {4}}};
                var newRecord = #{{{0}}}.insert(0, defaultData);
                var field = #{{{1}}}; 
                var fn = field.{2}; 
                if (fn != null)
                    fn(field.getValue());
                window.added{5} = true;
",
                ValuesStoreID,
                ID,
                ScriptFunctionConstant.OnChangedValues,
                HttpUtility.JavaScriptStringEncode(name),
                id,
                Guid.NewGuid().ToString("N"));
        }

        protected override void OnInit(EventArgs e)
        {
            base.OnInit(e);

            if (!string.IsNullOrEmpty(ComboboxView))
            {
                var type = BuildManager.GetType(ComboboxView, true, true);
                ComboboxViewListConfig = (IListConfig)Activator.CreateInstance(type);
            }

            var width = new Unit(Width.Value - ((Padding ?? 0) * 4), UnitType.Pixel);
            LookupBox = new LookupBox
                       {
                           ID = "combo" + ID,
                           DisplayField = "RowName",
                           ValueField = "id",
                           StoreID = MembersStoreID,
                           QueryCaching = false,
                           Hidden = ReadOnly,
                           ReadOnly = ReadOnly,
                           MultipleSelect = true,
                       };
            LookupBox.InitializeListConfig(ComboboxViewListConfig);

            _containerLookupBox = new Container
            {
                ID = "container" + LookupBox.ID,                
            };            

            if (width.Value > 0) LookupBox.Width = width;
            else _containerLookupBox.Layout = "Fit";

            _containerLookupBox.Items.Add(LookupBox); 

            GridPanel = new GridPanel
                {
                    ID = "grid" + ID, 
                    StoreID = ValuesStoreID,
                    HideHeaders = true,
                    Height = GridHeight ?? GetDefaultGridHeight(), 
                };

            var containerGridPanel = new Container
            {
                ID = "container" + GridPanel.ID,                
            };

            if (width.Value > 0) GridPanel.Width = width;
            else containerGridPanel.Layout = "Fit";

            containerGridPanel.Items.Add(GridPanel); 

            GridPanel.View.Add(new GridView());
            GridPanel.SelectionModel.Add(new RowSelectionModel { Mode = SelectionMode.Multi });

            if (!string.IsNullOrEmpty(OnChangedValuesFn))
                SetOnChangedValuesFn();

            if (!string.IsNullOrEmpty(MembersStoreID))
            {
                LookupBox.Listeners.Change.Handler = string.Format(
                    @"
if (newValue != null && newValue != '') {{
    var newRow = #{{{3}}}.getById(newValue);
    if (newRow != null) {{
        var defaultData = {{RowName: newRow.data.RowName, Value: newRow.data.id}};
        var newRecord = #{{{0}}}.insert(0, defaultData);
        var field = #{{{1}}}; 
        var fn = field.{2}; 
        if (fn != null)
            fn(field.getValue());
        #{{{4}}}.setValue(null);
    }}
}}",
                    ValuesStoreID,
                    ID,
                    ScriptFunctionConstant.OnChangedValues,
                    MembersStoreID,
                    LookupBox.ID);
            }

            DeleteCommand = GetDeleteCommand();
            GridPanel.ColumnModel.Add(DeleteCommand);
            GridPanel.ColumnModel.Add(
                new Column
                    {
                        DataIndex = "RowName", 
                        Text = Properties.Resources.SSelectedItems, 
                        Flex = 1, 
                        Wrap = true,
                    });
            
            HiddenInsertedValues = new Hidden { ID = "hiddenInsertedValues" + ID };
            HiddenDeletedValues = new Hidden { ID = "hiddenDeletedValues" + ID };
            HiddenFilters = new Hidden { ID = ID + "Filters" };

            LabelErrorText = new Label
                {
                    ID = "labelErrorText" + ID,
                    Text = string.Format(Web.Controls.Properties.Resources.SRequiredFieldMessage, FieldLabel),
                    Hidden = true,
                    Icon = Icon.Error
                };
            LabelErrorText.Style.Add("color", "red");

            BeforeElementText = new Label
            {
                ID = "beforeElementText" + ID,
                Hidden = true
            };
            BeforeElementText.Style.Add("color", "blue");

            /*Items.Add(LookupBox);
            Items.Add(GridPanel);*/
            Items.Add(BeforeElementText);
            Items.Add(_containerLookupBox);
            Items.Add(containerGridPanel);

            Items.Add(HiddenInsertedValues);
            Items.Add(HiddenDeletedValues);
            Items.Add(HiddenFilters);
            Items.Add(LabelErrorText);

            var store = LookupBox.GetStore();
            store.Listeners.BeforeLoad.Handler +=
                string.Format(
                    @"store.getProxy().extraParams.parameters = GetExtNetLookupFilters(#{{{0}}});", HiddenFilters.ID);
            store.InitializeListConfig(ComboboxViewListConfig);

            LookupBox.LookupFiltersID = HiddenFilters.ID;
        }

        protected override void OnPreRender(EventArgs e)
        {
            base.OnPreRender(e);
            AddBeforeSaveClientScript();
            var store = LookupBox.GetStore();
            if (store == null || store.Proxy == null || store.Proxy.Primary == null)
                return;

            var proxy = (AjaxProxy)store.Proxy.Primary;
            var dataSourceTypeName = proxy.ExtraParams["dataSourceType"];

            if (string.IsNullOrEmpty(dataSourceTypeName))
                return;

            var sourceView = (BaseDataSourceView<long>)Activator.CreateInstance(BuildManager.GetType(dataSourceTypeName, true, true));
            LookupBox.TableName = sourceView.TableName;
            LookupBox.WindowTitle = ((IHeaderControl)sourceView).Header;
        }

        private void AddBeforeSaveClientScript()
        {
            var valueColumns = GetKeyFieldsForScript(DataValueField);
            var selectedColumns = GetKeyFieldsForScript(DataSelectedKeyField);

            var script = string.Format(
                @"
var PrepareMultipleLookupSelect_{5} = function() {{
var dataArray = [];
var store = #{{{0}}};
var newRecords = store.getNewRecords();
Ext.each(newRecords, function(record){{
    dataArray.push({6});
}});
#{{{1}}}.setValue(Ext.encode(dataArray));

dataArray = [];
var removedRecords = store.getRemovedRecords();
Ext.each(removedRecords, function(record){{
    dataArray.push({7});
}});
#{{{2}}}.setValue(Ext.encode(dataArray));
                                        
var isValid = true;
var errorLabel = #{{{4}}};
if(#{{{3}}}.allowBlank)
{{
    errorLabel.hide();
}}
else if (store.getCount() == 0)
{{
    errorLabel.show();
    isValid = false;
}}
else
{{
    errorLabel.hide();
}}

return isValid; }}; ",
                GridPanel.GetStore().ID,
                HiddenInsertedValues.ID,
                HiddenDeletedValues.ID,
                ID,
                LabelErrorText.ID,
                ID,
                valueColumns,
                selectedColumns);
            Ext.Net.X.AddScript(script);
        }
        
        private CommandColumn GetDeleteCommand()
        {
            var deleteCommand = new CommandColumn
                {
                    ID = "delCommand" + ID,
                    Width = 24,
                    Enabled = !ReadOnly,
                };
            var editCommand = new GridCommand
                {
                    CommandName = "Delete", 
                    Icon = Icon.Cross, 
                };
            editCommand.ToolTip.Text = Web.Controls.Properties.Resources.SDeleteText;
            deleteCommand.Commands.Add(editCommand);
            deleteCommand.Listeners.Command.Handler =
                string.Format(
                    "#{{{0}}}.remove(record); var field = #{{{1}}}; var fn = field.{2}; if (fn != null) fn(field.getValue());",
                    ValuesStoreID,
                    ID,
                    ScriptFunctionConstant.OnChangedValues);
            deleteCommand.Hidden = ReadOnly;
            return deleteCommand;
        }

        protected override List<Dictionary<string, object>> GetDataForDelete()
        {
            if (string.IsNullOrEmpty((string)HiddenDeletedValues.Value))
                return null;

            return JsonConvert.DeserializeObject<List<Dictionary<string, object>>>(
                (string)HiddenDeletedValues.Value);
        }

        protected override List<Dictionary<string, object>> GetDataForInsert()
        {
            if (string.IsNullOrEmpty((string)HiddenInsertedValues.Value))
                return null;

            return JsonConvert.DeserializeObject<List<Dictionary<string, object>>>(
                (string)HiddenInsertedValues.Value);
        }

        public override IEnumerable<string> SelectedValues
        {
            get
            {
                var dataSource = (EditableListDataSource)GridPanel.FindControl(GridPanel.GetStore().DataSourceID);
                IEnumerable<EditableListDataSourceView.SelectedItems> result = null;
                dataSource.BaseView.Select(new DataSourceSelectArguments(),
                    delegate (IEnumerable data)
                    {
                        result = data.Cast<EditableListDataSourceView.SelectedItems>();
                    }
                );

                var deletedValues = !string.IsNullOrEmpty((string)HiddenDeletedValues.Value)
                    ? JsonConvert
                        .DeserializeObject<List<Dictionary<string, object>>>((string)HiddenDeletedValues.Value)
                        .SelectMany(r => r.Values.Select(q => q.ToString()))
                        .ToArray()
                    : new string[0];

                var selectedValues = result.Where(r =>
                        !string.IsNullOrEmpty(r.SelectedKey) && r.Selected && !deletedValues.Contains(r.SelectedKey))
                    .Select(r => r.Value).ToList();

                var insertedValues = !string.IsNullOrEmpty((string)HiddenInsertedValues.Value) ?
                    JsonConvert.DeserializeObject<List<Dictionary<string, object>>>((string)HiddenInsertedValues.Value)
                        .SelectMany(r => r.Values.Select(q => q.ToString()))
                        .ToArray()
                    : new string[0];

                selectedValues.AddRange(insertedValues);

                return selectedValues;
            }
        }

        protected override string GetValueJavaScriptFunction()
        {
            return string.Format(
                @"function (){{
    var records = {0}.getRecordsValues();
    if (records == null || records.length == 0)
        return null;

    var result = new Array();
    for (var i = 0; i < records.length; i++) {{
        Array.add(result, records[i].Value);
    }}

    return result;
}}",
                GridPanel.GetStore().ClientID);
        }

        private string GetKeyFieldsForScript(string keyFields)
        {
            var sb = new StringBuilder();
            sb.Append("{");

            var valueKeyArray = keyFields.Split(',');
            foreach (string field in valueKeyArray)
            {
                sb.Append("'").Append(field).Append("': record.data.").Append(field);
            }

            sb.Append("}");
            return sb.ToString();
        }

        #endregion
    }
}