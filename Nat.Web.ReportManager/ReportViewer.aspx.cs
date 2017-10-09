using Nat.Web.Controls;
using Nat.Web.ReportManager.Properties;
using Nat.Web.ReportManager.UserControls;
using Nat.Web.Tools.Initialization;

namespace Nat.Web.ReportManager
{
    public partial class ReportViewer : BaseSPPage
    {
        static ReportViewer()
        {
            WebInitializer.Initialize();
        }

        public ReportManagerControl ReportManagerControl => ReportManagerControl1;

        public override string ResourceName => Resources.SReportViewer;

        protected override string OnSessionEmptyRedirect => ReportInitializerSection.GetReportInitializerSection().ReportPageViewer + "?open=off&setDefaultParams=on&ClassName=" + Request.QueryString["ClassName"];
    }
}