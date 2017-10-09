using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Globalization;
using System.Resources;
using System.Web;
using System.Web.Script.Serialization;
using System.Web.UI;
using System.Xml.Serialization;
using AjaxControlToolkit;
using Nat.Web.Controls.GenerationClasses.BaseJournal;
using Nat.Web.Controls.Properties;
using Nat.Web.Tools;

namespace Nat.Web.Controls.GenerationClasses.HierarchyFields
{
    using System.Runtime.Remoting.Messaging;

    [Serializable]
    public class ColumnHierarchy
    {
        private string _clientID;
        private bool _visible;

        public ColumnHierarchy()
        {
            Childs = new List<ColumnHierarchy>();
            Visible = true;
        }

        public ColumnHierarchy(ResourceManager resourceManager, string headerResourceKey, string columnName, ColumnHierarchyDirection direction, bool visible, ColumnAggregateType aggregateType, bool hideInHeader, bool isVerticalHeader, int width, params ColumnHierarchy[] childs)
        {
            ResourceManager = resourceManager;
            HeaderResourceKey = headerResourceKey;
            ColumnName = columnName;
            Direction = direction;
            Visible = visible;
            AggregateType = aggregateType;
            HideInHeader = hideInHeader;
            IsVerticalHeader = isVerticalHeader;
            Width = width;
            Childs = new List<ColumnHierarchy>(childs);

        }

        public ColumnHierarchy(ResourceManager resourceManager, string headerResourceKey, string columnName, ColumnHierarchyDirection direction, bool visible, ColumnAggregateType aggregateType, bool hideInHeader, params ColumnHierarchy[] childs)
            : this(resourceManager, headerResourceKey, columnName, direction, visible, aggregateType, hideInHeader, false, 0, childs)
        {
        }

        public ColumnHierarchy(ResourceManager resourceManager, string headerResourceKey, string columnName, ColumnHierarchyDirection direction, bool visible, ColumnAggregateType aggregateType, params ColumnHierarchy[] childs)
            : this(resourceManager, headerResourceKey, columnName, direction, visible, aggregateType, false, childs)
        {
        }

        public ColumnHierarchy(ResourceManager resourceManager, string headerResourceKey, string columnName, ColumnHierarchyDirection direction, bool visible, params ColumnHierarchy[] childs)
            : this(resourceManager, headerResourceKey, columnName, direction, visible, ColumnAggregateType.None, childs)
        {
        }

        public ColumnHierarchy(ResourceManager resourceManager, string headerResourceKey, string columnName, ColumnHierarchyDirection direction, params ColumnHierarchy[] childs)
            : this(resourceManager, headerResourceKey, columnName, direction, true, childs)
        {
        }

        public string HeaderResourceKey { get; set; }
        public string ColumnName { get; set; }
        public string GroupColumnName { get; set; }
        public string CrossColumnID { get; set; }

        public ColumnHierarchyDirection Direction { get; set; }
        public List<ColumnHierarchy> Childs { get; set; }
        //public List<ColumnHierarchy> CrossColumns { get; set; }
        public string CrossColumnKey { get; set; }
        public bool IsVerticalHeader { get; set; }
        public ColumnAggregateType AggregateType { get; set; }
        public bool HideInHeader { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }
        public string BColor { get; set; }
        public string PColor { get; set; }
        public string HyperLinkUrl { get; set; }
        public string HyperLinkTarget { get; set; }
        public string HyperLinkOnClick { get; set; }
        [ScriptIgnore]
        [XmlIgnore]
        public BaseColumn.GroupKeys CrossDataItemKey { get; set; }
        [ScriptIgnore]
        [XmlIgnore]
        public bool Delete { get; set; }

        public bool Visible
        {
            get
            {
                if (Parent == null)
                    return _visible;
                return _visible && Parent.Visible;
            }
            set { _visible = value; }
        }

