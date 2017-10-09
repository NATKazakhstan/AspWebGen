namespace Nat.Web.Controls.GenerationClasses.BaseJournal.ColumnInitializers
{
    using System;
    using System.Collections.Generic;
    using System.Data.Linq;
    using System.Linq;
    using System.Linq.Expressions;

    using Nat.Web.Tools;

    public static class GroupInHeaderCustomDataInitializer
    {
        public static List<KeyValuePair<object, string>> GetData<TTable>(
            DataContext db,
            Expression<Func<TTable, bool>> filter,
            Expression<Func<TTable, int>> orderBy,
            Expression<Func<TTable, KeyValuePair<object, string>>> selectorRu,
            Expression<Func<TTable, KeyValuePair<object, string>>> selectorKz) where TTable : class
        {
            return db.GetTable<TTable>()
                .Where(filter)
                .OrderBy(orderBy)
                .Select(LocalizationHelper.IsCultureKZ ? selectorKz : selectorRu)
                .ToList();
        }

        public static void InitializeCustomInHeaderData<TRow, TCrossTableKey, TCrossTable, TAggregateRow, TValueKey, TDataContext>(
           this GroupedAggregateColumn<TRow, TCrossTableKey, TCrossTable, TAggregateRow, TValueKey, TDataContext> column,
           List<KeyValuePair<object, string>> data)
            where TRow : BaseRow
            where TCrossTableKey : struct
            where TCrossTable : class
            where TAggregateRow : class, new()
            where TDataContext : DataContext
        {
            new GroupInHeaderCustomDataInitializer<TRow, TCrossTableKey, TCrossTable, TAggregateRow, TValueKey, TDataContext, long>(column, data).Initialize();
        }
    }

    public static class GroupInHeaderCustomDataInitializer<TKey>
    {
        public static void InitializeCustomInHeaderData<TRow, TCrossTableKey, TCrossTable, TAggregateRow, TValueKey, TDataContext>(
            GroupedAggregateColumn<TRow, TCrossTableKey, TCrossTable, TAggregateRow, TValueKey, TDataContext> column,
            List<KeyValuePair<object, string>> data)
            where TRow : BaseRow
            where TCrossTableKey : struct
            where TCrossTable : class
            where TAggregateRow : class, new()
            where TDataContext : DataContext
        {
            new GroupInHeaderCustomDataInitializer<TRow, TCrossTableKey, TCrossTable, TAggregateRow, TValueKey, TDataContext, TKey>(column, data).Initialize();
        }
    }

    public class GroupInHeaderCustomDataInitializer<TRow, TCrossTableKey, TCrossTable, TAggregateRow, TValueKey, TDataContext, TKey>
        where TRow : BaseRow
        where TCrossTableKey : struct
        where TCrossTable : class
        where TAggregateRow : class, new()
        where TDataContext : DataContext
    {
        private readonly GroupedAggregateColumn<TRow, TCrossTableKey, TCrossTable, TAggregateRow, TValueKey, TDataContext> column;

        private readonly List<KeyValuePair<object, string>> data;

        public GroupInHeaderCustomDataInitializer(GroupedAggregateColumn<TRow, TCrossTableKey, TCrossTable, TAggregateRow, TValueKey, TDataContext> column, List<KeyValuePair<object, string>> data)
        {
            this.column = column;
            this.data = data;
        }

        public void Initialize()
        {
            column.CreateDataSourceHandler = this.CreateDataSource;
        }

        private GroupedAggregateColumn<TRow, TCrossTableKey, TCrossTable, TAggregateRow, TValueKey, TDataContext>.GroupedAggregateColumnDS<TKey> CreateDataSource(
            GroupedAggregateColumn<TRow, TCrossTableKey, TCrossTable, TAggregateRow, TValueKey, TDataContext> groupedAggregateColumn)
        {
            return new DataSource(this.data, groupedAggregateColumn);
        }

        private class DataSource :
            GroupedAggregateColumn<TRow, TCrossTableKey, TCrossTable, TAggregateRow, TValueKey, TDataContext>.
                GroupedAggregateColumnDS<TKey>
        {
            private readonly List<KeyValuePair<object, string>> data;

            public DataSource(List<KeyValuePair<object, string>> data, GroupedAggregateColumn<TRow, TCrossTableKey, TCrossTable, TAggregateRow, TValueKey, TDataContext> baseColumn)
                : base(baseColumn)
            {
                this.data = data;
            }

            protected override IQueryable<GroupedAggregateColumn<TRow, TCrossTableKey, TCrossTable, TAggregateRow, TValueKey, TDataContext>.DataItem<TKey>> GetData()
            {
                return data.AsQueryable()
                    .Select(r => new GroupedAggregateColumn<TRow, TCrossTableKey, TCrossTable, TAggregateRow, TValueKey, TDataContext>.DataItem<TKey>(r.Key, r.Value));
            }
        }
    }
}