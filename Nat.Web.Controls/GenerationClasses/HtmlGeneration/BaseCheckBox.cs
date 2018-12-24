using System.Collections.Specialized;
using System.ComponentModel;
using System.Web.UI;
using Nat.Web.Controls.Properties;

namespace Nat.Web.Controls.GenerationClasses
{
    [DefaultProperty("Checked")]
    [DefaultBindingProperty("Checked")]
    [ValidationProperty("Checked")]
    public class BaseCheckBox : BaseEditControl, ICheckBox, IPostBackDataHandler
    {
        public bool Checked { get; set; }
        public string TrueText { get; set; }
        public string FalseText { get; set; }
        public string OnChange { get; set; }
        public string OnClick { get; set; }
        public string Label { get; set; }

        public override object Value
        {
            get { return Checked; }
            set { Checked = true.Equals(value) || "on".Equals(value); }
        }

        public override string Text
        {
            get { return Checked ? TrueText ?? Resources.SYes : FalseText ?? Resources.SNo; }
            set { }
        }

        protected override void AddMandatoryValidator()
        {
        }

        protected override void RenderContents(HtmlTextWriter writer)
        {
            writer.RenderCheckBox(this, null, AddAttributes);
        }

        public override void Render(HtmlTextWriter writer, ExtenderAjaxControl extenderAjaxControl)
        {
            writer.RenderCheckBox(this, extenderAjaxControl, AddAttributes);
        }

        public override void InitAjax(Control control, ExtenderAjaxControl extenderAjaxControl)
        {
        }

        public bool LoadPostData(string postDataKey, NameValueCollection postCollection)
        {
            Checked = postCollection[UniqueID] != null;
            return true;
        }

        public void RaisePostDataChangedEvent()
        {
        }
    }
}