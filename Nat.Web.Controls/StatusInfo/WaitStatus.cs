/*
 * Created by: Denis M. Silkov
 * Created: 24 сент€бр€ 2007 г.
 */

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Web.UI;
using System.Web.UI.WebControls;
using AjaxControlToolkit;
using Nat.Web.Controls.Properties;

[assembly: WebResource("Nat.Web.Controls.StatusInfo.WaitStatus.js", "text/javascript")]

namespace Nat.Web.Controls.StatusInfo
{
    /// <summary>
    /// ќтображает текующее состо€ние во врем€ отправки PostBack.
    /// </summary>
    [ClientScriptResource("Nat.Web.Controls.StatusInfo.WaitStatus", "Nat.Web.Controls.StatusInfo.WaitStatus.js")]
    public class WaitStatus : WebControl, INamingContainer, IScriptControl
    {
        public WaitStatus()
        {
#pragma warning disable DoNotCallOverridableMethodsInConstructor
            BackColor = Color.FromArgb(255, 128, 128);
#pragma warning restore DoNotCallOverridableMethodsInConstructor
        }

        private ScriptManager CurrentScriptManager
        {
            get { return ScriptManager.GetCurrent(Page); }
        }


        #region IScriptControl Members

        public IEnumerable<ScriptDescriptor> GetScriptDescriptors()
        {
            if (Page != null && Visible)
            {
                var desc = new ScriptControlDescriptor("Nat.Web.Controls.StatusInfo.WaitStatus", ClientID);
                yield return desc;
            }
        }

        public IEnumerable<ScriptReference> GetScriptReferences()
        {
            var references = new List<ScriptReference>();
            references.AddRange(ScriptObjectBuilder.GetScriptReferences(GetType()));
            return references;
        }

        #endregion


        protected override void OnPreRender(EventArgs args)
        {
            base.OnPreRender(args);
            if (CurrentScriptManager != null)
                CurrentScriptManager.RegisterScriptControl(this);
        }

        protected override void Render(HtmlTextWriter writer)
        {
            base.Render(writer);
            if(DesignMode)
                writer.Write(Resources.SLoading);
            else
            {
                if (CurrentScriptManager != null)
                    CurrentScriptManager.RegisterScriptDescriptors(this);
            }
        }

        internal void ResetBackColor()
        {
            BackColor = Color.FromArgb(255, 128, 128);
        }

        internal bool ShouldSerializeBackColor()
        {
            return BackColor != Color.FromArgb(255, 128, 128);
        }
    }
}