using System;
using System.ComponentModel;
using System.Data;
using System.Configuration;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;
using System.Web.UI.HtmlControls;

namespace Nat.Web.Controls
{
    public delegate void TableDataSourceUpdatingDataEventHandler(object sender, TableDataSourceUpdatingDataEventArgs e);

    public class TableDataSourceUpdatingDataEventArgs : CancelEventArgs
    {
        private readonly DataTable table;

        public TableDataSourceUpdatingDataEventArgs(DataTable table)
        {
            this.table = table;
        }

        public TableDataSourceUpdatingDataEventArgs(bool cancel, DataTable table) : base(cancel)
        {
            this.table = table;
        }

        public DataTable Table
        {
            get { return table; }
        }
    }
}
