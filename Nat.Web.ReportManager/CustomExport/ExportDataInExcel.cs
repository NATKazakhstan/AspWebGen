namespace Nat.Web.ReportManager.CustomExport
{
    using System;
    using System.Collections.Generic;
    using System.Data;
    using System.IO;
    using System.Reflection;
    using System.Text;
    using System.Web;
    using System.Web.UI;

    using Nat.ExportInExcel;
    using Nat.Web.Controls.GenerationClasses;
    using Nat.Web.Controls.Properties;
    using Nat.Web.Tools;
    using Nat.Web.Tools.Export;
    using Nat.Web.Tools.Initialization;

    public class ExportDataInExcel
    {
        private const string CellDelemitor = ";";

        public void ExportDataToCSV(DataTable table, string reportName)
        {
            using (var stream = new MemoryStream())
            {
                stream.WriteByte(0xEF);
                stream.WriteByte(0xBB);
                stream.WriteByte(0xBF);
                using (var streamWriter = new StreamWriter(stream, Encoding.UTF8))
                {
                    foreach (DataColumn column in table.Columns)
                        WriteCell(column.Caption, streamWriter);

                    streamWriter.WriteLine();

                    foreach (DataRow row in table.Rows)
                    {
                        for (int i = 0; i < table.Columns.Count; i++)
                            WriteCell(row[i], streamWriter);

                        streamWriter.WriteLine();
                    }

                    streamWriter.Flush();
                    stream.Flush();
                    stream.Position = 0;
                    PageHelper.DownloadFile(stream, reportName + ".csv", HttpContext.Current.Response);
                }
            }
        }

        public void ExportDataToHtmlTable(DataTable table, string reportName)
        {
            using (var stream = new MemoryStream())
            {
                stream.WriteByte(0xEF);
                stream.WriteByte(0xBB);
                stream.WriteByte(0xBF);
                using (var streamWriter = new StreamWriter(stream, Encoding.UTF8))
                using (var htmlWriter = new HtmlTextWriter(streamWriter))
                {
                    htmlWriter.RenderBeginTag(HtmlTextWriterTag.Html);
                    htmlWriter.RenderBeginTag(HtmlTextWriterTag.Body);
                    htmlWriter.RenderBeginTag(HtmlTextWriterTag.Thead);
                    htmlWriter.RenderBeginTag(HtmlTextWriterTag.Tr);
                    foreach (DataColumn column in table.Columns)
                        WriteHeadCell(column.Caption, htmlWriter);
                    htmlWriter.RenderEndTag();
                    htmlWriter.RenderEndTag(); // Thead

                    foreach (DataRow row in table.Rows)
                    {
                        htmlWriter.RenderBeginTag(HtmlTextWriterTag.Tr);
                        for (int i = 0; i < table.Columns.Count; i++)
                            WriteCell(row[i], htmlWriter);
                        htmlWriter.RenderEndTag();
                    }

                    htmlWriter.RenderEndTag(); // Body
                    htmlWriter.RenderEndTag(); // Html
                    htmlWriter.Flush();
                    streamWriter.Flush();
                    stream.Flush();
                    stream.Position = 0;
                    PageHelper.DownloadFile(stream, reportName + ".xls", HttpContext.Current.Response);
                }
            }
        }

        public void ExportDataToXlsx(DataTable table, string reportName)
        {
            var exporter = new ExporterXslxByArgs();
            var logMonitor = InitializerSection.GetSection().LogMonitor;
            logMonitor.Init();
            exporter.LogMonitor = logMonitor;
            var args = new JournalExportEventArgs
                       {
                           Header = reportName,
                           CheckPermit = false,
                           Columns = GetColumns(table),
                           Data = table.Rows,
                           FilterValues = new List<string>(),
                           FixedRowsCount = 3,
                       };
            using (var stream = exporter.GetExcel(args))
            {
                PageHelper.DownloadFile(stream, reportName + ".xlsx", HttpContext.Current.Response);                
            }
        }

        private IEnumerable<IExportColumn> GetColumns(DataTable table)
        {
            var list = new List<IExportColumn>();
            for (int i = 0; i < table.Columns.Count; i++)
                list.Add(new XlsxExportColumn(table.Columns[i], i));

            return list;
        }

        private static void WriteCell(object value, TextWriter streamWriter)
        {
            var strValue = value as string;
            if(!string.IsNullOrEmpty(strValue))
                streamWriter.Write("\"" + strValue.Replace("\"", "\"\"") + "\" ");
            else if (value != DBNull.Value && value != null)
                streamWriter.Write(value);
            streamWriter.Write(CellDelemitor);
        }

        private static void WriteCell(object value, HtmlTextWriter streamWriter)
        {
            streamWriter.RenderBeginTag(HtmlTextWriterTag.Td);

            var strValue = value as string;
            if(!string.IsNullOrEmpty(strValue))
                streamWriter.Write(HttpUtility.HtmlEncode(strValue));
            else if (value != DBNull.Value && value != null)
                streamWriter.Write(value);

            streamWriter.RenderEndTag();
        }

        private static void WriteHeadCell(string value, HtmlTextWriter streamWriter)
        {
            streamWriter.RenderBeginTag(HtmlTextWriterTag.Td);

            if(!string.IsNullOrEmpty(value))
                streamWriter.Write(HttpUtility.HtmlEncode(value));

            streamWriter.RenderEndTag();
        }

        private class XlsxExportColumn : IExportColumn
        {
            private readonly Func<object, string> getValueHandler;

            public XlsxExportColumn(DataColumn column, int columnIndex)
            {
                ColumnIndex = columnIndex;
                ColumnName = column.ColumnName;
                ColSpan = 1;
                RowSpan = 1;
                Header = column.Caption;
                Width = 20;
                IsNumericColumn = column.DataType == typeof(long)
                                  || column.DataType == typeof(int)
                                  || column.DataType == typeof(short)
                                  || column.DataType == typeof(byte)
                                  || column.DataType == typeof(decimal)
                                  || column.DataType == typeof(double)
                                  || column.DataType == typeof(float);

                if (column.DataType == typeof(bool))
                    getValueHandler = value => (bool)value ? Resources.SYes : Resources.SNo;
                else if (column.DataType == typeof(DateTime))
                    getValueHandler = value => ((DateTime)value).ToShortDateString();
                else
                    getValueHandler = value => value.ToString();
            }

            private int ColumnIndex { get; set; }
            public string Header { get; private set; }
            public string ColumnName { get; private set; }
            public bool IsVerticalHeaderText { get; set; }
            public bool IsVerticalDataText { get; set; }
            public string NullItemText { get; set; }
            public int ColSpan { get; set; }
            public int RowSpan { get; set; }
            public bool HasChild { get; set; }
            public decimal Width { get; set; }
            public bool IsNumericColumn { get; set; }

            public bool Visible
            {
                get { return true; }
            }

            public string GetValue(object row)
            {
                var value = ((DataRow)row)[ColumnIndex];
                if (value == DBNull.Value || value == null)
                    return null;

                return getValueHandler(value);
            }

            public string GetHyperLink(object row)
            {
                return null;
            }

            public IEnumerable<IExportColumn> GetChilds()
            {
                return null;
            }

            public int? GetDataRowSpan(object row)
            {
                return null;
            }
        }
    }
}