        [XmlIgnore]
        public int ClientRowSpan { get; set; }
        [XmlIgnore]
        public int ClientColSpan { get; set; }

        [XmlIgnore]
        public string ColumnKey
        {
            get
            {
                return ColumnName ?? CrossColumnKey ?? string.Empty;
            }
        }

        [XmlIgnore]
        public bool AllowAggregate 
        {
            get
            {
                var columns = JournalControl.BaseInnerHeader.ColumnsDic;
                if (columns.ContainsKey(ColumnKey))
                    return columns[ColumnKey].AllowAggregate;
                return false;
            }
        }

        [XmlIgnore]
        public bool AllowAggregateSeted
        {
            get
            {
                var columns = JournalControl.BaseInnerHeader.ColumnsDic;
                if (columns.ContainsKey(ColumnKey))
                    return columns[ColumnKey].AllowAggregateSeted;
                return false;
            }
        }

        [ScriptIgnore]
        [XmlIgnore]
        public BaseJournalControl JournalControl { get; set; }

        private bool? _visibleInClient;

        [XmlIgnore]
        public bool VisibleInClient
        {
            get
            {
                if (_visibleInClient != null)
                    return _visibleInClient.Value;

                if (string.IsNullOrEmpty(ColumnName)) return true;
                
                var groupColumn = JournalControl.GroupColumns.FirstOrDefault(r => ColumnName.Equals(r));
                if (groupColumn != null && groupColumn.GroupType == GroupType.InHeader) return false;
                
                if (JournalControl.BaseInnerHeader.ColumnsDic.ContainsKey(ColumnName)
                    && JournalControl.BaseInnerHeader.ColumnsDic[ColumnName].InlineGrouping)
                {
                    return groupColumn != null;
                }

                if (groupColumn != null
                    && (groupColumn.GroupType == GroupType.Top || groupColumn.GroupType == GroupType.TopTotal
                        || groupColumn.GroupType == GroupType.Total))
                {
                    return false;
                }

                return true;
            }
            set { _visibleInClient = value; }
        }

        [XmlIgnore]
        public string ClientID
        {
            get
            {
                if (_clientID == null)
                {
                    _clientID = !string.IsNullOrEmpty(ColumnKey)
                                    ? JournalControl.ClientID + "_th_" + ColumnKey + "_" + Guid.NewGuid()
                                    : JournalControl.ClientID + "_th_" + Guid.NewGuid();
                }

                return _clientID;
            }
        }

        [XmlIgnore]
        public string OrderByColumn
        {
            get 
            {
                var column = string.IsNullOrEmpty(ColumnName) ? null : JournalControl.BaseInnerHeader.ColumnsDic[ColumnName];
                if (column != null)
                {
                    if (column.OrderColumnNameKz != null && LocalizationHelper.IsCultureKZ)
                        return column.OrderColumnNameKz;
                    if (column.OrderColumnNameRu != null)
                        return column.OrderColumnNameRu;
                    if (column.OrderColumnName != null)
                        return column.OrderColumnName;
                }
                return "";
            }
        }

        [ScriptIgnore]
        [XmlIgnore]
        public object CrossColumnIDObject { get; set; }
     
        [ScriptIgnore]
        [XmlIgnore]
        public object CrossColumnHeaderRow { get; set; }

        [XmlIgnore]
        [ScriptIgnore]
        public bool CrossColumnsHierarchyCreated { get; set; }

        [ScriptIgnore]
        [XmlIgnore]
        public ColumnHierarchy Parent { get; internal set; }

        [ScriptIgnore]
        [XmlIgnore]
        public int RowSpan { get; set; }

        [ScriptIgnore]
        [XmlIgnore]
        public int ColSpan { get; set; }

        [ScriptIgnore]
        [XmlIgnore]
        public bool IsTreeColumn { get; set; }

        /// <summary>
        /// Показывать ссылка на переход по дереву
        /// </summary>
        [ScriptIgnore]
        [XmlIgnore]
        public bool HideTreeLink { get; set; }

