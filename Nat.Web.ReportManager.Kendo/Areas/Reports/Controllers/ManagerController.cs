using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using System.Web.Routing;
using Kendo.Mvc.Extensions;
using Kendo.Mvc.UI;
using Nat.Web.Core.System.EventLog;
using Nat.Web.ReportManager.Kendo.Areas.Reports.ViewModels;
using Nat.Web.Tools;

namespace Nat.Web.ReportManager.Kendo.Areas.Reports.Controllers
{
    public class ManagerController : Controller
    {
        private readonly IEventLogManager _eventLogManager;

        public ManagerController(IEventLogManager eventLogManager)
        {
            _eventLogManager = eventLogManager;
        }

        protected override void Initialize(RequestContext requestContext)
        {
            LocalizationHelper.SetThreadCulture();
            base.Initialize(requestContext);
        }

        // GET: Reports/Manager
        public ActionResult Index()
        {
            ViewData["refChildMenu"] = 105;
            return View();
        }

        [HttpPost]
        public ActionResult GetReports(string searchValue, DataSourceRequest request)
        {
            var plugins = WebReportManager.GetPlugins(out _).AsEnumerable();
            if (!string.IsNullOrEmpty(searchValue))
            {
                searchValue = searchValue.ToLower();
                plugins = plugins.Where(r =>
                        r.Key.ToLower().Contains(searchValue)
                        || r.Value.Description.ToLower().Contains(searchValue)
                        || (r.Value.ReportGroup?.ToLower().Contains(searchValue) ?? false))
                    .ToList();
            }

            var dicIDs = new Dictionary<string, int>();
            var groupData = plugins.Where(r => !string.IsNullOrEmpty(r.Value.ReportGroup))
                .SelectMany(r => PluginViewModel.ParseGroups(r.Value.ReportGroup, dicIDs))
                .Distinct()
                .Where(r => !string.IsNullOrEmpty(r.Key))
                .OrderBy(r => r.Name)
                .ToList();
            var data = plugins.Select(r => new PluginViewModel(r.Key, r.Value.ReportGroup, dicIDs)
                {
                    Name = r.Value.Description,
                    Visible = r.Value.Visible,
                    PluginName = r.Key,
                })
                .OrderBy(r => r.Name)
                .ToList();
            return Json(groupData.Union(data).ToDataSourceResult(request));
        }

        public ActionResult Plugins()
        {
            return View();
        }

        public ActionResult Parameters()
        {
            return View();
        }
    }
}