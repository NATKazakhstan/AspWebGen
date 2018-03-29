/*
 * Created by: Sergey V. Shpakovskiy
 * Created: 11 сентября 2008 г.
 * Copyright © JSC New Age Technologies 2008
 */

using System;
using System.Configuration;
using System.Web.Configuration;
using Nat.Tools.Specific;
using System.Web.Compilation;

namespace Nat.Web.Tools.Initialization
{
    public class InitializerSection : ConfigurationSection
    {
        private static bool? useMainPageExt;
        private static bool? registerMainScripts;
        private static bool? addFilterInMainPageInternal;
        private static bool? redirectOnSQLTimeoutInternal;

        private static readonly ConfigurationPropertyCollection _properties;
        private static readonly ConfigurationProperty _propInitializerClasses;
        private static readonly ConfigurationProperty _propFilterNamesResourcesType;
        private static readonly ConfigurationProperty _propLogMonitorType;
        private static readonly ConfigurationProperty _propExporterType;
        private static readonly ConfigurationProperty _propSiteUrl;
        private static readonly ConfigurationProperty _propSecurityRoles;
        private static readonly ConfigurationProperty _propUseMainPageExtension;
        private static readonly ConfigurationProperty _propRegisterMainScripts;
        private static readonly ConfigurationProperty _propRedirectOnSQLTimeout;
        private static readonly ConfigurationProperty _propAddFilterInMainPage;
        private static readonly ConfigurationProperty _propReportAccess;

        private static readonly ConfigurationProperty _propTypeOfMethodGetSubdivisionKSP;
        private static readonly ConfigurationProperty _propTypeOfMethodEnsurePersonInfoCorrect;
        private static readonly ConfigurationProperty _propDoesNotHavePermitionsPage;
        private static readonly ConfigurationProperty _propTypeOfMethodCheckPersonInfo;
        private static readonly ConfigurationProperty _propGroupProviderType;
        

        private static readonly ConfigurationProperty _propDatasources;
        private static readonly ConfigurationProperty _propExternalSystems;
        private static readonly ConfigurationProperty _isConvertToSSDL;

        static InitializerSection()
        {
            _propInitializerClasses = new ConfigurationProperty("initializerClasses", typeof(InitializerSectionCollection), null, ConfigurationPropertyOptions.IsDefaultCollection);
            //_propReprotPlugins = new ConfigurationProperty("reportPlugins", typeof(ReportPluginCollection), null, ConfigurationPropertyOptions.IsDefaultCollection);
            _propFilterNamesResourcesType = new ConfigurationProperty("filterNamesResourcesType", typeof(string), "Nat.Web.Controls.DefaultFilterNamesResources, Nat.Web.Controls, Version=1.4.0.0, Culture=neutral, PublicKeyToken=11c252a207597415", ConfigurationPropertyOptions.None);
            _propLogMonitorType = new ConfigurationProperty("logMonitorType", typeof(string), "Nat.Web.Controls.LogMonitor, Nat.Web.Controls, Version=1.4.0.0, Culture=neutral, PublicKeyToken=11c252a207597415", ConfigurationPropertyOptions.None);
            _propExporterType = new ConfigurationProperty("excelExporterType", typeof(string), "Nat.ExportInExcel.Exporter, Nat.ExportInExcel, Version=1.4.0.0, Culture=neutral, PublicKeyToken=11c252a207597415", ConfigurationPropertyOptions.None);
            _propSiteUrl = new ConfigurationProperty("siteUrl", typeof(string), "", ConfigurationPropertyOptions.None);
            _propSecurityRoles = new ConfigurationProperty("securityRoles", typeof(string), "", ConfigurationPropertyOptions.None);
            _propUseMainPageExtension = new ConfigurationProperty("useMainPageExtension", typeof(bool), false, ConfigurationPropertyOptions.None);
            _propRegisterMainScripts = new ConfigurationProperty("registerMainScripts", typeof(bool), false, ConfigurationPropertyOptions.None);
            _propRedirectOnSQLTimeout = new ConfigurationProperty("redirectOnSQLTimeout", typeof(bool), false, ConfigurationPropertyOptions.None);
            _propAddFilterInMainPage = new ConfigurationProperty("addFilterInMainPage", typeof(bool), false, ConfigurationPropertyOptions.None);
            _propReportAccess = new ConfigurationProperty("reportAccess", typeof(string), "Nat.Web.ReportManager.WebReportManager, Nat.Web.ReportManager, Version=1.4.0.0, Culture=neutral, PublicKeyToken=11c252a207597415", ConfigurationPropertyOptions.None);

            _propTypeOfMethodGetSubdivisionKSP = new ConfigurationProperty("typeOfMethodGetSubdivisionKSP", typeof(string), "", ConfigurationPropertyOptions.None);
            _propTypeOfMethodEnsurePersonInfoCorrect = new ConfigurationProperty("typeOfMethodEnsurePersonInfoCorrect", typeof(string), "", ConfigurationPropertyOptions.None);
            _propDoesNotHavePermitionsPage = new ConfigurationProperty("noPermitPage", typeof(string), "/NoPermit.aspx", ConfigurationPropertyOptions.None);
            _propTypeOfMethodCheckPersonInfo = new ConfigurationProperty("typeOfMethodCheckPersonInfo", typeof(string), "", ConfigurationPropertyOptions.None);
            _propGroupProviderType = new ConfigurationProperty("groupProviderType", typeof(string), "", ConfigurationPropertyOptions.None);
            
            _propDatasources = new ConfigurationProperty("datasources", typeof(DatasourcesSectionCollection), null, ConfigurationPropertyOptions.IsDefaultCollection);
            _propExternalSystems = new ConfigurationProperty("externalSystems", typeof(ExternalSystemSectionCollection), null, ConfigurationPropertyOptions.IsDefaultCollection);

            _isConvertToSSDL = new ConfigurationProperty("isConvertToSSDL", typeof(bool), false, ConfigurationPropertyOptions.None);

            _properties = new ConfigurationPropertyCollection
                              {
                                  _propInitializerClasses,
                                  _propFilterNamesResourcesType,
                                  _propLogMonitorType,
                                  _propSecurityRoles,
                                  _propUseMainPageExtension,
                                  _propRegisterMainScripts,
                                  _propRedirectOnSQLTimeout,
                                  _propAddFilterInMainPage,
                                  _propReportAccess,
                                  _propTypeOfMethodGetSubdivisionKSP,
                                  _propTypeOfMethodEnsurePersonInfoCorrect,
                                  _propDoesNotHavePermitionsPage,
                                  _propTypeOfMethodCheckPersonInfo,
                                  _propGroupProviderType,
                                  _propDatasources,
                                  _propExternalSystems,
                                  _isConvertToSSDL
                              };
        }

