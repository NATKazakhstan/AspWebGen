using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Data.Linq;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using AjaxControlToolkit;
using Nat.Tools.Specific;
using Nat.Web.Controls.GenerationClasses.Data;
using Nat.Web.Controls.GenerationClasses.HierarchyFields;
using System.Diagnostics;
using System.Text;
using Nat.Web.Controls.EnableController;
using Nat.Web.Controls.Properties;
using Nat.Web.Controls.Trace;
using Nat.Web.Tools;
using Nat.Web.Tools.Initialization;
using Nat.Web.Tools.Security;

namespace Nat.Web.Controls.GenerationClasses.BaseJournal
{
    public abstract class BaseJournalRow : Control
    {
        private string _rowKey;
        private List<string> _errorMessages;
        protected LinkButton SaveButton;
        protected LinkButton RefreshButton;
        protected LinkButton CancelButton;
        protected LinkButton EditButton;
        protected Label StillEditEventControl;
        protected CrossJournalStillEditExtender StillEditExtender;
        protected List<LinkButton> Buttons;
        public bool NeedSaveButton;
        public bool NeedEditButton;
        public bool NeedCancelButton;
        public abstract ExtenderAjaxControl ExtenderAjaxControl { get; }
        public bool Selected { get; set; }
        public BaseColumn GroupColumn { get; set; }
        //public BaseColumn TotalColumn { get; set; }
        public object[] TotalValues { get; set; }
        public object[] GroupKeys { get; set; }
        public BaseColumn.GroupKeys InlineGroupDataItemKey { get; set; }
        public string CurrentCustomTotalRow { get; set; }
        protected Dictionary<string, RenderContext> AllRenderContext { get; set; }
        protected Dictionary<string, RenderContext> SplitedRenderContext { get; set; }

        public int SplitedRowIndex { get; set; }
        public int MaxRowsCount { get; protected set; }

        public string RowKey
        {
            get
            {
                if (_rowKey == null)
                {
                    _rowKey = "row_" + GetId();
                    if (GroupColumn != null)
                    {
                        if (GroupKeys != null)
                            _rowKey = _rowKey + "_g_" + GroupColumn.ColumnName + "_" + JoinKeys(GroupKeys);
                        else
                            _rowKey = _rowKey + "_t_" + GroupColumn.ColumnName + "_" + JoinKeys(TotalValues);
                    }

                    if (InlineGroupDataItemKey?.Count > 0)
                        _rowKey += "__" + InlineGroupDataItemKey;

                    _rowKey += SplitedRowIndex > 0 ? "_" + SplitedRowIndex : string.Empty;
                    if (!string.IsNullOrEmpty(CurrentCustomTotalRow))
                        _rowKey += "_ct_" + CurrentCustomTotalRow;
                    _rowKey = _rowKey
                        .Replace(' ', '_')
                        .Replace('(', '_')
                        .Replace(')', '_')
                        .Replace('-', '_')
                        .Replace('.', '_')
                        .Replace(',', '_');
                }

                return _rowKey;
            }
            protected set { _rowKey = value; }
        }

        public RenderContext RenderContext { get; set; }
        public Controller Controller { get; protected set; }
        public abstract string GetId();

        public override bool EnableViewState
        {
            get
            {
                return false;
            }
        }

        internal static string JoinKeys(IEnumerable<object> values)
        {
            if (values == null || values.Count() == 0) return "";
            return string.Join("_", values.Select(r => r == null ? "" : r.ToString()).ToArray());
        }

        public void AddErrorMessage(string errorMessage)
        {
            if (string.IsNullOrEmpty(errorMessage)) return;

            if (_errorMessages == null)
                _errorMessages = new List<string>();
            _errorMessages.Add(errorMessage);
        }

        public void AddErrorMessages(List<string> errorMessages)
        {
            if (errorMessages == null) return;

            if (_errorMessages == null)
                _errorMessages = new List<string>();
            _errorMessages.AddRange(errorMessages);
        }

        protected void RenderErrorMessages(HtmlTextWriter writer)
        {
            if (_errorMessages != null && _errorMessages.Count > 0)
            {
                writer.RenderBeginTag(HtmlTextWriterTag.Br);
                writer.RenderEndTag();
                foreach (var errorMessage in _errorMessages.Distinct())
                {
                    writer.AddAttribute(HtmlTextWriterAttribute.Class, "errorText");
                    writer.AddStyleAttribute(HtmlTextWriterStyle.FontSize, "13pt");
                    writer.RenderBeginTag(HtmlTextWriterTag.Span);
                    writer.Write(errorMessage);
                    writer.RenderEndTag();
                    writer.RenderBeginTag(HtmlTextWriterTag.Br);
                    writer.RenderEndTag();
                }
            }
        }

        public IEnumerable<string> GetErrors()
        {
            return _errorMessages;
        }
        
        protected class CrossJournalEditQueryParam
        {
            public string JorunalName;
            public string SID;
            public string RowID;
            public string CrossRowID;
        }
    }

