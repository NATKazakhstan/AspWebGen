using System.Collections.Generic;
using System.Collections.Specialized;
using System.Data;
using System.Web.UI;
using Nat.ReportManager.QueryGeneration;
using Nat.ReportManager.ReportGeneration;

namespace Nat.Web.ReportManager.ReportGeneration
{
    public abstract class DefaultReportPlugin : DefaultReportPlugin<Control>
    {
        protected DefaultReportPlugin(string description) : base(description) { }

        protected DefaultReportPlugin(string description, int position) : base(description, position) { }
    }

    public abstract class DefaultReportPlugin<T> : AbstractReportPlugin<T> where T : Control, new()
    {
        private List<AdapterToTable> _adapterToTables;

        public DefaultReportPlugin(string description) : base(description) {}

        public DefaultReportPlugin(string description, int position) : base(description, position) {}

        public override ViewType ViewType
        {
            get { return ViewType.DefaultView; }
        }

        public override void Clear()
        {
            foreach(DataTable table in Table.DataSet.Tables)
                table.Rows.Clear();
        }

        public override void InitReport(StringDictionary selectedTexts, StringDictionary selectedFilterTexts, Dictionary<string, DataRow[]> selectedRows) {}

        public override void PostActions() {}

        public override void PreActions() {}

        public override void ShowCustomReport() {}
    }
}