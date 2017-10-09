/*
 * Created by: Valeriy A. Suhorukov
 * Created: 2012.11.18
 * Copyright © JSC NAT Kazakhstan 2012
 */

namespace Nat.Web.ReportManager
{
    using System;

    using Nat.Tools.Filtering;
    using Nat.Web.ReportManager.Data;

    internal class ParamReportSubscriptionsP6 : ParamReportSubscriptions
    {
        internal override DateTime[] Calculate()
        {
            var d = new[] { DateTime.Now, DateTime.Now };            
            if (this.DateNext != null)
            {
                d[0] = this.GetGeneralChangesEndDate(DateNext.Value);
            }

            if (RowParam.ParamFilterType == ColumnFilterType.Between.ToString())
            {
                d[1] = d[0];
            }

            return d;
        }
    }
}