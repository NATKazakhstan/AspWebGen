using System.Configuration;

namespace Nat.Web.ReportManager
{
    public class ReportInitializerSectionGroup : ConfigurationSectionGroup
    {
        [ConfigurationProperty("ReportInitializer")]
        public ReportInitializerSection ReprotInitializer
        {
            get { return (ReportInitializerSection)base.Sections["ReportInitializer"]; }
        }
        [ConfigurationProperty("SubscriptionInitializer")]
        public ReportInitializerSection SubscriptionInitializer
        {
            get { return (ReportInitializerSection)base.Sections["SubscriptionInitializer"]; }
        }
    }
}