        [ScriptIgnore]
        [XmlIgnore]
        public BaseColumn BaseColumn { get; set; }
        
        [ScriptIgnore]
        [XmlIgnore]
        public string StyleId { get; set; }

        [ScriptIgnore]
        [XmlIgnore]
        public ResourceManager ResourceManager { get; private set; }
        
        public IEnumerable<string> GetColumnNames()
        {
            var childs = GetChilds();
            if (string.IsNullOrEmpty(ColumnName) && childs.Count() == 0)
                return new[] {"EmptyCell"};
            if (string.IsNullOrEmpty(ColumnName))
                return childs.SelectMany(r => r.GetColumnNames());
            if (childs.Count() == 0) return new[] { ColumnName };
            return new[] { ColumnName }.Union(childs.SelectMany(r => r.GetColumnNames()).Where(r => !r.Equals("EmptyCell")));
        }

        public IEnumerable<string> GetVisibleColumnNames(Dictionary<string, BaseColumn> columnsDic)
        {
            var childs = GetChilds().Where(r => r.IsVisibleColumn(columnsDic));
            if (string.IsNullOrEmpty(ColumnName) && childs.Count() == 0)
                return new[] { "EmptyCell" };
            if (string.IsNullOrEmpty(ColumnName))
                return childs.SelectMany(r => r.GetVisibleColumnNames(columnsDic));
            if (childs.Count() == 0) return new[] { ColumnName };
            return new[] { ColumnName }.Union(childs.SelectMany(r => r.GetVisibleColumnNames(columnsDic)).Where(r => !r.Equals("EmptyCell")));
        }

        public IEnumerable<ColumnHierarchy> GetVisibleColumns(Dictionary<string, BaseColumn> columnsDic)
        {
            var childs = GetChilds().Where(r => r.IsVisibleColumn(columnsDic));
            if (string.IsNullOrEmpty(ColumnName) && childs.Count() == 0)
                return new ColumnHierarchy[0];
            if (string.IsNullOrEmpty(ColumnName))
                return childs.SelectMany(r => r.GetVisibleColumns(columnsDic));
            if (childs.Count() == 0) return new[] { this };
            return new[] { this }.Union(childs.SelectMany(r => r.GetVisibleColumns(columnsDic)));
        }
        
        public bool ShowHeaderText { get; set; }
        public string HeaderTextRu { get; set; }
        public string HeaderTextKz { get; set; }

        [XmlIgnore]
        public string Header
        {
            get
            {
                if (string.IsNullOrEmpty(HeaderResourceKey) || ShowHeaderText)
                    return LocalizationHelper.IsCultureKZ ? HeaderTextKz : HeaderTextRu;
                return ResourceManager.GetString(HeaderResourceKey);
            }
            set
            {
                if (!ShowHeaderText) return;
                if (LocalizationHelper.IsCultureKZ)
                {
                    HeaderTextKz = value;
                    if (string.IsNullOrEmpty(HeaderTextRu))
                    {
                        HeaderTextRu = value;
                    }
                }
                else
                {
                    HeaderTextRu = value;
                    if (string.IsNullOrEmpty(HeaderTextKz))
                    {
                        HeaderTextKz = value;
                    }
                }
            }
        }

        public string ToolTipRu { get; set; }
        public string ToolTipKz { get; set; }

        [XmlIgnore]
        [ScriptIgnore]
        internal int Order { get; set; }

        public string GetHeader(string culture)
        {
            if (string.IsNullOrEmpty(HeaderResourceKey))
                return "kk-kz".Equals(culture, StringComparison.OrdinalIgnoreCase) ? HeaderTextKz : HeaderTextRu;
            return ResourceManager.GetString(HeaderResourceKey, new CultureInfo(culture));
        }

