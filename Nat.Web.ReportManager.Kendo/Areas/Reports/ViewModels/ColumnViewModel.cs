using System.Data;
using Nat.Tools.Constants;
using Nat.Tools.ResourceTools;

namespace Nat.Web.ReportManager.Kendo.Areas.Reports.ViewModels
{
    public class ColumnViewModel
    {
        public string field { get; set; }
        public string title { get; set; }

        public static ColumnViewModel From(DataColumn dc)
        {
            var showColumn = (bool)(DataSetResourceManager.GetColumnExtProperty(dc, ColumnExtProperties.VISIBLE) ?? false);
            var visibleCulture = (string)(DataSetResourceManager.GetColumnExtProperty(dc, ColumnExtProperties.VISIBLE_CULTURE));
            if (showColumn && (string.IsNullOrEmpty(visibleCulture) || System.Threading.Thread.CurrentThread.CurrentUICulture.Name == visibleCulture))
            {
                var columnCaption = (string)(DataSetResourceManager.GetColumnExtProperty(dc, ColumnExtProperties.CAPTION) ?? dc.Caption);

                var model = new ColumnViewModel
                {
                    field = dc.ColumnName,
                    title = columnCaption.Replace("'", "\"")
                };
                /*
var htmlEncode = (bool)(DataSetResourceManager.GetColumnExtProperty(dc, ColumnExtProperties.HTML_ENCODED) ?? false);
boundField.HtmlEncode = htmlEncode;
*/
                return model;
            }

            return null;
        }
    }
}