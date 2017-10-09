using System;
using System.Net;
using System.Security.Principal;
using Microsoft.Reporting.WebForms;

namespace Nat.Web.ReportManager
{
    [Serializable]
    public class ReportServerCredentials : IReportServerCredentials
    {
        public bool GetFormsCredentials(out Cookie authCookie, out string userName, out string password, out string authority)
        {
            authCookie = null;
            userName = password = authority = null;
            return false;
        }

        public WindowsIdentity ImpersonationUser
        {
            get { return null; }
        }

        public ICredentials NetworkCredentials
        {
            get
            {
                var reportSection = ReportInitializerSection.GetReportInitializerSection();
                return new NetworkCredential(reportSection.ReportingServicesUserName, reportSection.ReportingServicesPassword, reportSection.ReportingServicesUserDomain);
            }
        }

        public static bool HasConfiguration()
        {
            var reportSection = ReportInitializerSection.GetReportInitializerSection();
            return !string.IsNullOrEmpty(reportSection.ReportingServicesUserName)
                   && !string.IsNullOrEmpty(reportSection.ReportingServicesPassword);
        }
    }
}