using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Security.Principal;
using System.Threading;
using System.Web;
using System.Web.UI;
using System.Data.Linq;
using System.Web.UI.WebControls;
using Nat.Web.Controls.Data;
using Nat.Web.Controls.GenerationClasses.Data;
using Nat.Web.Controls.GenerationClasses.Navigator;
using Nat.Web.Controls.Properties;
using Nat.Web.Tools;
using Nat.Web.Tools.Security;

[assembly: WebResource("Nat.Web.Controls.GenerationClasses.CrossJournal.js", "text/javascript")]

namespace Nat.Web.Controls.GenerationClasses.BaseJournal
{
    public abstract class BaseJournalUserControl : BaseHeaderControl, IAccessControl, IScriptControl, IFilterSupport
    {
        private bool _fixedHeader;
        private int _fixedRowsCount;
        private int _fixedColumnsCount;
        private ILogMonitor _logMonitor;
        private ScriptManager _scriptManager;

        protected BaseJournalUserControl()
        {
            ExtenderAjaxControl = new ExtenderAjaxControl();
        }

        public ExtenderAjaxControl ExtenderAjaxControl { get; set; }
        public abstract MainPageUrlBuilder Url { get; set; }
        public abstract BaseJournalControl BaseJournal { get; }
        public abstract BaseFilter BaseFilter { get; }

        public virtual bool OnExportNewSavedProperties { get; } = false;

        public ScriptManager ScriptManager 
        {
            get { return _scriptManager ?? (_scriptManager = ScriptManager.GetCurrent(Page)); } 
        }

        public bool ValuesLoaded { get; set; }

        public bool FixedHeader
        {
            get { return _fixedHeader; }
            set
            {
                _fixedHeader = value;
                if (BaseJournal != null && BaseJournal.SelectingColumnControl != null)
                    BaseJournal.SelectingColumnControl.FixedHeader = value;
            }
        }

        public int FixedRowsCount
        {
            get { return _fixedRowsCount; }
            set
            {
                _fixedRowsCount = value;
                if (BaseJournal != null && BaseJournal.SelectingColumnControl != null)
                    BaseJournal.SelectingColumnControl.FixedRowsCount = value;
            }
        }

        public int FixedColumnsCount
        {
            get { return _fixedColumnsCount; }
            set
            {
                _fixedColumnsCount = value;
                if (BaseJournal != null && BaseJournal.SelectingColumnControl != null)
                    BaseJournal.SelectingColumnControl.FixedColumnsCount = value;
            }
        }

        public string[] ViewRoles { get; set; }
        public string[] ExportRoles { get; set; }
        public StorageValues StorageValues { get; set; }

        public abstract BaseNavigatorControl BaseNavigatorControl { get; }

        public abstract string GetTableName();
        public abstract string GetFilterTableName();
        public abstract string GetSelectMode();

        [DefaultValue("")]
        [IDReferenceProperty]
        [TypeConverter(typeof(FilterConverter))]
        [Themeable(false)]
        public string FilterControl { get; set; }

        public virtual string GetDefaultFilterControl()
        {
            return GetFilterTableName() + "Filter";
        }

        public ILogMonitor LogMonitor
        {
            get
            {
                if (_logMonitor == null)
                {
                    _logMonitor = new LogMonitor();
                    _logMonitor.Init();
                }

                return _logMonitor;
            }
            set { _logMonitor = value; }
        }

        public virtual LogMessageType ExportLog
        {
            get { return LogMessageType.SystemRVSSettingsExport; }
        }

        public virtual LogMessageType LoadSettingsLog
        {
            get { return LogMessageType.SystemRVSSettingsLoadSavedSettings; }
        }

        public virtual LogMessageType SaveSettingsLog
        {
            get { return LogMessageType.SystemRVSSettingsSaveSettings; }
        }

        public virtual LogMessageType ViewLog
        {
            get { return LogMessageType.SystemRVSSettingsView; }
        }

        public virtual LogMessageType DeniedAccessReadLog
        {
            get { return LogMessageType.SystemRVSSettingsDeniedAccess; }
        }

        public virtual LogMessageType DeniedAccessExportLog
        {
            get { return LogMessageType.SystemRVSSettingsDeniedAccess; }
        }

        public virtual long? UpdateMessageLog
        {
            get { return null; }
        }

        public string DefaultReportPluginName { get; set; }

        public bool CheckViewRolesByPlugin { get; set; }

        public bool CheckExportRolesByPlugin { get; set; }

        public string ReportPluginName
        {
            get { return string.IsNullOrEmpty(Url.ReportPluginName) ? DefaultReportPluginName : Url.ReportPluginName; }
        }

