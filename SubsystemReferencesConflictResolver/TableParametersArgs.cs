namespace SubsystemReferencesConflictResolver
{
    using System;
    using System.Collections.Generic;
    using System.Data.SqlClient;

    public class TableParametersArgs : EventArgs
    {
        public TableParametersArgs()
        {
            DependentTables = new List<DependentTable>();
        }

        public string ConflictedTableCode { get; internal set; }
        public string ConflictedColumnCode { get; internal set; }

        public string DeleteTable { get; internal set; }
        public long DeleteID { get; internal set; }

        public SqlException Exception { get; internal set; }
        public SqlError SqlError { get; internal set; }
        internal DBDataContext DB { get; set; }

        public IList<DependentTable> DependentTables { get; private set; }
    }

    public class DependentTable
    {
        public string ProjectCode { get; set; }
        public string TableCode { get; set; }
        public string TableHeader { get; set; }
        public long? ID { get; set; }
        public string RowName { get; set; }
        public string Url { get; set; }
    }
}