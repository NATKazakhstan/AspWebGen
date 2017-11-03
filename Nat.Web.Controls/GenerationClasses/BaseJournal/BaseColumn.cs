using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.UI;
using System.Linq.Expressions;
using Nat.Web.Controls.GenerationClasses.HierarchyFields;
using Nat.Web.Controls.Properties;
using Nat.Web.Tools;
using System.Data.Linq;

namespace Nat.Web.Controls.GenerationClasses.BaseJournal
{
    using System.ServiceModel.Configuration;
    using System.Text;

    using Nat.Web.Tools.Export.Computing;

    public class BaseColumn
    {
        #region fields

        private bool _allowAggregate;
        private int _rowIndex;
        internal AggregateData _aggregateData;
        private decimal _customCrossRowNumber;
        private readonly Dictionary<int, bool> _customCrossRowNumberAdded = new Dictionary<int, bool>();
        private bool finishComputeExecuted;
        private bool? isVertical;

        private Dictionary<string, object> extendedObjects;

        private static readonly GroupType[] DefaultSupportGroupTypes =
            {
                GroupType.None, GroupType.Top, GroupType.Left,
            };

        private static readonly GroupType[] DefaultSupportGroupTypesWithTotals =
            {
                GroupType.None, GroupType.Top,
                GroupType.TopTotal, GroupType.Total,
                GroupType.Left, GroupType.LeftTotal,
            };

        #endregion

        #region Events

        public event EventHandler DetectedCrossColumn;

        #endregion

        protected internal BaseColumn()
        {
            Visible = true;
            AddRefTable = true;
            AllowRemoveGrouping = true;
            CheckCanEditRow = true;
        }

        #region properties
        public BaseJournalTypeCell TypeCell { get; set; }
        public string ColumnName { get; set; }
        public string NullText { get; set; }
        public string Format { get; set; }
        public string Header { get; set; }
        public string TableNameForLog { get; set; }
        public string HeaderRu { get; set; }
        public string HeaderKz { get; set; }
        public string ParentTable { get; set; }
        public string HideIfValueEquals { get; set; }
        public bool LookJournalInHyperLink { get; set; }
        public bool DisableHtmlEncode { get; set; }
        public BaseJournalCustomColumn TemplateColumn { get; set; }
        public BaseJournalCustomColumn TemplateEditColumn { get; set; }
        public bool AddRefTable { get; set; }
        public virtual bool Visible { get; set; }
        public virtual bool VisibleInGroup { get; set; } = true;
        public virtual bool AllowGrouping { get; set; }
        public virtual bool UsingInGroup { get; set; }
        public GroupType GroupType { get; set; }
        public ColumnType ColumnType { get; set; } 
        public string AggregateFormat { get; set; }
        public virtual bool InlineGrouping { get { return false; } }
        public virtual bool VisibleColumn
        {
            get
            {
                if (UsingInGroup && !InlineGrouping
                    && ((GroupType & GroupType.Top) == GroupType.Top || GroupType == GroupType.Total))
                    return false;
                return Visible;
            }
        }
        public virtual bool AllowJoinRows { get; set; }
        public bool AllowEditColumnValue { get; set; }
        public bool AllowEditOnlyInDetails { get; set; }
        public int Tab { get; set; }
        public KeyValuePair<string, string>[] EditRegexValidation { get; set; }
        public bool EditMandatory { get; set; }
        public Type ValueType { get; set; }
        public bool IsRadioButtonInRow { get; set; }
        public bool IsRadioButtonForRows { get; set; }
        public virtual bool IsCrossColumn { get; set; }
        public virtual CrossColumnDataSource BaseCrossColumnDataSource { get; set; }
        public virtual BaseJournalCrossTable BaseJournalCrossTable { get; set; }
        public bool HideInHeader { get; set; }
        
        /// <summary>
        /// Вертикально выводтить заголовок колонки.
        /// </summary>
        public bool IsVerticalHeader { get; set; }
        public bool CheckCanEditRow { get; set; }

        public Expression NameRuExpression { get; set; }
        public Expression NameKzExpression { get; set; }
        public Expression NameExpression { get; set; }
        public Expression ValueExpression { get; set; }
        public string OrderColumnName { get; set; }
        public string OrderColumnNameRu { get; set; }
        public string OrderColumnNameKz { get; set; }
        public bool AllowRemoveGrouping { get; set; }

        public ColumnAggregateType AggregateType { get; set; }
        public ColumnAggregateType UserAggregateType { get; set; }
        public bool ComputeCountIfNullValue { get; set; }
        public bool AggregateGroupedByRowIndex { get; set; }
        public string[] GroupByColumns { get; set; }
        public IEnumerable<GroupType> SupportGroupTypes { get; set; }

        public Func<object, object> GetValueForGroup { get; set; }

        public Func<RenderContext, ColumnAggregateType, bool> ComputeAggregatesHandler { get; set; }

        public Dictionary<string, object> ExtendedObjects
        {
            get { return extendedObjects ?? (extendedObjects = new Dictionary<string, object>()); }
        }

        public bool AllowAggregate
        {
            get
            {
                return _allowAggregate
                    || InlineGrouping
                    || AggregateType == ColumnAggregateType.GroupText
                    || AggregateType == ColumnAggregateType.GroupTextWithTotalPhrase
                    || UserAggregateType == ColumnAggregateType.GroupText
                    || UserAggregateType == ColumnAggregateType.GroupTextWithTotalPhrase;
            }
            set { _allowAggregate = value; }
        }

        public bool AllowAggregateSeted
        {
            get { return _allowAggregate; }
        }

        protected virtual internal int RowIndex
        {
            get { return _rowIndex; }
            set
            {
                if (_rowIndex != value)
                {
                    if (IsCrossColumn)
                        BaseCrossColumnDataSource.RowIndex = value;
                    _rowIndex = value;
                }
            }
        }

        public int CrossRowNumber
        {
            get { return RowIndex + 1; }
        }

        public bool RowNumberRight { get; set; }

        public virtual IEnumerable<GroupType> GetSupportGroupTypes(BaseJournalControl journal)
        {
            return SupportGroupTypes ?? (journal.ShowTotals ? DefaultSupportGroupTypesWithTotals : DefaultSupportGroupTypes);
        }

        public virtual string GetGroupTypeImage(GroupType groupType, bool selected)
        {
            if (selected)
            {
                switch (groupType)
                {
                    case GroupType.Top:
                        return Themes.IconUrlCrossJournalGroup1S;
                    case GroupType.Total:
                        return Themes.IconUrlCrossJournalGroup3S;
                    case GroupType.TopTotal:
                        return Themes.IconUrlCrossJournalGroup2S;
                    case GroupType.Left:
                        return Themes.IconUrlCrossJournalGroup4S;
                    case GroupType.LeftTotal:
                        return Themes.IconUrlCrossJournalGroup5S;
                    case GroupType.None:
                        return Themes.IconUrlCrossJournalNoGroupS;
                    default:
                        throw new ArgumentOutOfRangeException("groupType");
                }
            }

            switch (groupType)
            {
                case GroupType.Top:
                    return Themes.IconUrlCrossJournalGroup1;
                case GroupType.Total:
                    return Themes.IconUrlCrossJournalGroup3;
                case GroupType.TopTotal:
                    return Themes.IconUrlCrossJournalGroup2;
                case GroupType.Left:
                    return Themes.IconUrlCrossJournalGroup4;
                case GroupType.LeftTotal:
                    return Themes.IconUrlCrossJournalGroup5;
                case GroupType.None:
                    return Themes.IconUrlCrossJournalNoGroup;
                default:
                    throw new ArgumentOutOfRangeException("groupType");
            }
        }

