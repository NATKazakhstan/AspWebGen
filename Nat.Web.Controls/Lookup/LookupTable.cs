/*
 * Created by : Daniil Kovalev
 * Created    : 28.11.2007
 */

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Design;
using System.Web.Compilation;
using System.Web.UI;
using System.Web.UI.WebControls;
using AjaxControlToolkit;
using Nat.Tools.Constants;
using Nat.Tools.QueryGeneration;
using Nat.Tools.ResourceTools;
using Nat.Web.Controls.Filters;
using Nat.Web.Controls.Properties;
using Nat.Web.Tools.Security;
using Image=System.Web.UI.WebControls.Image;

#region Resources

[assembly: WebResource("Nat.Web.Controls.Lookup.LookupTable.css", "text/css", PerformSubstitution = true)]
[assembly: WebResource("Nat.Web.Controls.Lookup.LookupTable.png", "image/png")]
[assembly: WebResource("Nat.Web.Controls.Lookup.LookupTable.js", "text/javascript")]
[assembly: WebResource("Nat.Web.Controls.Lookup.control_fastforward_blue.png", "image/png")]
[assembly: WebResource("Nat.Web.Controls.Lookup.control_rewind_blue.png", "image/png")]

#endregion


namespace Nat.Web.Controls
{
    using System.Globalization;

    using Nat.Web.Tools;

    [ClientScriptResource("Nat.Web.Controls.LookupTable", "Nat.Web.Controls.Lookup.LookupTable.js")]
    public class LookupTable : DataBoundControl, IScriptControl, INamingContainer
    {
        #region Fields

        private Button applyButton;
        private Image collapseImage;
        private ColumnFilterListBase columnFilterList;
        private Int32 conditionValue = 1;
        private String dataDisableRowField;
        private Object dataSource;
        private String dataSourceID;
        private GridViewExt gridView;
        private GridViewExtender gridViewExtender;
        private HiddenField hiddenField;
        private Boolean showFullBriefViewButton;
        private Panel gridContainerPanel;
        private Panel gridPanel;
        private Unit gridHeight = Unit.Percentage(99);
        private Boolean allowSorting = true;
        private Boolean allowMultiSelectColumn;
        private String multiSelectColumnCaption;
        private Boolean fillGridViewFlag = false;
        private Boolean postBackOnTreeExpand = true;
        private UpdatePanel updatePanel;
        private String[] dataKeyNames;

        #endregion

        public LookupTable()
        {
            ErrorHeaderText = Resources.SErrorHeaderText;
        }

        #region Methods

