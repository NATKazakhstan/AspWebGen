using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data.Linq;
using System.Linq;
using System.Linq.Expressions;
using System.Web;
using Nat.Tools.Data;
using Nat.Web.Controls.Trace;

namespace Nat.Web.Controls.GenerationClasses.BaseJournal
{
    using System.Collections;

    public abstract class BaseJournalCrossTable
    {
        internal protected Dictionary<string, string> InlineGroupingColumns { get; set; }
        public Dictionary<string, string> ColumnAggregates { get; protected set; }
        public bool LoadDataIndependentForVisibilityOfColumns { get; set; }
        public virtual bool CanReturnInfo { get { return false; } }
        public abstract Type CrossTableType { get; }
        public BaseFilter Filter { get; set; }
        public abstract object GetDataItem(RenderContext context);
        public abstract object GetGroupDataItem(RenderContext context);
        public abstract int GetGroupRowsCount(RenderContext context);
        public virtual int GetRowsCount(RenderContext context)
        {
            throw new NotSupportedException();
        }
        public abstract void LoadData(RenderContext context);
    }

    public abstract class BaseJournalCrossTable<TRow> : BaseJournalCrossTable
        where TRow : BaseRow
    {
        private Dictionary<TRow, bool> loadedData = new Dictionary<TRow, bool>();

        protected BaseJournalCrossTable()
        {
            GroupDataLists = new Dictionary<string, IList>();
        }

        public Dictionary<string, IList> GroupDataLists { get; private set; }

        public TRow CurrentRow { get; protected set; }

        public BaseJournalControl<TRow> Journal { get; set; }

        protected virtual bool CacheGroupValuesForRow
        {
            get { return false; }
        }

        public virtual void GetValues(TRow row)
        {
            CurrentRow = row;
        }

        public override void LoadData(RenderContext context)
        {
            var row = context.GetDataRow<TRow>();
            LoadData(row);
        }

        internal void LoadData(TRow row)
        {
            if (row != null && !loadedData.ContainsKey(row))
            {
                GetValues(row);
                loadedData[row] = true;
            }
        }

        public virtual Expression GetSelectExpressionInner()
        {
            return null;
        }

        public virtual Expression<Func<TDataContext, TRow, IQueryable<TResult>>> GetDataExpression<TDataContext, TResult>()
            where TDataContext : DataContext
        {
            return (Expression<Func<TDataContext, TRow, IQueryable<TResult>>>)GetSelectExpressionInner();
        }

        public virtual IQueryable<TResult> GetDataExpression<TDataContext, TResult>(TRow row, TDataContext dataContext)
           where TDataContext : DataContext
        {
            var dataExp = GetDataExpression<TDataContext, TResult>();
            return dataExp.Compile()(dataContext, row);
        }

        public virtual IQueryable<TResult> GetQueryOfData<TDataContext, TResult>(TRow row, TDataContext dataContext)
          where TDataContext : DataContext
        {
            var provider = Journal.DataContext.GetTable(CrossTableType).Provider;
            var dataExp = GetDataExpression<TDataContext, TResult>();
            var rowExp = Expression.Constant(row);
            var dbExp = Expression.Constant(dataContext);
            var query = Expression.Invoke(dataExp, dbExp, rowExp);
            return provider.CreateQuery<TResult>(query);
        }

        protected virtual Expression FilterData(Expression query, DataContext db)
        {
            Filter.SetDB(db);
            return Filter.FilterCrossData(query, this);
        }

        public override object GetDataItem(RenderContext context)
        {
            throw new NotImplementedException();
        }

        public override int GetGroupRowsCount(RenderContext context)
        {
            throw new NotImplementedException();
        }

        public override object GetGroupDataItem(RenderContext context)
        {
            throw new NotImplementedException();
        }

        public List<TAggregateRow> GetAllGroupData<TDataContext, TAggregateRow>(string groupColumnName)
            where TAggregateRow : class, new()
            where TDataContext : DataContext
        {
            return GetAllGroupData<TDataContext, TAggregateRow>(null, groupColumnName);
        }

        public virtual List<TAggregateRow> GetAllGroupData<TDataContext, TAggregateRow>(RenderContext context, string groupColumnName)
            where TAggregateRow : class, new()
            where TDataContext : DataContext
        {
            var cacheKey = groupColumnName;
            if (CacheGroupValuesForRow && context?.DataRow != null) 
                cacheKey += ":" + context.DataRow.Value;

            if (GroupDataLists.ContainsKey(cacheKey))
                return (List<TAggregateRow>)GroupDataLists[cacheKey];

#if DEBUG
            HttpContext.Current.Trace.WriteExt("LoadData", "BaseJournalCrossTable.GetAllGroupData.Begin");
#endif
            var db = (TDataContext)Journal.DataContext;
            IQueryable<TAggregateRow> data;
            if (CacheGroupValuesForRow && context?.DataRow != null)
                data = GetQueryOfData<TDataContext, TAggregateRow>(CurrentRow, db);
            else
            {
                var dataExp = GetDataExpression<TDataContext, TAggregateRow>();
                var param = Expression.Parameter(typeof(TRow), "row");
                var dbExp = Expression.Constant(db);
                var exp = Expression.Invoke(dataExp, dbExp, param);
                var selectExp = Expression.Lambda<Func<TRow, IEnumerable<TAggregateRow>>>(exp, param);
                var rows = Journal.ParentUserControl.GetSelect().Cast<TRow>();
                rows = GetAllGroupDataFilter(rows);
                data = rows.SelectMany(selectExp);
            }

            try
            {
                var source = data.Aggregate(db, ColumnAggregates, groupColumnName).AsQueryable();
                var groupData = OrderBy(source, groupColumnName.Split(',').ToArray()).ToList();
                GroupDataLists[cacheKey] = groupData;
                return groupData;
            }
            finally
            {
#if DEBUG
                HttpContext.Current.Trace.WriteExt("LoadData", "BaseJournalCrossTable.GetAllGroupData.End");                
#endif
            }
        }

        protected virtual IQueryable<TRow> GetAllGroupDataFilter(IQueryable<TRow> rows)
        {
            return rows;
        }

        protected virtual IEnumerable<T> OrderBy<T>(IQueryable<T> source, params string[] orderColumns)
        {
            var query = OrderBy<T>(source.Expression, orderColumns);
            return source.Provider.CreateQuery<T>(query);
        }

        protected virtual Expression OrderBy<T>(Expression query, params string[] orderColumns)
        {
            string str = "OrderBy";
            foreach (var columnName in orderColumns)
            {
                ParameterExpression param = Expression.Parameter(typeof(T), "c");
                var property = Expression.Property(param, columnName);
                query = Expression.Call(
                    typeof(Queryable),
                    str,
                    new[] { typeof(T), property.Type },
                    query,
                    Expression.Lambda(property, param));
                str = "ThenBy";
            }

            return query;
        }

        protected virtual Expression OrderBy<T>(Expression query, IEnumerable<LambdaExpression> orderColumns)
        {
            string str = "OrderBy";
            foreach (var expression in orderColumns)
            {
                query = Expression.Call(
                    typeof(Queryable),
                    str,
                    new[] { typeof(T), expression.Type },
                    query,
                    expression);
                str = "ThenBy";
            }

            return query;
        }
    }

