using System;
using System.Configuration;
using System.Web.Configuration;
using Nat.Tools.Specific;
using Nat.Web.Tools;

namespace Nat.Web.ReportManager
{
    public class ReportInitializerSection : ConfigurationSection
    {
        private static readonly ConfigurationProperty _propColumnFilterFactoryType;
        private static readonly ConfigurationPropertyCollection _properties;
        private static readonly ConfigurationProperty _propReprotPlugins;
        private static readonly ConfigurationProperty _propReportPageViewer;
        private static readonly ConfigurationProperty _propReportingStiReportResultPage;
        private static readonly ConfigurationProperty _propReportingServicesPageViewer;
        private static readonly ConfigurationProperty _propReportIsTreeViewControlVisible;
        private static readonly ConfigurationProperty _propSaveDataFile;
        private static readonly ConfigurationProperty _propDefaultTreeExpanded;
        private static readonly ConfigurationProperty _propReportingServicesUrl;
        private static readonly ConfigurationProperty _propReportingServicesReportsFolder;
        private static readonly ConfigurationProperty _propReportingServicesUserName;
        private static readonly ConfigurationProperty _propReportingServicesPassword;
        private static readonly ConfigurationProperty _propReportingServicesUserDomain;
//        private static readonly ConfigurationProperty _propReportPageResult;
        private static Configuration WebConfiguration;

        static ReportInitializerSection()
        {
            _propColumnFilterFactoryType = new ConfigurationProperty("columnFilterFactoryType", typeof(string), "Nat.Web.Controls.Filters.WebColumnFilterFactory, Nat.Web.Controls, Version=1.0.0.0, Culture=neutral, PublicKeyToken=11c252a207597415", ConfigurationPropertyOptions.None);
            _propReprotPlugins = new ConfigurationProperty("reportPlugins", typeof(ReportPluginCollection), null, ConfigurationPropertyOptions.IsDefaultCollection);
            _propReportPageViewer = new ConfigurationProperty("reportPageViewer", typeof(string), @"/ReportViewer.aspx", ConfigurationPropertyOptions.None);
            _propReportingStiReportResultPage = new ConfigurationProperty("reportingStiPageViewer", typeof(string), @"/ReportResultPage.aspx", ConfigurationPropertyOptions.None);
            _propReportingServicesPageViewer = new ConfigurationProperty("reportingServicesPageViewer", typeof(string), @"/ReportingServicesViewer.aspx", ConfigurationPropertyOptions.None);

            _propReportIsTreeViewControlVisible = new ConfigurationProperty("isTreeViewControlVisible", typeof(bool), true, ConfigurationPropertyOptions.None);

            _propSaveDataFile = new ConfigurationProperty("saveDataFile", typeof(string), @"", ConfigurationPropertyOptions.None);
            _propDefaultTreeExpanded = new ConfigurationProperty("defaultTreeExpanded", typeof(bool), false, ConfigurationPropertyOptions.None);
//            _propReportPageResult = new ConfigurationProperty("reportPageResult", typeof(string), @"~\ReportResultPage.aspx", ConfigurationPropertyOptions.None);
            _propReportingServicesUrl = new ConfigurationProperty("reportingServicesUrl", typeof(string), "", ConfigurationPropertyOptions.None);
            _propReportingServicesReportsFolder = new ConfigurationProperty("reportingServicesReportsFolder", typeof(string), "", ConfigurationPropertyOptions.None);
            _propReportingServicesUserName = new ConfigurationProperty("reportingServicesUserName", typeof(string), "", ConfigurationPropertyOptions.None);
            _propReportingServicesPassword = new ConfigurationProperty("reportingServicesPassword", typeof(string), "", ConfigurationPropertyOptions.None);
            _propReportingServicesUserDomain = new ConfigurationProperty("reportingServicesUserDomain", typeof(string), "", ConfigurationPropertyOptions.None);
            _properties = new ConfigurationPropertyCollection
                              {
                                  _propColumnFilterFactoryType,
                                  _propReprotPlugins,
                                  _propReportPageViewer,
                                  _propReportingStiReportResultPage,
                                  _propReportingServicesPageViewer,
                                  _propReportIsTreeViewControlVisible,
                                  _propSaveDataFile,
                                  _propDefaultTreeExpanded,
                                  _propReportingServicesUrl,
                                  _propReportingServicesReportsFolder,
                                  _propReportingServicesUserName,
                                  _propReportingServicesPassword,
                                  _propReportingServicesUserDomain,
                              };
//            _properties.Add(_propReportPageResult);
        }

