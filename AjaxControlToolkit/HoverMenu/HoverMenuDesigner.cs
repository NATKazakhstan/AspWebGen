// (c) Copyright Microsoft Corporation.
// This source is subject to the Microsoft Permissive License.
// See http://www.microsoft.com/resources/sharedsource/licensingbasics/sharedsourcelicenses.mspx.
// All other rights reserved.


using System;
using System.Web.UI;
using System.Web.UI.WebControls;
using AjaxControlToolkit.Design;

namespace AjaxControlToolkit
{
    [TargetControlType(typeof(WebControl))]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Security", "CA2117:AptcaTypesShouldOnlyExtendAptcaBaseTypes", Justification = "Security handled by base class")]
    public class HoverMenuDesigner : ExtenderControlBaseDesigner<HoverMenuExtender>
    {
        /// <summary>
        /// Signature of the page method for DynamicPopulateExtenderControlBase's web
        /// service that is used to support adding/navigating to the page method from
        /// the designer
        /// </summary>
        /// <param name="contextKey">User specific context</param>
        /// <returns>Dynamically generated content</returns>
        [PageMethodSignature("Dynamic Populate", "DynamicServicePath", "DynamicServiceMethod")]
        private delegate string GetDynamicContent(string contextKey);
    }
}