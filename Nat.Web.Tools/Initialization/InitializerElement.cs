using System.Configuration;

namespace Nat.Web.Tools.Initialization
{
    public class InitializerElement : ConfigurationElement
    {
        private static readonly ConfigurationProperty _propClassName;
        private static readonly ConfigurationPropertyCollection _properties;

        static InitializerElement()
        {
            _propClassName = new ConfigurationProperty("className", typeof(string), "ClassName", ConfigurationPropertyOptions.None);
            _properties = new ConfigurationPropertyCollection {_propClassName};
        }

        [StringValidator(MinLength = 1)]
        [ConfigurationProperty("className", IsRequired = true, IsKey = true, DefaultValue = "")]
        public string ClassName
        {
            get
            {
                return (string)base[_propClassName];
            }
        }

        protected override ConfigurationPropertyCollection Properties
        {
            get { return _properties; }
        }
    }
}