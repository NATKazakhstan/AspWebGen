using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Configuration;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Web;
using System.Web.Compilation;
using Nat.ReportManager.QueryGeneration;
using Nat.Web.Controls;
using Nat.Web.Controls.Trace;
using Nat.Web.Tools.Initialization;
using Nat.Web.Tools.Security;

namespace Nat.Web.ReportManager
{
    [ConfigurationCollection(typeof(ReportPluginInfo))]
    public class ReportPluginCollection : ConfigurationElementCollection
    {
        private static readonly ConfigurationPropertyCollection _properties;
        private List<IReportList> _reportList;

        static ReportPluginCollection()
        {
            _properties = new ConfigurationPropertyCollection();
        }

        public new ReportPluginInfo this[string reportPluginName]
        {
            get { return (ReportPluginInfo)BaseGet(reportPluginName); }
        }

        public ReportPluginInfo this[int index]
        {
            get { return (ReportPluginInfo)BaseGet(index); }
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

        public void Add(ReportPluginInfo reportPluginInformation)
        {
            BaseAdd(reportPluginInformation);
        }

        public void Clear()
        {
            BaseClear();
        }

        protected override ConfigurationElement CreateNewElement()
        {
            return new ReportPluginInfo();
        }

        protected override object GetElementKey(ConfigurationElement element)
        {
            return ((ReportPluginInfo)element).ReportPlugin;
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

        public Type[] GetReportPlugins()
        {
            CreateLists();
            return _reportList.SelectMany(r => r.GetReportTypes()).ToArray();
        }

        public Dictionary<Type, IReportList> GetTypeReportLists()
        {
            CreateLists();
            return _reportList
                .SelectMany(r => r.GetReportTypes().Select(t => new {Type = t, List = r}))
                .ToDictionary(r => r.Type, r => r.List);
        }

        private void CreateLists()
        {
            if (_reportList != null)
                return;

            var list = new List<IReportList>(Count);
            var reportListTypeNames = new List<string>();
            var errors = new StringBuilder();
            
            foreach (ReportPluginInfo pluginInfo in this)
            {
                if(!reportListTypeNames.Contains(pluginInfo.ReportPlugin.ToLower()))
                    reportListTypeNames.Add(pluginInfo.ReportPlugin.ToLower());
            }

            foreach (string reportListTypeName in reportListTypeNames)
            {
                Type type = null;
                try
                {
                    type = Type.GetType(reportListTypeName, false, true) ??
                           BuildManager.GetType(reportListTypeName, false, true);
                    if (type != null)
                    {
                        var reportList = Activator.CreateInstance(type) as IReportList;
                        if (reportList != null)
                            list.Add(reportList);
                    }
                    else
                    {
                        errors.AppendLine("Not found type '" + reportListTypeName);
                        errors.AppendLine();
                        errors.AppendLine();
                    }
                }
                catch (Exception e)
                {
                    errors.AppendLine("Can't create '" + (type == null ? reportListTypeName : type.FullName) + "':");
                    errors.AppendLine(e.ToString());
                    errors.AppendLine();
                    errors.AppendLine();
                }
            }

            _reportList = list;

            if (errors.Length > 0)
            {
                var errorsStr = errors.ToString();
                var sid = HttpContext.Current == null || HttpContext.Current.User == null ? "Nat.Initializer" : User.GetSID();
                var logMonitor = InitializerSection.GetSection().LogMonitor;
                logMonitor.Init();
                logMonitor.Log(
                    LogConstants.SystemErrorInApp,
                    () => new LogMessageEntry(sid, LogMessageType.SystemErrorInApp, errorsStr));
            }
        }
    }
}