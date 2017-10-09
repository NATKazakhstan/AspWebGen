using System.Configuration;

namespace Nat.Web.ReportManager
{
    public class ReportPluginInfo : ConfigurationElement
    {
        // Fields
        private static readonly ConfigurationProperty _propReportPlugin;
        private static readonly ConfigurationPropertyCollection _properties;

        static ReportPluginInfo()
        {
            _propReportPlugin = new ConfigurationProperty("reportPluginName", typeof(string), null, ConfigurationPropertyOptions.IsKey | ConfigurationPropertyOptions.IsRequired);
            _properties = new ConfigurationPropertyCollection {_propReportPlugin};
        }

        internal ReportPluginInfo() {}

        public ReportPluginInfo(string reportPluginName)
        {
            ReportPlugin = reportPluginName;
        }

        [StringValidator(MinLength = 1)]
        [ConfigurationProperty("reportPluginName", IsRequired = true, IsKey = true, DefaultValue = "")]
        public string ReportPlugin
        {
            get { return (string)base[_propReportPlugin]; }
            set { base[_propReportPlugin] = value; }
        }

        protected override ConfigurationPropertyCollection Properties
        {
            get { return _properties; }
        }
    }
}