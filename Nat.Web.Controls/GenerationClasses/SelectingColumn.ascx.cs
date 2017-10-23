using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Script.Serialization;
using System.Web.UI;
using Nat.Web.Controls.GenerationClasses.BaseJournal;
using Nat.Web.Controls.GenerationClasses.HierarchyFields;
using Nat.Web.Tools;
using AjaxControlToolkit;
using Nat.Web.Controls.Properties;
using System.Web.UI.WebControls;

namespace Nat.Web.Controls.GenerationClasses
{
    using System.Web.Configuration;

    public partial class SelectingColumn : UserControl
    {
        private bool? _fixedHeader;
        private int? _fixedRowsCount;
        private int? _fixedColumnsCount;
        private bool _valuesInited;

        public BaseJournalControl Journal { get; set; }

        private bool? _hasAjax;
        public bool HasAjax
        {
            get
            {
                if (_hasAjax == null)
                {
                    var updatePanel = ControlHelper.FindControl<UpdatePanel>(this);
                    _hasAjax = ScriptManager != null && updatePanel != null;
                }
                return _hasAjax.Value;
            }
        }

        protected string HeaderTableClientID
        {
            get { return ClientID + "_hTable"; }
        }

        protected string ConcatenateColumnDivClientID
        {
            get { return ClientID + "_ConcatenateColumnDiv"; }
        }

        private ScriptManager _scriptManager;
        public ScriptManager ScriptManager
        {
            get
            {
                if (_scriptManager == null)
                    _scriptManager = ScriptManager.GetCurrent(Page);
                return _scriptManager;
            }
        }

        protected override void OnInit(EventArgs e)
        {
            base.OnInit(e);
            
            tColsCount.Text = Journal.ParentUserControl.FixedColumnsCount.ToString();
            tRowsCount.Text = Journal.ParentUserControl.FixedRowsCount.ToString();
            chkHeader.Checked = Journal.ParentUserControl.FixedHeader;
            //hfSort.Value = Journal.DefaultOrder;

            if (IsPostBack)
                PrepareSettings();
            if (!Journal.DetailsRender && Journal.DrawPanelVisible && !ScriptManager.IsInAsyncPostBack)
            {
                var extender = new CollapsiblePanelExtender
                                   {
                                       ID = "cp_" + ClientID,
                                       BehaviorID = "cpb_" + ClientID,
                                       Collapsed = true,
                                       ExpandDirection = CollapsiblePanelExpandDirection.Vertical,
                                       CollapseControlID = legendSC.ClientID,
                                       ExpandControlID = legendSC.ClientID,
                                   };
                Journal.ParentUserControl.ExtenderAjaxControl.AddExtender(extender, contentSC.ClientID);
            }

            ppc.OkControlID = ClientID + "_Ok";
            ppc.OnOkScript = string.Format("ApplyTableSettings('{0}');{1}", HeaderTableClientID, Page.ClientScript.GetPostBackEventReference(Journal, BaseJournalControl.ApplyTableSettings));
            ppc.CancelControlID = ClientID + "_Cancel";
            
            ppcFixedHeader.OkControlID = ClientID + "_FHOk";
            ppcFixedHeader.OnOkScript =
                string.Format(
                    "CreateFixedHeader($get('{0}'), $get('{1}').checked, $get('{2}').value, $get('{3}').value);",
                    Journal.ClientID, chkHeader.ClientID, tRowsCount.ClientID, tColsCount.ClientID);
            //------------------------------
            ccc.OkControlID = ClientID + "_CCCOk";
            ccc.OnOkScript = string.Format("createConcatenateColumns({0}); {1}", ConcatenateColumnDivClientID, Page.ClientScript.GetPostBackEventReference(Journal, BaseJournalControl.CreateConcatenateColumns));
            ccc.CancelControlID = ClientID + "_CCCCancel";

            panelColorSample.ToolTip = Resources.SCurrentColor;
            imgColorPicker.ToolTip = Resources.SColorPalette;
            imgColorPicker.ImageUrl = Themes.IconUrlColorPickerButton;
        }

