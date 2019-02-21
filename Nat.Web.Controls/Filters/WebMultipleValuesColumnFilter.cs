/*
 * Created by: Roman V. Kurbangaliev
 * Created: 4 апреля 2008 г.
 * Copyright © JSC New Age Technologies 2008
 */

using System;
using System.Collections.Generic;
using System.Data;
using System.Text;
using System.Web.UI;
using System.Web.UI.WebControls;
using Nat.Controls.DataGridViewTools;
using Nat.Tools;
using Nat.Tools.Constants;
using Nat.Tools.Data;
using Nat.Tools.Filtering;
using Nat.Tools.ResourceTools;
using Nat.Tools.Validation;
using Nat.Web.Controls;
using Nat.Web.Controls.Properties;
using Nat.Web.Tools;

namespace Nat.Web.Controls.Filters
{
    using System.Drawing;
    using System.Linq;
    using System.Web;

    using Nat.Tools.Classes;
    using Nat.Tools.QueryGeneration;

    public class WebMultipleValuesColumnFilter : WebControl, IMultipleValuesColumnFilter, ISupportSessionWorker,
        ISupportLog, IColumnFilterStorageChanged, IDefaultFilterValues, INamingContainer, ISupportPostBack
    {
        #region Fields

        private ColumnFilterStorage _storage;
        private Panel panel;
        private GridViewExt gridViewExt;
        TextBox QuickSearchTextBox;
        HiddenField checkedIds;
        Button quickSearchButton;
        private string errorText;
        private TableDataSource tableDataSource;
        private object dataSource;
        private string dataMember;
        private bool emptyValidation;
        private ColumnFilterType columnFilterType;
        private string valueMember;
        private SessionWorker sessionWorker;
        private bool _gridViewCreated;
        private bool _inited;
        private bool _inCreateGridView;
        private string displayMember;
        private ScriptManager _ScriptManager;
        private bool checkedIdsCleared;

        private IColumnFilterStorageChanged _columnFilterStorageChangedImplementation;

        #endregion

        public event EventHandler<EventArgs> ColumnFilterStorageChanged;

        public WebMultipleValuesColumnFilter()
        {
            _storage = new ColumnFilterStorage();
            _storage.DataType = typeof(Int64);
            _storage.AvailableFilters = ColumnFilterType.In | ColumnFilterType.Equal | ColumnFilterType.None;
            PanelHeight = Unit.Pixel(380);
        }

        public Unit PanelHeight { get; set; }

        public bool AllowNone { get; set; }

        public ScriptManager ScriptManager
        {
            get
            {
                if (_ScriptManager == null)
                    _ScriptManager = ScriptManager.GetCurrent(Page);
                return _ScriptManager;
            }
        }
        private void Initialization()
        {
            if (tableDataSource == null)
            {
                DataTable filterRefTable = DataSourceHelper.GetDataTable(DataSource);

                // Initialization SessionWorker
                if (SessionWorker == null)
                {
                    var sw = new SessionWorker { Key = Guid.NewGuid().ToString() };
                    sw.SetSession(HttpContext.Current.Session);
                    sw.Object = filterRefTable.DataSet;
                    SessionWorker = sw;
                }

                // Initialization tableDataSource                        
                Type tableAdapterType =
                    TableAdapterTools.GetTableAdapterType(filterRefTable.GetType());
                tableDataSource = new TableDataSource
                    {
                        ID = "tableDataSource_ID",
                        TypeName = tableAdapterType.FullName,
                        SelectMethod = this._storage.SelectMethod,
                        FillType = TableDataSourceFillType.ParametersNotChanged,
                        SessionWorker = this.SessionWorker,
                        SetFilterByCustomConditions = false,
                    };
                tableDataSource.View.CustomConditions.AddRange(_storage.CustomConditions);                
            }
        }

        protected virtual void CreateGridView()
        {
            if (_storage == null)
                throw new NullReferenceException("ColumnFilterStorage must be specified");

            var table = new Table();
            Controls.Add(table);
            table.Width = Width;
            table.Style["table-layout"] = "fixed";
            //table.CssClass = "ms-gridtext";
            table.Rows.Add(new TableRow());
            table.Rows[0].Cells.Add(
                new TableCell
                    {
                        Text = string.Format("<div style='width:170px;'>{0}</div>", GetStorage().Caption),
                        Width = Unit.Pixel(170),
                        HorizontalAlign = HorizontalAlign.Right,
                    });
            table.Rows[0].Cells.Add(new TableCell { Width = Unit.Percentage(100) });
            table.Rows[0].Cells[0].Style["padding-right"] = "6px";
            table.Rows[0].Cells[1].Style["padding-right"] = "6px";
            
            DataTable filterRefTable = DataSourceHelper.GetDataTable(DataSource);

            panel = new Panel();

            panel.ID = "panel_ID";
            panel.Width = Unit.Percentage(100);
            panel.Height = PanelHeight;
            panel.ScrollBars = ScrollBars.Auto;
            panel.BorderWidth = 1;
            panel.BorderColor = Color.LightGray;
            table.Rows[0].Cells[1].Controls.Add(panel);

            Initialization();

            panel.Controls.Add(tableDataSource);
            gridViewExt = new GridViewExt();
            gridViewExt.ID = "gridViewExt_ID";
            gridViewExt.DataKeyNames = new string[] { ValueMember };
            gridViewExt.AutoGenerateColumns = false;
            gridViewExt.AllowNewButtonForTree = false;
            gridViewExt.Height = Unit.Percentage(100);
            gridViewExt.Width = Unit.Percentage(100);
            gridViewExt.EmptyDataText = Resources.SEmptyDataText;
            gridViewExt.UseDeleteField = true;
            gridViewExt.AllowPaging = (bool)(DataSetResourceManager.GetTableExtProperty(filterRefTable, TableExtProperties.ALLOW_PAGING) ?? true);
            if ((Boolean)(DataSetResourceManager.GetTableExtProperty(filterRefTable, TableExtProperties.IS_TREE_REF) ?? false))
            {
                gridViewExt.AllowPaging = false;
                gridViewExt.ShowAsTree = true;
                gridViewExt.RelationName = DataSetResourceManager.GetTableExtProperty(
                                           filterRefTable, TableExtProperties.TREE_REF_RELATION).ToString();
            }

            tableDataSource.View.EnablePaging = gridViewExt.AllowPaging;
            gridViewExt.DataSourceID = tableDataSource.ID;
            foreach (DataColumn dc in tableDataSource.Table.Columns)
            {
                Boolean showColumn = (Boolean)(DataSetResourceManager.GetColumnExtProperty(dc, ColumnExtProperties.VISIBLE) ?? false);
                string visibleCulture = (string)(DataSetResourceManager.GetColumnExtProperty(dc, ColumnExtProperties.VISIBLE_CULTURE));
                if (showColumn && (string.IsNullOrEmpty(visibleCulture) || System.Threading.Thread.CurrentThread.CurrentUICulture.Name == visibleCulture))
                {
                    String columnCaption = (String)(DataSetResourceManager.GetColumnExtProperty(dc, ColumnExtProperties.CAPTION) ?? dc.Caption);
                    
                    BoundField boundField = new BoundField();
                    boundField.DataField = dc.ColumnName;
                    boundField.HeaderText = columnCaption;
                    boundField.HeaderStyle.Height = new Unit(25, UnitType.Pixel);
                    boundField.SortExpression = dc.ColumnName;
                    Boolean htmlEncode = (Boolean)(DataSetResourceManager.GetColumnExtProperty(dc, ColumnExtProperties.HTML_ENCODED) ?? false);
                    boundField.HtmlEncode = htmlEncode;
                    gridViewExt.Columns.Add(boundField);
                }
            }

            var boundColumns = gridViewExt.Columns.OfType<BoundField>().Where(r => !string.IsNullOrEmpty(r.DataField)).ToList();
            if (boundColumns.Count == 1)
                boundColumns[0].HeaderStyle.Width = new Unit(100, UnitType.Percentage);

            table.Rows[0].Cells[1].Controls.Add(GetSearchPanel());

            if (gridViewExt.AllowPaging)
            {
                checkedIds = new HiddenField { ID = "checkedIds" };
                if (_storage.Values == null || _storage.Values.Length == 0)
                    checkedIds.Value = "";
                else
                    checkedIds.Value = string.Join(",", _storage.Values.Select(Convert.ToString).ToArray());
                Controls.Add(checkedIds);
            }

            panel.Controls.Add(gridViewExt);

            var searchValue = Page.Request.Params[QuickSearchTextBox.UniqueID];
            if (!string.IsNullOrEmpty(searchValue) && gridViewExt.AllowPaging)
            {
                var searchCondition = new QueryCondition
                                      {
                                          Conditions =
                                              new QueryConditionList
                                              {
                                                  ConditionJunction = ConditionJunction.Or
                                              }
                                      };
                foreach (DataColumn dc in tableDataSource.Table.Columns)
                {
                    var showColumn = (Boolean)(DataSetResourceManager.GetColumnExtProperty(dc, ColumnExtProperties.VISIBLE) ?? false);
                    if (showColumn)
                    {
                        searchCondition.Conditions.Add(
                            new QueryCondition(dc.ColumnName, ColumnFilterType.Contains, searchValue, null));
                    }
                }

                tableDataSource.View.CustomConditions.Add(searchCondition);
            }

            if (checkedIds != null)
            {
                var checkedField = (CheckedField)gridViewExt.GetIGridColumn("cfDelete");
                checkedField.OnChanged = string.Format("{0}(this);", GetCheckChangedFunctionName());
                checkedField.OnChangedAll = string.Format("{0}(this);", GetCheckChangedAllFunctionName());
            }
        }

        private Control GetSearchPanel()
        {
            var panel = new Panel();

            panel.ID = "searchPanel_ID";
            panel.Width = Unit.Percentage(100);
            panel.Height = Unit.Pixel(30);
            QuickSearchTextBox = new TextBox { ID = "QuickSearch" };
            QuickSearchTextBox.Attributes["onchange"] = string.Format("{0}();", GetQuickSearchValueChangeFunctionName());
            var label = new Label { Text = Resources.SQuickSearch, AssociatedControlID = QuickSearchTextBox.ID };
            label.Style[HtmlTextWriterStyle.PaddingRight] = "5px";
            panel.Controls.Add(label);
            panel.Controls.Add(QuickSearchTextBox);
            quickSearchButton = new Button
                     {
                         ID = "searchButton",
                         Text = Resources.SSearch,
                         OnClientClick = string.Format("return {0}();", GetQuickSearchValueChangeFunctionName()),
                     };
            quickSearchButton.Click += QuickSearchClick;
            quickSearchButton.Style[HtmlTextWriterStyle.Visibility] = "hidden";
            panel.Controls.Add(quickSearchButton);
            panel.DefaultButton = quickSearchButton.ID;
            return panel;
        }

        private void QuickSearchClick(object sender, EventArgs eventArgs)
        {
            if (GridViewCreated)
            {
                gridViewExt.DataBind();
            }
        }

        protected virtual void EnsureGridViewCreated()
        {
            if (!GridViewCreated && !_inCreateGridView)
            {
                _inCreateGridView = true;
                try
                {
                    CreateGridView();
                }
                finally
                {
                    _inCreateGridView = false;
                }
                GridViewCreated = true;
            }
        }

        protected override void OnInit(EventArgs e)
        {
            base.OnInit(e);
            _inited = true;
        }

        protected override void OnPreRender(EventArgs e)
        {
            base.OnPreRender(e);
            EnsureGridViewCreated();
            gridViewExt.EnsureDataBoundInternal();
            string script;
            if (!gridViewExt.AllowPaging)
            {
                if (gridViewExt.ShowAsTree)
                {
                    script = string.Format(@"function {2}(){{
    var value = $('#{0}').val();
    $('#{1} tr').show();

    if (value == null || value == ''){{
        $('#{1} tr').filter(function(){{
            return !$(this).hasClass('ms-vh') && $(this).find('input[role=""row-visible""]').val() != '1';
        }}).hide();

        return false;
    }}

    $('#{1} tr').filter(function(){{
        return !$(this).hasClass('ms-vh') && !this.innerText.match(new RegExp(value,'i'));
    }}).hide();
    return false;
}}
", QuickSearchTextBox.ClientID, gridViewExt.ClientID, GetQuickSearchValueChangeFunctionName());
                }
                else
                {
                    script = string.Format(@"function {2}(){{
    var value = $('#{0}').val();
    $('#{1} tr').show();
    if (value == null || value == '')
        return false;
    $('#{1} tr').filter(function(){{
        return !$(this).hasClass('ms-vh') && !this.innerText.match(new RegExp(value,'i'));
    }}).hide();
    return false;
}}
", QuickSearchTextBox.ClientID, gridViewExt.ClientID, GetQuickSearchValueChangeFunctionName());
                }
            }
            else
            {
                script = string.Format("function {0}(){{ return true; }}\r\n", GetQuickSearchValueChangeFunctionName());
            }

