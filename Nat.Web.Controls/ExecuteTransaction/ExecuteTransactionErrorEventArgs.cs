using System;
using Nat.Tools.Data;

namespace Nat.Web.Controls
{
    public delegate void ExecuteTransactionErrorEventHandler(object sender, ExecuteTransactionErrorEventArgs e);

    public class ExecuteTransactionErrorEventArgs : EventArgs
    {
        private readonly ExceptionExecuteTransaction exceptionET;
        private readonly Exception exception;
        private readonly string[] skipTables;
        private string errorMessage;
        private bool throwException = false;

        public ExecuteTransactionErrorEventArgs(ExceptionExecuteTransaction exceptionET, Exception exception, string[] skipTables)
        {
            this.exceptionET = exceptionET;
            this.exception = exception;
            this.skipTables = skipTables;
        }

        public string[] SkipTables
        {
            get { return skipTables; }
        }

        public ExceptionExecuteTransaction ExceptionET
        {
            get { return exceptionET; }
        }

        public Exception Exception
        {
            get { return exception; }
        }

        public string ErrorMessage
        {
            get { return errorMessage; }
            set { errorMessage = value; }
        }

        public bool ThrowException
        {
            get { return throwException; }
            set { throwException = value; }
        }
    }
}