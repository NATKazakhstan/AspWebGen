/*
 * Created by: Valeriy A. Suhorukov
 * Created: 2012.11.18
 * Copyright © JSC NAT Kazakhstan 2012
 */

namespace Nat.Web.ReportManager
{
    using System;

    using Nat.Tools.Filtering;

    internal class ParamReportSubscriptionsP2 : ParamReportSubscriptions
    {
        internal override DateTime[] Calculate()
        {
            var d = new[] { DateTime.Now, DateTime.Now };
            if (this.DateNext != null)
            {
                d[0] = DateTimeHelper.GetFirstDayHalfYear(this.DateNext.Value);
                d[1] = this.RowParam.CreateOnTheDayPublication
                           ? this.DateNext.Value
                           : DateTimeHelper.GetLastDayHalfYear(this.DateNext.Value);
                d[0] = this.GetGeneralChangesDate(d[0]);
                d[1] = this.GetGeneralChangesEndDate(d[1]);
            }

            return d;
        }
    }
}