    public abstract class BaseJournalCrossTableManyData<TRow, TCrossTable, TResultData> : BaseJournalCrossTable<TRow>
        where TRow : BaseRow
        where TCrossTable : class
        where TResultData : class, new()
    {
        protected BaseJournalCrossTableManyData()
        {
            CurrentValues = new Dictionary<TRow, List<TResultData>>();
            LoadDataIndependentForVisibilityOfColumns = true;
        }

        public Dictionary<TRow, List<TResultData>> CurrentValues { get; protected set; }

        protected virtual IQueryable<TCrossTable> FilterData(IQueryable<TCrossTable> data, DataContext db)
        {
            Filter.SetDB(db);
            return (IQueryable<TCrossTable>)Filter.FilterCrossData(data, this);
        }

        public override Type CrossTableType
        {
            get { return typeof(TCrossTable); }
        }

        public override object GetDataItem(RenderContext context)
        {
            var row = context.GetDataRow<TRow>();
            if (CurrentValues.ContainsKey(row))
            {
                var rows = CurrentValues[row];
                if (rows.Count > context.RowIndex)
                    return rows[context.RowIndex];
            }
            return null;
        }
    }

    public abstract class BaseJournalCrossTable<TRow, TCrossTableKey, TCrossTable> : BaseJournalCrossTable<TRow>
        where TRow : BaseRow
        where TCrossTableKey : struct 
        where TCrossTable : class
    {
        public ILookup<TCrossTableKey, TCrossTable> CurrentValues { get; protected set; }

        protected virtual IQueryable<TCrossTable> FilterData(IQueryable<TCrossTable> data, DataContext db)
        {
            Filter.SetDB(db);
            return (IQueryable<TCrossTable>)Filter.FilterCrossData(data, this);
        }

        public override Type CrossTableType
        {
            get { return typeof(TCrossTable); }
        }

        public override object GetDataItem(RenderContext context)
        {
            if (context.CrossColumnIdObject == null || context.GroupValues != null) return null;
            var key = (TCrossTableKey) context.CrossColumnIdObject;
            if (!CurrentValues.Contains(key))
                return null;
            var values = CurrentValues[key];
            return values.Skip(context.RowIndex).FirstOrDefault();
        }
    }

