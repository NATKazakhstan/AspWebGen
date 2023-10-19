using Nat.ExportInExcel;
using Nat.Web.Controls;
using Nat.Web.Controls.Data;
using Nat.Web.Controls.GenerationClasses;
using Nat.Web.Controls.GenerationClasses.BaseJournal;
using Nat.Web.Controls.GenerationClasses.Navigator;
using Nat.Web.Tools;
using Nat.Web.Tools.Export;
using Nat.Web.Tools.Report;
using System;
using System.Data.Linq;
using System.Globalization;
using System.IO;
using System.Threading;
using System.Web.Mvc;
using Telerik.Windows.Documents.Spreadsheet.FormatProviders.OpenXml.Xlsx;
using Nat.Web.Tools.Security;
using System.Web.Compilation;
using System.Web;
using System.Reflection;
using Nat.Web.ReportManager;
using System.Windows;
using Telerik.Windows.Documents.Fixed.Model.ColorSpaces;
using System.Linq;
using Telerik.Windows.Documents.Fixed.Model.Editing;

namespace Nat.ExportInPdf
{
    public class Exporter : IExporter
    {
        public Stream GetExcelByType( Type journalType, object properties, ILogMonitor logMonitor, bool checkPermit, out string fileNameExtention )
        {
            throw new NotImplementedException();
        }

        public Stream GetExcelByType( Type journalType, string format, object properties, string culture, ILogMonitor logMonitor, bool checkPermit, out string fileNameExtention )
        {
            throw new NotImplementedException();
        }

        public Stream GetExcelByType( Type journalType, string format, long idProperties, StorageValues storageValues, string culture, ILogMonitor logMonitor, bool checkPermit, out string fileNameExtention )
        {
            throw new NotImplementedException();
        }

        public Stream GetExcelByTypeName( string journalType, string format, long idProperties, StorageValues storageValues, string culture, ILogMonitor logMonitor, bool checkPermit, out string fileNameExtention )
        {
            throw new NotImplementedException();
        }

        public Stream GetExcelStream( JournalExportEventArgs args )
        {
            throw new NotImplementedException();
        }

        public Stream GetPdfByType( Type journalType, object properties, ILogMonitor logMonitor, bool checkPermit, out string fileNameExtention )
        {
            var rvsProperties = (RvsSavedProperties)properties;
            var oldCulture = Thread.CurrentThread.CurrentUICulture;
            if (!string.IsNullOrEmpty( rvsProperties.Culture ))
                Thread.CurrentThread.CurrentUICulture = new CultureInfo( rvsProperties.Culture );
            try
            {
                var obj = Activator.CreateInstance( journalType );
                var methodInfo = typeof( Exporter ).GetMethod( "GetPdf" );
                Type[] gTypes = null;
                if (journalType.IsGenericType)
                    gTypes = journalType.GetGenericArguments();
                else if (journalType.BaseType.IsGenericType)
                    gTypes = journalType.BaseType.GetGenericArguments();
                else if (journalType.BaseType.BaseType.IsGenericType)
                    gTypes = journalType.BaseType.BaseType.GetGenericArguments();

                methodInfo = methodInfo.MakeGenericMethod( gTypes );
                fileNameExtention = "pdf";
                return (Stream)methodInfo.Invoke( null, new[] { obj, properties, logMonitor, checkPermit } );
            }
            finally
            {
                if (!string.IsNullOrEmpty( rvsProperties.Culture ))
                    Thread.CurrentThread.CurrentUICulture = oldCulture;
            }
        }

        public static Stream GetPdf<TDataContext, TFilterControl, TKey, TTable, TDataSource, TRow, TJournal, TNavigatorControl, TNavigatorValues, TFilter>(
            BaseJournalUserControl<TDataContext, TFilterControl, TKey, TTable, TDataSource, TRow, TJournal, TNavigatorControl, TNavigatorValues, TFilter>
                journalControl,
            RvsSavedProperties properties,
            ILogMonitor logMonitor,
            bool checkPermit )

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
            if (checkPermit)
                journalControl.CheckExportPermit();
            stream = GetDataAsExcelStream( journalControl, properties, logMonitor );
            stream = ConvertExcelToPdf( stream );
            AddWatermark( journalControl, stream );
            long? logId = GetLogId( journalControl, properties, logMonitor );
            AddSigns( stream, logId );

            return stream;
        }