        public virtual string GetGroupTypeTitle(GroupType groupType, bool selected)
        {
            if (selected)
            {
                switch (groupType)
                {
                    case GroupType.Top:
                        return Resources.SAddedGroupColumnTop;
                    case GroupType.Total:
                        return Resources.SAddedGroupColumnTotal;
                    case GroupType.TopTotal:
                        return Resources.SAddedGroupColumnTopTotal;
                    case GroupType.Left:
                        return Resources.SAddedGroupColumnLeft;
                    case GroupType.LeftTotal:
                        return Resources.SAddedGroupColumnLeftTotal;
                    case GroupType.InHeader:
                        return Resources.SAddedGroupColumnInHeader;
                    case GroupType.None:
                        return Resources.SGroupColumnNotSet;
                    default:
                        throw new ArgumentOutOfRangeException("groupType");
                }
            }

            switch (groupType)
            {
                case GroupType.Top:
                    return Resources.SAddGroupColumnTop;
                case GroupType.Total:
                    return Resources.SAddGroupColumnTotal;
                case GroupType.TopTotal:
                    return Resources.SAddGroupColumnTopTotal;
                case GroupType.Left:
                    return Resources.SAddGroupColumnLeft;
                case GroupType.LeftTotal:
                    return Resources.SAddGroupColumnLeftTotal;
                case GroupType.InHeader:
                    return Resources.SAddGroupColumnInHeader;
                case GroupType.None:
                    return Resources.SRemoveGroupColumn;
                default:
                    throw new ArgumentOutOfRangeException("groupType");
            }
        }

        #endregion

        #region properties of handlers

        public GetResultBool AllowEditValueHandler { get; set; }
        public EventHandler<BaseJournalValidateValueEventArgs> ValidateValueHandler { get; set; }
        public EventHandler<BaseJournalUpdateValueEventArgs> UpdateValueHandler { get; set; }
        public GetResult<IRenderComponent> GetEditComponentHandler { get; set; }
        public GetResult<Control> GetEditControlHandler { get; set; }
        public GetResultBool ShowAsTemplateHandler { get; set; }

        public GetResult<object> GetValueHandler { get; set; }
        public GetResult<Formula> GetFormulaHandler { get; set; }
        public GetResult<object> GetTotalValueHandler { get; set; }
        public GetResult<string> GetRowNumberHandler { get; set; }
        public GetResult<string> GetNameHandler { get; set; }
        public GetResult<string> GetEmptyCellHandler { get; set; }
        public GetResult<string> GetTotalNameHandler { get; set; }
        public GetResult<string> GetHyperLinkHandler { get; set; }
        public GetResultStruct<int> GetRowsCountHandler { get; set; }
        public GetResultBool GetVisibleHandler { get; set; }
        public GetResultStruct<decimal> GetCustomCrossRowNumberHandler { get; set; }
        public GetResult<string> GetTotalPhraseHandler { get; set; }
        public GetResult1<string, string> GetTotalPhraseForHandler { get; set; }

        public string Target { get; set; }

        public Action<ColumnHierarchy, HtmlTextWriter> CustomRenderInHeader { get; set; }
        public Action<BaseJournalCell, HtmlTextWriter> CustomRenderInCellBeforeText { get; set; }
        public Action<BaseJournalCell, HtmlTextWriter> CustomRenderInCellAfterText { get; set; }

        /// <summary>
        /// Вертикально выводтить данные ячейки.
        /// </summary>
        public bool IsVerticalCell
        {
            get { return isVertical ?? (GroupType & GroupType.Left) == GroupType.Left; }
            set { isVertical = value; }
        }

        #endregion

        //public virtual bool IsGroupColumn { get { return false; } }
        //public virtual List<BaseColumn> Columns { get; set; }

        #region virtual NotSupportedException

        public virtual object GetValue(object row, object item, string crossColumnId, object[] groupValues)
        {
            throw new NotSupportedException();
        }
        public virtual string GetName(object row, object item, string crossColumnId, object[] groupValues)
        {
            throw new NotSupportedException();
            
        }
        public virtual int GetRowsCount(object row, object item, object journalRow, BaseJournalControl journal, string crossColumnId, object[] groupValues)
        {
            throw new NotSupportedException();
        }
        public virtual void GetContent(object row, object item, HtmlTextWriter writer, string crossColumnId, object[] groupValues)
        {
            throw new NotSupportedException();            
        }
        public virtual string GetCustomHyperLinkParameters(object row, object item, string crossColumnId, object[] groupValues)
        {
            throw new NotSupportedException();            
        }

        #endregion

        #region Methods

        public decimal GetCustomCrossRowNumber(RenderContext context)
        {
            if (GetCustomCrossRowNumberHandler == null)
                throw new NotSupportedException("Required initialized handler GetCustomCrossRowNumberHandler");
            if (RowIndex == 0)
            {
                _customCrossRowNumber = GetCustomCrossRowNumberHandler(context);
                _customCrossRowNumberAdded.Clear();
            }
            else if (!_customCrossRowNumberAdded.ContainsKey(RowIndex))
            {
                _customCrossRowNumber += GetCustomCrossRowNumberHandler(context);
                _customCrossRowNumberAdded[RowIndex] = true;
            }
            return _customCrossRowNumber;
        }

        public virtual void DetectIsCrossColumn(BaseJournalControl control, string crossColumnName)
        {
            if (GroupByColumns == null || GroupType == GroupType.InHeader) return;
            for (int i = 0; i < control.GroupColumns.Count; i++)
            {
                if (control.GroupColumns[i].GroupType != GroupType.InHeader
                    || control.GroupColumns[i].Equals(ColumnName)
                    || !GroupByColumns.Contains(control.GroupColumns[i].ColumnName)) continue;
                IsCrossColumn = true;
                BaseCrossColumnDataSource = new BaseColumnDS
                                                {
                                                    BaseColumn = this,
                                                    BaseColumnName = ColumnName + "_" + crossColumnName,
                                                    HeaderControl = control.BaseInnerHeader,
                                                    Filter = control.Filter,
                                                };
                DetectedCrossColumn?.Invoke(this, EventArgs.Empty);
            }
        }

        private bool _executeGetValue;

        public virtual object GetValueByContext(RenderContext context)
        {
            return GetValue(context.GetRenderContextFor(this));
        }

        public virtual object GetValue(RenderContext context)
        {
            if (context.GroupValues != null)
                return GetGroupValue(context);

            context.LoadData(BaseJournalCrossTable);

            if (GetValueHandler != null && !_executeGetValue)
            {
                _executeGetValue = true;
                try
                {
                    return GetValueHandler(context);
                }
                finally
                {
                    _executeGetValue = false;
                }
            }

            return GetValue(context.DataRow, context.CrossDataItemRow, context.CrossColumnId, context.GroupValues);
        }

        public virtual bool GetVisible(RenderContext context)
        {
            if (GetVisibleHandler != null) return GetVisibleHandler(context);
            return true;
        }

        public virtual object GetValue(RenderContext context, bool allowGetGroupValue)
        {
            if (allowGetGroupValue && context.GroupValues != null) return GetGroupValue(context);
            if (GetValueHandler != null) return GetValueHandler(context);
            return GetValue(context.DataRow, context.CrossDataItemRow, context.CrossColumnId, allowGetGroupValue ? context.GroupValues : null);
        }

