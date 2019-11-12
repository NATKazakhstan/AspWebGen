/*
 * Created by: Sergey V. Shpakovskiy
 * Created: 24 декабря 2008 г.
 * Copyright © JSC New Age Technologies 2008
 */

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.UI;
using Nat.Web.Tools;
using System.Web;
using System.Data.Linq;
using System.Reflection;
using Nat.Web.Tools.Security;

namespace Nat.Web.Controls.GenerationClasses
{
    public class BaseGridColumns
    {
        public delegate void GenearetHtml(StringBuilder sb, GridHtmlGenerator.Column column);
        public delegate void SetDataRow<TRow>(TRow row) where TRow : BaseRow, new();

        private readonly Dictionary<string, GridHtmlGenerator.Column> _dicColumns;
        private readonly List<GridHtmlGenerator.Column> _columns;
        private bool _parity;
        private const int LoadTreeCount = 200;
        private const int MaxLevelTreeLoad = 4;
        private const int MinCountNotCheckLoad = 3;
        private const int MaxSumCounts = 150;
        private int _sumCounts;
        private readonly Dictionary<string, bool> _loadedIDs = new Dictionary<string, bool>();
        private List<string> ChildsID { get; set; }

        public Page Page { get; set; }
        public Control Control { get; set; }

        /*
                private const string checkbox =
                    "<input type=\"checkbox\" id=\"{0}\" title=\"{1}\" checked=\"{2}\" disabled=\"{3}\" style=\"{4}\" class\"{5}\" />";
                private const string checkbox1 =
                    "<input type=\"checkbox\" id=\"{0}\" checked=\"{2}\" class\"{3}\" />";
         */

        public BaseGridColumns(IEnumerable<GridHtmlGenerator.Column> columns)
        {
            UsePostBackForChilds = true;
            if (columns == null)
                throw new ArgumentNullException("columns");
            _columns = new ListColumns(columns, this);
            _dicColumns = _columns.ToDictionary(p => p.ColumnName);
            SortByGroup(_columns);
        }

        public void MoveColumnAfter(GridHtmlGenerator.Column column, GridHtmlGenerator.Column afterColumn)
        {
            var indexOfColumn = Columns.IndexOf(column);
            var toIndex = Columns.IndexOf(afterColumn) + 1;
            /*if (toIndex == Columns.Count)
            {
                Columns.RemoveAt(indexOfColumn);
                Columns.Add(column);
                return;
            }*/

            MoveColumnToIndex(indexOfColumn, toIndex);
        }

        public void MoveColumnBefore(GridHtmlGenerator.Column column, GridHtmlGenerator.Column beforeColumn)
        {
            var indexOfColumn = Columns.IndexOf(column);
            var toIndex = Columns.IndexOf(beforeColumn);
            /*if (toIndex == 0)
            {
                Columns.RemoveAt(indexOfColumn);
                Columns.Insert(0, column);
                return;
            }*/

            MoveColumnToIndex(indexOfColumn, toIndex);
        }

        public void MoveColumnToIndex(int indexOfColumn, int toIndex)
        {
            var column = Columns[indexOfColumn];
            if (toIndex > indexOfColumn)
            {
                toIndex--;
                for (int i = indexOfColumn; i < toIndex; i++)
                    Columns[i] = Columns[i + 1];
                Columns[toIndex] = column;
            }
            else
            {
                for (int i = indexOfColumn; i > toIndex; i--)
                    Columns[i] = Columns[i - 1];
                Columns[toIndex] = column;
            }
        }

        /// <summary>
        /// Настройка количества строчек на одну запись. Если 0 то все в одну строку независимо от индекса строки колонки.
        /// </summary>
        public int CountRowsInOneRow { get; set; }

        public bool GenerateWithoutRow { get; set; }

        public bool UsePostBackForChilds { get; private set; }

        public void StartGenerate()
        {
            _sumCounts = 0;
            _loadedIDs.Clear();
        }

        public void GeneateReadHtml(StringBuilder sb, string cssClass)
        {
            if (GenerateWithoutRow)
                GenerateHtmlWithoutRow(sb, GenerateReadHtml);
            else if (CountRowsInOneRow == 0)
                GenerateHtmlInOneRow(sb, cssClass, cssClass, GenerateReadHtml);
            else
                GenerateHtmlInMultipleRow(sb, cssClass, cssClass, GenerateReadHtml);
        }