        public static bool UseMainPageExt => useMainPageExt ?? (useMainPageExt = GetSection().UseMainPageExtension).Value;
        internal static bool RegisterMainScriptsInternal => registerMainScripts ?? (registerMainScripts = GetSection().RegisterMainScripts).Value;
        internal static bool AddFilterInMainPageInternal => addFilterInMainPageInternal ?? (addFilterInMainPageInternal = GetSection().AddFilterInMainPage).Value;
        internal static bool RedirectOnSQLTimeoutInternal => redirectOnSQLTimeoutInternal ?? (redirectOnSQLTimeoutInternal = GetSection().RedirectOnSQLTimeout).Value;

        public static InitializerSection GetSection()
        {
            InitializerSection section;
            if (SpecificInstances.DbFactory == null)
                section = WebConfigurationManager.GetSection("Nat.Initializer/Initializer") as InitializerSection;
            else
            {
                try
                {
                    section = WebConfigurationManager.GetSection("Nat.Initializer/Initializer") as InitializerSection;
                }
                catch (Exception)
                {
                    section = null;
                }

                if (section == null)
                    section = ((IWebConfiguration)SpecificInstances.DbFactory).WebConfiguration.GetSection("Nat.Initializer/Initializer") as InitializerSection;
            }
            if (section == null) throw new Exception("Config not containt InitializerSection");
            return section;
        }

        [ConfigurationProperty("filterNamesResourcesType", DefaultValue = @"Nat.Web.Controls.DefaultFilterNamesResources, Nat.Web.Controls, Version=1.4.0.0, Culture=neutral, PublicKeyToken=11c252a207597415")]
        public string FilterNamesResourcesType
        {
            get { return (string)base[_propFilterNamesResourcesType]; }
        }

        [ConfigurationProperty("logMonitorType", DefaultValue = @"Nat.Web.Controls.LogMonitor, Nat.Web.Controls, Version=1.4.0.0, Culture=neutral, PublicKeyToken=11c252a207597415")]
        public string LogMonitorType
        {
            get { return (string)base[_propLogMonitorType]; }
        }

        [ConfigurationProperty("excelExporterType", DefaultValue = @"Nat.ExportInExcel.Exporter, Nat.ExportInExcel, Version=1.4.0.0, Culture=neutral, PublicKeyToken=11c252a207597415")]
        public string ExcelExporterType
        {
            get { return (string)base[_propExporterType]; }
        }

