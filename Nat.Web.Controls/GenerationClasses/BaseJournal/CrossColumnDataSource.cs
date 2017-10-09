using System;
using System.Collections;
using System.Collections.Generic;
using System.Data.Linq;
using System.Linq;
using System.Web.UI;
using Nat.Web.Controls.GenerationClasses.HierarchyFields;

namespace Nat.Web.Controls.GenerationClasses.BaseJournal
{
    public abstract class CrossColumnDataSource
    {
        private IEnumerable<string> _crossColumnNames;

        public string BaseColumnName { get; set; }
        public bool ColumnHierarchyCreated { get; set; }
        protected virtual internal int RowIndex { get; set; }
        public abstract Type HeaderType { get; }
        public BaseFilter Filter { get; set; }
        public virtual BaseJournalHeaderControl HeaderControl { get; set; }
        public abstract IList GetKeys();
        public abstract IEnumerable<CrossColumnDataSourceItem> GetListItems();
        public abstract int MaxLevel { get; }
        public abstract void RenderColumns(HtmlTextWriter writer, int level, int rowSpan);
        public abstract void RenderBackUrls(HtmlTextWriter writer);
        public abstract int GetFullColSpan();
        public abstract IEnumerable<BaseColumn> GetColumns();

        public virtual void CreateHierarchy(ColumnHierarchy parent, List<ColumnHierarchy> columnHierarchy, Dictionary<string, BaseColumn> columnsDic)
        {
            columnHierarchy.SetOrders();
            var existsColumns = columnHierarchy.GetAllItems().Where(r => r.ColumnKey != null).ToDictionary(r => r.ColumnKey);
            columnHierarchy.Clear();
            CreateHierarchy(parent, columnHierarchy, existsColumns, GetListItems(), columnsDic);
            columnHierarchy.Order();
            ColumnHierarchyCreated = true;
        }

        protected virtual void CreateHierarchy(ColumnHierarchy parent, List<ColumnHierarchy> columnHierarchy, Dictionary<string, ColumnHierarchy> existsColumns, IEnumerable<CrossColumnDataSourceItem> listItems, Dictionary<string, BaseColumn> columnsDic)
        {
            foreach (var item in listItems)
            {
                if (item.BaseColumn != null && !item.BaseColumn.Visible) continue;
                //if (!item.HideInHeader) continue;
                var newItem =
                    new ColumnHierarchy
                        {
                            HeaderTextRu = item.HeaderRu ?? item.Header,
                            HeaderTextKz = item.HeaderKz ?? item.Header,
                            ToolTipRu = item.ToolTipRu,
                            ToolTipKz = item.ToolTipKz,
                            HyperLinkOnClick = item.HyperLinkOnClick,
                            HyperLinkTarget = item.HyperLinkTarget,
                            HyperLinkUrl = item.HyperLinkUrl,
                            IsVerticalHeader = item.IsVerticalHeader,
                            CrossColumnKey = item.BaseColumn != null
                                                 ? item.BaseColumnName + "_" + item.BaseColumn.ColumnName + "_" + item.StringColumnId
                                                 : item.BaseColumnName + "_Group_" + item.StringColumnId,
                            ColumnName = item.BaseColumn != null
                                             ? item.BaseColumnName + "_" + item.BaseColumn.ColumnName + "_" + item.StringColumnId
                                             : null,
                            ColSpan = item.ColSpan,
                            ClientColSpan = item.ColSpan,
                            AggregateType = item.BaseColumn != null
                                                ? item.BaseColumn.AggregateType
                                                : ColumnAggregateType.None,
                            BaseColumn = item.BaseColumn,
                            CrossColumnIDObject = item.GetColumnID(),
                            HideInHeader = item.HideInHeader,
                            CrossColumnHeaderRow = item.Row,
                            Parent = parent,
                        };
                newItem.CrossColumnID = newItem.CrossColumnIDObject == null
                                            ? null
                                            : newItem.CrossColumnIDObject.ToString();
                InitHierarchy(newItem, item, existsColumns, columnsDic);
                if (!newItem.Delete)
                {
                    if (item.ChildItems.FirstOrDefault() != null)
                    {
                        columnHierarchy.Add(newItem);
                        CreateHierarchy(newItem, newItem.Childs, existsColumns, item.ChildItems, columnsDic);
                    }
                    else if (newItem.BaseColumn != null && newItem.BaseColumn.IsCrossColumn)
                    {
                        newItem.BaseColumn.BaseCrossColumnDataSource.CreateHierarchy(
                            parent, columnHierarchy, existsColumns,
                            newItem.BaseColumn.BaseCrossColumnDataSource.GetListItems(), columnsDic);
                        newItem.BaseColumn.BaseCrossColumnDataSource.ColumnHierarchyCreated = true;
                    }
                    else
                        columnHierarchy.Add(newItem);
                    item.CreatedColumnKey = newItem.ColumnKey;
                    item.CreatedColumnName = newItem.ColumnName;
                }
            }
        }

