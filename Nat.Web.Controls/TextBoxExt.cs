/*
 * Created by : Daniil Kovalev
 * Created    : 28.11.2007
 */

using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Web.UI;
using System.Web.UI.WebControls;
using AjaxControlToolkit;


#region Resources

[assembly: WebResource("Nat.Web.Controls.TextBoxExt.js", "text/javascript")]

#endregion


namespace Nat.Web.Controls
{
    [ClientScriptResource("Nat.Web.Controls.TextBoxExt", "Nat.Web.Controls.TextBoxExt.js")]
    public class TextBoxExt : TextBox, IScriptControl, IPostBackDataHandler
    {
        #region Methods

        protected override void OnPreRender(EventArgs e)
        {
            base.OnPreRender(e);

            if(!DesignMode)
            {
                ToolTip = Text;
                ScriptManager.GetCurrent(Page).RegisterScriptControl(this);
            }
        }

        protected override void Render(HtmlTextWriter writer)
        {
            if(!DesignMode)
                ScriptManager.GetCurrent(Page).RegisterScriptDescriptors(this);
            base.Render(writer);
        }

        #endregion


        #region IPostBackDataHandler Members

        bool IPostBackDataHandler.LoadPostData(string postDataKey, NameValueCollection postCollection)
        {
            ToolTip = postCollection[postDataKey];
            return base.LoadPostData(postDataKey, postCollection);
        }

        void IPostBackDataHandler.RaisePostDataChangedEvent()
        {
            base.RaisePostDataChangedEvent();
        }

        #endregion


        #region IScriptControl Members

        public IEnumerable<ScriptDescriptor> GetScriptDescriptors()
        {
            if(Page != null && Visible)
            {
                var desc = new ScriptControlDescriptor("Nat.Web.Controls.TextBoxExt", ClientID);
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
    }
}