        public void GeneateReadHtml(StringBuilder sb, string cssClass, string cssClassNotSelected)
        {
            if (GenerateWithoutRow)
                GenerateHtmlWithoutRow(sb, GenerateReadHtml);
            else if (CountRowsInOneRow == 0)
                GenerateHtmlInOneRow(sb, cssClass, cssClassNotSelected, GenerateReadHtml);
            else
                GenerateHtmlInMultipleRow(sb, cssClass, cssClassNotSelected, GenerateReadHtml);
        }

        public void GeneateReadHtml<TKey, TTable, TDataContext, TRow>(StringBuilder sb, string cssClass, string cssClass0, string cssClass1, BaseDataSourceView<TKey, TTable, TDataContext, TRow> view, TTable currentRow, SetDataRow<TRow> setDataRowDelegate, int countChilds)
            where TKey : struct
            where TTable : class
            where TDataContext : DataContext, new()
            where TRow : BaseRow, new()
        {
            if (ChildsID == null)
                ChildsID = new List<string>();
            if (GenerateWithoutRow)
                GenerateHtmlWithoutRow(sb, GenerateReadHtml);
            else if (CountRowsInOneRow == 0)
                GenerateHtmlInOneRow(sb, cssClass, cssClass0, cssClass1, GenerateReadHtml, view, currentRow, setDataRowDelegate, countChilds, countChilds, 0);
            else
                GenerateHtmlInMultipleRow(sb, cssClass, cssClass, GenerateReadHtml);
        }

        public void GenerateEditHtml(StringBuilder sb, string cssClass)
        {
            if (CountRowsInOneRow == 0)
                GenerateHtmlInOneRow(sb, cssClass, cssClass, GenerateEditHtml);
            else
                GenerateHtmlInMultipleRow(sb, cssClass, cssClass, GenerateEditHtml);
        }

        public void GenerateEditHtml(StringBuilder sb, string cssClass, string cssClassNotSelected)
        {
            if (CountRowsInOneRow == 0)
                GenerateHtmlInOneRow(sb, cssClass, cssClassNotSelected, GenerateEditHtml);
            else
                GenerateHtmlInMultipleRow(sb, cssClass, cssClassNotSelected, GenerateEditHtml);
        }

        private void GenerateReadHtml(StringBuilder sb, GridHtmlGenerator.Column column)
        {
            column.ColumnContentHandler(sb);
        }

        private void GenerateEditHtml(StringBuilder sb, GridHtmlGenerator.Column column)
        {
            if (column.CanEdit && column.ColumnEditContentHandler != null)
                column.ColumnEditContentHandler(sb);
            else
                column.ColumnContentHandler(sb);
        }

        private void GenerateHtmlWithoutRow(StringBuilder sb, GenearetHtml handler)
        {
            var id = GetNextID();
            _parity = !_parity;
            foreach (var column in Columns.Where(p => p.Visible && (p.VisiblePermissions == null || UserRoles.IsInAnyRoles(p.VisiblePermissions))))
            {
                if (column is GridHtmlGenerator.ButtonsColumn)
                {
                    sb.Append("<td class=\"gridActionCellIsSelectedAsRow\" style=\"display:none\" id=\"buttons_");
                    sb.Append(id);
                }
                else
                {
                    if (column.ColumnValueHandler != null && column.ColumnValueHandler() == null) continue;
                    sb.Append("<td style=\"cursor:hand;\" onclick=\"javascript:_selectCell(this,");
                    sb.Append(id);
                    sb.Append(");\" keyID=\"");
                    sb.Append(id);
                    sb.Append("\" class=\"");
                    sb.Append(_parity ? "gridCellIsNotSelectedAsRow1" : "gridCellIsNotSelectedAsRow2");
                }
                if (!string.IsNullOrEmpty(column.Width))
                {
                    sb.Append("\" width=\"");
                    sb.Append(column.Width);
                }
                sb.Append("\" parity=\"");
                sb.Append(_parity ? 1 : 0);
                sb.Append("\">");
                handler(sb, column);
                sb.Append("</td>");
            }
        }

        private static int GetNextID()
        {
            if (HttpContext.Current.Items["BaseGridColumns.GetNextID"] == null)
            {
                HttpContext.Current.Items["BaseGridColumns.GetNextID"] = 0;
                return 0;
            }
            var value = (int)HttpContext.Current.Items["BaseGridColumns.GetNextID"];
            HttpContext.Current.Items["BaseGridColumns.GetNextID"] = ++value;
            return value;
        }