        private void InitConcatenateColumnLists()
        {
            if (lvLeftColumnList != null)
                foreach (var columnHierarchy in Journal.BaseInnerHeader.ColumnHierarchy.GetAllItems().Where(r => !string.IsNullOrEmpty(r.ColumnName) && string.IsNullOrEmpty(r.CrossColumnKey) && !r.ColumnName.Contains(ConcatenateColumn.ColumnNamePrefix)))
                {
                    if (Journal.BaseInnerHeader.ColumnsDic[columnHierarchy.ColumnName].IsCrossColumn) 
                        continue;
                    var item = new ListItem(columnHierarchy.Header, columnHierarchy.ColumnName);
                    item.Attributes.Add("title", columnHierarchy.Header);
                    lvLeftColumnList.Items.Add(item);
                }
            if (ddlConcatenatedColumns != null)
                foreach (var concColumn in Journal.BaseInnerHeader.ConcatenateColumns)
                    if (!concColumn.MarkAsDeleted)
                    {
                        var item = new ListItem(concColumn.Header, concColumn.ColumnName);
                        item.Attributes.Add("title", concColumn.Header);
                        ddlConcatenatedColumns.Items.Add(item);
                    }
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            ppc.HeaderText = Resources.SViewSettings;
            ppcFixedHeader.HeaderText = Resources.SViewSettingsFixedHeader;
            ccc.HeaderText = Resources.SConcatenateColumns;
            chkHeader.Text = Resources.SSCFixedHeader;
            colorPicker.Text = "FFFFFF";
            SetRowsProperties(Journal.RowsProperties);
            SetCellsProperties(Journal.CellsProperties);
            SetHeaderRowsProperties(Journal.BaseInnerHeader.RowsProperties);
            if (Journal.ConcatenateColumns != null)
                SetConcatenateColumnProperties(Journal.ConcatenateColumns);
        }

        protected override void OnPreRender(EventArgs e)
        {
            base.OnPreRender(e);
            if (!Journal.DetailsRender)
            {
                if (_fixedHeader != null)
                    FixedHeader = _fixedHeader.Value;
                if (_fixedRowsCount != null)
                    FixedRowsCount = _fixedRowsCount.Value;
                if (_fixedColumnsCount != null)
                    FixedColumnsCount = _fixedColumnsCount.Value;

                var startupScript = $"$(function() {{ {GetScriptToInitialFixedHeader()} }});";
                if (HasAjax)
                    ScriptManager.RegisterStartupScript(this, typeof (SelectingColumn),
                                                        "initFixedHeader", startupScript, true);
                else
                    Page.ClientScript.RegisterStartupScript(typeof (SelectingColumn),
                                                            "initFixedHeader", startupScript, true);
            }
            InitializeControls();
        }

        public string GetScriptToInitialFixedHeader()
        {
            return $"CreateFixedHeader($get('{Journal.ClientID}'), {FixedHeader.ToString().ToLower()}, {FixedRowsCount}, {FixedColumnsCount});";
        }

