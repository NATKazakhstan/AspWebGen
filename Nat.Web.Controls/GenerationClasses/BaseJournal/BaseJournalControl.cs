using System;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Data;
using System.Resources;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Data.Linq;
using System.Linq;
using System.Collections;
using System.Security.Permissions;
using System.Collections.Generic;
using AjaxControlToolkit;
using AjaxControlToolkit.Design;
using Nat.Tools;
using Nat.Tools.Filtering;
using Nat.Web.Controls.Data;
using Nat.Web.Controls.EnableController;
using Nat.Web.Controls.GenerationClasses.Data;
using Nat.Web.Controls.GenerationClasses.Filter;
using Nat.Web.Controls.GenerationClasses.HierarchyFields;
using Nat.Web.Tools;
using Nat.Web.Controls.Properties;
using System.Threading;

namespace Nat.Web.Controls.GenerationClasses.BaseJournal
{
    using Microsoft.JScript;

    using Nat.Web.Tools.Export.Formatting;

    using Convert = System.Convert;

    public abstract class BaseJournalControl : CompositeDataBoundControl, IPostBackEventHandler
    {
        #region Constants
        public const string SetFilters = "SetFilters:";
        public const string SetFiltersUncodedUri = "SetFiltersC:";
        public const string FilterByColumn = "FilterBy:";
        public const string FilterEqualsByColumn = "FilterBy:{0}:Equals:{1}:{2}";
        public const string ClearFilter = "ClearFilter";
        public const string GroupByColumn = "GroupBy:";
        public const string GroupByColumnAdd = "GroupBy:{0}";
        public const string GroupByColumnRemove = "GroupBy:{0}:Remove";
        public const string GroupByColumnMoveUp = "GroupBy:{0}:MoveUp";
        public const string GroupByColumnMoveDown = "GroupBy:{0}:MoveDown";
        public const string GroupByClear = "GroupBy:Clear";
        public const string OrderByColumn = "OrderBy:";
        public const string OrderByColumnAsc = "OrderBy:{0}:Asc";
        public const string OrderByColumnDesc = "OrderBy:{0}:Desc";
        public const string OrderByColumnRemove = "OrderBy:{0}:Remove";
        public const string ExportExcel = "ExcelExport";
        public const string ApplyTableSettings = "ApplyTableSettings";
        public const string CreateConcatenateColumns = "CreateConcatenateColumns";
        public const string Subscription = "Subscription";
        public const string SaveViewSettings = "SaveViewSettings:";
        public const string LoadViewSettings = "LoadViewSettings:";
        public const string SaveJournalValues = "SaveJournalValues:";
        public const string EditJournalValues = "EditJournalValues";
        public const string CancelJournalValues = "CancelJournalValues";
        public const string RefreshJournalValues = "RefreshJournalValues";
        public const string SetPageIndex = "SetPageIndex:";
        #endregion

        #region fields

        private readonly List<string> _messages = new List<string>();
        private readonly List<string> _errorMessages = new List<string>();
        protected bool IsExport;
        private List<RowProperties> _rowsProperties;
        private List<CellProperties> _cellsProperties;
        private Dictionary<string, RowProperties> _rowsPropertiesDic;
        private Dictionary<string, CellProperties> _cellsPropertiesDic;
        private List<string> _orderByColumns;
        private string _saveValidationGroup;
        private string _defaultOrder;
        public List<int> RowNumbering = new List<int>();
        public List<int> GroupNumbering = new List<int>();
        public int RowNumberingTable;
        public int SplitRowNumberingTable;
        public int RowNumberingTableCustomChange;
        public List<int> RowContinuousNumbering = new List<int>();

        #endregion

        protected BaseJournalControl()
        {
            DetailsGroupsCSS = new List<string>();
            DetailsGroupsAlternateCSS = new List<string>();
            EndEditRowKeys = new List<string>();
            CustomTotals = new List<string>();
            PageSize = 50;
            DrawPanelVisible = true;
            GroupPanelVisible = true;
        }

        #region AutoProperties

        protected bool GroupColumnsInitialized { get; set; }
        public BaseFilter Filter { get; set; }
        public BaseJournalUserControl ParentUserControl { get; set; }
        public SelectingColumn SelectingColumnControl { get; protected set; }
        public SavingJournalSettings SavingJournalSettingsControl { get; protected set; }
        public abstract BaseJournalHeaderControl BaseInnerHeader { get; }
        public virtual ResourceManager ResourceManager { get { return null; } }
        public event EventHandler FilterChanged;
        public event EventHandler GroupingChanged;
        public bool IsLoaded { get; private set; }
        public abstract DataContext DataContext { get; }
        public bool ShowTotals { get; set; }
        public bool ShowFullTotal { get; set; }
        public bool NeedSaveButton { get; set; }
        public bool DetailsRender { get; set; }
        public List<string> DetailsGroupsCSS { get; set; }
        public List<string> DetailsGroupsAlternateCSS { get; set; }
        public string EditRowKey { get; set; }
        public List<string> EndEditRowKeys { get; set; }
        public bool EditAllRows { get; set; }
        public bool EndEditAllRows { get; set; }
        public bool AllowPaging { get; set; }
        public int PageSize { get; set; }
        internal List<ConcatenateColumnTransporter> ConcatenateColumns { get; set; }
        public List<string> CustomTotals { get; set; }
        public bool DrawPanelVisible { get; set; }
        public bool GroupPanelVisible { get; set; }

        #endregion

        #region Properties

        protected internal bool AllowEditEmptyCell { get; set; }

        public List<GroupColumn> GroupColumns
        {
            get
            {
                var list = (List<GroupColumn>)ViewState["GroupColumns"];
                if (list == null)
                {
                    list = new List<GroupColumn>();
                    ViewState["GroupColumns"] = list;
                    EnsureInitializeGroupColumns();
                }
                return list;
            }
        }

        public string SaveValidationGroup
        {
            get { return _saveValidationGroup ?? ParentUserControl.GetTableName() + "Save"; }
            set { _saveValidationGroup = value; }
        }

        public string DefaultOrder
        {
            get
            {
                return _defaultOrder;
            }
            set
            {
                _orderByColumns = null;
                _defaultOrder = value;
                /*if (SelectingColumnControl != null)
                    SelectingColumnControl.SetOrderColumns(value);*/
            }
        }

        public List<string> OrderByColumns
        {
            get
            {
                if (_orderByColumns == null)
                    _orderByColumns = (DefaultOrder ?? "").Split(',').ToList();
                return _orderByColumns;
            }
        }

        private bool[] _isInlineGroup;

        private Table exportFooter;
        private Table exportHeader;
        private Table[] additionalSheetsTable;

        public bool[] IsInlineGroup
        {
            get
            {
                if (_isInlineGroup == null)
                {
                    InitializeGroupTypes();
                    _isInlineGroup = new bool[GroupColumns.Count];
                    for (int i = 0; i < GroupColumns.Count; i++)
                        _isInlineGroup[i] = BaseInnerHeader.ColumnsDic[GroupColumns[i].ColumnName].InlineGrouping;
                }
                return _isInlineGroup;
            }
        }

        protected abstract void InitializeGroupTypes();

        public List<RowProperties> RowsProperties
        {
            get
            {
                if (_rowsProperties == null)
                {
                    _rowsProperties = new List<RowProperties>();
                    InitializeRowsProperties();
                }
                return _rowsProperties;
            }
            set
            {
                _rowsProperties = value;
                _rowsPropertiesDic = null;
                if (SelectingColumnControl != null)
                    SelectingColumnControl.SetRowsProperties(value);
            }
        }

        public Dictionary<string, RowProperties> RowsPropertiesDic
        {
            get
            {
                if (_rowsPropertiesDic == null)
                {
                    if (RowsProperties.Count == 0)
                        _rowsPropertiesDic = new Dictionary<string, RowProperties>();
                    else
                        _rowsPropertiesDic = RowsProperties.ToDictionary(r => r.Key);
                }
                return _rowsPropertiesDic;
            }
            protected set { _rowsPropertiesDic = value; }
        }

        public List<CellProperties> CellsProperties
        {
            get
            {
                if (_cellsProperties == null)
                {
                    _cellsProperties = new List<CellProperties>();
                    InitializeCellsProperties();
                }
                return _cellsProperties;
            }
            set
            {
                _cellsProperties = value;
                _cellsPropertiesDic = null;
                if (SelectingColumnControl != null)
                    SelectingColumnControl.SetCellsProperties(value);
            }
        }

        public Dictionary<string, CellProperties> CellsPropertiesDic
        {
            get
            {
                if (_cellsPropertiesDic == null)
                {
                    if (CellsProperties.Count == 0)
                        _cellsPropertiesDic = new Dictionary<string, CellProperties>();
                    else
                        _cellsPropertiesDic = CellsProperties.ToDictionary(r => r.Key);
                }
                return _cellsPropertiesDic;
            }
            protected set { _cellsPropertiesDic = value; }
        }

        public virtual int PageIndex
        {
            get { return (int?)ViewState["PageIndex"] ?? 0; } 
            set { ViewState["PageIndex"] = value; }
        }

        public Table ExportFooter
        {
            get
            {
                EnsureInitializedExportFooter();
                return exportFooter;
            }
            protected set => exportFooter = value;
        }

        public Table ExportHeader
        {
            get
            {
                EnsureInitializedExportFooter();
                return exportHeader;
            }
            protected set => exportHeader = value;
        }

        public Table[] AdditionalSheetsTable
        {
            get
            {
                EnsureInitializedExportFooter();
                return additionalSheetsTable;
            }
            protected set => additionalSheetsTable = value;
        }

        protected bool InitializedExportFooter { get; set; }

        private void EnsureInitializedExportFooter()
        {
            if (InitializedExportFooter) return;

            InitializedExportFooter = true;
            InitializeExportFooter();
            InitializeExportHeader();
            InitializeAdditionalSheetsTable();
        }

        protected virtual void InitializeExportFooter()
        {
        }

        protected virtual void InitializeExportHeader()
        {
        }

