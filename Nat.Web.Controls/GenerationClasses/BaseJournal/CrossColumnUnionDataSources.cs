using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Web.UI;

namespace Nat.Web.Controls.GenerationClasses.BaseJournal
{
    public abstract class CrossColumnUnionDataSources<TRow> : CrossColumnDataSource<TRow>
        where TRow : BaseRow
    {
        private IEnumerable<string> _crossColumnNames;

        protected CrossColumnUnionDataSources()
        {
            DataSources = new List<CrossColumnDataSource>(2);
        }

        public List<CrossColumnDataSource> DataSources { get; private set; }
        protected bool ListItemsInitialized { get; set; }

        private List<CrossColumnDataSourceItem> _listItems;
        protected List<CrossColumnDataSourceItem> ListItems
        {
            get
            {
                if (_listItems == null)
                    InitializeListItems();
                return _listItems;
            }
            set { _listItems = value; }
        }

        protected virtual void InitializeListItems()
        {
            ListItemsInitialized = true;
            ListItems = new List<CrossColumnDataSourceItem>();
            FillListItems();
        }

        protected virtual void FillListItems()
        {
            foreach (var dataSource in DataSources)
                ListItems.AddRange(dataSource.GetListItems());
            OrderListItems(ListItems);
        }

        protected virtual void OrderListItems(List<CrossColumnDataSourceItem> listItems)
        {
        }

        public override Type HeaderType
        {
            get { throw new NotImplementedException(); }
        }

        public override IList GetKeys()
        {
            throw new NotImplementedException();
        }

        public override IEnumerable<CrossColumnDataSourceItem> GetListItems()
        {
            return ListItems;
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

        public override void RenderColumns(HtmlTextWriter writer, int level, int rowSpan)
        {
            throw new NotImplementedException();
        }

        public override void RenderBackUrls(HtmlTextWriter writer)
        {
            foreach (var dataSource in DataSources)
                dataSource.RenderBackUrls(writer);
        }

        public override int GetFullColSpan()
        {
            return ListItems.Sum(r => r.ColSpan);
        }

        public override IEnumerable<BaseColumn> GetColumns()
        {
            return DataSources.SelectMany(r => r.GetColumns());
        }

        public override int GetRowsCount(RenderContext context)
        {
            return InternalGetRowsCount(context, ListItems);
        }

        public override bool ComputeAggregates(RenderContext context, ColumnAggregateType userAggregateType)
        {
            foreach (var dataSource in DataSources)
                dataSource.ComputeAggregates(context, userAggregateType);
            return true;
        }

    }
}