        private static void AddWatermark<TDataContext, TFilterControl, TKey, TTable, TDataSource, TRow, TJournal, TNavigatorControl, TNavigatorValues, TFilter>( BaseJournalUserControl<TDataContext, TFilterControl, TKey, TTable, TDataSource, TRow, TJournal, TNavigatorControl, TNavigatorValues, TFilter> journalControl, Stream stream )
            where TDataContext : DataContext, new()
            where TFilterControl : BaseFilterControl<TKey, TTable>, new()
            where TKey : struct
            where TTable : class
            where TDataSource : BaseDataSource<TKey, TTable, TDataContext, TRow>
            where TRow : BaseRow, new()
            where TJournal : BaseJournalControl<TKey, TTable, TRow, TDataContext>
            where TNavigatorControl : BaseNavigatorControl<TNavigatorValues>
            where TNavigatorValues : BaseNavigatorValues, new()
            where TFilter : BaseFilter<TKey, TTable, TDataContext>, new()
        {
            var isSubscription = (bool)(HttpContext.Current.Items[ "ReportSubscriptionInitialize" ] ?? false);
            if (isSubscription)
            {
                return;
            }
            var watermark = DependencyResolver.Current.GetService<IReportWatermark>();
            watermark.AddToPdfStream( stream, journalControl.ReportPluginName );
        }

        private static void AddSigns( Stream stream, long? logId )
        {
            if (logId == null)
            {
                return;
            }
            try
            {
                var qrCodeTextFormat = DependencyResolver.Current.GetService<IQrCodeTextFormat>();
                using (var logo = qrCodeTextFormat.GetHorizontalImageStream())
                using (var qrImgStream = qrCodeTextFormat.GetQrCodeImageStream( (long)logId ))
                {
                    if (qrImgStream == null || logo == null)
                    {
                        return;
                    }
                    var txt = qrCodeTextFormat.GetUserText( (long)logId );
                    var formatProvider = new Telerik.Windows.Documents.Fixed.FormatProviders.Pdf.PdfFormatProvider();
                    var doc = formatProvider.Import( stream );
                    for (int i = 0; i < doc.Pages.Count; i++)
                    {
                        var page = doc.Pages[ i ];
                        var editor = new FixedContentEditor( page );
                        double paddingLeftRight = 20;
                        double paddingBottom = 10;
                        int scalePadding = 20;
                        int qrSize = 0;
                        // reset position
                        editor.Position.Translate( 0, 0 );                        
                        using (var logoBmp = new System.Drawing.Bitmap( logo ))
                        {
                            Size logoSize = new Size( logoBmp.Width - scalePadding, logoBmp.Height - scalePadding );
                            var logoBlock = new Block();
                            logoBlock.InsertImage( logo, logoSize );
                            editor.Position.Translate( paddingLeftRight, page.Size.Height - logoSize.Height - paddingBottom );
                            editor.DrawBlock( logoBlock );
                            qrSize = Math.Min( (int)logoSize.Height, (int)logoSize.Width );
                        }
                        // reset position
                        editor.Position.Translate( 0, 0 );
                        var qrBlock = new Block();
                        qrBlock.InsertImage( qrImgStream, qrSize, qrSize );
                        editor.Position.Translate( page.Size.Width - qrSize - paddingLeftRight, page.Size.Height - qrSize - paddingBottom );
                        editor.DrawBlock( qrBlock );
                        // reset position
                        editor.Position.Translate( 0, 0 );
                        Block txtBlock = GetTxtBlock();
                        var txtBlockSize = new Size( page.Size.Width*0.8, txtBlock.TextProperties.FontSize + 3 );
                        var txtLines = txt.Split( '\n' );
                        for (int k = 0; k < txtLines.Count(); k++)
                        {
                            txtBlock.InsertText( txtLines[ k ] );
                            editor.Position.Translate( page.Size.Width / 2 - txtBlockSize.Width / 2, (page.Size.Height - qrSize) + k * txtBlockSize.Height );
                            editor.DrawBlock( txtBlock, txtBlockSize );
                            txtBlock = GetTxtBlock();
                        }
                    }
                    stream.Position = 0;
                    stream.SetLength( 0 );
                    formatProvider.ExportSettings = new Telerik.Windows.Documents.Fixed.FormatProviders.Pdf.Export.PdfExportSettings()
                    {
                        ImageQuality = Telerik.Windows.Documents.Fixed.FormatProviders.Pdf.Export.ImageQuality.Low,
                        IsEncrypted = false
                    };
                    formatProvider.Export( doc, stream );
                }
            }
            finally
            {
                stream.Position = 0;
            }
        }

        private static Block GetTxtBlock()
        {
            var txtBlock = new Block();
            txtBlock.BackgroundColor = RgbColors.Transparent;
            txtBlock.TextProperties.FontSize = 12;
            txtBlock.TextProperties.TrySetFont( new System.Windows.Media.FontFamily( "Times New Roman" ), FontStyles.Normal, FontWeights.Normal );
            txtBlock.HorizontalAlignment = Telerik.Windows.Documents.Fixed.Model.Editing.Flow.HorizontalAlignment.Center;
            return txtBlock;
        }

