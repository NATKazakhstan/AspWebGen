using Nat.Tools.Filtering;

namespace Nat.Web.ReportManager.Kendo.Areas.Reports.ViewModels
{
    public class ColumnFilterTypeViewModel
    {
        public ColumnFilterTypeViewModel()
        {
        }

        public ColumnFilterTypeViewModel(ColumnFilterType columnFilterType)
        {
            id = (int) columnFilterType;
            Name = columnFilterType.GetFilterTypeCaption();
            VisibleValue1 = columnFilterType.HasArgs();
            VisibleValue2 = columnFilterType.IsBinaryFilter();
        }

        public int id { get; set; }
        public string Name { get; set; }
        public bool VisibleValue1 { get; set; }
        public bool VisibleValue2 { get; set; }
    }
}