        public virtual void CheckReadPermit()
        {
            if (!DoesUserHaveViewRoles())
            {
                LogMonitor.Log(new LogMessageEntry(DeniedAccessReadLog, string.Format(Resources.EUserDoesNotHaveViewPermitions, HeaderRu)));
                UserRoles.UserDoesNotHavePermitions(string.Format(Resources.EUserDoesNotHaveViewPermitions, Header));
            }
        }

        public virtual void CheckExportPermit()
        {
            CheckReadPermit();
            if (!DoesUserHaveExportRoles())
            {
                LogMonitor.Log(new LogMessageEntry(DeniedAccessExportLog, string.Format(Resources.EUserDoesNotHaveExportPermitions, HeaderRu)));
                UserRoles.UserDoesNotHavePermitions(string.Format(Resources.EUserDoesNotHaveExportPermitions, Header));
            }
        }

        public virtual bool DoesUserHaveViewRoles()
        {
            if (CheckViewRolesByPlugin)
            {
                if (string.IsNullOrEmpty(ReportPluginName))
                    return false;
                return UserRoles.DoesHaveUserPermissionToReport(ReportPluginName);
            }

            return ViewRoles == null || ViewRoles.Length == 0 || UserRoles.IsInAnyRoles(ViewRoles);            
        }

        public virtual bool DoesUserHaveExportRoles()
        {
            if (CheckExportRolesByPlugin)
            {
                if (string.IsNullOrEmpty(ReportPluginName))
                    return false;
                return UserRoles.DoesHaveUserPermissionToReport(ReportPluginName);
            }

            return ExportRoles == null || ExportRoles.Length == 0 || UserRoles.IsInAnyRoles(ExportRoles);
        }

        public bool CheckPermit(Page page)
        {
            return DoesUserHaveViewRoles();
        }

        public virtual void PrepareSettings()
        {
        }

        protected override void OnPreRender(EventArgs e)
        {
            if (!IsPostBack)
            {
                EnsureChildControls();
                LogMonitor.Log(new LogMessageEntry(ViewLog, HeaderRu, RvsSavedProperties.GetFromJournal(this)));
            }

            base.OnPreRender(e);
            ScriptManager.RegisterScriptControl(this);
        }

        protected override object SaveViewState()
        {
            return new Pair(StorageValues, base.SaveViewState());
        }

        protected override void LoadViewState(object savedState)
        {
            var pair = (Pair)savedState;
            if (pair != null)
            {
                StorageValues = (StorageValues)pair.First;
                base.LoadViewState(pair.Second);
            }
            else
                base.LoadViewState(null);
        }

        protected internal abstract void InitFilterControl(StorageValues storageValues);

        public abstract IQueryable<BaseRow> GetSelect();

        public IEnumerable<ScriptDescriptor> GetScriptDescriptors()
        {
            return new ScriptDescriptor[0];
        }

        IEnumerable<ScriptReference> IScriptControl.GetScriptReferences()
        {
            return new[]
                {
                    new ScriptReference(
                        "Nat.Web.Controls.GenerationClasses.CrossJournal.js", typeof(BaseJournalUserControl).Assembly.FullName)
                };
        }
    }

