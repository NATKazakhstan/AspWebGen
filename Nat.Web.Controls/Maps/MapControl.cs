using System;
using System.Drawing;
using System.Text;
using System.Web.UI;
using System.Web.UI.WebControls;
using Nat.Web.Controls.GenerationClasses;

namespace Nat.Web.Controls.Maps
{
    public class MapControl : WebControl
    {
        public string Provider { get; set; }
        public string MapID { get { return "map" + ClientID; } }
        public int Zoom { get; set; }
        public PointF Position { get; set; }
        public Unit WorkPanelWidth { get; set; }

        public string PositionS
        {
            get { return string.Format("{0};{1}", Position.X, Position.Y); }
            set
            {
                var split = value.Split(';');
                Position = new PointF(float.Parse(split[0]), float.Parse(split[1]));
            }
        }

        protected override void OnPreRender(EventArgs e)
        {
            base.OnPreRender(e);
            var sb = new StringBuilder();
            sb.AppendLine();
            sb.AppendFormat("var {0};\r\n", MapID);
            sb.AppendFormat("function initialize_{0}() {{\r\n", MapID);
            sb.AppendFormat("{0} = new mxn.Mapstraction('{1}','{2}');\r\n", MapID, ClientID, Provider);
            sb.AppendFormat("{0}.addControls({{zoom:'large', map_type: true}});\r\n", MapID);
            sb.AppendFormat("var latlon = new mxn.LatLonPoint({0}, {1});\r\n", Position.X.ToString().Replace(",", "."), Position.Y.ToString().Replace(",", "."));
            sb.AppendFormat("{0}.setCenterAndZoom(latlon, {1});\r\n", MapID, Zoom);
            sb.Append("}\r\n");
            sb.AppendFormat("window.onload = function(){{initialize_{0}();}};\r\n", MapID);
            Page.ClientScript.RegisterStartupScript(GetType(), "StartMap" + ClientID, sb.ToString(), true);
            Page.ClientScript.RegisterClientScriptInclude("maps.google.com", "/source/googleAPI.js");
            Page.ClientScript.RegisterClientScriptInclude("Mapstraction", string.Format("/source/mxn.js?({0})", Provider));
        }

        public override void RenderBeginTag(HtmlTextWriter writer)
        {
            writer.RenderBeginTag(HtmlTextWriterTag.Center);
            if (!Width.IsEmpty)
                writer.AddAttribute(HtmlTextWriterAttribute.Width, Width.ToString());
            writer.AddAttribute(HtmlTextWriterAttribute.Border, "1");
            writer.RenderBeginTag(HtmlTextWriterTag.Table);
        }

        protected override void RenderContents(HtmlTextWriter writer)
        {
            #region Menu of Map

            writer.RenderBeginTag(HtmlTextWriterTag.Tr);
            writer.AddAttribute(HtmlTextWriterAttribute.Colspan, "2");
            writer.AddStyleAttribute(HtmlTextWriterStyle.TextAlign, "left");
            writer.RenderBeginTag(HtmlTextWriterTag.Td);
            RenderMapMenu(writer);
            writer.RenderEndTag();//Td
            writer.RenderEndTag();//Tr

            #endregion

            writer.RenderBeginTag(HtmlTextWriterTag.Tr);

            #region work panel

            //writer.AddAttribute(HtmlTextWriterAttribute.Rowspan, "2");
            writer.AddStyleAttribute(HtmlTextWriterStyle.Width, WorkPanelWidth.IsEmpty ? "300px" : WorkPanelWidth.ToString());
            writer.AddStyleAttribute(HtmlTextWriterStyle.TextAlign, "left");
            writer.AddStyleAttribute(HtmlTextWriterStyle.VerticalAlign, "top");
            writer.RenderBeginTag(HtmlTextWriterTag.Td);
            RenderWorkPanel(writer);
            writer.RenderEndTag();//Td

            #endregion

            #region Map

            writer.AddStyleAttribute(HtmlTextWriterStyle.Width, "100%");
            writer.RenderBeginTag(HtmlTextWriterTag.Td);

            writer.AddAttribute(HtmlTextWriterAttribute.Id, ClientID);
            writer.AddStyleAttribute(HtmlTextWriterStyle.Height, Height.IsEmpty ? "400px" : Height.ToString());
            writer.RenderBeginTag(HtmlTextWriterTag.Div);
            writer.RenderEndTag();//Div
            writer.RenderEndTag();//Td

            #endregion

            writer.RenderEndTag();//Tr

            #region Status Panel

            writer.RenderBeginTag(HtmlTextWriterTag.Tr);
            writer.AddAttribute(HtmlTextWriterAttribute.Colspan, "2");
            writer.AddStyleAttribute(HtmlTextWriterStyle.TextAlign, "left");
            writer.RenderBeginTag(HtmlTextWriterTag.Td);
            RenderStatusPanel(writer);
            writer.RenderEndTag();//Td
            writer.RenderEndTag();//Tr

            #endregion
        }