        private bool _executeGetHyperLink;
        public virtual string GetHyperLink(RenderContext context)
        {
            if (GetHyperLinkHandler != null && !_executeGetHyperLink)
            {
                _executeGetHyperLink = true;
                try
                {
                    return GetHyperLinkHandler(context);
                }
                finally
                {
                    _executeGetHyperLink = false;
                }
            }
            var refValue = HttpUtility.UrlEncode(Convert.ToString(GetValue(context)));
            var customParameters = GetCustomHyperLinkParameters(context.DataRow, context.CrossDataItemRow,
                                                                context.CrossColumnId, context.TotalGroupValues);
            var typePage = LookJournalInHyperLink ? "Journal" : "Edit/read";
            var refParentTable = AddRefTable
                                     ? string.Format("ref{0}={1}", HttpUtility.UrlEncode(ParentTable), refValue)
                                     : string.Empty;
            if (customParameters != null && !string.IsNullOrEmpty(refParentTable))
                customParameters = "&" + customParameters;
            return "/MainPage.aspx/data/" + HttpUtility.UrlEncode(ParentTable) + typePage + "?" + refParentTable + customParameters;
        }

        public virtual string GetNameByContext(RenderContext context)
        {
            if (!context.OtherColumns.ContainsKey(ColumnName))
                throw new ArgumentException("RenderContext does not contain column with name " + ColumnName);
            return GetName(context.OtherColumns[ColumnName]);
        }

        private bool _executeGetName;

        public virtual string GetName(RenderContext context)
        {
            if (context.GroupValues != null) return GetGroupName(context);
            if (GetNameHandler != null && !_executeGetName)
            {
                _executeGetName = true;
                try
                {
                    return GetNameHandler(context);
                }
                finally
                {
                    _executeGetName = false;
                }
            }
            return GetName(context.DataRow, context.CrossDataItemRow, context.CrossColumnId, context.GroupValues);
        }

        private bool _executeGetEmptyCell;

        public virtual string GetEmptyCell(RenderContext renderContext)
        {
            if (GetEmptyCellHandler != null && !_executeGetEmptyCell)
            {
                _executeGetEmptyCell = true;
                try
                {
                    return GetEmptyCellHandler(renderContext);
                }
                finally
                {
                    _executeGetEmptyCell = false;
                }
            }

            return "&nbsp;";
        }

        public virtual string GetRowNumber(RenderContext context)
        {
            if (GetRowNumberHandler != null) return GetRowNumberHandler(context);
            return string.Empty;
        }

        public virtual Control GetEditControl(RenderContext context)
        {
            return GetEditControlHandler == null ? null : GetEditControlHandler(context);
        }

        public virtual IRenderComponent GetEditComponent(RenderContext context)
        {
            return GetEditComponentHandler == null ? null : GetEditComponentHandler(context);
        }

        public virtual IRenderComponent GetEditRenderComponent(RenderContext renderContext)
        {
            var editComponent = GetEditComponent(renderContext);
            editComponent.UniqueID = renderContext.UniqueID;
            editComponent.ClientID = renderContext.ClientID;
            editComponent.Mandatory |= EditMandatory;
            return editComponent;
        }

        public virtual bool ShowAsTemplate(RenderContext context)
        {
            if (ShowAsTemplateHandler != null)
                return ShowAsTemplateHandler(context);
            return true;
        }

        public virtual bool CustomRenderEditControl(HtmlTextWriter writer, BaseJournalCell cell, RenderContext renderContext, object value, string text)
        {
            var editComponent = GetEditRenderComponent(renderContext);
            if (editComponent != null)
            {
                if (text != null) editComponent.Text = text;
                if (value != null) editComponent.Value = value;
                editComponent.Enabled = cell.Enabled;
                editComponent.Render(writer, renderContext.JournalRow.ExtenderAjaxControl);
                return true;
            }
            return false;
        }

        public virtual string GetClientID(RenderContext context)
        {
            var sb = new StringBuilder();
            sb.Append("c").Append(context.DataRow == null ? "0" : context.DataRow.Value.Replace(",", "_"));
            AddKeyForCross(sb, context);
            sb.Append("_").Append(ColumnName);
            sb.Replace(' ', '_')
                .Replace('(', '_')
                .Replace(')', '_')
                .Replace('-', '_')
                .Replace('.', '_')
                .Replace(',', '_');
            return sb.ToString();
        }

        private static void AddKeyForCross(StringBuilder sb, RenderContext context)
        {
            if (!string.IsNullOrEmpty(context.CrossColumnId))
                sb.Append("_" + context.CrossColumnId);

            if (context.CrossDataItemKey != null && context.CrossDataItemKey.Count > 0)
            {
                foreach (var groupKeyItem in context.CrossDataItemKey)
                    sb.Append("_").Append(groupKeyItem.Key).Append("_").Append(groupKeyItem.Value.Value);
            }
            else if (string.IsNullOrEmpty(context.CrossColumnId))
                sb.Append("_0");
        }

        public virtual string GetRadioButtonCellID(RenderContext context)
        {
            var sb = new StringBuilder();
            sb.Append("c").Append(context.DataRow == null ? "0" : context.DataRow.Value.Replace(",", "_"));

            if (IsRadioButtonInRow)
            {
                AddKeyForCross(sb, context);
                return sb.ToString();
            }
            
            if (IsRadioButtonForRows)
                return sb.ToString();

            AddKeyForCross(sb, context);
            sb.Append("_").Append(ColumnName);
            return sb.ToString();
        }

        public virtual bool AllowEditValue(RenderContext context)
        {
            return AllowEditColumnValue
                   //либо можно редактировать в журнале, либо стоит текущий рендеринг в детализированном виде
                   && (!AllowEditOnlyInDetails || context.Journal.DetailsRender)
                   //есть значение в строке, у строки флаг того что можно редактировать
                   && (!CheckCanEditRow || context.DataRow != null && context.DataRow.CanEdit)
                   //либо не кросовая колонка, либо у кросовой колонки есть строка
                   && (context.CrossColumnId == null || context.CrossDataItemRow != null || context.Journal.AllowEditEmptyCell)
                   //либо нет обработчика, либо делегат вернул true
                   && (AllowEditValueHandler == null || AllowEditValueHandler(context));
        }

        public virtual bool ValidateValue(BaseJournalValidateValueEventArgs args)
        {
            if (EditMandatory && (args.NewValue == null || string.Empty.Equals(args.NewValue)))
            {
                args.RenderContext.AddErrorMessage("{1}: " + Resources.SRequiredFieldMessage,
                                        (LocalizationHelper.IsCultureKZ ? HeaderKz : HeaderRu) ?? Header,
                                        args.RenderContext.DataRow.Name);
                return false;
            }
            if (EditRegexValidation != null)
            {
                var newValueStr = args.NewValue == null ? string.Empty : args.NewValue.ToString();
                if (!string.IsNullOrEmpty(newValueStr))
                {
                    var validateRegex = EditRegexValidation.
                        Select(r => new
                        {
                            Match = (new Regex(r.Key)).Match(newValueStr),
                            Message = r.Value,
                        }).
                        Where(r => !r.Match.Success || r.Match.Index != 0 || r.Match.Length != newValueStr.Length).
                        Select(r => "{0}: " + r.Message);
                    args.RenderContext.AddErrorMessage(validateRegex, args.RenderContext.DataRow.Name);
                }
            }
            if (ValidateValueHandler != null) ValidateValueHandler(this, args);
            args.Cancel = args.RenderContext.Errors != null && args.RenderContext.Errors.Count > 0;
            return !args.Cancel;
        }

        public virtual bool UpdateValue(BaseJournalUpdateValueEventArgs args)
        {
            if (UpdateValueHandler != null)
                UpdateValueHandler(this, args);
            return !args.Cancel;
        }

        public virtual void DependenceColumns(RenderContext context, params BaseColumn[] columns)
        {
            foreach (var column in columns)
            {
                if (column.IsCrossColumn)
                {
                    foreach (var columnName in column.BaseCrossColumnDataSource.GetCrossColumnNames())
                    {
                        var renderContext = context.OtherColumns[columnName];
                        this.DependedColumn(renderContext, column);
                    }
                }
                else if (context.OtherColumns.ContainsKey(column.ColumnName))
                {
                    var renderContext = context.OtherColumns[column.ColumnName];
                    this.DependedColumn(renderContext, column);
                }
            }
        }

