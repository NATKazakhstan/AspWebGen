/*
 * Created by : Daniil Kovalev
 * Created    : 28.11.2007
 */

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics.CodeAnalysis;
using System.Drawing.Design;
using System.Security.Principal;
using System.Web.UI;
using System.Web.UI.WebControls;
using AjaxControlToolkit;
using Nat.Controls.DataGridViewTools;
using Nat.Tools.Classes;
using Nat.Tools.Constants;
using Nat.Tools.Data;
using Nat.Tools.Filtering;
using Nat.Tools.QueryGeneration;
using Nat.Tools.ResourceTools;
using Nat.Web.Controls.Filters;
using Nat.Web.Controls.Properties;
using Nat.Web.Tools;
using Nat.Web.Tools.Initialization;
using System.Linq;
using Nat.Web.Tools.Security;

#region Resources

[assembly: WebResource("Nat.Web.Controls.Filters.ColumnFilterList.png", "image/png")]
[assembly: WebResource("Nat.Web.Controls.Filters.ColumnFilterList.js", "text/javascript")]
[assembly: WebResource("Nat.Web.Controls.Filters.application_double.png", "image/png")]
[assembly: WebResource("Nat.Web.Controls.Filters.application_cascade.png", "image/png")]
[assembly: WebResource("Nat.Web.Controls.Filters.table_save.png", "image/png")]

#endregion


namespace Nat.Web.Controls
{
    using System.Text;
    using System.Globalization;

    [ClientScriptResource("Nat.Web.Controls.ColumnFilterList", "Nat.Web.Controls.Filters.ColumnFilterList.js")]
    public class ColumnFilterList : ColumnFilterListBase, IScriptControl
    {
        #region Fields

        private List<ColumnFilter> columnFilters;
        private Boolean defaultFullView;
        private Panel filterPanel;
        private Image btnFullBriefView;
        private HiddenField hiddenField;
        private Boolean showFullBriefViewButton;
        private ImageButton btnSaveFiltersState;
        private TableDataSourceView view;
        private DataTable dataTable;
        private Unit filterHeight = Unit.Percentage(100);
        private bool fixHeight = true;

        #endregion

        public override event EventHandler<ColumnFilterListCreatingEventArgs> ColumnFilterListCreating;

        #region Methods

