// (c) Copyright Microsoft Corporation.
// This source is subject to the Microsoft Permissive License.
// See http://www.microsoft.com/resources/sharedsource/licensingbasics/sharedsourcelicenses.mspx.
// All other rights reserved.

using System;
using System.ComponentModel;
using System.Drawing;
using System.Web.UI;
using System.Web.UI.WebControls;

[assembly: WebResource("AjaxControlToolkit.ValidatorCallout.ValidatorCalloutBehavior.js", "text/javascript")]
[assembly: WebResource("AjaxControlToolkit.ValidatorCallout.alert-large.gif", "image/gif")]
[assembly: WebResource("AjaxControlToolkit.ValidatorCallout.alert-small.gif", "image/gif")]
[assembly: WebResource("AjaxControlToolkit.ValidatorCallout.close.gif", "image/gif")]

namespace AjaxControlToolkit
{
    [Designer("AjaxControlToolkit.ValidatorCalloutDesigner, AjaxControlToolkit")]
    [RequiredScript(typeof(CommonToolkitScripts))]
    [RequiredScript(typeof(PopupExtender))]
    [RequiredScript(typeof(AnimationExtender))]
    [TargetControlType(typeof(IValidator))]
    [ClientScriptResource("AjaxControlToolkit.ValidatorCalloutBehavior", "AjaxControlToolkit.ValidatorCallout.ValidatorCalloutBehavior.js")]
    [ToolboxItem("System.Web.UI.Design.WebControlToolboxItem, System.Design, Version=2.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a")]
    [ToolboxBitmap(typeof(ValidatorCalloutExtender), "ValidatorCallout.ValidatorCallout.ico")]
    public class ValidatorCalloutExtender : AnimationExtenderControlBase
    {
        [DefaultValue("")]
        [UrlProperty]
        [ExtenderControlProperty]
        [ClientPropertyName("warningIconImageUrl")]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1056:UriPropertiesShouldNotBeStrings", Justification = "Using string to avoid Uri complications")]
        public string WarningIconImageUrl
        {
            get { return GetPropertyValue("WarningIconImageUrl", (string)null) ?? (DesignMode ? "" : Page.ClientScript.GetWebResourceUrl(typeof(ValidatorCalloutExtender), "AjaxControlToolkit.ValidatorCallout.alert-large.gif")); }
            set { SetPropertyValue("WarningIconImageUrl", value); }
        }

        [DefaultValue("")]
        [UrlProperty]
        [ExtenderControlProperty]
        [ClientPropertyName("closeImageUrl")]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1056:UriPropertiesShouldNotBeStrings", Justification = "Using string to avoid Uri complications")]
        public string CloseImageUrl
        {
            get { return GetPropertyValue("CloseImageUrl", (string)null) ?? (DesignMode ? "" : Page.ClientScript.GetWebResourceUrl(typeof(ValidatorCalloutExtender), "AjaxControlToolkit.ValidatorCallout.close.gif")); }
            set { SetPropertyValue("CloseImageUrl", value); }
        }

        [DefaultValue("")]
        [ExtenderControlProperty]
        [ClientPropertyName("highlightCssClass")]
        public string HighlightCssClass
        {
            get { return GetPropertyValue("HighlightCssClass", string.Empty); }
            set { SetPropertyValue("HighlightCssClass", value); }
        }

        [DefaultValue(typeof(Unit), "")]
        [ExtenderControlProperty]
        [ClientPropertyName("width")]
        public Unit Width
        {
            get { return GetPropertyValue("Width", Unit.Empty); }
            set { SetPropertyValue("Width", value); }
        }

        /// <summary>
        /// OnShow animation
        /// </summary>
        [ExtenderControlProperty]
        [ClientPropertyName("onShow")]
        [Browsable(false)]
        [DefaultValue(null)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public Animation OnShow
        {
            get { return GetAnimation(ref _onShow, "OnShow"); }
            set { SetAnimation(ref _onShow, "OnShow", value); }
        }
        private Animation _onShow;

        /// <summary>
        /// OnHide animation
        /// </summary>
        [ExtenderControlProperty]
        [ClientPropertyName("onHide")]
        [Browsable(false)]
        [DefaultValue(null)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public Animation OnHide
        {
            get { return GetAnimation(ref _onHide, "OnHide"); }
            set { SetAnimation(ref _onHide, "OnHide", value); }
        }
        private Animation _onHide;

        /// <summary>
        /// Convert server IDs into ClientIDs for animations
        /// </summary>
        protected override void OnPreRender(EventArgs e)
        {
            base.OnPreRender(e);

            ResolveControlIDs(_onShow);
            ResolveControlIDs(_onHide);
        }
    }
}
