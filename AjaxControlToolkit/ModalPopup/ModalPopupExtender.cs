// (c) Copyright Microsoft Corporation.
// This source is subject to the Microsoft Permissive License.
// See http://www.microsoft.com/resources/sharedsource/licensingbasics/sharedsourcelicenses.mspx.
// All other rights reserved.

using System;
using System.ComponentModel;
using System.Drawing;
using System.Globalization;
using System.Web.UI;
using System.Web.UI.WebControls;

[assembly: System.Web.UI.WebResource("AjaxControlToolkit.ModalPopup.ModalPopupBehavior.js", "text/javascript")]

namespace AjaxControlToolkit
{
    /// <summary>
    /// Extender for the ModalPopup
    /// </summary>
    [Designer("AjaxControlToolkit.ModalPopupDesigner, AjaxControlToolkit")]
    [ClientScriptResource("AjaxControlToolkit.ModalPopupBehavior", "AjaxControlToolkit.ModalPopup.ModalPopupBehavior.js")]
    [RequiredScript(typeof(CommonToolkitScripts))]
    [RequiredScript(typeof(DragPanelExtender))]
    [RequiredScript(typeof(DropShadowExtender))]
    [TargetControlType(typeof(Control))]
    [ToolboxItem("System.Web.UI.Design.WebControlToolboxItem, System.Design, Version=2.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a")]
    [ToolboxBitmap(typeof(ModalPopupExtender), "ModalPopup.ModalPopup.ico")]
    public class ModalPopupExtender : DynamicPopulateExtenderControlBase
    {
        /// <summary>
        /// Desired visibility state: true, false or none
        /// </summary>
        private bool? _show;

        /// <summary>
        /// The ID of the element to display as a modal popup
        /// </summary>
        [ExtenderControlProperty]
        [DefaultValue("")]
        [IDReferenceProperty(typeof(WebControl))]
        [RequiredProperty]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1706:ShortAcronymsShouldBeUppercase", Justification = "Following ASP.NET AJAX pattern")]
        public string PopupControlID
        {
            get { return GetPropertyValue("PopupControlID", ""); }
            set { SetPropertyValue("PopupControlID", value); }
        }

        /// <summary>
        /// The CSS class to apply to the background when the modal popup is displayed
        /// </summary>
        [ExtenderControlProperty]
        [DefaultValue("")]
        public string BackgroundCssClass
        {
            get { return GetPropertyValue("BackgroundCssClass", ""); }
            set { SetPropertyValue("BackgroundCssClass", value); }
        }

        /// <summary>
        /// The ID of the element that dismisses the modal popup
        /// </summary>
        [ExtenderControlProperty]
        [DefaultValue("")]
        [IDReferenceProperty(typeof(WebControl))]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1706:ShortAcronymsShouldBeUppercase", Justification = "Following ASP.NET AJAX pattern")]
        public string OkControlID
        {
            get { return GetPropertyValue("OkControlID", ""); }
            set { SetPropertyValue("OkControlID", value); }
        }

        /// <summary>
        /// The ID of the element that cancels the modal popup
        /// </summary>
        [ExtenderControlProperty]
        [DefaultValue("")]
        [IDReferenceProperty(typeof(WebControl))]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1706:ShortAcronymsShouldBeUppercase", Justification = "Following ASP.NET AJAX pattern")]
        public string CancelControlID
        {
            get { return GetPropertyValue("CancelControlID", ""); }
            set { SetPropertyValue("CancelControlID", value); }
        }

        /// <summary>
        /// Script to run when the modal popup is dismissed with the OkControlID
        /// </summary>
        [ExtenderControlProperty]
        [DefaultValue("")]
        public string OnOkScript
        {
            get { return GetPropertyValue("OnOkScript", ""); }
            set { SetPropertyValue("OnOkScript", value); }
        }

        /// <summary>
        /// Script to run when the modal popup is dismissed with the CancelControlID
        /// </summary>
        [ExtenderControlProperty]
        [DefaultValue("")]
        public string OnCancelScript
        {
            get { return GetPropertyValue("OnCancelScript", ""); }
            set { SetPropertyValue("OnCancelScript", value); }
        }