        protected override void CreateChildControls()
        {
            base.CreateChildControls();
            bool initStorage = false;

            // HiddenField
            hiddenField = new HiddenField();
            Controls.Add(hiddenField);

            btnFullBriefView = new Image();
            Controls.Add(btnFullBriefView);
            btnFullBriefView.Style["position"] = "absolute";
            btnFullBriefView.Style["right"] = "25px";
            btnFullBriefView.Style["top"] = "0px";
            btnFullBriefView.ImageUrl = defaultFullView ? BriefViewImageUrl : FullViewImageUrl;
            btnFullBriefView.Style["display"] = showFullBriefViewButton ? "" : "none";
            btnFullBriefView.Style["cursor"] = "hand";

            UpdatePanel buttonUpdatePanel = new UpdatePanel();
            buttonUpdatePanel.ID = "buttonUpdatePanelID";
            buttonUpdatePanel.UpdateMode = UpdatePanelUpdateMode.Conditional;
            Controls.AddAt(1, buttonUpdatePanel);

            btnSaveFiltersState = new ImageButton();
            btnSaveFiltersState.ID = "btnSaveFiltersStateID";
            buttonUpdatePanel.ContentTemplateContainer.Controls.Add(btnSaveFiltersState);
            btnSaveFiltersState.Style["position"] = "absolute";
            btnSaveFiltersState.Style["right"] = "50px";
            btnSaveFiltersState.Style["top"] = "0px";
            btnSaveFiltersState.Style["display"] = "";
            btnSaveFiltersState.ImageUrl = SaveFilterStateImageUrl;
            btnSaveFiltersState.Style["cursor"] = "hand";
            btnSaveFiltersState.Click += BtnSaveFiltersState_OnClick;
            btnSaveFiltersState.ToolTip = LookupControlsResources.SSaveFiltersState;

            // Get DataTable
            view = (TableDataSourceView)GetData();
            dataTable = view.Table;

            // Ensure ColumnFilterStorages
            if (ColumnFilterStorages == null)
            {
                initStorage = true;
                ColumnFilterStorageList columnFilterStorages = new ColumnFilterStorageList();
                foreach (DataColumn dataColumn in dataTable.Columns)
                {
                    Boolean showInFilter = (Boolean)(DataSetResourceManager.GetColumnExtProperty(dataColumn, ColumnExtProperties.SHOW_IN_FILTER) ?? false);
                    var visibleCulture = (string)DataSetResourceManager.GetColumnExtProperty(dataColumn, ColumnExtProperties.SHOW_IN_FILTER_CULTURE);
                    if (!string.IsNullOrEmpty(visibleCulture))
                        showInFilter = showInFilter && visibleCulture.Equals(CultureInfo.CurrentUICulture.Name, StringComparison.CurrentCultureIgnoreCase);
                    
                    if (showInFilter)
                    {
                        ColumnFilterStorage columnFilterStorage = new ColumnFilterStorage();

                        columnFilterStorage.Name = dataColumn.ColumnName;
                        columnFilterStorage.Caption = (String)(DataSetResourceManager.GetColumnExtProperty(dataColumn, ColumnExtProperties.CAPTION) ?? dataColumn.Caption);
                        columnFilterStorage.DataType = dataColumn.DataType;
                        columnFilterStorage.DbType = (NullableHelper.CreateNullable<DbType>(DataSetResourceManager.GetColumnExtProperty(dataColumn, ColumnExtProperties.DB_TYPE)));
                        columnFilterStorage.FilterType = ColumnFilterType.None;
                        columnFilterStorage.IsRefBound = (Boolean)(DataSetResourceManager.GetColumnExtProperty(dataColumn, ColumnExtProperties.FILTER_COLUMN_IS_REF) ?? false);
                        columnFilterStorage.HideInBriefView = (Boolean)(DataSetResourceManager.GetColumnExtProperty(dataColumn, ColumnExtProperties.FILTER_HIDE_IN_BRIEF_VIEW) ?? false);
                        if (columnFilterStorage.IsRefBound)
                        {
                            DataTable filterRefTable = (DataTable)DataSetResourceManager.GetColumnExtProperty(dataColumn, ColumnExtProperties.FILTER_REF_TABLE);
                            columnFilterStorage.TableName = filterRefTable.TableName;
                            columnFilterStorage.RefTableRolledIn = (Boolean)(DataSetResourceManager.GetTableExtProperty(filterRefTable, TableExtProperties.ROLLED_IN) ?? false);
                            columnFilterStorage.DisplayColumn = DataSetResourceManager.GetColumnExtProperty(dataColumn, ColumnExtProperties.FILTER_REF_DISPLAY_COLUMN) as String;
                            columnFilterStorage.ValueColumn = DataSetResourceManager.GetColumnExtProperty(dataColumn, ColumnExtProperties.FILTER_REF_VALUE_COLUMN) as String;
                        }
                        columnFilterStorage.AvailableFilters = columnFilterStorage.GetDefaultFilterTypes();
                        columnFilterStorage.DateTimeFormat = (String)(DataSetResourceManager.GetColumnExtProperty(dataColumn, ColumnExtProperties.FILTER_DATE_TIME_FORMAT));

                        columnFilterStorages.Add(columnFilterStorage);
                    }
                }
                SetColumnFilterStorages(columnFilterStorages);
            }

            if (view.SessionWorker == null)
                throw new Exception("SessionWorker cannot be null");

            // Get data source from session in case when column refers to another table
            DataSet dataSet = view.SessionWorker.Object as DataSet;

            if (dataSet == null)
                throw new Exception("SessionWorker.Object is not DataSet or null");

            foreach(ColumnFilterStorage storage in ColumnFilterStorages)
            {
                if(storage.IsRefBound)
                    storage.RefDataSource = dataSet.Tables[storage.TableName];
            }

            if (ViewState["FiltersStateLoaded"] == null)
            {
                ViewState["FiltersStateLoaded"] = true;
                LoadFiltersState();
            }

            // Filter panel
            filterPanel = new Panel();
            filterPanel.ID = "filterPanelID";
            Controls.Add(filterPanel);
            filterPanel.Height = filterHeight;
            filterPanel.ScrollBars = ScrollBars.Auto;

            // ColumnFilters
            columnFilters = new List<ColumnFilter>();


            var args = new ColumnFilterListCreatingEventArgs();
            OnColumnFilterListCreating(args);
            if (initStorage)
                ColumnFilterStorages.AddRange(args.ListStorages);                
            foreach (ColumnFilterStorage columnFilterStorage in ColumnFilterStorages)
            {
                if(args.ListStorages.Where(s => s.Name.Equals(columnFilterStorage.Name)).Count() > 0)
                {
                    AddColumnFilter(columnFilterStorage);
                    continue;
                }
                string[] rights = (string[])(DataSetResourceManager.GetColumnExtProperty(
                    dataTable.Columns[columnFilterStorage.Name], 
                    ColumnExtProperties.FILTER_RIGHTS) ?? new string[] { });
                Boolean userHasRights = true;
                foreach (string right in rights)
                {
                    userHasRights = UserRoles.IsInRole(Convert.ToString(right));
                    if (!userHasRights) break;
                }

                if (userHasRights) AddColumnFilter(columnFilterStorage);
            }
        }