        [ConfigurationProperty("siteUrl", DefaultValue = @"")]
        public string SiteUrl
        {
            get { return (string)base[_propSiteUrl]; }
        }

        [ConfigurationProperty("securityRoles", DefaultValue = @"")]
        public string SecurityRoles
        {
            get { return (string)base[_propSecurityRoles]; }
        }

        [ConfigurationProperty("useMainPageExtension", DefaultValue = false)]
        public bool UseMainPageExtension => (bool)base[_propUseMainPageExtension];

        [ConfigurationProperty("registerMainScripts", DefaultValue = false)]
        public bool RegisterMainScripts => (bool)base[_propRegisterMainScripts];

        [ConfigurationProperty("redirectOnSQLTimeout", DefaultValue = false)]
        public bool RedirectOnSQLTimeout => (bool)base[_propRedirectOnSQLTimeout];

        [ConfigurationProperty("addFilterInMainPage", DefaultValue = false)]
        public bool AddFilterInMainPage => (bool)base[_propAddFilterInMainPage];

        [ConfigurationProperty("reportAccess", DefaultValue = @"Nat.Web.ReportManager.WebReportManager, Nat.Web.ReportManager, Version=1.4.0.0, Culture=neutral, PublicKeyToken=11c252a207597415")]
        public string ReportAccess
        {
            get { return (string)base[_propReportAccess]; }
        }

        [ConfigurationProperty("typeOfMethodGetSubdivisionKSP", DefaultValue = @"")]
        public string TypeOfMethodGetSubdivisionKSP
        {
            get { return (string)base[_propTypeOfMethodGetSubdivisionKSP]; }
        }
        
        [ConfigurationProperty("typeOfMethodEnsurePersonInfoCorrect", DefaultValue = @"")]
        public string TypeOfMethodEnsurePersonInfoCorrect
        {
            get { return (string)base[_propTypeOfMethodEnsurePersonInfoCorrect]; }
        }
        
        [ConfigurationProperty("noPermitPage", DefaultValue = @"/NoPermit.aspx")]
        public string DoesNotHavePermitionsPage
        {
            get { return (string)base[_propDoesNotHavePermitionsPage]; }
        }

        [ConfigurationProperty("typeOfMethodCheckPersonInfo", DefaultValue = @"")]
        public string TypeOfMethodCheckPersonInfo
        {
            get { return (string)base[_propTypeOfMethodCheckPersonInfo]; }
        }

        [ConfigurationProperty("groupProviderType", DefaultValue = @"")]
        public string GroupProviderType
        {
            get { return (string)base[_propGroupProviderType]; }
        }

        public IExporter GetExcelExporter()
        {
            var type = Type.GetType(ExcelExporterType) ?? BuildManager.GetType(ExcelExporterType, false, true);
            return (IExporter) Activator.CreateInstance(type);
        }

        public ILogMonitor LogMonitor
        {
            get
            {
                if (!string.IsNullOrEmpty(LogMonitorType))
                {
                    string typeName = LogMonitorType;
                    Type type = BuildManager.GetType(typeName, false);
                    if (type == null)
                        type = Type.GetType(typeName);
                    if (type != null)
                        return Activator.CreateInstance(type) as ILogMonitor;
                }
                return null;
            }
        }

        public static IFilterNamesResources StaticFilterNamesResources
        {
            get
            {
                return GetSection().FilterNamesResources;
            }
        }

        public IFilterNamesResources FilterNamesResources
        {
            get
            {
                if (!string.IsNullOrEmpty(FilterNamesResourcesType))
                {
                    string typeName = FilterNamesResourcesType;
                    Type type = BuildManager.GetType(typeName, false);
                    if (type == null)
                        type = Type.GetType(typeName);
                    if (type != null)
                        return Activator.CreateInstance(type) as IFilterNamesResources;
                }

                return null;
            }
        }

        protected override ConfigurationPropertyCollection Properties
        {
            get { return _properties; }
        }

        [ConfigurationProperty("initializerClasses")]
        public InitializerSectionCollection InitializerClasses
        {
            get { return (InitializerSectionCollection)base[_propInitializerClasses]; }
        }

        [ConfigurationProperty("datasources")]
        public DatasourcesSectionCollection Datasources
        {
            get { return (DatasourcesSectionCollection)base[_propDatasources]; }
        }

        [ConfigurationProperty("externalSystems")]
        public ExternalSystemSectionCollection ExternalSystems
        {
            get { return (ExternalSystemSectionCollection)base[_propExternalSystems]; }
        }

        [ConfigurationProperty("isConvertToSSDL", DefaultValue = false)]
        public bool IsConvertToSSDL => (bool)base[_isConvertToSSDL];
    }
}