        private void DependedColumn(RenderContext renderContext, BaseColumn column)
        {
            renderContext.ComputeAggregates();

            if (column.GetRowsCount(renderContext) > this.RowIndex)
            {
                column.RowIndex = this.RowIndex;
            }

            renderContext.LoadData(column.BaseJournalCrossTable);
        }

        public virtual void DependenceColumns(object row, object item, params BaseColumn[] columns)
        {
            DependenceColumns(row, item, null, null, null, null, columns);
        }

        public virtual void DependenceColumns(object row, object item, object journalRow, BaseJournalControl journal, string crossColumnId, object[] groupValues,
            params BaseColumn[] columns)
        {
            foreach (var column in columns)
                if (column.GetRowsCount(row, item, journalRow, journal, crossColumnId, groupValues) > RowIndex)
                    column.RowIndex = RowIndex;
        }

        protected virtual int GetRowsCount(BaseJournalControl journal, object[] groupValues, string crossColumnId)
        {
            if (!ComputeForGroups(journal, groupValues) || _aggregateData == null) return 1;
            //todo: необходимо посчитать количество уникальных записей инлайн группы
            /*int computeIndex = groupValues.Length - 2;
            if (InlineGrouping)
                computeIndex = journal.GroupColumns.IndexOf(ColumnName);
            else if (UsingInGroup || AggregateType == ColumnAggregateType.GroupText || AggregateType == ColumnAggregateType.GroupTextWithTotalPhrase)
                computeIndex = groupValues.Length - 1;
            //те что значения в колекции относятся к группе их проверяем на совпадения
            // по последней инлайнгруппе определяем количество записей
            // те колонки что используются в инлайн группе определяют значения по своему индексу
            // те группы что выше инлайн используются для перехода на дочки
            count = _aggregateData.GetCount(groupValues, computeIndex, 0, groupValues.Length - 1);*/
            int count;
            var agg = _aggregateData;
            if (crossColumnId != null) agg = agg.GetChildGroup(crossColumnId);

            if (InlineGrouping)
            {
                var index = journal.GroupColumns.IndexOf(ColumnName);
                count = agg.GetCount(groupValues, journal.IsInlineGroup, index, 0);
            }
            else
            {
                count = agg.GetCount(groupValues, journal.IsInlineGroup, 0);
            }
            if (count == 0) return 1;
            return count;
        }

        private bool _executeGetRowsCountHandler;

        protected bool ExecuteGetRowsCount(RenderContext context, out int count)
        {
            count = -1;
            if (GetRowsCountHandler == null || _executeGetRowsCountHandler)
                return false;

            try
            {
                _executeGetRowsCountHandler = true;
                count = GetRowsCountHandler(context);
                return true;
            }
            finally
            {
                _executeGetRowsCountHandler = false;
            }
        }

        public virtual int GetRowsCount(RenderContext context)
        {
            int count;
            if (ExecuteGetRowsCount(context, out count))
                return count;

            if (IsCrossColumn)
                return BaseCrossColumnDataSource.GetRowsCount(context);
            var groupValues = context.GroupValues;
            if (!ComputeForGroups(context.Journal, groupValues) || _aggregateData == null) return 1;
            //todo: необходимо посчитать количество уникальных записей инлайн группы
            /*int computeIndex = groupValues.Length - 2;
            if (InlineGrouping)
                computeIndex = journal.GroupColumns.IndexOf(ColumnName);
            else if (UsingInGroup || AggregateType == ColumnAggregateType.GroupText || AggregateType == ColumnAggregateType.GroupTextWithTotalPhrase)
                computeIndex = groupValues.Length - 1;
            //те что значения в колекции относятся к группе их проверяем на совпадения
            // по последней инлайнгруппе определяем количество записей
            // те колонки что используются в инлайн группе определяют значения по своему индексу
            // те группы что выше инлайн используются для перехода на дочки
            count = _aggregateData.GetCount(groupValues, computeIndex, 0, groupValues.Length - 1);*/
            var agg = _aggregateData;
            if (context.CrossColumnId != null) agg = agg.GetChildGroup(context.CrossColumnId);
            if (context.CrossDataItemKey != null) agg = agg.GetChildGroup(context.CrossDataItemKey.CloneNotInlineGroups());

            if (InlineGrouping)
            {
                var index = context.Journal.GroupColumns.IndexOf(ColumnName);
                count = agg.GetCount(groupValues, context.Journal.IsInlineGroup, index, 0);
            }
            else
            {
                count = agg.GetCount(groupValues, context.Journal.IsInlineGroup, 0);
            }
            if (count == 0) return 1;
            return count;
        }

        #endregion

        #region CrossColumns methods

        public virtual BaseColumn GetColumn(string columnName)
        {
            if (!IsCrossColumn)
                throw new NotSupportedException("supported if IsCrossColumn is true");
            return BaseCrossColumnDataSource.GetColumns().
                FirstOrDefault(r => columnName.Equals(r.ColumnName));
        }

        public virtual object GetCrossItem(RenderContext context)
        {
            if (BaseJournalCrossTable != null)
            {
                context.LoadData(BaseJournalCrossTable);
                return BaseJournalCrossTable.GetDataItem(context);
            }

            return GetCrossItem(context.JournalRow, context.CrossColumnIdObject, 0);
        }
        
        public virtual object GetCrossItem(object journalRow, object key, int index)
        {
            return null;
        }

        public virtual int CountOfCrossData(object journalRow, object key)
        {
            return 1;
        }

        #endregion

        #region Aggregate

        private bool ComputeForGroups(BaseJournalControl journal, object[] groupValues)
        {
            if (string.IsNullOrEmpty(ColumnName)) return false;
            var index = journal.GroupColumns.IndexOf(ColumnName);
            return AllowAggregate && (index < 0 || groupValues.Length > index);
        }
        
        public virtual object[] GetTotalRowsValues(BaseJournalControl journal, object[] groupValues, string crossColumnId)
        {
            if (!ComputeForGroups(journal, groupValues) || _aggregateData == null) return null;
            /*int computeIndex = groupValues.Length - 2;
            if (InlineGrouping)
                computeIndex = journal.GroupColumns.IndexOf(ColumnName);
            else if (UsingInGroup || AggregateType == ColumnAggregateType.GroupText || AggregateType == ColumnAggregateType.GroupTextWithTotalPhrase)
                computeIndex = groupValues.Length - 1;
            values = _aggregateData.GetTotalRowsValues(groupValues, computeIndex, 0, groupValues.Length - 1, true).ToArray();*/
            object[] values;
            var agg = _aggregateData;
            if (crossColumnId != null) agg = agg.GetChildGroup(crossColumnId);
            if (InlineGrouping)
            {
                var index = journal.GroupColumns.IndexOf(ColumnName);
                values = agg.GetValues(groupValues, journal.IsInlineGroup, index, 0).ToArray();
            }
            else
            {
                values = agg.GetValues(groupValues, journal.IsInlineGroup, 0).Cast<object>().ToArray();
            }
            return values;
        }