        private void GenerateHtmlInOneRow(StringBuilder sb, string cssClass, string cssClassNotSelected, GenearetHtml handler)
        {
            int colSpan = 0;

            sb.AppendLine();
            int tabCount = ChildsID != null ? ChildsID.Count : 0;
            sb.Append('\t', tabCount);
            sb.AppendFormat("<tr class=\"{0}\" notSelectedCSS=\"{1}\"", cssClass, cssClassNotSelected);
            if (ChildsID != null)
            {
                if (ChildsID.Count > 0)
                    sb.Append(" style=\"display:none\"");
                sb.AppendFormat(" level=\"{0}\" parentID=\"{1}\"", ChildsID.Count, ChildsID.LastOrDefault());
                /*int i = 0;
                foreach (var childID in ChildsID)
                {
                    sb.Append(" level");
                    sb.Append(i);
                    sb.Append("=\"");
                    sb.Append(childID);
                    i++;
                }*/
            }
            sb.Append(">");
            sb.AppendLine();

            foreach (var column in Columns.Where(p => p.Visible && (p.VisiblePermissions == null || UserRoles.IsInAnyRoles(p.VisiblePermissions))))
            {
                colSpan--;
                if (colSpan > 0 || column.ColSpan == 0) continue;
                var rowSpan = column.GetRowSpan();
                if (rowSpan < 0) continue;

                var style = column.GetStyle();
                colSpan = column.ColSpan;
                sb.AppendLine();
                sb.Append('\t', tabCount + 1);
                sb.Append("<td");
                if (colSpan > 1)
                {
                    sb.Append(" colspan=");
                    sb.Append(colSpan);
                }
                if (rowSpan > 1)
                {
                    sb.Append(" rowspan=");
                    sb.Append(rowSpan);
                }

                if (!string.IsNullOrEmpty(style))
                {
                    sb.Append(" style=\"");
                    sb.Append(style);
                    sb.Append("\"");
                }

                sb.Append(">");

                handler(sb, column);
                sb.Append("</td>");
                sb.AppendLine();
            }

            sb.AppendLine();
            sb.Append('\t', tabCount);
            sb.AppendLine("</tr>");
        }

        private void GenerateHtmlInOneRow<TKey, TTable, TDataContext, TRow>(StringBuilder sb, string cssClass, string cssClass0, string cssClass1, GenearetHtml handler, BaseDataSourceView<TKey, TTable, TDataContext, TRow> view, TTable currentRow, SetDataRow<TRow> setDataRowDelegate, int countChilds, int allCountChilds, int level)
            where TKey : struct
            where TTable : class
            where TDataContext : DataContext, new()
            where TRow : BaseRow, new()
        {
            string id = view.GetStringID(currentRow);
            if (_loadedIDs.ContainsKey(id))
            {
                UsePostBackForChilds = false;
                GenerateHtmlInOneRow(sb, cssClass, null, handler);
            }
            else if ((
                        (LoadTreeCount >= allCountChilds && _sumCounts < MaxSumCounts)
                        || countChilds <= MinCountNotCheckLoad
                    ) && level < MaxLevelTreeLoad && countChilds > 0)
            {
                _sumCounts += countChilds;
                UsePostBackForChilds = false;
                GenerateHtmlInOneRow(sb, cssClass, null, handler);
                ChildsID.Add(id);
                _loadedIDs.Add(id, true);
                var data = view.GetDataByRefParent(currentRow);
                bool class0 = true;
                PropertyInfo itemPropertyInfo = null;
                foreach (var row in data)
                {
                    if (itemPropertyInfo == null)
                        itemPropertyInfo = row.GetType().GetProperty("Item");
                    var count = row.CountChildsData();
                    setDataRowDelegate(row);
                    GenerateHtmlInOneRow(sb, class0 ? cssClass0 : cssClass1,
                        cssClass0, cssClass1, handler, view,
                        (TTable)itemPropertyInfo.GetValue(row, null), setDataRowDelegate,
                        count, allCountChilds * count, level + 1);
                    class0 = !class0;
                }
                ChildsID.RemoveAt(ChildsID.Count - 1);
            }
            else
            {
                UsePostBackForChilds = true;
                GenerateHtmlInOneRow(sb, cssClass, null, handler);
            }
        }

