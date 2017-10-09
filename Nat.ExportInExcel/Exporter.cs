namespace Nat.ExportInExcel
{
    using System;
    using System.Data.Linq;
    using System.Globalization;
    using System.IO;
    using System.Security.Principal;
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
            var type = Type.GetType(journalType) ?? BuildManager.GetType(journalType, false, true);
            return GetExcelByType(type, rvsProp, logMonitor, checkPermit, out fileNameExtention);
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
            BaseJournalUserControl<TDataContext, TFilterControl, TKey, TTable, TDataSource, TRow, TJournal, TNavigatorControl, TNavigatorValues, TFilter> journalControl,
            RvsSavedProperties properties, ILogMonitor logMonitor, bool checkPermit)

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
            var identity = Thread.CurrentPrincipal.Identity as WindowsIdentity;
            var sid = identity == null ? Thread.CurrentPrincipal.Identity.Name : identity.User.Value;
            logMonitor.Log(
                new LogMessageEntry(
                    sid,
                    journalControl.ExportLog,
                    properties.NameRu,
                    properties));

            DBDataContext.AddViewReports(
                User.GetSID(),
                HttpContext.Current.User.Identity.Name,
                HttpContext.Current.User.Identity.Name,
                HttpContext.Current.Request.Url.PathAndQuery,
                HttpContext.Current.Request.Url.GetLeftPart(UriPartial.Authority),
                Environment.MachineName,
                true,
                BuildManager.GetType(properties.ReportPluginName, true, true));

            if ("xml".Equals(properties.Format, StringComparison.OrdinalIgnoreCase))
            {
                var exporterXml = new ExporterXml<TDataContext, TFilterControl, TKey, TTable, TDataSource, TRow, TJournal,
                    TNavigatorControl, TNavigatorValues, TFilter>();
                journalControl.LogMonitor = logMonitor;
                if (checkPermit) journalControl.CheckExportPermit();
                exporterXml.LogMonitor = logMonitor;
                var stream = exporterXml.GetExcel(journalControl, properties);
                /*logMonitor.Log(
                    new LogMessageEntry(
                        sid,
                        journalControl.ExportLog,
                        properties.NameRu,
                        RvsSavedProperties.GetFromJournal(journalControl)));*/
                
                return stream;
            }

            var exporterXslx = new ExporterXslx<TDataContext, TFilterControl, TKey, TTable, TDataSource, TRow, TJournal,
                TNavigatorControl, TNavigatorValues, TFilter>();
            journalControl.LogMonitor = logMonitor;
            if (checkPermit) journalControl.CheckExportPermit();
            exporterXslx.LogMonitor = logMonitor;
            var excel = exporterXslx.GetExcel(journalControl, properties);
            /*logMonitor.Log(
                new LogMessageEntry(
                    sid,
                    journalControl.ExportLog,
                    properties.NameRu,
                    RvsSavedProperties.GetFromJournal(journalControl)));*/
            return excel;
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
            if (args.ExportLog != 0)
            {
                args.LogMonitor.Log(
                    args.ExportLog,
                    () =>
                        {
                            var message = new StringBuilder();
                            message.Append(args.Header);
                            message.Append("; Фильтры: \r\n");
                            foreach (var value in args.FilterValues)
                            {
                                message.Append(value);
                                message.Append(";\r\n");
                            }

                            message.AddHyperLink(docLocation, Resources.SViewJournal, string.Empty);
                            return new LogMessageEntry(User.GetSID(), args.ExportLog, message.ToString());
                        });
            }

            return stream;
        }
    }
}