        protected override void OnInit(EventArgs e)
        {
            base.OnInit(e);

            if(DesignMode) return;

            Panel panel = new Panel();
            Controls.Add(panel);
            panel.Style["position"] = "relative";

            // Filters' list 
            collapseImage = new Image();
            collapseImage.ID = "collapseImageID";
            panel.Controls.Add(collapseImage);
            collapseImage.Style["position"] = "absolute";
            collapseImage.Style["right"] = "4px";
            collapseImage.Style["top"] = "0px";
            collapseImage.Style["cursor"] = "hand";
            collapseImage.Style["z-index"] = "1";


            Panel filterContainerPanel = new Panel();
            filterContainerPanel.ID = "filterContaiterPanelID";
            panel.Controls.Add(filterContainerPanel);
            filterContainerPanel.GroupingText = LookupControlsResources.SFilterCriteria;

            columnFilterList = GetColumnFilterList();
            columnFilterList.ID = "columnFilterListID";
            columnFilterList.ColumnFilterListCreating += ColumnFilterList_OnColumnFilterListCreating;

            columnFilterList.PopupBehaviorParentNode = ClientID;
            filterContainerPanel.Controls.Add(columnFilterList);
            columnFilterList.DataSourceID = dataSourceID;
            columnFilterList.DataSource = dataSource;
            columnFilterList.ShowFullBriefViewButton = showFullBriefViewButton;

            CollapsiblePanelExtender collapsiblePanelExtender = new CollapsiblePanelExtender();
            panel.Controls.Add(collapsiblePanelExtender);
            collapsiblePanelExtender.TargetControlID = filterContainerPanel.ID;
            collapsiblePanelExtender.ExpandControlID = collapseImage.ID;
            collapsiblePanelExtender.CollapseControlID = collapseImage.ID;
            collapsiblePanelExtender.ImageControlID = collapseImage.ID;

            collapsiblePanelExtender.CollapsedImage =
                Page.ClientScript.GetWebResourceUrl(GetType(), "Nat.Web.Controls.Lookup.control_fastforward_blue.png");
            collapsiblePanelExtender.ExpandedImage =
                Page.ClientScript.GetWebResourceUrl(GetType(), "Nat.Web.Controls.Lookup.control_rewind_blue.png");

            collapsiblePanelExtender.ExpandedText = LookupControlsResources.SHideFilter;
            collapsiblePanelExtender.CollapsedText = LookupControlsResources.SShowFilter;
            collapsiblePanelExtender.CollapsedSize = 17;

            // UpdatePanel for GridView
            updatePanel = new UpdatePanel();
            updatePanel.ID = "updatePanelID";
            updatePanel.UpdateMode = UpdatePanelUpdateMode.Conditional;
            panel.Controls.Add(updatePanel);

            // HiddenField
            hiddenField = new HiddenField();
            hiddenField.ID = "hiddenFieldID";
            //panel.Controls.Add(hiddenField);
            updatePanel.ContentTemplateContainer.Controls.Add(hiddenField);
            hiddenField.ValueChanged += HiddenField_OnValueChanged;


            // GridView
            gridContainerPanel = new Panel();
            gridContainerPanel.ID = "gridContainerPanelID";
            updatePanel.ContentTemplateContainer.Controls.Add(gridContainerPanel);
            gridContainerPanel.Style["clear"] = "both";

            gridPanel = new Panel();
            gridPanel.ID = "gridPanelID";
            gridContainerPanel.Controls.Add(gridPanel);
            gridPanel.ScrollBars = ScrollBars.Auto;
            gridPanel.Height = GridHeight;


            gridView = new GridViewExt();
            gridView.ID = "gridViewID";
            gridView.DataKeyNames = dataKeyNames;
            gridView.UseDeleteField = allowMultiSelectColumn;
            gridView.DeleteFieldColumnCaption = multiSelectColumnCaption;

            gridView.DataSourceID = "666";
            gridView.DataSourceID = "";

            gridView.DataSourceID = dataSourceID;
            gridView.DataSource = dataSource;

            gridPanel.Controls.Add(gridView);
            gridView.EmptyDataText = EmptyDataText;

            gridView.AllowSorting = allowSorting;
            gridView.PageIndexChanged += GridView_OnPageIndexChanged;
            gridView.Sorted += GridView_OnSorted;
            gridView.PagerSettings.Mode = PagerButtons.NumericFirstLast;
            gridView.PagerSettings.PageButtonCount = 5;
            gridView.DataBound += GridView_OnDataBound;

            gridViewExtender = new GridViewExtender();
            gridViewExtender.ID = "gridViewExtenderID";
            gridPanel.Controls.Add(gridViewExtender);
            gridViewExtender.TargetControlID = gridView.ID;
            gridViewExtender.DataDisableRowField = dataDisableRowField;
            gridViewExtender.ConditionValue = conditionValue;

            // Hidden apply filter button
            applyButton = new Button();
            applyButton.ID = "applyButtonID";
            updatePanel.ContentTemplateContainer.Controls.AddAt(0, applyButton);
            applyButton.Click += ApplyButton_OnClick;
            applyButton.Text = "Apply Filter";
            applyButton.Style["float"] = "right";
            applyButton.Style["display"] = "none";
        }

