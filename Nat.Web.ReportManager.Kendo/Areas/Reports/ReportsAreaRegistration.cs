namespace Nat.Web.ReportManager.Kendo.Areas.Reports
{
    using System.Web.Mvc;

    public class ReportsAreaRegistration : AreaRegistration 
    {
        public override string AreaName => "Reports";

        public override void RegisterArea(AreaRegistrationContext context) 
        {
            context.MapRoute(
                "Reports_default",
                "Reports/{controller}/{action}/{id}",
                new { action = "Index", id = UrlParameter.Optional },
                new[] { "Nat.Web.ReportManager.Kendo.Areas.Reports.Controllers" }
            );
        }
    }
}