        public string GetHeader(CultureInfo culture)
        {
            if (string.IsNullOrEmpty(HeaderResourceKey))
                return "kk-kz".Equals(culture.Name, StringComparison.OrdinalIgnoreCase) ? HeaderTextKz : HeaderTextRu;
            return ResourceManager.GetString(HeaderResourceKey, culture);            
        }

        public IEnumerable<ColumnHierarchy> GetChilds()
        {
            return Childs;
        }

        public bool IsEmptyColumn()
        {
            return string.IsNullOrEmpty(ColumnName);
        }

        public bool IsVisibleColumn(Dictionary<string, BaseColumn> columnsDic)
        {
            return Visible && (string.IsNullOrEmpty(ColumnName) || columnsDic[ColumnName].VisibleColumn)
                   && (Parent == null || Parent.IsVisibleColumn(columnsDic));
        }

        public void DetectColSpan(Dictionary<string, BaseColumn> columnsDic)
        {
            if (!IsVisibleColumn(columnsDic))
            {
                ColSpan = 0;
                return;
            }
            if (Childs.Count == 0)
            {
                if (!HideInHeader)
                    ColSpan = 1;
                return;
            }
            foreach (var child in Childs)
                child.DetectColSpan(columnsDic);
            ColSpan = Childs.Sum(r => r.ColSpan);
            if (ColSpan == 0 && !HideInHeader)
                ColSpan = 1;
        }

        public bool DetectRowSpan(int maxRowSpan, Dictionary<string, BaseColumn> columnsDic)
        {
            if (!IsVisibleColumn(columnsDic))
            {
                RowSpan = 0;
                return false;
            }

            if (Childs.Count == 0)
            {
                if (HideInHeader)
                    return false;
                RowSpan = maxRowSpan;
            }
            else
            {
                var childInited = false;
                var nextRowSpan = HideInHeader ? maxRowSpan : GetLevelsCount(columnsDic) - 1;
                foreach (var child in Childs)
                    childInited |= child.DetectRowSpan(nextRowSpan, columnsDic);

                if (!HideInHeader)
                    RowSpan = childInited ? maxRowSpan - GetLevelsCount(columnsDic) + 1 : maxRowSpan;
            }
            return true;
        }

        public void DetectClientColSpan(Dictionary<string, BaseColumn> columnsDic)
        {
            if (Childs.Count == 0)
                ClientColSpan = 1;
            else
            {
                foreach (var child in Childs)
                    child.DetectClientColSpan(columnsDic);
                ClientColSpan = Childs.Sum(r => r.ClientColSpan);
            }
        }

        public void EnsureCrossColumnsHierarchy(Dictionary<string, BaseColumn> columnsDic)
        {
            if (!CrossColumnsHierarchyCreated)
            {
                CrossColumnsHierarchyCreated = true;
                CreateCrossColumnsHierarchy(columnsDic);
            }
        }

        public void CreateCrossColumnsHierarchy(Dictionary<string, BaseColumn> columnsDic)
        {
            if (!IsEmptyColumn() && columnsDic.ContainsKey(ColumnName))
            {
                var column = columnsDic[ColumnName];
                if (column.IsCrossColumn)
                {
                    column.BaseCrossColumnDataSource.CreateHierarchy(this, Childs, columnsDic);
                    Width = 0;
                    //HideInHeader = true;
                }
            }
            foreach (var child in Childs)
            {
                child.Init();
                child.CreateCrossColumnsHierarchy(columnsDic);
            }
        }

        public bool DetectClientRowSpan(int maxRowSpan, Dictionary<string, BaseColumn> columnsDic)
        {
            if (GetChilds().Count() == 0)
                ClientRowSpan = maxRowSpan;
            else
            {
                var childInited = false;
                var nextRowSpan = GetClientLevelsCount(columnsDic) - 1;
                foreach (var child in GetChilds())
                    childInited |= child.DetectClientRowSpan(nextRowSpan, columnsDic);
                ClientRowSpan = childInited ? maxRowSpan - GetClientLevelsCount(columnsDic) + 1 : maxRowSpan;
            }
            return true;
        }