            Page.ClientScript.RegisterClientScriptBlock(GetType(), GetQuickSearchValueChangeFunctionName(), script, true);

            if (checkedIds != null)
            {
                script = string.Format(@"function {0} (checkbox){{
    var rowId = $(checkbox).parent().parent().find('input[role=""row-id""]').val();
    var hf = $('#{2}')[0];
    if (checkbox.checked)
        AddSelectedValueInHiddenField(hf, rowId);
    else
        RemoveSelectedValueInHiddenField(hf, rowId);
}}
function {1} (checkbox){{
    var rows = $(checkbox).parent().parent().parent().find('input[role=""row-id""]');
    var hf = $('#{2}')[0];
    for(var rowIndex = 0; rowIndex < rows.length; rowIndex ++) {{
        var rowId = $(rows[rowIndex]).val();
        if (checkbox.checked)
            AddSelectedValueInHiddenField(hf, rowId);
        else
            RemoveSelectedValueInHiddenField(hf, rowId);
    }}
}}
", GetCheckChangedFunctionName(), GetCheckChangedAllFunctionName(), checkedIds.ClientID);
                Page.ClientScript.RegisterClientScriptBlock(GetType(), GetCheckChangedAllFunctionName(), script, true);
                var value = Page.Request.Params[checkedIds.UniqueID];
                if (!string.IsNullOrEmpty(value) && !checkedIdsCleared)
                {
                    var values = value.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries).Distinct().ToDictionary(r => r);
                    foreach (GridViewExtRow row in gridViewExt.Rows)
                    {
                        if (row.ItemValue == null || !values.ContainsKey(row.ItemValue))
                            continue;
                        var checkBox = row.Cells.Cast<TableCell>()
                            .SelectMany(r => r.Controls.OfType<CheckBox>())
                            .FirstOrDefault();
                        if (checkBox != null)
                            checkBox.Checked = true;
                    }
                }
            }
        }

        private string GetQuickSearchValueChangeFunctionName()
        {
            return ID + "QuickSearchValueChange";
        }

        private string GetCheckChangedFunctionName()
        {
            return ID + "CheckChanged";
        }

        private string GetCheckChangedAllFunctionName()
        {
            return ID + "CheckChangedAll";
        }

        public override void DataBind()
        {
            if (tableDataSource != null)
                tableDataSource.FillType = TableDataSourceFillType.Always;
            
            base.DataBind();
            
            if (tableDataSource != null)
                tableDataSource.FillType = TableDataSourceFillType.ParametersNotChanged;
        }

        #region Properties

        protected bool GridViewCreated
        {
            get { return _gridViewCreated; }
            set { _gridViewCreated = value; }
        }

        public override ControlCollection Controls
        {
            get
            {
                if (_inited) EnsureGridViewCreated();
                return base.Controls;
            }
        }

        public string ErrorText
        {
            get { return errorText; }
            set { errorText = value; }
        }

        public object DataSource
        {
            get { return dataSource; }
            set
            {
                dataSource = value;
                _storage.RefDataSource = value;
                OnColumnFilterStorageChanged();
            }
        }

        public string DataMember
        {
            get { return dataMember; }
            set { dataMember = value; }
        }

        public bool EmptyValidation
        {
            get { return emptyValidation; }
            set { emptyValidation = value; }
        }

        public string DisplayMember
        {
            get { return displayMember; }
            set { displayMember = value; }
        }

        public string ValueMember
        {
            get { return valueMember; }
            set { valueMember = value; }
        }

        /// <summary>
        /// Тип фильтра (Equal, In).
        /// </summary>
        public ColumnFilterType FilterType
        {
            get { return columnFilterType; }
            set
            {
                if (value != ColumnFilterType.Equal && value != ColumnFilterType.In && value != ColumnFilterType.None)
                    throw new ArgumentOutOfRangeException("value");

                columnFilterType = value;
                _storage.FilterType = value;
                _storage.AvailableFilters = ColumnFilterType.In | ColumnFilterType.Equal | ColumnFilterType.None;
                OnColumnFilterStorageChanged();
            }
        }

        #endregion

        #region IColumnFilter Members

        public event ValidateEventHandler FilterValidate;

        protected virtual void OnColumnFilterStorageChanged()
        {
            ColumnFilterStorageChanged?.Invoke(this, EventArgs.Empty);
        }

        public ColumnFilterStorage GetStorage()
        {
            if (gridViewExt != null)
            {
                List<object> list = new List<object>();
                if (checkedIds != null && Page.Request.Params[checkedIds.UniqueID] != null)
                {
                    var value = Page.Request.Params[checkedIds.UniqueID];
                    if (!checkedIdsCleared)
                    {
                        list.AddRange(
                        value.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
                            .Select(v => Convert.ChangeType(v, _storage.DataType))
                            .Distinct());
                    }
                }
                else
                {
                    var checkedField = (CheckedField)gridViewExt.GetIGridColumn("cfDelete");
                    bool[] checkedFields = checkedField.GetCheckedValues(gridViewExt);
                    for (int index = 0; index < gridViewExt.Rows.Count && index < checkedFields.Length; index++)
                    {
                        if (checkedFields[index])
                            list.Add(gridViewExt.DataKeys[gridViewExt.Rows[index].DataItemIndex].Value);
                    }
                }
                
                _storage.Values = list.ToArray();
                if (_storage.Values.Length == 0 && AllowNone)
                    _storage.FilterType = ColumnFilterType.None;
                else
                    _storage.FilterType = ColumnFilterType.In;
            }

            return (ColumnFilterStorage)_storage.Clone();
        }

        public void SetStorage(ColumnFilterStorage storage)
        {
            _storage = (ColumnFilterStorage)storage.Clone();
            OnColumnFilterStorageChanged();
            if (checkedIds != null)
            {
                if (_storage.Values == null || _storage.Values.Length == 0)
                    checkedIds.Value = "";
                else
                    checkedIds.Value = string.Join(",", _storage.Values.Select(Convert.ToString).ToArray());
            }
        }

        public DataRow GetRow(int index)
        {
            return tableDataSource.Table.Rows[index];
        }

        public string GetText(int index)
        {
            StringBuilder text = new StringBuilder();

            foreach (DataColumn dc in tableDataSource.Table.Columns)
            {
                Boolean showColumn = (Boolean)(DataSetResourceManager.GetColumnExtProperty(dc, ColumnExtProperties.VISIBLE) ?? false);
                if (showColumn)
                {
                    String columnCaption = (String)(DataSetResourceManager.GetColumnExtProperty(dc, ColumnExtProperties.CAPTION) ?? dc.Caption);
                    text.Append(columnCaption + ": "  + tableDataSource.View.DataView[index].Row[dc] + "; ");
                }
            }

            return text.Remove(text.Length-1,1).ToString();
        }

        public string[] GetTexts()
        {
            return null;
        }

        public string GetLog()
        {
            StringBuilder log = new StringBuilder();
            Initialization();
            List<DataColumn> columnList = new List<DataColumn>();
            //Ишим колонки для логирования(LOG_ENTITY=True)
            foreach (DataColumn dc in tableDataSource.Table.Columns)
            {
                if ((Boolean)(DataSetResourceManager.GetColumnExtProperty(dc, ColumnExtProperties.LOG_ENTITY) ?? false))
                    columnList.Add(dc);
            }
            //Если не выбрали колонки для логирования, то логируем все кроме LOG_ENTITY=False
            if (columnList.Count == 0)
            {
                foreach (DataColumn column in tableDataSource.Table.Columns)
                {
                    if ((Boolean)(DataSetResourceManager.GetColumnExtProperty(column, ColumnExtProperties.LOG_ENTITY) ?? true))
                        columnList.Add(column);
                }
            }

            //Если у всех колонок стоит LOG_ENTITY=False, то возврашаем NULL
            if (columnList.Count == 0)
                return null;

            List<string> logList = new List<string>();
            if (gridViewExt != null)
            {
                if (checkedIds != null && Page.Request.Params[checkedIds.UniqueID] != null)
                {
                    var values = Page.Request.Params[checkedIds.UniqueID];
                    var keys = values.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries).Distinct().ToDictionary(r => r);
                    for (int index = 0; index < tableDataSource.View.DataView.Count; index++)
                    {
                        if (keys.ContainsKey(tableDataSource.View.DataView[index].Row[ValueMember].ToString()))
                        {
                            string message = null;
                            foreach (DataColumn column in columnList)
                                message += tableDataSource.View.DataView[index].Row[column] + ", ";

                            if (message != null) logList.Add(message.TrimEnd(',', ' '));
                        }
                    } 
                }
                else
                {
                    CheckedField checkedField = (CheckedField)gridViewExt.GetIGridColumn("cfDelete");
                    bool[] checkedFields = checkedField.GetCheckedValues(gridViewExt);
                    for (int index = 0; index < tableDataSource.View.DataView.Count && index < checkedFields.Length; index++)
                    {
                        if (checkedFields[index])
                        {
                            string message = null;
                            foreach (DataColumn column in columnList) message += tableDataSource.View.DataView[index].Row[column] + ", ";

                            if (message != null) logList.Add(message.TrimEnd(',', ' '));
                        }
                    }                    
                }
            }
            else
            {
                tableDataSource.View.CustomConditions.Add(new QueryCondition(ValueMember, ColumnFilterType.In, _storage.Values));
                tableDataSource.View.Select(new DataSourceSelectArguments());
                for (int index = 0; index < tableDataSource.View.DataView.Count; index++)
                {
                    string message = null;
                    foreach (DataColumn column in columnList) 
                        message += tableDataSource.View.DataView[index].Row[column] + ", ";

                    if (message != null)
                        logList.Add(message.TrimEnd(',', ' '));
                }
            }

            string caption = GetStorage().Caption;
            string text = "Поле '" + caption + "' содержит '";
            
            foreach (string message in logList)
                text += message + "; ";
            log.Append(text.Remove(text.Length-2)+'\'');

            return log.AppendLine().ToString();
        }

        public bool Validate()
        {
            ValidateEventArgs args = new ValidateEventArgs();
            this.GetStorage();
            _storage.Validate(args);
            errorText = args.ErrorText;
            if (args.Valid && (_storage.Values == null || _storage.Values.Length == 0) && _storage.FilterType == ColumnFilterType.In)
            {
                args.Valid = false;
                errorText = string.Format(Resources.SRequiredFieldMessage, _storage.Caption);
            }

            return args.Valid;
        }

        #endregion

        #region ISupportSessionWorker Members

        public SessionWorker SessionWorker
        {
            get { return sessionWorker; }
            set { sessionWorker = value; }
        }

        #endregion

        #region ISupportPostBack Members

        public Boolean PostBack { get; set; }

        public string[] DefaultValues { get; set; }

        public ColumnFilterType DefaultFilterType { get; set; }

        #endregion

        /// <summary>
        /// Возвращает идентификаторы html-элементов, в которые осуществляется ввод данных.
        /// </summary>
        /// <returns></returns>
        public IEnumerable<DefaultFilterValues> GetInputElements()
        {
            EnsureGridViewCreated();
            gridViewExt.EnsureDataBoundInternal();
            var list = new List<DefaultFilterValues>();
            foreach (GridViewExtRow row in gridViewExt.Rows)
                AddInputElement(row, list);

            if (gridViewExt.HeaderRow != null)
                AddInputElement(gridViewExt.HeaderRow, list);
            

            if (checkedIds != null)
                list.Add(new DefaultFilterValues { ClientId = checkedIds.ClientID, Value = string.Empty });

            return list;
        }

        private void AddInputElement(GridViewRow row, List<DefaultFilterValues> list)
        {
            var checkedField = row.Cells
                .OfType<DataControlFieldCell>()
                .FirstOrDefault(
                    r => r.ContainingField is CheckedField
                         && ((CheckedField)r.ContainingField).ColumnName == gridViewExt.DeleteFieldColumnName);
            var checkBox = checkedField?.Controls.OfType<CheckBox>().FirstOrDefault();
            if (checkBox == null)
                return;

            if (row.DataItemIndex >= 0 && _storage.Values.Contains(gridViewExt.DataKeys[row.DataItemIndex].Value))
                checkBox.Checked = true;
            checkBox.AutoPostBack = PostBack;
            list.Add(new DefaultFilterValues { ClientId = checkBox.ClientID, Value = string.Empty });
        }

        /// <summary>
        /// Возвращает контролы, в которые осуществляется ввод данных.
        /// </summary>
        /// <returns></returns>
        public ICollection<Control> GetInputControls()
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Возвращает провайдеры форматов для html-элементов, в которые осуществляется ввод данных.
        /// </summary>
        /// <returns>Тройку объектов: идентификатор элемента, провайдер формата, строковый шаблон (дополнительно).</returns>
        public ICollection<Pair<string, IFormatProvider>> GetInputFormatProviders()
        {
            throw new NotImplementedException();
        }

        public void ClearSelection()
        {
            if (checkedIds != null)
                checkedIds.Value = string.Empty;

            checkedIdsCleared = true;
        }
    }
}