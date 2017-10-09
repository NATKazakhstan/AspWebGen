using System.Collections.Generic;
using System.Web.UI.WebControls;
using Nat.ReportManager.ReportGeneration;

namespace Nat.Web.ReportManager
{
    public class ReportTableRow : TableRow
    {
        private List<BaseReportCondition> reportConditions;
        private ImageButton imageButton;

        public List<BaseReportCondition> ReportConditions
        {
            get { return reportConditions; }
            set { reportConditions = value; }
        }

        public ImageButton ImageButton
        {
            get { return imageButton; }
            set { imageButton = value; }
        }
    }
}