        public int GetClientLevelsCount(Dictionary<string, BaseColumn> columnsDic)
        {
            var childs = GetChilds();
            if (!VisibleInClient)
                return 0;
            if (childs.Count() == 0)
            {
                if (!IsEmptyColumn())
                {
                    var column = columnsDic[ColumnName];
                    if (column.IsCrossColumn)
                    {
                        if (column.BaseCrossColumnDataSource.MaxLevel == 0)
                            return 1;
                        return column.BaseCrossColumnDataSource.MaxLevel;
                    }
                    /*{
                        return CrossColumns.Count == 0
                                   ? 0
                                   : CrossColumns.Max(r => r.GetClientLevelsCount(columnsDic));
                    }*/
                }
                return 1;
            }
            return childs.Max(r => r.GetClientLevelsCount(columnsDic)) + 1;
        }

        public int GetLevelsCount(Dictionary<string, BaseColumn> columnsDic)
        {
            if (!IsVisibleColumn(columnsDic))
                return 0;
            if (Childs.Count == 0)
            {
                if (!IsEmptyColumn())
                {
                    var column = columnsDic[ColumnName];
                    if (column.IsCrossColumn)
                        return column.BaseCrossColumnDataSource.MaxLevel;
                }

                return HideInHeader ? 0 : 1;
            }

            return Childs.Max(r => r.GetLevelsCount(columnsDic)) + (HideInHeader ? 0 : 1);
        }

        public void Init(ResourceManager resourceManager, BaseJournalControl journalControl)
        {
            JournalControl = journalControl;
            if (ResourceManager == null)
                ResourceManager = resourceManager;
            foreach (var child in GetChilds())
            {
                child.Parent = this;
                child.Init(resourceManager, journalControl);
            }
        }

        public void Init()
        {
            foreach (var child in GetChilds())
            {
                child.Parent = this;
                child.Init();
            }
        }

        public void Init(ColumnHierarchy columnHierarchy)
        {
            Visible = columnHierarchy._visible;
            AggregateType = columnHierarchy.AggregateType;
            IsVerticalHeader = columnHierarchy.IsVerticalHeader;
            Direction = columnHierarchy.Direction;
            HideInHeader = columnHierarchy.HideInHeader;
            Width = columnHierarchy.Width;
            Height = columnHierarchy.Height;
            Order = columnHierarchy.Order;
            BColor = columnHierarchy.BColor;
            PColor = columnHierarchy.PColor;
        }

        public void PreRender(Dictionary<string, BaseColumn> columnsDic, BaseJournalControl journalControl)
        {
            /*if (!HideInHeader && !string.IsNullOrEmpty(ColumnKey))
            {
                var clientID = journalControl.ClientID + "_th_" + ColumnKey;
                var extender = new ResizableControlExtender
                                   {
                                       MinimumWidth = 0,
                                       MinimumHeight = 0,
                                       MaximumWidth = 2000,
                                       MaximumHeight = 400,
                                       BehaviorID = clientID + "_b",
                                       HandleCssClass = "HandleCssClass",
                                       ResizableCssClass = "ResizableCssClass",
                                       EnableClientState = false,
                                       HandleOffsetX = 3,
                                       HandleOffsetY = 3,
                                   };
                journalControl.ParentUserControl.ExtenderAjaxControl.AddExtender(extender, clientID);
            }
            foreach (var child in GetChilds().Where(r => r.IsVisibleColumn(columnsDic)))
                child.PreRender(columnsDic, journalControl);*/
        }

