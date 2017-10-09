using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

using Nat.Web.ReportManager;

namespace Nat.Web.ReportManager
{
    using Nat.Web.ReportManager.Data;

    public class ParamReportSubscriptions
    {
        public ParamReportSubscriptions(ReportSubscriptions_Param rowParam, DateTime? dateNext)
        {
            RowParam = rowParam;
            DateNext = dateNext;
            switch (rowParam.DIC_ReportTimePeriodsParameter_refReportTimePeriodsParameters.code)
            {
                case "1":
                    var calcParamReportP1 = new ParamReportSubscriptionsP1 { RowParam = rowParam, DateNext = dateNext };
                    DateParams = calcParamReportP1.Calculate();
                    break;
                case "2":
                    var calcParamReportP2 = new ParamReportSubscriptionsP2 { RowParam = rowParam, DateNext = dateNext };
                    DateParams = calcParamReportP2.Calculate();
                    break;
                case "3":
                    var calcParamReportP3 = new ParamReportSubscriptionsP3 { RowParam = rowParam, DateNext = dateNext };
                    DateParams = calcParamReportP3.Calculate();
                    break;
                case "4":
                    var calcParamReportP4 = new ParamReportSubscriptionsP4 { RowParam = rowParam, DateNext = dateNext };
                    DateParams = calcParamReportP4.Calculate();
                    break;
                case "5":
                    var calcParamReportP5 = new ParamReportSubscriptionsP5 { RowParam = rowParam, DateNext = dateNext };
                    DateParams = calcParamReportP5.Calculate();
                    break;
                case "6":
                    var calcParamReportP6 = new ParamReportSubscriptionsP6 { RowParam = rowParam, DateNext = dateNext };
                    DateParams = calcParamReportP6.Calculate();
                    break;
                case "7":
                    var calcParamReportP7 = new ParamReportSubscriptionsP7 { RowParam = rowParam, DateNext = dateNext };
                    DateParams = calcParamReportP7.Calculate();
                    break;
            } 
        }

        protected ParamReportSubscriptions()
        {            
        }

        internal ReportSubscriptions_Param RowParam { get; set; }

        internal DateTime? DateNext { get; set; }

        public DateTime[] DateParams { get; set; }

        internal virtual DateTime[] Calculate()
        {
            throw new NotImplementedException();
        }

        internal DateTime GetGeneralChangesDate(DateTime dateIn)
        {
            return dateIn.AddYears(
                        -this.RowParam.DeviationsFromThePeriodYear ?? 0).AddMonths(
                            -this.RowParam.DeviationsFromThePeriodMonth ?? 0).AddDays(
                                -this.RowParam.DeviationsFromThePeriodDay ?? 0);
        }

        internal DateTime GetGeneralChangesEndDate(DateTime dateIn)
        {
            return GetGeneralChangesDate(dateIn).AddDays(-this.RowParam.ExceptDaysFromTheDatePublication ?? 0);
        }
    }
}