        public virtual object[] GetTotalRowsKeys(RenderContext context)
        {
            if (!ComputeForGroups(context.Journal, context.GroupValues) || _aggregateData == null) return null;
            /*int computeIndex = groupValues.Length - 2;
            if (InlineGrouping)
                computeIndex = journal.GroupColumns.IndexOf(ColumnName);
            else if (UsingInGroup || AggregateType == ColumnAggregateType.GroupText || AggregateType == ColumnAggregateType.GroupTextWithTotalPhrase)
                computeIndex = groupValues.Length - 1;
            values = _aggregateData.GetTotalRowsValues(groupValues, computeIndex, 0, groupValues.Length - 1, true).ToArray();*/
            object[] values = null;
            /*if (InlineGrouping)
            {
                //var index = journal.GroupColumns.IndexOf(ColumnName);
                //values = _aggregateData.GetKeys(groupValues, journal.IsInlineGroup, index, 0).ToArray();
                values = null;
            }
            else*/
            {
                var agg = _aggregateData;
                if (context.CrossColumnId != null) agg = agg.GetChildGroup(context.CrossColumnId);
                if (context.CrossDataItemKey != null) agg = agg.GetChildGroup(context.CrossDataItemKey.CloneNotInlineGroups());
                var result = agg.GetKeys(context.GroupValues, context.Journal.IsInlineGroup, 0);
                if (result != null)
                    values = result.ToArray();
            }
            return values;
        }

        public void ComputeAggregates(RenderContext context, ColumnAggregateType userAggregateType)
        {
            if (ComputeAggregatesHandler != null && ComputeAggregatesHandler(context, userAggregateType))
                return;

            if (IsCrossColumn && BaseCrossColumnDataSource.ComputeAggregates(context, userAggregateType))
                return;

            UserAggregateType = userAggregateType;
            if (!AllowAggregate || context.GroupValues != null) return;

            if (_aggregateData == null)
                _aggregateData = new AggregateDataSimple { AggregateType = userAggregateType };

            ComputeAggregatesFor(context, userAggregateType, _aggregateData, context.Journal.GroupColumns);
        }

        public void ComputeAggregatesFor(RenderContext context, ColumnAggregateType userAggregateType, AggregateData agg, List<GroupColumn> groups)
        {
            if (context.CrossColumnId != null)
            {
                agg = agg.GetChildGroup(context.CrossColumnId);
                agg.AggregateType = userAggregateType;
            }

            if (context.CrossDataItemKey != null)
            {
                agg = agg.GetChildGroup(context.CrossDataItemKey.CloneNotInlineGroups());
                agg.AggregateType = userAggregateType;
            }

            if (AggregateGroupedByRowIndex)
            {
                agg = agg.GetChildGroup(context.RowIndex);
                agg.AggregateType = userAggregateType;
            }

            double? value = null;
            if (userAggregateType == ColumnAggregateType.GroupText
                || userAggregateType == ColumnAggregateType.GroupTextWithTotalPhrase
                || (ColumnType == ColumnType.Other && userAggregateType != ColumnAggregateType.Count))
            {
                agg.StrValue = GetTotalPhrase(context, this);
            }
            else if (userAggregateType != ColumnAggregateType.None)
            {
                var objValue = GetValue(context);
                if (objValue != null && userAggregateType != ColumnAggregateType.Count)
                    value = Convert.ToDouble(objValue);
                else if (userAggregateType == ColumnAggregateType.Count)
                    value = 0;
                ComputeAggregate(agg, value);
            }

            var getGroupName = !InlineGrouping;
            foreach (var groupColumnName in groups)
            {
                var groupColumnContext = !context.OtherColumns.ContainsKey(groupColumnName.ColumnName) ? null : context.OtherColumns[groupColumnName.ColumnName];
                var groupColumn = context.Journal.BaseInnerHeader.ColumnsDic[groupColumnName.ColumnName];
                var rowValue = groupColumnContext == null ? null : groupColumn.GetValue(groupColumnContext, false);

                var childAgg = agg.GetChildGroup(rowValue);
                if (childAgg == null)
                    break;

                if (userAggregateType == ColumnAggregateType.GroupText || InlineGrouping /* || (GroupType & GroupType.Left) == GroupType.Left*/)
                {
                    if (!getGroupName) getGroupName = groupColumn == this;

                    string str = null;
                    if (groupColumn.InlineGrouping && groupColumn != this)
                        str = GetTotalPhrase(context, groupColumn);
                    else if (InlineGrouping && !getGroupName)
                        str = string.Empty;
                    else if (groupColumnContext != null)
                        str = groupColumn.GetName(groupColumnContext);
                    childAgg.StrValue = str;
                }
                else if (userAggregateType == ColumnAggregateType.GroupTextWithTotalPhrase)
                {
                    var str = groupColumnContext == null || groupColumn.InlineGrouping ? null : groupColumn.GetName(groupColumnContext);
                    childAgg.StrValue = GetTotalPhraseFor(context, groupColumn, str);
                }
                else if (userAggregateType != ColumnAggregateType.None)
                    ComputeAggregate(childAgg, value);
                agg = childAgg;
            }
        }

        protected virtual string GetTotalPhraseFor(RenderContext context, BaseColumn groupColumn, string value)
        {
            if (groupColumn.GetTotalPhraseForHandler != null) return groupColumn.GetTotalPhraseForHandler(context.OtherColumns[groupColumn.ColumnName], value);
            if (GetTotalPhraseForHandler != null) return GetTotalPhraseForHandler(context, value);
            if (!string.IsNullOrEmpty(value)) 
                return value.IndexOf(Resources.STotal, StringComparison.InvariantCultureIgnoreCase) > -1 ? value : string.Format(Resources.STotalFor, value);

            return this.GetTotalPhrase(context, groupColumn);
        }

        protected virtual string GetTotalPhrase(RenderContext context, BaseColumn groupColumn)
        {
            if (groupColumn.GetTotalPhraseHandler != null) return groupColumn.GetTotalPhraseHandler(context.OtherColumns[groupColumn.ColumnName]);
            if (GetTotalPhraseHandler != null) return GetTotalPhraseHandler(context);
            return Resources.STotal;
        }

