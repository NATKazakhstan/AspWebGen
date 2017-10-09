using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Security.Principal;
using System.Web;
using System.Web.Caching;
using System.Web.UI;
using System.Web.UI.WebControls;
using Nat.ReportManager.ReportGeneration.StimulSoft;
using Nat.Web.Controls;
using Nat.Web.ReportManager.CustomExport;
using Nat.Web.Tools;
using Nat.Web.Tools.Initialization;
using Stimulsoft.Report;
using Stimulsoft.Report.Web;
using Stimulsoft.Report.Components;
using System.Threading;

namespace Nat.Web.ReportManager
{
    using System.Collections;
    using System.Data;
    using System.Linq;
    using System.Web.Compilation;

    using Nat.Tools.Data;
    using Nat.Web.Controls.Trace;
    using Nat.Web.ReportManager.ReportGeneration;
    using Nat.Web.Tools.Security;

    using Stimulsoft.Report.Dictionary;
    using static Stimulsoft.Report.Web.StiWebViewer;

    public partial class ReportResultPage : BaseSPPage
    {
        static ReportResultPage()
        {
            WebInitializer.Initialize();
//            StiWebViewer.ExportNameRtf = "Microsoft Word";
//            StiWebViewer.ExportNameWord2007 = "Microsoft Word 2007";
            
        }

        protected override string OnSessionEmptyRedirect => ReportInitializerSection.GetReportInitializerSection().ReportPageViewer + "?open=off&setDefaultParams=on&ClassName=" + Request.QueryString["reportName"];
        
        protected void Page_Load(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(Request.QueryString["sr_print"]))
            {
                Page.LoadComplete += Page_OnLoadComplete;
                StiWebViewer1.ReportExport += StiWebViewer1OnReportExport;
            }
            //Page.Error += PageError;
        }
        
        private void StiWebViewer1OnReportExport(object sender, StiExportDataEventArgs stiExportDataEventArgs)
        {
            DBDataContext.AddViewReports(
                Tools.Security.User.GetSID(),
                HttpContext.Current.User.Identity.Name,
                HttpContext.Current.User.Identity.Name,
                ReportInitializerSection.GetReportInitializerSection().ReportPageViewer + "?ClassName=" + Request.QueryString["reportName"],
                HttpContext.Current.Request.Url.GetLeftPart(UriPartial.Authority),
                Environment.MachineName,
                true,
                BuildManager.GetType(Request.QueryString["reportName"], true, true));
        }

        private void PageError(object sender, EventArgs e)
        {
            if (StiWebViewer1 == null)
                return;

            if (StiWebViewer1.Report != null)
            {
                var renderingMessages = StiWebViewer1.Report.ReportRenderingMessages;
                if (renderingMessages == null
                    || renderingMessages.Count == 0)
                    Trace.WarnExt("ReportResultPage", "StiWebViewer1.Report.ReportRenderingMessages.Count == 0");
                else
                {
                    var messages = renderingMessages
                        .Cast<string>()
                        .Aggregate("ReportRenderingMessages: ", (current, message) => current + ("\r\n" + message));
                    Trace.WarnExt("ReportResultPage", messages);
                }
            }
            else
                Trace.WarnExt("ReportResultPage", "StiWebViewer1.Report is null");
        }

        private void Page_OnLoadComplete(object sender, EventArgs e)
        {
            if (!string.IsNullOrEmpty(Request.QueryString["backPath"]) &&
                !string.IsNullOrEmpty(Request.QueryString["text"]) 
                && bBack != null)
            {
                bBack.Visible = true;
                bBack.Text = Request.QueryString["text"];
            }

            if (IsPostBack) return;

            string guid = Request.QueryString["values"];
            var storageValues = (StorageValues)Session[guid];
            if (storageValues == null)
            {
                Response.Redirect(ReportInitializerSection.GetReportInitializerSection().ReportPageViewer + "?open=off&setDefaultParams=on&ClassName=" + Request.QueryString["reportName"]);
                return;
            }

            var expToWord = !string.IsNullOrEmpty(Request.QueryString["expword"]);
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
                expToWord,
                (Dictionary<string, object>)Session["constants" + guid],
                StiWebViewer1,
                out fileNameExtention,
                true);