        private void OnColumnFilterListCreating(ColumnFilterListCreatingEventArgs args)
        {
            if (ColumnFilterListCreating != null)
                ColumnFilterListCreating(this, args);
        }

        private void AddColumnFilter(ColumnFilterStorage columnFilterStorage)
        {
            var id = String.Format("{0}_ColumnFilterID", columnFilterStorage.Name.Replace(".", "_"));
            var columnFilter = new ColumnFilter
                                   {
                                       ID = id,
                                       PopupBehaviorParentNode = PopupBehaviorParentNode,
                                       SessionWorker = view.SessionWorker,
                                       ColumnFilterStorage = columnFilterStorage
                                   };

            filterPanel.Controls.Add(columnFilter);
            columnFilters.Add(columnFilter);
        }

        private void BtnSaveFiltersState_OnClick(object sender, ImageClickEventArgs e)
        {
            SaveFiltersState();
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            if(ChildControlsCreated)
            {
                // Here ColumnFilterStorages is available and can be accessed
                btnFullBriefView.ImageUrl = DefaultFullView ? BriefViewImageUrl : FullViewImageUrl;
            }
            EnsureChildControls();
        }

        protected override void OnPreRender(EventArgs e)
        {
            if(!DesignMode)
                ScriptManager.GetCurrent(Page).RegisterScriptControl(this);
            RequiresDataBinding = false;
            base.OnPreRender(e);
        }

        protected override void EnsureDataBound()
        {
            RequiresDataBinding = false;
            base.EnsureDataBound();
        }

        protected override void Render(HtmlTextWriter writer)
        {
            if(FixHeight)
            {
//                Int32 filterPanelHeight = ColumnFilterStorages.Count * 30;

//                if (filterPanelHeight < 100)
//                    filterPanel.Height = filterPanelHeight;
            }

            if(!DesignMode)
                ScriptManager.GetCurrent(Page).RegisterScriptDescriptors(this);

            base.Render(writer);
        }

        private void SetColumnFilterStorages(ColumnFilterStorageList value)
        {
            ViewState["ColumnFilterStorages"] = value;
        }

        private void SaveFiltersState()
        {
            StorageValues storageValues = new StorageValues();
            bool storageValuesNotSet = true;
            foreach (ColumnFilterStorage storage in ColumnFilterStorages)
            {
                storageValues.AddStorage(storage);
                if (storage.FilterType != ColumnFilterType.None)
                    storageValuesNotSet = false;
            }
            if (storageValuesNotSet)
                storageValues = null;

            var sid = GetSidBytes();

            StorageValues.SetStorageValues(string.Format("{0}_{1}", Page.AppRelativeVirtualPath, ClientID), sid, storageValues);
        }

