using System;
using System.Web.UI;
using System.Web.UI.WebControls.WebParts;
using Nat.Web.Tools.Initialization;

namespace Nat.Web.Controls
{
    public abstract class BaseSPWebPart : WebPart
    {
        static BaseSPWebPart()
        {
            WebInitializer.Initialize();
        }

        public BaseSPWebPart()
        {
            ExportMode = WebPartExportMode.All;
        }

        public abstract string ChildControl { get; }

//        protected override void OnInit(EventArgs e)
//        {
//            Page.ClientScript.RegisterClientScriptResource(typeof(GridViewExt), "Nat.Web.Controls.GridView.GridViewExtRowExpand.js");
//            Page.ClientScript.RegisterClientScriptResource(typeof(DatePicker), "Nat.Web.Controls.DateTimeControls.DatePicker.js");
//            Page.ClientScript.RegisterClientScriptResource(typeof(WaitStatus), "Nat.Web.Controls.StatusInfo.WaitStatus.js");
//            base.OnInit(e);
//        }

        protected override void CreateChildControls()
        {
            EnsureUpdatePanelFixups();
        }

        protected override void OnInit(EventArgs e)
        {
            base.OnInit(e);
            string userControl = ChildControl;
            try
            {
//                UpdatePanel up = new UpdatePanel();
//                DatePicker dp = new DatePicker();
//                dp.Style["display"] = "none";
//                WaitStatus wait = new WaitStatus();
//                wait.ID = "wait1";
//                wait.Style["position"] = "absolute";
//                wait.Style["left"] = "500px";
//                up.UpdateMode = UpdatePanelUpdateMode.Conditional;
//                Controls.Add(up);
//                Controls.Add(dp);
//                up.ContentTemplateContainer.Controls.Add(wait);
//                up.ContentTemplateContainer.Controls.Add(Page.LoadControl(userControl));
                Controls.Add(Page.LoadControl(userControl));
            }
            catch(Exception exception)
            {
                string message = string.Format("{0} failed to load: {1}", userControl, exception.Message);
                Controls.Add(new LiteralControl(message));
            }
            UpdateProgressBar updateProgressBar = new UpdateProgressBar();
            Controls.Add(updateProgressBar);
        }

        private void EnsureUpdatePanelFixups()
        {
            if(Page.Form != null)
            {
                string formOnSubmitAtt = Page.Form.Attributes["onsubmit"];
                if(formOnSubmitAtt == "return _spFormOnSubmitWrapper();")
                    Page.Form.Attributes["onsubmit"] = "_spFormOnSubmitWrapper();";
            }
            ScriptManager.RegisterStartupScript(this, typeof(BaseSPWebPart), "UpdatePanelFixup", "_spOriginalFormAction = document.forms[0].action; _spSuppressFormOnSubmitWrapper=true;", true);
        }
    }
}