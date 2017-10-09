using System;
using System.ComponentModel;
using System.Web.UI;
using AjaxControlToolkit;

[assembly: WebResource("Nat.Web.Controls.ModalPopupStyle.css", "text/css", PerformSubstitution = true)]

namespace Nat.Web.Controls
{
    [ClientCssResource("Nat.Web.Controls.ModalPopupStyle.css")]
    public class ModalPopupExtenderExt : ModalPopupExtender
    {
        #region properties

        [Category("Styles")]
        [DefaultValue(true)]
        [Description("Использовать стиль по умолчанию")]
        public bool UseDefaultCssStyle
        {
            get { return (bool?)ViewState["UseDefaultCssStyle"] ?? true; }
            set { ViewState["UseDefaultCssStyle"] = value; }
        }

        #endregion properties

        #region override methods

        protected override void OnLoad(EventArgs e)
        {
            ScriptObjectBuilder.RegisterCssReferences(this);
            base.OnLoad(e);
        }

        protected override void Render(HtmlTextWriter writer)
        {
            CssApply(UseDefaultCssStyle);
            base.Render(writer);
        }

        #endregion override methods

        #region private methods

        private void CssApply(bool UseClassicalStyle)
        {
            if (UseClassicalStyle)
            {
                BackgroundCssClass = "modalBackground";
            }
        }

        #endregion
    }
}