        private void GenerateHtmlInMultipleRow(StringBuilder sb, string firstCssClass, string otherCssClass, GenearetHtml handler)
        {
            int colSpan = 0;
            sb.Append("<tr class=\"");
            sb.Append(firstCssClass);
            sb.Append("\">");

            var listSb = new List<StringBuilder>(CountRowsInOneRow)
                {
                    sb
                };
            for (int i = 0; i < CountRowsInOneRow - 1; i++)
            {
                sb = new StringBuilder();
                sb.Append("<tr class=\"");
                sb.Append(otherCssClass);
                sb.Append("\">");
                listSb.Add(sb);
            }

            var columns = Columns
                .Where(
                    p => (p.VisiblePermissions == null || UserRoles.IsInAnyRoles(p.VisiblePermissions))
                         && p.Visible);
            var countColumns = columns
                .Count(
                    r => r.ColumnName != "__icons"
                         && r.ColumnName != "__toChilds"
                         && r.ColumnName != "__buttons"
                         && r.RowIndex == 0);

            var countEmptyRows =
                columns.Count(
                    r =>
                    r.AsNewRow
                    && (r.ColumnValueHandler() == null || string.Empty.Equals(r.ColumnValueHandler())));
            var countRowsInOneRow = CountRowsInOneRow - countEmptyRows;

            foreach (var column in columns)
            {
                if (column.ColumnName == "__icons" || column.ColumnName == "__toChilds")
                    column.RowSpan = countRowsInOneRow;

                sb = listSb[column.RowIndex];
                if (column.AsNewRow)
                {
                    if (column.ColumnValueHandler() == null || string.Empty.Equals(column.ColumnValueHandler()))
                    {
                        listSb[column.RowIndex] = null;
                        continue;
                    }

                    sb.Append("<td class='nat-headerInRow'>");
                    GridHtmlGenerator.RenderHeaderText(Page, Control, column, sb);
                    sb.Append("</td>");                    
                }

                colSpan--;
                if (colSpan > 0 || column.ColSpan == 0) continue;
                if (!column.AsNewRow)
                    colSpan = column.ColSpan;
                else
                    column.ColSpan = countColumns;
                if (column.ColSpan > 1 || column.RowSpan > 1)
                {
                    sb.Append("<td");
                    if (column.ColSpan > 1)
                    {
                        sb.Append(" colspan=");
                        sb.Append(column.ColSpan);
                    }

                    if (column.RowSpan > 1)
                    {
                        sb.Append(" rowspan=");
                        sb.Append(column.RowSpan);
                    }

                    sb.Append(">");
                }
                else
                    sb.Append("<td>");
                handler(sb, column);
                sb.Append("</td>");
            }

            listSb[0].Append("</tr>");
            for (int i = 1; i < CountRowsInOneRow; i++)
                if (listSb[i] != null)
                    listSb[0].Append(listSb[i].ToString()).Append("</tr>");
        }

        public static void SortByGroup(IList<GridHtmlGenerator.Column> columns)
        {
            int maxRowSpan = columns.Max(p => p.GroupParts.Count);
            var groups = new Dictionary<string, int>();
            for (int i = 0; i < maxRowSpan; i++)
            {
                for (int col = 0; col < columns.Count; col++)
                {
                    var column = columns[col];
                    if (column.GroupParts.Count <= i) continue;
                    var part = column.GroupParts[i];
                    if (groups.ContainsKey(part))
                    {
                        var prevColumn = groups[part];
                        if (prevColumn != col) //колонка оказалась дальше расположения группы
                        {
                            //перемещение колонки на перед
                            columns.RemoveAt(col);
                            columns.Insert(prevColumn, column);
                            //пометка что каждая следующая колонка переместилась на один
                            foreach (var key in groups.Keys.ToArray())
                                if (groups[key] >= prevColumn) groups[key]++;
                        }
                        else
                            groups[part]++; //сдвиг, следующая колонка должна стоять следом за этой

                    }
                    else
                        groups.Add(part, col + 1); //добавление следующей позиции колонки этой группы
                }
            }
        }

        public IList<GridHtmlGenerator.Column> Columns
        {
            get { return _columns; }
        }