        protected virtual void InitializeAdditionalSheetsTable()
        {
        }

        public virtual string GetExportSheetName()
        {
            return null;
        }

        #endregion

        #region Methods

        internal static bool IsIE()
        {
            return "IE".Equals(HttpContext.Current.Request.Browser.Browser, StringComparison.OrdinalIgnoreCase)
                   && string.Compare("9.0", HttpContext.Current.Request.Browser.Version, StringComparison.OrdinalIgnoreCase) > 0;
        }

        protected abstract void SaveJournalValuesClick(string rowKey);

        protected abstract void ExportToExcel();

        protected virtual bool InitializeRowsProperties()
        {
            if (SelectingColumnControl != null)
            {
                var rows = SelectingColumnControl.DesirializeRows();
                if (rows != null)
                {
                    RowsProperties = rows;
                    RowsPropertiesDic = RowsProperties.ToDictionary(r => r.Key);
                    return true;
                }
            }
            RowsProperties = new List<RowProperties>();
            RowsPropertiesDic = new Dictionary<string, RowProperties>();
            return false;
        }

        protected virtual void InitializeCellsProperties()
        {
            if (SelectingColumnControl != null)
            {
                var cells = SelectingColumnControl.DesirializeCells();
                if (cells != null)
                {
                    CellsProperties = cells;
                    CellsPropertiesDic = CellsProperties.ToDictionary(r => r.Key);
                    return;
                }
            }
            CellsProperties = new List<CellProperties>();
            CellsPropertiesDic = new Dictionary<string, CellProperties>();
        }

        protected virtual void OnFilterChanged(EventArgs e)
        {
            if (FilterChanged != null) 
                FilterChanged(this, e);
        }

        protected virtual void OnGroupingChanged(EventArgs e)
        {
            if (GroupingChanged != null)
                GroupingChanged(this, e);
        }

        protected virtual List<GroupColumn> GetDefaultGroupColumns()
        {
            return new List<GroupColumn>();
        }

        protected virtual void EnsureInitializeGroupColumns()
        {
            if (GroupColumnsInitialized) return;
            GroupColumnsInitialized = true;
            InitializeGroupColumns();
        }

        protected virtual void InitializeGroupColumns()
        {
        }

        public virtual List<ControllerItem> GetEnableControllers()
        {
            return null;
        }

        protected virtual void OnGetValuesByRenderContextHandler(object sender, string codeItem, ControllerItemValueEventArgs args)
        {
        }