        private void ColumnFilterList_OnColumnFilterListCreating(object sender, ColumnFilterListCreatingEventArgs e)
        {
            if(ColumnFilterListCreating != null) ColumnFilterListCreating(sender, e);
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            if(DesignMode) return;

            TableDataSourceView view = (TableDataSourceView)GetData();
            DataTable dataTable = view.Table;

            Boolean allowPaging = (Boolean)(DataSetResourceManager.GetTableExtProperty(dataTable, TableExtProperties.ALLOW_PAGING) ?? true);

            if (allowPaging)
            {
                Int32 pageSize = (Int32)(DataSetResourceManager.GetTableExtProperty(dataTable, TableExtProperties.PAGE_SIZE) ?? 10);
                gridView.AllowPaging = true;
                gridView.PageSize = pageSize;
                view.EnablePaging = true;
            }
            var isTree = AllowTreeAndPaging || (bool)(DataSetResourceManager.GetTableExtProperty(dataTable, TableExtProperties.IS_TREE_REF) ?? false);
            if (!allowPaging || isTree)
            {
                view.EnablePaging = false;
                if (!isTree)
                    gridView.AllowPaging = false;
                if (GridTreeMode)
                {
                    String Relation = (String)(DataSetResourceManager.GetTableExtProperty(dataTable, TableExtProperties.TREE_REF_RELATION) ?? "");
                    gridView.ShowAsTree = !String.IsNullOrEmpty(Relation);
                    gridView.AllowNewButtonForTree = false;
                    gridView.PostBackOnExpnad = postBackOnTreeExpand;
                    if (gridView.ShowAsTree)
                        gridView.RelationName = Relation;
                }
            }

            gridView.AutoGenerateColumns = false;
            
            if (ViewState["ColumnsCreated"] == null)
            {
                ViewState["ColumnsCreated"] = true;

                if (AllowErrorIconColumn)
                {
                    var field = new TemplateFieldExt
                    {
                        ColumnName = "ErrorIconColumn",
                    };
                    gridView.Columns.Add(field);
                }

                foreach(DataColumn dc in dataTable.Columns)
                {
                    var showColumn = (Boolean)(DataSetResourceManager.GetColumnExtProperty(dc, ColumnExtProperties.VISIBLE) ?? false);
                    var visibleCulture = (string)DataSetResourceManager.GetColumnExtProperty(dc, ColumnExtProperties.VISIBLE_CULTURE);
                    if (!string.IsNullOrEmpty(visibleCulture))
                        showColumn = showColumn && visibleCulture.Equals(CultureInfo.CurrentUICulture.Name, StringComparison.CurrentCultureIgnoreCase);

                    Object[] rights = (Object[])(DataSetResourceManager.GetColumnExtProperty(dc, ColumnExtProperties.RIGHTS) ?? new Object[] {});
                    Boolean userHasRights = true;
                    foreach(Object right in rights)
                    {
                        userHasRights = UserRoles.IsInRole(Convert.ToString(right));
                        if (!userHasRights) break;
                    }

                    if (showColumn && userHasRights)
                    {
                        String columnCaption = (String)(DataSetResourceManager.GetColumnExtProperty(dc, ColumnExtProperties.CAPTION) ?? dc.Caption);
                        if(dc.DataType == typeof(DateTime))
                        {
                            String dataFormatString = (String)(DataSetResourceManager.GetColumnExtProperty(dc, ColumnExtProperties.FILTER_DATE_TIME_FORMAT) ?? null);

                            BoundField boundField = new BoundField();
                            boundField.DataField = dc.ColumnName;
                            boundField.HeaderText = columnCaption;
                            boundField.DataFormatString = dataFormatString;
                            boundField.HtmlEncode = false;
                            boundField.SortExpression = dc.ColumnName;
                            gridView.Columns.Add(boundField);
                        }
                        else if(dc.DataType == typeof(Boolean))
                        {
                            CheckBoxField checkBoxField = new CheckBoxField();
                            checkBoxField.DataField = dc.ColumnName;
                            checkBoxField.HeaderText = columnCaption;
                            checkBoxField.SortExpression = dc.ColumnName;
                            gridView.Columns.Add(checkBoxField);
                        }
                        else
                        {
                            BoundField boundField = new BoundField();
                            boundField.DataField = dc.ColumnName;
                            boundField.HeaderText = columnCaption;
                            boundField.SortExpression = dc.ColumnName;
                            Boolean htmlEncode = (Boolean)(DataSetResourceManager.GetColumnExtProperty(dc, ColumnExtProperties.HTML_ENCODED) ?? false);
                            boundField.HtmlEncode = htmlEncode;
                            gridView.Columns.Add(boundField);
                        }
                    }
                }

                if(AllowErrorColumn)
                {
                    var field = new TemplateFieldExt
                                    {
                                        ColumnName = "ErrorColumn",
                                        HeaderText = ErrorHeaderText,
                                    };
                    gridView.Columns.Add(field);
                }
            }
            if (AllowErrorColumn || AllowErrorIconColumn)
                gridView.RowCreated += GridView_OnRowCreated;
        }