    public abstract class BaseJournalRow<TRow> : BaseJournalRow, IDataItemContainer
           where TRow : BaseRow
    {
        private readonly int _itemIndex;

        protected BaseJournalRow(int itemIndex, TRow row, BaseJournalControl<TRow> journalControl)
        {
            DataItem = row;
            _itemIndex = itemIndex;
            JournalControl = journalControl;
            Cells = new Dictionary<BaseColumn, Control>();
            AllCells = new List<KeyValuePair<BaseColumn, Control>>();
        }

        public Dictionary<BaseColumn, Control> Cells { get; private set; }
        public List<KeyValuePair<BaseColumn, Control>> AllCells { get; private set; }

        public TRow DataItem { get; private set; }
        public BaseJournalControl<TRow> JournalControl { get; private set; }

        public override ExtenderAjaxControl ExtenderAjaxControl
        {
            get { return JournalControl.ParentUserControl.ExtenderAjaxControl; }
        }

        public int GetMaxRowsCount()
        {
            if (AllCells.Count == 0) return 0;
            Func<BaseColumn, BaseJournalCell, int> getRowsCount =
                (column, cell) => column == null || cell == null ? 1 : column.GetRowsCount(cell.RenderContext);
            return AllCells.Max(cell => getRowsCount(cell.Key, (BaseJournalCell)cell.Value));
        }

        public void AddCell(BaseColumn column, Control cell, string id)
        {
            cell.ID = id;
            if (column != null)
            {
                Cells[column] = cell;
                var baseJournalCell = cell as BaseJournalCell;
                if (baseJournalCell != null)
                    baseJournalCell.RowIndex = column.RowIndex;
            }
            Controls.Add(cell);
            AddValidationControls(column, cell);
            cell.DataBind();
            AllCells.Add(new KeyValuePair<BaseColumn, Control>(column, cell));
        }

        public string GetCellID(BaseColumn column)
        {
            return string.IsNullOrEmpty(column.ColumnName) ? "" : column.GetClientID(RenderContext);
        }

        public string GetEditControlID(BaseColumn column)
        {
            return string.IsNullOrEmpty(column.ColumnName) ? "" : column.GetClientID(RenderContext);
        }

        public void InsertCell(BaseColumn column, Control cell, string id, Control afterCell)
        {
            cell.ID = id;
            if (column != null)
                Cells[column] = cell;
            var index = Controls.IndexOf(afterCell) + 1;
            Controls.AddAt(Controls.IndexOf(afterCell) + 1, cell);
            cell.DataBind();
            AddValidationControls(column, cell);
            AllCells.Insert(index, new KeyValuePair<BaseColumn, Control>(column, cell));
        }

        protected void AddValidationControls(BaseColumn column, Control cell)
        {
            var baseCell = cell as BaseJournalCell;
            if (baseCell == null || baseCell.IsEmpty) return;

            if (RenderContext.AllowEdit && Page != null)
            {
                if (!JournalControl.DetailsRender)
                    JournalControl.NeedSaveButton = true;
                if (column.EditRegexValidation != null && column.EditRegexValidation.Length > 0)
                {
                    foreach (var regexValidation in column.EditRegexValidation)
                    {
                        var regexProps = new RegexValidatorProperties
                            {
                                    RegularExpression = regexValidation.Key,
                                    ErrorMessage = regexValidation.Value,
                                    ErrorMessageInSummary = regexValidation.Value,
                            };
                        baseCell.AddValidator(regexProps);
                    }
                }
            }
        }

        public override string GetId()
        {
            if (TotalValues != null) JoinKeys(TotalValues);
            if (DataItem != null) return DataItem.Value;
            return null;
        }

        protected bool AllowEditRow(RenderContext renderContext)
        {
            if (!JournalControl.DetailsRender) return true;

            var rowInfo = GetAllowEditRowInfo(renderContext);
            if (rowInfo.SID == User.GetSID())
            {
                NeedSaveButton = true;
                NeedCancelButton = true;
                NeedEditButton = false;
                return true;
            }
            if (string.IsNullOrEmpty(rowInfo.SID))
            {
                RenderContext.AllowEdit = false;
                if (!NeedSaveButton)
                    NeedEditButton = true;
            }
            return false;
        }

        private SYS_GetCrossJournalEditsResult GetAllowEditRowInfo(RenderContext renderContext)
        {
            var dic = (Dictionary<string, SYS_GetCrossJournalEditsResult>)HttpContext.Current.Items["AllowEditRowInfo"];
            if (dic == null)
                HttpContext.Current.Items["AllowEditRowInfo"] =
                    dic = new Dictionary<string, SYS_GetCrossJournalEditsResult>();
            var cacheKey = JournalControl.GetType().FullName + "#" + RowKey
                           + "#" + renderContext.Column.GetType().FullName + "#" + renderContext.CrossColumnId;
            if (dic.ContainsKey(cacheKey)) return dic[cacheKey];
            SYS_GetCrossJournalEditsResult info;

            WebInitializer.Initialize();
            using (var db = new CrossJournalDataContext(SpecificInstances.DbFactory.CreateConnection()))
            {
                info = db.SYS_GetCrossJournalEdits(
                    User.GetSID(), JournalControl.GetType().FullName, RowKey,
                    renderContext.Column.GetType().FullName,
                    renderContext.CrossColumnId,
                    RowKey.Equals(JournalControl.EditRowKey) || JournalControl.EditAllRows,
                    JournalControl.EndEditRowKeys.Contains(RowKey) || JournalControl.EndEditAllRows).
                    First();
            }
            dic[cacheKey] = info;
            return info;
        }

        public abstract void SetTotalsValues(BaseColumn column, BaseJournalCell<TRow> cell, string crossColumnId);

        object IDataItemContainer.DataItem
        {
            get
            {
                return DataItem;
            }
        }

        public int DataItemIndex
        {
            get
            {
                return _itemIndex;
            }
        }

        int IDataItemContainer.DisplayIndex
        {
            get
            {
                return _itemIndex;
            }
        }
    }