        public static ReportInitializerSection GetReportInitializerSection()
        {
            //var section = WebConfigurationManager.GetSection("Nat.WebReportManager/ReportInitializer") as ReportInitializerSection;
            //var section = WebConfiguration == null ? WebConfigurationManager.GetSection("Nat.WebReportManager/ReportInitializer") as ReportInitializerSection : WebConfiguration.GetSection("Nat.WebReportManager/ReportInitializer") as ReportInitializerSection;
            var WebConfiguration = ((IWebConfiguration)SpecificInstances.DbFactory).WebConfiguration;
            var section = WebConfiguration == null
             ? WebConfigurationManager.GetSection("Nat.WebReportManager/ReportInitializer") as ReportInitializerSection
            : WebConfiguration.GetSection("Nat.WebReportManager/ReportInitializer")
             as ReportInitializerSection;
            if (section == null) throw new Exception("Config not containt ReportInitializer section");
            return section;
        }

        [ConfigurationProperty("columnFilterFactoryType", DefaultValue = "Nat.Web.Controls.Filters.WebColumnFilterFactory, Nat.Web.Controls, Version=1.0.0.0, Culture=neutral, PublicKeyToken=11c252a207597415")]
        public string ColumnFilterFactoryType
        {
            get
            {
                return (string)base[_propColumnFilterFactoryType];
            }
        }

        protected override ConfigurationPropertyCollection Properties
        {
            get { return _properties; }
        }

        [ConfigurationProperty("reportPageViewer", DefaultValue = @"/ReportViewer.aspx")]
        public string ReportPageViewer
        {
            get { return (string)base[_propReportPageViewer]; }
        }

        [ConfigurationProperty("reportPagePath", DefaultValue = @"/ReportResultPage.aspx")]
        public string ReportingStiReportResultPage
        {
            get { return (string)base[_propReportingStiReportResultPage]; }
        }
        
        [ConfigurationProperty("reportingServicesPageViewer", DefaultValue = @"/ReportingServicesViewer.aspx")]
        public string ReportingServicesPageViewer
        {
            get { return (string)base[_propReportingServicesPageViewer]; }
        }
        
        [ConfigurationProperty("saveDataFile", DefaultValue = @"")]
        public string PropSaveDataFile
        {
            get { return (string)base[_propSaveDataFile]; }
        }

        [ConfigurationProperty("defaultTreeExpanded", DefaultValue = false)]
        public bool DefaultTreeExpanded
        {
            get { return (bool)base[_propDefaultTreeExpanded]; }
        }

        [ConfigurationProperty("isTreeViewControlVisible", DefaultValue = true)]
        public bool IsTreeViewControlVisible
        {
            get { return (bool)base[_propReportIsTreeViewControlVisible]; }
        }

//        [ConfigurationProperty("reportPageResult")]
//        public string ReportPageResult
//        {
//            get { return (string)base[_propReportPageResult]; }
//        }

        [ConfigurationProperty("reportPlugins")]
        public ReportPluginCollection ReprotPlugins
        {
            get { return (ReportPluginCollection)base[_propReprotPlugins]; }
        }

        [ConfigurationProperty("reportingServicesUrl", DefaultValue = @"")]
        public string ReportingServicesUrl
        {
            get { return (string)base[_propReportingServicesUrl]; }
        }

        [ConfigurationProperty("reportingServicesReportsFolder", DefaultValue = @"")]
        public string ReportingServicesReportsFolder
        {
            get { return (string)base[_propReportingServicesReportsFolder]; }
        }

        [ConfigurationProperty("reportingServicesUserName", DefaultValue = @"")]
        public string ReportingServicesUserName
        {
            get { return (string)base[_propReportingServicesUserName]; }
        }

        [ConfigurationProperty("reportingServicesPassword", DefaultValue = @"")]
        public string ReportingServicesPassword
        {
            get { return (string)base[_propReportingServicesPassword]; }
        }

        [ConfigurationProperty("reportingServicesUserDomain", DefaultValue = @"")]
        public string ReportingServicesUserDomain
        {
            get { return (string)base[_propReportingServicesUserDomain]; }
        }
        
        
    }
}