        public bool ContainsColumn(string nameColumn)
        {
            return _dicColumns.ContainsKey(nameColumn);
        }

        public GridHtmlGenerator.Column this [string nameColumn]
        {
            get { return _dicColumns[nameColumn]; }
        }

        public static string AddCheckBox(StringBuilder sb, string key, string id, bool check)
        {
            var idValue = key + "_" + id;
            sb.AppendFormat(check ? HtmlGenerator.checkbox2 : HtmlGenerator.checkbox3, idValue);
            return idValue;
        }

        public static string AddTextBox(StringBuilder sb, string key, string id, object value, int maxLength, int columns, string width)
        {
            var idValue = key + "_" + id;
            HtmlGenerator.AddTextBox(sb, idValue, value, maxLength, columns, width);
            return idValue;
        }

        public static string AddTextArea(StringBuilder sb, string key, string id, object value, int rows, int columns)
        {
            var idValue = key + "_" + id;
            HtmlGenerator.AddTextArea(sb, idValue, value, rows, columns);
            return idValue;
        }

        public static string AddLookupTextBox(StringBuilder sb, string key, string id, long? value, string textValue, string tableName, string projectName, string mode, 
            ExtenderAjaxControl extenderAjaxControl, int minimumPrefixLength)
        {
            var idValue = key + "_" + id;
            HtmlGenerator.AddLookupTextBox(sb, idValue, value, textValue, tableName, projectName, mode, new BrowseFilterParameters(), 
                                           extenderAjaxControl, minimumPrefixLength, "180px");
            return idValue;
        }

        public static string AddLookupTextBox(StringBuilder sb, string key, string id, long? value, string textValue, string tableName, string projectName, string mode,
            ExtenderAjaxControl extenderAjaxControl, int minimumPrefixLength, string width)
        {
            var idValue = key + "_" + id;
            HtmlGenerator.AddLookupTextBox(sb, idValue, value, textValue, tableName, projectName, mode, new BrowseFilterParameters(),
                extenderAjaxControl, minimumPrefixLength, string.IsNullOrEmpty(width) ? "180px" : width);
            return idValue;
        }

        public static string AddLookupTextBox(StringBuilder sb, string key, string id, long? value, string textValue, string tableName, string projectName, string mode,
            ExtenderAjaxControl extenderAjaxControl, int minimumPrefixLength, string width, BrowseFilterParameters browseFilterParameters)
        {
            var idValue = key + "_" + id;
            HtmlGenerator.AddLookupTextBox(sb, idValue, value, textValue, tableName, projectName, mode, browseFilterParameters,
                extenderAjaxControl, minimumPrefixLength, string.IsNullOrEmpty(width) ? "180px" : width);
            return idValue;
        }

        public static string AddDropDownList(StringBuilder sb, string key, string id, long? value, long? valueNotSet, string textNotSet, IDataSource dataSource)
        {
            return AddDropDownList(sb, key, id, value, valueNotSet, textNotSet, dataSource, "");
        }

        public static string AddDropDownList(StringBuilder sb, string key, string id, long? value, long? valueNotSet, string textNotSet, IDataSource dataSource, string width)
        {
            var idValue = key + "_" + id;
            var view = dataSource.GetView("default");
            IEnumerable dataSourceData = null;
            view.Select(new DataSourceSelectArguments(), delegate(IEnumerable data) { dataSourceData = data; });
            if (dataSourceData != null)
            {
                sb.AppendFormat(HtmlGenerator.dropDownListStart, idValue, width, HtmlGenerator.dropDownListScript);
                sb.AppendFormat(
                    valueNotSet == value ? HtmlGenerator.dropDownListSelectedOption : HtmlGenerator.dropDownListOption,
                    valueNotSet, textNotSet);
                foreach (var row in dataSourceData)
                {
                    var propertyID = row.GetType().GetProperty("id");
                    var propertyName = row.GetType().GetProperty(FindHelper.GetContentFieldName("nameRu", "nameKz"));
                    var rowID = (long)propertyID.GetValue(row, null);
                    var rowName = propertyName.GetValue(row, null);
                    sb.AppendFormat(
                        rowID == value ? HtmlGenerator.dropDownListSelectedOption : HtmlGenerator.dropDownListOption,
                        rowID, rowName);
                }
                sb.AppendFormat(HtmlGenerator.dropDownListEnd);
            }
            return idValue;
        }

