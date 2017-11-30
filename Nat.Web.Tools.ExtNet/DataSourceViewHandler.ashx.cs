namespace Nat.Web.Tools.ExtNet
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Web;
    using System.Web.Compilation;
    using System.Web.SessionState;
    using System.Web.UI;

    using Ext.Net;

    using Microsoft.JScript;

    using Nat.Web.Controls;
    using Nat.Web.Controls.GenerationClasses;
    using Nat.Web.Controls.GenerationClasses.BaseJournal;
    using Nat.Web.Tools.Export;
    using Nat.Web.Tools.ExtNet.Data;
    using Nat.Web.Tools.Initialization;

    using Convert = System.Convert;

    public class DataSourceViewHandler : IHttpHandler, IReadOnlySessionState
    {
        public const string ExcelContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";

        private const string Limit = "limit";
        private const string Start = "start";
        private const string DataSourceType = "dataSourceType";
        private const string JournalType = "journalType";

        #region IHttpHandler Members

        public bool IsReusable
        {
            get { return true; }
        }

        public void ProcessRequest(HttpContext context)
        {
            WebInitializer.Initialize();
            var excelExport = "excel".Equals(context.Request["export"], StringComparison.OrdinalIgnoreCase);
            context.Response.ContentType = excelExport ? ExcelContentType : "text/json";
            int total;
            var data = GetData(context, excelExport, out total);
            context.Response.Write(string.Format("{{Total:{1},'Data':{0}}}", JSON.Serialize(data), total));
        }

        #endregion

        private static IEnumerable<IDataRow> GetData(HttpContext context, bool excelExport, out int total)
        {
            var start = 0;
            var limit = 0;
            var isKz = false;
            var showHistory = false;
            var dataSourceType = string.Empty;
            var parameters = string.Empty;
            var refParent = string.Empty;
            var search = string.Empty;

            if (!string.IsNullOrEmpty(context.Request[Start]))
                start = Convert.ToInt32(context.Request[Start]);

            if (!string.IsNullOrEmpty(context.Request[Limit]))
                limit = Convert.ToInt32(context.Request[Limit]);

            if (!string.IsNullOrEmpty(context.Request[DataSourceType]))
                dataSourceType = context.Request[DataSourceType];

            if (!string.IsNullOrEmpty(context.Request["isKz"]))
                isKz = Convert.ToBoolean(context.Request["isKz"]);

            if (!string.IsNullOrEmpty(context.Request["showHistory"]))
                showHistory = Convert.ToBoolean(context.Request["showHistory"]);

            if (!string.IsNullOrEmpty(context.Request["parameters"]))
                parameters = context.Request["parameters"];

            if (!string.IsNullOrEmpty(context.Request["node"]))
                refParent = context.Request["node"];

            if (!string.IsNullOrEmpty(context.Request["search"]))
                search = context.Request["search"];

            if (isKz)
                LocalizationHelper.SetThreadCulture("kk-kz", null);

            var sourceObj = Activator.CreateInstance(BuildManager.GetType(dataSourceType, true, true), null);
            var journal = string.IsNullOrEmpty(context.Request[JournalType])
                              ? null
                              : Activator.CreateInstance(BuildManager.GetType(context.Request[JournalType], true, true));
            var queryParameters = GetQueryParameters(parameters, showHistory, search, context.Request["gridFilters"], (IJournal)journal);

            if (!excelExport)
                return GetData(start, limit, (IDataSourceViewExtNet)sourceObj, queryParameters, refParent, out total);

            ICollection<string> selectedValues = context.Request["selectedValues"]?.Split(',');
            Export(context, queryParameters, (IDataSourceView4)sourceObj, (IExportJournal)journal, selectedValues);
            total = 0;
            return null;
        }

        private static string GetQueryParameters(string parameters, bool showHistory, string search, string gridFiltersStr, IJournal journal)
        {
            var queryParameters = GlobalObject.decodeURIComponent(parameters);
            if (showHistory)
                queryParameters = "/showhistory?" + queryParameters;
            if (string.IsNullOrEmpty(queryParameters) || queryParameters[queryParameters.Length - 1] == '?')
                queryParameters += BaseFilterParameterSearch<object>.SearchQueryParameter + "=" + search;
            else
                queryParameters += "&" + BaseFilterParameterSearch<object>.SearchQueryParameter + "=" + search;

            if (!string.IsNullOrEmpty(gridFiltersStr) && journal != null)
            {
                var gridFilters = new FilterConditions(gridFiltersStr);
                if (gridFilters.Conditions.Count > 0)
                {
                    var columnsDic = journal.GetColumns().OfType<GridColumn>().ToDictionary(r => r.ColumnNameIndex);
                    foreach (var condition in gridFilters.Conditions)
                    {
                    if (!string.IsNullOrEmpty(queryParameters))
                        queryParameters += "&" + (columnsDic[condition.Field].FilterColumnMapping ?? condition.Field);
                        
                        switch (condition.Type)
                        {
                            case FilterType.List:
                                queryParameters += ".EqualsCollection=" + string.Join(",", condition.List);
                                break;
                            case FilterType.Boolean:
                            case FilterType.Date:
                            case FilterType.Numeric:
                                switch (condition.Comparison)
                                {
                                    case Comparison.Eq:
                                        queryParameters += ".Equals=";
                                        break;
                                    case Comparison.Gt:
                                        queryParameters += ".EqualsOrMore=";
                                        break;
                                    case Comparison.Lt:
                                        queryParameters += ".EqualsOrLess=";
                                        break;
                                    default:
                                        throw new ArgumentOutOfRangeException();
                                }
                                queryParameters += condition.Value<string>();
                                break;
                            case FilterType.String:
                                queryParameters += ".Contains=" + condition.Value<string>();
                                break;
                            default:
                                throw new ArgumentOutOfRangeException();
                        }
                    }
                }
            }
            return queryParameters;
        }

        private static IEnumerable<IDataRow> GetData(
            int start,
            int limit,
            IDataSourceViewExtNet dataSource,
            string queryParameters,
            string refParent,
            out int total)
        {
            total = 0;

            if (!dataSource.CheckPermit())
            {
                // todo: log
                return null;
            }

            MainPageUrlBuilder.Current.IsDataControl = true;
            IQueryable<IDataRow> queryable;

            if (string.IsNullOrEmpty(refParent))
                queryable = dataSource.GetFullModelData(queryParameters);
            else
            {
                var refParentValue = "NaN".Equals(refParent) || "Root".Equals(refParent)
                                         ? null
                                         : refParent;
                queryable = dataSource.GetFullModelData(queryParameters, refParentValue);
            }

            total = queryable.Count();

            if (limit > 0)
                queryable = queryable.Skip(start).Take(limit);

            return queryable.ToList();
        }

        private static void Export(HttpContext context, string queryParameters, IDataSourceView4 dataSourceView, IExportJournal journal, ICollection<string> selectedValues)
        {
            if (!dataSourceView.CheckPermit() || !dataSourceView.CheckPermitExport())
            {
                context.Response.StatusCode = 404;
                context.Response.End();
                return;
            }

            var data = selectedValues != null && selectedValues.Count > 0
                           ? dataSourceView.GetSelectIRowByID(queryParameters, selectedValues.ToArray())
                           : dataSourceView.GetSelectIRow(queryParameters);
            var log = InitializerSection.GetSection().LogMonitor;
            log.Init();
            var exportData = data.ToList();
            journal.Url = new MainPageUrlBuilder("/MainPage.aspx/data/" + journal.TableName + "Journal" + queryParameters);
            journal.PrepareExportData(exportData);
            var args = new JournalExportEventArgs
                {
                    CheckPermit = true,
                    Columns = GridHtmlGenerator.GetColumnsForExport(journal.GetColumns()),
                    FilterValues = journal.GetFilterValues(),
                    Format = "Excel",
                    Header = journal.TableHeader,
                    LogMonitor = log,
                    Control = new AccessControl(dataSourceView.CheckPermitExport),
                    Data = exportData,
                };
            var url = new MainPageUrlBuilder
                {
                    UserControl = journal.TableName + "Journal",
                    IsDataControl = true,
                    ShowHistory = true,
                };
            args.ViewJournalUrl = new StringBuilder();
            url.CreateUrl(args.ViewJournalUrl);
            args.ViewJournalUrl.Append("?id.EqualsCollection=");

            var stream = WebSpecificInstances.GetExcelExporter().GetExcelStream(args);
            PageHelper.DownloadFile(stream, journal.TableHeader + "." + args.FileNameExtention, context.Response);
        }
    }
}
