using System.Web.Mvc;
using Nat.Web.Tools.Report;

namespace Nat.ExportInExcel
{
    using System;
    using System.Collections.Generic;
    using System.Data.Linq;
    using System.Globalization;
    using System.IO;
    using System.Reflection;
    using System.Text;
    using System.Threading;
    using System.Web;
    using System.Web.Compilation;

    using Nat.Web.Controls;
    using Nat.Web.Controls.Data;
    using Nat.Web.Controls.GenerationClasses;
    using Nat.Web.Controls.GenerationClasses.BaseJournal;
    using Nat.Web.Controls.GenerationClasses.Navigator;
    using Nat.Web.Controls.Properties;
    using Nat.Web.Tools;
    using Nat.Web.Tools.Export;
    using Nat.Web.Tools.Security;

    public class Exporter : IExporter
    {
        public Stream GetExcelByType(Type journalType, string format, object properties, string culture, ILogMonitor logMonitor, bool checkPermit, out string fileNameExtention)
        {
            var rvsProp = (RvsSavedProperties) properties;
            rvsProp.Format = format;
            rvsProp.Culture = culture;
            return GetExcelByType(journalType, rvsProp, logMonitor, checkPermit, out fileNameExtention);
        }

        public Stream GetExcelByType(Type journalType, object properties, ILogMonitor logMonitor, bool checkPermit, out string fileNameExtention)
        {
            return GetExcelByType(journalType, (RvsSavedProperties)properties, logMonitor, checkPermit, out fileNameExtention);
        }

        public Stream GetExcelByType(Type journalType, string format, long idProperties, StorageValues storageValues, string culture, ILogMonitor logMonitor, bool checkPermit, out string fileNameExtention)
        {
            var rvsProp = RvsSavedProperties.LoadFrom(idProperties, logMonitor);
            rvsProp.Format = format;
            rvsProp.Culture = culture;
            rvsProp.StorageValues = storageValues;
            return GetExcelByType(journalType, rvsProp, logMonitor, checkPermit, out fileNameExtention);
        }

        public Stream GetExcelByTypeName(string journalType, string format, long idProperties, StorageValues storageValues, string culture, ILogMonitor logMonitor, bool checkPermit, out string fileNameExtention)
        {
            var rvsProp = RvsSavedProperties.LoadFrom(idProperties, logMonitor);
            rvsProp.Format = format;
            rvsProp.Culture = culture;
            rvsProp.StorageValues = storageValues;
            journalType = journalType ?? rvsProp.JournalTypeName;
            var type = Type(journalType);
            return GetExcelByType(type, rvsProp, logMonitor, checkPermit, out fileNameExtention);
        }
        private static Type Type(string journalType)
        {
            if (journalType.Contains("1.0.0.0, Culture=neutral, PublicKeyToken=55f6c56e6ab9709a"))
                journalType = journalType.Replace("1.0.0.0", "1.4.0.0");
            return System.Type.GetType(journalType) ?? BuildManager.GetType(journalType, false, true);
        }

        /* public Stream GetExcelByTypeName(string journalType, string format, long idProperties, ILogMonitor logMonitor, bool checkPermit)
        {
            var properties = RvsSavedProperties.LoadFrom(idProperties);
            return GetExcelByTypeName(journalType, properties, logMonitor, checkPermit);
        }

        public Stream GetExcelByTypeName(string journalType, RvsSavedProperties properties, ILogMonitor logMonitor, bool checkPermit)
        {
            var type = Type.GetType(journalType) ?? BuildManager.GetType(journalType, false, true);
            return GetExcelByType(type, properties, logMonitor, checkPermit);
        }
        */