        private void LoadFiltersState()
        {

            var sid = GetSidBytes();
            // Loading user dependend filters state settings
            // In case there is no user setting apply default settings 
            StorageValues storageValues = StorageValues.GetStorageValues(string.Format("{0}_{1}", Page.AppRelativeVirtualPath, ClientID), sid);
            if (storageValues != null)
            {
                foreach (ColumnFilterStorage storage in ColumnFilterStorages)
                {
                    storageValues.SetStorage(storage);
                }
            }
            else
            {
                WebInitializer.Initialize();
                foreach (ColumnFilterStorage storage in ColumnFilterStorages)
                {
                    ColumnFilterType columnFilterType = (ColumnFilterType)(DataSetResourceManager.GetColumnExtProperty(
                        dataTable.Columns[storage.Name], ColumnExtProperties.FILTER_DEFAULT_CONDITION) ?? ColumnFilterType.None);
                    if (columnFilterType != ColumnFilterType.None)
                    {
                        if(!dataTable.Columns.Contains(storage.Name)) continue;
                        object[] values = new object[2];
                        values[0] = DataSetResourceManager.GetColumnExtProperty(dataTable.Columns[storage.Name], ColumnExtProperties.FILTER_DEFAULT_VALUE_1) ?? null;
                        values[1] = DataSetResourceManager.GetColumnExtProperty(dataTable.Columns[storage.Name], ColumnExtProperties.FILTER_DEFAULT_VALUE_2) ?? null;

                        string codeField = (String)DataSetResourceManager.GetColumnExtProperty(dataTable.Columns[storage.Name], ColumnExtProperties.FILTER_DEFAULT_VALUE_CODE_FIELD);

                        if (storage.IsRefBound && !string.IsNullOrEmpty(codeField))
                        {
                            for(int i = 0; i < values.Length; i++)
                            {
                                if (values[i] != null)
                                {
                                    DataTable table = (DataTable)storage.RefDataSource;

                                    QueryConditionList queryConditionList = new QueryConditionList();
                                    QueryCondition queryCondition = new QueryCondition(codeField, ColumnFilterType.Equal, values[i], null);
                                    queryConditionList.Add(queryCondition);
                                    Type tableAdapterType = TableAdapterTools.GetTableAdapterType(table.GetType());
                                    
                                    QueryGenerator queryGenerator;
                                    
                                    if (table.Columns.IndexOf("dateEnd") != -1 && table.Columns.IndexOf("dateStart") != -1)
                                    {
                                        Component adapter = HistoricalData.GetTableAdapterToHistoricalData("dateEnd", "dateStart", DateTime.Now, tableAdapterType, 0);
                                        queryGenerator = new QueryGenerator(adapter);
                                    }
                                    else
                                        queryGenerator = new QueryGenerator(table);

                                    queryGenerator.TopCount = 1;
                                    queryGenerator.Fill(table, queryConditionList);
                                    if (table.Rows.Count != 0)
                                        values[i] = table.Rows[0][storage.ValueColumn];
                                }
                            }
                            
                        }
                        storage.FilterType = columnFilterType;
                        try
                        {
                            for (int i = 0; i != 2; i++)
                            {
                                if (values[i] != null)
                                    storage.SetValue(i, Convert.ChangeType(values[i], storage.DataType));
                            }
                        }
                        catch (InvalidCastException)
                        {
                        }
                    }
                }
            }
        }

        private byte[] GetSidBytes()
        {
            var sid = new byte[] { };
            switch (this.Page.User.Identity.AuthenticationType)
            {
                case "Windows":
                    var windowsIdentity = (WindowsIdentity)this.Page.User.Identity;
                    sid = new byte[windowsIdentity.User.BinaryLength];
                    windowsIdentity.User.GetBinaryForm(sid, 0);
                    break;
                case "Forms": // note: Получение сида при идентификации по формам. 
                    sid = Encoding.Default.GetBytes(User.GetSID());
                    break;
            }

            return sid;
        }

        #endregion


        #region Properties

        [Category("Behavior")]
        public Unit FilterHeight
        {
            get
            {
                if (filterPanel == null)
                    return filterHeight;
                else
                    return filterPanel.Height;
            }
            set
            {
                filterHeight = value;
                if (filterPanel != null)
                    filterPanel.Height = value;
            }
        }


        [Category("Behavior")]
        [DefaultValue(true)]
        public bool FixHeight
        {
            get { return fixHeight; }
            set { fixHeight = value; }
        }

        public override ColumnFilterStorageList ColumnFilterStorages
        {
            get { return (ColumnFilterStorageList)ViewState["ColumnFilterStorages"]; }
        }

        public override bool ShowFullBriefViewButton
        {
            get { return showFullBriefViewButton; }
            set
            {
                showFullBriefViewButton = value;
                if(btnFullBriefView != null)
                    btnFullBriefView.Style["display"] = value ? "" : "none";
            }
        }

