/*
 * Created by: Sergey V. Shpakovskiy
 * Created: 11 сентября 2008 г.
 * Copyright © JSC New Age Technologies 2008
 */

using System;
using System.Collections.Generic;
using System.Configuration;
using System.Web.Compilation;

namespace Nat.Web.Tools.Initialization
{
    [ConfigurationCollection(typeof(InitializerElement))]
    public class InitializerSectionCollection : ConfigurationElementCollection
    {
        private static readonly ConfigurationPropertyCollection _properties;
        private Dictionary<Type, IInitializer> _Initializers;
        private static readonly object _lock = new object();

        static InitializerSectionCollection()
        {
            _properties = new ConfigurationPropertyCollection();
        }

        protected override ConfigurationElement CreateNewElement()
        {
            return new InitializerElement();
        }

        protected override object GetElementKey(ConfigurationElement element)
        {
            return ((InitializerElement) element).ClassName;
        }

        public new InitializerElement this[string reportPluginName]
        {
            get { return (InitializerElement)BaseGet(reportPluginName); }
        }

        public InitializerElement this[int index]
        {
            get { return (InitializerElement)BaseGet(index); }
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

        public void Add(InitializerElement initializerElement)
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

        public Dictionary<Type, IInitializer> GetInitializers()
        {
            EnsureCreateLists();
            return _Initializers;
        }

        private void EnsureCreateLists()
        {
            lock(_lock)
            {
                if (_Initializers != null) return;

                var classNames = new List<string>();
                var typeReportLists = new Dictionary<Type, IInitializer>();

                foreach (InitializerElement element in this)
                {
                    if (!classNames.Contains(element.ClassName))
                        classNames.Add(element.ClassName);
                }
                classNames.Sort();

                Type typeInitializer = typeof(IInitializer);
                Type typeLogMonitor = typeof(ILogMonitor);
                foreach (string plugin in classNames)
                {
                    Type type = BuildManager.GetType(plugin, true, true);
                    if (type != null)
                    {
                        if (IsInterfaceOf(type, typeInitializer))
                        {
                            var reportList = (IInitializer)Activator.CreateInstance(type);
                            if (reportList != null)
                            {
                                typeReportLists.Add(type, reportList);
                            }
                        }
                    }
                }
                _Initializers = typeReportLists;
            }
        }

        private bool IsInterfaceOf(Type type, Type interfaceType)
        {
            return !type.IsInterface && interfaceType.IsAssignableFrom(type)
                        && !type.ContainsGenericParameters && !type.IsAbstract;
        }

        public void Initialize()
        {
            foreach (var value in GetInitializers().Values)
                value.Initialize();
        }
    }
}