        private void GridView_OnRowCreated(object sender, GridViewRowEventArgs e)
        {
            if (e.Row.RowType != DataControlRowType.DataRow) return;
            TableCell cell = null;
            if(AllowErrorColumn)
                cell = e.Row.Cells[gridView.Columns.Count - 1];
            var index = e.Row.RowIndex;
            long? id = null;
            if (DataKeyNames.Length == 1 && e.Row.DataItem != null) id = (long)DataBinder.Eval(e.Row.DataItem, DataKeyNames[0]);
            var args = new ErrorRowCreatedEventArgs(cell, index, id);
            OnErrorColumnCreated(args);
            if(AllowErrorIconColumn)
            {
                foreach (var iconUrl in args.ErrorIconUrls)
                    e.Row.Cells[1].Controls.Add(new Image {ImageUrl = iconUrl});
            }
        }

        protected virtual void OnErrorColumnCreated(ErrorRowCreatedEventArgs args)
        {
            if (ErrorRowCreated != null) ErrorRowCreated(this, args);
        }

        protected override void OnPagePreLoad(object sender, EventArgs e)
        {
            base.OnPagePreLoad(sender, e);

            if(DesignMode) return;

            if(ChildControlsCreated)
            {
                if (!String.IsNullOrEmpty(hiddenField.Value))
                    gridView.SelectedIndex = Convert.ToInt32(hiddenField.Value);
                else
                    gridView.SelectedIndex = -1;
            }
        }

        protected override void OnPreRender(EventArgs e)
        {
            if (DesignMode) return;
            
            if (!DesignMode)
                ScriptManager.GetCurrent(Page).RegisterScriptControl(this);

            gridView.EmptyDataText = EmptyDataText;
            if (GridViewFilled && columnFilterList.ColumnFilterStorages.QueryConditions.Count != 0)
                gridContainerPanel.GroupingText = String.Format("{0} ({1})", 
                    LookupControlsResources.SData, 
                    LookupControlsResources.SFilterApplied);
            else
                gridContainerPanel.GroupingText = LookupControlsResources.SData;

            if (fillGridViewFlag || (FillGridViewFirstTime && !GridViewFilled))
                FillView();

            RequiresDataBinding = false;

            base.OnPreRender(e);
        }

        protected override void Render(HtmlTextWriter writer)
        {
            if(DesignMode) return;

            if(!DesignMode)
                ScriptManager.GetCurrent(Page).RegisterScriptDescriptors(this);
            base.Render(writer);
        }

        protected void FillView()
        {
            var view = (TableDataSourceView)GetData();
            QueryConditionList queryConditions = GetQueryConditions();
            view.FillType = TableDataSourceFillType.Always;
            SetToViewConditions(queryConditions);

            try
            {
                gridView.DataBind();
                GridViewFilled = true;
                gridView.SelectedIndex = -1;
                hiddenField.Value = String.Empty;
                EmptyDataText = LookupControlsResources.SNoDataByFilterCriteria;
            }
            finally
            {
                RemoveFromViewConditions(queryConditions);
                view.FillType = TableDataSourceFillType.Never;
            }
        }

        public void RemoveFromViewConditions(QueryConditionList queryConditions)
        {
            if (queryConditions != null)
            {
                var view = (TableDataSourceView)GetData();
                foreach (QueryCondition queryCondition in queryConditions)
                    view.CustomConditions.Remove(queryCondition);
            }
        }

        public void SetToViewConditions(QueryConditionList queryConditions)
        {
            if (queryConditions != null)
            {
                var view = (TableDataSourceView)GetData();
                foreach (QueryCondition queryCondition in queryConditions)
                    view.CustomConditions.Add(queryCondition);
            }
        }

        public QueryConditionList GetQueryConditions()
        {
            QueryConditionList queryConditions;
            ColumnFilterStorageList columnFilterStorageList = columnFilterList.ColumnFilterStorages;
            queryConditions = columnFilterStorageList != null
                                  ? columnFilterStorageList.QueryConditions
                                  : new QueryConditionList();

            return queryConditions;
        }

        public override void DataBind()
        {
            if (GridDataInitialized)
            {
                fillGridViewFlag = true;
                updatePanel.Update();
            }
            base.DataBind();
        }

        public void Refresh()
        {
            fillGridViewFlag = true;
            updatePanel.Update();
        }

