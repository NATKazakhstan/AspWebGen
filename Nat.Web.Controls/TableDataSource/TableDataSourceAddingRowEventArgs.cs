using System.ComponentModel;
using System.Data;

namespace Nat.Web.Controls
{
    public delegate void TableDataSourceAddingRowEventHandler(object sender, TableDataSourceAddingRowEventArgs e);

    public class TableDataSourceAddingRowEventArgs : CancelEventArgs
    {
        private readonly DataRow row;

        public TableDataSourceAddingRowEventArgs(DataRow row)
        {
            this.row = row;
        }

        public TableDataSourceAddingRowEventArgs(bool cancel, DataRow row) : base(cancel)
        {
            this.row = row;
        }

        public DataRow Row
        {
            get { return row; }
        }
    }
}