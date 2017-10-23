using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.UI;
using System.Web.UI.WebControls;
using AjaxControlToolkit;
using Nat.Web.Controls.Properties;

namespace Nat.Web.Controls.GenerationClasses.BaseJournal
{
    using Nat.Web.Tools;

    public class BaseGroupingControl : WebControl
    {
        public override bool EnableViewState
        {
            get
            {
                return false;
            }
        }

        public bool HideGroupControls { get; set; }
    }

    public class BaseGroupingControl<TRow> : BaseGroupingControl
        where TRow : BaseRow
    {
        private ScriptManager _scriptManager;
        public ScriptManager ScriptManager => _scriptManager ?? (_scriptManager = ScriptManager.GetCurrent(Page));

        public BaseJournalControl<TRow> Journal { get; set; }
        protected BaseJournalHeaderControl<TRow> InnerHeader { get { return Journal.InnerHeader; } }
        protected List<GroupColumn> GroupColumns { get { return Journal.GroupColumns; } }

        public override void RenderControl(HtmlTextWriter writer)
        {
            writer.AddStyleAttribute(HtmlTextWriterStyle.Width, "100%");
            writer.AddStyleAttribute(HtmlTextWriterStyle.ZIndex, "11");
            writer.AddStyleAttribute(HtmlTextWriterStyle.Position, "relative");
            //writer.AddStyleAttribute("float", "right");
            writer.RenderBeginTag(HtmlTextWriterTag.Div);
            var enabled = InnerHeader.Columns.Any(r => r.AllowGrouping) && Enabled;
            if (!enabled)
                writer.AddAttribute(HtmlTextWriterAttribute.Disabled, "disabled");
            writer.AddStyleAttribute(HtmlTextWriterStyle.PaddingTop, "4px");
            writer.AddStyleAttribute(HtmlTextWriterStyle.PaddingBottom, "4px");
            writer.AddStyleAttribute(HtmlTextWriterStyle.BackgroundColor, "white");
            writer.RenderBeginTag(HtmlTextWriterTag.Fieldset);

            #region Legend

            writer.RenderBeginTag(HtmlTextWriterTag.Legend);
            
            #region legend text

            if (HideGroupControls)
                writer.AddStyleAttribute(HtmlTextWriterStyle.Cursor, "default");
            else
            {
                writer.AddStyleAttribute(HtmlTextWriterStyle.Cursor, "pointer");
                writer.AddAttribute(HtmlTextWriterAttribute.Title, Resources.PressToOpenCloseGroup);
            }

            writer.AddAttribute(HtmlTextWriterAttribute.Id, ClientID + "_Legend");
            writer.RenderBeginTag(HtmlTextWriterTag.Span);
            writer.Write(Resources.SGroupOfJournal);
            writer.RenderEndTag();
            #endregion

            #region Убрать все группы
            if (GroupColumns.FirstOrDefault(r => r.GroupType != GroupType.InHeader) != null && Enabled)
            {
                writer.Write("&nbsp;");
                var postBack = Page.ClientScript.GetPostBackClientHyperlink(
                    Journal, BaseJournalControl.GroupByClear);
                writer.AddAttribute(HtmlTextWriterAttribute.Href, postBack);
                writer.AddAttribute(HtmlTextWriterAttribute.Title, Resources.SRemoveAllGroups);
                writer.RenderBeginTag(HtmlTextWriterTag.A);
                writer.AddAttribute(HtmlTextWriterAttribute.Src, Themes.IconUrlSmallRemove);
                writer.AddAttribute(HtmlTextWriterAttribute.Alt, Resources.SRemoveAllGroups);
                writer.AddAttribute(HtmlTextWriterAttribute.Border, "0");
                writer.AddStyleAttribute(HtmlTextWriterStyle.MarginTop, "-4px");
                writer.AddStyleAttribute(HtmlTextWriterStyle.Top, "4px");
                writer.AddStyleAttribute(HtmlTextWriterStyle.Position, "relative");
                writer.RenderBeginTag(HtmlTextWriterTag.Img);
                writer.RenderEndTag();
                writer.RenderEndTag();
            }

            #endregion 

            writer.RenderEndTag();

            #endregion

            writer.AddAttribute(HtmlTextWriterAttribute.Id, ClientID);
            if (HideGroupControls)
                writer.AddStyleAttribute(HtmlTextWriterStyle.Display, "none");
            writer.RenderBeginTag(HtmlTextWriterTag.Div);
            writer.RenderBeginTag(HtmlTextWriterTag.Table);

            #region колонки по которым группируется журнал

            for (int i = 0; i < GroupColumns.Count; i++)
            {
                var column = InnerHeader.ColumnsDic[GroupColumns[i].ColumnName];
                if (!column.VisibleInGroup) continue;

                writer.RenderBeginTag(HtmlTextWriterTag.Tr);

                #region заголовок колонки

                writer.RenderBeginTag(HtmlTextWriterTag.Td);
                writer.RenderBeginTag(HtmlTextWriterTag.B);
                writer.Write(column.Header);
                writer.RenderEndTag();
                writer.RenderEndTag();

                #endregion

                #region уменьшить вес группировки

                writer.RenderBeginTag(HtmlTextWriterTag.Td);
                if (i + 1 < GroupColumns.Count && Enabled)
                {
                    var postBack = Page.ClientScript.GetPostBackClientHyperlink(
                        Journal, string.Format(BaseJournalControl.GroupByColumnMoveDown, column.ColumnName));
                    writer.AddAttribute(HtmlTextWriterAttribute.Href, postBack);
                    writer.AddAttribute(HtmlTextWriterAttribute.Title, Resources.SDownGroup);
                    writer.RenderBeginTag(HtmlTextWriterTag.A);
                    writer.AddAttribute(HtmlTextWriterAttribute.Src, Themes.IconUrlSmallArrowDown);
                    writer.AddAttribute(HtmlTextWriterAttribute.Border, "0");
                    writer.AddAttribute(HtmlTextWriterAttribute.Alt, Resources.SDownGroup);
                    writer.RenderBeginTag(HtmlTextWriterTag.Img);
                    writer.RenderEndTag();
                    writer.RenderEndTag();
                }
                writer.RenderEndTag();

                #endregion

                #region увеличить вес группировки

                writer.RenderBeginTag(HtmlTextWriterTag.Td);
                if (i > 0 && Enabled)
                {
                    var postBack = Page.ClientScript.GetPostBackClientHyperlink(
                        Journal, string.Format(BaseJournalControl.GroupByColumnMoveUp, column.ColumnName));
                    writer.AddAttribute(HtmlTextWriterAttribute.Href, postBack);
                    writer.AddAttribute(HtmlTextWriterAttribute.Title, Resources.SUpGroup);
                    writer.RenderBeginTag(HtmlTextWriterTag.A);
                    writer.AddAttribute(HtmlTextWriterAttribute.Src, Themes.IconUrlSmallArrowUp);
                    writer.AddAttribute(HtmlTextWriterAttribute.Border, "0");
                    writer.AddAttribute(HtmlTextWriterAttribute.Alt, Resources.SUpGroup);
                    writer.RenderBeginTag(HtmlTextWriterTag.Img);
                    writer.RenderEndTag();
                    writer.RenderEndTag();
                }
                writer.RenderEndTag();

                #endregion

                RenderGroupButtons(writer, column);
                
                writer.RenderEndTag();
            }

            #endregion

            #region вывод колонок по которым может группироваться журнал

            foreach (var column in InnerHeader.Columns)
            {
                if (column.AllowGrouping && !GroupColumns.Contains(column.ColumnName) && column.GetSupportGroupTypes(Journal).Any())
                {
                    if (!column.VisibleInGroup) continue;

                    writer.RenderBeginTag(HtmlTextWriterTag.Tr);

                    #region заголовок колонки

                    writer.RenderBeginTag(HtmlTextWriterTag.Td);
                    writer.Write(column.Header);
                    writer.RenderEndTag();

                    #endregion

                    #region Empty TD
                    
                    writer.RenderBeginTag(HtmlTextWriterTag.Td);
                    writer.RenderEndTag();
                    writer.RenderBeginTag(HtmlTextWriterTag.Td);
                    writer.RenderEndTag();
                    
                    #endregion

                    RenderGroupButtons(writer, column);
                    
                    #region Empty TD

                    writer.RenderBeginTag(HtmlTextWriterTag.Td);
                    writer.RenderEndTag();

                    writer.RenderBeginTag(HtmlTextWriterTag.Td);
                    writer.RenderEndTag();

                    #endregion

                    writer.RenderEndTag();
                }
            }

            #endregion

            writer.RenderEndTag(); //Table
            writer.RenderEndTag(); //Div
            writer.RenderEndTag(); //Fieldset
            writer.RenderEndTag(); //Div
        }

        private void RenderGroupButtons(HtmlTextWriter writer, BaseColumn column)
        {
            GroupColumn groupColumn = null;
            var indexOfColumn = GroupColumns.IndexOf(column.ColumnName);
            if (indexOfColumn > -1)
                groupColumn = GroupColumns[indexOfColumn];

            var supportGroupTypes = column.GetSupportGroupTypes(Journal);
            if (groupColumn != null && groupColumn.GroupType == GroupType.InHeader)
                supportGroupTypes = supportGroupTypes.Where(r => r != GroupType.Top && r != GroupType.TopTotal);

            foreach (var groupType in supportGroupTypes) 
                RenderGroupButton(column, groupColumn, writer, groupType);
        }

        private void RenderGroupButton(
            BaseColumn column,
            GroupColumn groupColumn,
            HtmlTextWriter writer,
            GroupType groupType)
        {
            if ((groupColumn != null && groupColumn.GroupType == groupType)
                || (groupColumn == null && groupType == GroupType.None))
            {
                RenderGroupIcon(writer, column, groupType, true);
            }
            else 
                RenderGroupButton(writer, column, groupType);
        }

        private void RenderGroupIcon(HtmlTextWriter writer, BaseColumn column, GroupType groupType, bool selected)
        {
            writer.RenderBeginTag(HtmlTextWriterTag.Td);
            var title = column.GetGroupTypeTitle(groupType, selected);
            var iconUrl = column.GetGroupTypeImage(groupType, selected);
            
            if (iconUrl != string.Empty)
            {
                writer.AddAttribute(HtmlTextWriterAttribute.Title, title);
                writer.RenderBeginTag(HtmlTextWriterTag.Span);
                writer.AddAttribute(HtmlTextWriterAttribute.Src, iconUrl);
                writer.AddAttribute(HtmlTextWriterAttribute.Border, "0");
                writer.AddAttribute(HtmlTextWriterAttribute.Alt, title);
                if (!Enabled)
                    writer.AddStyleAttribute(HtmlTextWriterStyle.Filter, "gray");
                writer.RenderBeginTag(HtmlTextWriterTag.Img);
                writer.RenderEndTag();
                writer.RenderEndTag();
            }

            writer.RenderEndTag();
        }

        private void RenderGroupButton(HtmlTextWriter writer, BaseColumn column, GroupType groupType)
        {
            if (!Enabled)
            {
                RenderGroupIcon(writer, column, groupType, false);
                return;
            }

            writer.RenderBeginTag(HtmlTextWriterTag.Td);
            string postBack;
            if (groupType == GroupType.None)
            {
                postBack = Page.ClientScript.GetPostBackClientHyperlink(
                    Journal,
                    string.Format(BaseJournalControl.GroupByColumnRemove, column.ColumnName));
            }
            else
            {
                postBack = Page.ClientScript.GetPostBackClientHyperlink(
                    Journal,
                    string.Format(BaseJournalControl.GroupByColumnAdd, "$" + (int)groupType + column.ColumnName));
            }

            var title = column.GetGroupTypeTitle(groupType, false);
            var iconUrl = column.GetGroupTypeImage(groupType, false);

            if (iconUrl != string.Empty)
            {
                writer.AddAttribute(HtmlTextWriterAttribute.Href, postBack);
                writer.AddAttribute(HtmlTextWriterAttribute.Title, title);
                writer.RenderBeginTag(HtmlTextWriterTag.A);
                writer.AddAttribute(HtmlTextWriterAttribute.Src, iconUrl);
                writer.AddAttribute(HtmlTextWriterAttribute.Border, "0");
                writer.AddAttribute(HtmlTextWriterAttribute.Alt, title);
                writer.RenderBeginTag(HtmlTextWriterTag.Img);
                writer.RenderEndTag();
                writer.RenderEndTag();
            }

            writer.RenderEndTag();
        }

        protected override void OnInit(EventArgs e)
        {
            base.OnInit(e);
            if (!Journal.DetailsRender && Journal.GroupPanelVisible && !HideGroupControls && !ScriptManager.IsInAsyncPostBack)
            {
                var extender = new CollapsiblePanelExtender
                                   {
                                       ID = "cp_" + ClientID,
                                       BehaviorID = "cpb_" + ClientID,
                                       Collapsed = true,
                                       ExpandDirection = CollapsiblePanelExpandDirection.Vertical,
                                       CollapseControlID = ClientID + "_Legend",
                                       ExpandControlID = ClientID + "_Legend",
                                   };
                Journal.ParentUserControl.ExtenderAjaxControl.AddExtender(extender, ClientID);
            }
        }
    }
}