    public abstract class BaseJournalCrossTable<TRow, TCrossTableKey, TCrossTable, TAggregateRow> : BaseJournalCrossTable<TRow>
        where TRow : BaseRow
        where TCrossTable : class
        where TAggregateRow : class, new()
    {
        protected BaseJournalCrossTable()
        {
            ColumnAggregates = new Dictionary<string, string>();
            InlineGroupingColumns = new Dictionary<string, string>();
            RowValues = new Dictionary<TRow, TAggregateRow>();
            RowGroupedValues = new Dictionary<TRow, List<TAggregateRow>>();
            CurrentValuesRows = new Dictionary<TRow, List<TCrossTableKey>>();
            CurrentValues = new Dictionary<TRow, Dictionary<TCrossTableKey, TAggregateRow>>();
            LoadDataIndependentForVisibilityOfColumns = true;
        }

        public Dictionary<TRow, Dictionary<TCrossTableKey, TAggregateRow>> CurrentValues { get; protected set; }
        public Dictionary<TRow, List<TCrossTableKey>> CurrentValuesRows { get; protected set; }
        public Dictionary<TRow, TAggregateRow> RowValues { get; protected set; }
        public Dictionary<TRow, List<TAggregateRow>> RowGroupedValues { get; protected set; }

        protected virtual IQueryable<TCrossTable> FilterData(IQueryable<TCrossTable> data, DataContext db)
        {
            Filter.SetDB(db);
            return (IQueryable<TCrossTable>)Filter.FilterCrossData(data, this);
        }

        protected virtual IEnumerable<TAggregateRow> CustomSortAggregateGroups(IQueryable<TAggregateRow> source)
        {
            var inlineGroupColumns = Journal.GetInlineGroupColumns();

            string str = "OrderBy";
            var query = source.Expression;
            foreach (var columnName in inlineGroupColumns)
            {
                ParameterExpression param = Expression.Parameter(typeof(TAggregateRow), "c");
                var property = Expression.Property(param, columnName.ColumnName);
                query = Expression.Call(typeof(Queryable),
                                        str,
                                        new[] { typeof(TAggregateRow), property.Type },
                                        query,
                                        Expression.Lambda(property, param));
                str = "ThenBy";
            }
            return source.Provider.CreateQuery<TAggregateRow>(query);
        }

        public override Type CrossTableType
        {
            get { return typeof(TCrossTable); }
        }

        protected virtual string DefaultGroupBy
        {
            get { return null;}
        }

        public virtual string GetGroupBy()
        {
            var inlineGroupColumns = Journal.GetInlineGroupColumns();
            string inlineGroups = null;
            foreach (var groupColumn in inlineGroupColumns)
            {
                if (InlineGroupingColumns.ContainsKey(groupColumn.ColumnName))
                    inlineGroups += inlineGroups == null ? groupColumn.ColumnName : "," + groupColumn.ColumnName;
            }
            if (string.IsNullOrEmpty(DefaultGroupBy))
                return inlineGroups;
            if (string.IsNullOrEmpty(inlineGroups))
                return DefaultGroupBy;
            return DefaultGroupBy + "," + inlineGroups;
        }

        public override object GetDataItem(RenderContext context)
        {
            /*var row = context.GetDataRow<TRow>();
            if (CurrentValues.ContainsKey(row))
            {
                var rows = CurrentValues[row];
                if (rows.Count > context.RowIndex)
                    return rows[context.RowIndex] as TResult;
            }*/
            return null;
        }
    }