        protected virtual void RenderMapMenu(HtmlTextWriter writer)
        {
            new BaseHyperLink
                {
                    CssClass = "map-buttons",
                    ImgUrl = "/images/document_add.png",
                    ToolTip = "Создать проект",
                }.RenderControl(writer);
            new BaseHyperLink
                {
                    CssClass = "map-buttons",
                    ImgUrl = "/images/folder.png",
                    ToolTip = "Открыть проект",
                }.RenderControl(writer);
            new BaseHyperLink
                {
                    CssClass = "map-buttons",
                    ImgUrl = "/images/folder_time.png",
                    ToolTip = "Открыть недавний проект",
                }.RenderControl(writer);
            new BaseHyperLink
                {
                    CssClass = "map-buttons",
                    ImgUrl = "/images/disk_blue.png",
                    ToolTip = "Сохранить проект",
                }.RenderControl(writer);
            new BaseHyperLink
                {
                    CssClass = "map-buttons",
                    ImgUrl = "/images/save_as.png",
                    ToolTip = "Сохранить проект как",
                }.RenderControl(writer);

            new BaseHyperLink
                {
                    CssClass = "map-buttons",
                    ImgUrl = "/images/printer3.png",
                    ToolTip = "Экспорт для печати",
                }.RenderControl(writer);

            writer.AddAttribute(HtmlTextWriterAttribute.Class, "map-buttons-vdiv");
            writer.AddAttribute(HtmlTextWriterAttribute.Src, "/images/hLine.png");
            writer.RenderBeginTag(HtmlTextWriterTag.Img);
            writer.RenderEndTag();

            new BaseHyperLink
                {
                    CssClass = "map-buttons",
                    ImgUrl = "/images/addBig.png",
                    ToolTip = "Добавить слой",
                }.RenderControl(writer);
            new BaseHyperLink
                {
                    CssClass = "map-buttons",
                    ImgUrl = "/images/editBig.png",
                    ToolTip = "Редактировать слой",
                }.RenderControl(writer);
            new BaseHyperLink
                {
                    CssClass = "map-buttons",
                    ImgUrl = "/images/deleteBig.png",
                    ToolTip = "Удалить слой",
                }.RenderControl(writer);

            new BaseHyperLink
                {
                    CssClass = "map-buttons",
                    ImgUrl = "/images/undo.png",
                    ToolTip = "Отменить",
                }.RenderControl(writer);

            new BaseHyperLink
                {
                    CssClass = "map-buttons",
                    ImgUrl = "/images/redo.png",
                    ToolTip = "Вернуть",
                }.RenderControl(writer);
        }

