// (c) Copyright Microsoft Corporation.
// This source is subject to the Microsoft Permissive License.
// See http://www.microsoft.com/resources/sharedsource/licensingbasics/sharedsourcelicenses.mspx.
// All other rights reserved.

using System;
using System.Web.UI;

namespace AjaxControlToolkit
{
    /// <summary>
    /// Utility methods for use with the Toolkit
    /// </summary>
    public static class Utility
    {
        /// <summary>
        /// Set the focus of a control after all the client-side behaviors
        /// have finished initializing
        /// </summary>
        /// <param name="control">Control to focus</param>
        /// <remarks>
        /// SetFocusOnLoad is a replacement for ScriptManager.SetFocus and
        /// Page.SetFocus that will focus the control after any client-side
        /// behaviors have been initialized.
        /// </remarks>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Globalization", "CA1303:DoNotPassLiteralsAsLocalizedParameters", Justification = "Assembly is not localized")]
        public static void SetFocusOnLoad(Control control)
        {
            if (control == null)
            {
                throw new ArgumentNullException("control", "Control cannot be null!");
            }

            string script =
                "(function() { " +
                    "var fn = function() { " +
                        "var control = $get('" + control.ClientID + "'); " +
                        "if (control && control.focus) { control.focus(); } " +
                        "Sys.Application.remove_load(fn);" +
                    "};" +
                    "Sys.Application.add_load(fn);" +
                "})();";
            ScriptManager.RegisterStartupScript(control.Page, control.GetType(),
                control.ClientID + "_SetFocusOnLoad", script, true);
        }
    }
}