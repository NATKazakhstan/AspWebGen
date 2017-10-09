using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web;

namespace Nat.Web.Tools.Initialization
{
    public class DatasourcesElement : ConfigurationElement
    {
        private static readonly ConfigurationProperty PropClassName;
        private static readonly ConfigurationProperty PropDataSourceName;
        private static readonly ConfigurationPropertyCollection _properties;

        static DatasourcesElement()
        {
            PropClassName = new ConfigurationProperty("className", typeof (string), "", ConfigurationPropertyOptions.None);
            PropDataSourceName = new ConfigurationProperty("dataSourceName", typeof (string), "", ConfigurationPropertyOptions.None);
            _properties = new ConfigurationPropertyCollection
                              {
                                  PropClassName,
                                  PropDataSourceName,
                              };
        }

        [StringValidator(MinLength = 1)]
        [ConfigurationProperty("className", IsRequired = true, IsKey = true, DefaultValue = "")]
        public string ClassName
        {
            get { return (string) base[PropClassName]; }
        }

        [StringValidator(MinLength = 1)]
        [ConfigurationProperty("dataSourceName", IsRequired = true, IsKey = true, DefaultValue = "")]
        public string DataSourceName
        {
            get { return (string)base[PropDataSourceName]; }
        }

        protected override ConfigurationPropertyCollection Properties
        {
            get { return _properties; }
        }
    }
}