        public Stream GetExcelByType(Type journalType, RvsSavedProperties properties, ILogMonitor logMonitor, bool checkPermit, out string fileNameExtention)
        {
            var oldCulture = Thread.CurrentThread.CurrentUICulture;
            if (!string.IsNullOrEmpty(properties.Culture))
                Thread.CurrentThread.CurrentUICulture = new CultureInfo(properties.Culture);
            try
            {
                var obj = Activator.CreateInstance(journalType);
                var methodInfo = typeof (Exporter).GetMethod("GetExcel");
                Type[] gTypes = null;
                if (journalType.IsGenericType)
                    gTypes = journalType.GetGenericArguments();
                else if (journalType.BaseType.IsGenericType)
                    gTypes = journalType.BaseType.GetGenericArguments();
                else if (journalType.BaseType.BaseType.IsGenericType)
                    gTypes = journalType.BaseType.BaseType.GetGenericArguments();

                methodInfo = methodInfo.MakeGenericMethod(gTypes);
                fileNameExtention = GetFileNameExtension(properties.Format);
                return (Stream)methodInfo.Invoke(null, new[] { obj, properties, logMonitor, checkPermit });
            }
            finally
            {
                if (!string.IsNullOrEmpty(properties.Culture))
                    Thread.CurrentThread.CurrentUICulture = oldCulture;
            }
        }

        public static Stream GetExcel<TDataContext, TFilterControl, TKey, TTable, TDataSource, TRow, TJournal, TNavigatorControl, TNavigatorValues, TFilter>(
            BaseJournalUserControl<TDataContext, TFilterControl, TKey, TTable, TDataSource, TRow, TJournal, TNavigatorControl, TNavigatorValues, TFilter>
                journalControl,
            RvsSavedProperties properties,
            ILogMonitor logMonitor,
            bool checkPermit)

            where TDataContext : DataContext, new()
            where TKey : struct
            where TTable : class
            where TFilterControl : BaseFilterControl<TKey, TTable>, new()
            where TDataSource : BaseDataSource<TKey, TTable, TDataContext, TRow>
            where TRow : BaseRow, new()
            where TJournal : BaseJournalControl<TKey, TTable, TRow, TDataContext>
            where TNavigatorControl : BaseNavigatorControl<TNavigatorValues>
            where TNavigatorValues : BaseNavigatorValues, new()
            where TFilter : BaseFilter<TKey, TTable, TDataContext>, new()
        {
            Stream stream;

            journalControl.LogMonitor = logMonitor;
            journalControl.Url = MainPageUrlBuilder.Current.Clone();
            if (checkPermit) journalControl.CheckExportPermit();
            bool isXml = "xml".Equals(properties.Format, StringComparison.OrdinalIgnoreCase);
            if (isXml)
            {
                var exporterXml = new ExporterXml<TDataContext, TFilterControl, TKey, TTable, TDataSource, TRow, TJournal,
                    TNavigatorControl, TNavigatorValues, TFilter> { LogMonitor = logMonitor };
                stream = exporterXml.GetExcel(journalControl, properties);
            }
            else
            {
                var exporterXslx = new ExporterXslx<TDataContext, TFilterControl, TKey, TTable, TDataSource, TRow, TJournal,
                    TNavigatorControl, TNavigatorValues, TFilter> { LogMonitor = logMonitor };
                stream = exporterXslx.GetExcel(journalControl, properties);
                var watermark = DependencyResolver.Current.GetService<IReportWatermark>();
                watermark.AddToExcelStream(stream, journalControl.ReportPluginName);
            }

            var plugin = BuildManager.GetType(properties.ReportPluginName, false, true) ?? GetTypeByReportManager(properties.ReportPluginName);
            DBDataContext.AddViewReports(
                User.GetSID(),
                HttpContext.Current.User.Identity.Name,
                HttpContext.Current.User.Identity.Name,
                HttpContext.Current.Request.Url.PathAndQuery,
                HttpContext.Current.Request.Url.GetLeftPart(UriPartial.Authority),
                Environment.MachineName,
                true,
                plugin);

            var logInfo = (ILogReportGenerateCode) Activator.CreateInstance(BuildManager.GetType("Event_LOG.LogReportGenerateCode", true, false));
            var logType = (LogMessageType) logInfo.GetCodeFor(
                plugin.FullName,
                properties.NameRu,
                (long) journalControl.ExportLog,
                LogReportGenerateCodeType.Export);

            var logId = logMonitor.WriteLog(
                new LogMessageEntry(
                    User.GetSID(),
                    logType,
                    properties.NameRu,
                    journalControl.OnExportNewSavedProperties ? RvsSavedProperties.GetFromJournal(journalControl) : properties));

            if (!isXml)
            {
                AddQrCode(stream, logId);
            }
            
            return stream;
        }