    public abstract class BaseJournalRow<TKey, TTable, TRow, TDataContext> : BaseJournalRow<TRow>
        where TKey : struct
        where TTable : class
        where TRow : BaseRow
        where TDataContext : DataContext, new()
    {
        internal Dictionary<BaseColumn, KeyValuePair<BaseJournalCell, object>> PreviousCells =
            new Dictionary<BaseColumn, KeyValuePair<BaseJournalCell, object>>();
        internal Dictionary<BaseColumn, KeyValuePair<BaseJournalCell, object>> PreviousLeftGroupCells =
            new Dictionary<BaseColumn, KeyValuePair<BaseJournalCell, object>>();

        protected BaseJournalRow(int itemIndex, TRow row, BaseJournalControl<TRow> journalControl)
            : base(itemIndex, row, journalControl)
        {
        }

        protected virtual BaseJournalHeaderCustomTemplateColumn GetTemplateContainer(TRow row)
        {
            throw new NotSupportedException();
        }

        protected virtual BaseJournalHeaderCustomTemplateColumn GetTemplateContainer(RenderContext renderContext)
        {
            return new BaseJournalHeaderCustomTemplateColumn(renderContext);
        }

        protected virtual BaseJournalHeaderCustomTemplateColumn GetEditTemplateContainer(RenderContext renderContext)
        {
            return new BaseJournalHeaderCustomTemplateColumn(renderContext);
        }

        public BaseJournalCell<TRow> AddCell(BaseColumn column, object crossColumnID, int index, ColumnHierarchy columnHierarchy)
        {            
            if (AllowJoinRows(column)
                && PreviousCells.ContainsKey(column)
                && PreviousCells[column].Value != null
                && PreviousCells[column].Value.Equals(column.GetValue(RenderContext, false)))
            {
                PreviousCells[column].Key.RowSpan++;
                return null;
            }
            if ((column.GroupType & GroupType.Left) == GroupType.Left 
                && PreviousLeftGroupCells.ContainsKey(column)
                && PreviousLeftGroupCells[column].Value != null
                && PreviousLeftGroupCells[column].Value.Equals(column.GetValue(RenderContext, false)))
            {
                PreviousLeftGroupCells[column].Key.RowSpan++;
                return null;
            }
            var cell = new BaseJournalCell<TRow>
                           {
                               Row = this,
                               ID = GetCellID(column),
                               Column = column,
                               RowIndex = column.RowIndex,
                               Item = RenderContext.CrossDataItemRow,
                               IsEmpty = RenderContext.CrossColumnId != null && RenderContext.CrossDataItemRow == null && RenderContext.TotalGroupValues == null,
                               CrossColumnId = RenderContext.CrossColumnId,
                               ColumnHierarchy = columnHierarchy,
                               RowNumber = DataItem == null || TotalValues != null ? null : column.GetRowNumber(RenderContext),
                               RenderContext = RenderContext,
                           };
            Cells[column] = cell;
            Controls.Add(cell);

            RenderContext.EditClientID = column.AllowEditColumnValue ? GetEditControlID(column) : null;
            RenderContext.ClientID = cell.ClientID;
            RenderContext.UniqueID = cell.UniqueID;
            RenderContext.Control = cell;
            RenderContext.AllowEdit = (!cell.IsEmpty || JournalControl.AllowEditEmptyCell) && column.AllowEditValue(RenderContext) && AllowEditRow(RenderContext);
            if (RenderContext.AllowEdit)
            {
                var editControl = column.GetEditControl(RenderContext);
                if (editControl != null)
                {
                    editControl.ID = RenderContext.EditClientID;
                    cell.EditColtrol = editControl;
                    cell.IsEmpty = false;
                }
            }

            AddValidationControls(column, cell);
            AllCells.Add(new KeyValuePair<BaseColumn, Control>(column, cell));
            if (AllowJoinRows(column))
                PreviousCells[column] = new KeyValuePair<BaseJournalCell, object>(cell, column.GetValue(RenderContext, false));
            if ((column.GroupType & GroupType.Left) == GroupType.Left)
                PreviousLeftGroupCells[column] = new KeyValuePair<BaseJournalCell, object>(cell, column.GetValue(RenderContext, false));
            return cell;
        }

        private bool AllowJoinRows(BaseColumn column)
        {
            return ((column.AllowJoinRows 
                     && (TotalValues == null || !column.InlineGrouping))
                    || (TotalValues != null 
                        && !column.InlineGrouping 
                        && column.AllowAggregate 
                        && (column.AggregateType == ColumnAggregateType.GroupText 
                            || column.AggregateType == ColumnAggregateType.GroupTextWithTotalPhrase)));
        }

        protected internal void InitializeControls(BaseJournalRow<TKey, TTable, TRow, TDataContext> previousRow, BaseJournalRow<TKey, TTable, TRow, TDataContext> previousInlineRow)
        {
#if TRACE
            HttpContext.Current.Trace.WriteExt(string.Format(
                "Initialize Row: {0} previouseRow: {1} previousInlineRow: {2}",
                GetId(),
                previousRow == null ? "null" : previousRow.GetId(),
                previousInlineRow == null ? "null" : previousInlineRow.GetId()));
#endif
            if (previousRow != null)
                PreviousLeftGroupCells = previousRow.PreviousLeftGroupCells;
            if (previousInlineRow != null)
            {
                PreviousCells = previousInlineRow.PreviousCells;
                SplitedRenderContext = previousInlineRow.SplitedRenderContext;
            }

            InitializeControls();
        }

        protected internal void InitializeControls(BaseJournalRow<TKey, TTable, TRow, TDataContext> previousRow, BaseJournalRow<TKey, TTable, TRow, TDataContext> previousInlineRow, int notUseJoinGroupsAfterIndex)
        {
#if TRACE
            HttpContext.Current.Trace.WriteExt(string.Format(
                "Initialize Row: {0} previouseRow: {1} previousInlineRow: {2} notUseJoinGroupsAfterIndex: {3}",
                GetId(),
                previousRow == null ? "null" : previousRow.GetId(),
                previousInlineRow == null ? "null" : previousInlineRow.GetId(),
                notUseJoinGroupsAfterIndex));
#endif
            if (previousRow != null)
            {
                PreviousLeftGroupCells = previousRow.PreviousLeftGroupCells;
                for (int i = notUseJoinGroupsAfterIndex + 1; i < JournalControl.GroupColumns.Count; i++)
                {
                    var column = JournalControl.InnerHeader.ColumnsDic[JournalControl.GroupColumns[i]];
                    if (PreviousLeftGroupCells.ContainsKey(column))
                        PreviousLeftGroupCells.Remove(column);
                }
            }

            if (previousInlineRow != null)
            {
                PreviousCells = previousInlineRow.PreviousCells;
                SplitedRenderContext = previousInlineRow.SplitedRenderContext;
            }

            InitializeControls();
        }

        protected internal void InitializeControls()
        {
#if TRACE
            HttpContext.Current.Trace.WriteExt(string.Format("Initialize Row: {0}", GetId()));
#endif

            if (JournalControl.DetailsRender && JournalControl.ParentUserControl.ScriptManager.IsInAsyncPostBack)
            {
                var up = ControlHelper.FindControl<UpdatePanel>(this);
                var target = Page.Request.Params["__EVENTTARGET"];
                if (!up.IsInPartialRendering && (target == null || !target.StartsWith(JournalControl.UniqueID + "$" + RowKey + "$")))
                    return;
            }

            if (SplitedRowIndex == 0)
            {
                SplitedRenderContext = new Dictionary<string, RenderContext>();
                AllRenderContext = new Dictionary<string, RenderContext>();
            }
            else
            {
                AllRenderContext = SplitedRenderContext.ToDictionary(r => r.Key, r => r.Value);
            }

            Func<ColumnHierarchy, BaseColumn, bool> addColumn; //метод добавления ячеек, зависит от того итог это или нет
            var index = 0;
            /*RowKey = GetId();
            if (GroupColumn != null)
                RowKey = RowKey + "_g_" + GroupColumn.ColumnName + "_" + JoinKeys(GroupKeys ?? TotalValues);*/

            //колекция колонок по которым строим данные
            IEnumerable<ColumnHierarchy> columns = JournalControl.InnerHeader.GetAllDataColumns();
            InitializeAllRenderContext(index, columns);

            if (TotalValues != null)
                addColumn = (colH, column) => AddTotalColumnInRow(column, colH.CrossColumnIDObject, index, colH);
            else if (GroupColumn != null) //если группа, то добавляется одна ячейка и возврат
            {
                if (AllRenderContext.ContainsKey(GroupColumn.ColumnName))
                    RenderContext = AllRenderContext[GroupColumn.ColumnName];
                else
                {
                    RenderContext = new RenderContext
                        {
                            DataRow = DataItem,
                            Column = GroupColumn,
                            Journal = JournalControl,
                            JournalRow = this,
                            OtherColumns = AllRenderContext,
                            RowIndex = SplitedRowIndex,
                        };
                    GroupColumn.RowIndex = SplitedRowIndex;
                }
                AddColumnInRow(GroupColumn, null, 0, null);
                return;
            }
            else
            {
                if (SplitedRowIndex == 0)
                {
                    foreach (var crossTable in JournalControl.CrossTables.Values.Where(r => r.LoadDataIndependentForVisibilityOfColumns))
                        crossTable.LoadData(DataItem);
                }

                addColumn = (colH, column) => AddColumnInRow(column, colH.CrossColumnIDObject, index, colH);
            }

            if (SplitedRowIndex == 0)
                PreviousCells.Clear();

            InitializeKeyAndID();

            #region Create All Cells

            var nextItaration = new List<ColumnHierarchy>();

            foreach (var columnHierarchy in columns)
            {
                if (!AllRenderContext.ContainsKey(columnHierarchy.ColumnKey)) continue;
                RenderContext = AllRenderContext[columnHierarchy.ColumnKey];
                if (InlineGroupDataItemKey != null && !SplitedRenderContext.ContainsKey(columnHierarchy.ColumnKey))
                    RenderContext.CrossDataItemKey.AddValues(InlineGroupDataItemKey, RenderContext);
                RenderContext.CrossDataItemRow = columnHierarchy.CrossColumnIDObject == null || DataItem == null
                                                     ? null
                                                     : RenderContext.Column.GetCrossItem(RenderContext);

                if (!columnHierarchy.IsVisibleColumn(JournalControl.InnerHeader.ColumnsDic))
                    continue;

                var rowsCount = JournalControl.InnerHeader.ColumnsDic[columnHierarchy.ColumnName].GetRowsCount(RenderContext);
                if (SplitedRowIndex == 0 && rowsCount <= 1 && (RenderContext.CrossDataItemKey == null || RenderContext.CrossDataItemKey.Count == 0))
                    SplitedRenderContext[columnHierarchy.ColumnKey] = RenderContext;

                if (SplitedRowIndex != 0
                    && ((JournalControl.InnerHeader.ColumnsDic[columnHierarchy.ColumnName].GroupType & GroupType.Left) != GroupType.Left)
                    && SplitedRowIndex >= rowsCount)
                    continue;

                if (columnHierarchy.Childs == null || columnHierarchy.Childs.Count == 0 || columnHierarchy.Childs.All(r => r.BaseColumn == null))
                    nextItaration.Add(columnHierarchy);
            }

            foreach (var columnHierarchy in nextItaration)
            {
                RenderContext = AllRenderContext[columnHierarchy.ColumnKey];
                RenderContext.ComputeAggregates();
                addColumn(columnHierarchy, RenderContext.Column);
            }

            #endregion

            ComputeAggregates();
            var controllers = JournalControl.GetEnableControllers();
            if (controllers != null && controllers.Count > 0)
            {
                Controller = new Controller
                    {
                        Items = controllers,
                        RenderContext = AllCells.Select(r => r.Value).
                            OfType<BaseJournalCell>().Select(r => r.RenderContext).ToList(),
                    };
                Controls.Add(Controller);
            }
            if (JournalControl.DetailsRender)
                AddAjaxControlsInDetailsRender();
        }

        private void InitializeKeyAndID()
        {
            var key = new BaseColumn.GroupKeys();
            for (int i = 0; i < JournalControl.GroupColumns.Count; i++)
            {
                var columnName = JournalControl.GroupColumns[i];
                var column = JournalControl.InnerHeader.ColumnsDic[columnName.ColumnName];
                if (column.InlineGrouping)
                {
                    var value = column.GetValue(AllRenderContext[column.ColumnName], false);
                    key.AddValue(column.ColumnName, value, true);
                    /*if (TotalValues != null && TotalValues.Length > i)
                            TotalValues[i] = value;*/
                }
            }

            InlineGroupDataItemKey = key;
            RowKey = null;
            ID = RowKey;
        }

        private void InitializeAllRenderContext(int index, IEnumerable<ColumnHierarchy> columns)
        {
            foreach (var columnHierarchy in columns)
            {
                if (AllRenderContext.ContainsKey(columnHierarchy.ColumnKey)) continue;

                RenderContext = new RenderContext
                                    {
                                        DataRow = DataItem,
                                        ColumnHierarchy = columnHierarchy,
                                        Column = JournalControl.InnerHeader.ColumnsDic[columnHierarchy.ColumnName],
                                        Journal = JournalControl,
                                        JournalRow = this,
                                        ValidationGroup = JournalControl.DetailsRender
                                                              ? JournalControl.SaveValidationGroup + RowKey
                                                              : JournalControl.SaveValidationGroup,
                                        TotalGroupValues = GroupColumn == null || TotalValues != null ? TotalValues : null,
                                        CrossColumnId = columnHierarchy.CrossColumnID,
                                        OtherColumns = AllRenderContext,
                                        RowIndex = SplitedRowIndex,
                                        CrossDataItemKey = new BaseColumn.GroupKeys(),
                                    };
                RenderContext.Column.RowIndex = SplitedRowIndex;

                RenderContext.CrossColumnIdObject = columnHierarchy.CrossColumnIDObject;
                RenderContext.CrossColumnId = columnHierarchy.CrossColumnID;
                if (columnHierarchy.CrossDataItemKey != null)
                    RenderContext.CrossDataItemKey = (BaseColumn.GroupKeys)columnHierarchy.CrossDataItemKey.Clone();
                RenderContext.ColumnIndex = index;
                RenderContext.GroupValues = GroupColumn == null || TotalValues != null ? GroupKeys : null;
                RenderContext.TotalGroupValues = GroupColumn == null || TotalValues != null ? TotalValues : null;
                RenderContext.CrossColumnHeaderRow = columnHierarchy.CrossColumnHeaderRow;
                RenderContext.ConditionalFormatting = JournalControl.GetConditionalFormatting(columnHierarchy.ColumnKey);
                AllRenderContext.Add(columnHierarchy.ColumnKey, RenderContext);
            }
        }

        protected internal void InitializeRowSpan()
        {
            MaxRowsCount = GetMaxRowsCount();
            if (MaxRowsCount > 1)
            {
                foreach (var cell in AllCells)
                {
                    var baseCell = cell.Value as BaseJournalCell;
                    if (baseCell != null && cell.Key != null && (cell.Key.GroupType & GroupType.Left) != GroupType.Left)
                    {
                        var rowsCount = cell.Key.GetRowsCount(baseCell.RenderContext);
                        if (rowsCount == 0)
                        {
                            baseCell.RowSpan = MaxRowsCount;
                        }
                        else if (SplitedRowIndex + 1 < rowsCount)
                        {
                            baseCell.RowSpan = 1;
                        }
                        else
                        {
                            baseCell.RowSpan = MaxRowsCount - (rowsCount - 1);
                        }
                    }
                }
            }
        }

        private bool AddColumnInRow(BaseColumn column, object crossColumnID, int index, ColumnHierarchy columnHierarchy)
        {
            var cell = AddCell(column, crossColumnID, index, columnHierarchy);
            if (cell != null && columnHierarchy != null)
            {
                if (column.TemplateColumn != null && !cell.IsEmpty && column.ShowAsTemplate(cell.RenderContext))
                {
                    var template = GetTemplateContainer(cell.RenderContext);
                    column.TemplateColumn.InternalTemplate.InstantiateIn(template);
                    cell.ReadColtrol = template;
                }
                if (column.TemplateEditColumn != null && !cell.IsEmpty && column.ShowAsTemplate(cell.RenderContext)
                    && cell.RenderContext.AllowEdit)
                {
                    var template = GetTemplateContainer(cell.RenderContext);
                    column.TemplateEditColumn.InternalTemplate.InstantiateIn(template);
                    cell.EditColtrol = template;
                }
            }
            return true;
        }

        private bool AddTotalColumnInRow(BaseColumn column, object crossColumnID, int index, ColumnHierarchy columnHierarchy)
        {
            /*var crossColumn = column as CrossColumn<TRow>;
            if (crossColumn != null)
                crossColumn.CrossColumnDataSource.RenderRowItems(TotalValues, this);
            else if (column.TemplateColumn != null && column.ShowAsTemplate(TotalValues))
            {
                var template = GetTemplateContainer(TotalValues);
                column.TemplateColumn.InternalTemplate.InstantiateIn(template);
                AddCell(column, template, column.ColumnName + "_" + GetId());
            }
            else*/
            {
                var cell = AddCell(column, crossColumnID, index, columnHierarchy);
                if (cell != null)
                    SetTotalsValues(column, cell, crossColumnID == null ? null : crossColumnID.ToString());
            }
            return true;
        }

        public override void SetTotalsValues(BaseColumn column, BaseJournalCell<TRow> cell, string crossColumnId)
        {
            /*
                    var groupIndex = JournalControl.GroupColumns.IndexOf(column.ColumnName);
                    var countTotalValues = !column.InlineGrouping || groupIndex < 0 || groupIndex >= TotalValues.Length// || GroupColumn == column
                                               ? TotalValues.Length
                                               : groupIndex + 1;
                    cell.TotalGroupValues = new object[countTotalValues];
                    for (int i = 0; i < countTotalValues; i++)
                        cell.TotalGroupValues[i] = TotalValues[i];
                    */
            cell.RenderContext.TotalGroupValues = new object[TotalValues.Length];
            TotalValues.CopyTo(cell.RenderContext.TotalGroupValues, 0);

            if (column.AllowAggregate)
            {
                /*if (column.InlineGrouping)
                        {
                            var values = column.GetTotalRowsValues(JournalControl, cell.TotalGroupValues);
                        }
                        else*/
                {
                    var values = column.GetTotalRowsKeys(RenderContext);
                    if (values != null)
                    {
                        //todo: подумать как переделать сортировку, наверно при получении ключей сортируются на каждом уровне
                        values = values.OrderBy(r => r).ToArray();
                        var count = column.GetRowsCount(cell.RenderContext);
                        var i = 0;
                        for (int index = SplitedRowIndex; index < values.Length; index += count)
                        {
                            var key = values[index];
                            while (JournalControl.IsInlineGroup.Length > i && !JournalControl.IsInlineGroup[i])
                                i++;
                            if (JournalControl.IsInlineGroup.Length <= i
                                || cell.RenderContext.TotalGroupValues.Length <= i)
                                break;
                            cell.RenderContext.TotalGroupValues[i] = key;
                            i++;
                        }
                    }
                }
            }
        }

        protected override void Render(HtmlTextWriter writer)
        {
            if (!Visible) return;
            writer.WriteLine();

            if (JournalControl.DetailsRender)
            {
                RenderButtons(writer);
                if (StillEditEventControl != null)
                {
                    StillEditEventControl.RenderControl(writer);
                    StillEditExtender.RenderControl(writer);
                }
                RenderErrorMessages(writer);
                writer.AddAttribute(HtmlTextWriterAttribute.Class, "ms-vb");
                writer.AddAttribute(HtmlTextWriterAttribute.Cellspacing, "0");
                writer.AddAttribute(HtmlTextWriterAttribute.Rules, "all");
                writer.AddAttribute(HtmlTextWriterAttribute.Border, "1");
                writer.AddStyleAttribute(HtmlTextWriterStyle.BorderCollapse, "collapse");
                writer.AddStyleAttribute(HtmlTextWriterStyle.Width, "100%");
                writer.RenderBeginTag(HtmlTextWriterTag.Table); //Table of row
            }

            AddRowAttributes(writer);
            writer.RenderBeginTag(HtmlTextWriterTag.Tr);

            if (JournalControl.DetailsRender)
                DetailsRender(writer);
            else
                base.Render(writer);

            writer.RenderEndTag();

            if (JournalControl.DetailsRender)
            {
                writer.RenderEndTag(); //Table of row
            }
        }

        protected override void OnInit(EventArgs e)
        {
            base.OnInit(e);
            if (JournalControl.DetailsRender)
            {
                AddButtons();
            }
        }

        protected override void OnPreRender(EventArgs e)
        {
            base.OnPreRender(e);
            if (JournalControl.DetailsRender && NeedSaveButton && SaveButton != null)
            {
                SaveButton.Visible = true;
                StillEditEventControl.Visible = true;
            }
            if (JournalControl.DetailsRender && NeedCancelButton && CancelButton != null)
                CancelButton.Visible = true;
            if (JournalControl.DetailsRender && NeedEditButton && EditButton != null)
                EditButton.Visible = true;
            if (JournalControl.DetailsRender && RefreshButton != null)
                RefreshButton.Visible = true;
        }

        protected virtual void AddRowAttributes(HtmlTextWriter writer)
        {
            writer.AddAttribute(HtmlTextWriterAttribute.Class,
                                Selected ? "ms-selected" : (DataItemIndex%2 == 0 ? "ms-vb" : "ms-alternating"));
            writer.AddAttribute(HtmlTextWriterAttribute.Id, JournalControl.ID + "_row_" + RowKey);
            writer.AddAttribute("rowKey", RowKey);
            var bold = GroupColumn != null || TotalValues != null;
            if (RowKey != null && JournalControl.RowsPropertiesDic.ContainsKey(RowKey))
            {
                var props = JournalControl.RowsPropertiesDic[RowKey];
                if (!string.IsNullOrEmpty(props.BColor))
                    writer.AddStyleAttribute(HtmlTextWriterStyle.BackgroundColor, props.BColor);
                if (!string.IsNullOrEmpty(props.PColor))
                    writer.AddStyleAttribute(HtmlTextWriterStyle.Color, props.PColor);
                if (props.Size != null) //note: -6 т.к. разница в шрифтах на 6
                    writer.AddStyleAttribute(HtmlTextWriterStyle.FontSize, (props.Size - 6).ToString());
                if (props.Bold != null)
                    bold = props.Bold.Value;
            }
            writer.AddStyleAttribute(HtmlTextWriterStyle.FontWeight, bold ? "bold" : "normal");
        }

        protected virtual void AddAjaxControlsInDetailsRender()
        {
            if (JournalControl == null || JournalControl.ParentUserControl == null
                || ExtenderAjaxControl == null
                || !JournalControl.DetailsRender)
                return;

            StillEditEventControl = new Label { Text = "", ID = "crossEditCancel", Visible = false, };
            StillEditExtender = new CrossJournalStillEditExtender
                                    {
                                        TargetControlID = "crossEditCancel",
                                        JournalName = JournalControl.GetType().FullName,
                                        RowID = RowKey,
                                        BehaviorID = "bCrossEditCancel",
                                        ID = "eCrossEditCancel",
                                    };
            Controls.Add(StillEditEventControl);
            Controls.Add(StillEditExtender);
            JournalControl.ParentUserControl.ScriptManager.Services.Add(new ServiceReference("/WebServiceSavedJournalSettings.asmx"));

            if (AllCells.Count == 0) return;

            var columns = JournalControl.InnerHeader.ColumnHierarchy.
                Where(r => r.IsVisibleColumn(JournalControl.InnerHeader.ColumnsDic));
            var maxLevel = columns.Max(r => r.GetLevelsCount(JournalControl.InnerHeader.ColumnsDic));
            var cellIndex = 0;
            AddAjaxControlsInDetailsRender(columns, maxLevel > 5, ref cellIndex);
        }

        protected virtual void AddAjaxControlsInDetailsRender(IEnumerable<ColumnHierarchy> list, bool collapsed, ref int cellIndex)
        {
            foreach (var columnHierarchy in list)
            {
                var getChilds = columnHierarchy.GetChilds().
                    Where(r => r.IsVisibleColumn(JournalControl.InnerHeader.ColumnsDic));

                var levelsCount = columnHierarchy.GetLevelsCount(JournalControl.InnerHeader.ColumnsDic);
                if (levelsCount == 1)
                {
                    cellIndex++;
                    continue;
                }
                if (levelsCount == 2)
                {
                    cellIndex += getChilds.Count();
                    continue;
                }

                if (!columnHierarchy.HideInHeader)
                {
                    var getAllChilds = columnHierarchy.SelectAll().
                        Where(r => r != columnHierarchy && r.Childs.Count == 0
                                   && r.IsVisibleColumn(JournalControl.InnerHeader.ColumnsDic));

                    var allChilds = getAllChilds.ToList();
                    int index = cellIndex;
                    var allEmpty = allChilds.Select((r, ind) => (BaseJournalCell)AllCells[ind + index].Value).All(r => r.IsEmpty);

                    if (allEmpty)
                    {
                        cellIndex += allChilds.Count;
                        continue;
                    }

                    var clientID = GetDetailsGroupClientID(columnHierarchy);
                    var extender = new CollapsiblePanelExtender
                                       {
                                           ID = "cp_" + clientID,
                                           BehaviorID = "cpb_" + clientID,
                                           Collapsed = collapsed,
                                           ExpandDirection = CollapsiblePanelExpandDirection.Vertical,
                                           CollapseControlID = clientID + "_Legend",
                                           ExpandControlID = clientID + "_Legend",
                                       };
                    ExtenderAjaxControl.AddExtender(extender, clientID);
                }
                AddAjaxControlsInDetailsRender(getChilds, collapsed, ref cellIndex);
            }
        }

        protected virtual string GetDetailsGroupClientID(ColumnHierarchy columnHierarchy)
        {
            return ClientID + "_" + RowKey + "_" + columnHierarchy.ColumnKey;
        }

        protected virtual void DetailsRender(HtmlTextWriter writer)
        {
            if (AllCells.Count == 0) return;

            if (GroupColumn != null)
            {
                writer.RenderBeginTag(HtmlTextWriterTag.Td);

                writer.Write(GroupColumn.Header);
                writer.Write(": ");

                writer.AddAttribute(HtmlTextWriterAttribute.Border, "0");
                writer.RenderBeginTag(HtmlTextWriterTag.Table);
                AddRowAttributes(writer);
                writer.RenderBeginTag(HtmlTextWriterTag.Tr);

                base.Render(writer);

                writer.RenderEndTag(); //tr
                writer.RenderEndTag(); //table

                writer.RenderBeginTag(HtmlTextWriterTag.Hr);
                writer.RenderEndTag(); //Hr

                writer.RenderEndTag(); //td
            }
            else if (TotalValues != null)
            {
                
            }
            else
            {
                var index = 0;
                var columns = JournalControl.InnerHeader.ColumnHierarchy.
                    Where(r => r.IsVisibleColumn(JournalControl.InnerHeader.ColumnsDic));
                DetailsRender(writer, columns, true, 0, ref index);
            }
        }

        protected virtual void DetailsRender(HtmlTextWriter writer, IEnumerable<ColumnHierarchy> list, bool isTop, int groupLevel, ref int cellIndex)
        {
            var renderHeaders = new List<string>();

            foreach (var columnHierarchy in list)
            {
                var getChilds = columnHierarchy.GetChilds().
                    Where(r => r.IsVisibleColumn(JournalControl.InnerHeader.ColumnsDic));
                var getAllChilds = columnHierarchy.SelectAll().
                    Where(r => r != columnHierarchy && r.Childs.Count == 0 && r.IsVisibleColumn(JournalControl.InnerHeader.ColumnsDic));

                var levelsCount = columnHierarchy.GetLevelsCount(JournalControl.InnerHeader.ColumnsDic);
                if (levelsCount == 2)
                {
                    var childs = getChilds.ToList();
                    int index = cellIndex;
                    var allEmpty = childs.Select((r, ind) => (BaseJournalCell)AllCells[ind + index].Value).All(r => r.IsEmpty);

                    if (allEmpty)
                    {
                        cellIndex += childs.Count;
                        continue;
                    }

                    if (renderHeaders.Count == 0)
                        BeginTableWithHeader(writer, childs, renderHeaders);
                    else if (renderHeaders.Intersect(childs.Select(r => r.Header)).Count() != childs.Count)//правильно ли?
                    {
                        EndTableWithHeader(writer, renderHeaders, groupLevel);
                        BeginTableWithHeader(writer, childs, renderHeaders);                        
                    }
                    writer.RenderBeginTag(HtmlTextWriterTag.Tr);

                    writer.RenderBeginTag(HtmlTextWriterTag.Td);
                    writer.Write(columnHierarchy.Header);
                    writer.RenderEndTag(); //Td

                    DetailsRender(writer, childs, false, groupLevel, ref cellIndex);

                    writer.RenderEndTag(); //tr
                    continue;
                }
                if (renderHeaders.Count > 0)
                    EndTableWithHeader(writer, renderHeaders, groupLevel);

                if (levelsCount == 1)
                {
                    if (((BaseJournalCell)AllCells[cellIndex].Value).IsEmpty)
                    {
                        cellIndex++;
                        continue;
                    }
                    if (isTop)
                    {
                        writer.AddStyleAttribute(HtmlTextWriterStyle.Width, "25%");
                        writer.RenderBeginTag(HtmlTextWriterTag.Td);

                        writer.Write(columnHierarchy.Header);
                        writer.Write(": ");
                        writer.RenderEndTag(); //td
                        AllCells[cellIndex++].Value.RenderControl(writer);
                        writer.RenderEndTag(); //tr
                        if (groupLevel == 0)
                            AddRowAttributes(writer);
                        writer.RenderBeginTag(HtmlTextWriterTag.Tr);
                    }
                    else
                        AllCells[cellIndex++].Value.RenderControl(writer);
                }
                else if (columnHierarchy.HideInHeader)
                    DetailsRender(writer, getChilds, true, groupLevel + 1, ref cellIndex);
                else
                {
                    int index = cellIndex;
                    var allChilds = getAllChilds.ToList();
                    var allEmpty = allChilds.Select((r, ind) => (BaseJournalCell)AllCells[ind + index].Value).All(r => r.IsEmpty);

                    if (allEmpty)
                    {
                        cellIndex += allChilds.Count;
                        continue;
                    }

                    var clientID = GetDetailsGroupClientID(columnHierarchy);
                    writer.AddAttribute(HtmlTextWriterAttribute.Colspan, "2");
                    writer.RenderBeginTag(HtmlTextWriterTag.Td);

                    var css = DataItemIndex%2 == 0
                                  ? JournalControl.DetailsGroupsCSS[groupLevel%JournalControl.DetailsGroupsCSS.Count]
                                  : JournalControl.DetailsGroupsAlternateCSS[groupLevel%JournalControl.DetailsGroupsAlternateCSS.Count];
                    writer.AddAttribute(HtmlTextWriterAttribute.Class, css);
                    writer.AddAttribute(HtmlTextWriterAttribute.Title, columnHierarchy.Header);
                    writer.RenderBeginTag(HtmlTextWriterTag.Div); //outer DIV
                    writer.RenderBeginTag(HtmlTextWriterTag.Fieldset); //Fieldset

                    #region Legend
                    
                    writer.RenderBeginTag(HtmlTextWriterTag.Legend);

                    writer.AddStyleAttribute(HtmlTextWriterStyle.Cursor, "pointer");
                    writer.AddAttribute(HtmlTextWriterAttribute.Id, clientID + "_Legend");
                    writer.AddAttribute(HtmlTextWriterAttribute.Title, Resources.PressToOpenCloseGroup);
                    writer.AddAttribute("ondblclick", "ClickInnerGroup(this);");
                    writer.AddAttribute("clickOnDblClickInParent", "on");
                    writer.RenderBeginTag(HtmlTextWriterTag.Span);
                    writer.Write(columnHierarchy.Header);
                    writer.RenderEndTag(); //Span

                    writer.RenderEndTag(); //Legend

                    #endregion

                    writer.AddAttribute(HtmlTextWriterAttribute.Id, clientID);
                    writer.RenderBeginTag(HtmlTextWriterTag.Div); //inner DIV

                    #region Table
                    writer.AddAttribute(HtmlTextWriterAttribute.Border, "0");
                    writer.AddStyleAttribute(HtmlTextWriterStyle.Width, "100%");
                    writer.RenderBeginTag(HtmlTextWriterTag.Table);
                    writer.RenderBeginTag(HtmlTextWriterTag.Tr);

                    DetailsRender(writer, getChilds, true, groupLevel + 1, ref cellIndex);

                    writer.RenderEndTag(); //tr
                    writer.RenderEndTag(); //table
                    #endregion

                    writer.RenderEndTag(); //inner DIV

                    writer.RenderEndTag(); //Fieldset
                    writer.RenderEndTag(); //outer DIV

                    writer.RenderEndTag(); //Td

                    writer.RenderEndTag(); //tr
                    if (groupLevel == 0)
                        AddRowAttributes(writer);
                    writer.RenderBeginTag(HtmlTextWriterTag.Tr);
                }
            }
            if (renderHeaders.Count > 0)
                EndTableWithHeader(writer, renderHeaders, groupLevel);
        }

        protected virtual void RenderButtons(HtmlTextWriter writer)
        {
            foreach (var button in Buttons)
            {
                if (button.Visible)
                    button.RenderControl(writer);
                else
                    Page.ClientScript.RegisterForEventValidation(button.UniqueID);
            }
        }

        private void EndTableWithHeader(HtmlTextWriter writer, List<string> renderHeaders, int groupLevel)
        {
            renderHeaders.Clear();
            writer.RenderEndTag(); //Table 1
            writer.RenderEndTag(); //Td 1

            writer.RenderEndTag(); //Tr
            if (groupLevel == 0)
                AddRowAttributes(writer);
            writer.RenderBeginTag(HtmlTextWriterTag.Tr);
        }

        private void BeginTableWithHeader(HtmlTextWriter writer, IEnumerable<ColumnHierarchy> childs, List<string> renderHeaders)
        {
            writer.AddAttribute(HtmlTextWriterAttribute.Colspan, "2");
            writer.RenderBeginTag(HtmlTextWriterTag.Td); //Td 1

            writer.AddAttribute(HtmlTextWriterAttribute.Border, "1");
            //writer.AddStyleAttribute(HtmlTextWriterStyle.Width, "100%");
            writer.RenderBeginTag(HtmlTextWriterTag.Table); //Table 1

            #region Header

            writer.RenderBeginTag(HtmlTextWriterTag.Tr);
            writer.RenderBeginTag(HtmlTextWriterTag.Th);
            writer.RenderEndTag(); //th

            foreach (var child in childs)
            {
                writer.RenderBeginTag(HtmlTextWriterTag.Th);
                writer.Write(child.Header);
                renderHeaders.Add(child.Header);
                writer.RenderEndTag(); //th                        
            }
            writer.RenderEndTag(); //tr

            #endregion
        }

        public void ComputeAggregates()
        {
            /*foreach (var pair in AllCells)
            {
                var cell = (BaseJournalCell) pair.Value;
                cell.Column.ComputeAggregates(cell.RenderContext, cell.RenderContext.ColumnHierarchy.AggregateType);
            }*/
        }

        internal protected bool ProcessPostData(NameValueCollection postData)
        {
            var isValid = true;
            foreach (var pair in AllCells)
            {
                var cell = (BaseJournalCell) pair.Value;
                var control = cell.EditColtrol;
                var postBackDataHandler = control as IPostBackDataHandler;
                try
                {
                    if (control != null && postBackDataHandler != null)
                        postBackDataHandler.LoadPostData(control.UniqueID, postData);
                    else
                    {
                        var editComponent = control as IRenderComponent;
                        if (editComponent != null)
                            editComponent.Value = postData[editComponent.UniqueID];
                    }
                }
                catch (Exception e)
                {
                    AddErrorMessage(e.Message + " (" + cell.Column.Header + ")");
                    isValid = false;
                }
            }
            return isValid;
        }

        protected virtual void AddButtons()
        {
            Buttons = new List<LinkButton>();

            #region RefreshButton

            RefreshButton = new LinkButton
            {
                ToolTip = Resources.SRefresh,
                ID = "refresh",
                CausesValidation = false,
                EnableViewState = false,
                Visible = false,
            };
            RefreshButton.Controls.Add(
                new Literal
                {
                    Text = string.Format("<img src='{0}' style='border:0px;padding:2px' alt='{1}'/>",
                                         Themes.IconUrlRefresh, Resources.SRefresh),
                });
            RefreshButton.Click += Button_Click;

            #endregion

            #region SaveButton

            SaveButton = new LinkButton
                             {
                                 ToolTip = Resources.SSave,
                                 ID = "saveButton",
                                 ValidationGroup = JournalControl.SaveValidationGroup + RowKey,
                                 EnableViewState = false,
                                 Visible = false,
                             };
            SaveButton.Controls.Add(
                new Literal
                    {
                        Text = string.Format("<img src='{0}' style='border:0px;padding:5px' alt='{1}'/>",
                                             Themes.IconUrlSave, Resources.SSave),
                    });
            SaveButton.Click += Button_Click;

            #endregion

            #region CancelButton

            CancelButton = new LinkButton
            {
                ToolTip = Resources.SCancelText,
                ID = "cancel",
                CausesValidation = false,
                EnableViewState = false,
                Visible = false,
            };
            CancelButton.Controls.Add(
                new Literal
                {
                    Text = string.Format("<img src='{0}' style='border:0px;padding:5px' alt='{1}'/>",
                                         Themes.IconUrlCancel, Resources.SCancelText),
                });
            CancelButton.Click += Button_Click;

            #endregion

            #region EditButton

            EditButton = new LinkButton
            {
                ToolTip = Resources.SEditText,
                ID = "edit",
                CausesValidation = false,
                EnableViewState = false,
                Visible = false,
            };
            EditButton.Controls.Add(
                new Literal
                {
                    Text = string.Format("<img src='{0}' style='border:0px;padding:5px' alt='{1}'/>",
                                         Themes.IconUrlEdit, Resources.SEditText),
                }); 
            EditButton.Click += Button_Click;

            #endregion

            Buttons.Add(SaveButton);
            Buttons.Add(CancelButton);
            Buttons.Add(EditButton);
            Buttons.Add(RefreshButton);

            foreach (var button in Buttons)
                Controls.Add(button);
        }

        private void Button_Click(object sender, EventArgs e)
        {
        }

        public virtual ILogMessageEntry GetLogUpdateMessageEntry()
        {
            var message = string.Format("{0}, {1}: {2}",
                                        JournalControl.ParentUserControl.HeaderRu,
                                        DataItem.Value, DataItem.nameRu);
            return new LogMessageEntry(User.GetSID(), JournalControl.ParentUserControl.UpdateMessageLog ?? 0, message);
        }
    }
}
