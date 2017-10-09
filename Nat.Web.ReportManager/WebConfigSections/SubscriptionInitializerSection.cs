using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web;
using System.Web.Configuration;
using Nat.Tools.Specific;
using Nat.Web.Tools;

namespace Nat.Web.ReportManager
{
    public class SubscriptionInitializerSection : ConfigurationSection
    {
        private static readonly ConfigurationPropertyCollection _properties;
        private static readonly ConfigurationProperty _propIdentificationType;
        private static readonly ConfigurationProperty _propUserIdentificationUserName;
        private static readonly ConfigurationProperty _propUserIdentificationPassword;
        private static readonly ConfigurationProperty _propUserIdentificationUserDomain;
        private static Configuration WebConfiguration;

        static SubscriptionInitializerSection()
        {
            _propIdentificationType = new ConfigurationProperty("identificationType", typeof(SubscriptionIdentificationType), "None", ConfigurationPropertyOptions.None);
            _propUserIdentificationUserName = new ConfigurationProperty("userIdentificationUserName", typeof(string), "", ConfigurationPropertyOptions.None);
            _propUserIdentificationPassword = new ConfigurationProperty("userIdentificationPassword", typeof(string), "", ConfigurationPropertyOptions.None);
            _propUserIdentificationUserDomain = new ConfigurationProperty("userIdentificationUserDomain", typeof(string), "", ConfigurationPropertyOptions.None);
            _properties = new ConfigurationPropertyCollection
                              {
                                  _propIdentificationType,
                                  _propUserIdentificationUserName,
                                  _propUserIdentificationPassword,
                                  _propUserIdentificationUserDomain,
                              };
        }

        public static SubscriptionInitializerSection GetSubscriptionInitializerSection()
        {
            var WebConfiguration = ((IWebConfiguration)SpecificInstances.DbFactory).WebConfiguration;
            var section = WebConfiguration == null
             ? WebConfigurationManager.GetSection("Nat.WebReportManager/SubscriptionInitializer") as SubscriptionInitializerSection
            : WebConfiguration.GetSection("Nat.WebReportManager/SubscriptionInitializer")
             as SubscriptionInitializerSection;
            if (section == null) throw new Exception("Config not containt SubscriptionInitializer section");
            return section;
        }

        [ConfigurationProperty("identificationType", DefaultValue = "None")]
        public SubscriptionIdentificationType IdentificationType
        {
            get
            {
                return (SubscriptionIdentificationType)base[_propIdentificationType];
            }
        }

        protected override ConfigurationPropertyCollection Properties
        {
            get { return _properties; }
        }

        [ConfigurationProperty("userIdentificationUserName", DefaultValue = @"")]
        public string UserIdentificationUserName
        {
            get { return (string)base[_propUserIdentificationUserName]; }
        }

        [ConfigurationProperty("userIdentificationPassword", DefaultValue = @"")]
        public string UserIdentificationPassword
        {
            get { return (string)base[_propUserIdentificationPassword]; }
        }

        [ConfigurationProperty("userIdentificationUserDomain", DefaultValue = @"")]
        public string UserIdentificationUserDomain
        {
            get { return (string)base[_propUserIdentificationUserDomain]; }
        }
        
 
    }
}