        protected virtual void InitHierarchy(ColumnHierarchy newItem, CrossColumnDataSourceItem item, Dictionary<string, ColumnHierarchy> existsColumns, Dictionary<string, BaseColumn> columnsDic)
        {
            if (newItem.ColumnName != null && !newItem.Delete)
                columnsDic[newItem.ColumnName] = item.BaseColumn;
            if (existsColumns.ContainsKey(newItem.ColumnKey))
                newItem.Init(existsColumns[newItem.ColumnKey]);
            InitedHierarchy(newItem, item, existsColumns, columnsDic);
        }

        protected virtual void InitedHierarchy(ColumnHierarchy newItem, CrossColumnDataSourceItem item, Dictionary<string, ColumnHierarchy> existsColumns, Dictionary<string, BaseColumn> columnsDic)
        {
        }

        public abstract int GetRowsCount(RenderContext context);
        //public abstract int GetRowsCount(BaseJournalControl journal, object[] groupValues);
        public abstract bool ComputeAggregates(RenderContext context, ColumnAggregateType userAggregateType);

        public virtual void DetectIsCrossColumn(BaseJournalControl control, string crossColumnName)
        {
            foreach (var column in GetColumns())
                column.DetectIsCrossColumn(control, crossColumnName);
        }


        public virtual IEnumerable<string> GetCrossColumnNames()
        {
            if (!ColumnHierarchyCreated) return new string[0];
            return _crossColumnNames ?? (_crossColumnNames = GetCrossColumnNames(GetListItems()).Where(r => !string.IsNullOrEmpty(r)));
        }

        protected IEnumerable<string> GetCrossColumnNames(IEnumerable<CrossColumnDataSourceItem> items)
        {
            var childItems = items.SelectMany(r => r.ChildItems).ToList();
            if (childItems.Count == 0)
                return items.Select(r => r.CreatedColumnName);
            return items.Select(r => r.CreatedColumnName).Union(GetCrossColumnNames(childItems));
        }
    }

    public abstract class CrossColumnDataSource<TRow> : CrossColumnDataSource
        where TRow : BaseRow
    {
        internal static int InternalGetRowsCount(RenderContext context, IEnumerable<CrossColumnDataSourceItem> listItems)
        {
            var rowsCount = 1;
            foreach (var column in listItems)
            {
                if (column.BaseColumn != null)
                {
                    var item = column.BaseColumn.GetCrossItem(context);
                    context.CrossDataItemRow = item;
                    var count = column.BaseColumn.GetRowsCount(context);
                    //if (count > rowsCount) rowsCount = count;
                    return count;
                }

                if (column.ChildItems != null)
                {
                    var count = InternalGetRowsCount(context, column.ChildItems);
                    //if (count > rowsCount) rowsCount = count;
                    return count;
                }
            }

            return rowsCount;
        }
    }