        public void Render(HtmlTextWriter writer, int level, int inLevel, int maxRowSpan, Dictionary<string, BaseColumn> columnsDic, BaseJournalControl journalControl, List<RowProperties> rowsPropertieses)
        {
            if (level == 0 && !HideInHeader)
            {
                int height = 0;
                for (int i = inLevel; i < inLevel + RowSpan; i++)
                {
                    if (rowsPropertieses.Count <= i || rowsPropertieses[i].Height == null || rowsPropertieses[i].Height == 0)
                    {
                        height = 0;
                        break;
                    }
                    height += rowsPropertieses[i].Height.Value;
                    if (i > inLevel) height += 11;
                }
                writer.WriteLine();
                writer.AddAttribute("onmousemove", "changeWidthOnCellMouseMove(event, this);");
                writer.AddAttribute("onmousedown", "changeWidthCellMouseDown(event, this);");
                writer.AddAttribute(HtmlTextWriterAttribute.Rowspan, RowSpan.ToString());
                writer.AddAttribute(HtmlTextWriterAttribute.Colspan, ColSpan.ToString());
                //writer.AddAttribute(HtmlTextWriterAttribute.Align, "left");
                writer.AddAttribute(HtmlTextWriterAttribute.Id, ClientID);
                if (Width > 0) writer.AddStyleAttribute(HtmlTextWriterStyle.Width, Width + "px");
                if (height > 0) writer.AddStyleAttribute(HtmlTextWriterStyle.Height, height + "px");
                if (!string.IsNullOrEmpty(BColor))
                    writer.AddStyleAttribute(HtmlTextWriterStyle.BackgroundColor, BColor);
                if (!string.IsNullOrEmpty(PColor))
                    writer.AddStyleAttribute(HtmlTextWriterStyle.Color, PColor);
                writer.RenderBeginTag(HtmlTextWriterTag.Th);//th

                var isIE = "IE".Equals(HttpContext.Current.Request.Browser.Browser, StringComparison.OrdinalIgnoreCase)
                         && string.Compare("9.0", HttpContext.Current.Request.Browser.Version, StringComparison.OrdinalIgnoreCase) > 0;

                if (IsVerticalHeader && !isIE)
                {
                    if (Width > 0) writer.AddStyleAttribute(HtmlTextWriterStyle.Height, Width + "px");
                    if (height > 0) writer.AddStyleAttribute(HtmlTextWriterStyle.Width, height + "px");
                    if (Width > 0 && height > 0)
                    {
                        writer.AddStyleAttribute("margin-top", (height - Width) / 2 + "px");
                        writer.AddStyleAttribute("margin-bottom", (height - Width) / 2 + "px");
                        writer.AddStyleAttribute("margin-left", (Width - height) / 2 + "px");
                        writer.AddStyleAttribute("margin-right", (Width - height) / 2 + "px");
                        writer.AddAttribute(HtmlTextWriterAttribute.Class, "DefaultRotate270Deg");
                    }

                    writer.AddStyleAttribute(HtmlTextWriterStyle.TextAlign, "left");
                }
                else
                {
                    if (Width > 0) writer.AddStyleAttribute(HtmlTextWriterStyle.Width, Width + "px");
                    if (height > 0) writer.AddStyleAttribute(HtmlTextWriterStyle.Height, height + "px");
                    if (IsVerticalHeader)
                        writer.AddAttribute(HtmlTextWriterAttribute.Class, "IE8Rotate270Deg");
                }

                if (Width > 0 || height > 0) writer.AddStyleAttribute(HtmlTextWriterStyle.Overflow, "hidden");
                if (!string.IsNullOrEmpty(ToolTipRu) && !LocalizationHelper.IsCultureKZ)
                    writer.AddAttribute(HtmlTextWriterAttribute.Title, ToolTipRu);
                else if (!string.IsNullOrEmpty(ToolTipKz) && LocalizationHelper.IsCultureKZ)
                    writer.AddAttribute(HtmlTextWriterAttribute.Title, ToolTipKz);
                writer.RenderBeginTag(HtmlTextWriterTag.Div);//div

                if (BaseColumn == null && CrossColumnIDObject != null && IsTreeColumn && !HideTreeLink)//если это иерархичная кросс колонка
                {
                    var postBack = journalControl.Page.ClientScript.GetPostBackClientHyperlink(
                        journalControl,
                        string.Format("FilterBy:{0}:Equals:{1}:{2}",
                                      BaseFilterParameter.TreeStartLevelFilterName + CrossColumnHeaderRow.GetType().Name,
                                      CrossColumnIDObject,
                                      Header));
                    writer.AddAttribute(HtmlTextWriterAttribute.Href, postBack);
                    writer.AddAttribute(HtmlTextWriterAttribute.Title, Resources.SFilterHeader);
                    writer.RenderBeginTag(HtmlTextWriterTag.A);
                    RenderHeaderText(writer);
                    writer.RenderEndTag();
                }
                else if (!string.IsNullOrEmpty(HyperLinkUrl) || !string.IsNullOrEmpty(HyperLinkOnClick))
                {

                    var url = string.IsNullOrEmpty(HyperLinkUrl) ? "javascript:void(0);" : HyperLinkUrl;
                    var target = string.IsNullOrEmpty(HyperLinkTarget) ? "_blank" : HyperLinkTarget;
                    writer.AddAttribute(HtmlTextWriterAttribute.Href, url);
                    writer.AddAttribute(HtmlTextWriterAttribute.Target, target);
                    if (!string.IsNullOrEmpty(HyperLinkOnClick))
                        writer.AddAttribute(HtmlTextWriterAttribute.Onclick, HyperLinkOnClick);
                    writer.RenderBeginTag(HtmlTextWriterTag.A);
                    RenderHeaderText(writer);
                    writer.RenderEndTag();//A
                }
                else
                    RenderHeaderText(writer);

                writer.RenderEndTag();//div

                /*var column = string.IsNullOrEmpty(ColumnName) ? null : columnsDic[ColumnName];
                if (column != null && false)
                {
                    if (column.OrderColumnNameRu != null && !LocalizationHelper.IsCultureKZ)
                        RenderHeaderOrder(writer, column.OrderColumnNameRu, journalControl);
                    else if (column.OrderColumnNameKz != null && LocalizationHelper.IsCultureKZ)
                        RenderHeaderOrder(writer, column.OrderColumnNameKz, journalControl);
                    if (column.OrderColumnName != null)
                        RenderHeaderOrder(writer, column.OrderColumnName, journalControl);
                }*/

                writer.RenderEndTag();//Th
            }
            foreach (var child in GetChilds().Where(r => r.IsVisibleColumn(columnsDic)))
                child.Render(writer, level - RowSpan, inLevel + RowSpan, maxRowSpan - RowSpan, columnsDic, journalControl, rowsPropertieses);
        }

