using System;
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
    public delegate void TableDataSourceClearedDataEventHandler(object sender, TableDataSourceClearedDataEventArgs e);

    public class TableDataSourceClearedDataEventArgs : EventArgs
    {
        private readonly Exception exception;
        private readonly DataTable dataTable;
        private bool throwException = true;

        public TableDataSourceClearedDataEventArgs(Exception exception, DataTable dataTable)
        {
            this.exception = exception;
            this.dataTable = dataTable;
        }

        public DataTable DataTable
        {
            get { return dataTable; }
        }

        public Exception Exception
        {
            get { return exception; }
        }

        public bool ThrowException
        {
            get { return throwException; }
            set { throwException = value; }
        }
    }
}
