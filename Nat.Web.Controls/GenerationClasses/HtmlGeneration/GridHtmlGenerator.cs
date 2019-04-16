/*
 * Created by: Sergey V. Shpakovskiy
 * Created: 23 ������� 2008 �.
 * Copyright � JSC New Age Technologies 2008
 */

namespace Nat.Web.Controls.GenerationClasses
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using System.Text;
    using System.Web;
    using System.Web.UI;

    using Nat.Web.Controls.Properties;
    using Nat.Web.Tools;
    using Nat.Web.Tools.Export;
    using Nat.Web.Tools.Security;

    public delegate void GetColumnContent(StringBuilder sb);

    public delegate object GetColumnValue();

    public class GridHtmlGenerator
    {
        /// <summary>
        /// �������� Html ��������� ������� �� ������ �������.
        /// </summary>
        /// <param name="page">�������� ��� ����������.</param>
        /// <param name="control">������� ����������� ������� ���������� "sort:����������".</param>
        /// <param name="columns">������� ��� ����������.</param>
        /// <returns>Html ��������� �������.</returns>
        public static string GetHeader(Page page, Control control, IEnumerable<Column> columns)
        {
            columns = columns
                .Where(p => (p.VisiblePermissions == null || UserRoles.IsInAnyRoles(p.VisiblePermissions))
                            && !p.AsNewRow && p.Visible)
                .ToArray();
            var sb = new StringBuilder();
            var maxRowSpan = columns.Max(p => p.GroupParts.Count) + 1;
            var fillGroup = new Dictionary<string, byte>();
            var visibleColumns = columns.Where(p => !p.Added);
            for (int i = 0; i < maxRowSpan; i++)
            {
                sb.Append("<tr class=\"ms-vh\">");
                foreach (var column in visibleColumns)
                {
                    // ��������� �������
                    // ���������� �������� ������� �� �� ������� (�� ����������� �����)
                    if (column.GroupParts.Count == i)
                    {
                        sb.Append("<th rowSpan=\"");
                        sb.Append(maxRowSpan - i);
                        if (!string.IsNullOrEmpty(column.Width))
                        {
                            sb.Append("\" width=\"");
                            sb.Append(column.Width);
                        }

                        if (!string.IsNullOrEmpty(column.ToolTip))
                        {
                            sb.Append("\" title=\"");
                            sb.Append(HttpUtility.HtmlAttributeEncode(column.ToolTip));
                        }

                        if (column.IsVerticalHeaderText)
                            sb.Append("\" style=\"writing-mode:tb-rl; filter:flipv fliph;\">");
                        else
                            sb.Append("\">");
                        RenderHeaderText(page, control, column, sb);
                        sb.Append("</th>");
                        column.Added = true;
                    }
                    else if (!fillGroup.ContainsKey(column.GroupParts[i]))
                    {
                        // ��������� ������
                        // ���� ������ ��� �� �����������
                        fillGroup.Add(column.GroupParts[i], 0);
                        var index = column.GroupParts[i].LastIndexOf('#');
                        string groupCaption = column.GroupParts[i].Substring(index + 1);
                        sb.Append("<th colSpan=\"");

                        // ������ ������ � ��������, �.�. ���������� ������� ������� �� �� �������� ������
                        sb.Append(visibleColumns.Count(p =>
                                                       p.GroupParts.Count > i && p.GroupParts[i] == column.GroupParts[i]));
                        sb.Append("\">");
                        sb.Append(groupCaption);
                        sb.Append("</th>");
                    }
                }

                sb.Append("</tr>");
                fillGroup.Clear();
            }

            return sb.ToString();
        }

        public static List<IExportColumn> GetColumnsForExport(IEnumerable<Column> columns)
        {
            columns = columns
                .Where(p => (p.VisiblePermissions == null || UserRoles.IsInAnyRoles(p.VisiblePermissions))
                            && p.ExportVisible
                            && !p.ColumnName.StartsWith("__"))
                .ToArray();
            return GetColumnsHierarchy<Column, Column>(columns).Cast<IExportColumn>().ToList();
        }

        public static List<Column> GetColumnsHierarchy(IEnumerable<Column> columns)
        {
            return GetColumnsHierarchy<Column>(columns);
        }

        public static List<Column> GetColumnsHierarchy<TColumn>(IEnumerable<Column> columns)
            where TColumn : Column, new()
        {
            return GetColumnsHierarchy<TColumn>(columns, p => p.Visible);
        }
        public static List<Column> GetColumnsHierarchy<TColumn>(IEnumerable<Column> columns, Func<Column, bool> isVisible)
            where TColumn : Column, new()
        {
            columns = columns
                .Where(p => (p.VisiblePermissions == null || UserRoles.IsInAnyRoles(p.VisiblePermissions))
                            && !p.AsNewRow
                            && isVisible(p))
                .ToArray();

            return GetColumnsHierarchy<TColumn, Column>(columns);
        }

        private static List<TResultColumn> GetColumnsHierarchy<TColumn, TResultColumn>(IEnumerable<TResultColumn> columns)
            where TColumn : TResultColumn, new()
            where TResultColumn : Column
        {
            var exportList = new List<TResultColumn>();
            foreach (var column in columns)
                column.Added = false;

            var maxRowSpan = columns.Max(p => p.GroupParts.Count) + 1;
            var fillGroup = new Dictionary<string, byte>();
            var groupColumns = new Dictionary<string, TResultColumn>();
            var visibleColumns = columns.Where(p => !p.Added);
            for (int i = 0; i < maxRowSpan; i++)
            {
                foreach (var column in visibleColumns)
                {
                    // ��������� �������
                    // ���������� �������� ������� �� �� ������� (�� ����������� �����)
                    if (column.GroupParts.Count == i)
                    {
                        if (column.GroupParts.Count > 0)
                            groupColumns[column.GroupParts[i - 1]].ChildColumns.Add(column);
                        else
                            exportList.Add(column);
                        column.Added = true;
                        if (column.ExportWidth == 0) column.ExportWidth = 16;
                    }
                    else if (!fillGroup.ContainsKey(column.GroupParts[i]))
                    {
                        // ��������� ��������� ������, ���� ������ ��� �� �����������
                        fillGroup.Add(column.GroupParts[i], 0);
                        var index = column.GroupParts[i].LastIndexOf('#');
                        string groupCaption = column.GroupParts[i].Substring(index + 1);
                        var groupColumn = new TColumn
                            {
                                Header = groupCaption,
                                ExportVisible = true,
                                ChildColumns = new List<Column>(),
                            };
                        if (i > 0)
                            groupColumns[column.GroupParts[i - 1]].ChildColumns.Add(groupColumn);
                        else
                            exportList.Add(groupColumn);
                        groupColumns[column.GroupParts[i]] = groupColumn;
                    }
                }

                fillGroup.Clear();
            }

            return exportList;
        }

        public static void RenderHeaderText(Page page, Control control, Column column, StringBuilder sb)
        {
            if (string.IsNullOrEmpty(column.Sort))
                sb.Append(column.Header);
            else
                RenderSortHeader(page, control, column, sb);
        }

        public static void RenderSortHeader(Page page, Control control, Column column, StringBuilder sb)
        {
            sb.AppendFormat(
                "<a href=\"javascript:{0}\">{1}</a>",
                page.ClientScript.GetPostBackEventReference(control, "sort:" + column.Sort),
                column.Header);
        }

        #region Column
        public class Column : IExportColumn
        {
            private List<string> groupParts;
            private string group;
            private int? _rowSpan = null;

            public Column()
            {
                Visible = true;
                GroupVisible = true;
                CanEdit = true;
                ColSpan = 1;
                TotalColSpan = 1;
                Format = "{0}";
                Validators = new List<ValidatorProperties>();
            }

            /// <summary>
            /// ������������ �������. ������ ���� ����������.
            /// </summary>
            public string ColumnName { get; set; }

            /// <summary>
            /// ������������ �������. ������������ ������ �� �������� ���������� ��������� �������. ��� ����� ��������������� ��� ��������� ������������ ������ �� ������� � InitializeDinamicColumnsInRow.
            /// </summary>
            public string TableName { get; set; }

            /// <summary>
            /// ��������� �������.
            /// </summary>
            public string TableTitle { get; set; }

            /// <summary>
            /// ID ����������� �������� (� ���� ������� ������ "refMarks_{0}", �.�. ��� ������ �������� �� ��� ������),
            /// ������� �������� ��������.
            /// </summary>
            public string ControlID { get; set; }

            /// <summary>
            /// ��������� �������. 
            /// </summary>
            public string Header { get; set; }

            /// <summary>
            /// ����������� ��������� � ���������.
            /// </summary>
            public string ToolTip { get; set; }

            /// <summary>
            /// ���� �� �������� ����� �����������. ���������� ����������� �� �������� DB, ������ [...].Row ��� �� ����������, ��� ����������.
            /// </summary>
            public string Sort { get; set; }

            /// <summary>
            /// ����������� ����� �������. ����������� ������������ ����� "#".
            /// </summary>
            public string Group
            {
                get
                {
                    return group;
                }

                set
                {
                    group = value;
                    groupParts = null;
                }
            }

            /// <summary>
            /// ��������� �������. �� ��������� true.
            /// </summary>
            public bool Visible { get; set; }

            public bool GroupVisible { get; set; }

            /// <summary>
            /// �������� ������� ��� ��������� ������.
            /// </summary>
            public bool AsNewRow { get; set; }

            /// <summary>
            /// ������ ���� ��� ��������� �������.
            /// </summary>
            public string[] VisiblePermissions { get; set; }

            /// <summary>
            /// ������ �������.
            /// </summary>
            public virtual string Width { get; set; }

            public virtual void ColumnHyperLinkRenderer(StringBuilder sb)
            {
                var columnValue = ColumnValueHandler();
                if (columnValue == null)
                    return;
                sb.Append("/MainPage.aspx/navigateto/");
                sb.Append(TableName);
                sb.Append("Edit/read?ref");
                sb.Append(TableName);
                sb.Append("=");
                sb.Append(HttpUtility.UrlEncode(Convert.ToString(columnValue)));
                AddToHyperLinkParameters?.Invoke(sb);
            }

            public virtual void ColumnContentRenderer(StringBuilder sb)
            {
                var columnValue = ColumnValueHandler();
                if (columnValue == null)
                    return;
                sb.Append("<a href=\"");
                ColumnHyperLinkRenderer(sb);
                sb.Append("\" target=\"_blank\" tabindex=\"-1\" title=\"");
                sb.Append(Resources.SLook);
                sb.Append("\">");
                sb.Append(HttpUtility.HtmlEncode(ColumnTextHandler()));
                sb.Append("</a>");
            }

            /// <summary>
            /// ����� ���������� ������ �������, ���� ������ �� ������������� ��� ������ � ������� ������ �������������.
            /// </summary>
            public virtual GetColumnContent ColumnContentHandler { get; set; }

            /// <summary>
            /// ����� ���������� ������ �������, ���� ������ �� ������������� ��� ������ � ������� ������ �������������.
            /// </summary>
            public Func<string> ColumnTextHandler { get; set; }

            /// <summary>
            /// ����� ���������� ������ �������, ���� ������ �������������, ������ � ������� ����� �������������.
            /// </summary>
            public GetColumnContent ColumnEditContentHandler { get; set; }

            /// <summary>
            /// ����� ���������� ������ �������, ���� ������ �������������, ������ � ������� ����� �������������.
            /// </summary>
            public GetColumnContent ColumnHyperLinkHandler { get; set; }

            /// <summary>
            /// ����� ���������� ������ ������ �������.
            /// </summary>
            public GetColumnContent ColumnContentTotalHandler { get; set; }

            public GetColumnContent AddToHyperLinkParameters { get; set; }

            /// <summary>
            /// �����, ����������� �������� �������� ����. 
            /// ������������ ��� �����������, ��� ������������ ������ �� ��������.
            /// </summary>
            public GetColumnValue ColumnValueHandler { get; set; }

            public Func<string> ClientLabelIDHandler { get; set; }
            public Func<string> ClientHiddenIDHandler { get; set; }
            public Func<SelectColumnParameters> SelectInfoHandler { get; set; }
            public Func<SelectParameters> SelectParametersHandler { get; set; }

            /// <summary>
            /// ����� �� ������������� ������ ������� (������). �� ��������� true.
            /// </summary>
            public bool CanEdit { get; set; }

            /// <summary>
            /// ����� �������� �� �������. �� ��������� null. 
            /// ��� ���������� ������, ���� �������� null, �� ������� ����� Resources.SNotSpecified(�� �������).
            /// </summary>
            public string NullItemText { get; set; }

            /// <summary>
            /// ����� ��������� ������ �� ��������� �����.
            /// </summary>
            public bool IsVerticalHeaderText { get; set; }

            /// <summary>
            /// �������������� ������.
            /// </summary>
            public string Format { get; set; }

            /// <summary>
            /// ������������ ������ �� ��������� �������, ��� ���� ����� ��������� ���������� ������� �� ������������.
            /// </summary>
            public int ColSpan { get; set; }

            public double? MaxValue { get; set; }
            public double? MinValue { get; set; }

            public Func<int> GetRowSpan { get; set; } = () => 1;
            public Func<string> GetStyle { get; set; } = () => string.Empty;

            /// <summary>
            /// ������������ ������ �� ��������� ������� ��� �����������.
            /// </summary>
            public int ColGroupSpan { get; set; }

            /// <summary>
            /// ������������ ������ �� ��������� �������, ��� ���� ����� ��������� ���������� ������� �� ������������.
            /// ������������ ��� ������.
            /// </summary>
            public int TotalColSpan { get; set; }

            /// <summary>
            /// ������������ ������ �� ��������� �����.
            /// </summary>
            public int RowSpan
            {
                get => _rowSpan ?? GetRowSpan();
                set => _rowSpan = value;
            }

            /// <summary>
            /// ������ ������ � ������� ��������� ������ ������.
            /// </summary>
            public int RowIndex { get; set; }

            /// <summary>
            /// �������, ��� ������� ��������� � ���������, ������������ ��� ������������ ��������� �������.
            /// </summary>
            internal bool Added { get; set; }

            /// <summary>
            /// �������� ������ ��� ���������� ����������� ������, ������������ ��� ��������� DataSourceOther, ���� DataSourceOther == null.
            /// �.�. ���� DataSourceOther == null, �� ���������� ��� ���������� �� DataSource. ����� �������������� ��������� DataSourceOther.
            /// </summary>
            public IDataSource DataSource { get; set; }

            /// <summary>
            /// �������� ������ ��� ���������� ����������� ������. ���� ����� null, �� ����������� ������� DataSource. 
            /// ��� ��������������� ��������� ������������ ����� ...
            /// </summary>
            public IEnumerable<KeyValuePair<long, string>> DataSourceOther { get; set; }

            internal List<string> GroupParts
            {
                get
                {
                    if (groupParts == null)
                    {
                        groupParts = new List<string>();
                        if (string.IsNullOrEmpty(Group)) return groupParts;
                        int i = Group.IndexOf('#');
                        while (i > -1)
                        {
                            groupParts.Add(Group.Substring(0, i));
                            i = Group.IndexOf('#', i + 1);
                        }

                        groupParts.Add(Group);
                    }

                    return groupParts;
                }
            }

            public IList<ValidatorProperties> Validators { get; private set; }

            #region Implementation of IExportColumn

            public bool IsVerticalDataText { get; set; }

            public bool IsNumericColumn { get; set; }

            public decimal ExportWidth { get; set; }

            public bool ExportVisible { get; set; }

            public bool ExportHyperLink { get; set; }

            protected internal List<Column> ChildColumns { get; set; }

            bool IExportColumn.HasChild
            {
                get
                {
                    return ChildColumns != null && ChildColumns.Count > 0;
                }
            }

            bool IExportColumn.Visible
            {
                get { return ExportVisible; }
            }

            decimal IExportColumn.Width
            {
                get { return ExportWidth; }
            }

            string IExportColumn.GetValue(object row)
            {
                return GetValueForExport(row);
            }

            protected virtual string GetValueForExport(object row)
            {
                if (ColumnTextHandler != null)
                    return ColumnTextHandler();
                return null;
            }

            string IExportColumn.GetHyperLink(object row)
            {
                if (ExportHyperLink && ColumnHyperLinkHandler != null)
                {
                    var sb = new StringBuilder();
                    ColumnHyperLinkHandler(sb);
                    return sb.Length == 0 ? null : sb.ToString();
                }

                return null;
            }

            IEnumerable<IExportColumn> IExportColumn.GetChilds()
            {
                return ChildColumns.Cast<IExportColumn>();
            }

            #endregion

            /// <summary>
            /// ��������� ��� ������ DataSourceOther ��������.
            /// </summary>
            public void EnsureDataSourceOtherFilled()
            {
                if (DataSourceOther == null)
                    FillDataSourceOther();
            }

            /// <summary>
            /// ��������� ��� ������ DataSourceOther ��������.
            /// </summary>
            public void EnsureDataSourceOtherFilled(int maximumRows)
            {
                if (DataSourceOther == null)
                    FillDataSourceOther(maximumRows);
            }

            /// <summary>
            /// ��������� ����� DataSourceOther.
            /// </summary>
            public void FillDataSourceOther()
            {
                FillDataSourceOther(0);
            }

            /// <summary>
            /// ��������� ����� DataSourceOther.
            /// </summary>
            public void FillDataSourceOther(int maximumRows)
            {
                var list = new List<KeyValuePair<long, string>>();
                DataSourceView view = DataSource.GetView("default");
                var baseView = view as BaseDataSourceView<long>;
                if (baseView != null) baseView.CancelTreeUse = true;
                IEnumerable dataSourceData = null;
                var arguments = new DataSourceSelectArguments();
                if (maximumRows > 0)
                    arguments.MaximumRows = maximumRows;

                view.Select(arguments, delegate(IEnumerable data) { dataSourceData = data; });
                if (dataSourceData != null)
                {
                    var enumerator = dataSourceData.GetEnumerator();
                    PropertyInfo propertyID = null;
                    PropertyInfo propertyName = null;
                    if (enumerator.MoveNext())
                    {
                        var fieldName = FindHelper.GetContentFieldName("nameRu", "nameKz");
                        var row = enumerator.Current;
                        propertyID = row.GetType().GetProperty("id");
                        propertyName = row.GetType().GetProperty(fieldName);
                        list.Add(
                            new KeyValuePair<long, string>(
                                (long)propertyID.GetValue(row, null),
                                (propertyName.GetValue(row, null) ?? string.Empty).ToString()));
                    }

                    while (enumerator.MoveNext())
                    {
                        var row = enumerator.Current;
                        list.Add(
                            new KeyValuePair<long, string>(
                                (long)propertyID.GetValue(row, null),
                                (propertyName.GetValue(row, null) ?? string.Empty).ToString()));
                    }
                }

                DataSourceOther = list;
            }

            public string GetClientHiddenID()
            {
                if (ClientHiddenIDHandler == null)
                    return string.Empty;

                return ClientHiddenIDHandler();
            }

            public string GetClientLabelID()
            {
                if (ClientLabelIDHandler == null)
                    return string.Empty;

                return ClientLabelIDHandler();
            }

            public SelectColumnParameters GetSelectInfo()
            {
                if (SelectInfoHandler == null)
                    return null;

                return SelectInfoHandler();
            }

            private SelectParameters _selectParameters;

            public SelectParameters GetSelectParameters()
            {
                if (SelectParametersHandler == null)
                    return null;

                return _selectParameters ?? (_selectParameters = SelectParametersHandler());
            }
        }

        #endregion 

        #region ButtonsColumn

        public class ButtonsColumn : Column
        {
            private List<GetColumnContent> additionalButtons;

            public ButtonsColumn()
            {
                EditVisible = true;
                LookVisible = true;
                DeleteVisible = true;
                SelectVisible = true;
            }

            public string ConfirmDeleteText { get; set; } = Resources.SConfirmDeleteText;

            public Func<object> GetRenderID { get; set; }

            public bool EditVisible { get; set; }

            public bool LookVisible { get; set; }

            public bool DeleteVisible { get; set; }

            public bool HasSelectCheckBox { get; set; }

            public bool SelectVisible { get; set; }

            public GetColumnContent SelectParametersHandler { get; set; }

            public List<GetColumnContent> AdditionalButtons
            {
                get
                {
                    return additionalButtons ?? (additionalButtons = new List<GetColumnContent>());
                }

                set
                {
                    additionalButtons = value;
                }
            }

            public virtual int CountIcons
            {
                get
                {
                    return (SelectVisible ? 1 : 0)
                        + (EditVisible ? 1 : 0)
                        + (LookVisible ? 1 : 0)
                        + (DeleteVisible ? 1 : 0)
                        + (HasSelectCheckBox ? 1 : 0)
                        + (AdditionalButtons?.Count ?? 0);
                }
            }

            public override string Width => CountIcons * 21 + "px";

            public virtual IEnumerable<GetColumnContent> GetAdditionalButtons()
            {
                return additionalButtons ?? (IEnumerable<GetColumnContent>)new GetColumnContent[0];
            }

            public void RenderDeleteButton(Control control, StringBuilder sb)
            {
                sb.Append("<a href=\"javascript:if(confirm('");
                sb.Append(HttpUtility.HtmlAttributeEncode(ConfirmDeleteText));
                sb.Append("')) ");
                sb.Append(HttpUtility.HtmlAttributeEncode(control.Page.ClientScript.GetPostBackEventReference(new PostBackOptions(control, "delete:" + GetRenderID()))));
                sb.Append(";\" tabindex=\"-1\" title=\"");
                sb.Append(HttpUtility.HtmlAttributeEncode(Resources.SDeleteText));
                sb.Append("\"><img style=\"border:0px\" src=\"");
                sb.Append(Themes.IconUrlDelete);
                sb.Append("\"/></a>");
            }

            public override GetColumnContent ColumnContentHandler
            {
                set
                {
                    base.ColumnContentHandler = sb =>
                        {
                            sb.AppendFormat("<div style='width:{0}'>", Width);
                            value(sb);
                            sb.Append("</div>");
                        };
                }
            }
        }

        #endregion
    }
}