// (c) Copyright Microsoft Corporation.
// This source is subject to the Microsoft Permissive License.
// See http://www.microsoft.com/resources/sharedsource/licensingbasics/sharedsourcelicenses.mspx.
// All other rights reserved.

using System;
using System.ComponentModel;
using System.Collections.Generic;
using System.Text;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

#region Assembly Resource Attribute
[assembly: System.Web.UI.WebResource("AjaxControlToolkit.ReorderList.DraggableListItemBehavior.js", "text/javascript")]
#endregion

namespace AjaxControlToolkit
{
    /// <summary>
    /// This just wraps the Ajax draggableListItem behavior.
    /// </summary>
    [ClientScriptResource("AjaxControlToolkit.DraggableListItem", typeof(AjaxControlToolkit.DropWatcherExtender), "ReorderList.DraggableListItemBehavior.js")]
    [RequiredScript(typeof(CommonToolkitScripts))]
    [TargetControlType(typeof(ReorderListItem))]
    [ToolboxItem(false)]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Draggable", Justification = "Common term")]
    public class DraggableListItemExtender : ExtenderControlBase
    {
        [IDReferenceProperty(typeof(Control))]
        [ClientPropertyName("handle")]
        [ExtenderControlProperty()]
        [ElementReference()]
        [DefaultValue("")]
        public string Handle {
            [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode", Justification = "Used by VS designer")]
            get {
                return GetPropertyValue("handle", "");
            }
            set {
                SetPropertyValue("handle", value);
            }
        }
    }
}
