using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Principal;
using System.Web.UI.WebControls;
using Microsoft.Reporting.WebForms;
using Nat.ReportManager.ReportGeneration.SqlReportingServices;
using Nat.Web.Controls;
using Nat.Web.ReportManager.Properties;
using Nat.Web.Tools;
using Nat.Web.Tools.Initialization;
using System.IO;
using System.Web;
using System.Web.UI;

namespace Nat.Web.ReportManager
{
    using System.Web.Compilation;

    using Nat.Web.ReportManager.Modules;
    using Nat.Web.Tools.Security;

    public partial class ReportingServicesViewer : BaseSPPage
    {
        static ReportingServicesViewer()
        {
            WebInitializer.Initialize();
        }

        protected override string OnSessionEmptyRedirect => ReportInitializerSection.GetReportInitializerSection().ReportPageViewer + "?open=off&setDefaultParams=on&ClassName=" + Request.QueryString["reportName"];

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
        }

        protected override void OnLoadComplete(EventArgs e)
        {
            base.OnLoadComplete(e);

            if (!string.IsNullOrEmpty(Request.QueryString["backPath"]) &&
                !string.IsNullOrEmpty(Request.QueryString["text"]) 
                && bBack != null)
            {
                bBack.Visible = true;
                bBack.Text = Request.QueryString["text"];
            }

            if (!Page.IsPostBack)
            {
                string guid = Request.QueryString["values"];
                var storageValues = (StorageValues)Session[guid];
                if (storageValues == null)
                {
                    Response.Redirect(
                        ReportInitializerSection.GetReportInitializerSection().ReportPageViewer + "?open=off&setDefaultParams=on&ClassName="
                        + Request.QueryString["reportName"]);
                    return;
                }

                string fileNameExtention;
                GetReport(
                    false,
                    Request.QueryString["reportName"],
                    guid,
                    storageValues,
                    Request.QueryString["culture"],
                    Page,
                    Request.QueryString["rs:format"],
                    Request.QueryString["rs:command"],
                    LogMonitor.LogMonitor,
                    rv.ServerReport,
                    out fileNameExtention,
                    true);

                var property = rv.GetType().GetProperty(
                    "InstanceIdentifier",
                    System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                if (property != null)
                {
                    var instanceIdentifier = Convert.ToString(property.GetValue(rv, null));
                    if (!string.IsNullOrEmpty(instanceIdentifier))
                    {
                        var type = BuildManager.GetType(Request.QueryString["reportName"], true, true);
                        SqlReportViewerLogExportModule.SetPlugin(instanceIdentifier, type);
                    }
                }
            }
        }

        private static ServerReport ReportViewerInitializer(ServerReport report)
        {
            var reportSection = ReportInitializerSection.GetReportInitializerSection();
            if (ReportServerCredentials.HasConfiguration())
                report.ReportServerCredentials = new ReportServerCredentials();
            //report.ReportServerUrl = new Uri(@"http://stendnat/ReportServer/");
            report.ReportServerUrl = new Uri(reportSection.ReportingServicesUrl);
            report.ReportPath = reportSection.ReportingServicesReportsFolder;
            return report;
        }

        public static Stream GetReport(bool useReturnStream, string pluginName, string guid,
            StorageValues storageValues, string culture, Page page, string format, string command,
            LogMonitor logMonitor, ServerReport report, out string fileNameExtention, bool RoleCheck, 
            bool isPreview = false)
        {
            switch (format.ToLower())
            {
                case "html":
                    format = "HTML4.0";
                    break;
                case "auto":
                    format = "WORD";
                    break;
            }

            fileNameExtention = "";
            WebInitializer.Initialize();
            if (report == null)
                report = new Microsoft.Reporting.WebForms.ReportViewer().ServerReport;
            report = ReportViewerInitializer(report);
            var webReportManager = new WebReportManager(new TreeView());
            if (string.IsNullOrEmpty(pluginName)) 
                return null;
            webReportManager.RoleCheck = RoleCheck;
            webReportManager.Plugin = webReportManager.GetPlugins()[pluginName];
            webReportManager.Plugin.InitializeReportCulture(culture);
            var webReportPlugin = (IWebReportPlugin)webReportManager.Plugin;
            webReportPlugin.Page = page;
            webReportManager.CreateView();
            webReportManager.InitValues(storageValues, true);
            var list = new List<ReportParameter>();
            foreach (var reportCondition in webReportManager.Plugin.Conditions)
            {
                var conditions = reportCondition.GetQueryConditions().ToList();
                if (conditions.Count == 0)
                    list.Add(new ReportParameter(reportCondition.ColumnFilter.GetStorage().Name, (string)null, false));
                else
                {
                    foreach (var condition in conditions)
                    {
                        if (condition.Parameters.Length == 0)
                            throw new Exception("Условие без значения не поддерживаются при формировании отчета");
                        var values = condition.GetParamValues().Select(r => r.ToString()).ToArray();
                        list.Add(new ReportParameter(condition.ColumnName, values, false));
                    }
                }
            }

            if (!string.IsNullOrEmpty(webReportManager.Plugin.CultureParameter))
            {
                if (webReportManager.Plugin.SupportRuKz)
                    list.Add(new ReportParameter(webReportManager.Plugin.CultureParameter, webReportManager.Plugin.InitializedKzCulture.ToString(), false));
                else
                    list.Add(new ReportParameter(webReportManager.Plugin.CultureParameter, webReportManager.Plugin.InitializedCulture, false));
            }
            //+= потому что инициализируется папка в которой лежат отчеты
            report.ReportPath += ((ISqlReportingServicesPlugin)webReportManager.Plugin).ReportUrl;
            report.SetParameters(list);

            Log(logMonitor, guid, webReportManager);

            var export = !string.IsNullOrEmpty(format) && "render".Equals(command, StringComparison.OrdinalIgnoreCase);

            DBDataContext.AddViewReports(
                Tools.Security.User.GetSID(),
                HttpContext.Current?.User.Identity.Name ?? "anonymous",
                HttpContext.Current?.User.Identity.Name ?? "anonymous",
                ReportInitializerSection.GetReportInitializerSection().ReportPageViewer + "?ClassName=" + webReportManager.Plugin.GetType().FullName,
                HttpContext.Current?.Request.Url.GetLeftPart(UriPartial.Authority) ?? "https://srvmax.vvmvd.kz",
                Environment.MachineName,
                !isPreview && export,
                webReportManager.Plugin.GetType());

            if (export)
            {
                string mimeType;                
                if (useReturnStream)
                    return report.Render(format, "", null, out mimeType, out fileNameExtention);

                PageHelper.DownloadFile(
                    report.Render(format, "", null, out mimeType, out fileNameExtention),
                    webReportManager.Plugin.Description + "." + fileNameExtention, HttpContext.Current.Response);
            }

            return null;
        }



        protected void bBack_Click(object sender, EventArgs e)
        {
            Response.Redirect(Request.QueryString["backPath"]);
        }

        private static void Log(LogMonitor logMonitor, string guid, WebReportManager reportManager)
        {
            LogMessageType logType;
            string message;
            if (HttpContext.Current.Session["logcode" + guid] == null)
            {
                logType = reportManager.GetLogCode(reportManager.Plugin);
                message = reportManager.GetLogInformation().Replace("\r\n", "<br/>");
            }
            else
            {
                logType = (LogMessageType)HttpContext.Current.Session["logcode" + guid];
                message = (string)HttpContext.Current.Session["logmsg" + guid];
            }
            if (logType != LogMessageType.None)
            {
                logMonitor.Log(new LogMessageEntry(((WindowsIdentity)HttpContext.Current.User.Identity).User.Value, logType, message));
            }
        }

        public override string ResourceName
        {
            get { return Resources.SReportViewer; }
        }
    }
}