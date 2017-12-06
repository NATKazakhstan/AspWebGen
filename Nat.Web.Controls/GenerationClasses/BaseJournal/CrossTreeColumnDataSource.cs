using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.UI;
using Nat.Web.Controls.Properties;

namespace Nat.Web.Controls.GenerationClasses.BaseJournal
{
    public abstract class CrossTreeColumnDataSource<THeaderKey, TRow, THeaderTable> : CrossColumnDataSource<THeaderKey, TRow, THeaderTable>
        where THeaderKey : struct
        where TRow : BaseRow
        where THeaderTable : class, ICrossTreeTable<THeaderKey, THeaderTable>
    {
        protected CrossTreeColumnDataSource()
        {
            MaxRecursion = int.MaxValue;
        }

        public int MaxRecursion { get; set; }

        /// <summary>
        /// Колонки выводятся до дочерних данных.
        /// </summary>
        public bool FirstColumns { get; set; }

        protected override void AddItems(THeaderTable row)
        {
            AddItems(row, ListItems, 1);
        }

        protected void AddItems(THeaderTable row, List<CrossColumnDataSourceItem<THeaderKey, TRow, THeaderTable>> listItems, int level)
        {
            var item = new CrossColumnDataSourceItem<THeaderKey, TRow, THeaderTable>
                           {
                               Header = GetColumnHeader(row),
                               HeaderRu = GetColumnHeaderRu(row),
                               HeaderKz = GetColumnHeaderKz(row),
                               Childs = new List<CrossColumnDataSourceItem<THeaderKey, TRow, THeaderTable>>(),
                               ColumnId = row.Id,
                               Row = row,
                               BaseColumnName = BaseColumnName,
                           };
            listItems.Add(item);
            if (DicItems.ContainsKey(row.Id.ToString()))
            {
                throw new Exception(
                    "Detect circle in cross header datasource. Exists: "
                    + string.Join(";", DicItems.Keys.Select(r => r.ToString()).ToArray()) + " try to add: "
                    + row.Id);
            }

            DicItems[row.Id.ToString()] = item;

            if (FirstColumns)
                AddColumns(row, item);

            if (MaxRecursion > level && row.ChildObjects != null)
                foreach (var childRow in FilterData(row.ChildObjects.AsQueryable(), null))
                    AddItems(childRow, item.Childs, level + 1);

            if (!FirstColumns)
                AddColumns(row, item);
        }

        protected override IQueryable<THeaderTable> FilterData(IQueryable<THeaderTable> data, System.Data.Linq.DataContext db)
        {
            MaxRecursion = Filter.GetMaxRecursion<THeaderTable>(this);
            if (MaxRecursion <= 0) MaxRecursion = int.MaxValue;
            return base.FilterData(data, db);
        }

        protected override void RenderHeaderContent(CrossColumnDataSourceItem<THeaderKey, TRow, THeaderTable> column, HtmlTextWriter writer)
        {
            if (column.Column != null)
                base.RenderHeaderContent(column, writer);
            else
                RenderTreeLink(column, writer);
        }

        protected virtual void RenderTreeLink(CrossColumnDataSourceItem<THeaderKey, TRow, THeaderTable> column, HtmlTextWriter writer)
        {
            var postBack = HeaderControl.Page.ClientScript.GetPostBackClientHyperlink(
                HeaderControl.Journal,
                string.Format(
                    "FilterBy:{0}:Equals:{1}:{2}",
                    BaseFilterParameter.TreeStartLevelFilterName + typeof(THeaderTable).Name,
                    column.ColumnId,
                    column.Header));
            writer.AddAttribute(HtmlTextWriterAttribute.Href, postBack);
            writer.AddAttribute(HtmlTextWriterAttribute.Title, Resources.SFilterHeader);
            writer.RenderBeginTag(HtmlTextWriterTag.A);
            base.RenderHeaderContent(column, writer);
            writer.RenderEndTag();
        }

        protected override void InitHierarchy(HierarchyFields.ColumnHierarchy newItem, CrossColumnDataSourceItem item, Dictionary<string, HierarchyFields.ColumnHierarchy> existsColumns, Dictionary<string, BaseColumn> columnsDic)
        {
            newItem.IsTreeColumn = true;
            base.InitHierarchy(newItem, item, existsColumns, columnsDic);
        }

        public override void RenderBackUrls(HtmlTextWriter writer)
        {
            var keys = Filter.GetStartTreeKeys<THeaderTable>(this);
            if (keys.Count != 1) return;
            var data = GetData(keys.First());
            var row = data.FirstOrDefault();
            if (row != null)
            {
                writer.AddAttribute(HtmlTextWriterAttribute.Class, "font13");
                writer.RenderBeginTag(HtmlTextWriterTag.Div);

                var postBack = HeaderControl.Page.ClientScript.GetPostBackClientHyperlink(
                    HeaderControl.Journal,
                    string.Format("FilterBy:{0}:Non::",
                                  BaseFilterParameter.TreeStartLevelFilterName + typeof(THeaderTable).Name));
                writer.AddAttribute(HtmlTextWriterAttribute.Href, postBack);
                writer.AddAttribute(HtmlTextWriterAttribute.Title, Resources.SFilterHeader);
                writer.RenderBeginTag(HtmlTextWriterTag.A);
                writer.Write(Resources.SAllHierarchy);
                writer.RenderEndTag();
                writer.Write("</br>");

                if (row.ParentObject != null)
                    RenderBackUrls(writer, row.ParentObject, 1);
                writer.RenderEndTag();
            }
        }

        protected virtual int RenderBackUrls(HtmlTextWriter writer, THeaderTable item, int level)
        {
            var parentItem = item.ParentObject;
            if (parentItem != null)
                level = RenderBackUrls(writer, parentItem, level);
            for (int i = 0; i < level; i++)
                writer.Write("&nbsp;&nbsp;&nbsp;");

            var header = GetColumnHeader(item);
            var postBack = HeaderControl.Page.ClientScript.GetPostBackClientHyperlink(
                HeaderControl.Journal,
                string.Format("FilterBy:{0}:Equals:{1}:{2}",
                              BaseFilterParameter.TreeStartLevelFilterName + typeof(THeaderTable).Name,
                              item.Id,
                              header));
            writer.AddAttribute(HtmlTextWriterAttribute.Href, postBack);
            writer.AddAttribute(HtmlTextWriterAttribute.Title, Resources.SFilterHeader);
            writer.RenderBeginTag(HtmlTextWriterTag.A);
            writer.Write(header);
            writer.RenderEndTag();
            writer.Write("</br>");
            return level + 1;
        }
    }
}