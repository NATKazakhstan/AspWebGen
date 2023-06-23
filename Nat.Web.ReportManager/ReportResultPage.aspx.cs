using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Net.Sockets;
using System.Web;
using System.Web.Caching;
using System.Web.UI.WebControls;
using Nat.ReportManager.ReportGeneration.StimulSoft;
using Nat.Web.Controls;
using Nat.Web.ReportManager.CustomExport;
using Nat.Web.Tools;
using Nat.Web.Tools.Initialization;
using Nat.Web.Tools.Report;
using Stimulsoft.Report;
using Stimulsoft.Report.Web;
using Stimulsoft.Report.Components;
using System.Threading;
using System.Web.Mvc;
using Stimulsoft.Base.Drawing;
using Page = System.Web.UI.Page;

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
            LogMonitor logMonitor, bool expToWord, Dictionary<string, object> constants, StiWebViewer report, out string fileNameExtention, bool RoleCheck,
            bool isPreview = false)
        {
            return GetReport(useReturnStream, pluginName, guid, storageValues, culture, page, format, command, logMonitor, expToWord, constants, report,
                out _, out fileNameExtention, RoleCheck, isPreview);
        }

        public static Stream GetReport(bool useReturnStream, string pluginName, string guid,
            StorageValues storageValues, string culture, Page page, string format, string command,
            LogMonitor logMonitor, bool expToWord, Dictionary<string, object> constants, StiWebViewer report, out string fileName, out string fileNameExtension, bool RoleCheck, bool isPreview)
        {
            var originalUICulture = Thread.CurrentThread.CurrentUICulture;
            var originalCulture = Thread.CurrentThread.CurrentCulture;
            try
            {
                if (string.IsNullOrEmpty(culture)) culture = "ru-ru";
                Thread.CurrentThread.CurrentUICulture =
                    Thread.CurrentThread.CurrentCulture =
                    new CultureInfo(culture == "ru" ? "ru-ru" : (culture == "kz" ? "kk-kz" : culture));

                fileNameExtension = "";
                fileName = "";
                WebInitializer.Initialize();
                var webReportManager = new WebReportManager(new TreeView());
                if (string.IsNullOrEmpty(pluginName))
                    return null;
                webReportManager.RoleCheck = RoleCheck;
                webReportManager.Plugin = webReportManager.GetPlugins()[pluginName];
                if (webReportManager.Plugin == null)
                    return null;
                
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
                
                var autoExport = webReportPlugin as IReportAutoExport;
                var isExport = !isPreview && (useReturnStream || autoExport != null || expToWord || stiPlugin.AutoExportTo != null);

                var logId = Log(logMonitor, guid, webReportManager,
                    isExport ? LogReportGenerateCodeType.Export : LogReportGenerateCodeType.Preview);
                var qrCodeTextFormat = DependencyResolver.Current.GetService<IQrCodeTextFormat>();
                if (logId != null)
                {
                    stiPlugin.Report["IsHtml"] = format != null && format.ToLower().Equals("html"); 
                    stiPlugin.Report["BarCodeLogText"] = qrCodeTextFormat.GetUserText(logId.Value);
                    stiPlugin.Report["BarCodeLogData"] = qrCodeTextFormat.GetQrCodeData(logId.Value);
                    stiPlugin.Report["BarCodeLogImageH"] = qrCodeTextFormat.GetHorizontalImage();
                    stiPlugin.Report["BarCodeLogImageV"] = qrCodeTextFormat.GetVerticalImage();
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

                DBDataContext.AddViewReports(
                    Tools.Security.User.GetSID(),
                    HttpContext.Current?.User.Identity.Name ?? "anonymous",
                    HttpContext.Current?.User.Identity.Name ?? "anonymous",
                    ReportInitializerSection.GetReportInitializerSection().ReportPageViewer + "?ClassName=" + pluginName,
                    HttpContext.Current?.Request.Url.GetLeftPart(UriPartial.Authority) ?? "https://srvmax.vvmvd.kz",
                    Environment.MachineName,
                    isExport,
                    webReportManager.Plugin.GetType());

                var availableFormat = WebReportManager.GetAvailableStiFormat(stiPlugin).ToList();
                if (useReturnStream)
                {
                    StiExportFormat? stiExportFormat = null;
                    CustomExportType? stiCustomExportType = null;
                    try
                    {
                        if ("Auto".Equals(format, StringComparison.InvariantCulture))
                        {
                            stiExportFormat = stiPlugin.AutoExportTo == null 
                                ? availableFormat.First()
                                : (availableFormat.Contains(stiPlugin.AutoExportTo.Value)
                                    ? stiPlugin.AutoExportTo.Value 
                                    : availableFormat.First());
                            stiCustomExportType = webReportPlugin.CustomExportType;
                        }
                        else if ("Word".Equals(format, StringComparison.InvariantCulture))
                            stiExportFormat = availableFormat.Contains(StiExportFormat.Word2007)
                                ? StiExportFormat.Word2007
                                : availableFormat.First();
                        else if ("Excel".Equals(format, StringComparison.InvariantCulture))
                            stiExportFormat = availableFormat.Contains(StiExportFormat.Excel2007) ? StiExportFormat.Excel2007 : availableFormat.First();
                        else
                            stiExportFormat = (StiExportFormat) Enum.Parse(typeof(StiExportFormat), format);
                    }
                    catch (ArgumentException)
                    {
                        stiCustomExportType = (CustomExportType) Enum.Parse(typeof(CustomExportType), format);
                    }

                    AddWatermark(webReportManager);

                    if (stiCustomExportType != null && stiCustomExportType != CustomExportType.None)
                    {
                        return ExportWithoutShow(
                            webReportManager.Report,
                            stiCustomExportType.Value,
                            true,
                            out fileNameExtension, 
                            out fileName);
                    }

                    fileNameExtension = WebReportManager.GetFileExtension(stiExportFormat.Value);
                    return ExportWithoutShow(webReportManager, stiExportFormat.Value, true, out fileName);
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
                        out fileNameExtension, 
                        out fileName);
                }
                else if (stiPlugin.AutoExportTo != null)
                {
                    return ExportWithoutShow(webReportManager, stiPlugin.AutoExportTo.Value, false, out fileName);
                }
                else
                {
                    return ExportWithoutShow(webReportManager, availableFormat.First(), false, out fileName);
                }
            }
            finally
            {
                Thread.CurrentThread.CurrentUICulture = originalUICulture;
                Thread.CurrentThread.CurrentCulture = originalCulture;
            }

            return null;
        }

        private static void AddWatermark(WebReportManager webReportManager)
        {
            var rWatermark = DependencyResolver.Current.GetService<IWatermark>();
            var watermarkImage = rWatermark.GetImage(webReportManager.Plugin.GetType().FullName);
            if (watermarkImage == null) return;
            var watermark = new StiWatermark
            {
                AspectRatio = true,
                Image = StiImageConverter.BytesToImage(watermarkImage),
                ImageTiling = false
            };
            foreach (StiPage pg in webReportManager.Report.Pages)
            {
                pg.Watermark = watermark;
            }
        }

        public static Stream GetCustomStreamReport(string pluginName, string guid, StorageValues storageValues, string culture, Page page, string format, LogMonitor logMonitor, bool expToWord, Dictionary<string, object> constants, out string fileName, out string fileNameExtension, bool RoleCheck, bool isPreview)
        {
            var originalUICulture = Thread.CurrentThread.CurrentUICulture;
            var originalCulture = Thread.CurrentThread.CurrentCulture;
            
            try
            {
                if (string.IsNullOrEmpty(culture)) culture = "ru-ru";
                Thread.CurrentThread.CurrentUICulture =
                    Thread.CurrentThread.CurrentCulture =
                    new CultureInfo(culture == "ru" ? "ru-ru" : (culture == "kz" ? "kk-kz" : culture));

                fileNameExtension = "";
                fileName = "";
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
                    var stiPlugin = (ICustomStreamReport) webReportPlugin;
                    webReportPlugin.Page = page;

                    webReportManager.CreateView();
                    
                    if (!string.IsNullOrEmpty(webReportManager.Plugin.CultureParameter))
                    {
                        values.SetStorageValues(
                            webReportManager.Plugin.CultureParameter, webReportManager.Plugin.InitializedKzCulture);
                    }

                    webReportManager.InitValues(values, webReportPlugin.InitSavedValuesInvisibleConditions);
                    webReportPlugin.Constants = constants;
                    
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

                    var isExport = !isPreview;
                    var logId = Log(logMonitor, guid, webReportManager, 
                                    isExport ? LogReportGenerateCodeType.Export : LogReportGenerateCodeType.Preview)
                                ?? 0;
                    var qrCodeTextFormat = DependencyResolver.Current.GetService<IQrCodeTextFormat>();
                    using (var hImage = qrCodeTextFormat.GetHorizontalImageStream())
                    using (var vImage = qrCodeTextFormat.GetVerticalImageStream())
                    using (var qrCodeStream = qrCodeTextFormat.GetQrCodeImageStream(logId))
                    {
                        stiPlugin.BarCodeLogText = qrCodeTextFormat.GetUserText(logId);
                        stiPlugin.BarCodeLogData = qrCodeTextFormat.GetQrCodeData(logId);
                        stiPlugin.BarCodeLogDataStream = qrCodeStream;
                        stiPlugin.BarCodeLogImageH = hImage;
                        stiPlugin.BarCodeLogImageV = vImage;

                        DBDataContext.AddViewReports(
                            Tools.Security.User.GetSID(),
                            HttpContext.Current?.User.Identity.Name ?? "anonymous",
                            HttpContext.Current?.User.Identity.Name ?? "anonymous",
                            ReportInitializerSection.GetReportInitializerSection().ReportPageViewer + "?ClassName=" + pluginName,
                            HttpContext.Current?.Request.Url.GetLeftPart(UriPartial.Authority) ?? "https://srvmax.vvmvd.kz",
                            Environment.MachineName,
                            isExport,
                            webReportManager.Plugin.GetType());

                        var availableFormat = WebReportManager.GetAvailableFormat(stiPlugin).ToList();
                        ExportFormat stiExportFormat;

                        if ("Auto".Equals(format, StringComparison.InvariantCulture))
                        {
                            stiExportFormat = availableFormat.First();
                        }
                        else if ("Word".Equals(format, StringComparison.InvariantCulture))
                            stiExportFormat = availableFormat.Contains(ExportFormat.Word)
                                ? ExportFormat.Word
                                : availableFormat.First();
                        else if ("Excel".Equals(format, StringComparison.InvariantCulture))
                            stiExportFormat = availableFormat.Contains(ExportFormat.Excel) ? ExportFormat.Excel : availableFormat.First();
                        else
                            stiExportFormat = (ExportFormat) Enum.Parse(typeof(ExportFormat), format);

                        return stiPlugin.Export(stiExportFormat, out fileName, out fileNameExtension);
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

        private static long? Log(LogMonitor logMonitor, string guid, WebReportManager reportManager, LogReportGenerateCodeType type)
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
                var logInfo = DependencyResolver.Current.GetService<ILogReportGenerateCode>();
                logType = (LogMessageType) logInfo.GetCodeFor(
                    reportManager.Plugin.GetType().FullName,
                    reportManager.Plugin.Description,
                    (long) logType,
                    type);
                message = reportManager.GetLogInformation().Replace("\r\n", "<br/>");
            }

            var logId = logMonitor.WriteLog(new LogMessageEntry(logType, message));
            if (logId != null && !string.IsNullOrEmpty(HttpContext.Current?.Request.UserHostAddress))
            {
                try
                {
                    logMonitor.WriteFieldChanged(logId.Value, string.Empty, "Имя ПК пользователя",
                        System.Net.Dns.GetHostEntry(HttpContext.Current.Request.UserHostAddress).HostName, "");
                }
                catch (SocketException)
                {
                }
            }

            return logId;
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

        private static Stream ExportWithoutShow(WebReportManager webReportManager, StiExportFormat exportFormat, bool useReturnStream, out string fileName)
        {
            StiReport stiReport = webReportManager.Report;
            stiReport.Render(false);
            if (exportFormat == StiExportFormat.Html)
                stiReport.Bookmark?.Bookmarks.Clear();

            if (stiReport.AutoLocalizeReportOnRun)
                LocalizeReport(stiReport, Thread.CurrentThread.CurrentUICulture.TwoLetterISOLanguageName);

            // ReSharper disable AccessToStaticMemberViaDerivedType
            // ReSharper restore AccessToStaticMemberViaDerivedType
            var stream = new MemoryStream();
            stiReport.ExportDocument(exportFormat, stream);
            stream.Position = 0;

            AddWatermark(webReportManager, exportFormat, stream);
            
            fileName = GetReportFileName(stiReport);
            if (!useReturnStream)
            {
                var exportingFileName = Path.ChangeExtension(fileName, WebReportManager.GetFileExtension(exportFormat));
                PageHelper.DownloadFile(stream, exportingFileName, HttpContext.Current.Response);
            }
            else 
                return stream;
            return null;
        }

        private static void AddWatermark(WebReportManager webReportManager, StiExportFormat exportFormat, MemoryStream stream)
        {
            if (exportFormat == StiExportFormat.Excel2007)
            {
                var rw = DependencyResolver.Current.GetService<IReportWatermark>();
                rw.AddToExcelStream(stream, webReportManager.Plugin.GetType().FullName);
            }
            else if (exportFormat == StiExportFormat.Word2007)
            {
                var rw = DependencyResolver.Current.GetService<IReportWatermark>();
                rw.AddToWordStream(stream, webReportManager.Plugin.GetType().FullName);
            }
        }

        private static string GetReportFileName(StiReport stiReport)
        {
            if (!string.IsNullOrEmpty(stiReport.ReportDescription))
                return stiReport.ReportDescription;
            if (stiReport.GetType().Name == stiReport.ReportName)
                return stiReport.ReportName != stiReport.ReportAlias ? stiReport.ReportAlias : (stiReport.ReportDescription ?? stiReport.ReportName);
            return stiReport.ReportName;
        }

        private static Stream ExportWithoutShow(StiReport stiReport, CustomExportType exportFormat, bool useReturnStream, out string fileNameExtension, out string fileName)
        {
            stiReport.Render(false);

            if (stiReport.AutoLocalizeReportOnRun)
                LocalizeReport(stiReport, Thread.CurrentThread.CurrentUICulture.TwoLetterISOLanguageName);

            var stream = new MemoryStream();
            switch (exportFormat)
            {
                case CustomExportType.RtfNonTable:
                    fileNameExtension = "Rtf";
                    var rtfExporter = new StiRtfExportService();
                    rtfExporter.ExportRtf(stiReport, stream);
                    break;
                default:
                    throw new ArgumentOutOfRangeException("exportFormat");
            }
            stream.Position = 0;
            fileName = GetReportFileName(stiReport);
            if (!useReturnStream)
            {
                var exportFileName = Path.ChangeExtension(fileName, fileNameExtension);
                PageHelper.DownloadFile(stream, exportFileName, HttpContext.Current.Response);
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