        private ColumnFilterListBase GetColumnFilterList()
        {
            String filterTypeName;
            if(String.IsNullOrEmpty(FilterControlType))
                filterTypeName = "Nat.Web.Controls.ColumnFilterList";
            else
                filterTypeName = FilterControlType;

            Type filterType = BuildManager.GetType(filterTypeName, true, true);

            if(!typeof(ColumnFilterListBase).IsAssignableFrom(filterType))
                throw new Exception("{} must be inherited from ColumnFilterListBase");

            return (ColumnFilterListBase)Activator.CreateInstance(filterType);
        }

        private void HiddenField_OnValueChanged(object sender, EventArgs e)
        {
//            if(!String.IsNullOrEmpty(hiddenField.Value))
//                gridView.SelectedIndex = Convert.ToInt32(hiddenField.Value);
//            else
//                gridView.SelectedIndex = -1;

            OnItemChanged(e);
        }

        private void ApplyButton_OnClick(object sender, EventArgs e)
        {
            gridView.PageIndex = 0;
            fillGridViewFlag = true;
        }

        private void GridView_OnPageIndexChanged(object sender, EventArgs e)
        {
            fillGridViewFlag = true;
        }

        private void GridView_OnSorted(object sender, EventArgs e)
        {
            fillGridViewFlag = true;
        }

        private void GridView_OnDataBound(object sender, EventArgs e)
        {
            GridDataInitialized = true;
        }

        #endregion


        #region Events

        public event EventHandler ItemChanged;
        public event EventHandler<ErrorRowCreatedEventArgs> ErrorRowCreated;
        public event EventHandler<ColumnFilterListCreatingEventArgs> ColumnFilterListCreating;

        protected virtual void OnItemChanged(EventArgs e)
        {
            if(ItemChanged != null)
                ItemChanged(this, e);
        }

        #endregion


        #region Properties

        [DefaultValue(false)]
        public bool AllowErrorColumn { get; set; }

        [DefaultValue(false)]
        public bool AllowErrorIconColumn { get; set; }

        public string ErrorHeaderText { get; set; }

        public override string DataSourceID
        {
            get { return base.DataSourceID; }
            set
            {
                base.DataSourceID = value;
                dataSourceID = value;
                if(columnFilterList != null)
                    columnFilterList.DataSourceID = value;
                if(gridView != null)
                    gridView.DataSourceID = value;
            }
        }

        public override object DataSource
        {
            get { return base.DataSource; }
            set
            {
                base.DataSource = value;
                dataSource = value;
                if(columnFilterList != null)
                    columnFilterList.DataSource = value;
                if(gridView != null)
                    gridView.DataSource = value;
            }
        }

        [Browsable(false)]
        public Boolean GridTreeMode
        {
            get { return (Boolean)(ViewState["gridTreeMode"] ?? false); }
            set { ViewState["gridTreeMode"] = value; }
        }

        [DefaultValue(false)]
        [Category("Behavior")]
        public Boolean FillGridViewFirstTime
        {
            get { return (Boolean)(ViewState["FillGridViewFirstTime"] ?? false); }
            set { ViewState["FillGridViewFirstTime"] = value; }
        }

        [Browsable(false)]
        private Boolean GridViewFilled
        {
            get { return (Boolean)(ViewState["gridViewFilled"] ?? false); }
            set { ViewState["gridViewFilled"] = value; }
        }

        [Browsable(false)]
        private Boolean GridDataInitialized
        {
            get { return (Boolean)(ViewState["GridDataInitialized"] ?? false); }
            set { ViewState["GridDataInitialized"] = value; }
        }

        [DefaultValue("")]
        [Category("Data")]
        [TypeConverter(typeof(StringArrayConverter))]
        [Editor("System.Web.UI.Design.WebControls.DataFieldEditor, System.Design, Version=2.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a", typeof(UITypeEditor))]
        public String[] DataKeyNames
        {
            get
            {
                if (gridView == null)
                    return dataKeyNames;
                else
                    return gridView.DataKeyNames;
            }
            set
            {
                dataKeyNames = value;
                if (gridView != null)
                    gridView.DataKeyNames = value;
            }
        }

        [Browsable(false)]
        public DataKey SelectedDataKey
        {
            get { return gridView.SelectedDataKey; }
        }

        [Browsable(false)]
        public DataKey[] SelectedDataKeys
        {
            get { return gridView.SelectedDataKeys; }
        }

