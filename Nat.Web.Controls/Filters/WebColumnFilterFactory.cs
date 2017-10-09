using System.Data;
using Nat.Controls.DataGridViewTools;
using Nat.Tools.Filtering;

namespace Nat.Web.Controls.Filters
{
    public class WebColumnFilterFactory : ColumnFilterFactory
    {
        public override IColumnFilter CreateColumnFilter()
        {
            return new ColumnFilter();
        }

        public override IColumnFilter CreateColumnFilter(DataColumn column)
        {
            throw new System.NotImplementedException();
        }

        public override IColumnFilter CreateColumnFilter(ColumnFilterStorage storage)
        {
            ColumnFilter columnFilter = new ColumnFilter();
            columnFilter.SetStorage(storage);
            return columnFilter;
        }

        public override IMultipleValuesColumnFilter CreateMultipleValuesColumnFilter()
        {
            return new WebMultipleValuesColumnFilter();
        }

        public override IMultipleValuesColumnFilter CreateMultipleValuesColumnFilter(ColumnFilterStorage storage)
        {
            WebMultipleValuesColumnFilter webMultipleValuesColumnFilter = new WebMultipleValuesColumnFilter();
            webMultipleValuesColumnFilter.SetStorage(storage);
            return webMultipleValuesColumnFilter;
        }
    }
}