        private void RenderHeaderOrder(HtmlTextWriter writer, string orderByColumn, BaseJournalControl journalControl)
        {
            string argument;
            string postBack;
            string title;
            string src;

            writer.AddStyleAttribute("writing-mode", "");
            writer.AddStyleAttribute("filter", "");
            writer.RenderBeginTag(HtmlTextWriterTag.Div);

            #region order by column asc

            if (journalControl.OrderByColumns.Contains(orderByColumn))
            {
                title = Resources.SOrderByColumnRemove;
                argument = string.Format(BaseJournalControl.OrderByColumnRemove, orderByColumn);
                src = Themes.IconUrlOrderByColumnAscSelected;
            }
            else
            {
                title = Resources.SOrderByColumn;
                argument = string.Format(BaseJournalControl.OrderByColumnAsc, orderByColumn);
                src = Themes.IconUrlOrderByColumnAsc;
            }
            postBack = journalControl.Page.ClientScript.GetPostBackClientHyperlink(journalControl, argument);
            RenderAImage(writer, postBack, title, src);

            #endregion

            #region order by column desc

            if (journalControl.OrderByColumns.Contains(orderByColumn + " desc"))
            {
                title = Resources.SOrderByColumnRemove;
                argument = string.Format(BaseJournalControl.OrderByColumnRemove, orderByColumn);
                src = Themes.IconUrlOrderByColumnDescSelected;
            }
            else
            {
                title = Resources.SOrderByColumnDesc;
                argument = string.Format(BaseJournalControl.OrderByColumnDesc, orderByColumn);
                src = Themes.IconUrlOrderByColumnDesc;
            }
            postBack = journalControl.Page.ClientScript.GetPostBackClientHyperlink(journalControl, argument);
            RenderAImage(writer, postBack, title, src);

            #endregion

            writer.RenderEndTag(); //DIV
        }

