using System.ComponentModel;
using System.Data;

namespace Nat.Web.Controls
{
    public delegate void TableDataSourceClearingDataEventHandler(object sender, TableDataSourceClearingDataEventArgs e);

    public class TableDataSourceClearingDataEventArgs : CancelEventArgs
    {
        private readonly DataTable mergingTable;
        private readonly DataTable table;
        private bool cancelClearChild = false;
        private bool stopEnforceConstraint = false;

        public TableDataSourceClearingDataEventArgs(DataTable table, DataTable mergingTable)
        {
            this.table = table;
            this.mergingTable = mergingTable;
        }

        public bool CancelClearChild
        {
            get { return cancelClearChild; }
            set { cancelClearChild = value; }
        }

        public bool StopEnforceConstraints
        {
            get { return stopEnforceConstraint; }
            set { stopEnforceConstraint = value; }
        }

        public DataTable Table
        {
            get { return table; }
        }

        public DataTable MergingTable
        {
            get { return mergingTable; }
        }

        public void ClearAllChildKeyConstraintTables()
        {
            foreach (DataRelation relation in table.ChildRelations)
                if (relation.ChildKeyConstraint != null) ClearTable(relation.ChildTable);
        }

        private static void ClearTable(DataTable table)
        {
            foreach (DataRelation relation in table.ChildRelations)
                if (relation.ChildKeyConstraint != null) ClearTable(relation.ChildTable);
            table.Clear();
        }
    }
}