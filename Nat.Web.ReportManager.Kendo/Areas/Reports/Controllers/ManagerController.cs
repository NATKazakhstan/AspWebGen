using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Web.Mvc;
using System.Web.Routing;
using System.Web.UI;
using Kendo.Mvc;
using Kendo.Mvc.Extensions;
using Kendo.Mvc.UI;
using Nat.ReportManager.ReportGeneration.StimulSoft;
using Nat.Tools.Filtering;
using Nat.Tools.QueryGeneration;
using Nat.Web.Controls;
using Nat.Web.Core.System.EventLog;
using Nat.Web.ReportManager.Kendo.Areas.Reports.ViewModels;
using Nat.Web.ReportManager.Kendo.Properties;
using Nat.Web.ReportManager.ReportGeneration;
using Nat.Web.Tools;
using Nat.Web.Tools.Initialization;
using Newtonsoft.Json;

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

        [HttpPost]
        public ActionResult GetPluginInfo(string pluginName)
        {
            var plugin = WebReportManager.GetPlugin(pluginName);
            if (plugin == null)
                return Json(new {error = Resources.SPluginNotFound});

            var list = new List<ConditionViewModel>();
            foreach (var condition in plugin.Conditions)
            {
                var storage = condition.ColumnFilter.GetStorage();
                var model = ConditionViewModel.From(storage);
                model.Visible = condition.Visible;
                list.Add(model);
            }

            return Json(list);
        }

        [HttpPost]
        public ActionResult GetConditionData(string pluginName, string key, string text, [DataSourceRequest]DataSourceRequest request)
        {
            var plugin = WebReportManager.GetPlugin(pluginName);
            if (plugin == null || string.IsNullOrEmpty(key))
                return Json(new { error = Resources.SPluginNotFound });

            foreach (var condition in plugin.Conditions)
            {
                var storage = condition.ColumnFilter.GetStorage();
                if (key.Equals(storage.Name, StringComparison.OrdinalIgnoreCase))
                {
                    var ds = ConditionViewModel.GetTableDataSource(storage);
                    ds.EnablePaging = true;

                    var filter = Convert.ToString((request.Filters?.FirstOrDefault() as FilterDescriptor)?.Value);
                    var paramNameId = 1;

                    if (!string.IsNullOrEmpty(filter))
                    {
                        var table = DataSourceHelper.GetDataTable(storage.RefDataSource);
                        var filterSplit = filter.Split(new[] {' '}, StringSplitOptions.RemoveEmptyEntries);
                        var list = new QueryCondition
                        {
                            Conditions = new QueryConditionList
                            {
                                ConditionJunction = ConditionJunction.And
                            }
                        };
                        list.Conditions.AddRange(filterSplit.Select(r => new QueryCondition
                        {
                            Conditions = new QueryConditionList {ConditionJunction = ConditionJunction.Or}
                        }));

                        foreach (DataColumn dc in table.Columns)
                        {
                            var column = ColumnViewModel.From(dc);
                            if (column == null)
                                continue;

                            if (dc.DataType == typeof(string))
                            {
                                for (int i = 0; i < filterSplit.Length; i++)
                                    list.Conditions[i].Conditions.Add(new QueryCondition(dc.ColumnName,
                                        ColumnFilterType.Contains,
                                        new QueryParameter("@filterParameter" + paramNameId++, filterSplit[i]), null));
                            }
                        }
                        ds.View.CustomConditions.Add(list);
                    }

                    var data = ds.View.Select(true,
                        new DataSourceSelectArguments(storage.DisplayColumn, 0, 50) {RetrieveTotalRowCount = true});
                    return Json(ConditionViewModel.ParseDataView(data));
                }
            }

            return Json(new { error = Resources.SPluginNotFound });
        }

        [HttpPost]
        public ActionResult CreateReport(string pluginName, string culture, List<ConditionViewModel> parameters, string parametersStr, string export)
        {
            if (!string.IsNullOrEmpty(parametersStr))
                parameters = JsonConvert.DeserializeObject<List<ConditionViewModel>>(parametersStr);
            if (parameters == null)
                parameters = new List<ConditionViewModel>();

            var plugin = WebReportManager.GetPlugin(pluginName);
            if (plugin == null)
                return Json(new { error = Resources.SPluginNotFound });

            var guid = Guid.NewGuid().ToString();
            var storageValues = new StorageValues();
            var paramsDic = parameters.ToDictionary(r => r.Key);
            foreach (var condition in plugin.Conditions)
            {
                var storage = condition.ColumnFilter.GetStorage();
                if (paramsDic.ContainsKey(storage.Name))
                {
                    paramsDic[storage.Name].SetToStorageValues(storage, storageValues);
                    condition.ColumnFilter.SetStorage(storage);
                    storageValues.SetStorageTextValues(storage.Name, condition.ColumnFilter.GetTexts());
                }
            }

            //(plugin as IWebReportPlugin).Constants

            //logType = (LogMessageType)HttpContext.Current.Session["logcode" + guid];
            //message = (string)HttpContext.Current.Session["logmsg" + guid];
            Session[guid] = storageValues;
            Session["logmsg" + guid] = plugin.GetLogInformation().Replace("\r\n", "<br/>");
            Session["logcode" + guid] = ReportInitializerSection.GetReportInitializerSection().ReprotPlugins.GetTypeReportLists()[plugin.GetType()].LogMessageType;
            Session["constants" + guid] = (plugin as IWebReportPlugin)?.Constants ?? new Dictionary<string, object>();

            var logMonitor = InitializerSection.GetSection().LogMonitor;
            logMonitor.Init();

            var stream = ReportResultPage.GetReport(true, pluginName, guid, storageValues, culture,
                null, export ?? "Html",
                "export", (LogMonitor) logMonitor, false, null, null, out var fileNameExt, true);

            if (string.IsNullOrEmpty(export))
            {
                using (stream)
                using (var reader = new StreamReader(stream))
                {
                    stream.Position = 0;
                    return Json(new {ReportContent = reader.ReadToEnd()});
                }
            }

            return File(stream, "application/octet-stream", plugin.Description + "." + fileNameExt);
        }
    }
}