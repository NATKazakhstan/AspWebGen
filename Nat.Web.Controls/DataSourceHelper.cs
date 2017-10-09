using System.Data;
using Nat.Web.Controls;

namespace Nat.Web.Controls
{
    public class DataSourceHelper
    {
        public static DataTable GetDataTable(object dataSource)
        {
            DataTable dataTable = dataSource as DataTable;
            if (dataTable != null) return dataTable;
            DataView dataView = dataSource as DataView;
            if (dataView != null) return dataView.Table;
            TableDataSourceView tableDataSourceView = dataSource as TableDataSourceView;
            if (tableDataSourceView != null) return tableDataSourceView.Table;
            TableDataSource tableDataSource = dataSource as TableDataSource;
            if (tableDataSource != null) return tableDataSource.Table;
            return null;
        }

    }
}