    public abstract class BaseJournalCrossTable<TRow, TCrossTableKey, TCrossTable, TAggregateRow, TDataContext> : BaseJournalCrossTable<TRow>
        where TRow : BaseRow
        where TCrossTable : class
        where TAggregateRow : class, new()
        where TDataContext : DataContext
    {
        protected Dictionary<string, bool> FilledData { get; } = new Dictionary<string, bool>();

        protected BaseJournalCrossTable()
        {
            ColumnAggregates = new Dictionary<string, string>();
            InlineGroupingColumns = new Dictionary<string, string>();
            Data = new Dictionary<Key, List<TAggregateRow>>(new Comparer());
            AllGroupData = new Dictionary<string, List<TAggregateRow>>();
        }

        public override bool CanReturnInfo { get { return true; } }

        public Dictionary<Key, List<TAggregateRow>> Data { get; private set; }

        public Dictionary<string, List<TAggregateRow>> AllGroupData { get; private set; }

        protected virtual IQueryable<TCrossTable> FilterData(IQueryable<TCrossTable> data, DataContext db)
        {
            Filter.SetDB(db);
            return (IQueryable<TCrossTable>)Filter.FilterCrossData(data, this);
        }

        protected virtual IEnumerable<TAggregateRow> CustomSortAggregateGroups(IQueryable<TAggregateRow> source)
        {
            var inlineGroupColumns = Journal.GetInlineGroupColumns();

            string str = "OrderBy";
            var query = source.Expression;
            foreach (var columnName in inlineGroupColumns)
            {
                if (!InlineGroupingColumns.ContainsKey(columnName.ColumnName))
                    continue;
                ParameterExpression param = Expression.Parameter(typeof(TAggregateRow), "c");
                var property = Expression.Property(param, columnName.ColumnName);
                query = Expression.Call(typeof(Queryable),
                                        str,
                                        new[] { typeof(TAggregateRow), property.Type },
                                        query,
                                        Expression.Lambda(property, param));
                str = "ThenBy";
            }
            return source.Provider.CreateQuery<TAggregateRow>(query);
        }

        protected virtual void SetData(TRow row, IEnumerable<TAggregateRow> data)
        {
            string[] groupColumns;
            var groupBy = GetGroupBy();
            groupColumns = string.IsNullOrEmpty(groupBy)
                               ? new string[0]
                               : groupBy.Split(new[] {','}, StringSplitOptions.RemoveEmptyEntries).Select(r => r.Trim()).ToArray();
            var defaultGroupColumns = string.IsNullOrEmpty(DefaultGroupBy)
                                          ? new string[0]
                                          : DefaultGroupBy.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries).Select(r => r.Trim()).ToArray();
            foreach (var item in data.GroupBy(r => GetKey(row, r, groupColumns, defaultGroupColumns)))
            {
                /*if (Data.ContainsKey(item.Key))
                    Data[item.Key].AddRange(item);
                else*/
                    Data.Add(item.Key, item.ToList());
            }
        }
        
        private Key GetKey(TRow row, TAggregateRow r, IEnumerable<string> groupColumns, string[] defaultGroupColumns)
        {
            var key = new Key {Row = row};
            var columnKey = new BaseColumn.GroupKeys();
            var aggRowTypes = TypeDescriptor.GetProperties(r);
            foreach (var groupColumn in groupColumns)
            {
                var aggRowProperty = aggRowTypes.Find(groupColumn, true);
                var value = aggRowProperty.GetValue(r);
                var strValue = value == null ? "" : value.ToString();
                if (defaultGroupColumns.Contains(groupColumn))
                {
                    if (string.IsNullOrEmpty(key.CrossTableKey))
                        key.CrossTableKey = strValue;
                    else
                        key.CrossTableKey += "," + strValue;
                }
                else if ((Journal.InnerHeader.ColumnsDic[groupColumn].GroupType & GroupType.InHeader) != 0
                         || (Journal.InnerHeader.ColumnsDic[groupColumn].InlineGrouping))
                    columnKey.AddValue(groupColumn, value, Journal.InnerHeader.ColumnsDic[groupColumn].InlineGrouping);
            }
            key.CrossDataItemKey = columnKey;
            return key;
        }

