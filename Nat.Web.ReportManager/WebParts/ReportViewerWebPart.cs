using System.Runtime.InteropServices;
using Nat.Web.Controls;

namespace Nat.Web.ReportManager.WebParts
{
    [Guid("3BF5773A-D09F-4062-B9F1-EE95D20DA52F")]
    public class ReportViewerWebPart : BaseSPWebPart
    {
        public override string ChildControl
        {
            get { return "/UserControls/ReportManagerControl.ascx"; }
        }

        //protected override void CreateChildControls()
        //{
        //    if(!Page.IsPostBack)
        //    {
        //        Page.Session["SessionWorker$PersonalCard_DsPersonalComposition"] = null;
        //        if(string.IsNullOrEmpty(Page.Request.QueryString["idrec"]))
        //            Page.Session["PersonalCard_refPerson"] = null;
        //        else
        //            Page.Session["PersonalCard_refPerson"] = Int64.Parse(Page.Request.QueryString["idrec"]);
        //    }
        //    base.CreateChildControls();
        //}
    }
}