    public abstract class BaseJournalUserControl<TDataContext, TFilterControl, TKey, TTable, TDataSource, TRow, TJournal, TNavigatorControl, TNavigatorValues, TFilter> : BaseJournalUserControl
        where TDataContext : DataContext, new()
        where TKey : struct
        where TTable : class
        where TFilterControl : BaseFilterControl<TKey, TTable>, new()
        where TDataSource : BaseDataSource<TKey, TTable, TDataContext, TRow>
        where TRow : BaseRow, new()
        where TJournal : BaseJournalControl<TKey, TTable, TRow, TDataContext>
        where TNavigatorControl : BaseNavigatorControl<TNavigatorValues>
        where TNavigatorValues : BaseNavigatorValues, new()
        where TFilter : BaseFilter<TKey, TTable, TDataContext>, new()
    {
        private MainPageUrlBuilder _url;
        private IFilterControl filterControl;
        private TFilterControl internalfilterControl;
        private ISelectedValue parentControl;
        private bool isFilterSeted = false;
        private TDataContext _db;
        public TNavigatorControl NavigatorControl;
        public TDataSource DataSource;
        protected HyperLink hlFilter;
        protected HyperLink hlCancelFilter;
        protected BaseJournalButtons buttons; 
        protected PlaceHolder phForInnerControls;

        private TJournal _Journal;
        public TJournal Journal 
        {
            get { return _Journal; }
            protected set 
            { 
                _Journal = value;
                value.ParentUserControl = this;
            }
        }

        private TFilter _filter;
        public TFilter Filter
        {
            get
            {
                if(_filter == null)
                {
                    _filter = new TFilter {DB = DB};
                    _filter.Initialize(Journal);
                }
                return _filter;
            }
            protected set { _filter = value; }
        }

        public override BaseJournalControl BaseJournal
        {
            get { return Journal; }
        }

        public override BaseFilter BaseFilter
        {
            get { return Filter; }
        }

        protected BaseJournalUserControl()
        {
            FilterControl = "";
            ParentControl = "";
        }

        public override BaseNavigatorControl BaseNavigatorControl => NavigatorControl;

        public virtual void InitializeControls(RvsSavedProperties properties)
        {
            InitializeControls();
            properties.SetToJournal(this);
            if (properties.StorageValues != null)
            {
                GetFilterControl(DB);
                BaseFilter.SetFiltersByStorageValues(properties.StorageValues, Url);
            }

            Journal.InitializeControls(properties);
            Journal.RowsProperties = properties.DataRowsProperties;
            Journal.CellsProperties = properties.DataCellProperties;
            Journal.InnerHeader.RowsProperties = properties.HeaderRowsProperties;
        }

        public virtual void InitializeControls()
        {
            NavigatorControl.Url = Url;
            Journal.Filter = Filter;
            Journal.InitializeControls();
        }

        protected virtual void InitializeFilterControl(IFilterControl filterControl)
        {
        }

        protected virtual void GetParentControl(ref ISelectedValue control)
        {
        }

        public override void PrepareSettings()
        {
            base.PrepareSettings();
            Journal.PrepareSettings();
        }

        protected void DataSource_Selecting(object sender, DataSourceSelectingEventArgs e)
        {
            e.FilterControl = GetFilterControl((TDataContext)e.DB);
            OnSelecting(e);
        }

        protected virtual void OnSelecting(DataSourceSelectingEventArgs e)
        {
        }
    
        internal IFilterControl FilterControlInternal
        {
            get 
            {
                if (Parent != null && filterControl == null)
                {
                    filterControl = (IFilterControl)(Parent.FindControl(FilterControl) ?? FindControl(FilterControl));
                }
                return filterControl;
            }
        }
    
        [DefaultValue(null)]
        public string FilterByParentControl { get; set; }
    
        [DefaultValue(null)]
        public string ParemeterNameParentControl { get; set; }
        public Type ParentControlTableType { get; set; }

        public override MainPageUrlBuilder Url
        {
            get
            {
                if (HasParentControl && ParentControlTableType != null && _url == MainPageUrlBuilder.Current)
                {
                    _url = MainPageUrlBuilder.Current.Clone();
                    var value = ParentControlInternal.SelectedValue != null ? ParentControlInternal.SelectedValue.ToString() : "";
                    _url.QueryParameters[FilterByParentControl] = value;
                }
                if (_url == null) return MainPageUrlBuilder.Current;
                return _url;
            }
            set { _url = value; }
        }

        public TDataContext DB
        {
            get { return Journal.DB; }
            set { Journal.DB = value; }
        }

        public virtual TDataContext CreateDataContext()
        {
            return new TDataContext();
        }

    
        [Browsable(false)]
        protected bool HasParentControl
        {
            get { return ParentControlInternal != null; }
        }
    
        [DefaultValue("")]
        [IDReferenceProperty]
        [TypeConverter(typeof (ControlIDConverter))]
        [Themeable(false)]
        public virtual string ParentControl { get; set; }
    
        internal ISelectedValue ParentControlInternal
        {
            get
            {
                if (Parent != null && parentControl == null)
                {
                    parentControl = (ISelectedValue)Parent.FindControl(ParentControl);
                    GetParentControl(ref parentControl);
                }
                return parentControl;
            }
            set
            {
                parentControl = value;
            }
        }
    
        private void ParentControl_OnSelectedIndexChanged(object sender, EventArgs e)
        {
            if (ParentControlTableType != null && NavigatorControl != null)
            {
                NavigatorControl.Values[ParentControlTableType] = ParentControlInternal.SelectedValue;
                Url.QueryParameters[FilterByParentControl] = (ParentControlInternal.SelectedValue ?? "").ToString();
            }
    
            isFilterSeted = false;
        }
    
        protected void FilterControl_OnFilterApply(object sender, EventArgs e)
        {
            isFilterSeted = false;
        }

        protected virtual IFilterControl GetFilterControl(TDataContext db)
        {
            return GetFilterControl(db, null);
        }

        protected IFilterControl GetFilterControl(TDataContext db, StorageValues storageValues)
        {
            var filterControlInternal = (TFilterControl)FilterControlInternal ?? internalfilterControl;
            MainPageUrlBuilder pageUrlBuilder;
            if (filterControlInternal == null)
            {
                filterControlInternal = new TFilterControl { Page = Page };
                pageUrlBuilder = new MainPageUrlBuilder
                                     {
                                         SelectMode = GetSelectMode(),
                                         ControlFilterParameters = Url.ControlFilterParameters,
                                         StorageValuesSessionKey = Url.StorageValuesSessionKey,
                                         ReportPluginName = Url.ReportPluginName,
                                         UserControl = GetFilterTableName() + "Journal",
                                     };
                foreach (var parameter in Url.QueryParameters)
                    pageUrlBuilder.QueryParameters.Add(parameter);
                filterControlInternal.CustomFilter = Filter;
                filterControlInternal.SetUrl(pageUrlBuilder);
                internalfilterControl = filterControlInternal;
            }
            else
            {
                pageUrlBuilder = new MainPageUrlBuilder(HttpContext.Current.Request.Url.PathAndQuery);
                filterControlInternal.CustomFilter = Filter;
            }

            filterControlInternal.SetDB(db);
            filterControlInternal.FilterByParentControl = FilterByParentControl;
            filterControlInternal.ParentControl = ParentControlInternal;
    
            filterControlInternal.ShowHistory = Url.ShowHistory;
            var tableName = GetTableName();
            if (storageValues != null)
            {
                var newFilterValue = filterControlInternal.GetFilterByStorageValues(
                    storageValues,
                    Url.GetFilter(tableName));

                Url.SetFilter(tableName, newFilterValue);
                pageUrlBuilder.SetFilter(tableName, newFilterValue);
                Url.CreateUrl();
                pageUrlBuilder.CreateUrl();
                filterControlInternal.SetUrl(pageUrlBuilder);
            }
            else if (Url.GetFilter(tableName) == null)
            {
                if (StorageValues == null && 
                    !string.IsNullOrEmpty(Url.StorageValuesSessionKey) 
                    && HttpContext.Current.Session[Url.StorageValuesSessionKey] != null)
                {
                    StorageValues = (StorageValues) HttpContext.Current.Session[Url.StorageValuesSessionKey];
                }

                var defFilter = filterControlInternal.GetDefaultFilter(StorageValues);
                Url.SetFilter(tableName, defFilter);
                pageUrlBuilder.SetFilter(tableName, defFilter);
                Url.CreateUrl();
                pageUrlBuilder.CreateUrl();
                filterControlInternal.SetUrl(pageUrlBuilder);
            }

            InitializeFilterControl(filterControlInternal);
            filterControlInternal.EnsureFilterInitialize();
            return filterControlInternal;
        }

        protected override void OnInit(EventArgs e)
        {
            if (phForInnerControls != null)
            {
                var panel = new UpdatePanel
                    {
                        UpdateMode = UpdatePanelUpdateMode.Always
                    };
                panel.ContentTemplateContainer.Controls.Add(ExtenderAjaxControl);
                phForInnerControls.Controls.Add(panel);
            }

            Journal.Filter = Filter;
            Journal.FilterChanged +=
                delegate
                    {
                        var filter = (TFilterControl)GetFilterControl(DB);
                        filter.ReParseFilterParameters();
                    };
            if (buttons != null)
                buttons.JournalUC = this;
            DataSource.SelectedQueryParameters +=
                delegate(object sender, SelectedQueryParametersEventArgs selectedQueryParametersEventArgs)
                    {
                        if (selectedQueryParametersEventArgs.QueryParameters != null
                            && selectedQueryParametersEventArgs.QueryParameters.Messages.Count > 0)
                        {
                            Journal.AddMessages(selectedQueryParametersEventArgs.QueryParameters.Messages);
                        }
                    };
            base.OnInit(e);
        }
        
        protected override void OnLoad(EventArgs e)
        {
            if (!IsPostBack && Url.CustomQueryParameters.ContainsKey("LoadView"))
            {
                var dic = (Dictionary<string, long>)HttpContext.Current.Session["LogViewReports2"];
                var loaded = false;
                if (dic != null)
                {
                    var guid = Url.CustomQueryParameters["LoadView"].Value;
                    if (dic.ContainsKey(guid))
                    {
                        var properties = RvsSavedProperties.LoadFrom(dic[guid], LogMonitor);
                        if (!string.IsNullOrEmpty(Url.StorageValuesSessionKey)
                            && HttpContext.Current.Session[Url.StorageValuesSessionKey] != null)
                        {
                            properties.StorageValues = (StorageValues)HttpContext.Current.Session[Url.StorageValuesSessionKey];
                            properties.ReportPluginName = Url.ReportPluginName;
                        }

                        LogMonitor.Log(new LogMessageEntry(ViewLog, properties.NameRu, properties));
                        properties.SetToJournal(this);
                        loaded = true;
                    }
                }

                if (!loaded)
                   Journal.AddErrorMessage(Resources.SSessionIsLost); 
            }

            #region настройка фильтра
    
            if (FilterControlInternal != null)
                FilterControlInternal.FilterApply += FilterControl_OnFilterApply;
            if (ParentControlInternal != null)
                ParentControlInternal.SelectedIndexChanged += ParentControl_OnSelectedIndexChanged;
    
            #endregion

            InitFilterControl(StorageValues);
            Page.LoadComplete += Page_LoadComplete;
        }

        private bool _filterCotrolInited;

        protected internal override void InitFilterControl(StorageValues storageValues)
        {
            if (_filterCotrolInited) return;
            var filter = (BaseFilterControl<TKey>)GetFilterControl(DB, storageValues);
            filter.PostBackFilterControl = BaseJournal;
            filter.PostBackFilterArguments = BaseJournalControl.SetFiltersUncodedUri;
            BaseFilter.SetFilterControl(filter); // Initialize
        }

        protected void NavigatorControl_ValuesInitialized(object sender, EventArgs e)
        {
            if (HasParentControl && ParentControlTableType != null)
            {
                if (ParentControlInternal.SelectedValue != null)
                {
                    NavigatorControl.Values[ParentControlTableType] = ParentControlInternal.SelectedValue;
                    Url.QueryParameters[FilterByParentControl] = ParentControlInternal.SelectedValue.ToString();
                }
            }

            NavigatorControl.Url = Url;
        }

        private void Page_LoadComplete(object sender, EventArgs e)
        {
            if (Url.IsSelect)
            {
                hlFilter.Visible = false;
            }
            else
            {
                var filterUrl = new MainPageUrlBuilder(Url.CreateUrl(true))
                                    {
                                        UserControl = GetDefaultFilterControl(),
                                        IsFilterWindow = true,
                                        SelectMode = GetSelectMode(),
                                        CustomFilterClassName = typeof(TFilter).FullName,
                                    };
                var thisUrl = new MainPageUrlBuilder(Url.Url);
    
                thisUrl.ControlFilterParameters.Clear();
                hlFilter.Attributes["onclick"] = string.Format(
                    "return OpenFilterPostBack(this,'{0}','{1}');",
                    filterUrl.CreateUrl(true),
                    thisUrl.CreateUrl(false));
                hlFilter.NavigateUrl = Page.ClientScript.GetPostBackClientHyperlink(Journal, BaseJournalControl.SetFilters);
                var tableName = GetTableName(); 
                if (!string.IsNullOrEmpty(Url.GetFilter(tableName)))
                {
                    var postBack = Page.ClientScript.GetPostBackClientHyperlink(Journal, BaseJournalControl.ClearFilter);
                    hlCancelFilter.NavigateUrl = postBack;
                    hlCancelFilter.Visible = true;
                }
            }
        }

        protected void HideFilterLinks()
        {
            if (hlFilter != null)
                hlFilter.Visible = false;
            if (hlCancelFilter != null)
                hlCancelFilter.Visible = false;
        }

        protected virtual void OnPreRenderUserControl()
        {
        }

        protected override void OnPreRender(EventArgs e)
        {
            base.OnPreRender(e);
            OnPreRenderUserControl();
            if (Url.ShowFilter)
            {
                hlFilter.Visible = false;
                hlCancelFilter.Visible = false;
            }

            var post = Page.ClientScript.GetPostBackEventReference(Journal, "{0}")
                .Replace("{0}'", BaseJournalControl.SetFilters + "' + value")
                .Replace("{0}\"", BaseJournalControl.SetFilters + "\" + value");
            var script = string.Format(
                @"
var setFilter{2} = function(value) {{
    {0};
}};
var getFilter{2} = function() {{
    return '{1}';
}};",
                post,
                Url.GetFilter(GetTableName()).Replace("\\\"", "\\\\\""),
                GetTableName());

            Page.ClientScript.RegisterClientScriptBlock(GetType(), "getAndSetFilter", script, true);
        }

        public override IQueryable<BaseRow> GetSelect()
        {
            return DataSource.BaseView2.GetSelect(LocalizationHelper.IsCultureKZ, string.Empty).Cast<BaseRow>();
        }
    }
}
