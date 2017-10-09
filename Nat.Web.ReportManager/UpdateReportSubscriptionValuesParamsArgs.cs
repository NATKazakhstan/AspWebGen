namespace Nat.Web.ReportManager
{
    using System;
    using System.Data.Linq;

    using Nat.Web.ReportManager.Data;

    public class UpdateReportSubscriptionValuesParamsArgs
    {
        public long refReportSubscriptions { get; set; }

        public Binary Values { get; set; }

        public Binary Constants { get; set; }

        public DateTime? DateNext { get; set; }

        public DBDataContext db { get; set; }
    }
}