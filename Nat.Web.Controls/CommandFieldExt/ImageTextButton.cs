/*
 * Created by: Arman K. Karibaev
 * Created: 19.08.2008
 * Copyright © JSC NAT Kazakhstan 2008
 */

using System.Web.UI;
using System.Web.UI.WebControls;

[assembly : WebResource("Nat.Web.Controls.CommandFieldExt.Images.buttom_bg.gif", "image/gif")]

namespace Nat.Web.Controls.CommandFieldExt
{
    public class ImageTextButton : ImageButton
    {
        private FieldDisplayStyle _displayStyle;

        public virtual string MainCss
        {
            get
            {
                object obj2 = ViewState["MainCss"];
                if (obj2 != null)
                    return (string) obj2;
                return "";
            }
            set
            {
                if (!Equals(value, ViewState["MainCss"]))
                    ViewState["MainCss"] = value;
            }
        }

        public virtual string ImageCss
        {
            get { return CssClass; }
            set { CssClass = value; }
        }

        public virtual string TextCss
        {
            get
            {
                object obj2 = ViewState["TextCss"];
                if (obj2 != null)
                    return (string) obj2;
                return "";
            }
            set
            {
                if (!Equals(value, ViewState["TextCss"]))
                    ViewState["TextCss"] = value;
            }
        }

        public FieldDisplayStyle DisplayStyle
        {
            get { return _displayStyle; }
            set { _displayStyle = value; }
        }

        public override void RenderBeginTag(HtmlTextWriter writer)
        {
            writer.WriteBeginTag(HtmlTextWriterTag.Span.ToString());
            writer.WriteAttribute(HtmlTextWriterAttribute.Class.ToString(), MainCss);
            writer.WriteAttribute(HtmlTextWriterAttribute.Style.ToString(),
                                  string.Format("{0}: url({1}) repeat-x", HtmlTextWriterAttribute.Background,
                                                Page.ClientScript.GetWebResourceUrl(GetType(),
                                                                                    "Nat.Web.Controls.CommandFieldExt.Images.buttom_bg.gif")));
            writer.Write(HtmlTextWriter.TagRightChar);
            base.RenderBeginTag(writer);
        }

        protected override void RenderContents(HtmlTextWriter writer)
        {
            base.RenderContents(writer);
            writer.WriteBeginTag(HtmlTextWriterTag.A.ToString());
            if (!string.IsNullOrEmpty(OnClientClick))
                writer.WriteAttribute(HtmlTextWriterAttribute.Onclick.ToString(), OnClientClick);
            writer.WriteAttribute(HtmlTextWriterAttribute.Href.ToString(),
                                  string.Format("javascript:{0}", Page.ClientScript.GetPostBackEventReference(this, ID)));
            writer.WriteAttribute(HtmlTextWriterAttribute.Class.ToString(), TextCss);
            writer.Write(HtmlTextWriter.TagRightChar);
            writer.Write(Text);
            writer.WriteEndTag(HtmlTextWriterTag.A.ToString());
        }

        public override void RenderEndTag(HtmlTextWriter writer)
        {
            base.RenderEndTag(writer);
            writer.WriteEndTag(HtmlTextWriterTag.Span.ToString());
        }
    }
}