        protected virtual void RenderWorkPanel(HtmlTextWriter writer)
        {
            writer.AddStyleAttribute(HtmlTextWriterStyle.Overflow, "auto");
            writer.AddStyleAttribute(HtmlTextWriterStyle.Width, WorkPanelWidth.IsEmpty ? "300px" : WorkPanelWidth.ToString());
            writer.AddStyleAttribute(HtmlTextWriterStyle.Height, Height.IsEmpty ? "400px" : Height.ToString());
            writer.RenderBeginTag(HtmlTextWriterTag.Div);


            writer.AddStyleAttribute(HtmlTextWriterStyle.Position, "relative");
            //writer.AddStyleAttribute(HtmlTextWriterStyle.Left, "5px");
            writer.AddStyleAttribute(HtmlTextWriterStyle.Top, "4px");
            new BaseHyperLink {ImgUrl = "/images/checkall.png", ToolTip = "Показать все слои", CssClass = "map-checkall",}.
                RenderControl(writer);

            writer.AddStyleAttribute(HtmlTextWriterStyle.Position, "relative");
            //writer.AddStyleAttribute(HtmlTextWriterStyle.Left, "28px");
            writer.AddStyleAttribute(HtmlTextWriterStyle.Top, "4px");
            new BaseHyperLink { ImgUrl = "/images/uncheckall.png", ToolTip = "Скрыть все слои", CssClass = "map-uncheckall", }.
                RenderControl(writer);

            writer.Write("Слои загрязнений<hr/>");

            AddLayer(writer, 0, "Слой 1", "Описание слоя 1");
            AddLayer(writer, 1, "Слой 2", "Описание слоя 2");
            AddLayer(writer, 2, "Слой 3", "Описание слоя 3");
            AddLayer(writer, 3, "Слой 4", "Описание слоя 4");
            writer.RenderEndTag();
        }

        private void AddLayer(HtmlTextWriter writer, int number, string name, string description)
        {
            if (WorkPanelWidth.Type == UnitType.Pixel)
                writer.AddStyleAttribute(HtmlTextWriterStyle.Width, (WorkPanelWidth.Value - 20).ToString() + "px");
            else if (WorkPanelWidth.IsEmpty)
                writer.AddStyleAttribute(HtmlTextWriterStyle.Width, "280px");
            else
                writer.AddStyleAttribute(HtmlTextWriterStyle.Width, "100%");
            writer.AddAttribute(HtmlTextWriterAttribute.Title, description);
            writer.AddAttribute(HtmlTextWriterAttribute.Class, number % 2 == 0 ? "map-Layer0" : "map-Layer1");
            writer.RenderBeginTag(HtmlTextWriterTag.Div);

            new BaseCheckBox
                {
                    ID = ClientID + "chk" + number,
                    OnClick = GetImageUrl(number, description),
                    Label = name,
                }.RenderControl(writer);

            writer.AddStyleAttribute(HtmlTextWriterStyle.Position, "relative");
            writer.AddStyleAttribute(HtmlTextWriterStyle.Top, "4px");
            new BaseHyperLink
                {
                    OnClick = "if(this.addComment){this.children[0].src = '/images/Comment.png'; this.addComment = false;} else {this.children[0].src = '/images/CommentHS.png'; this.addComment = true;}",
                    ImgUrl = "/images/Comment.png",
                    ToolTip = "Комментарий",
                }.RenderControl(writer);

            /*writer.AddStyleAttribute(HtmlTextWriterStyle.Position, "relative");
            writer.AddStyleAttribute(HtmlTextWriterStyle.Top, "4px");
            new BaseHyperLink
                {
                    ImgUrl = Themes.IconUrlRemove,
                    ToolTip = "Удалить слой",
                }.RenderControl(writer);*/

            writer.RenderEndTag();

        }

        private string GetImageUrl(int imageNumber, string description)
        {
            return string.Format("var img = $get('img{6}{0}'); if(img != null) img.parentNode.removeChild(img); else {{{0}.addImageOverlay('img{6}{0}', '{1}', 40, {2}, {3}, {4}, {5}); }}",
                                 MapID, string.Format("/images/img{0}.png", imageNumber),
                                 (Position.Y - 0.002).ToString().Replace(",", "."), (Position.X - 0.001).ToString().Replace(",", "."),
                                 (Position.Y + 0.002).ToString().Replace(",", "."), (Position.X + 0.001).ToString().Replace(",", "."), 
                                 imageNumber);
        }

        protected virtual void RenderStatusPanel(HtmlTextWriter writer)
        {
            writer.Write("Status panel");
        }

        protected virtual void AddButton(HtmlTextWriter writer, string url, string onclick, string imageUrl)
        {
            
        }

        public override void RenderEndTag(HtmlTextWriter writer)
        {
            writer.RenderEndTag();//Table
            writer.RenderEndTag();//Center
        }
    }
}