        public void FinishCompute()
        {
            if (_aggregateData == null || finishComputeExecuted) return;

            finishComputeExecuted = true;

            switch (AggregateType)
            {
                case ColumnAggregateType.None:
                case ColumnAggregateType.GroupText:
                case ColumnAggregateType.GroupTextWithTotalPhrase:
                    break;
                case ColumnAggregateType.Sum:
                    break;
                case ColumnAggregateType.Avg:
                    if (_aggregateData.Count > 0)
                        _aggregateData.Value = (_aggregateData.Value ?? 0) / _aggregateData.Count;
                    break;
                case ColumnAggregateType.Max:
                    break;
                case ColumnAggregateType.Min:
                    break;
                case ColumnAggregateType.Count:
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
            switch (_aggregateData.AggregateType)
            {
                case ColumnAggregateType.None:
                case ColumnAggregateType.GroupText:
                case ColumnAggregateType.GroupTextWithTotalPhrase:
                    break;
                case ColumnAggregateType.Sum:
                    break;
                case ColumnAggregateType.Avg:
                    if (_aggregateData.Count > 0)
                        _aggregateData.UserValue = (_aggregateData.UserValue ?? 0) / _aggregateData.Count;
                    break;
                case ColumnAggregateType.Max:
                    break;
                case ColumnAggregateType.Min:
                    break;
                case ColumnAggregateType.Count:
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            foreach (var agg in _aggregateData.Childs)
            {
                switch (AggregateType)
                {
                    case ColumnAggregateType.None:
                    case ColumnAggregateType.GroupText:
                    case ColumnAggregateType.GroupTextWithTotalPhrase:
                        break;
                    case ColumnAggregateType.Sum:
                        break;
                    case ColumnAggregateType.Avg:
                        FinishComputeAvg(agg);
                        break;
                    case ColumnAggregateType.Max:
                        break;
                    case ColumnAggregateType.Min:
                        break;
                    case ColumnAggregateType.Count:
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
                switch (agg.AggregateType)
                {
                    case ColumnAggregateType.None:
                    case ColumnAggregateType.GroupText:
                    case ColumnAggregateType.GroupTextWithTotalPhrase:
                        break;
                    case ColumnAggregateType.Sum:
                        break;
                    case ColumnAggregateType.Avg:
                        FinishUserComputeAvg(agg);
                        break;
                    case ColumnAggregateType.Max:
                        break;
                    case ColumnAggregateType.Min:
                        break;
                    case ColumnAggregateType.Count:
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
        }

        protected void FinishComputeAvg(AggregateData agg)
        {
            if (agg.Count > 0)
                agg.Value = (agg.Value ?? 0)/agg.Count;
            foreach (var childAgg in agg.Childs)
                FinishComputeAvg(childAgg);
        }

        protected void FinishUserComputeAvg(AggregateData agg)
        {
            if (agg.Count > 0)
                agg.UserValue = (agg.UserValue ?? 0) / agg.Count;
            foreach (var childAgg in agg.Childs)
                FinishUserComputeAvg(childAgg);
        }

        protected void ComputeAggregate(AggregateData agg, double? value)
        {
            if (value.HasValue || ComputeCountIfNullValue) 
                agg.Count++;
            switch (AggregateType)
            {
                case ColumnAggregateType.None:
                case ColumnAggregateType.GroupText:
                case ColumnAggregateType.GroupTextWithTotalPhrase:
                    break;
                case ColumnAggregateType.Avg:
                case ColumnAggregateType.Sum:
                    if (value != null)
                        agg.Value = (agg.Value ?? 0) + value.Value;
                    break;
                case ColumnAggregateType.Max:
                    if (value != null && (agg.Value == null || agg.Value.Value < value.Value))
                        agg.Value = value.Value;
                    break;
                case ColumnAggregateType.Min:
                    if (value != null && (agg.Value == null || agg.Value.Value > value.Value))
                        agg.Value = value.Value;
                    break;
                case ColumnAggregateType.Count:
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            switch (agg.AggregateType)
            {
                case ColumnAggregateType.None:
                case ColumnAggregateType.GroupText:
                case ColumnAggregateType.GroupTextWithTotalPhrase:
                    break;
                case ColumnAggregateType.Sum:
                case ColumnAggregateType.Avg:
                    if (value != null)
                        agg.UserValue = (agg.UserValue ?? 0) + value.Value;
                    break;
                case ColumnAggregateType.Max:
                    if (value != null && (agg.UserValue == null || agg.UserValue.Value < value.Value))
                        agg.UserValue = value.Value;
                    break;
                case ColumnAggregateType.Min:
                    if (value != null && (agg.UserValue == null || agg.UserValue.Value > value.Value))
                        agg.UserValue = value.Value;
                    break;
                case ColumnAggregateType.Count:
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
        
        protected virtual AggregateData CreateAggregateDataClass()
        {
            return new AggregateDataSimple();
        }
        
        #endregion

        #region Aggregate Value

        public virtual AggregateData GetAggregateData(RenderContext context)
        {
            return GetAggregateData(context, _aggregateData);
        }

        public AggregateData GetAggregateData(RenderContext context, AggregateData agg)
        {
            if (agg == null) return null;
            if (context.CrossColumnId != null) agg = agg.GetChildGroup(context.CrossColumnId);
            if (context.CrossDataItemKey != null) agg = agg.GetChildGroup(context.CrossDataItemKey.CloneNotInlineGroups());
            if (AggregateGroupedByRowIndex) agg = agg.GetChildGroup(context.RowIndex);
            UserAggregateType = agg.AggregateType;
            if (!AllowAggregate) return null;

            if (context.GroupValues == null || context.GroupValues.Length == 0)
                return agg;

            for (int i = 0; i < context.GroupValues.Length; i++)
                agg = agg.GetChildGroup(context.GroupValues[i]);
            return agg;
        }

        private bool _executeGetTotalValueHandler;
        protected object GetGroupValue(RenderContext context)
        {
            try
            {
                if (GetTotalValueHandler != null && !_executeGetTotalValueHandler)
                {
                    _executeGetTotalValueHandler = true;
                    return GetTotalValueHandler(context);
                }
            }
            finally
            {
                _executeGetTotalValueHandler = false;
            }
            var agg = GetAggregateData(context);
            if (agg == null) return null;
            if (UserAggregateType == ColumnAggregateType.GroupText 
                || UserAggregateType == ColumnAggregateType.GroupTextWithTotalPhrase
                || InlineGrouping)
                return agg.StrValue ?? string.Empty;
            if (UserAggregateType == ColumnAggregateType.Count)
                return agg.Count;
            if (UserAggregateType != ColumnAggregateType.None)
                return agg.UserValue ?? agg.Value;
            return null;
        }

        private bool _executeGetTotalNameHandler;

        protected string GetGroupName(RenderContext context)
        {
             try
            {
                if (GetTotalNameHandler != null && !_executeGetTotalNameHandler)
                {
                    _executeGetTotalNameHandler = true;
                    return GetTotalNameHandler(context);
                }
            }
            finally
            {
                _executeGetTotalNameHandler = false;
            }
            
            if (InlineGrouping && context.GroupValues.Length > 1)
            {
                //todo: -1 указывает сколько групп нужно убрать
                context.GroupValues = context.GroupValues.Take(1).ToArray();
            }

            var agg = GetAggregateData(context);
            if (agg == null) return string.Empty;
            if (UserAggregateType == ColumnAggregateType.GroupText
                || UserAggregateType == ColumnAggregateType.GroupTextWithTotalPhrase || InlineGrouping)
            {
                return agg.StrValue ?? string.Empty;
            }
            
            if (UserAggregateType == ColumnAggregateType.Count)
                return agg.Count.ToString();
            
            if (UserAggregateType != ColumnAggregateType.None)
            {
                if (string.IsNullOrEmpty(AggregateFormat)) 
                    return (agg.UserValue ?? agg.Value).ToString();

                return string.Format(AggregateFormat, agg.UserValue ?? agg.Value);
            }

            return string.Empty;
        }

        public virtual double? GetAggregateValue(RenderContext context)
        {
            var agg = GetAggregateData(context);
            if (agg == null) return null;
            return agg.Value;
        }

        public virtual double? GetAggregateValue(string crossColumnId, params object[] groupValues)
        {
            var agg = GetAggregateData(crossColumnId, groupValues);
            if (agg == null) return null;
            return agg.Value;
        }

        public virtual double? GetUserAggregateValue(string crossColumnId, params object[] groupValues)
        {
            var agg = GetAggregateData(crossColumnId, groupValues);
            if (agg == null) return null;
            return agg.UserValue;
        }

        protected void GetContent(HtmlTextWriter writer, RenderContext context)
        {
            var agg = GetAggregateData(context);
            if (agg == null) return;
            if (UserAggregateType == ColumnAggregateType.GroupText
                || UserAggregateType == ColumnAggregateType.GroupTextWithTotalPhrase
                || InlineGrouping)
            {
                writer.Write(agg.StrValue);
            }
            else if (UserAggregateType == ColumnAggregateType.Count)
                writer.Write(agg.Count);
            else if (UserAggregateType != ColumnAggregateType.None)
                writer.Write(agg.UserValue);
        }

        public virtual AggregateData GetAggregateData(string crossColumnId, params object[] groupValues)
        {
            var agg = _aggregateData;
            if (agg == null) return null;
            if (crossColumnId != null) agg = agg.GetChildGroup(crossColumnId);
            UserAggregateType = agg.AggregateType;
            if (!AllowAggregate) return null;

            if (groupValues == null || groupValues.Length == 0)
                return agg;

            foreach (var groupValue in groupValues)
                agg = agg.GetChildGroup(groupValue);
            return agg;
        }

        protected object GetGroupValue(string crossColumnId, object[] groupValues)
        {
            var agg = GetAggregateData(crossColumnId, groupValues);
            if (agg == null) return null;
            if (UserAggregateType == ColumnAggregateType.GroupText
                || UserAggregateType == ColumnAggregateType.GroupTextWithTotalPhrase
                || InlineGrouping)
                return agg.StrValue ?? string.Empty;
            if (UserAggregateType == ColumnAggregateType.Count)
                return agg.Count;
            if (UserAggregateType != ColumnAggregateType.None)
                return agg.UserValue ?? agg.Value;
            return null;
        }

        protected string GetGroupName(string crossColumnId, object[] groupValues)
        {
            if (InlineGrouping && groupValues.Length > 1)
            {
                //todo: -1 указывает сколько групп нужно убрать
                groupValues = groupValues.Take(groupValues.Length - 1).ToArray();
            }
            var agg = GetAggregateData(crossColumnId, groupValues);
            if (agg == null) return string.Empty;
            if (UserAggregateType == ColumnAggregateType.GroupText
                || UserAggregateType == ColumnAggregateType.GroupTextWithTotalPhrase 
                || InlineGrouping)
            {
                return agg.StrValue ?? string.Empty;
            }

            if (UserAggregateType == ColumnAggregateType.Count)
                return agg.Count.ToString();
            
            if (UserAggregateType != ColumnAggregateType.None)
            {
                if (string.IsNullOrEmpty(AggregateFormat)) 
                    return (agg.UserValue ?? agg.Value).ToString();

                return string.Format(AggregateFormat, agg.UserValue ?? agg.Value);
            }

            return string.Empty;
        }

        protected void GetContent(HtmlTextWriter writer, string crossColumnId, object[] groupValues)
        {
            var agg = GetAggregateData(crossColumnId, groupValues);
            if (agg == null) return;
            if (UserAggregateType == ColumnAggregateType.GroupText
                || UserAggregateType == ColumnAggregateType.GroupTextWithTotalPhrase
                || InlineGrouping)
            {
                writer.Write(agg.StrValue);
            }
            else if (UserAggregateType == ColumnAggregateType.Count)
                writer.Write(agg.Count);
            else if (UserAggregateType != ColumnAggregateType.None)
                writer.Write(agg.UserValue);
        }

        #endregion

        #region inner CrossDataSource

        private class BaseColumnDS : CrossTreeColumnDataSource<DataItemKey, BaseRow, DataItem>
        {
            public BaseColumn BaseColumn { get; set; }
            
            protected override IQueryable<DataItem> GetData()
            {
                List<DataItem> data = null;
                List<DataItem> lastData = null;

                var journal = HeaderControl.Journal;
                for (int i = 0; i < journal.GroupColumns.Count; i++)
                {
                    if (journal.GroupColumns[i].GroupType != GroupType.InHeader 
                        || journal.IsInlineGroup[i]
                        || journal.GroupColumns[i].Equals(BaseColumnName)
                        || !BaseColumn.GroupByColumns.Contains(journal.GroupColumns[i].ColumnName)) continue;
                    var column = HeaderControl.ColumnsDic[journal.GroupColumns[i].ColumnName];
                    var listItems = column.BaseCrossColumnDataSource.GetListItems();
                    if (data == null)
                        data = listItems.Select(
                            r => new DataItem(new GroupKeys(r.BaseColumnName, r.ColumnIdObject).AsDataItemKey(), r.Header)).
                            ToList();
                    else
                    {
                        var sourceItems = listItems.ToList();
                        foreach (var dataItem in lastData)
                        {
                            var id = dataItem.Id;
                            var list = sourceItems.Select(
                                r =>
                                new DataItem(id.GroupKeysValue.Clone(r.BaseColumnName, r.ColumnIdObject).AsDataItemKey(), r.Header));

                            dataItem.ChildObjects = new EntitySet<DataItem>();
                            dataItem.ChildObjects.AddRange(list);
                            foreach (var item in dataItem.ChildObjects)
                            {
                                item.ParentObject = dataItem;
                                item.refParent = dataItem.Id;
                            }
                        }
                    }
                    lastData = data;
                }
                if (data == null) throw new Exception("data not detect");
                return data.AsQueryable();
            }

            protected override IQueryable<DataItem> GetData(string key)
            {
                throw new System.NotImplementedException();
            }

            protected override string GetColumnHeader(DataItem row)
            {
                return row.Value;
            }

            protected override string GetColumnHeaderRu(DataItem row)
            {
                return GetColumnHeader(row);
            }

            protected override string GetColumnHeaderKz(DataItem row)
            {
                return GetColumnHeader(row);
            }

            protected override void InitializeColumns()
            {
                BaseColumn.HideInHeader = true;
                //Columns.Add(BaseColumn);
                Columns.Add(new BaseColumn
                                {
                                    HideInHeader = true,
                                    ColumnName = BaseColumnName + "CrossColumn",
                                    GetValueHandler = context => BaseColumn.GetValue(context),
                                    GetNameHandler = context => BaseColumn.GetName(context),
                                    GetEmptyCellHandler = context => BaseColumn.GetEmptyCell(context),
                                    GetRowNumberHandler = context => BaseColumn.GetRowNumber(context),
                                    GetRowsCountHandler = context => BaseColumn.GetRowsCount(context),
                                    GetTotalValueHandler = BaseColumn.GetTotalValueHandler,
                                    GetTotalNameHandler = BaseColumn.GetTotalNameHandler,
                                    GetVisibleHandler = BaseColumn.GetVisibleHandler,
                                    GetHyperLinkHandler = BaseColumn.GetHyperLinkHandler,
                                    Header = BaseColumn.Header,
                                    HeaderRu = BaseColumn.HeaderRu,
                                    HeaderKz = BaseColumn.HeaderKz,
                                    AllowAggregate = BaseColumn.AllowAggregate,
                                    AggregateType = BaseColumn.AggregateType,
                                    ComputeCountIfNullValue = BaseColumn.ComputeCountIfNullValue,
                                    HideIfValueEquals = BaseColumn.HideIfValueEquals,
                                    ColumnType = BaseColumn.ColumnType,
                                    ValueExpression = BaseColumn.ValueExpression,
                                    NullText = BaseColumn.NullText,
                                    BaseJournalCrossTable = BaseColumn.BaseJournalCrossTable,
                                    BaseCrossColumnDataSource = BaseColumn.BaseCrossColumnDataSource,
                                    Format = BaseColumn.Format,
                                    TypeCell = BaseColumn.TypeCell,
                                    AggregateFormat = BaseColumn.AggregateFormat,
                                    AggregateGroupedByRowIndex = BaseColumn.AggregateGroupedByRowIndex,
                                    ComputeAggregatesHandler = BaseColumn.ComputeAggregatesHandler,
                                    GroupByColumns =  BaseColumn.GroupByColumns,
                                });
                base.InitializeColumns();
            }
            
            protected override void RenderHeaderContent(CrossColumnDataSourceItem<DataItemKey,BaseRow,DataItem> column, HtmlTextWriter writer)
            {
                writer.Write(column.Header);
            }

            protected override void InitHierarchy(ColumnHierarchy newItem, CrossColumnDataSourceItem item, Dictionary<string, ColumnHierarchy> existsColumns, Dictionary<string, BaseColumn> columnsDic)
            {
                newItem.CrossColumnIDObject = newItem.Parent.CrossColumnIDObject;
                newItem.CrossColumnID = newItem.Parent.CrossColumnID;
                if (newItem.BaseColumn != null)
                {
                    newItem.Parent.BaseColumn = newItem.BaseColumn;
                    newItem.Delete = true;
                    newItem.Parent.CrossDataItemKey = ((DataItemKey)item.GetColumnID()).GroupKeysValue;
                    var groupColumn = newItem.Parent.CrossDataItemKey.Where(r => !r.Value.IsInlineGroup).Select(r => r.Key).FirstOrDefault();
                    if (!string.IsNullOrEmpty(groupColumn) && HeaderControl.ColumnsDic.ContainsKey(groupColumn))
                        newItem.Parent.IsVerticalHeader = HeaderControl.ColumnsDic[groupColumn].IsVerticalHeader;
                    if (newItem.Parent.CrossColumnID != null)
                    {
                        newItem.Parent.CrossColumnKey = newItem.CrossColumnKey + "_" + newItem.Parent.CrossColumnID;
                        newItem.Parent.ColumnName = newItem.ColumnName + "_" + newItem.Parent.CrossColumnID;
                        newItem.Parent.GroupColumnName = newItem.ColumnName;
                    }
                    else
                        newItem.Parent.ColumnName = newItem.ColumnName;
                    newItem.Parent.AggregateType = newItem.AggregateType;
                    columnsDic[newItem.Parent.ColumnName] = newItem.BaseColumn;
                    if (existsColumns.ContainsKey(newItem.Parent.ColumnKey))
                        newItem.Parent.Init(existsColumns[newItem.Parent.ColumnKey]);
                }
                /*newItem.CrossDataItemKey = ((DataItemKey)item.GetColumnID()).GroupKeysValue;
                newItem.CrossColumnIDObject = newItem.Parent.CrossColumnIDObject;
                newItem.CrossColumnID = newItem.Parent.CrossColumnID;
                if (newItem.Parent.CrossColumnID != null)
                {
                    newItem.CrossColumnKey += "_" + newItem.Parent.CrossColumnID;
                    if(newItem.ColumnName != null)
                        newItem.ColumnName += "_" + newItem.Parent.CrossColumnID;
                }*/
                base.InitHierarchy(newItem, item, existsColumns, columnsDic);
            }

            protected override void InitedHierarchy(ColumnHierarchy newItem, CrossColumnDataSourceItem item, Dictionary<string, ColumnHierarchy> existsColumns, Dictionary<string, BaseColumn> columnsDic)
            {
                newItem.IsTreeColumn = false;
                //newItem.Init(newItem.Parent);
                base.InitedHierarchy(newItem, item, existsColumns, columnsDic);
            }
        }

        [Serializable]
        public class GroupKeys : SortedList<string, GroupKeyItem>, ICloneable, IComparable<GroupKeys>
        {
            private string Key;

            public GroupKeys()
            {
                Key = string.Empty;
            }

            public GroupKeys(string columnName, object value, bool isInlineGroup)
                : this()
            {
                AddValue(columnName, value, isInlineGroup);
            }

            public GroupKeys(string columnName, object value)
                : this(columnName, value, false)
            {
            }

            public GroupKeys(GroupKeys groupKeys, RenderContext renderContext)
                : this()
            {
                AddValues(groupKeys, renderContext);
            }

            public void AddValue(string columnName, object value)
            {
                Add(columnName, new GroupKeyItem(value));
                RefreshKey();
            }

            public void AddValue(string columnName, object value, bool isInlineGroup)
            {
                Add(columnName, new GroupKeyItem(value) {IsInlineGroup = isInlineGroup});
                RefreshKey();
            }

            public void AddValues(GroupKeys groupKeys, RenderContext renderContext)
            {
                if (renderContext.Column.GroupByColumns == null || groupKeys == null)
                    return;
                foreach (var column in renderContext.Column.GroupByColumns.Where(groupKeys.ContainsKey))
                    Add(column, (GroupKeyItem)groupKeys[column].Clone());
                RefreshKey();
            }

            private void RefreshKey()
            {
                if (Keys.Count == 0)
                    Key = string.Empty;
                else if (Keys.Count == 1)
                    Key = Keys[0];
                else
                    Key = Keys.Aggregate((v1, v2) => v1 + "," + v2);
            }

            public object Clone()
            {
                var groupKeys = Clone(false);
                groupKeys.RefreshKey();
                return groupKeys;
            }

            private GroupKeys Clone(bool onlyNotInlineGroups)
            {
                var groupKeys = new GroupKeys();
                var items = onlyNotInlineGroups ? this.Where(r => !r.Value.IsInlineGroup) : this;
                foreach (var item in items)
                    groupKeys.Add(item.Key, (GroupKeyItem) item.Value.Clone());
                return groupKeys;
            }

            public GroupKeys CloneNotInlineGroups()
            {
                var groupKeys = Clone(true);
                groupKeys.RefreshKey();
                return groupKeys;
            }

            public GroupKeys Clone(string columnName, object value)
            {
                var groupKeys = Clone(false);
                groupKeys.AddValue(columnName, value);
                groupKeys.RefreshKey();
                return groupKeys;
            }

            internal DataItemKey AsDataItemKey()
            {
                return new DataItemKey(this);
            }

            public override bool Equals(object obj)
            {
                var other = (GroupKeys) obj;
                if (Key == null && other != null && other.Key == null) return true;
                //если ключи разные то return false
                if (Key == null || other == null || !Key.Equals(other.Key)) return false;
                for (int i = 0; i < Values.Count; i++)
                {
                    var v1 = Values[i].Value;
                    var v2 = other.Values[i].Value;
                    if (v1 == null && v2 == null) continue;
                    if (v1 == null || !v1.Equals(v2)) return false;
                }
                return true;
            }

            public override int GetHashCode()
            {
                return Key == null ? 0 : Key.GetHashCode();
            }

            public override string ToString()
            {
                if (Keys.Count == 0)
                    return string.Empty;
                if (Keys.Count == 1)
                {
                    var item = this[Keys[0]];
                    return Keys[0] + ":" + item.Value;
                }
                return this.Select(v => v.Key + ":" + v.Value.ToString()).Aggregate((v1, v2) => v1 + ";" + v2);
            }

            public int CompareTo(GroupKeys other)
            {
                var result = Key.CompareTo(other.Key);
                if (result != 0) return result;
                for (int i = 0; i < Values.Count; i++)
                {
                    var v1 = Values[i].Value;
                    var v2 = other.Values[i].Value;
                    if (v1 == null && v2 == null) continue;
                    if (v1 == null) return -1;
                    if (v2 == null) return 1;
                    result = v1.ToString().CompareTo(v2.ToString());
                    if (result != 0) return result;
                }
                return 0;
            }
        }

        [Serializable]
        public class GroupKeyItem : ICloneable
        {
            public GroupKeyItem(object value)
            {
                Value = value;
            }

            public object Value;
            public bool IsInlineGroup;

            #region ICloneable Members

            public object Clone()
            {
                return new GroupKeyItem(Value) {IsInlineGroup = IsInlineGroup};
            }

            #endregion

            public override string ToString()
            {
                return "(" + IsInlineGroup + "," + Value + ")";
            }
        }

        public class DataItem : ICrossTreeTable<DataItemKey, DataItem>
        {
            public DataItem(DataItemKey key, string value)
            {
                Id = key;
                Value = value;
            }

            public string Value { get; set; }
            public DataItemKey? refParent { get; set; }
            public DataItem ParentObject { get; set; }
            public EntitySet<DataItem> ChildObjects { get; set; }
            public DataItemKey Id { get; set; }
        }

        [Serializable]
        public struct DataItemKey
        {
            public DataItemKey(GroupKeys groupKeys)
            {
                GroupKeysValue = groupKeys;
            }

            public GroupKeys GroupKeysValue;
            
            public override bool Equals(object obj)
            {
                var item = (DataItemKey) obj;
                return GroupKeysValue.Equals(item.GroupKeysValue);
            }

            public override int GetHashCode()
            {
                unchecked
                {
                    return GroupKeysValue.GetHashCode();
                }
            }

            public override string ToString()
            {
                return GroupKeysValue.ToString();
            }
        }

        #endregion
    }
}