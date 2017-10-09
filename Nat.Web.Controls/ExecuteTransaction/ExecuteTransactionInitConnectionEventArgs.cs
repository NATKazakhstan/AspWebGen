using System;
using System.Data.Common;

namespace Nat.Web.Controls
{
    public delegate void ExecuteTransactionInitConnectionEventHandler(object sender, ExecuteTransactionInitConnectionEventArgs e);

    public class ExecuteTransactionInitConnectionEventArgs : EventArgs
    {
        private DbConnection connection;

        public DbConnection Connection
        {
            get { return connection; }
            set { connection = value; }
        }
    }
}