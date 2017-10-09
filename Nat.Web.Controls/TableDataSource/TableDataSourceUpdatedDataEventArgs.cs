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
    public delegate void TableDataSourceUpdatedDataEventHandler(object sender, TableDataSourceUpdatedDataEventArgs e);

    public class TableDataSourceUpdatedDataEventArgs : EventArgs
    {
        private readonly int updatedRowsCount;
        private readonly Exception exception;
        private bool throwException = true;

        public TableDataSourceUpdatedDataEventArgs(int updatedRowsCount, Exception exception)
        {
            this.updatedRowsCount = updatedRowsCount;
            this.exception = exception;
        }

        public int UpdatedRowsCount
        {
            get { return updatedRowsCount; }
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