        public static string AddDropDownList(StringBuilder sb, string key, string id, long? value, IDataSource dataSource)
        {
            var idValue = key + "_" + id;
            var view = dataSource.GetView("default");
            IEnumerable dataSourceData = null;
            view.Select(new DataSourceSelectArguments(), delegate(IEnumerable data) { dataSourceData = data; });
            if (dataSourceData != null)
            {
                sb.AppendFormat(HtmlGenerator.dropDownListStart, idValue, "", HtmlGenerator.dropDownListScript);
                foreach (var row in dataSourceData)
                {
                    var propertyID = row.GetType().GetProperty("id");
                    var propertyName = row.GetType().GetProperty(FindHelper.GetContentFieldName("nameRu", "nameKz"));
                    var rowID = (long)propertyID.GetValue(row, null);
                    var rowName = propertyName.GetValue(row, null);
                    sb.AppendFormat(
                        rowID == value ? HtmlGenerator.dropDownListSelectedOption : HtmlGenerator.dropDownListOption,
                        rowID, rowName);
                }
                sb.AppendFormat(HtmlGenerator.dropDownListEnd);
            }
            return idValue;
        }

        public static string AddDropDownList(StringBuilder sb, string key, string id, long? value, long? valueNotSet, string textNotSet, IEnumerable<KeyValuePair<long, string>> dataSource)
        {
            return AddDropDownList(sb, key, id, value, valueNotSet, textNotSet, dataSource, "");
        }

        public static string AddDropDownList(StringBuilder sb, string key, string id, long? value, long? valueNotSet, string textNotSet, IEnumerable<KeyValuePair<long, string>> dataSource, string width)
        {
            var idValue = key + "_" + id;
            sb.AppendFormat(HtmlGenerator.dropDownListStart, idValue, width, HtmlGenerator.dropDownListScript);
            sb.AppendFormat(
                valueNotSet == value ? HtmlGenerator.dropDownListSelectedOption : HtmlGenerator.dropDownListOption,
                valueNotSet, textNotSet);
            foreach (var row in dataSource)
            {
                sb.AppendFormat(
                    row.Key == value ? HtmlGenerator.dropDownListSelectedOption : HtmlGenerator.dropDownListOption,
                    row.Key, row.Value);
            }
            sb.AppendFormat(HtmlGenerator.dropDownListEnd);
            return idValue;
        }

        public static string AddDropDownList(StringBuilder sb, string key, string id, long? value, IEnumerable<KeyValuePair<long, string>> dataSource)
        {
            var idValue = key + "_" + id;
            sb.AppendFormat(HtmlGenerator.dropDownListStart, idValue, "", HtmlGenerator.dropDownListScript);
            foreach (var row in dataSource)
            {
                sb.AppendFormat(
                    row.Key == value ? HtmlGenerator.dropDownListSelectedOption : HtmlGenerator.dropDownListOption,
                    row.Key, row.Value);
            }
            sb.AppendFormat(HtmlGenerator.dropDownListEnd);
            return idValue;
        }

        private class ListColumns : List<GridHtmlGenerator.Column>, IList<GridHtmlGenerator.Column>
        {
            private readonly BaseGridColumns _gridColumns;

            public ListColumns(IEnumerable<GridHtmlGenerator.Column> collection, BaseGridColumns gridColumns) : base(collection)
            {
                _gridColumns = gridColumns;
            }

            void IList<GridHtmlGenerator.Column>.Insert(int index, GridHtmlGenerator.Column item)
            {
                _gridColumns._dicColumns.Add(item.ColumnName, item);
                Insert(index, item);
            }

            void IList<GridHtmlGenerator.Column>.RemoveAt(int index)
            {
                _gridColumns._dicColumns.Remove(this[index].ColumnName);
                RemoveAt(index);
            }

            void ICollection<GridHtmlGenerator.Column>.Add(GridHtmlGenerator.Column item)
            {
                _gridColumns._dicColumns.Add(item.ColumnName, item);
                Add(item);
            }

            bool ICollection<GridHtmlGenerator.Column>.Remove(GridHtmlGenerator.Column item)
            {
                _gridColumns._dicColumns.Remove(item.ColumnName);
                return Remove(item);
            }

            void ICollection<GridHtmlGenerator.Column>.Clear()
            {
                _gridColumns._dicColumns.Clear();
            }
        }
    }
}