        private static long? GetLogId<TDataContext, TFilterControl, TKey, TTable, TDataSource, TRow, TJournal, TNavigatorControl, TNavigatorValues, TFilter>( BaseJournalUserControl<TDataContext, TFilterControl, TKey, TTable, TDataSource, TRow, TJournal, TNavigatorControl, TNavigatorValues, TFilter> journalControl, RvsSavedProperties properties, ILogMonitor logMonitor )
            where TDataContext : DataContext, new()
            where TFilterControl : BaseFilterControl<TKey, TTable>, new()
            where TKey : struct
            where TTable : class
            where TDataSource : BaseDataSource<TKey, TTable, TDataContext, TRow>
            where TRow : BaseRow, new()
            where TJournal : BaseJournalControl<TKey, TTable, TRow, TDataContext>
            where TNavigatorControl : BaseNavigatorControl<TNavigatorValues>
            where TNavigatorValues : BaseNavigatorValues, new()
            where TFilter : BaseFilter<TKey, TTable, TDataContext>, new()
        {
            var plugin = BuildManager.GetType( properties.ReportPluginName, false, true ) ?? GetTypeByReportManager( properties.ReportPluginName );
            DBDataContext.AddViewReports(
                User.GetSID(),
                HttpContext.Current.User.Identity.Name,
                HttpContext.Current.User.Identity.Name,
                HttpContext.Current.Request.Url.PathAndQuery,
                HttpContext.Current.Request.Url.GetLeftPart( UriPartial.Authority ),
                Environment.MachineName,
                true,
                plugin );

            var logInfo = (ILogReportGenerateCode)Activator.CreateInstance( BuildManager.GetType( "Event_LOG.LogReportGenerateCode", true, false ) );
            var logType = (LogMessageType)logInfo.GetCodeFor(
                plugin.FullName,
                properties.NameRu,
                (long)journalControl.ExportLog,
                LogReportGenerateCodeType.Export );

            var logId = logMonitor.WriteLog(
                new LogMessageEntry(
                    User.GetSID(),
                    logType,
                    properties.NameRu,
                    journalControl.OnExportNewSavedProperties ? RvsSavedProperties.GetFromJournal( journalControl ) : properties ) );
            return logId;
        }

        private static Stream ConvertExcelToPdf( Stream stream )
        {
            var docxProvider = new XlsxFormatProvider();
            var document = docxProvider.Import( stream );
            var pdfProvider = new Telerik.Windows.Documents.Spreadsheet.FormatProviders.Pdf.PdfFormatProvider();
            Stream output = new MemoryStream();
            pdfProvider.Export( document, output );
            return output;
        }

        private static Stream GetDataAsExcelStream<TDataContext, TFilterControl, TKey, TTable, TDataSource, TRow, TJournal, TNavigatorControl, TNavigatorValues, TFilter>( BaseJournalUserControl<TDataContext, TFilterControl, TKey, TTable, TDataSource, TRow, TJournal, TNavigatorControl, TNavigatorValues, TFilter> journalControl, RvsSavedProperties properties, ILogMonitor logMonitor )
            where TDataContext : DataContext, new()
            where TFilterControl : BaseFilterControl<TKey, TTable>, new()
            where TKey : struct
            where TTable : class
            where TDataSource : BaseDataSource<TKey, TTable, TDataContext, TRow>
            where TRow : BaseRow, new()
            where TJournal : BaseJournalControl<TKey, TTable, TRow, TDataContext>
            where TNavigatorControl : BaseNavigatorControl<TNavigatorValues>
            where TNavigatorValues : BaseNavigatorValues, new()
            where TFilter : BaseFilter<TKey, TTable, TDataContext>, new()
        {
            var exporterXslx = new ExporterXslx<TDataContext, TFilterControl, TKey, TTable, TDataSource, TRow, TJournal, TNavigatorControl, TNavigatorValues, TFilter>
            {
                LogMonitor = logMonitor
            };
            Stream stream = exporterXslx.GetExcel( journalControl, properties );

            return stream;
        }

        private static Type GetTypeByReportManager( string pluginName )
        {
            var type = BuildManager.GetType( "Nat.Web.ReportManager.WebReportManager, Nat.Web.ReportManager, Version=1.4.0.0, Culture=neutral, PublicKeyToken=11c252a207597415", true, true );
            var getPlugin = type.GetMethod( "GetPlugin", BindingFlags.Public | BindingFlags.Static );
            return getPlugin.Invoke( null, new object[] { pluginName } )?.GetType();
        }

        public Stream GetPdfByTypeName( string journalType, string format, long idProperties, StorageValues storageValues, string culture, ILogMonitor logMonitor, bool checkPermit, out string fileNameExtention )
        {
            var rvsProp = RvsSavedProperties.LoadFrom( idProperties, logMonitor );
            rvsProp.Format = format;
            rvsProp.Culture = culture;
            rvsProp.StorageValues = storageValues;
            journalType = journalType ?? rvsProp.JournalTypeName;
            var type = Type( journalType );
            return GetPdfByType( type, rvsProp, logMonitor, checkPermit, out fileNameExtention );
        }

        private static Type Type( string journalType )
        {
            if (journalType.Contains( "1.0.0.0, Culture=neutral, PublicKeyToken=55f6c56e6ab9709a" ))
                journalType = journalType.Replace( "1.0.0.0", "1.4.0.0" );
            return System.Type.GetType( journalType ) ?? BuildManager.GetType( journalType, false, true );
        }
    }
}
