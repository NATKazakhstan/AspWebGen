// (c) Copyright Microsoft Corporation.
// This source is subject to the Microsoft Permissive License.
// See http://www.microsoft.com/resources/sharedsource/licensingbasics/sharedsourcelicenses.mspx.
// All other rights reserved.

using System;
using System.Web.UI;
using System.ComponentModel;

[assembly: System.Web.UI.WebResource("AjaxControlToolkit.HoverExtender.HoverBehavior.js", "text/javascript")]

namespace AjaxControlToolkit
{
    [Designer("AjaxControlToolkit.HoverExtenderDesigner, AjaxControlToolkit")]
    [ClientScriptResource("AjaxControlToolkit.HoverBehavior", "AjaxControlToolkit.HoverExtender.HoverBehavior.js")]
    [TargetControlType(typeof(Control))]
    [RequiredScript(typeof(CommonToolkitScripts))]
    [ToolboxItem(false)]
    public class HoverExtender : ExtenderControlBase
    {
        [ExtenderControlProperty]
        [ClientPropertyName("hoverDelay")]
        [DefaultValue(0)]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Unhover", Justification = "Common term")]
        public int HoverDelay
        {
            get { return GetPropertyValue("hoverDelay", 0); }
            set { SetPropertyValue("hoverDelay", value); }
        }

        [ExtenderControlProperty]
        [ClientPropertyName("hoverScript")]
        public string HoverScript
        {
            get { return GetPropertyValue("HoverScript", ""); }
            set { SetPropertyValue("HoverScript", value); }
        }

        [ExtenderControlProperty]
        [ClientPropertyName("unhoverDelay")]
        [DefaultValue(0)]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Unhover", Justification = "Common term")]
        public int UnhoverDelay
        {
            get { return GetPropertyValue("UnhoverDelay", 0); }
            set { SetPropertyValue("UnhoverDelay", value); }
        }

        [ExtenderControlProperty]
        [ClientPropertyName("unhoverScript")]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Unhover", Justification = "Common term")]
        public string UnhoverScript
        {
            get { return GetPropertyValue("UnhoverScript", ""); }
            set { SetPropertyValue("UnhoverScript", value); }
        }
    }
}