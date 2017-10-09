using System.ComponentModel;
using System.Web.UI;

namespace Nat.Web.Controls.GenerationClasses
{
    [DefaultProperty("ValueOfLink")]
    [DefaultBindingProperty("ValueOfLink")]
    [ValidationPropertyAttribute("ValueOfLink")]
    public class BaseHyperLink : BaseEditControl, IHyperLink
    {
        public BaseHyperLink()
        {
            RenderAsButton = true;
        }

        public override object Value
        {
            get { return ValueOfLink; }
            set { ValueOfLink = (value ?? "").ToString(); }
        }

        public override string Text { get; set; }

        /// <summary>
        /// Аттрибут value
        /// </summary>
        public string ValueOfLink { get; set; }
        public string Url { get; set; }
        public string ImgUrl { get; set; }
        public string OnClick { get; set; }
        public string OnClickQuestion { get; set; }
        public string Target { get; set; }
        public bool RenderAsButton { get; set; }

        protected override void RenderContents(HtmlTextWriter writer)
        {
            foreach (string key in Attributes.Keys)
                writer.AddAttribute(key, Attributes[key]);
            if (!string.IsNullOrEmpty(CssClass))//todo: перенести в интерфейс
                writer.AddAttribute(HtmlTextWriterAttribute.Class, CssClass);
            HtmlGenerator.RenderHyperLink(writer, this);
        }

        public override void Render(HtmlTextWriter writer, ExtenderAjaxControl extenderAjaxControl)
        {
            HtmlGenerator.RenderHyperLink(writer, this);
        }

        public override void InitAjax(Control control, ExtenderAjaxControl extenderAjaxControl)
        {
        }
    }
}