        protected virtual bool RaisePostBackEvent(string eventArgument)
        {
            if (eventArgument.StartsWith(FilterByColumn, StringComparison.OrdinalIgnoreCase))
            {
                var dic = MainPageUrlBuilder.Current.GetFilterItemsDic(Filter.TableName) ?? new Dictionary<string, List<FilterItem>>();
                var split = eventArgument.Split(':');
                var filterName = split[1];
                List<FilterItem> list;
                if (!dic.ContainsKey(filterName))
                    list = dic[filterName] = new List<FilterItem>();
                else
                    list = dic[filterName];
                list.Clear();
                var filterItem = new FilterItem
                                     {
                                         FilterName = filterName,
                                         FilterType = split[2],
                                         Value1 = split[3],
                                         Value2 = split[4]
                                     };
                list.Add(filterItem);
                MainPageUrlBuilder.Current.SetFilter(Filter.TableName, dic);
                MainPageUrlBuilder.ChangedUrl();
                SetFilterValuesToStorage(filterItem);
                OnFilterChanged(EventArgs.Empty);
                ParentUserControl.LogMonitor.Log(new LogMessageEntry(ParentUserControl.ViewLog, ParentUserControl.HeaderRu, RvsSavedProperties.GetFromJournal(ParentUserControl)));
                return true;
            }

            if (eventArgument.Equals(ClearFilter, StringComparison.OrdinalIgnoreCase))
            {
                MainPageUrlBuilder.Current.SetFilter(Filter.TableName, string.Empty);
                SetFiltersByStorage();
                MainPageUrlBuilder.ChangedUrl();
                OnFilterChanged(EventArgs.Empty);
                ParentUserControl.LogMonitor.Log(new LogMessageEntry(ParentUserControl.ViewLog, ParentUserControl.HeaderRu, RvsSavedProperties.GetFromJournal(ParentUserControl)));
                return true;
            }

            if (eventArgument.StartsWith(SetFilters, StringComparison.OrdinalIgnoreCase))
            {
                var filterValues = eventArgument.Substring(SetFilters.Length);
                MainPageUrlBuilder.Current.SetFilter(Filter.TableName, filterValues);
                MainPageUrlBuilder.ChangedUrl();
                SetFilterValuesToStorage();
                OnFilterChanged(EventArgs.Empty);
                ParentUserControl.LogMonitor.Log(new LogMessageEntry(ParentUserControl.ViewLog, ParentUserControl.HeaderRu, RvsSavedProperties.GetFromJournal(ParentUserControl)));
                return true;
            }

            if (eventArgument.StartsWith(SetFiltersUncodedUri, StringComparison.OrdinalIgnoreCase))
            {
                var filterValues = GlobalObject.decodeURIComponent(eventArgument.Substring(SetFiltersUncodedUri.Length));
                MainPageUrlBuilder.Current.SetFilter(Filter.TableName, filterValues);
                MainPageUrlBuilder.ChangedUrl();
                SetFilterValuesToStorage();
                OnFilterChanged(EventArgs.Empty);
                ParentUserControl.LogMonitor.Log(new LogMessageEntry(ParentUserControl.ViewLog, ParentUserControl.HeaderRu, RvsSavedProperties.GetFromJournal(ParentUserControl)));
                return true;
            }

            if (eventArgument.StartsWith(SetFiltersUncodedUri, StringComparison.OrdinalIgnoreCase))
            {
                var filterValues = GlobalObject.decodeURIComponent(eventArgument.Substring(SetFiltersUncodedUri.Length));
                MainPageUrlBuilder.Current.SetFilter(Filter.TableName, filterValues);
                MainPageUrlBuilder.ChangedUrl();
                SetFilterValuesToStorage();
                OnFilterChanged(EventArgs.Empty);
                ParentUserControl.LogMonitor.Log(new LogMessageEntry(ParentUserControl.ViewLog, ParentUserControl.HeaderRu, RvsSavedProperties.GetFromJournal(ParentUserControl)));
                return true;
            }

            if (eventArgument.Equals(ExportExcel, StringComparison.OrdinalIgnoreCase))
            {
                ExportToExcel();
            }

            if (eventArgument.Equals(ApplyTableSettings, StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }

            if (eventArgument.Equals(Subscription, StringComparison.OrdinalIgnoreCase))
            {
                RedirectToSubscriptions();
                return true;
            }

            if (eventArgument.StartsWith(SaveViewSettings, StringComparison.OrdinalIgnoreCase))
            {
                EnsureChildControls();
                SavingJournalSettings.SaveSettings(this, eventArgument.Substring(SaveViewSettings.Length), ParentUserControl.LogMonitor, ParentUserControl.SaveSettingsLog);
                return true;
            }

            if (eventArgument.StartsWith(LoadViewSettings, StringComparison.OrdinalIgnoreCase))
            {
                ////SavingJournalSettings.LoadSettings(this, eventArgument.Substring(LoadViewSettings.Length));
                return true;
            }

            if (eventArgument.StartsWith(OrderByColumn, StringComparison.OrdinalIgnoreCase))
            {
                var parameters = eventArgument.Substring(OrderByColumn.Length).Split(':');
                var columnName = parameters[0];
                var typeOrder = parameters[1];
                AddOrderByColumn(columnName, typeOrder, 0);
                return true;
            }

            if (eventArgument.StartsWith(SaveJournalValues, StringComparison.OrdinalIgnoreCase))
            {
                SaveJournalValuesClick(eventArgument.Substring(SaveJournalValues.Length));
                return true;
            }

            if (eventArgument.StartsWith(EditJournalValues, StringComparison.OrdinalIgnoreCase))
            {
                EditAllRows = true;
                return true;
            }

            if (eventArgument.StartsWith(CancelJournalValues, StringComparison.OrdinalIgnoreCase))
            {
                EndEditAllRows = true;
                return true;
            }

            if (eventArgument.StartsWith(RefreshJournalValues, StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }

            if (eventArgument.StartsWith(SetPageIndex, StringComparison.OrdinalIgnoreCase))
            {
                PageIndex = Convert.ToInt32(eventArgument.Substring(SetPageIndex.Length));
                OnFilterChanged(EventArgs.Empty);
                ParentUserControl.LogMonitor.Log(new LogMessageEntry(ParentUserControl.ViewLog, ParentUserControl.HeaderRu, RvsSavedProperties.GetFromJournal(ParentUserControl)));
                return true;
            }

            return false;
        }

        protected virtual void AddOrderByColumn(string columnName, string typeOrder, int insertIndex)
        {
            var orderColumns = new List<string>(DefaultOrder == null ? new string[0] : DefaultOrder.Split(','));
            if (orderColumns.Contains(columnName))
            {
                if (typeOrder == string.Empty) typeOrder = "Asc";
                orderColumns.Remove(columnName);
            }
            else if (orderColumns.Contains(columnName + " desc"))
            {
                if (typeOrder == string.Empty) typeOrder = "Desc";
                orderColumns.Remove(columnName + " desc");
            }
            if (typeOrder == string.Empty || typeOrder.Equals("Asc", StringComparison.OrdinalIgnoreCase))
                orderColumns.Insert(insertIndex, columnName);
            else if (typeOrder.Equals("Desc", StringComparison.OrdinalIgnoreCase))
                orderColumns.Insert(insertIndex, columnName + " desc");
            DefaultOrder = string.Join(",", orderColumns.ToArray());
        }

        private void SetFilterValuesToStorage()
        {
            if (ParentUserControl.StorageValues == null) return;
            ParentUserControl.InitFilterControl(null);
            ParentUserControl.BaseFilter.SetFiltersToStorageValues(ParentUserControl.StorageValues);
        }

        private void SetFiltersByStorage()
        {
            if (ParentUserControl.StorageValues == null) return;
            ParentUserControl.BaseFilter.SetFiltersByStorageValues(ParentUserControl.StorageValues, MainPageUrlBuilder.Current);
        }

        private void SetFilterValuesToStorage(FilterItem filterItem)
        {
            if (ParentUserControl.StorageValues == null) return;
            var storageValues = ParentUserControl.StorageValues;
            if (storageValues.GetStorageNames().Contains(filterItem.FilterName) 
                && storageValues.GetStorageFilterType(filterItem.FilterName) != null 
                && storageValues.GetStorageFilterType(filterItem.FilterName) != ColumnFilterType.None)
            {
                storageValues.SetStorageValues(filterItem.FilterName, filterItem.Value1, filterItem.Value2);
            }
        }

        protected virtual void RedirectToSubscriptions()
        {
            var prop = RvsSavedProperties.GetFromJournal(ParentUserControl);
            var id = prop.Save();
            var guid = Guid.NewGuid().ToString();
            HttpContext.Current.Session[guid] = prop.StorageValues;
            HttpContext.Current.Session["constants" + guid] = Filter.GetForPublicationStrings();
            var url = new MainPageUrlBuilder
                          {
                              UserControl = "ReportSubscriptionsEdit",
                              IsDataControl = true,
                              IsNew = true
                          };
            url.CustomQueryParameters.Add("guid", guid);
            url.CustomQueryParameters.Add("refRVSProperties", id.ToString());
            /*Определение типа отчета*/            
            url.CustomQueryParameters.Add("isSqlReportingServices", "2");
            url.CustomQueryParameters.Add("reportName", prop.ReportPluginName);
            url.CustomQueryParameters.Add("culture", ParentUserControl.Url.Culture);
            /*Определениe формата выгрузки отчета в зависимости от типа отчета*/
            url.CustomQueryParameters.Add("format", string.Empty);
            Page.Response.Redirect(url.CreateUrl(false, true));
        }

        public virtual void PrepareSettings()
        {
            IsLoaded = true;
            EnsureDataBound();
            SelectingColumnControl.PrepareSettings();
        }

        public string GetHfSortClientID()
        {
            return ClientID + "_hfSort";
        }

        public virtual List<ConditionalFormatting> GetConditionalFormatting()
        {
            return null;
        }

        #endregion

        #region Override methods of Control, interfaces

        void IPostBackEventHandler.RaisePostBackEvent(string eventArgument)
        {
            RaisePostBackEvent(eventArgument);
        }

        protected override void OnInit(EventArgs e)
        {
            base.OnInit(e);
            if (Page.IsPostBack && !ParentUserControl.ValuesLoaded && Page.Request.Params[GetHfSortClientID()] != null)
                DefaultOrder = Page.Request.Params[GetHfSortClientID()];
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            IsLoaded = true;
        }

        protected override void OnPreRender(EventArgs e)
        {
            if (ParentUserControl.Url.CustomQueryParameters.ContainsKey("ExportExcel"))
            {
                BaseInnerHeader.ColumnHierarchy
                    .SelectMany(r => r.SelectAll())
                    .Where(r => r.Width == 0)
                    .Where(r => !string.IsNullOrEmpty(r.ColumnKey))
                    .ForEach(r => r.Width = 200);
                ExportToExcel();
            }

            base.OnPreRender(e);
            Page.ClientScript.RegisterHiddenField(GetHfSortClientID(), DefaultOrder);

            if (DetailsGroupsCSS.Count == 0)
                DetailsGroupsCSS.AddRange(new[] { "DetailsGroupsCSS1", "DetailsGroupsCSS2", "DetailsGroupsCSS3" });
            if (DetailsGroupsAlternateCSS.Count == 0)
                DetailsGroupsAlternateCSS.AddRange(new[] { "DetailsGroupsAlternateCSS1", "DetailsGroupsAlternateCSS2", "DetailsGroupsAlternateCSS3" });
        }

        #endregion

        #region Messages 

        public bool HasErrorMessages
        {
            get { return _errorMessages != null && _errorMessages.Count > 0; }
        }

        public void AddMessages(IEnumerable<string> messages)
        {
            _messages.AddRange(messages);
        }

        public void AddMessage(string message)
        {
            _messages.Add(message);
        }

        public void AddErrorMessages(IEnumerable<string> messages)
        {
            _errorMessages.AddRange(messages);
        }

        public void AddErrorMessage(string message)
        {
            _errorMessages.Add(message);
        }

        protected void RenderMessages(HtmlTextWriter writer)
        {
            if (_messages.Count > 0)
            {
                writer.AddAttribute(HtmlTextWriterAttribute.Class, "font13 infoMessageText");
                if (!IsIE())
                {
                    writer.AddAttribute(
                        HtmlTextWriterAttribute.Style,
                        "padding: 6px; border: 2px solid red; color: red; position: absolute; z-index: 10; background-color: white; top: 220px; left: 64px; right: 64px;");
                }

                writer.RenderBeginTag(HtmlTextWriterTag.Div);
                foreach (var message in _messages)
                {
                    writer.Write(message);
                    writer.WriteBreak();
                }
                writer.WriteBreak();
                writer.RenderEndTag();
            }
        }

        protected void RenderErrorMessages(HtmlTextWriter writer)
        {
            if (_errorMessages.Count > 0)
            {
                writer.AddAttribute(HtmlTextWriterAttribute.Class, "font13");
                writer.RenderBeginTag(HtmlTextWriterTag.Div); 
                writer.AddAttribute(HtmlTextWriterAttribute.Class, "errorText");
                writer.RenderBeginTag(HtmlTextWriterTag.Div);
                foreach (var errorMessage in _errorMessages)
                {
                    writer.Write(errorMessage);
                    writer.WriteBreak();
                }
                writer.WriteBreak();
                writer.RenderEndTag();
                writer.RenderEndTag();
            }
        }

        #endregion

        public GroupType GetGroupType(string groupColumnName)
        {
            var index = GroupColumns.IndexOf(groupColumnName);
            return index < 0 ? GroupType.None : GroupColumns[index].GroupType; 
        }
    }

    public abstract class BaseJournalControl<TRow> : BaseJournalControl
        where TRow : BaseRow
    {
        private Dictionary<string, BaseJournalCrossTable<TRow>> _crossTables;

        #region Properies

        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public Dictionary<string, BaseJournalCrossTable<TRow>> CrossTables
        {
            get
            {
                if (_crossTables == null)
                {
                    _crossTables = new Dictionary<string, BaseJournalCrossTable<TRow>>();
                    EnsureCrossTablesInitialized();
                }
                return _crossTables;
            }
        }

        public override bool EnableViewState
        {
            get
            {
                return false;
            }
        }

        private BaseJournalHeaderControl<TRow> _innerHeader;

        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public BaseJournalHeaderControl<TRow> InnerHeader
        {
            get
            {
                if (_innerHeader == null)
                    _innerHeader = CreateHeaderControl();
                return _innerHeader;
            }
            set { _innerHeader = value; }
        }

        Dictionary<string, ConditionalFormatting> conditionalFormatting;

        internal ConditionalFormatting GetConditionalFormatting(string columnKey)
        {
            if (conditionalFormatting == null)
            {
                var formatting = GetConditionalFormatting();
                if (formatting == null)
                    conditionalFormatting = new Dictionary<string, ConditionalFormatting>();
                else
                    conditionalFormatting = formatting
                        .SelectMany(r => r.Columns.Select(c => new { column = c, formatting = r }))
                        .ToDictionary(r => r.column, r => r.formatting);
            }

            return conditionalFormatting.ContainsKey(columnKey) ? conditionalFormatting[columnKey] : null;
        }

        public override BaseJournalHeaderControl BaseInnerHeader
        {
            get { return InnerHeader; }
        }

        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public virtual TRow SelectedDataRow { get; protected set; }

        protected bool CrossTablesInitialized { get; set; }

        public BaseGroupingControl<TRow> GroupControl { get; protected set; }

        #endregion

        #region Methods

        public override void PrepareSettings()
        {
            base.PrepareSettings();
            InnerHeader.EnsureColumnsInitialized();
        }

        protected abstract BaseJournalHeaderControl<TRow> CreateHeaderControl();
        protected abstract object[] GetGroupRowValues(TRow row);

        protected virtual void InitializeCrossTables()
        {
            CrossTablesInitialized = true;
        }

        protected virtual void EnsureCrossTablesInitialized()
        {
            if (!CrossTablesInitialized)
                InitializeCrossTables();
        }

        protected override bool RaisePostBackEvent(string eventArgument)
        {
            if (base.RaisePostBackEvent(eventArgument)) return true;
            return RaiseGroupCommand(eventArgument);
        }

        public virtual IEnumerable<GroupColumn> GetInlineGroupColumns()
        {
            return GroupColumns.Where(r => InnerHeader.ColumnsDic[r.ColumnName].InlineGrouping 
                || InnerHeader.ColumnsDic[r.ColumnName].GroupType == GroupType.InHeader);
        }

        protected virtual bool IsVisibleRow(TRow row)
        {
            return true;
        }

        protected virtual bool IsVisibleGroupRow(TRow row, GroupColumn groupColumn)
        {
            return true;
        }

        protected virtual bool IsVisibleGroupRow(TRow row, GroupColumn groupColumn, RenderContext context)
        {
            return true;
        }

        protected virtual bool IsVisibleTotalRow(TRow row, GroupColumn groupColumn, int count)
        {
            return IsVisibleTotalRow(row, groupColumn);
        }

        protected virtual bool IsVisibleTotalRow(TRow row, GroupColumn groupColumn)
        {
            return true;
        }

        protected virtual RowProperties GetDefaultRowProperties(BaseJournalRow<TRow> row)
        {
            return null;
        }

        protected virtual RowProperties GetDefaultGroupRowProperties(BaseJournalRow<TRow> row, GroupColumn groupColumn)
        {
            return null;
        }

        protected virtual RowProperties GetDefaultTotalRowProperties(BaseJournalRow<TRow> row, GroupColumn groupColumn)
        {
            return null;
        }

        /*public BaseColumn.DataItemKey? GetDataItemKey(TRow row)
        {
            BaseColumn.DataItemKey? value = null;
            var groupValues = GetGroupRowValues(row);
            for (int i = 0; i < IsInlineGroup.Length; i++)
            {
                if (!IsInlineGroup[i] || (GroupColumns[i].GroupType & GroupType.InHeader) == 0) continue;
                value = new BaseColumn.DataItemKey(groupValues[i] == null ? "" : groupValues[i].ToString(),
                                                   GroupColumns[i].ColumnName, value);
            }
            return value;
        }*/

        #region RaiseGroupCommand and its methods

        protected virtual bool RaiseGroupCommand(string eventArgument)
        {
            if (eventArgument.Equals(GroupByClear, StringComparison.OrdinalIgnoreCase))
            {
                if (ClearGrouping())
                    OnGroupingChanged(EventArgs.Empty);
                return true;
            }

            if (!eventArgument.StartsWith(GroupByColumn, StringComparison.OrdinalIgnoreCase))
                return false;

            var split = eventArgument.Substring(GroupByColumn.Length).Split(':');
            var raiseEvent = false;
            if (split.Length == 1)
                raiseEvent = AddGroupBy(split[0]);
            else if (split[1].Equals("MoveUp", StringComparison.OrdinalIgnoreCase))
                raiseEvent = MoveUpGroupColumn(split[0]);
            else if (split[1].Equals("MoveDown", StringComparison.OrdinalIgnoreCase))
                raiseEvent = MoveDownGroupColumn(split[0]);
            else if (split[1].Equals("Remove", StringComparison.OrdinalIgnoreCase))
                raiseEvent = RemoveGroupColumn(split[0]);
            if (raiseEvent)
                OnGroupingChanged(EventArgs.Empty);
            return true;
        }

        protected virtual bool AddGroupBy(GroupColumn groupColumn)
        {
            var index = GroupColumns.IndexOf(groupColumn);
            if ((index < 0 || GroupColumns[index].GroupType != groupColumn.GroupType)
                && InnerHeader.ColumnsDic.ContainsKey(groupColumn.ColumnName))
            {
                if (index < 0)
                    GroupColumns.Add(groupColumn);
                else
                    GroupColumns[index] = groupColumn;
                var column = InnerHeader.ColumnsDic[groupColumn.ColumnName];
                if (!column.InlineGrouping)
                {
                    /*if (LocalizationHelper.IsCultureKZ && !string.IsNullOrEmpty(column.OrderColumnNameKz))
                        AddOrderByColumn(column.OrderColumnNameKz, "Asc", 0);
                    else if (!string.IsNullOrEmpty(column.OrderColumnNameRu))
                        AddOrderByColumn(column.OrderColumnNameRu, "Asc", 0);
                    else if (!string.IsNullOrEmpty(column.OrderColumnName))
                        AddOrderByColumn(column.OrderColumnName, "Asc", 0);
                    else*/
                    AddOrderByColumn(groupColumn.ColumnName, "Asc", 0);
                }
                return true;
            }
            return false;
        }

        protected virtual bool MoveUpGroupColumn(GroupColumn groupColumn)
        {
            var index = GroupColumns.IndexOf(groupColumn);
            if (index > 0)
            {
                groupColumn = GroupColumns[index];
                GroupColumns.RemoveAt(index);
                GroupColumns.Insert(index - 1, groupColumn);
                return true;
            }
            return false;
        }

        protected virtual bool MoveDownGroupColumn(GroupColumn groupColumn)
        {
            var index = GroupColumns.IndexOf(groupColumn);
            if (index < GroupColumns.Count - 1)
            {
                groupColumn = GroupColumns[index];
                GroupColumns.RemoveAt(index);
                GroupColumns.Insert(index + 1, groupColumn);
                return true;
            }
            return false;
        }

        protected virtual bool ClearGrouping()
        {
            if (GroupColumns.Count == 0) return false;
            for (int i = GroupColumns.Count - 1; i >= 0; i--)
                if (GroupColumns[i].GroupType != GroupType.InHeader)
                    GroupColumns.RemoveAt(i);
            return true;
        }

        protected virtual bool RemoveGroupColumn(GroupColumn groupColumn)
        {
            if (groupColumn.GroupType == GroupType.InHeader) return false;
            return GroupColumns.Remove(groupColumn);
        }

        #endregion

        protected virtual bool? ExistsGroupValue(TRow row, GroupColumn groupColumn, params string[] columns)
        {
            if (!columns.Contains(groupColumn.ColumnName))
                return null;

            var type = typeof (TRow);
            var properties = TypeDescriptor.GetProperties(type);
            var property = properties.Find("Item", false);
            if (property != null)
            {
                var item = property.GetValue(row);
                if (item == null)
                    return false;

                type = item.GetType();
                properties = TypeDescriptor.GetProperties(type);
                property = properties.Find(groupColumn.ColumnName, false);
                if (property != null)
                    return property.GetValue(item) != null;
            }

            throw new ArgumentException(
                string.Format("Не найдено свойство '{0}' в типе {1}", groupColumn.ColumnName, type.FullName),
                "groupColumn");
        }

        #endregion
    }

    [PermissionSet(SecurityAction.Demand, Name = "FullTrust")]
    [ParseChildren(true)]
    public abstract class BaseJournalControl<TKey, TTable, TRow, TDataContext> : BaseJournalControl<TRow>
        where TKey : struct
        where TTable : class
        where TRow : BaseRow, new()
        where TDataContext : DataContext, new()
    {
        #region fields
        private AdditionalButtons _buttons;
        private readonly List<BaseJournalRow<TKey, TTable, TRow, TDataContext>> _rows = new List<BaseJournalRow<TKey, TTable, TRow, TDataContext>>();
        private readonly List<BaseJournalRow<TKey, TTable, TRow, TDataContext>> _grouprows = new List<BaseJournalRow<TKey, TTable, TRow, TDataContext>>();
        private readonly List<BaseJournalRow<TKey, TTable, TRow, TDataContext>> _totalrows = new List<BaseJournalRow<TKey, TTable, TRow, TDataContext>>();

        //private readonly Dictionary<string, int> addIds = new Dictionary<string, int>();
        
        private TDataContext _db;

        #endregion

        #region abstracts methods

        public abstract TKey? GetKey(TRow row);
        public abstract bool IsEquals(TKey? key1, TKey? key2);
        public abstract bool IsEquals(TRow row, TKey? key);

        #endregion

        #region Properies

        public override bool EnableViewState
        {
            get
            {
                return false;
            }
        }

        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public virtual TKey? SelectedValueKey { get; set; }

        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public virtual TDataContext DB 
        {
            get
            {
                if (_db == null)
                {
                    var dataSourceView = GetData() as BaseDataSourceView<TKey, TTable, TDataContext, TRow>;
                    _db = dataSourceView == null ? new TDataContext() : dataSourceView.DB;
                }
                return _db;
            }
            set
            {
                _db = value;
                var dataSourceView = GetData() as BaseDataSourceView<TKey, TTable, TDataContext, TRow>;
                if (dataSourceView != null)
                    dataSourceView.DB = _db;
            }
        }

        public override DataContext DataContext
        {
            get { return DB; }
        }

        public List<BaseJournalRow<TKey, TTable, TRow, TDataContext>> Rows
        {
            get
            {
                EnsureChildControls();
                return _rows;
            }
        }

        public List<BaseJournalRow<TKey, TTable, TRow, TDataContext>> GroupRows
        {
            get
            {
                EnsureChildControls();
                return _grouprows;
            }
        }

        #endregion

        #region methods

        protected override void InitializeGroupColumns()
        {
            base.InitializeGroupColumns();
            foreach (var group in GetDefaultGroupColumns())
                GroupColumns.Add(group);
        }

        public virtual void InitializeControls(RvsSavedProperties properties)
        {
            GetData().Select(CreateDataSourceSelectArguments(), dataSource => CreateDataRows(dataSource));
        }

        public virtual void InitializeGroupControl()
        {
        }

        public virtual void InitializeControls()
        {
            IsExport = true;
            SelectingColumnControl = new SelectingColumn
                {
                    Journal = this,
                };

            GroupControl = new BaseGroupingControl<TRow>
                {
                    Journal = this,
                    ID = "GroupingControl",
                };
            InitializeGroupControl();

            Controls.Add(GroupControl);
        }

        protected override int CreateChildControls(IEnumerable dataSource, bool dataBinding)
        {
            if (SelectingColumnControl == null)
            {
                SelectingColumnControl = (SelectingColumn)Page.LoadControl("/GenerationClasses/SelectingColumn.ascx");
                SelectingColumnControl.Journal = this;
                SelectingColumnControl.ID = "sc";
                SelectingColumnControl.EnableViewState = false;
            }

            Controls.Add(SelectingColumnControl);

            if (SavingJournalSettingsControl == null)
            {
                SavingJournalSettingsControl =
                    (SavingJournalSettings)Page.LoadControl("/GenerationClasses/SavingJournalSettings.ascx");
                SavingJournalSettingsControl.Journal = this;
                SavingJournalSettingsControl.ID = "sjs";
                SavingJournalSettingsControl.EnableViewState = false;
            }

            Controls.Add(SavingJournalSettingsControl);

            if (GroupControl == null)
            {
                GroupControl = new BaseGroupingControl<TRow>
                                   {
                                       Journal = this,
                                       ID = "GroupingControl",
                                   };
                InitializeGroupControl();
            }

            Controls.Add(GroupControl);

            if (dataBinding)
                return CreateDataRows(dataSource);
            return 0;
        }

        private int CreateDataRows(IEnumerable dataSource)
        {
            _rows.Clear();
            _grouprows.Clear();
            _totalrows.Clear();
            InitializeGroupTypes();
            if (InnerHeader.ColumnHierarchy != null)
                foreach (var columnHierarchy in InnerHeader.ColumnHierarchy)
                {
                    columnHierarchy.DetectColSpan(InnerHeader.ColumnsDic);
                    columnHierarchy.DetectRowSpan(InnerHeader.GetMaxRowSpan(), InnerHeader.ColumnsDic);
                    columnHierarchy.InitColumnParams(InnerHeader.ColumnsDic);
                }

            Controls.Add(InnerHeader);
            _rows.Clear();

            RowNumberingTable = PageIndex * PageSize;
            for (int i = 0; i < GroupColumns.Count; i++)
            {
                RowNumbering.Add(0);
                GroupNumbering.Add(0);
                RowContinuousNumbering.Add(0);
            }

            int count = 0;
            if (dataSource != null)
            {
                BaseJournalRow<TKey, TTable, TRow, TDataContext> row;
                BaseJournalRow<TKey, TTable, TRow, TDataContext> previousRow = null;
                IEnumerator e = dataSource.GetEnumerator();

                while (e.MoveNext())
                {
                    var dataRow = e.Current as TRow;
                    if (dataRow != null)
                    {
                        count = EnsureAddedTotalRows(dataRow, previousRow, count);
                        count = EnsureAddedGroupRows(dataRow, count);
                        var visibleRow = IsVisibleRow(dataRow);
                        row = GetRow(count, dataRow);
                        RowNumberingTableCustomChange = ++RowNumberingTable;
                        SplitRowNumberingTable = 1;
                        for (int i = 0; i < GroupColumns.Count; i++)
                        {
                            RowNumbering[i]++;
                            RowContinuousNumbering[i]++;
                        }

                        if (SelectedValueKey != null && IsEquals(dataRow, SelectedValueKey))
                        {
                            SelectedDataRow = dataRow;
                            row.Selected = true;
                        }

                        if (visibleRow)
                            AddRow(row);
                        row.InitializeControls(previousRow, null);
                        row.InitializeRowSpan();
                        if(visibleRow)
                            AddDefaultRowProperties(row, GetDefaultRowProperties(row));
                        previousRow = row;

                        var rowsCount = row.MaxRowsCount;
                        var previousInlineRow = row;
                        while (rowsCount > 1)
                        {
                            SplitRowNumberingTable++;
                            rowsCount--;
                            var childRow = GetRow(count, dataRow);
                            childRow.SplitedRowIndex = row.MaxRowsCount - rowsCount;
                            if (SelectedValueKey != null && IsEquals(dataRow, SelectedValueKey))
                            {
                                SelectedDataRow = dataRow;
                                childRow.Selected = true;
                            }

                            if (visibleRow)
                                AddRow(childRow);
                            childRow.InitializeControls(previousRow, previousInlineRow);
                            childRow.InitializeRowSpan();
                            if (visibleRow)
                                AddDefaultRowProperties(row, GetDefaultRowProperties(childRow));
                            previousRow = previousInlineRow = childRow;
                        }

                        RowNumberingTable = RowNumberingTableCustomChange;
                        ++count;
                    }
                }

                if (count > 0)
                {
                    count = EnsureAddedTotalRows(null, previousRow, count);
                    foreach (var column in InnerHeader.Columns)
                        column.FinishCompute();
                    EnsureGroupRowsColSpanRight();
                }
            }
            else
                throw new ArgumentNullException("dataSource");
            return count;
        }

        protected override void InitializeGroupTypes()
        {
            foreach (var column in InnerHeader.Columns)
            {
                column.UsingInGroup = false;
                column.Tab = 0;
            }
            int tab = 0;
            for (int i = 0; i < GroupColumns.Count; i++)
            {
                var column = InnerHeader.ColumnsDic[GroupColumns[i].ColumnName];
                column.UsingInGroup = true;
                column.GroupType = GroupColumns[i].GroupType;
                /*if (!column.AllowAggregate)
                    column.AllowAggregate = (GroupColumns[i].GroupType & GroupType.Total) == GroupType.Total;*/
                if (!column.InlineGrouping)
                    column.Tab = tab++;
            }
        }

        protected override void ExportToExcel()
        {
            HttpContext.Current.Items[((BaseFilter<TKey, TTable, TDataContext>)Filter).FilterControl.GetTableName() + ".FiltersCache"] = null;
            string extention;
            var stream = WebSpecificInstances.GetExcelExporter().GetExcelByType(
                ParentUserControl.GetType(),
                RvsSavedProperties.GetFromJournal(ParentUserControl),
                ParentUserControl.LogMonitor,
                true,
                out extention);
            PageHelper.DownloadFile(stream, ParentUserControl.TableHeader.Replace("\r\n", " ") + "." + extention, Page.Response);
        }

        #region Groups, Totals

        private void EnsureGroupRowsColSpanRight()
        {
            if (_grouprows.Count == 0) return;
            var colSpan = InnerHeader.GetFullColSpan();
            foreach (var groupRow in _grouprows)
            {
                var cell = groupRow.Controls[0] as BaseJournalCell<TRow>;
                if (cell != null) cell.ColSpan = colSpan;
            }
        }

        protected object[] PrevRowValues { get; private set; }

        protected override object[] GetGroupRowValues(TRow row)
        {
            var values = new object[GroupColumns.Count];
            var itemInfo = typeof(TRow).GetProperty("Item");
            var item = (TTable)itemInfo.GetValue(row, null);
            for (int i = 0; i < GroupColumns.Count; i++)
            {
                var column = InnerHeader.ColumnsDic[GroupColumns[i].ColumnName];
                if (column.InlineGrouping
                    || column.GroupType == GroupType.InHeader)
                    continue;

                if (column.GetValueForGroup != null)
                    values[i] = column.GetValueForGroup(row);
                else
                {
                    var valueInfo = typeof(TTable).GetProperty(GroupColumns[i].ColumnName);
                    values[i] = valueInfo.GetValue(item, null);
                }
            }

            return values;
        }

        protected virtual int EnsureAddedGroupRows(TRow row, int count)
        {
            var values = GetGroupRowValues(row);
            if (PrevRowValues == null)
                count = AddGroupRows(row, count, 0, values);
            else
            {
                for (int i = 0; i < GroupColumns.Count; i++)
                {
                    if (InnerHeader.ColumnsDic[GroupColumns[i].ColumnName].InlineGrouping
                        || InnerHeader.ColumnsDic[GroupColumns[i].ColumnName].GroupType == GroupType.InHeader
                        || (values[i] != null && values[i].Equals(PrevRowValues[i]))
                        || (values[i] == null && PrevRowValues[i] == null))
                    {
                        continue;
                    }

                    count = AddGroupRows(row, count, i, values);
                    break;
                }
            }

            PrevRowValues = values;
            return count;
        }

        protected virtual int AddGroupRows(TRow dataRow, int count, int startIndex, object[] values)
        {
            /*BaseJournalRow<TKey, TTable, TRow, TDataContext> groupRow = null;
            if (_grouprows.Count > 0)
             {
                 groupRow = _grouprows[_grouprows.Count - 1];
                 for (int i = GroupColumns.Count - 1; i > startIndex; i--)
                 {
                     if (groupRow == null) break;
                     if (InnerHeader.ColumnsDic[GroupColumns[i]].InlineGrouping) continue;
                     groupRow = groupRow.GroupRow;
                 }
             }*/
            for (int i = startIndex; i < GroupColumns.Count; i++)
            {
                RowNumbering[i] = 0;
                if (i == startIndex)
                    GroupNumbering[i]++;
                else
                    GroupNumbering[i] = 1;
                var column = InnerHeader.ColumnsDic[GroupColumns[i].ColumnName];
                if (column.InlineGrouping
                    || InnerHeader.ColumnsDic[GroupColumns[i].ColumnName].GroupType == GroupType.InHeader
                    || (column.GroupType & GroupType.Top) != GroupType.Top)
                    continue;

                if (!IsVisibleGroupRow(dataRow, GroupColumns[i])) continue;

                var row = GetGroupRow(count++, dataRow, column, values);

                if (SelectedValueKey != null && IsEquals(dataRow, SelectedValueKey))
                {
                    SelectedDataRow = dataRow;
                    row.Selected = true;
                }

                _grouprows.Add(row);
                AddRow(row);
                row.InitializeControls();
                AddDefaultRowProperties(row, GetDefaultGroupRowProperties(row, GroupColumns[i]));
                if (row.RenderContext.OtherColumns.ContainsKey(GroupColumns[i])
                    && !IsVisibleGroupRow(dataRow, GroupColumns[i], row.RenderContext.OtherColumns[GroupColumns[i]]))
                {
                    count--;
                    row.Visible = false;
                }
            }

            return count;
        }

        protected virtual int EnsureAddedTotalRows(TRow row, BaseJournalRow<TKey, TTable, TRow, TDataContext> previousRow, int count)
        {
            if (!ShowTotals) return count;
            if (row == null)
            {
                count = AddTotalRows(row, previousRow, count, 0, PrevRowValues);

                if (ShowFullTotal)
                {
                    var controlRow = GetTotalRow(count++, row, null, new object[0]);
                    _totalrows.Add(controlRow);
                    AddRow(controlRow);
                    controlRow.InitializeControls();
                    AddDefaultRowProperties(controlRow, GetDefaultTotalRowProperties(controlRow, null));
                }
                foreach (var customTotal in CustomTotals)
                {
                    var controlRow = GetTotalRow(count++, null, null, new object[0]);
                    controlRow.CurrentCustomTotalRow = customTotal;
                    _totalrows.Add(controlRow);
                    AddRow(controlRow);
                    controlRow.InitializeControls();
                    AddDefaultRowProperties(controlRow, GetDefaultTotalRowProperties(controlRow, null));
                }
                return count;
            }
            var values = GetGroupRowValues(row);
            if (PrevRowValues != null)
            {
                for (int i = 0; i < GroupColumns.Count; i++)
                {
                    if (/*InnerHeader.ColumnsDic[GroupColumns[i].ColumnName].InlineGrouping
                        || */(values[i] != null && values[i].Equals(PrevRowValues[i]))
                        || (values[i] == null && PrevRowValues[i] == null))
                    {
                        continue;
                    }
                    count = AddTotalRows(row, previousRow, count, i, PrevRowValues);
                    break;
                }
            }
            return count;
        }

        protected virtual int AddTotalRows(TRow dataRow, BaseJournalRow<TKey, TTable, TRow, TDataContext> previousRow, int count, int startIndex, object[] values)
        {
            bool addRows = false;
            var first = true;
            for (int i = GroupColumns.Count - 1; i >= startIndex; i--)
            {
                var groupColumn = GroupColumns[i];
                if (InnerHeader.ColumnsDic[groupColumn.ColumnName].GroupType == GroupType.InHeader) continue;
                if (first)
                {
                    if (InnerHeader.ColumnsDic[groupColumn.ColumnName].InlineGrouping)
                        continue;
                    first = false;
                }
                if (!IsVisibleTotalRow(previousRow == null ? null : previousRow.DataItem, groupColumn, count)) continue;
                var column = InnerHeader.ColumnsDic[groupColumn.ColumnName];
                if ((column.GroupType & GroupType.Total) != GroupType.Total) continue;

                addRows = true;
                var totalValues = values == null ? null : values.Take(i + 1).ToArray();
                previousRow = AddTotalRows(previousRow, count, totalValues, groupColumn, column, i);
            }
            return count + (addRows ? 1 : 0);
        }

        private BaseJournalRow<TKey, TTable, TRow, TDataContext> AddTotalRows(BaseJournalRow<TKey, TTable, TRow, TDataContext> previousRow,
            int count, object[] totalValues, GroupColumn groupColumn, BaseColumn column, int i)
        {
            var row = GetTotalRow(count, previousRow == null ? null : previousRow.DataItem, column, totalValues);
            _totalrows.Add(row);
            AddRow(row);
            row.InitializeControls(previousRow, null, i);
            row.InitializeRowSpan();
            AddDefaultRowProperties(row, GetDefaultTotalRowProperties(row, groupColumn));

            var rowsCount = row.MaxRowsCount;
            var previousInlineRow = row;
            while (rowsCount > 1)
            {
                rowsCount--;
                var childRow = GetTotalRow(count, previousRow == null ? null : previousRow.DataItem, column, totalValues);
                childRow.SplitedRowIndex = row.MaxRowsCount - rowsCount;
                AddRow(childRow);
                childRow.InitializeControls(previousRow, previousInlineRow);
                AddDefaultRowProperties(childRow, GetDefaultTotalRowProperties(childRow, groupColumn));
                previousRow = previousInlineRow = childRow;
                _totalrows.Add(childRow);
            }
            return previousRow;
        }

        #endregion


/*        protected override void OnInit(EventArgs e)
        {
            base.OnInit(e);
            //метод для возможности изменить ширину и высоту контрола
            ExtenderControlBase extender = new ResizableControlExtender()
                                               {
                                                   BehaviorID = ClientID + "_resizeBeh",
                                                   Size = new Size(600, 480),
                                                   MinimumHeight = 100,
                                                   MinimumWidth = 200,
                                                   ResizableCssClass = "ResizableCssClass",
                                                   HandleCssClass = "HandleCssClass",
                                               };
            ParentUserControl.ExtenderAjaxControl.AddExtender(extender, ClientID + "_container");
        }*/

        protected override void SaveJournalValuesClick(string rowKey)
        {
            var notSavedJournalRows = new Dictionary<string, string>();
            try
            {
                EnsureDataBound();
                DB.Connection.Open();
                BeginSave();
                foreach (var journalRow in Rows)
                {
                    if (journalRow.TotalValues != null || journalRow.GroupColumn != null
                        || (!string.IsNullOrEmpty(rowKey) && !rowKey.Equals(journalRow.RowKey)))
                        continue;

                    SaveJournalRow(journalRow, notSavedJournalRows);
                }
                EndSave();
            }
            finally 
            {
                DB.Connection.Close();
            }
            DB = null;
            if (string.IsNullOrEmpty(rowKey) || notSavedJournalRows.Count == 0)
            {
                //EndEditRowKeys.Add(rowKey);//когда будет запрашиваться информация о возможности редактирования, проставится окончание редактирования
                var dic = (IDictionary)HttpContext.Current.Items["AllowEditRowInfo"];
                if (dic != null) dic.Clear();//очищаем кеш
                NeedSaveButton = false;

                RowNumbering.Clear();
                RowNumberingTable = 0;
                RowNumberingTableCustomChange = 0;
                RowContinuousNumbering.Clear();
                GroupNumbering.Clear();
                SplitRowNumberingTable = 0;

                _rows.Clear();
                _grouprows.Clear();
                _totalrows.Clear();
                PrevRowValues = null;
                //addIds.Clear();
                ((BaseDataSourceView<TKey, TTable, TDataContext, TRow>)GetData()).ClearCustomCache();
                DataBind();
                if (notSavedJournalRows.Count > 0)
                    foreach (var journalRow in Rows)
                    {
                        if (!notSavedJournalRows.ContainsKey(journalRow.RowKey))
                            continue;
                        journalRow.ProcessPostData(Page.Request.Params);
                        journalRow.AddErrorMessage(notSavedJournalRows[journalRow.RowKey]);
                    }
            }
        }

        protected virtual void EndSave()
        {
        }

        protected virtual void BeginSave()
        {
        }

        public virtual void SaveJournalRow(BaseJournalRow<TKey, TTable, TRow, TDataContext> journalRow, Dictionary<string, string> notSavedJournalRows)
        {
            var openedThere = false;
            if (DB.Connection.State != ConnectionState.Open)
            {
                DB.Connection.Open();
                openedThere = true;
            }

            try
            {
                var transaction = DB.Connection.BeginTransaction();
                try
                {
                    DB.Transaction = transaction;
                    var saved = SaveJournalRowValues(journalRow);
                    if (saved != null && saved.Value && SaveJournalRowSubmitChanges(DB, journalRow))
                    {
                        if (ParentUserControl.UpdateMessageLog != null)
                            ParentUserControl.LogMonitor.Log(ParentUserControl.UpdateMessageLog.Value, journalRow.GetLogUpdateMessageEntry);
                        transaction.Commit();
                        EndEditRowKeys.Add(journalRow.RowKey);
                    }
                    else
                    {
                        transaction.Rollback();
                        transaction.Dispose();
                        transaction = null;

                        // todo: добавить весь список ошибок
                        if (saved != null)
                            notSavedJournalRows.Add(journalRow.RowKey, journalRow.GetErrors().FirstOrDefault());
                        else
                            EndEditRowKeys.Add(journalRow.RowKey);
                    }
                }
                catch (Exception e)
                {
                    if (transaction != null)
                        transaction.Rollback();
                    notSavedJournalRows.Add(journalRow.RowKey, e.ToString());
                    throw;
                }
                finally
                {
                    if (transaction != null)
                        transaction.Dispose();
                }
            }
            finally
            {
                if (openedThere) DB.Connection.Close();
            }
        }
        
        protected virtual bool SaveJournalRowSubmitChanges(TDataContext db, BaseJournalRow<TKey, TTable, TRow, TDataContext> journalRow)
        {
            db.SubmitChanges();
            return true;
        }

        private bool? SaveJournalRowValues(BaseJournalRow<TKey, TTable, TRow, TDataContext> journalRow)
        {
            var count = 0;
            if (!journalRow.ProcessPostData(Page.Request.Params))
                return false;
            foreach (var cell in journalRow.AllCells)
            {
                var journalCell = cell.Value as BaseJournalCell;
                if (journalCell != null && journalCell.RenderContext != null
                    && journalCell.RenderContext.AllowEdit)
                {
                    var column = journalCell.RenderContext.Column;
                    object newValue;
                    if (journalCell.EditColtrol != null)
                        newValue = GetNewValue(journalCell.EditColtrol, journalRow, column, journalCell);
                    else
                        newValue = GetNewValue(journalRow, column, journalCell);
                    //if (newValue == null) return false;
                    var validateArgs = new BaseJournalValidateValueEventArgs(newValue, journalCell.RenderContext);
                    column.ValidateValue(validateArgs);
                    if (validateArgs.Cancel)
                    {
                        journalRow.AddErrorMessages(journalCell.RenderContext.Errors);
                        return false;
                    }
                    if (!journalCell.ValidateValue(newValue == null ? "" : newValue.ToString()))
                    {
                        journalRow.AddErrorMessages(journalCell.RenderContext.Errors);
                        return false;
                    }
                    object oldValue = null;
                    string oldName = null;
                    if (ParentUserControl.UpdateMessageLog != null)
                    {
                        oldValue = column.GetValue(journalCell.RenderContext);
                        oldName = column.GetName(journalCell.RenderContext);
                    }
                    var updateArgs = new BaseJournalUpdateValueEventArgs(newValue, journalCell.RenderContext);
                    column.UpdateValue(updateArgs);
                    if (updateArgs.Cancel)
                    {
                        journalRow.AddErrorMessages(journalCell.RenderContext.Errors);
                        return false;
                    }
                    if (updateArgs.Updated)
                    {
                        if (ParentUserControl.UpdateMessageLog != null)
                        {
                            var newName = column.GetName(journalCell.RenderContext);
                            string rowEntity;
                            if (journalCell.CrossColumnId == null)
                                rowEntity = column.TableNameForLog;
                            else
                                rowEntity = column.TableNameForLog + ": " + journalCell.CrossColumnId;
                            object oldLogValue = GetLogValue(oldValue, oldName);
                            object newLogValue = GetLogValue(newValue, newName);
                            ParentUserControl.LogMonitor.FieldChanged(rowEntity,
                                journalCell.RenderContext.ColumnHierarchy.FullHeaderRu(), oldLogValue, newLogValue);
                        }
                        count++;
                    }
                }
            }
            return count > 0 ? (bool?)true : null;
        }

        private static string GetLogValue(object objValue, string nameValue)
        {
            var value = objValue == null ? null : objValue.ToString();
            if (string.IsNullOrEmpty(value)) 
                return nameValue;
            if (string.IsNullOrEmpty(nameValue))
                return value;
            if (value.Equals(nameValue))
                return value;
            if (objValue is bool) return nameValue;
            return string.Format("{0}: {1}", value, nameValue);
        }

        private object GetNewValue(Control control, BaseJournalRow<TKey, TTable, TRow, TDataContext> journalRow, BaseColumn column, BaseJournalCell journalCell)
        {
            var type = control.GetType();
            var attributes =
                type.GetCustomAttributes(typeof(DefaultPropertyAttribute), true)
                .Cast<DefaultPropertyAttribute>()
                .ToArray();
            if (attributes.Length == 0 || string.IsNullOrEmpty(attributes[0].Name))
                return GetNewValue(journalRow, column, journalCell);
            var property = TypeDescriptor.GetProperties(type).Find(attributes[0].Name, false);
            if (property == null)
                return GetNewValue(journalRow, column, journalCell);
            var newValue = property.GetValue(control);
            if (newValue == null) return null;
            if (newValue.GetType() == column.ValueType)
                return newValue;
            try
            {
                var newValueStr = newValue as string;
                if (newValueStr != null &&
                    (column.ValueType == typeof(decimal)
                     || column.ValueType == typeof(double)
                     || column.ValueType == typeof(float)))
                {
                    var decimalSeparator = Thread.CurrentThread.CurrentUICulture.NumberFormat.NumberDecimalSeparator;
                    newValue = decimalSeparator == "."
                                   ? newValueStr.Replace(",", ".")
                                   : newValueStr.Replace(".", ",");
                }

                return Convert.ChangeType(newValue, column.ValueType);
            }
            catch (Exception e)
            {
                journalRow.AddErrorMessage(e.Message + " (" + column.Header + ")");
                return null;
            }
        }

        private static object GetNewValue(BaseJournalRow<TKey, TTable, TRow, TDataContext> journalRow, BaseColumn column, BaseJournalCell journalCell)
        {
            object newValue;
            var newValueStr = HttpContext.Current.Request.Form[journalCell.RenderContext.UniqueID];
            var decimalSeparator = Thread.CurrentThread.CurrentUICulture.NumberFormat.NumberDecimalSeparator;
            if (newValueStr != null &&
                (column.ValueType == typeof(decimal)
                 || column.ValueType == typeof(double)
                 || column.ValueType == typeof(float)))
            {
                newValueStr = decimalSeparator == "."
                                  ? newValueStr.Replace(",", ".")
                                  : newValueStr.Replace(".", ",");
            }

            /*if (!journalCell.ValidateValue(newValueStr))
            {
                journalRow.AddErrorMessages(journalCell.RenderContext.Errors);
                return null;
            }*/

            if (column.ValueType == typeof (bool))
                newValue = !string.IsNullOrEmpty(newValueStr);
            else
            {
                try
                {
                    newValue = string.IsNullOrEmpty(newValueStr)
                                   ? null
                                   : Convert.ChangeType(newValueStr, column.ValueType);
                }
                catch (Exception e)
                {
                    journalRow.AddErrorMessage(e.Message + " (" + column.Header + ")");
                    return null;
                }
            }
            return newValue;
        }

        protected virtual void InitializeAdditionalButtons(AdditionalButtons buttons)
        {
            var saveMessage = GetSaveMessage();
            if (NeedSaveButton)
            {
                buttons.AddLinkButtonValidationWithSkip(
                    SaveJournalValues, saveMessage, Resources.SInfoNotSaveInvalidated, "",
                    Resources.SQuestionNotSaveInvalidated, SaveValidationGroup);
            }
            bool needSaveButton = Rows.Any(r => r.NeedSaveButton);
            bool needEditButton = Rows.Any(r => r.NeedEditButton);
            bool needCancelButton = Rows.Any(r => r.NeedCancelButton);
            if (needSaveButton)
                buttons.AddImageButton(SaveJournalValues, Themes.IconUrlSave, saveMessage);
            /*{
                buttons.AddLinkButtonValidationWithSkip(
                    SaveJournalValues,
                    string.Format("<img border=0 src=\"{0}\" alt='{1}' />",
                                  Themes.IconUrlSave,
                                  Resources.SSaveValidated),
                    Resources.SInfoNotSaveInvalidated, "",
                    Resources.SQuestionNotSaveInvalidated, "");
            }*/
            if (needEditButton)
                buttons.AddImageButton(EditJournalValues, Themes.IconUrlEdit, Resources.SEditText);
            if (needCancelButton)
                buttons.AddImageButton(CancelJournalValues, Themes.IconUrlCancel, Resources.SCancelText);
            if (needEditButton || needCancelButton || needSaveButton)
                buttons.AddImageButton(RefreshJournalValues, Themes.IconUrlRefresh, Resources.SRefresh);
        }

        protected virtual string GetSaveMessage()
        {
            return Resources.SSaveValidated;
        }

        protected override DataSourceSelectArguments CreateDataSourceSelectArguments()
        {
            var args = base.CreateDataSourceSelectArguments();
            if (!IsExport)
            {
                if (AllowPaging)
                {
                    args.MaximumRows = PageSize;
                    args.StartRowIndex = PageIndex * PageSize;
                    args.RetrieveTotalRowCount = true;
                }
                else
                    args.MaximumRows = 100;
            }

            if (GroupColumns.Count > 0)
            {
                var addedColumns = new List<string>();
                var insertIndex = 0;
                foreach (var groupColumn in GroupColumns)
                {
                    var column = InnerHeader.ColumnsDic[groupColumn.ColumnName];
                    if (column.OrderColumnName != null)
                    {
                        if (!addedColumns.Contains(column.OrderColumnName))
                        {
                            AddOrderByColumn(column.OrderColumnName, string.Empty, insertIndex++);
                            addedColumns.Add(column.OrderColumnName);
                        }

                        if (groupColumn.ColumnName != column.OrderColumnName)
                            AddOrderByColumn(groupColumn.ColumnName, string.Empty, insertIndex++);
                    }
                    else if (LocalizationHelper.IsCultureKZ && column.OrderColumnNameKz != null)
                    {
                        if (!addedColumns.Contains(column.OrderColumnNameKz))
                        {
                            AddOrderByColumn(column.OrderColumnNameKz, string.Empty, insertIndex++);
                            addedColumns.Add(column.OrderColumnNameKz);
                        }

                        AddOrderByColumn(groupColumn, string.Empty, insertIndex++);
                    }
                    else if (column.OrderColumnNameRu != null)
                    {
                        if (!addedColumns.Contains(column.OrderColumnNameRu))
                        {
                            AddOrderByColumn(column.OrderColumnNameRu, string.Empty, insertIndex++);
                            addedColumns.Add(column.OrderColumnNameRu);
                        }

                        AddOrderByColumn(groupColumn, string.Empty, insertIndex++);
                    }
                }
            }

            args.SortExpression = DefaultOrder;
            return args;
        }

        protected abstract BaseJournalRow<TKey, TTable, TRow, TDataContext> GetRow(int itemIndex, TRow row);

        protected virtual BaseJournalRow<TKey, TTable, TRow, TDataContext> GetGroupRow(int itemIndex, TRow row, BaseColumn column, object[] values)
        {
            var journalRow = GetRow(itemIndex, row);
            journalRow.GroupColumn = column;
            journalRow.GroupKeys = values;
            return journalRow;
        }

        protected virtual BaseJournalRow<TKey, TTable, TRow, TDataContext> GetTotalRow(int itemIndex, TRow row, BaseColumn column, object[] values)
        {
            var journalRow = GetRow(itemIndex, row);
            journalRow.GroupColumn = column;
            journalRow.TotalValues = values;
            return journalRow;
        }

        protected void AddRow(BaseJournalRow<TKey, TTable, TRow, TDataContext> row)
        {
            _rows.Add(row);
            row.ID = row.RowKey;
            /*if (addIds.ContainsKey(row.ID))
            {
                var i = addIds[row.ID];
                addIds[row.ID] = i + 1;
                row.ID = row.ID + "_uindex_" + i;
            }
            else
                addIds.Add(row.ID, 0);*/
            if (DetailsRender)
            {
                var tr = new TableRow();
                var td = new TableCell();
                var up = new UpdatePanel
                             {
                                 ID = "up_" + row.RowKey,
                                 UpdateMode = UpdatePanelUpdateMode.Conditional,
                                 RenderMode = UpdatePanelRenderMode.Block,
                             };

                Controls.Add(tr);
                tr.Controls.Add(td);
                td.Controls.Add(up);
                up.ContentTemplateContainer.Controls.Add(row);
            }
            else
                Controls.Add(row);
        }

        private void AddDefaultRowProperties(BaseJournalRow<TKey, TTable, TRow, TDataContext> row, RowProperties rowProperties)
        {
            if (rowProperties != null && row.RowKey != null && !RowsPropertiesDic.ContainsKey(row.RowKey))
            {
                rowProperties.Key = row.RowKey;
                RowsPropertiesDic[row.RowKey] = rowProperties;
                RowsProperties.Add(rowProperties);
            }
        }

        #endregion

        #region Override methods of Control, interfaces

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            var target = Page.Request.Form["__EVENTTARGET"];
            string uniqueID = UniqueID + "$row_";
            if (target != null && target.StartsWith(uniqueID))
            {
                if (target.EndsWith("$saveButton"))
                {
                    var rowKey = target.Substring(uniqueID.Length,
                                                  target.Length - uniqueID.Length - "$saveButton".Length);
                    SaveJournalValuesClick(rowKey);
                }
                if (target.EndsWith("$edit"))
                {
                    var rowKey = target.Substring(uniqueID.Length,
                                                  target.Length - uniqueID.Length - "$edit".Length);
                    EditRowKey = rowKey;
                }
                if (target.EndsWith("$cancel"))
                {
                    var rowKey = target.Substring(uniqueID.Length,
                                                  target.Length - uniqueID.Length - "$cancel".Length);
                    EndEditRowKeys.Add(rowKey);
                }
            }
        }

        protected override void OnPreRender(EventArgs e)
        {
            base.OnPreRender(e);
            EnsureDataBound();
            GroupControl.Visible = GroupColumns.Count > 0 ||
                                   InnerHeader.Columns.FirstOrDefault(r => r.AllowGrouping) != null;

            var startupScript = $"if (window.Ext != null) Ext.onReady(function() {{ changeWidthCell_RememberSizeOnLoad($get('{ClientID}')); }}); else $(function() {{ changeWidthCell_RememberSizeOnLoad($get('{ClientID}')); }});";

            if (!DetailsRender)
                Page.ClientScript.RegisterStartupScript(GetType(), "RememberSizeOnLoad", startupScript, true);
            _buttons = new AdditionalButtons(Page, this, ParentUserControl.ExtenderAjaxControl);
            InitializeAdditionalButtons(_buttons);
        }

        public override void RenderBeginTag(HtmlTextWriter writer)
        {
            writer.AddStyleAttribute(HtmlTextWriterStyle.Position, "absolute");
            writer.AddStyleAttribute(HtmlTextWriterStyle.Top, GetGridTop());
            writer.AddStyleAttribute("bottom", GetGridBottom());

            var isIE = IsIE();
            if (!isIE)
            {
                writer.AddStyleAttribute(HtmlTextWriterStyle.Left, GetGridLeft());
                writer.AddStyleAttribute("right", GetGridRight());
            }

            writer.RenderBeginTag(HtmlTextWriterTag.Div);
            writer.AddStyleAttribute(HtmlTextWriterStyle.Width, "100%");
            if (!DetailsRender)
            {
                writer.AddStyleAttribute(HtmlTextWriterStyle.Overflow, "scroll");
                writer.AddStyleAttribute(HtmlTextWriterStyle.Height, isIE ? "575px" : "100%");
                writer.AddStyleAttribute(HtmlTextWriterStyle.Position, "relative");
            }

            writer.RenderBeginTag(HtmlTextWriterTag.Div);

            writer.AddAttribute("hfColsID", SelectingColumnControl.GetColHId());
            writer.AddAttribute("hfRowsHID", SelectingColumnControl.GetRowsHId());
            writer.AddAttribute("hfRowsID", SelectingColumnControl.GetRowsId());
            writer.AddAttribute("hfCellsID", SelectingColumnControl.GetCellsId());
            writer.AddAttribute(HtmlTextWriterAttribute.Id, ClientID);
            writer.AddAttribute(HtmlTextWriterAttribute.Class, "ms-vb");
            writer.AddAttribute(HtmlTextWriterAttribute.Cellspacing, "0");
            writer.AddAttribute(HtmlTextWriterAttribute.Rules, "all");
            writer.AddAttribute(HtmlTextWriterAttribute.Border, "1");
            writer.AddStyleAttribute(HtmlTextWriterStyle.BorderCollapse, "collapse");
            writer.AddStyleAttribute(HtmlTextWriterStyle.Width, "100%");
            writer.RenderBeginTag(HtmlTextWriterTag.Table);
        }

        protected virtual string GetGridRight()
        {
            return "0px";
        }

        protected virtual string GetGridLeft()
        {
            return "3px";
        }

        protected virtual string GetGridTop()
        {
            return "150px";
        }

        protected virtual bool AutoDefineTop()
        {
            return true;
        }

        protected virtual string GetPagingBottom()
        {
            if (_buttons.ContainsAny())
                return "37px;";
            return "11px";
        }

        protected virtual string GetButtonsBottom()
        {
            return "11px";
        }

        protected virtual string GetGridBottom()
        {
            var bottom = string.Empty;
            if (PageSize > 0 && AllowPaging && _buttons.ContainsAny())
                bottom = "57px";
            else if (PageSize > 0 && AllowPaging)
                bottom = "31px";
            else if (_buttons.ContainsAny())
                bottom = "37px";
            else
                bottom = "11px";
            return bottom;
        }

        public override void RenderEndTag(HtmlTextWriter writer)
        {
            writer.RenderEndTag();
            writer.RenderEndTag();
            writer.RenderEndTag();
        }

        protected virtual void RenderPaging(HtmlTextWriter writer)
        {
            if (!AllowPaging)
                return;

            int countPages = (int)Math.Ceiling((double)SelectArguments.TotalRowCount / PageSize);

            writer.AddAttribute(HtmlTextWriterAttribute.Class, "crossJournal-Paging");
            if (!IsIE())
            {
                writer.AddStyleAttribute(HtmlTextWriterStyle.Position, "absolute");
                writer.AddStyleAttribute(HtmlTextWriterStyle.Width, "initial");
                writer.AddStyleAttribute("bottom", GetPagingBottom());
                writer.AddStyleAttribute(HtmlTextWriterStyle.Left, GetGridLeft());
                writer.AddStyleAttribute("right", GetGridRight());
            }
            writer.RenderBeginTag(HtmlTextWriterTag.Div);

            writer.AddStyleAttribute(HtmlTextWriterStyle.Display, "inline");
            writer.RenderBeginTag(HtmlTextWriterTag.Div);
            writer.RenderBeginTag(HtmlTextWriterTag.Span);
            writer.Write(Resources.SCountPages, countPages);
            writer.RenderEndTag();
            writer.RenderEndTag();

            var startIndex = PageIndex - PageIndex % 5;
            var endIndex = PageIndex - PageIndex % 5 + 5;

            RenderPagingLinkToPage(writer, 0, "<<", PageIndex > 0);

            if (startIndex > 0)
                RenderPagingLinkToPage(writer, startIndex - 1, "...", true);

            for (int index = startIndex; index < countPages && index < endIndex; index++)
            {
                RenderPagingLinkToPage(writer, index, (index + 1).ToString(), index != PageIndex);
            }

            if (endIndex < countPages)
                RenderPagingLinkToPage(writer, endIndex, "...", true);

            RenderPagingLinkToPage(writer, countPages - 1, ">>", countPages - 1 > PageIndex);

            writer.RenderEndTag();
        }
        
        private void RenderPagingLinkToPage(HtmlTextWriter writer, int index, string text, bool isLink)
        {
            writer.AddStyleAttribute(HtmlTextWriterStyle.Display, "inline");
            writer.RenderBeginTag(HtmlTextWriterTag.Div);

            if (!isLink)
            {
                writer.RenderBeginTag(HtmlTextWriterTag.Span);
                writer.Write(text);
                writer.RenderEndTag();
            }
            else
            {
                writer.AddAttribute(
                    HtmlTextWriterAttribute.Href,
                    Page.ClientScript.GetPostBackClientHyperlink(this, SetPageIndex + index));
                writer.RenderBeginTag(HtmlTextWriterTag.A);
                writer.Write(text);
                writer.RenderEndTag();
            }

            writer.RenderEndTag();
        }

        protected override void Render(HtmlTextWriter writer)
        {
            if (AutoDefineTop())
                writer.Write("<div id='idDivTopDetecter1'></div>");
            writer.RenderBeginTag(HtmlTextWriterTag.Div);
            SelectingColumnControl.RenderControl(writer);
            if (!DetailsRender && GroupPanelVisible)
                GroupControl.RenderControl(writer);
            SavingJournalSettingsControl.RenderControl(writer);
            writer.RenderEndTag();
            if (AutoDefineTop())
                writer.Write("<div id='idDivTopDetecter2'></div>");
            else
                writer.Write("<br />");
            if (DetailsRender)
                writer.Write(_buttons.GetHtml());
            RenderErrorMessages(writer);
            RenderMessages(writer);
            InnerHeader.RenderBackUrls(writer);

            if (AutoDefineTop())
                writer.Write("<div id='idDivTopDetecter3'></div>");
            base.Render(writer);

            RenderPaging(writer);
            RenderButtons(writer);

            //RenderDebugInformation(writer);
        }

        private void RenderDebugInformation(HtmlTextWriter writer)
        {
            writer.AddStyleAttribute(HtmlTextWriterStyle.Display, "none");
            writer.RenderBeginTag(HtmlTextWriterTag.Div);
            
            foreach (var baseColumn in InnerHeader.Columns.Where(r => r._aggregateData != null))
            {
                writer.RenderBeginTag(HtmlTextWriterTag.B);
                writer.Write(baseColumn.ColumnName);
                writer.Write(":");
                writer.RenderEndTag();
                writer.WriteBreak();

                writer.Write(baseColumn._aggregateData.ToString().Replace("\r\n", "<br/>").Replace(" ", "&nbsp;").Replace("\t", "&nbsp;&nbsp;&nbsp;&nbsp;"));
                writer.WriteBreak();
            }

            writer.RenderEndTag();
        }

        protected virtual void RenderButtons(HtmlTextWriter writer)
        {
            writer.AddAttribute(HtmlTextWriterAttribute.Class, "font14");
            if (!IsIE())
            {
                writer.AddStyleAttribute(HtmlTextWriterStyle.Position, "absolute");
                writer.AddStyleAttribute(HtmlTextWriterStyle.Width, "initial");
                writer.AddStyleAttribute("bottom", GetButtonsBottom());
                writer.AddStyleAttribute(HtmlTextWriterStyle.Left, GetGridLeft());
                writer.AddStyleAttribute("right", GetGridRight());
            }
            else
            {
                writer.AddStyleAttribute(HtmlTextWriterStyle.PaddingTop, "10px");
                writer.AddStyleAttribute(HtmlTextWriterStyle.PaddingBottom, "10px");
            }

            writer.RenderBeginTag(HtmlTextWriterTag.Div);
            writer.Write(_buttons.GetHtml());
            writer.RenderEndTag();
        }

        protected override void RenderChildren(HtmlTextWriter writer)
        {
            foreach (Control control in Controls)
            {
                if (control != GroupControl && control != SelectingColumnControl && control != SavingJournalSettingsControl
                    && (!DetailsRender || control != InnerHeader))
                    control.RenderControl(writer);
            }
        }

        #endregion       
    }
}