        private static void AddQrCode(Stream stream, long? logId)
        {
            if(logId == null)return;
            var qrCodeTool = DependencyResolver.Current.GetService<IReportQr>();
            qrCodeTool.AddToExcel(stream, logId.Value);
        }

        private static Type GetTypeByReportManager(string pluginName)
        {
            var type = BuildManager.GetType("Nat.Web.ReportManager.WebReportManager, Nat.Web.ReportManager, Version=1.4.0.0, Culture=neutral, PublicKeyToken=11c252a207597415", true, true);
            var getPlugin = type.GetMethod("GetPlugin", BindingFlags.Public | BindingFlags.Static);
            return getPlugin.Invoke(null, new object[] { pluginName })?.GetType();
        }

        public string GetFileNameExtension(string format)
        {
            if ("xml".Equals(format, StringComparison.OrdinalIgnoreCase))
                return "xml";
            return "xlsx";
        }

        public Stream GetExcelStream(JournalExportEventArgs args)
        {
            var docLocation = args.ViewJournalUrl.ToString();
            var startIndex = docLocation.IndexOf("?", StringComparison.Ordinal);
            var pluginName = startIndex > -1 ? docLocation.Substring(0, startIndex) : docLocation;
            DBDataContext.AddViewReports(
                User.GetSID(),
                HttpContext.Current.User.Identity.Name,
                HttpContext.Current.User.Identity.Name,
                docLocation,
                HttpContext.Current.Request.Url.GetLeftPart(UriPartial.Authority),
                Environment.MachineName,
                true,
                pluginName,
                LocalizationHelper.IsCultureKZ ? null : args.Header,
                LocalizationHelper.IsCultureKZ ? args.Header : null);
            
            var export = new ExporterXslxByArgs();
            var stream = export.GetExcel(args);
            
            var logInfo = (ILogReportGenerateCode) Activator.CreateInstance(BuildManager.GetType("Event_LOG.LogReportGenerateCode", true, false));
            var logType = logInfo.GetCodeFor(pluginName, args.Header, args.ExportLog, LogReportGenerateCodeType.Export);
            args.LogMonitor.Log(
                logType,
                () =>
                {
                    var message = new StringBuilder();
                    message.Append(args.Header);
                    if (args.FilterValues != null)
                    {
                        message.Append("; Фильтры: \r\n");
                        foreach (var value in args.FilterValues)
                        {
                            message.Append(value);
                            message.Append(";\r\n");
                        }
                    }

                    message.AddHyperLink(docLocation, Resources.SViewJournal, string.Empty);
                    return new LogMessageEntry(User.GetSID(), args.ExportLog, message.ToString());
                });

            return stream;
        }
        
        public ExportResultArgs GetExcelResult(JournalExportEventArgs args)
        {
            try
            {
                var export = new ExporterXslxByArgs();
                var stream = export.GetExcel(args);
                if (args.ExportLog != 0)
                {
                    args.LogMonitor.Log(
                        args.ExportLog,
                        () =>
                            {
                                var message = new StringBuilder();
                                message.Append(args.Header);
                                if (args.FilterValues != null)
                                {
                                    message.Append("; Фильтры: \r\n");
                                    foreach (var value in args.FilterValues)
                                    {
                                        message.Append(value);
                                        message.Append(";\r\n");
                                    }
                                }

                                return new LogMessageEntry(User.GetSID(), args.ExportLog, message.ToString());
                            });
                }
                return new ExportResultArgs
                    {
                        Stream = stream
                    };
            }
            catch (Exception e)
            {
                return new ExportResultArgs
                    {
                        Exception = e,
                        ErrorMessage = e.ToString()
                    };
            }
        }
    }
}