            if (this.StiWebViewer1.Report != null
                && this.StiWebViewer1.Report.ReportRenderingMessages != null
                && this.StiWebViewer1.Report.ReportRenderingMessages.Count > 0)
            {
                foreach (var renderingMessage in this.StiWebViewer1.Report.ReportRenderingMessages)
                    this.Trace.WarnExt("ReportRendering", renderingMessage);
            }
        }
        
        public static Stream GetReport(bool useReturnStream, string pluginName, string guid,
            StorageValues storageValues, string culture, Page page, string format, string command,
            LogMonitor logMonitor, bool expToWord, Dictionary<string, object> constants, StiWebViewer report, out string fileNameExtention, bool RoleCheck)
        {
            var originalUICulture = Thread.CurrentThread.CurrentUICulture;
            var originalCulture = Thread.CurrentThread.CurrentCulture;
            try
            {
                if (string.IsNullOrEmpty(culture)) culture = "ru-ru";
                Thread.CurrentThread.CurrentUICulture =
                    Thread.CurrentThread.CurrentCulture =
                    new CultureInfo(culture == "ru" ? "ru-ru" : (culture == "kz" ? "kk-kz" : culture));

                fileNameExtention = "";
                WebInitializer.Initialize();
                var webReportManager = new WebReportManager(new TreeView());
                if (string.IsNullOrEmpty(pluginName))
                    return null;
                webReportManager.RoleCheck = RoleCheck;
                webReportManager.Plugin = webReportManager.GetPlugins()[pluginName];
                if (webReportManager.Plugin != null)
                {
                    webReportManager.Plugin.InitializeReportCulture(culture);
                    var values = storageValues;
                    var webReportPlugin = (IWebReportPlugin) webReportManager.Plugin;
                    var stiPlugin = (IStimulsoftReportPlugin) webReportPlugin;
                    webReportPlugin.Page = page;

                    webReportManager.CreateView();
                    
                    if (!string.IsNullOrEmpty(webReportManager.Plugin.CultureParameter))
                    {
                        values.SetStorageValues(
                            webReportManager.Plugin.CultureParameter, webReportManager.Plugin.InitializedKzCulture);
                    }

                    webReportManager.InitValues(values, webReportPlugin.InitSavedValuesInvisibleConditions);
                    webReportPlugin.Constants = constants;
                    if (!string.IsNullOrEmpty(webReportManager.Plugin.CultureParameter)
                        && stiPlugin.Report.Dictionary.Variables.Contains(webReportManager.Plugin.CultureParameter))
                    {
                        if (webReportManager.Plugin.SupportRuKz)
                        {
                            stiPlugin.Report[webReportManager.Plugin.CultureParameter] =
                                webReportManager.Plugin.InitializedKzCulture;
                        }
                        else
                        {
                            stiPlugin.Report[webReportManager.Plugin.CultureParameter] =
                                webReportManager.Plugin.InitializedCulture;
                        }
                    }

                    try
                    {
                        webReportManager.ShowReport();
                    }
                    catch (ConstraintException e)
                    {
                        var allErrors = webReportManager.Plugin.Table.DataSet?.GetAllErrors();
                        if (!string.IsNullOrEmpty(allErrors))
                            throw new Exception(allErrors, e);
                        var errors = webReportManager.Plugin.Table.GetErrors();
                        if (errors.Length > 0)
                            throw new Exception(errors.Select(r => r.RowError).Aggregate((v1, v2) => v1 + "; " + v2), e);
                        throw;
                    }

                    var section = ReportInitializerSection.GetReportInitializerSection();
                    if (section != null && !string.IsNullOrEmpty(section.PropSaveDataFile))
                    {
                        if (webReportManager.Plugin.Table.DataSet != null)
                            webReportManager.Plugin.Table.DataSet.WriteXml(section.PropSaveDataFile);
                        else
                            webReportManager.Plugin.Table.WriteXml(section.PropSaveDataFile);
                    }

                    if (HttpContext.Current != null && HttpContext.Current.Request.QueryString["GetDataSet"] == "on" && UserRoles.IsInRole(UserRoles.ADMIN))
                    {
                        using (var stream = new MemoryStream())
                        {
                            if (webReportManager.Plugin.Table.DataSet != null)
                                webReportManager.Plugin.Table.DataSet.WriteXml(stream);
                            else
                                webReportManager.Plugin.Table.WriteXml(stream);

                            stream.Position = 0;
                            PageHelper.DownloadFile(stream, "data.xml", HttpContext.Current.Response);
                        }
                    }

                    var autoExport = webReportPlugin as IReportAutoExport;

                    DBDataContext.AddViewReports(
                        Tools.Security.User.GetSID(),
                        HttpContext.Current?.User.Identity.Name ?? "anonymous",
                        HttpContext.Current?.User.Identity.Name ?? "anonymous",
                        ReportInitializerSection.GetReportInitializerSection().ReportPageViewer + "?ClassName=" + pluginName,
                        HttpContext.Current?.Request.Url.GetLeftPart(UriPartial.Authority) ?? "https://maxat",
                        Environment.MachineName,
                        useReturnStream || autoExport != null || expToWord || stiPlugin.AutoExportTo != null,
                        webReportManager.Plugin.GetType());

                    Log(logMonitor, guid, webReportManager);
                    if (useReturnStream)
                    {
                        StiExportFormat stiExportFormat;
                        try
                        {
                            stiExportFormat = (StiExportFormat) Enum.Parse(typeof (StiExportFormat), format);
                        }
                        catch (Exception)
                        {
                            var stiCustomExportType = (CustomExportType) Enum.Parse(typeof (CustomExportType), format);

                            return ExportWithoutShow(
                                webReportManager.Report,
                                stiCustomExportType,
                                true,
                                out fileNameExtention);
                        }

                        fileNameExtention = WebReportManager.GetFileExtension(stiExportFormat);
                        return ExportWithoutShow(webReportManager.Report, stiExportFormat, true);
                    }

                    //webReportManager.Report.EndRender += (sender, args) => LogMessages(webReportManager.Report, logMonitor);
                    if (autoExport != null)
                        autoExport.Export();

                    if (!expToWord && stiPlugin.AutoExportTo == null)
                    {
                        report.Report = webReportManager.Report;
                        page.Cache.Insert(report.Report.ReportName,
                                          report.Report,
                                          null,
                                          Cache.NoAbsoluteExpiration,
                                          new TimeSpan(0, 10, 0),
                                          CacheItemPriority.NotRemovable,
                                          null);
                        report.ResetCurrentPage();
                    }
                    else if (webReportPlugin.CustomExportType != CustomExportType.None)
                    {
                        return ExportWithoutShow(
                            webReportManager.Report,
                            webReportPlugin.CustomExportType,
                            false,
                            out fileNameExtention);
                    }
                    else if (stiPlugin.AutoExportTo != null)
                    {
                        return ExportWithoutShow(webReportManager.Report, stiPlugin.AutoExportTo.Value, false);
                    }
                    else
                    {
                        return ExportWithoutShow(webReportManager.Report, StiExportFormat.Word2007, false);
                    }
                }
            }
            finally
            {
                Thread.CurrentThread.CurrentUICulture = originalUICulture;
                Thread.CurrentThread.CurrentCulture = originalCulture;
            }

            return null;
        }

        private static void LogMessages(StiReport report, LogMonitor logMonitor)
        {
            if (report.ReportRenderingMessages == null || report.ReportRenderingMessages.Count <= 0)
                return;
        }

        private static void LocalizeReport(StiReport report, string cultureName)
        {
            var container = report.GlobalizationStrings[cultureName];
            if (container == null) return;

            LocalizeReport(report, container);
        }

        private static void LocalizeReport(StiReport report, StiGlobalizationContainer container)
        {
            var components = report.RenderedPages.OfType<StiPage>().Select(r => r.GetComponents()).ToList();
            var hashtable = new Hashtable();
            foreach (StiVariable variable in report.Dictionary.Variables)
            {
                hashtable["Variable." + variable.Name] = variable;
            }
            foreach (StiGlobalizationItem item in container.Items)
            {
                string propertyName = item.PropertyName;
                if (propertyName == "ReportAuthor")
                {
                    report.ReportAuthor = item.Text;
                }
                else
                {
                    if (propertyName == "ReportDescription")
                    {
                        report.ReportDescription = item.Text;
                        continue;
                    }
                    if (propertyName == "ReportAlias")
                    {
                        report.ReportAlias = item.Text;
                        continue;
                    }
                    if (hashtable[propertyName] is StiVariable)
                    {
                        StiVariable variable2 = hashtable[propertyName] as StiVariable;
                        variable2.Value = item.Text;
                        continue;
                    }
                    int index = propertyName.IndexOf(".");
                    if (index != -1)
                    {
                        string str2 = propertyName.Substring(0, index);
                        string str3 = propertyName.Substring(index + 1);
                        foreach (var collection in components)
                        {
                            var provider = collection[str2] as IStiGlobalizationProvider;
                            if (provider != null)
                                provider.SetString(str3, item.Text);
                        }
                    }
                }
            }
        }

        private static void Log(LogMonitor logMonitor, string guid, WebReportManager reportManager)
        {
            LogMessageType logType;
            string message;
            if (HttpContext.Current != null && HttpContext.Current.Session != null && HttpContext.Current.Session["logcode" + guid] != null)
            {
                logType = (LogMessageType)HttpContext.Current.Session["logcode" + guid];
                message = (string)HttpContext.Current.Session["logmsg" + guid];
            }
            else
            {
                logType = reportManager.GetLogCode(reportManager.Plugin);
                message = reportManager.GetLogInformation().Replace("\r\n", "<br/>");
            }
            if (logType != LogMessageType.None)
            {
                logMonitor.Log(new LogMessageEntry(logType, message));
            }
        }

        private List<object> TestCache()
        {
            var enumerator = Page.Cache.GetEnumerator();
            var list = new List<object>();
            while (enumerator.MoveNext())
                list.Add(enumerator.Value);
            return list;
        }

        protected void bBack_Click(object sender, EventArgs e)
        {
            Response.Redirect(Request.QueryString["backPath"]);
        }

        private static Stream ExportWithoutShow(StiReport stiReport, StiExportFormat exportFormat, bool useReturnStream)
        {
            stiReport.Render(false);

            if (stiReport.AutoLocalizeReportOnRun)
                LocalizeReport(stiReport, Thread.CurrentThread.CurrentUICulture.TwoLetterISOLanguageName);

            // ReSharper disable AccessToStaticMemberViaDerivedType
            var exportingFileName = Path.ChangeExtension(GetReportFileName(stiReport), WebReportManager.GetFileExtension(exportFormat));
            // ReSharper restore AccessToStaticMemberViaDerivedType
            var stream = new MemoryStream();
            stiReport.ExportDocument(exportFormat, stream);
            stream.Position = 0;
            if (!useReturnStream)
                PageHelper.DownloadFile(stream, exportingFileName, HttpContext.Current.Response);
            else 
                return stream;
            return null;
        }

        private static string GetReportFileName(StiReport stiReport)
        {
            if (stiReport.GetType().Name == stiReport.ReportName)
                return stiReport.ReportName != stiReport.ReportAlias ? stiReport.ReportAlias : (stiReport.ReportDescription ?? stiReport.ReportName);
            return stiReport.ReportName;
        }

        private static Stream ExportWithoutShow(StiReport stiReport, CustomExportType exportFormat, bool useReturnStream, out string fileNameExtention)
        {
            stiReport.Render(false);

            if (stiReport.AutoLocalizeReportOnRun)
                LocalizeReport(stiReport, Thread.CurrentThread.CurrentUICulture.TwoLetterISOLanguageName);

            var stream = new MemoryStream();
            switch (exportFormat)
            {
                case CustomExportType.RtfNonTable:
                    fileNameExtention = "Rtf";
                    var rtfExporter = new StiRtfExportService();
                    rtfExporter.ExportRtf(stiReport, stream);
                    break;
                default:
                    throw new ArgumentOutOfRangeException("exportFormat");
            }
            stream.Position = 0;
            if (!useReturnStream)
            {
                var exportingFileName = Path.ChangeExtension(GetReportFileName(stiReport), fileNameExtention);
                PageHelper.DownloadFile(stream, exportingFileName, HttpContext.Current.Response);
            }
            else 
                return stream;
            return null;
        }

        private static void CustomExport(StiReport stiReport)
        {
            var pageRange = new StiPagesRange();
            StiPagesCollection selectedPages = pageRange.GetSelectedPages(stiReport.RenderedPages);
            foreach (StiPage page in selectedPages)
            {
                selectedPages.GetPage(page);
                foreach (StiComponent component in page.Components)
                {
                    if (!component.Enabled)
                    {
                        continue;
                    }
                }
            }
        }
    }
}