        [DefaultValue("")]
        [UrlProperty]
        [Bindable(true)]
        [SuppressMessage("Microsoft.Design", "CA1056:UriPropertiesShouldNotBeStrings", Justification = "Using string to avoid Uri complications")]
        [Editor("System.Web.UI.Design.ImageUrlEditor, System.Design, Version=2.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a", typeof(UITypeEditor))]
        public string FullViewImageUrl
        {
            get { return (String)(ViewState["FullViewImageUrl"] ?? (DesignMode ? "" : Page.ClientScript.GetWebResourceUrl(GetType(), "Nat.Web.Controls.Filters.application_cascade.png"))); }
            set { ViewState["FullViewImageUrl"] = value; }
        }

        [DefaultValue("")]
        [UrlProperty]
        [Bindable(true)]
        [SuppressMessage("Microsoft.Design", "CA1056:UriPropertiesShouldNotBeStrings", Justification = "Using string to avoid Uri complications")]
        [Editor("System.Web.UI.Design.ImageUrlEditor, System.Design, Version=2.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a", typeof(UITypeEditor))]
        public string BriefViewImageUrl
        {
            get { return (String)(ViewState["BriefViewImageUrl"] ?? (DesignMode ? "" : Page.ClientScript.GetWebResourceUrl(GetType(), "Nat.Web.Controls.Filters.application_double.png"))); }
            set { ViewState["BriefViewImageUrl"] = value; }
        }

        [DefaultValue("")]
        [UrlProperty]
        [Bindable(true)]
        [SuppressMessage("Microsoft.Design", "CA1056:UriPropertiesShouldNotBeStrings", Justification = "Using string to avoid Uri complications")]
        [Editor("System.Web.UI.Design.ImageUrlEditor, System.Design, Version=2.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a", typeof(UITypeEditor))]
        public string SaveFilterStateImageUrl
        {
            get { return (String)(ViewState["SaveFilterStateImageUrl"] ?? (DesignMode ? "" : Page.ClientScript.GetWebResourceUrl(GetType(), "Nat.Web.Controls.Filters.table_save.png"))); }
            set { ViewState["SaveFilterStateImageUrl"] = value; }
        }

        [DefaultValue(false)]
        public bool DefaultFullView
        {
            get
            {
                if (hiddenField != null)
                    return hiddenField.Value == "on";
                else
                    return defaultFullView;
            }
            set
            {
                defaultFullView = value;
                if (hiddenField != null)
                    hiddenField.Value = value ? "on" : "";
            }
        }

        public override string PopupBehaviorParentNode
        {
            get { return (String)ViewState["PopupBehaviorParentNode"]; }
            set { ViewState["PopupBehaviorParentNode"] = value; }
        }

        #endregion


        #region IScriptControl Members

        public IEnumerable<ScriptDescriptor> GetScriptDescriptors()
        {
            if(Page != null && Visible)
            {
                var desc = new ScriptControlDescriptor("Nat.Web.Controls.ColumnFilterList", ClientID);
                var columnFilterProps = new List<Pair>();

                foreach(ColumnFilter columnFilter in columnFilters)
                    columnFilterProps.Add(new Pair(columnFilter.ClientID, columnFilter.ColumnFilterStorage.HideInBriefView));
                desc.AddProperty("hiddenFieldID", hiddenField.ClientID);
                desc.AddProperty("fullBriefViewButtonID", btnFullBriefView.ClientID);
                desc.AddProperty("columnFilterProps", columnFilterProps);
                desc.AddProperty("filterPanelID", filterPanel.ClientID);

                desc.AddProperty("imageControlID", btnFullBriefView.ClientID);

                desc.AddProperty("fullViewImage", FullViewImageUrl);
                desc.AddProperty("briefViewImage", BriefViewImageUrl);
                desc.AddProperty("briefViewText", LookupControlsResources.SChangeToFullView);
                desc.AddProperty("fullViewText", LookupControlsResources.SChangeToBriefView);

                yield return desc;
            }
        }

        public IEnumerable<ScriptReference> GetScriptReferences()
        {
            var references = new List<ScriptReference>();
            references.AddRange(ScriptObjectBuilder.GetScriptReferences(GetType()));
            return references;
        }

        #endregion
    }
}