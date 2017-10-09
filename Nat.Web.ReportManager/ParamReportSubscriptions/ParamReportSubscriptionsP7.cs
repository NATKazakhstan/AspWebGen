/*
 * Created by: Valeriy A. Suhorukov
 * Created: 2012.11.19
 * Copyright © JSC NAT Kazakhstan 2012
 */

namespace Nat.Web.ReportManager
{
    using System;

    using Nat.Tools.Filtering;

    internal class ParamReportSubscriptionsP7 : ParamReportSubscriptions
    {
        internal override DateTime[] Calculate()
        {
            var d = new[] { DateTime.Now, DateTime.Now };
            var d1 = d[1];
            if (this.DateNext != null)
            {
                d1 = d[0] = this.GetGeneralChangesEndDate(this.DateNext.Value);
                d[0] = d[0].AddDays(-this.RowParam.CreateOnTheLastDayPublication ?? 0);
            }

            if (RowParam.ParamFilterType == ColumnFilterType.Between.ToString())
            {
                d[1] = d1;
            }

            return d;
        }         
    }
}