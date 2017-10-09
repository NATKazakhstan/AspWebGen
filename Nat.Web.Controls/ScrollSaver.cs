using System;
using System.Collections.Generic;
using System.Web.Script.Serialization;
using System.Web.UI;
using System.Web.UI.WebControls;
using AjaxControlToolkit;

#region Resources

[assembly: WebResource("Nat.Web.Controls.ScrollSaver.js", "text/javascript")]

#endregion


namespace Nat.Web.Controls
{
    [ClientScriptResource("Nat.Web.Controls.ScrollSaver", "Nat.Web.Controls.ScrollSaver.js")]
    public class ScrollSaver : HiddenField, IScriptControl
    {
        #region IScriptControl Members

        public IEnumerable<ScriptDescriptor> GetScriptDescriptors()
        {
            if(Page != null)
            {
                var desc = new ScriptComponentDescriptor("Nat.Web.Controls.ScrollSaver");

                desc.ID = String.Format("scrollSaver_{0}", ID);
                desc.AddProperty("scrollControls", SerializeControls());
                desc.AddProperty("clientID", ClientID);

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


        protected override void OnPreRender(EventArgs e)
        {
            base.OnPreRender(e);

            if (!DesignMode && !ScriptRegistred1)
            {
                CurrentScriptManager.RegisterScriptControl(this);
                ScriptRegistred1 = true;
            }
        }

        protected override void Render(HtmlTextWriter writer)
        {
            base.Render(writer);
//            string Script =
//            @"
//            function SaveState(){
//                debugger;
//                var Ctrls=document.getElementsByTagName('*');
//                x=Ctrls.length;
//                var i;
//                for(i=0;i<x;i++){
//                    if (Ctrls[i].scrollTop!=null && Ctrls[i].scrollLeft!=null){
//                    if (Ctrls[i].scrollTop!=0 || Ctrls[i].scrollLeft!=0)
//                        document.getElementById ('" + ClientID + @"').value+=
//                            Ctrls[i].id+','+Ctrls[i].scrollTop+','+Ctrls[i].scrollLeft+';';
//                    }
//                }
//            }";

//            Script +=
//            @"
//            function LoadState() {
//                var Ctrls=document.getElementsByTagName('*');
//                x=Ctrls.length;
//                mass=document.getElementById ('" + ClientID + @"').value.split(';');
//                var i;var j;
//                for(i=0;i<mass.length-1;i++) {
//                    m2=mass[i].split(',');
//                    if (m2.length==3){
//                        for(j=0;j<x;j++){
//                            if (m2[0]==Ctrls[j].id){
//                                Ctrls[j].scrollTop=m2[1];
//                                Ctrls[j].scrollLeft=m2[2];
//                            }
//                        }
//                    }
//                }
//            }";

//            Page.ClientScript.RegisterStartupScript(Page.GetType(), "Scroll", Script, true);
//            Page.ClientScript.RegisterStartupScript(Page.GetType(), "Scroll2", "LoadState();", true);
//            Page.ClientScript.RegisterStartupScript(Page.GetType(), "Scroll3", "document.getElementById('" + Page.Form.ClientID + "').onsubmit=function(){SaveState();};", true);

            if (!DesignMode && !ScriptRegistred)
            {
                CurrentScriptManager.RegisterScriptDescriptors(this);
                ScriptRegistred = true;
            }
        }

        private Object SerializeControls()
        {
            JavaScriptSerializer jss = new JavaScriptSerializer();
            List<String> controlIDs = new List<String>();
            foreach (Control control in ScrollControls)
            {
                controlIDs.Add(control.ClientID);
            }
            return jss.Serialize(controlIDs);
        }


        #region Properties

        private Boolean ScriptRegistred;

        private Boolean ScriptRegistred1;

        private ScriptManager CurrentScriptManager
        {
            get
            {
                ScriptManager scriptManager = ScriptManager.GetCurrent(Page);
                if(scriptManager == null)
                    throw new Exception("Page should contain ScriptManager");
                return scriptManager;
            }
        }

        public List<Control> ScrollControls
        {
            get { return scrollControls; }
            set { scrollControls = value; }
        }

        #endregion


        #region Fields

        private List<Control> scrollControls = new List<Control>();

        #endregion

    }
}