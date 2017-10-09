using System;
using System.Collections.Generic;
using System.Configuration;
using System.Web.Compilation;
using Nat.Web.Tools.Security;

namespace Nat.Web.Tools.Initialization
{
    [ConfigurationCollection(typeof(DatasourcesElement))]
    public class DatasourcesSectionCollection : ConfigurationElementCollection
    {
        private static readonly ConfigurationPropertyCollection _properties;
        private static Dictionary<string, Type> _dataSources;
        private static readonly object Lock = new object();

        static DatasourcesSectionCollection()
        {
            _properties = new ConfigurationPropertyCollection();
        }

        protected override ConfigurationElement CreateNewElement()
        {
            return new DatasourcesElement();
        }

        protected override object GetElementKey(ConfigurationElement element)
        {
            return ((DatasourcesElement)element).DataSourceName;
        }

        public new DatasourcesElement this[string dataSourceName]
        {
            get { return (DatasourcesElement)BaseGet(dataSourceName); }
        }

        public DatasourcesElement this[int index]
        {
            get { return (DatasourcesElement)BaseGet(index); }
            set
            {
                if(BaseGet(index) != null)
                    BaseRemoveAt(index);
                BaseAdd(index, value);
            }
        }

        protected override ConfigurationPropertyCollection Properties
        {
            get { return _properties; }
        }

        public void Add(DatasourcesElement initializerElement)
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

        public static Dictionary<string, Type> DataSources
        {
            get
            {
                EnsureCreateLists();
                return _dataSources;
            }
        }
        
        private static void EnsureCreateLists()
        {
            lock (Lock)
            {
                if (_dataSources != null) return;
                _dataSources = new Dictionary<string, Type>();
                var section = InitializerSection.GetSection();
                var logM = section.LogMonitor;
                logM.Init();
                foreach (DatasourcesElement element in section.Datasources)
                {
                    var type = BuildManager.GetType(element.ClassName, false, true);
                    if (type == null)
                        type = Type.GetType(element.ClassName, false, true);

                    if (type != null)
                    {
                        _dataSources[element.DataSourceName] = type;
                        continue;
                    }

                    var elementForLog = element;
                    logM.Log(
                        LogConstants.SystemErrorInApp,
                        () => new BaseLogMessageEntry
                                  {
                                      MessageCodeAsLong = LogConstants.SystemErrorInApp,
                                      DateTime = DateTime.Now,
                                      Message = string.Format(
                                          "Не найден тип '{0}' объявленного источника данных '{1}' в конфигурации",
                                          elementForLog.ClassName,
                                          elementForLog.DataSourceName),
                                  });
                }
            }
        }
    }
}