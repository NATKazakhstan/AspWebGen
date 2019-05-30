using System;
using System.Data;
using Nat.Tools.Constants;
using Nat.Tools.ResourceTools;

namespace Nat.Web.ReportManager.Kendo.Areas.Reports.ViewModels
{
    public class ColumnViewModel
    {
        public string field { get; set; }
        public string title { get; set; }
        public string width { get; set; }

        public static ColumnViewModel From(DataColumn dc)
        {
            var showColumn = (bool)(DataSetResourceManager.GetColumnExtProperty(dc, ColumnExtProperties.VISIBLE) ?? false);
            var visibleCulture = (string)(DataSetResourceManager.GetColumnExtProperty(dc, ColumnExtProperties.VISIBLE_CULTURE));
            if (!showColumn || (!string.IsNullOrEmpty(visibleCulture) &&
                                System.Threading.Thread.CurrentThread.CurrentUICulture.Name != visibleCulture))
                return null;

            var columnCaption = (string)(DataSetResourceManager.GetColumnExtProperty(dc, ColumnExtProperties.CAPTION) ?? dc.Caption);

            var model = new ColumnViewModel
            {
                field = dc.ColumnName,
                title = columnCaption.Replace("'", "\"")
            };

            if (dc.DataType == typeof(DateTime) || dc.DataType == typeof(DateTime?))
            {
                model.width = "120px";
            }

            if (dc.DataType == typeof(long) || dc.DataType == typeof(long?)
                                            || dc.DataType == typeof(int) || dc.DataType == typeof(int?)
                                            || dc.DataType == typeof(byte) || dc.DataType == typeof(byte?)
                                            || dc.DataType == typeof(decimal) || dc.DataType == typeof(decimal?))
            {
                model.width = "100px";
            }


            /*
var htmlEncode = (bool)(DataSetResourceManager.GetColumnExtProperty(dc, ColumnExtProperties.HTML_ENCODED) ?? false);
boundField.HtmlEncode = htmlEncode;
*/
            return model;
        }
    }
}