namespace Nat.Web.Tools.Initialization
{
    using System.Configuration;

    public class ExternalSystemElement : ConfigurationElement
    {
        private static readonly ConfigurationProperty PropSystemName;

        private static readonly ConfigurationProperty PropAddUrlTemplate;

        private static readonly ConfigurationProperty PropViewUrlTemplate;

        private static readonly ConfigurationProperty PropUrlTemplate;

        private static readonly ConfigurationProperty PropSyncRestServiceUrl;

        private static readonly ConfigurationProperty PropSyncRestServiceName;

        private static readonly ConfigurationProperty PropUsedInViews;

        private static readonly ConfigurationPropertyCollection _properties;

        static ExternalSystemElement()
        {
            PropSystemName = new ConfigurationProperty("SystemName", typeof(string), "", ConfigurationPropertyOptions.None);
            PropAddUrlTemplate = new ConfigurationProperty("AddUrlTemplate", typeof(string), "", ConfigurationPropertyOptions.None);
            PropViewUrlTemplate = new ConfigurationProperty("ViewUrlTemplate", typeof(string), "", ConfigurationPropertyOptions.None);
            PropUrlTemplate = new ConfigurationProperty("UrlTemplate", typeof(string), "", ConfigurationPropertyOptions.None);
            PropSyncRestServiceUrl = new ConfigurationProperty("SyncRestServiceUrl", typeof(string), "", ConfigurationPropertyOptions.None);
            PropSyncRestServiceName = new ConfigurationProperty("SyncRestServiceName", typeof(string), "", ConfigurationPropertyOptions.None);
            PropUsedInViews = new ConfigurationProperty("UsedInViews", typeof(string), "", ConfigurationPropertyOptions.None);

            _properties = new ConfigurationPropertyCollection
                              {
                                  PropSystemName,
                                  PropAddUrlTemplate,
                                  PropViewUrlTemplate,
                                  PropUrlTemplate,
                                  PropSyncRestServiceUrl,
                                  PropSyncRestServiceName,
                                  PropUsedInViews,
                              };
        }

        [StringValidator(MinLength = 1)]
        [ConfigurationProperty("SystemName", IsRequired = true, IsKey = true, DefaultValue = "")]
        public string SystemName
        {
            get { return (string)base[PropSystemName]; }
        }

        [ConfigurationProperty("AddUrlTemplate", IsRequired = false, DefaultValue = "")]
        public string AddUrlTemplate
        {
            get { return (string)base[PropAddUrlTemplate]; }
        }

        [ConfigurationProperty("ViewUrlTemplate", IsRequired = false, DefaultValue = "")]
        public string ViewUrlTemplate
        {
            get { return (string)base[PropViewUrlTemplate]; }
        }

        [ConfigurationProperty("UrlTemplate", IsRequired = false, DefaultValue = "")]
        public string UrlTemplate
        {
            get { return (string)base[PropUrlTemplate]; }
        }

        [ConfigurationProperty("SyncRestServiceUrl", IsRequired = false, DefaultValue = "")]
        public string SyncRestServiceUrl
        {
            get { return (string)base[PropSyncRestServiceUrl]; }
        }

        [ConfigurationProperty("SyncRestServiceName", IsRequired = false, DefaultValue = "")]
        public string SyncRestServiceName
        {
            get { return (string)base[PropSyncRestServiceName]; }
        }

        [ConfigurationProperty("UsedInViews", IsRequired = false, DefaultValue = "")]
        public string UsedInViews
        {
            get { return (string)base[PropUsedInViews]; }
        }

        protected override ConfigurationPropertyCollection Properties
        {
            get { return _properties; }
        }
    }
}