        public override Type CrossTableType
        {
            get { return typeof(TCrossTable); }
        }

        protected virtual string DefaultGroupBy
        {
            get { return null; }
        }

        public virtual string GetGroupBy()
        {
            var inlineGroupColumns = Journal.GetInlineGroupColumns();
            string inlineGroups = null;
            foreach (var groupColumn in inlineGroupColumns)
            {
                if (InlineGroupingColumns.ContainsKey(groupColumn.ColumnName))
                    inlineGroups += inlineGroups == null ? groupColumn.ColumnName : "," + groupColumn.ColumnName;
            }
            if (string.IsNullOrEmpty(DefaultGroupBy))
                return inlineGroups;
            if (string.IsNullOrEmpty(inlineGroups))
                return DefaultGroupBy;
            return DefaultGroupBy + "," + inlineGroups;
        }

        public override object GetDataItem(RenderContext context)
        {
            var key = new Key
                          {
                              Row = context.GetDataRow<TRow>(),
                              CrossTableKey = context.CrossColumnId,
                              CrossDataItemKey = context.CrossDataItemKey,
                          };
            if (Data.ContainsKey(key))
            {
                var rows = Data[key];
                if (rows.Count > 0)
                    return rows[0];
                //todo: для использования такого варианта, нужно перегенерить все кроссы
                /*if (context.RowIndex < rows.Count)
                    return rows[context.RowIndex];*/
            }

            return null;
        }

        public List<TAggregateRow> GetAllGroupData(string groupColumnName)
        {
            return GetAllGroupData(null, groupColumnName);
        }

        public List<TAggregateRow> GetAllGroupData(RenderContext context, string groupColumnName)
        {
            var cacheKey = groupColumnName;
            if (CacheGroupValuesForRow && context?.DataRow != null) 
                cacheKey += ":" + context.DataRow.Value;

            if (AllGroupData.ContainsKey(cacheKey))
                return AllGroupData[cacheKey];
            return AllGroupData[cacheKey] = GetAllGroupData<TDataContext, TAggregateRow>(context, groupColumnName);
        }

        protected virtual IEnumerable<TAggregateRow> OrderGroupData(IEnumerable<TAggregateRow> aggregate)
        {
            return aggregate;
        }

        public override object GetGroupDataItem(RenderContext context)
        {
            context.LoadData();
            var rows = GetAllGroupData(context, InlineGroupingColumns[context.Column.ColumnName]);
            if (rows.Count == 0) return null;
            var inlineGroups = GetInlineGroups().
                Where(r => Journal.BaseInnerHeader.ColumnsDic[r].UsingInGroup
                           && Journal.BaseInnerHeader.ColumnsDic[r].InlineGrouping).
                ToList();
            var rowIndex = context.RowIndex;

            if (inlineGroups.Count == 0)
            {
                if (rows.Count > rowIndex)
                    return rows[rowIndex];
                return null;
            }

            var innerCount = inlineGroups.
                SkipWhile(r => !r.Equals(context.Column.ColumnName)).
                Skip(1).
                Aggregate(0, (current, item) => current == 0
                                                    ? GetAllGroupData(context, item).Count
                                                    : current*GetAllGroupData(context, item).Count);
            /*var count = inlineGroups.
                SkipWhile(r => !r.Equals(context.Column.ColumnName)).
                Aggregate(0, (current, item) => current == 0
                                                    ? GetAllGroupData(item).Count
                                                    : current*GetAllGroupData(item).Count);*/
            var count = rows.Count;
            if (innerCount > 0) rowIndex /= innerCount;
            rowIndex = rowIndex%count;
            return rows[rowIndex];
        }

        private IEnumerable<string> GetInlineGroups()
        {
            var inlineGroupColumns = Journal.GetInlineGroupColumns();
            var inlineGroups = inlineGroupColumns.
                Select(r => r.ColumnName).
                Where(columnName => InlineGroupingColumns.ContainsKey(columnName));
            return inlineGroups;
        }

