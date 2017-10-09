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
    public delegate void TableDataSourceErrorEventHandler(object sender, TableDataSourceErrorEventArgs e);

    public class TableDataSourceErrorEventArgs : EventArgs
    {
        private readonly Exception exception;
        private readonly EventArgs innerEvent;
        private readonly bool hasInnerEvent;
        private bool throwException = true;


        public TableDataSourceErrorEventArgs(Exception exception, EventArgs innerEvent, bool hasInnerEvent, bool throwException)
        {
            this.exception = exception;
            this.innerEvent = innerEvent;
            this.hasInnerEvent = hasInnerEvent;
            this.throwException = throwException;
        }

        public TableDataSourceErrorEventArgs(Exception exception, EventArgs innerEvent, bool hasInnerEvent)
        {
            this.exception = exception;
            this.innerEvent = innerEvent;
            this.hasInnerEvent = hasInnerEvent;
        }

        public bool HasInnerEvent
        {
            get { return hasInnerEvent; }
        }

        public EventArgs InnerEvent
        {
            get { return innerEvent; }
        }

        public bool ThrowException
        {
            get { return throwException; }
            set { throwException = value; }
        }

        public Exception Exception
        {
            get { return exception; }
        }
    }
}
