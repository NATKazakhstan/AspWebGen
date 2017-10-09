using System;
using System.Configuration;
using System.Web.Configuration;

using Nat.Tools.Specific;
using Nat.Web.Tools;

namespace Nat.SqlDbInitializer
{
    public class DbInitializerSection : ConfigurationSection
    {
        private static readonly ConfigurationProperty _propConnectionString;
        private static readonly ConfigurationPropertyCollection _properties;

        static DbInitializerSection()
        {
            _propConnectionString = new ConfigurationProperty("connectionStringName", typeof(string), "ConnectionString", ConfigurationPropertyOptions.None);
            _properties = new ConfigurationPropertyCollection {_propConnectionString};
        }

        [ConfigurationProperty("connectionStringName", DefaultValue = "")]
        public string ConnectionStringName
        {
            get
            {
                return (string)base[_propConnectionString];
            }
        }

        protected override ConfigurationPropertyCollection Properties
        {
            get { return _properties; }
        }

        public static DbInitializerSection GetSection()
        {
            return GetSection(((IWebConfiguration)SpecificInstances.DbFactory).WebConfiguration 
                              ?? WebConfigurationManager.OpenWebConfiguration("~/"));
        }

        public static DbInitializerSection GetSection(Configuration webConfiguration)
        {
            var section = (DbInitializerSection)webConfiguration.GetSection("Nat.SqlDbInitializer/DbInitializer");
            if (section == null) throw new Exception("Config not containt DbInitializer section");
            return section;
        }
    }
}