        [DefaultValue("")]
        [Category("Behavior")]
        public String FilterControlType
        {
            get { return (String)ViewState["FilterControlType"]; }
            set { ViewState["FilterControlType"] = value; }
        }

        [DefaultValue("")]
        [Category("Data")]
        public String DataDisableRowField
        {
            get
            {
                if(gridViewExtender == null)
                    return dataDisableRowField;
                else
                    return gridViewExtender.DataDisableRowField;
            }
            set
            {
                if(gridViewExtender != null)
                    gridViewExtender.DataDisableRowField = value;
                dataDisableRowField = value;
            }
        }

        [DefaultValue(1)]
        [Category("Data")]
        public Int32 ConditionValue
        {
            get
            {
                if(gridViewExtender == null)
                    return conditionValue;
                else
                    return gridViewExtender.ConditionValue;
            }
            set
            {
                if(gridViewExtender != null)
                    gridViewExtender.ConditionValue = value;
                conditionValue = value;
            }
        }

        [Browsable(false)]
        private String EmptyDataText
        {
            get { return (String)(ViewState["EmptyDataText"] ?? LookupControlsResources.SFilterNotApplied); }
            set { ViewState["EmptyDataText"] = value; }
        }

        [DefaultValue(true)]
        [Category("Behavior")]
        public String MultiSelectColumnCaption
        {
            get
            {
                if (gridView == null)
                    return multiSelectColumnCaption;
                else
                    return gridView.DeleteFieldColumnCaption;
            }
            set
            {
                multiSelectColumnCaption = value;
                if (gridView != null)
                    gridView.DeleteFieldColumnCaption = value;
            }
        }

        [DefaultValue(false)]
        [Category("Behavior")]
        public Boolean ShowFullBriefViewButton
        {
            get
            {
                if (columnFilterList == null)
                    return showFullBriefViewButton;
                else
                    return columnFilterList.ShowFullBriefViewButton;
            }
            set
            {
                showFullBriefViewButton = value;
                if(columnFilterList != null)
                    columnFilterList.ShowFullBriefViewButton = value;
            }
        }

        [DefaultValue(true)]
        [Category("Behavior")]
        public Boolean AllowSorting
        {
            get
            {
                if (gridView == null)
                    return allowSorting;
                else
                    return gridView.AllowSorting;
            }
            set
            {
                allowSorting = value;
                if(gridView != null)
                    gridView.AllowSorting = value;
            }
        }

        [DefaultValue(false)]
        [Category("Behavior")]
        public Boolean AllowMultiSelectColumn
        {
            get
            {
                if (gridView == null)
                    return allowMultiSelectColumn;
                else
                    return gridView.UseDeleteField;
            }
            set
            {
                allowMultiSelectColumn = value;
                if (gridView != null)
                    gridView.UseDeleteField = value;
            }
        }

        [DefaultValue(true)]
        [Category("Behavior")]
        public Boolean PostBackOnTreeExpand
        {
            get
            {
                if (gridView == null)
                    return postBackOnTreeExpand;
                else
                    return gridView.PostBackOnExpnad;
            }
            set
            {
                postBackOnTreeExpand = value;
                if (gridView != null)
                    gridView.PostBackOnExpnad = value;
            }
        }

        [Category("Behavior")]
        public Unit GridHeight
        {
            get
            {
                if (gridPanel == null)
                    return gridHeight;
                else
                    return gridPanel.Height;
            }
            set
            {
                gridHeight = value;
                if (gridPanel != null)
                    gridPanel.Height = value;
            }
        }

        [DefaultValue(false)]
        public bool AllowTreeAndPaging
        {
            get { return (Boolean)(ViewState["AllowTreeAndPaging"] ?? false); }
            set { ViewState["AllowTreeAndPaging"] = value; }
        }

        #endregion


        #region IScriptControl Members

        public IEnumerable<ScriptDescriptor> GetScriptDescriptors()
        {
            if(Page != null && Visible)
            {
                ScriptControlDescriptor desc = new ScriptControlDescriptor("Nat.Web.Controls.LookupTable", ClientID);

                desc.AddProperty("collapseImageID", collapseImage.ClientID);
                desc.AddProperty("applyButtonID", applyButton.ClientID);
                desc.AddProperty("hiddenFieldID", hiddenField.ClientID);
                desc.AddProperty("gridViewBehaviorID", gridViewExtender.ClientID);

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