        /// <summary>
        /// The X coordinate of the top/left corner of the modal popup
        /// (the popup will be centered horizontally if not specified)
        /// </summary>
        [ExtenderControlProperty]
        [DefaultValue(-1)]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "X", Justification = "Common term")]
        public int X
        {
            get { return GetPropertyValue("X", -1); }
            set { SetPropertyValue("X", value); }
        }

        /// <summary>
        /// The Y coordinate of the top/left corner of the modal popup
        /// (the popup will be centered vertically if not specified)
        /// </summary>
        [ExtenderControlProperty]
        [DefaultValue(-1)]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Y", Justification = "Common term")]
        public int Y
        {
            get { return GetPropertyValue("Y", -1); }
            set { SetPropertyValue("Y", value); }
        }

        /// <summary>
        /// True to enable dragging the popup
        /// </summary>
        [ExtenderControlProperty]
        [DefaultValue(false)]
        [Obsolete("The drag feature on modal popup will be automatically turned on if you specify the PopupDragHandleControlID property. Setting the Drag property is a noop")]
        public bool Drag
        {
            get { return GetPropertyValue("stringDrag", false); }
            set { SetPropertyValue("stringDrag", value); }
        }

        /// <summary>
        /// The ID of the embedded element that contains the popup
        /// header/title which will be used as a drag handle
        /// </summary>
        [ExtenderControlProperty]
        [IDReferenceProperty(typeof(WebControl))]
        [DefaultValue("")]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1706:ShortAcronymsShouldBeUppercase", Justification = "Following ASP.NET AJAX pattern")]
        public string PopupDragHandleControlID
        {
            get { return GetPropertyValue("PopupDragHandleControlID", ""); }
            set { SetPropertyValue("PopupDragHandleControlID", value); }
        }

        /// <summary>
        /// True to automatically add a drop-shadow to the modal popup
        /// </summary>
        [ExtenderControlProperty]
        [DefaultValue(false)]
        public bool DropShadow
        {
            get { return GetPropertyValue("stringDropShadow", false); }
            set { SetPropertyValue("stringDropShadow", value); }
        }

        [ExtenderControlProperty]
        [DefaultValue(ModalPopupRepositionMode.RepositionOnWindowResizeAndScroll)]
        [ClientPropertyName("repositionMode")]
        public ModalPopupRepositionMode RepositionMode
        {
            get { return GetPropertyValue("RepositionMode", ModalPopupRepositionMode.RepositionOnWindowResizeAndScroll); }
            set { SetPropertyValue("RepositionMode", value); }
        }

        /// <summary>
        /// Cause the ModalPopup to be shown by sending script to the client
        /// </summary>
        public void Show()
        {
            // It is possible for Show() to be called numerous times during the same
            // postback so we just remember the desired state
            _show = true;
        }

        /// <summary>
        /// Cause the ModalPopup to be hidden by sending script to the client
        /// </summary>
        public void Hide()
        {
            // It is possible for Hide() to be called numerous times during the same
            // postback so we just remember the desired state
            _show = false;
        }

        /// <summary>
        /// Handles the PreRender event.
        /// </summary>
        /// <param name="e"></param>
        protected override void OnPreRender(EventArgs e)
        {
            // If Show() or Hide() were called during the request, change the visibility now
            if (_show.HasValue)
            {
                ChangeVisibility(_show.Value);
            }

            base.OnPreRender(e);
        }

        /// <summary>
        /// Emit script to the client that will cause the modal popup behavior
        /// to be shown or hidden
        /// </summary>
        /// <param name="show">True to show the popup, false to hide it</param>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Globalization", "CA1303:DoNotPassLiteralsAsLocalizedParameters", Justification = "Assembly is not localized")]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2208:InstantiateArgumentExceptionsCorrectly", Justification = "Raising for property, not parameter")]
        private void ChangeVisibility(bool show)
        {
            if (TargetControl == null)
            {
                throw new ArgumentNullException("TargetControl", "TargetControl property cannot be null");
            }

            string operation = show ? "show" : "hide";

            if (ScriptManager.GetCurrent(Page).IsInAsyncPostBack)
            {
                // RegisterDataItem is more elegant, but we can only call it during an async postback
                ScriptManager.GetCurrent(Page).RegisterDataItem(TargetControl, operation);
            }
            else
            {
                // Add a load handler to show the popup and then remove itself
                string script = string.Format(CultureInfo.InvariantCulture,
                    "(function() {{" +
                        "var fn = function() {{" +
                            "AjaxControlToolkit.ModalPopupBehavior.invokeViaServer('{0}', {1}); " +
                            "Sys.Application.remove_load(fn);" +
                        "}};" +
                        "Sys.Application.add_load(fn);" +
                    "}})();",
                    BehaviorID,
                    show ? "true" : "false");
                ScriptManager.RegisterStartupScript(this, typeof(ModalPopupExtender), operation + BehaviorID, script, true);
            }
        }
    }
}