        public void PrepareSettings()
        {
            if (Page != null && IsPostBack && !Journal.ParentUserControl.ValuesLoaded && !_valuesInited)
            {
                _valuesInited = true;
                tColsCount.Text = Page.Request.Params[tColsCount.UniqueID];
                tRowsCount.Text = Page.Request.Params[tRowsCount.UniqueID];
                chkHeader.Checked = Page.Request.Params[chkHeader.UniqueID] != null;
                hfCols.Value = Page.Request.Params[hfCols.UniqueID];
                hfRowsH.Value = Page.Request.Params[hfRowsH.UniqueID];
                hfRows.Value = Page.Request.Params[hfRows.UniqueID];
                hfCells.Value = Page.Request.Params[hfCells.UniqueID];
                hfConcCols.Value = Page.Request.Params[hfConcCols.UniqueID];
                hfConcColsNew.Value = Page.Request.Params[hfConcColsNew.UniqueID];
                hfConcColsRemove.Value = Page.Request.Params[hfConcColsRemove.UniqueID];
                //hfSort.Value = Page.Request.Params[hfSort.UniqueID];
                //Journal.DefaultOrder = hfSort.Value;
            }

            SetRowsProperties(Journal.RowsProperties);
            SetCellsProperties(Journal.CellsProperties);
            SetHeaderRowsProperties(Journal.BaseInnerHeader.RowsProperties);
            if (Journal.ConcatenateColumns != null)
                SetConcatenateColumnProperties(Journal.ConcatenateColumns);

            if (!Journal.ParentUserControl.ValuesLoaded)
            {
                int rowsCount;
                int colsCount;
                if (!int.TryParse(tRowsCount.Text, out rowsCount))
                    rowsCount = 0;
                if (!int.TryParse(tColsCount.Text, out colsCount))
                    colsCount = 0;
                Journal.ParentUserControl.FixedHeader = chkHeader.Checked;
                Journal.ParentUserControl.FixedColumnsCount = colsCount;
                Journal.ParentUserControl.FixedRowsCount = rowsCount;
            }
            //Journal.BaseInnerHeader.Ini
        }

        public List<ColumnHierarchy> GetColumnHierarchySettings()
        {
            if (hfCols == null) return null;
            if (string.IsNullOrEmpty(hfCols.Value)) return null;
            var ser = new JavaScriptSerializer();
            return ser.Deserialize<List<ColumnHierarchy>>(hfCols.Value);
        }

        public List<ConcatenateColumnTransporter> GetConcatenateColumnTransporters()
        {
            if (Journal.ConcatenateColumns != null)
            {
                SetConcatenateColumnProperties(Journal.ConcatenateColumns);
                return Journal.ConcatenateColumns;
            }
            if (hfConcCols == null) return Journal.ConcatenateColumns;
            if (string.IsNullOrEmpty(hfConcCols.Value)) return null;
            var ser = new JavaScriptSerializer();
            return ser.Deserialize<List<ConcatenateColumnTransporter>>(hfConcCols.Value);
        }

        public string GetNewConcatenatedColumns()
        {
            if (hfConcColsNew != null)
                return hfConcColsNew.Value;
            return "";
        }

        public string GetRemoveConcatenatedColumns()
        {
            if (hfConcColsRemove != null)
                return hfConcColsRemove.Value;
            return "";
        }

        public bool FixedHeader
        {
            get { return chkHeader.Checked; }
            set
            {
                if (chkHeader != null)
                    chkHeader.Checked = value;
                _fixedHeader = value;
            }
        }

        public int FixedRowsCount
        {
            get
            {
                int rowsCount;
                if (!int.TryParse(tRowsCount.Text, out rowsCount))
                    rowsCount = 0;
                return rowsCount;
            }
            set
            {
                if (tRowsCount != null)
                    tRowsCount.Text = value.ToString();
                _fixedRowsCount = value;
            }
        }

        public int FixedColumnsCount
        {
            get
            {
                int colsCount;
                if (!int.TryParse(tColsCount.Text, out colsCount))
                    colsCount = 0;
                return colsCount;
            }
            set
            {
                if (tColsCount != null)
                    tColsCount.Text = value.ToString();
                _fixedColumnsCount = value;
            }
        }

        public string GetColHId()
        {
            return hfCols.ClientID;
        }

        public string GetRowsHId()
        {
            return hfRowsH.ClientID;
        }

        public string GetRowsId()
        {
            return hfRows.ClientID;
        }

        public string GetCellsId()
        {
            return hfCells.ClientID;
        }

