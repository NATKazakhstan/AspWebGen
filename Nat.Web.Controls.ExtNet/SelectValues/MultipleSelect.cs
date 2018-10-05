/*
* Created by: Sergey V. Shpakovskiy
* Created: 2013.03.20
* Copyright © JSC NAT Kazakhstan 2013
*/

using Nat.Web.Tools;

namespace Nat.Web.Controls.ExtNet.SelectValues
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Web.UI;
    using System.Web.UI.WebControls;

    using Ext.Net;

    using Nat.Web.Controls.GenerationClasses.Data;
    using Nat.Web.Tools.ExtNet;

    using GridView = Ext.Net.GridView;
    using Label = Ext.Net.Label;

    [Meta]
    public class MultipleSelect : BaseMultipleSelect
    {
        #region Public Properties

        public Label LabelErrorText { get; protected set; }

        public new string ClientID => GridPanel.ClientID;

        public override bool ReadOnly
        {
            get => base.ReadOnly;

            set
            {
                base.ReadOnly = value;
                if (GridPanel != null)
                {
                    var selectionModel = (CheckboxSelectionModel)GridPanel.GetSelectionModel();
                    selectionModel.Visible = !ReadOnly;

                    var store = GridPanel.GetStore();
                    store.Filters.Clear();
                    if (ReadOnly)
                        store.Filters.Add(new DataFilter { Property = "Selected", Value = "true" });
                }
            }
        }

        public override bool Disabled
        {
            get => base.Disabled;
            set
            {
                base.Disabled = value;
                GridPanel.Disabled = value;
            }
        }

        protected Dictionary<string, string> LoadedSelection
        {
            get
            {
                var dataSource = (EditableListDataSource)Parent.NamingContainer.FindControl(DataSourceID);
                IEnumerable<EditableListDataSourceView.SelectedItems> result = null;
                dataSource.BaseView.Select(new DataSourceSelectArguments(),
                    delegate(IEnumerable data)
                        {
                            result = data.Cast<EditableListDataSourceView.SelectedItems>();
                        });
                if (result == null)
                    return new Dictionary<string, string>(0);
                return result.Where(r => !string.IsNullOrEmpty(r.SelectedKey) && r.Selected)
                    .ToDictionary(r => r.SelectedKey, r => r.Value);
            }
        }

        public override IEnumerable<string> SelectedValues
        {
            get
            {
                var rowModel = (RowSelectionModel)GridPanel.GetSelectionModel();
                return rowModel.SelectedRows
                               .Select(r => EditableListDataSourceView.SelectedItems.GetValues(r.RecordID).Value)
                               .Where(r => !string.IsNullOrEmpty(r))
                               .ToList();
            }

            set
            {
                var dataSource = (EditableListDataSource)Parent.NamingContainer.FindControl(DataSourceID);
                var view = dataSource.BaseView;
                var values = value.ToDictionary(r => r);
                view.SelectedRow +=
                    delegate(object sender, EditableListDataSourceView.SelectedEventArgs args)
                        {
                            if (values.ContainsKey(args.Value))
                                args.Selected = true;
                        };
            }
        }

        #endregion

        #region Methods

        protected override List<Dictionary<string, object>> GetDataForDelete()
        {
            if (LoadedSelection == null || LoadedSelection.Count == 0)
                return new List<Dictionary<string, object>>();

            var selectionModel = (RowSelectionModel)GridPanel.GetSelectionModel();
            var selected =
                selectionModel.SelectedRows
                              .Select(r => EditableListDataSourceView.SelectedItems.GetValues(r.RecordID))
                              .Where(r => !string.IsNullOrEmpty(r.Key))
                              .ToDictionary(r => r.Key);
            var data = LoadedSelection.Where(r => !selected.ContainsKey(r.Key));

            return data
                .Select(r => new Dictionary<string, object> { { DataSelectedKeyField, r.Key } })
                .ToList();
        }

        protected override List<Dictionary<string, object>> GetDataForInsert()
        {
            var selectionModel = (RowSelectionModel)GridPanel.GetSelectionModel();
            var data = selectionModel.SelectedRows
                                     .Select(
                                         r =>
                                         new
                                             {
                                                 Row = r,
                                                 ID = EditableListDataSourceView.SelectedItems.GetValues(r.RecordID),
                                             })
                                     .Where(r => !string.IsNullOrEmpty(r.ID.Value) && string.IsNullOrEmpty(r.ID.Key))
                                     .ToList();
            return data.Select(r => new Dictionary<string, object> { { DataValueField, r.ID.Value } }).ToList();
        }

        protected override string GetValueJavaScriptFunction()
        {
            return string.Format(
                @"function (){{
    var rows = {0}.selModel.getSelection();
    if (rows == null || rows.length == 0)
        return null; 

    var result = new Array();
    for (var i = 0; i < rows.length; i++) {{
        Array.add(result, rows[i].data.Value);
    }}

    return result;
}}",
                GridPanel.ClientID);
        }

        protected override void SetOnChangedValuesFn()
        {
            if (GridPanel == null)
                return;

            var rowSelectionModel = (RowSelectionModel)GridPanel.GetSelectionModel();
            if (string.IsNullOrEmpty(OnChangedValuesFn))
                rowSelectionModel.Listeners.SelectionChange.Handler = string.Empty;
            else
            {
                rowSelectionModel.Listeners.SelectionChange.Handler =
                    string.Format("{1}.{0}({1}.getValue())", ScriptFunctionConstant.OnChangedValues, base.ClientID);
            }
        }

        public override void SetFilters(ExtNetBrowseFilterParameters filters)
        {
        }

        protected override void OnInit(EventArgs e)
        {
            base.OnInit(e);           

            var width = new Unit(Width.Value - ((Padding ?? 0) * 4), UnitType.Pixel);
            GridPanel = new GridPanel
                {
                    ID = "grid" + ID,
                    StoreID = ValuesStoreID,
                    HideHeaders = true,
                    Height = GridHeight ?? GetDefaultGridHeight(), 
                };
            if (width.Value > 0) GridPanel.Width = width;
            else Layout = "Fit";

            GridPanel.View.Add(new GridView());
            GridPanel.ConfigOptions["fieldLabel"] = new ConfigOption("fieldLabel", null, string.Empty, FieldLabel);
            var checkboxSelectionModel = new CheckboxSelectionModel
                {
                    Mode = SelectionMode.Multi,
                    CheckOnly = true,
                    Visible = !ReadOnly
                };
            GridPanel.SelectionModel.Add(checkboxSelectionModel);

            if (!string.IsNullOrEmpty(OnChangedValuesFn))
                SetOnChangedValuesFn();

            var column = new Column
                {
                    Text = Properties.Resources.SSelectedItems,
                    Flex = 1,
                };

            if (string.IsNullOrEmpty(DataTextFormatString) || "{0}".Equals(DataTextFormatString))
                column.Renderer.Fn = "function (v, p, record, rowIndex) { return Ext.String.format('{0}', record.data.Value0); }";
            else
            {
                var sb = new StringBuilder();
                sb.Append("function (v, p, record, rowIndex) { return Ext.String.format('").Append(DataTextFormatString).Append("'");
                var length = DataTextField.Split(',').Length;
                for (int i = 0; i < length; i++)
                {
                    var formatD = "{" + i + ":d}";
                    var formatD2 = "{" + i + ":dd.MM.yyyy}";
                    if (DataTextFormatString.Contains(formatD) || DataTextFormatString.Contains(formatD2))
                    {
                        var newFormat = "{" + i + "}";
                        sb.Replace(formatD, newFormat);
                        sb.Replace(formatD2, newFormat);
                        sb.Append(", record.data.Value").Append(i)
                          .Append(" == null ? '-' : Date.parseLocale(record.data.Value").Append(i)
                          .Append(").localeFormat('dd.MM.yyyy')");
                    }
                    else
                        sb.Append(", record.data.Value").Append(i);
                }

                sb.Append("); }");
                column.Renderer.Fn = sb.ToString();
            }

            GridPanel.ColumnModel.Add(column);

            LabelErrorText = new Label
                {
                    ID = "labelErrorText" + ID, 
                    Text = string.Format(Web.Controls.Properties.Resources.SRequiredFieldMessage, FieldLabel), 
                    Hidden = true, 
                    Icon = Icon.Error
                };
            LabelErrorText.Style.Add("color", "red");

            Items.Add(GridPanel);
            Items.Add(LabelErrorText);

            var store = GridPanel.GetStore();
            if (ReadOnly)
            {
                store.Filters.Clear();
                store.Filters.Add(new DataFilter { Property = "Selected", Value = "true" });
            }

            AddScripts();

            // Локализация значений
            if (LocalizationHelper.IsCultureKZ)
            {
                foreach (var field in store.Model.Primary.Fields.Where(field => field.ServerMapping.EndsWith("Ru")))
                    field.ServerMapping = field.ServerMapping.Substring(0, field.ServerMapping.Length - 2) + "Kz";
            }
        }

        private void AddScripts()
        {
            var script = string.Format(
                @"
var PrepareMultipleLookupSelect_{4} = function() {{

    var isValid = true;
    var value = {2}.getValue();
    if({2}.allowBlank)
    {{
        {3}.hide();
    }}
    else if (value == null || value.length == 0)
    {{
        {3}.show();
        isValid = false;
    }}
    else
    {{
        {3}.hide();
    }}

return isValid; }}; ",
                GridPanel.GetStore().ClientID,
                string.Empty, // todo: add control
                base.ClientID,
                LabelErrorText.ClientID,
                ID);

            Page.ClientScript.RegisterClientScriptBlock(
                GetType(),
                "PrepareMultipleLookupSelect_" + ID,
                script,
                true);
        }

        #endregion
    }
}