        public override int GetGroupRowsCount(RenderContext context)
        {
            context.LoadData();
            return GetInlineGroups().
                Where(r => Journal.BaseInnerHeader.ColumnsDic[r].GroupType != GroupType.InHeader).
                Select(r => GetAllGroupData(context, r)).
                Aggregate(0, (current, rows) => current == 0 ? rows.Count : current*rows.Count);
        }

        public override int GetRowsCount(RenderContext context)
        {
            context.LoadData();
            var count = 0;
            foreach (var columnName in InlineGroupingColumns)
            {
                var column = Journal.InnerHeader.ColumnsDic[columnName.Key];
                if (!column.InlineGrouping || !column.UsingInGroup || column.GroupType == GroupType.InHeader) continue;
                count = count == 0
                            ? GetAllGroupData(context, columnName.Value).Count
                            : count*GetAllGroupData(context, columnName.Value).Count;
            }
            return count;
        }

        public override void GetValues(TRow row)
        {
            if (FilledData.ContainsKey(row.Value)) return;
            var db = (TDataContext)Journal.DataContext;
            var data = GetQueryOfData<TDataContext, TAggregateRow>(row, db);
            var groupBy = GetGroupBy();
            if (string.IsNullOrEmpty(groupBy))
                SetData(row, new[] { data.Aggregate(db, ColumnAggregates) });
            else
            {
                var resultValues = data.Aggregate(db, ColumnAggregates, groupBy);
                SetData(row, CustomSortAggregateGroups(resultValues.AsQueryable()));
            }
            base.GetValues(row);
            FilledData[row.Value] = true;
        }

        public struct Key
        {
            public TRow Row;
            public string CrossTableKey;
            public BaseColumn.GroupKeys CrossDataItemKey;

            public override bool Equals(object obj)
            {
                var key = (Key)obj;
                var rowEquals = ((Row == null && key.Row == null) || (Row != null && key.Row != null && Row.Value == key.Row.Value));
                if (CrossDataItemKey != null && key.CrossDataItemKey != null)
                {
                    return rowEquals
                           && ((CrossTableKey == null && key.CrossTableKey == null)
                               || (CrossTableKey != null && CrossTableKey.Equals(key.CrossTableKey)))
                           && CrossDataItemKey.Equals(key.CrossDataItemKey);
                }

                return rowEquals
                       && ((CrossTableKey == null && key.CrossTableKey == null)
                           || (CrossTableKey != null && CrossTableKey.Equals(key.CrossTableKey)));
            }

            public override int GetHashCode()
            {
                unchecked
                {
                    return (Row == null || Row.Value == null ? 0 : Row.Value.GetHashCode())
                           + (CrossTableKey != null ? CrossTableKey.GetHashCode() : 0)
                           + (CrossDataItemKey != null ? CrossDataItemKey.GetHashCode() : 0);
                }
            }
        }

        private class Comparer : IEqualityComparer<Key>
        {
            public bool Equals(Key x, Key y)
            {
                var rowEquals = ((x.Row == null && y.Row == null) || (x.Row != null && y.Row != null && x.Row.Value == y.Row.Value));
                if (x.CrossDataItemKey != null && y.CrossDataItemKey != null)
                {
                    return rowEquals
                           && ((x.CrossTableKey == null && y.CrossTableKey == null)
                               || (x.CrossTableKey != null && x.CrossTableKey.Equals(y.CrossTableKey)))
                           && x.CrossDataItemKey.Equals(y.CrossDataItemKey);
                }

                return rowEquals
                       && ((x.CrossTableKey == null && y.CrossTableKey == null)
                           || (x.CrossTableKey != null && x.CrossTableKey.Equals(y.CrossTableKey)));
            }

            public int GetHashCode(Key obj)
            {
                unchecked
                {
                    return (obj.Row == null || obj.Row.Value == null ? 0 : obj.Row.Value.GetHashCode())
                           + (obj.CrossTableKey != null ? obj.CrossTableKey.GetHashCode() : 0)
                           + (obj.CrossDataItemKey != null ? obj.CrossDataItemKey.GetHashCode() : 0);
                }
            }
        }
    }
}
