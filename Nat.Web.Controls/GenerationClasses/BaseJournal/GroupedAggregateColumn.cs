namespace Nat.Web.Controls.GenerationClasses.BaseJournal
{
    using System;
    using System.Collections.Generic;
    using System.Data.Linq;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Reflection;

    using Nat.Tools.Data;
    using Nat.Web.Controls.Properties;

    public class GroupedAggregateColumn<TRow, TCrossTableKey, TCrossTable, TAggregateRow> : AggregateColumn<TRow, TCrossTableKey, TCrossTable, TAggregateRow>
        where TRow : BaseRow
        where TCrossTableKey : struct
        where TCrossTable : class
        where TAggregateRow : class, new()
    {
        private static readonly GroupType[] DefaultSupportGroupTypes =
            {
                GroupType.None, GroupType.Top,
                GroupType.InHeader,
            };

        private static readonly GroupType[] DefaultSupportGroupTypesWithTotals =
            {
                GroupType.None, GroupType.Top,
                GroupType.TopTotal,
                GroupType.InHeader,
            };

        public GroupedAggregateColumn()
        {
            AllowJoinRows = true;
        }

        public override bool Visible
        {
            get { return UsingInGroup && !IsCrossColumn && base.Visible; }
        }

        public override bool InlineGrouping
        {
            get { return !IsCrossColumn; }
        }

        protected override TAggregateRow GetAggregateRow(RenderContext context, TRow row)
        {
            if (CrossTable == null && context != null && row != null && !IsCrossColumn)
                return (TAggregateRow)BaseCrossTable.GetGroupDataItem(context);
            return base.GetAggregateRow(context, row);
        }

        public override int GetRowsCount(RenderContext context)
        {
            var row = context.GetDataRow<TRow>();
            if (CrossTable == null && row != null && !IsCrossColumn && context.GroupValues == null)
                return BaseCrossTable.GetGroupRowsCount(context);
            return base.GetRowsCount(context);
        }

        public override IEnumerable<GroupType> GetSupportGroupTypes(BaseJournalControl journal)
        {
            return SupportGroupTypes ?? (journal.ShowTotals ? DefaultSupportGroupTypesWithTotals : DefaultSupportGroupTypes);
        }

        public override string GetGroupTypeImage(GroupType groupType, bool selected)
        {
            if (selected)
            {
                switch (groupType)
                {
                    case GroupType.Top:
                        return Themes.IconUrlCrossJournalGroup6S;
                    case GroupType.TopTotal:
                        return Themes.IconUrlCrossJournalGroup7S;
                    case GroupType.InHeader:
                        return Themes.IconUrlCrossJournalGroup8S;
                }
            }
            else
            {
                switch (groupType)
                {
                    case GroupType.Top:
                        return Themes.IconUrlCrossJournalGroup6;
                    case GroupType.TopTotal:
                        return Themes.IconUrlCrossJournalGroup7;
                    case GroupType.InHeader:
                        return Themes.IconUrlCrossJournalGroup8;
                }
            }

            return base.GetGroupTypeImage(groupType, selected);
        }

        public override string GetGroupTypeTitle(GroupType groupType, bool selected)
        {
            if (selected)
            {
                switch (groupType)
                {
                    case GroupType.InHeader:
                        return Resources.SAddedGroupColumnInHeader;
                }
            }
            else
            {
                switch (groupType)
                {
                    case GroupType.InHeader:
                        return Resources.SAddGroupColumnInHeader;
                }
            }

            return base.GetGroupTypeTitle(groupType, selected);
        }
    }

    public class GroupedAggregateColumn<TRow, TCrossTableKey, TCrossTable, TAggregateRow, TValueKey, TDataContext> :
        GroupedAggregateColumn<TRow, TCrossTableKey, TCrossTable, TAggregateRow>
        where TRow : BaseRow
        where TCrossTableKey : struct
        where TCrossTable : class
        where TAggregateRow : class, new()
        where TDataContext : DataContext
    {
        public Func<GroupedAggregateColumn<TRow, TCrossTableKey, TCrossTable, TAggregateRow, TValueKey, TDataContext>, CrossColumnDataSource<TRow>> CreateDataSourceHandler { get; set; }

        public override bool IsCrossColumn
        {
            get { return (GroupType & GroupType.InHeader) != 0; }
        }

        public override int GetRowsCount(RenderContext context)
        {
            if (context.GroupValues == null && IsCrossColumn)
                return 1;
            return base.GetRowsCount(context);
        }

        public override CrossColumnDataSource BaseCrossColumnDataSource
        {
            get
            {
                EnsureBaseCrossColumnDataSource();
                return base.BaseCrossColumnDataSource;
            }
        }

        private void EnsureBaseCrossColumnDataSource()
        {
            if (!IsCrossColumn) return;
            var ds = this.CreateDataSource();
            ds.BaseColumnName = this.ColumnName;
            ds.HeaderControl = this.BaseCrossTable.Journal.BaseInnerHeader;
            ds.Filter = this.BaseCrossTable.Journal.Filter;
            BaseCrossColumnDataSource = ds;
        }

        protected virtual CrossColumnDataSource<TRow> CreateDataSource()
        {
            if (CreateDataSourceHandler != null) 
                return CreateDataSourceHandler(this);

            var lambda = this.ValueExpression as LambdaExpression;
            if (lambda == null) 
                return CreateGroupedAggregateColumnDS<TValueKey>();

            var type = LinqFilterGenerator.IsNullableType(lambda.Body.Type)
                           ? lambda.Body.Type.GetGenericArguments()[0]
                           : lambda.Body.Type;

            var methodInfo = this.GetType().GetMethod("CreateGroupedAggregateColumnDS", BindingFlags.NonPublic | BindingFlags.InvokeMethod | BindingFlags.Instance);
            var method = methodInfo.MakeGenericMethod(type);
            return (CrossColumnDataSource<TRow>)method.Invoke(this, null);
        }

        private GroupedAggregateColumnDS<TKey> CreateGroupedAggregateColumnDS<TKey>()
        {
            return new GroupedAggregateColumnDS<TKey>(this);
        }


        public class GroupedAggregateColumnDS<TKey> : CrossColumnDataSource<TKey, TRow, DataItem<TKey>>
        {
            public GroupedAggregateColumnDS(GroupedAggregateColumn<TRow, TCrossTableKey, TCrossTable, TAggregateRow, TValueKey, TDataContext> baseColumn)
            {
                this.BaseColumn = baseColumn;
            }

            public GroupedAggregateColumn<TRow, TCrossTableKey, TCrossTable, TAggregateRow, TValueKey, TDataContext> BaseColumn { get; private set; }

            protected override IQueryable<DataItem<TKey>> GetData()
            {
                var journal = BaseColumn.BaseCrossTable.Journal;
                /*var db = (TDataContext)journal.DataContext;
                var dataExp = BaseColumn.BaseCrossTable.GetDataExpression<TDataContext, TAggregateRow>();
                var param = Expression.Parameter(typeof(TRow), "row");
                var dbExp = Expression.Constant(db);
                var exp = Expression.Invoke(dataExp, dbExp, param);
                var selectExp = Expression.Lambda<Func<TRow, IEnumerable<TAggregateRow>>>(exp, param);
                var data = journal.ParentUserControl.GetSelect().Cast<TRow>().SelectMany(selectExp);
                var rows = data.Aggregate(db, BaseColumn.BaseCrossTable.ColumnAggregates, BaseColumnName);*/
                var rows =
                    BaseColumn.BaseCrossTable.GetAllGroupData<TDataContext, TAggregateRow>(
                        BaseColumn.BaseCrossTable.InlineGroupingColumns[BaseColumn.ColumnName]);
                var result = rows
                    .Select(r => new RenderContext { Column = BaseColumn, AggregateDataRow = r, Journal = journal, })
                    .Select(r => new DataItem<TKey>(r.Column.GetValue(r), r.Column.GetName(r)));
                return result.AsQueryable();
            }

            protected override IQueryable<DataItem<TKey>> GetData(string key)
            {
                throw new System.NotImplementedException();
            }

            protected override string GetColumnHeader(DataItem<TKey> row)
            {
                return row.Value;
            }

            protected override string GetColumnHeaderRu(DataItem<TKey> row)
            {
                return GetColumnHeader(row);
            }

            protected override string GetColumnHeaderKz(DataItem<TKey> row)
            {
                return GetColumnHeader(row);
            }

            protected override void InitializeColumns()
            {
                Columns.Add(
                    new BaseColumn
                        {
                            ColumnName = BaseColumnName + "CrossColumn",
                            GetValueHandler = context => BaseColumn.GetGroupValue(context),
                            GetNameHandler = context => BaseColumn.GetGroupName(context),
                            GetRowNumberHandler = context => BaseColumn.GetRowNumber(context),
                            //GetRowsCountHandler = context => BaseColumn.GetRowsCount(context),
                            AggregateGroupedByRowIndex = BaseColumn.AggregateGroupedByRowIndex,
                            AggregateFormat = BaseColumn.AggregateFormat,
                            Format = BaseColumn.Format,
                        });
                base.InitializeColumns();
            }
        }

        public class DataItem<TKey> : ICrossTable<TKey>, ICrossTableNullabel
        {
            private bool idIsNull;

            public DataItem(object key, string value)
            {
                idIsNull = key == null;
                if (!idIsNull)
                {
                    Key = (TKey)Convert.ChangeType(key, typeof(TKey));
                    Value = value;
                }
                else
                    Value = string.IsNullOrEmpty(value) ? Resources.SNotSpecified : value;
            }

            public TKey Key { get; set; }

            public string Value { get; set; }

            public TKey Id
            {
                get { return Key; }
            }

            public bool IdIsNull
            {
                get { return idIsNull; }
            }
        }
    }
}
