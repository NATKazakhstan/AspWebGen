﻿using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Web.Configuration;
using System.Web.Mvc;
using System.Web.Routing;
using System.Web.UI;
using Kendo.Mvc;
using Kendo.Mvc.Extensions;
using Kendo.Mvc.UI;
using Nat.Controls.DataGridViewTools;
using Nat.ReportManager.QueryGeneration;
using Nat.ReportManager.ReportGeneration;
using Nat.ReportManager.ReportGeneration.SqlReportingServices;
using Nat.ReportManager.ReportGeneration.StimulSoft;
using Nat.Tools.Constants;
using Nat.Tools.Filtering;
using Nat.Tools.QueryGeneration;
using Nat.Tools.ResourceTools;
using Nat.Tools.Specific;
using Nat.Tools.Validation;
using Nat.Web.Controls;
using Nat.Web.Controls.Filters;
using Nat.Web.Core.System.EventLog;
using Nat.Web.ReportManager.Data;
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
        public ActionResult Index(string pluginName)
        {
            ViewData["refChildMenu"] = 105;
            var options = new {PluginName = pluginName, isKz = LocalizationHelper.IsCultureKZ, setDefaultParams = true};
            ViewBag.Options = JsonConvert.SerializeObject(options);
            return View();
        }

        // GET: Reports/Manager/V
        public ActionResult V(string className, string idrec, string expword, long? idSubscription,
            string idStorageValues, string open, string url, string culture, string format, 
            string setDefaultParams)
        {
            if (idSubscription != null)
                return ChangeSubscription(className, idSubscription.Value, idStorageValues, url);

            if (!string.IsNullOrEmpty(expword) || string.IsNullOrEmpty(open) ||
                "on".Equals(open, StringComparison.InvariantCulture))
            {
                if (string.IsNullOrEmpty(format))
                    format = Request.QueryString["rs:format"];
                if (format == string.Empty) format = null;
                return ExportReport(className, idrec, culture, format, setDefaultParams);
            }

            return OpenReport(className, idrec, culture, setDefaultParams);
        }

        private ActionResult OpenReport(string className, string idrec, string culture, string setDefaultParams)
        {
            ViewData["refChildMenu"] = 105;
            var isKz = LocalizationHelper.IsCultureKZ;
            if (!string.IsNullOrEmpty(culture))
            {
                culture = culture.ToLower();
                switch (culture)
                {
                    case "kz":
                    case "kk-kz":
                        isKz = true;
                        break;
                    case "ru":
                    case "ru-ru":
                        isKz = false;
                        break;
                }
            }

            var plugin = WebReportManager.GetPlugin(className);
            var options = new
            {
                PluginName = className,
                viewOne = true,
                idrec,
                isKz,
                name = plugin?.Description,
                setDefaultParams = !string.IsNullOrEmpty(setDefaultParams) && Convert.ToBoolean(setDefaultParams)
            };
            ViewBag.Options = JsonConvert.SerializeObject(options);
            return View("Index");
        }

        private ActionResult ChangeSubscription(string className, long idSubscription, string storageValuesKey,
            string url)
        {
            ViewData["refChildMenu"] = 105;
            var plugin = WebReportManager.GetPlugin(className);
            var storageValues = Session[storageValuesKey] as StorageValues;
            if (plugin == null || storageValues == null)
                return Redirect(url);

            var options = new
            {
                PluginName = className,
                viewOne = true,
                idSubscription,
                url,
                storageValuesKey,
                name = plugin.Description
            };
            ViewBag.Options = JsonConvert.SerializeObject(options);
            return View("Index");
        }

        private ActionResult ExportReport(string className, string idrec, string culture, string format, string setDefaultParams)
        {
            var value = CreateReport(className, idrec, culture, null, null, format ?? "Auto", false);
            return value is JsonResult ? OpenReport(className, idrec, culture, setDefaultParams) : value;
        }

        [HttpPost]
        public ActionResult SaveSubscription(string pluginName, List<ConditionViewModel> parameters, long idSubscription)
        {
            using (var connection = SpecificInstances.DbFactory.CreateConnection())
            using (var db = new DBDataContext(connection))
            {
                var plugin = WebReportManager.GetPlugin(pluginName);
                if (plugin == null)
                    return Json(new { error = Resources.SPluginNotFound });

                var storageValues = GetStorageValues(parameters, plugin);
                var errors = Validate(plugin);
                if (!string.IsNullOrEmpty(errors))
                    return Json(new {error = errors});

                var row = db.ReportSubscriptions.FirstOrDefault(q => q.id == idSubscription);
                if (row != null)
                {
                    row.values = ReportSubscriptionsHelper.ObjectToBinary(storageValues);
                    var constants = ((IWebReportPlugin) plugin).Constants;
                    row.constants = ReportSubscriptionsHelper.ObjectToBinary(constants);
                    db.SubmitChanges();

                    ReportSubscriptionsHelper.UpdateReportSubscriptionParams(db, row.id, null, null, storageValues, row.reportName);
                }
            }

            return Json(new { success = true });
        }

        [HttpPost]
        public ActionResult GetReports(string searchValue, DataSourceRequest request)
        {
            var plugins = WebReportManager.GetPlugins(out _)
                .Where(r => r.Value.Visible)
                .Where(r =>
                {
                    var roles = (r.Value as IWebReportPlugin)?.Roles();
                    return roles == null || roles.Length == 0 || Tools.Security.UserRoles.IsInAnyRoles(roles);
                })
                .ToList()
                .AsEnumerable();
            if (!string.IsNullOrEmpty(searchValue))
            {
                searchValue = searchValue.ToLower();
                plugins = plugins
                    .Where(r =>
                        r.Key.ToLower().Contains(searchValue)
                        || r.Value.Description.ToLower().Contains(searchValue)
                        || (r.Value.ReportGroup?.ToLower().Contains(searchValue) ?? false))
                    .ToList();
            }

            var dicIDs = new Dictionary<string, int>();
            var hasHash = new Dictionary<int, string>();
            var groupData = plugins.Where(r => !string.IsNullOrEmpty(r.Value.ReportGroup))
                .SelectMany(r => PluginViewModel.ParseGroups(r.Value.ReportGroup, dicIDs, hasHash))
                .Distinct()
                .Where(r => !string.IsNullOrEmpty(r.Key))
                .OrderBy(r => r.Name)
                .ToList();
            var data = plugins
                .Select(r => new PluginViewModel(r.Key, r.Value.ReportGroup, dicIDs, hasHash)
                {
                    Name = r.Value.Description,
                    Visible = r.Value.Visible,
                    PluginName = r.Key,
                    PluginType = r.Value is CrossJournalReportPlugin ? "CrossReport" : "Simple"
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
        public ActionResult GetPluginInfo(string pluginName, string idrec, string storageValuesKey, bool? setDefaultParams)
        {
            var plugin = WebReportManager.GetPlugin(pluginName);
            var webReportPlugin = (IWebReportPlugin) plugin;
            if (plugin == null)
                return Json(new {error = Resources.SPluginNotFound});

            webReportPlugin.DefaultValue = idrec;
            var storageValues = Session[storageValuesKey] as StorageValues;
            if (string.IsNullOrEmpty(idrec)
                && string.IsNullOrEmpty(storageValuesKey)
                && (setDefaultParams == null || setDefaultParams.Value)
                && webReportPlugin.AllowSaveValuesConditions)
            {
                // Load StorageValues
                storageValues = StorageValues.GetStorageValues(plugin.GetType().FullName,
                    Encoding.UTF8.GetBytes(Tools.Security.User.GetSID()));
            }

            var list = new List<ConditionViewModel>();
            foreach (var condition in plugin.Conditions)
            {
                if (storageValues != null && (condition.Visible || webReportPlugin.InitSavedValuesInvisibleConditions))
                {
                    var storage = condition.ColumnFilter.GetStorage();
                    storageValues.SetStorage(storage);
                    condition.ColumnFilter.SetStorage(storage);
                }

                var model = ConditionViewModel.From(condition.ColumnFilter);
                model.Visible = condition.Visible;
                list.Add(model);
            }

            var listCount = storageValues?.CountListValues ?? 1;
            if (listCount < 0)
                listCount = 1;

            for (int i = 0; i < listCount; i++)
            {
                foreach (var condition in plugin.CreateModelFillConditions())
                {
                    if (storageValues != null && (condition.Visible || webReportPlugin.InitSavedValuesInvisibleConditions))
                    {
                        var storage = condition.ColumnFilter.GetStorage();
                        storageValues.SetListStorage(storage, i);
                        condition.ColumnFilter.SetStorage(storage);
                    }

                    var model = ConditionViewModel.From(condition.ColumnFilter);
                    model.Visible = condition.Visible;
                    model.AllowAddParameter = i == 0;
                    list.Add(model);
                }
            }

            return Json(list);
        }

        [HttpPost]
        public ActionResult GetConditionData(string pluginName, string key, string text, List<ConditionViewModel> parameters, [DataSourceRequest]DataSourceRequest request)
        {
            var plugin = WebReportManager.GetPlugin(pluginName);
            if (plugin == null || string.IsNullOrEmpty(key))
                return Json(new { error = Resources.SPluginNotFound });

            GetStorageValues(parameters, plugin);

            foreach (var condition in plugin.Conditions)
            {
                var storage = condition.ColumnFilter.GetStorage();
                if (key.Equals(storage.Name, StringComparison.OrdinalIgnoreCase))
                    return GetConditionData(request, storage);
            }

            foreach (var condition in plugin.CreateModelFillConditions())
            {
                var storage = condition.ColumnFilter.GetStorage();
                if (key.Equals(storage.Name, StringComparison.OrdinalIgnoreCase))
                    return GetConditionData(request, storage);
            }

            return Json(new { error = Resources.SPluginNotFound });
        }

        private ActionResult GetConditionData(DataSourceRequest request, ColumnFilterStorage storage)
        {
            var ds = ConditionViewModel.GetTableDataSource(storage);
            ds.EnablePaging = true;

            var filterDescriptor = request.Filters?.FirstOrDefault() as FilterDescriptor;
            var filter = Convert.ToString(filterDescriptor?.Value);
            var paramNameId = 1;

            var table = DataSourceHelper.GetDataTable(storage.RefDataSource);
            if (!string.IsNullOrEmpty(filter) && filterDescriptor != null &&
                filterDescriptor.Member == storage.ValueColumn)
            {
                ds.View.CustomConditions.Add(new QueryCondition(storage.ValueColumn, ColumnFilterType.Equal,
                    Convert.ChangeType(filter, storage.DataType), null));
            }
            else if (!string.IsNullOrEmpty(filter))
            {
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

            var paging = (bool)(DataSetResourceManager.GetTableExtProperty(table, TableExtProperties.ALLOW_PAGING) ?? true);
            var arguments = paging
                ? new DataSourceSelectArguments(
                    storage.DisplayColumn,
                    (request.Page - 1) * request.PageSize,
                    request.PageSize > 0 ? request.PageSize : 1000) {RetrieveTotalRowCount = true}
                : new DataSourceSelectArguments();
            ds.EnablePaging = paging;
            var data = ds.View.Select(true, arguments);
            return Json(ConditionViewModel.ParseDataView(data));
        }

        [HttpPost]
        public ActionResult ValidateBeforeExport(string pluginName, string culture, List<ConditionViewModel> parameters,
            string parametersStr, string export)
        {
            if (!string.IsNullOrEmpty(parametersStr))
                parameters = JsonConvert.DeserializeObject<List<ConditionViewModel>>(parametersStr);
            if (parameters == null)
                parameters = new List<ConditionViewModel>();

            var plugin = WebReportManager.GetPlugin(pluginName);
            if (plugin == null)
                return Json(new { error = Resources.SPluginNotFound });

            GetStorageValues(parameters, plugin);

            var errors = Validate(plugin);
            return !string.IsNullOrEmpty(errors)
                ? Json(new {error = errors})
                : Json(new {success = true});
        }

        [HttpGet]
        public ActionResult CreateReport()
        {
            return Redirect("/Reports/Manager");
        }

        [HttpPost]
        public ActionResult CreateReport(string pluginName, string idrec, string culture, List<ConditionViewModel> parameters, string parametersStr, string export, bool? subscription)
        {
            if (!string.IsNullOrEmpty(parametersStr))
                parameters = JsonConvert.DeserializeObject<List<ConditionViewModel>>(parametersStr);
            if (parameters == null)
                parameters = new List<ConditionViewModel>();

            var plugin = WebReportManager.GetPlugin(pluginName);
            var webReportPlugin = (IWebReportPlugin) plugin;
            if (plugin == null)
                return Json(new { error = Resources.SPluginNotFound });

            if (!plugin.SupportCulture.Contains(culture))
                culture = null;

            webReportPlugin.DefaultValue = idrec;
            var storageValues = GetStorageValues(parameters, plugin);

            var errors = Validate(plugin);
            if (!string.IsNullOrEmpty(errors))
                return Json(new {error = errors});

            // Save StorageValues
            if (webReportPlugin.AllowSaveValuesConditions)
            {
                StorageValues.SetStorageValues(plugin.GetType().FullName,
                    Encoding.UTF8.GetBytes(Tools.Security.User.GetSID()), storageValues);
            }

            var guid = Guid.NewGuid().ToString();
            Session[guid] = storageValues;
            Session["logmsg" + guid] = plugin.GetLogInformation().Replace("\r\n", "<br/>");
            Session["logcode" + guid] = ReportInitializerSection.GetReportInitializerSection().ReprotPlugins.GetTypeReportLists()[plugin.GetType()].LogMessageType;
            Session["constants" + guid] = (plugin as IWebReportPlugin)?.Constants ?? new Dictionary<string, object>();

            if (subscription ?? false)
            {
                var url = new MainPageUrlBuilder
                {
                    UserControl = "ReportSubscriptionsEdit",
                    IsDataControl = true,
                    IsNew = true
                };
                url.CustomQueryParameters.Add("guid", guid);
                url.CustomQueryParameters.Add("reportName", plugin.GetType().FullName);
                url.CustomQueryParameters.Add("culture", culture);
                switch (plugin)
                {
                    case ISqlReportingServicesPlugin _:
                        url.CustomQueryParameters.Add("isSqlReportingServices", "1");
                        break;
                    case IRedirectReportPlugin _:
                        url.CustomQueryParameters.Add("isSqlReportingServices", "2");
                        break;
                    default:
                        url.CustomQueryParameters.Add("isSqlReportingServices", "0");
                        break;
                }
                return Json(new {Url = url.CreateUrl(false, true)});
            }

            var logMonitor = (LogMonitor)InitializerSection.GetSection().LogMonitor;
            logMonitor.Init();

            Stream stream;
            string fileNameExt;
            if (plugin is IStimulsoftReportPlugin)
                stream = ReportResultPage.GetReport(true, pluginName, guid, storageValues, culture,
                    null, export ?? "Html",
                    "export", logMonitor, false, null, null, out fileNameExt, true,
                    string.IsNullOrEmpty(export));
            else if (plugin is ISqlReportingServicesPlugin)
            {
                stream = ReportingServicesViewer.GetReport(
                    true,
                    pluginName,
                    guid,
                    storageValues,
                    culture,
                    null,
                    export ?? "Html",
                    "render",
                    logMonitor,
                    null,
                    out fileNameExt,
                    true,
                    string.IsNullOrEmpty(export));
            }
            else if (plugin is IRedirectReportPlugin crossPlugin)
            {
                if (crossPlugin.LogViewReport)
                {
                    Tools.Security.DBDataContext.AddViewReports(
                        Tools.Security.User.GetSID(),
                        HttpContext.User.Identity.Name,
                        HttpContext.User.Identity.Name,
                        ReportInitializerSection.GetReportInitializerSection().ReportPageViewer
                        + "?ClassName=" + crossPlugin.GetType().FullName,
                        HttpContext.Request.Url?.GetLeftPart(UriPartial.Authority) ?? "https://srvmax.vvmvd.kz",
                        Environment.MachineName,
                        false,
                        crossPlugin.GetType());
                }

                var url = crossPlugin.GetReportUrl(guid, culture);
                if (string.IsNullOrEmpty(export))
                    return Json(new {Url = url + "&__p__InIFrame=true"});

                return Redirect(url + "&__p__ExportExcel=true");
            }
            else
                return Json(new {error = "Отчет не поддерживается"});

            Session[guid] = null;
            Session["logmsg" + guid] = null;
            Session["logcode" + guid] = null;
            Session["constants" + guid] = null;

            if (string.IsNullOrEmpty(export))
            {
                using (stream)
                using (var reader = new StreamReader(stream))
                {
                    stream.Position = 0;
                    var actionResult = Json(new {ReportContent = reader.ReadToEnd()});
                    if (WebConfigurationManager.GetSection(
                            "system.web.extensions/scripting/webServices/jsonSerialization") is
                        ScriptingJsonSerializationSection section)
                    {
                        actionResult.MaxJsonLength = section.MaxJsonLength * 10;
                        actionResult.RecursionLimit = section.RecursionLimit;
                    }

                    return actionResult;
                }
            }

            return File(stream, "application/octet-stream", plugin.Description + "." + fileNameExt);
        }

        private static string Validate(IReportPlugin plugin)
        {
            var sbErrors = new StringBuilder();
            foreach (var condition in plugin.Conditions.Where(r => r.Visible))
                ValidateCondition(condition, sbErrors);

            if (plugin.CircleFillConditions != null)
                foreach (var condition in plugin.CircleFillConditions.SelectMany(r => r).Where(r => r.Visible))
                    ValidateCondition(condition, sbErrors);

            return sbErrors.ToString();
        }

        private static void ValidateCondition(BaseReportCondition condition, StringBuilder sbErrors)
        {
            var storage = condition.ColumnFilter.GetStorage();
            var valid = true;
            switch (storage.FilterType)
            {
                case ColumnFilterType.NotSet:
                case ColumnFilterType.None:
                case ColumnFilterType.IsNull:
                case ColumnFilterType.NotNull:
                case ColumnFilterType.Custom:
                    break;
                case ColumnFilterType.Equal:
                case ColumnFilterType.NotEqual:
                case ColumnFilterType.More:
                case ColumnFilterType.MoreOrEqual:
                case ColumnFilterType.Less:
                case ColumnFilterType.LessOrEqual:
                case ColumnFilterType.Contains:
                case ColumnFilterType.StartWith:
                case ColumnFilterType.EndWith:
                case ColumnFilterType.ContainsWords:
                case ColumnFilterType.ContainsAnyWords:
                case ColumnFilterType.BetweenColumns:
                    valid = storage.Value1 != null;
                    break;
                case ColumnFilterType.Between:
                case ColumnFilterType.OutOf:
                    valid = storage.Value1 != null && storage.Value2 != null;
                    break;
                case ColumnFilterType.In:
                    var allowNone = (condition.ColumnFilter as WebMultipleValuesColumnFilter)?.AllowNone ?? false;
                    valid = allowNone
                            || (storage.AvailableFilters & ColumnFilterType.None) != 0
                            || storage.Values != null && storage.Values.Length > 0;
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            if (!valid)
            {
                sbErrors.AppendLine(string.Format(Controls.Properties.Resources.SRequiredFieldMessage,
                    storage.Caption)).AppendLine("<br/>");
            }
            else if (storage.FilterType != ColumnFilterType.In && !condition.ColumnFilter.Validate())
                sbErrors.AppendLine(condition.ColumnFilter.ErrorText);
        }

        private static StorageValues GetStorageValues(List<ConditionViewModel> parameters, IReportPlugin plugin)
        {
            var storageValues = new StorageValues();
            var paramsDic = parameters.Where(r => !r.AllowAddParameter).ToDictionary(r => r.Key);
            foreach (var condition in plugin.Conditions)
            {
                var storage = condition.ColumnFilter.GetStorage();
                if (paramsDic.ContainsKey(storage.Name))
                {
                    paramsDic[storage.Name].InitStorage(storage);
                    condition.ColumnFilter.SetStorage(storage);
                }

                if (storage.DataType == null)
                    storageValues.AddStorage(storage);
                else
                    storageValues.AddStorage(storage, condition.ColumnFilter.GetTexts());
            }

            var paramsCircleDic = parameters.Where(r => !r.Removed && r.AllowAddParameter).ToLookup(r => r.Key);
            var conditions = plugin.CreateModelFillConditions();
            if (paramsCircleDic.Count == 0 || conditions == null || conditions.Count == 0)
                return storageValues;

            var count = paramsCircleDic[paramsCircleDic.First().Key].Count();
            plugin.CircleFillConditions = new List<List<BaseReportCondition>>();
            for (var i = 0; i < count; i++)
            {
                conditions = plugin.CreateModelFillConditions();
                foreach (var condition in conditions)
                {
                    var storage = condition.ColumnFilter.GetStorage();
                    if (!paramsCircleDic.Contains(storage.Name))
                        continue;
                    paramsCircleDic[storage.Name].Skip(i).First().InitStorage(storage);
                    condition.ColumnFilter.SetStorage(storage);
                    storageValues.AddListStorage(storage, i, condition.ColumnFilter.GetTexts());
                }
                plugin.CircleFillConditions.Add(conditions);
            }

            return storageValues;
        }
    }
}