namespace Nat.Web.Tools.Initialization
{
    using System.Configuration;

    [ConfigurationCollection(typeof(ExternalSystemElement))]
    public class ExternalSystemSectionCollection : ConfigurationElementCollection
    {
        private static readonly ConfigurationPropertyCollection _properties;

        static ExternalSystemSectionCollection()
        {
            _properties = new ConfigurationPropertyCollection();
        }

        protected override ConfigurationElement CreateNewElement()
        {
            return new ExternalSystemElement();
        }

        protected override object GetElementKey(ConfigurationElement element)
        {
            return ((ExternalSystemElement)element).SystemName;
        }

        public new ExternalSystemElement this[string systemName]
        {
            get { return (ExternalSystemElement)BaseGet(systemName); }
        }

        public ExternalSystemElement this[int index]
        {
            get { return (ExternalSystemElement)BaseGet(index); }
            set
            {
                if (BaseGet(index) != null)
                    BaseRemoveAt(index);
                BaseAdd(index, value);
            }
        }

        protected override ConfigurationPropertyCollection Properties
        {
            get { return _properties; }
        }

        public void Add(ExternalSystemElement initializerElement)
        {
            BaseAdd(initializerElement);
        }

        public void Clear()
        {
            BaseClear();
        }

        internal bool IsRemoved(string key)
        {
            return BaseIsRemoved(key);
        }

        public void Remove(string key)
        {
            BaseRemove(key);
        }

        public void RemoveAt(int index)
        {
            BaseRemoveAt(index);
        }
    }
}