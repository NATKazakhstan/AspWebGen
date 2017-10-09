using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace Nat.Web.Controls.EditComponent
{
    [DefaultProperty("Value")]
    public class SimpleTextEditor : WebControl
    {
        public string Value { get; set; }

        public override void RenderBeginTag(HtmlTextWriter writer)
        {
            writer.AddAttribute(HtmlTextWriterAttribute.Id, ClientID);
            writer.RenderBeginTag(HtmlTextWriterTag.Div);

            writer.AddAttribute(HtmlTextWriterAttribute.Id, ClientID + "_hf");
            writer.AddAttribute(HtmlTextWriterAttribute.Name, UniqueID + "$hf");
            writer.AddAttribute(HtmlTextWriterAttribute.Type, "input");
            writer.AddStyleAttribute(HtmlTextWriterStyle.Display, "none");
            writer.RenderBeginTag(HtmlTextWriterTag.Input);
            writer.Write(Value);
            writer.RenderEndTag();

            writer.AddAttribute("contentEditable", "true");
            writer.AddAttribute(HtmlTextWriterAttribute.Id, ClientID + "_content");
            writer.RenderBeginTag(HtmlTextWriterTag.Span);
        }

        protected override void OnInit(EventArgs e)
        {
            base.OnInit(e);

            if (Page.IsPostBack)
                Value = Page.Request.Form[UniqueID + "$hf"];
        }

        protected override void RenderContents(HtmlTextWriter writer)
        {
            if (string.IsNullOrEmpty(Value))
            {
                writer.RenderBeginTag(HtmlTextWriterTag.P);
                writer.RenderEndTag();
            }
            else
            {
                writer.Write(Value);
            }
        }

        public override void RenderEndTag(HtmlTextWriter writer)
        {
            writer.RenderEndTag();
            writer.RenderEndTag();
        }
    }
}