using System;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Web.UI;

namespace Nat.Web.Controls.GenerationClasses
{
    [DefaultProperty("TextValue")]
    [DefaultBindingProperty("TextValue")]
    [ValidationPropertyAttribute("TextValue")]
    public class BaseTextBox : BaseEditControl, ITextBox, IPostBackDataHandler
    {
        #region ITextBox Members

        public override string Text { get; set; }
        public bool IsMultipleLines { get; set; }
        public int? Columns { get; set; }
        public int? Rows { get; set; }
        public int? MaxLength { get; set; }
        public string TextValue { get; set; }

        public override object Value
        {
            get { return TextValue; }
            set { TextValue = (value ?? "").ToString(); }
        }

        #endregion

        protected override void RenderContents(HtmlTextWriter writer)
        {
            HtmlGenerator.RenderTextBox(writer, this, null);
        }

        public override void Render(HtmlTextWriter writer, ExtenderAjaxControl extenderAjaxControl)
        {
            HtmlGenerator.RenderTextBox(writer, this, extenderAjaxControl);
        }

        public override void InitAjax(Control control, ExtenderAjaxControl extenderAjaxControl)
        {
        }

        public bool LoadPostData(string postDataKey, NameValueCollection postCollection)
        {
            var text = TextValue;
            var str2 = postCollection[((IRenderComponent)this).UniqueID] ?? postCollection[UniqueID];
            if (!text.Equals(str2, StringComparison.Ordinal))
            {
                TextValue = str2;
                return true;
            }

            return false;
        }

        public void RaisePostDataChangedEvent()
        {
        }
    }
}
