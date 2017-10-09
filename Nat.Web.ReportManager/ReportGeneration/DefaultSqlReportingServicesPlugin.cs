using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Data;
using System.Web.UI;
using Nat.ReportManager.QueryGeneration;
using Nat.ReportManager.ReportGeneration;

namespace Nat.Web.ReportManager.ReportGeneration
{
    public abstract class DefaultSqlReportingServicesPlugin : DefaultSqlReportingServicesPlugin<Control>
    {
        protected DefaultSqlReportingServicesPlugin(string description) : base(description) { }

        protected DefaultSqlReportingServicesPlugin(string description, int position) : base(description, position) { }
    }

    public abstract class DefaultSqlReportingServicesPlugin<T> : AbstractSqlReportingServicesPlugin<T> 
        where T : Control, new()
    {
        protected DefaultSqlReportingServicesPlugin(string description) : base(description) {}

        protected DefaultSqlReportingServicesPlugin(string description, int position) : base(description, position) { }

        public override ViewType ViewType
        {
            get { return ViewType.DefaultView; }
        }

        public override List<AdapterToTable> ListAdapterToTable
        {
            get
            {
                return new List<AdapterToTable>(0);
            }
        }

        public override DataTable Table
        {
            get { return null; }
        }

        public override Component TableAdapter
        {
            get { return null; }
        }

        public override void Fill()
        {
        }

        public override void Clear()
        {
            if (Table != null)
                foreach (DataTable table in Table.DataSet.Tables)
                    table.Rows.Clear();
        }

        public override void InitReport(StringDictionary selectedTexts, StringDictionary selectedFilterTexts, Dictionary<string, DataRow[]> selectedRows) {}

        public override void PostActions() {}

        public override void PreActions() {}

        public override void ShowCustomReport() {}
    }
}