    public abstract class CrossColumnDataSource<THeaderKey, TRow, THeaderTable> : CrossColumnDataSource<TRow>
        where TRow : BaseRow
        where THeaderTable : class, ICrossTable<THeaderKey>
    {
        private readonly List<BaseJournalRow<TRow>> _rows = new List<BaseJournalRow<TRow>>();
        protected abstract IQueryable<THeaderTable> GetData();
        protected abstract IQueryable<THeaderTable> GetData(string key);
        protected abstract string GetColumnHeader(THeaderTable row);

        public Action<CrossColumnDataSourceItem<THeaderKey, TRow, THeaderTable>, THeaderTable> InitializeItemHandler;

        protected virtual string GetColumnHeaderRu(THeaderTable row)
        {
            return null;
        }
        
        protected virtual string GetColumnHeaderKz(THeaderTable row)
        {
            return null;
        }

        protected virtual IQueryable<THeaderTable> FilterData(IQueryable<THeaderTable> data, DataContext db)
        {
            Filter.SetDB(db);
            return (IQueryable<THeaderTable>)Filter.FilterHeaderData(data, this);
        }

        private List<BaseColumn> _columns;
        public List<BaseColumn> Columns
        {
            get
            {
                if (_columns == null)
                {
                    _columns = new List<BaseColumn>();
                    InitializeColumns();
                }
                return _columns;
            }
        }

        private List<CrossColumnDataSourceItem<THeaderKey, TRow, THeaderTable>> _listItems;
        protected List<CrossColumnDataSourceItem<THeaderKey, TRow, THeaderTable>> ListItems
        {
            get
            {
                if (_listItems == null)
                    InitializeListItems();
                return _listItems;
            }
            set { _listItems = value; }
        }

        public override IEnumerable<BaseColumn> GetColumns()
        {
            return Columns.Cast<BaseColumn>();
        }

        public override IEnumerable<CrossColumnDataSourceItem> GetListItems()
        {
            return ListItems.Cast<CrossColumnDataSourceItem>();
        }

        protected Dictionary<string, CrossColumnDataSourceItem<THeaderKey, TRow, THeaderTable>> DicItems { get; set; }

        protected internal override int RowIndex
        {
            set
            {
                if (base.RowIndex != value)
                {
                    base.RowIndex = value;
                    foreach (var column in Columns)
                        column.RowIndex = value;
                }
            }
        }

        public override Type HeaderType
        {
            get { return typeof(THeaderTable); }
        }

        protected virtual void InitializeListItems()
        {
            ListItemsInitialized = true;

            ListItems = new List<CrossColumnDataSourceItem<THeaderKey, TRow, THeaderTable>>();
            DicItems = new Dictionary<string, CrossColumnDataSourceItem<THeaderKey, TRow, THeaderTable>>();

           
            foreach (var row in GetData())
                AddItems(row);
        }

        protected void EnsureListItemsInitialized ()
        {
            if (!ListItemsInitialized)
                InitializeListItems();
        }

        protected bool ListItemsInitialized { get; set; }

        public virtual List<CrossColumnDataSourceItem<THeaderKey, TRow, THeaderTable>> GetColumnsItems()
        {
            EnsureListItemsInitialized();
            return ListItems;
        }

        protected virtual void InitializeColumns()
        {
        }

        protected virtual void AddItems(THeaderTable row)
        {
            var nullable = row as ICrossTableNullabel;
            var item = new CrossColumnDataSourceItem<THeaderKey, TRow, THeaderTable>
                           {
                               Header = GetColumnHeader(row),
                               HeaderRu = GetColumnHeaderRu(row),
                               HeaderKz = GetColumnHeaderKz(row),
                               Childs = new List<CrossColumnDataSourceItem<THeaderKey, TRow, THeaderTable>>(),
                               ColumnId = row.Id,
                               IdIsNull = nullable != null && nullable.IdIsNull,
                               Row = row,
                               BaseColumnName = BaseColumnName,
                           };
            InitializeItem(item, row);
            if (!item.Visible) return;

            ListItems.Add(item);

            var key = row.Id == null ? string.Empty : row.Id.ToString();
            if (DicItems.ContainsKey(key))
                throw new Exception("Detect non unique key in cross header datasource");

            DicItems[key] = item;
            AddColumns(row, item);
        }

        protected virtual void AddColumns(THeaderTable row, CrossColumnDataSourceItem<THeaderKey, TRow, THeaderTable> item)
        {
            var nullable = row as ICrossTableNullabel;
            var renderContext = new RenderContext {Journal = HeaderControl.Journal, };
            renderContext.OtherRows.Add(row);
            foreach (var column in Columns)
            {
                renderContext.Column = column;
                if (!column.GetVisible(renderContext)) continue;

                var columnItem = new CrossColumnDataSourceItem<THeaderKey, TRow, THeaderTable>
                                     {
                                         Column = column,
                                         Header = column.Header,
                                         HeaderKz = column.HeaderKz,
                                         HeaderRu = column.HeaderRu,
                                         ColumnId = row.Id,
                                         IdIsNull = nullable != null && nullable.IdIsNull,
                                         HideInHeader = column.HideInHeader,
                                         IsVerticalHeader = column.IsVerticalHeader,
                                         Row = row,
                                         BaseColumnName = BaseColumnName,
                                     };
                InitializeItem(columnItem, row);
                if (columnItem.Visible)
                {
                    item.Childs.Add(columnItem);
                    DicItems[column.ColumnName + "_" + row.Id] = columnItem;
                }
            }
        }

        protected virtual void InitializeItem(CrossColumnDataSourceItem<THeaderKey, TRow, THeaderTable> columnItem, THeaderTable headerRow)
        {
            InitializeItemHandler?.Invoke(columnItem, headerRow);
        }

        public override void RenderColumns(HtmlTextWriter writer, int level, int rowSpan)
        {
            RenderColumns(writer, level, ListItems, rowSpan);
        }

        protected virtual void RenderColumns(HtmlTextWriter writer, int level, List<CrossColumnDataSourceItem<THeaderKey, TRow, THeaderTable>> columns, int rowSpan)
        {
            if (columns.Count == 0) return;

            var maxCountData = 1;
            var maxLevel = columns.Max(r => r.Level);
            foreach (var column in columns)
            {
                var currentLevel = level - maxLevel + column.Level;
                if (column.Level > currentLevel && currentLevel > 0)
                    RenderColumns(writer, currentLevel, column.Childs, rowSpan + maxLevel - column.Level);
                else if (column.Level == currentLevel && !column.HideInHeader)
                {
                    if (column.CountColumnValues > maxCountData)
                        maxCountData = column.CountColumnValues;

                    writer.WriteLine();
                    //todo: подумать о том чтобы разворачивать текст заголовка по вертикали
                    writer.AddAttribute(HtmlTextWriterAttribute.Colspan, column.ColSpan.ToString());
                    if (column.Level == 1)
                        writer.AddAttribute(HtmlTextWriterAttribute.Rowspan,
                                            (rowSpan + maxLevel - column.Level).ToString());
                    writer.RenderBeginTag(HtmlTextWriterTag.Th);
                    RenderHeaderContent(column, writer);
                    writer.RenderEndTag();
                }
            }
            for (int i = 1; i < maxCountData; i++)
            {
                foreach (var column in columns)
                {
                    var currentLevel = level - maxLevel + column.Level;
                    if (column.Level == currentLevel && !column.HideInHeader)
                    {
                        writer.WriteLine();
                        writer.AddAttribute(HtmlTextWriterAttribute.Colspan, column.ColSpan.ToString());
                        if (column.Level == 1)
                            writer.AddAttribute(HtmlTextWriterAttribute.Rowspan,
                                                (rowSpan + maxLevel - column.Level).ToString());
                        writer.RenderBeginTag(HtmlTextWriterTag.Th);
                        writer.Write(column.Header);
                        writer.RenderEndTag();
                    }
                }
            }
        }

        protected virtual void RenderHeaderContent(CrossColumnDataSourceItem<THeaderKey, TRow, THeaderTable> column, HtmlTextWriter writer)
        {
            writer.Write(column.Header);
        }

        private int? _maxLevel;
        public override int MaxLevel
        {
            get
            {
                if (_maxLevel == null)
                {
                    if (ListItems == null || ListItems.Count == 0)
                        _maxLevel = 0;
                    else
                        _maxLevel = ListItems.Max(r => r.Level);
                }
                return _maxLevel.Value;
            }
        }

        public override void RenderBackUrls(HtmlTextWriter writer)
        {
        }

        public override IList GetKeys()
        {
            var keyList = new List<THeaderKey>();
            GetKeys(keyList, ListItems);
            return keyList;
        }

        protected virtual void GetKeys(List<THeaderKey> keyList, List<CrossColumnDataSourceItem<THeaderKey, TRow, THeaderTable>> items)
        {
            foreach (var list in items)
            {
                if (list.ColumnIdSeted && !keyList.Contains(list.ColumnId))
                    keyList.Add(list.ColumnId);
                if (list.Childs != null)
                    GetKeys(keyList, list.Childs);
            }
        }

        public override int GetFullColSpan()
        {
            return ListItems.Sum(r => r.ColSpan);
        }

        public override int GetRowsCount(RenderContext context)
        {
            return InternalGetRowsCount(context, ListItems.Cast<CrossColumnDataSourceItem>());
        }
/*
        public override int GetRowsCount(BaseJournalControl journal, object[] groupValues)
        {
            var rowsCount = 1;
            foreach (var column in Columns)
            {
                var count = column.GetRowsCount(null, null, journal, null, groupValues);
                if (count > rowsCount)
                    rowsCount = count;
            }
            return rowsCount;
        }*/

        public override bool ComputeAggregates(RenderContext context, ColumnAggregateType userAggregateType)
        {
            return InternalComputeAggregates(context, ListItems, userAggregateType);
        }

        private static bool InternalComputeAggregates(RenderContext context, IEnumerable<CrossColumnDataSourceItem<THeaderKey, TRow, THeaderTable>> listItems, ColumnAggregateType userAggregateType)
        {
            foreach (var column in listItems)
            {
                if (column.Column != null)
                {
                    //var item = column.Column.GetItem(context.JournalRow, column.ColumnId, 0);
                    if (column.Column == context.Column)
                        return false;
                    column.Column.ComputeAggregates(context, userAggregateType);
                }
                if (column.Childs != null)
                    InternalComputeAggregates(context, column.Childs, userAggregateType);
            }
            return true;
        }
    }
}