        public void InitializeControls()
        {
            var columnsDic = Journal.BaseInnerHeader.ColumnsDic;
            foreach (var columnHierarchy in Journal.BaseInnerHeader.ColumnHierarchy)
            {
                columnHierarchy.Init(Journal.ResourceManager, Journal);
                columnHierarchy.EnsureCrossColumnsHierarchy(columnsDic);
            }

            var maxLevelsCount = Journal.BaseInnerHeader.ColumnHierarchy.Max(r => r.GetClientLevelsCount(columnsDic));
            foreach (var columnHierarchy in Journal.BaseInnerHeader.ColumnHierarchy)
            {
                columnHierarchy.DetectClientColSpan(columnsDic);
                columnHierarchy.DetectClientRowSpan(maxLevelsCount, columnsDic);
            }

            if (hfCols != null)
            {
                var maxJsonLength = WebConfigurationManager.AppSettings["CrossJournalColumnHierarchy.MaxJsonLength"];
                var ser = new JavaScriptSerializer { MaxJsonLength = string.IsNullOrEmpty(maxJsonLength) ? 20971520 : Convert.ToInt32(maxJsonLength) };
                var str = Journal.BaseInnerHeader.ColumnHierarchy.Count == 0
                              ? string.Empty
                              : ser.Serialize(Journal.BaseInnerHeader.ColumnHierarchy);
                hfCols.Value = str;
            }

            InitConcatenateColumnLists();
            if (hfConcCols != null)
            {
                var ser = new JavaScriptSerializer();
                var str = Journal.BaseInnerHeader.ConcatenateColumns.Count == 0
                              ? string.Empty
                              : ser.Serialize(ConcatenateColumnMaker.GetConcatenateColumnTransporters(Journal.BaseInnerHeader.ConcatenateColumns));
                hfConcCols.Value = str;
            }

            if (hfConcColsNew != null)
                hfConcColsNew.Value = string.Empty;
            if (hfConcColsRemove != null)
                hfConcColsRemove.Value = string.Empty;
        }

        public string GetOpenViewSettingsScript()
        {
            return string.Format("ShowTableSettings('{0}');", HeaderTableClientID);
        }

        public string GetOpenFixedHeaderScript()
        {
            return string.Format("$find('{0}').show();", ppcFixedHeader.ModalPopupBehaviorID);
        }

        public string GetOpenCreateConcatenateColumnScript()
        {
            return string.Format("$find('{0}').show();", ccc.ModalPopupBehaviorID);
        }

        public List<RowProperties> DesirializeRowsH()
        {
            if (hfRowsH == null || string.IsNullOrEmpty(hfRowsH.Value)) return null;
            var ser = new JavaScriptSerializer();
            return ser.Deserialize<List<RowProperties>>(hfRowsH.Value);
        }

        public List<RowProperties> DesirializeRows()
        {
            if (hfRows == null || string.IsNullOrEmpty(hfRows.Value)) return null;
            var ser = new JavaScriptSerializer();
            return ser.Deserialize<List<RowProperties>>(hfRows.Value).Where(r => r != null).ToList();
        }

        public List<CellProperties> DesirializeCells()
        {
            if (hfCells == null) return null;
            if (string.IsNullOrEmpty(hfCells.Value))
                return null;
            var ser = new JavaScriptSerializer();
            return ser.Deserialize<List<CellProperties>>(hfCells.Value).Where(r => r != null).ToList();
        }

        public void SetConcatenateColumnProperties(List<ConcatenateColumnTransporter> transporters)
        {
            if (hfConcCols == null) return;
            var ser = new JavaScriptSerializer();
            hfConcCols.Value = ser.Serialize(transporters);
        }

        public void SetRowsProperties(List<RowProperties> properties)
        {
            if (hfRows == null) return;
            var ser = new JavaScriptSerializer();
            hfRows.Value = ser.Serialize(properties);
        }

        public void SetCellsProperties(List<CellProperties> properties)
        {
            if (hfCells == null) return;
            var ser = new JavaScriptSerializer();
            hfCells.Value = ser.Serialize(properties);
        }

        public void SetHeaderRowsProperties(List<RowProperties> properties)
        {
            if (hfCells == null) return;
            var ser = new JavaScriptSerializer();
            hfRowsH.Value = ser.Serialize(properties);
        }
        /*
        public void SetOrderColumns(string value)
        {
            if(hfSort == null) return;
            hfSort.Value = value;
        }*/
    }
}