        private void RenderAImage(HtmlTextWriter writer, string postBack, string title, string src)
        {
            writer.AddAttribute(HtmlTextWriterAttribute.Href, postBack);
            writer.AddAttribute(HtmlTextWriterAttribute.Title, title);
            writer.RenderBeginTag(HtmlTextWriterTag.A);

            #region image

            writer.AddAttribute(HtmlTextWriterAttribute.Alt, title);
            writer.AddAttribute(HtmlTextWriterAttribute.Src, src);
            writer.AddAttribute(HtmlTextWriterAttribute.Border, "0");
            writer.RenderBeginTag(HtmlTextWriterTag.Img);
            writer.RenderEndTag();

            #endregion

            writer.RenderEndTag();
        }

        private void RenderHeaderText(HtmlTextWriter writer)
        {
            if (string.IsNullOrEmpty(Header))
                writer.Write("Empty header");
            else
                writer.Write(HttpUtility.HtmlEncode(Header));

            if (BaseColumn != null && BaseColumn.CustomRenderInHeader != null)
                BaseColumn.CustomRenderInHeader(this, writer);
            else if (Childs.Count == 1 && Childs[0].HideInHeader && Childs[0].BaseColumn != null && Childs[0].BaseColumn.CustomRenderInHeader != null)
                Childs[0].BaseColumn.CustomRenderInHeader(this, writer);
        }

        public IEnumerable<ColumnHierarchy> SelectAll()
        {
            if (string.IsNullOrEmpty(ColumnKey))
                return GetChilds().SelectMany(r => r.SelectAll());
            return new[] { this }.Union(GetChilds().SelectMany(r => r.SelectAll()));
        }

        public void InitColumnParams(Dictionary<string, BaseColumn> columnsDic)
        {
            if (!IsEmptyColumn())
                columnsDic[ColumnKey].UserAggregateType = AggregateType;
            foreach (var child in GetChilds())
                child.InitColumnParams(columnsDic);
        }

        public void InitCrossColumnsHierarchy(Dictionary<string, BaseColumn> columnsDic)
        {
            CrossColumnsHierarchyCreated = false;
            EnsureCrossColumnsHierarchy(columnsDic);
        }

        public void CheckVisible(Dictionary<string, BaseColumn> columnsDic)
        {
            if (Visible && (IsEmptyColumn() || columnsDic[ColumnName].IsCrossColumn))
            {
                if (!ContainsVisibleColumns(columnsDic))
                    Visible = false;
                else
                    foreach (var item in Childs)
                        item.CheckVisible(columnsDic);
            }
        }

        private bool ContainsVisibleColumns(Dictionary<string, BaseColumn> columnsDic)
        {
            return Childs.FirstOrDefault(
                r => (r.Visible && !r.IsEmptyColumn() && !columnsDic[r.ColumnName].IsCrossColumn)
                     || r.ContainsVisibleColumns(columnsDic)) != null;
        }

        public string FullHeaderRu()
        {
            if (Parent != null)
                return Parent.FullHeaderRu() + " / " + GetHeader(new CultureInfo("ru-ru"));
            return GetHeader(new CultureInfo("ru-ru"));
        }
    }
}
