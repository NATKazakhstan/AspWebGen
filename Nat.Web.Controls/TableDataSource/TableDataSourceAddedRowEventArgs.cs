using System;
using System.Data;

namespace Nat.Web.Controls
{
    public delegate void TableDataSourceAddedRowEventHandler(object sender, TableDataSourceAddedRowEventArgs e);

    public class TableDataSourceAddedRowEventArgs : EventArgs
    {
        private readonly DataRow row;

        public TableDataSourceAddedRowEventArgs(DataRow row)
        {
            this.row = row;
        }

        public DataRow Row
        {
            get { return row; }
        }
    }
}