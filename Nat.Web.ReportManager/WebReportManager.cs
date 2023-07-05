using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Web.Compilation;
using System.Web.SessionState;
using System.Web.UI.WebControls;
using Nat.Controls.DataGridViewTools;
using Nat.ReportManager;
using Nat.ReportManager.QueryGeneration;
using Nat.ReportManager.ReportGeneration;
using Nat.Web.Tools.Security;
using Nat.Tools.Filtering;
using Nat.Tools.QueryGeneration;
using Nat.Tools.Specific;
using Nat.Web.Controls;
using Nat.Web.Tools;
using Nat.Web.Tools.Initialization;
using Stimulsoft.Report;
using System.Web;
using System.Text;

namespace Nat.Web.ReportManager
{
    using System.Linq;
    using Nat.Web.Controls.Trace;

    public class WebReportManager : BaseReportManager<TreeNode, Panel>, IReportAccess
    {
        private readonly List<BaseReportCondition> _filters = new List<BaseReportCondition>();
        private readonly TreeView _treeView;
        private DataSet[] _dataSets;
        private IReportPlugin _plugin;
        private Dictionary<string, IReportPlugin> _plugins;
//        private static object _lockPlugins = new object();
        private StiReport _report;
        private bool _requireNewControls;
        private bool _requireNewReport;
        private bool? _defaultTreeExpanded;
        private bool? _isTreeViewControlVisible;
        private SessionWorker _sessionWorker;
        private readonly Dictionary<DataSet, SessionWorker> _dataSetSessions = new Dictionary<DataSet, SessionWorker>();
        private readonly List<string> _sessionKeys = new List<string>();
        private HttpSessionState _session;
        private Dictionary<Type, IReportList> _typeReportLists;

        public WebReportManager()
        {
        }

        public WebReportManager(TreeView treeView)
        {
            _treeView = treeView;
        }

        public override bool ExecuteInitializeReportConstants
        {
            get { return false; }
        }

        protected override Dictionary<string, IReportPlugin> Plugins
        {
            get
            {
                if (_plugins != null)
                    return _plugins;

                _plugins = GetPlugins(out _typeReportLists);

                return _plugins;
            }
        }

        public static Dictionary<string, IReportPlugin> GetPlugins(out Dictionary<Type, IReportList> typeReportLists)
        {
            var errors = new StringBuilder();
            var plugins = new Dictionary<string, IReportPlugin>();
            var tPlugin = typeof(IWebReportPlugin);
            var section = ReportInitializerSection.GetReportInitializerSection();
            typeReportLists = null;
            try
            {
                var types = section.ReprotPlugins.GetReportPlugins();
                foreach (var type in types.Where(tPlugin.IsAssignableFrom))
                {
                    try
                    {
                        var instance = (IWebReportPlugin) Activator.CreateInstance(type);
                        plugins.Add(type.FullName, instance);
                    }
                    catch (Exception e)
                    {
                        errors.AppendLine("Can't create '" + type.FullName + "':");
                        errors.AppendLine(e.ToString());
                        errors.AppendLine();
                        errors.AppendLine();
                    }
                }

                typeReportLists = section.ReprotPlugins.GetTypeReportLists();
            }
            catch (Exception e)
            {
                errors.AppendLine(e.ToString());
                errors.AppendLine();
                errors.AppendLine();
            }

            if (errors.Length > 0)
            {
                var errorsStr = errors.ToString();
                var sid = HttpContext.Current == null || HttpContext.Current.User == null ? "Nat.Initializer" : User.GetSID();
                var logMonitor = InitializerSection.GetSection().LogMonitor;
                logMonitor.Init();
                logMonitor.Log(
                    LogConstants.SystemErrorInApp,
                    () => new LogMessageEntry(sid, LogMessageType.SystemErrorInApp, errorsStr));

                if (HttpContext.Current != null)
                    TraceContextExt.WarnExt(HttpContext.Current.Trace, errorsStr);
            }

            return plugins;
        }

        public bool RoleCheck { get; set; }

        public override IReportPlugin Plugin
        {
            get { return _plugin; }
            set
            {
                if (!RoleCheck||CanAccess(value)) 
                    _plugin = value;
                else
                    _plugin = null;
            }
        }

        public override List<BaseReportCondition> Filters
        {
            get { return _filters; }
        }

        public StiReport Report
        {
            get { return _report; }
        }

        public DataSet[] DataSets
        {
            get { return _dataSets; }
        }

        public bool RequireNewReport
        {
            get { return _requireNewReport; }
        }

        public bool RequireNewControls
        {
            get { return _requireNewControls; }
        }

        protected bool DefaultTreeExpanded
        {
            get
            {
                if (_defaultTreeExpanded == null)
                    _defaultTreeExpanded = ReportInitializerSection.GetReportInitializerSection().DefaultTreeExpanded;
                return _defaultTreeExpanded.Value;
            }
        }

        public bool IsTreeViewControlVisible
        {
            get
            {
                if (_isTreeViewControlVisible == null)
                    _isTreeViewControlVisible = ReportInitializerSection.GetReportInitializerSection().IsTreeViewControlVisible;
                return _isTreeViewControlVisible.Value;
            }
        }

        public SessionWorker SessionWorker
        {
            get { return _sessionWorker; }
            set { _sessionWorker = value; }
        }

        public HttpSessionState Session
        {
            get { return _session; }
            set { _session = value; }
        }

        public override string ErrorText { get; set; }

        protected override bool CanAccess(IReportPlugin plugin)
        {
            var webReportPlugin = (IWebReportPlugin)plugin;
            string[] roles = webReportPlugin.Roles();
            if (UserRoles.IsInAnyRoles(roles)) return true;
            return roles.Length == 0;
        }

        public override void GenerateReportModuleList()
        {
            _treeView.Nodes.Clear();
            base.GenerateReportModuleList();
            foreach(TreeNode treeNode in _treeView.Nodes)
            {
                treeNode.CollapseAll();
                if (DefaultTreeExpanded)
                    treeNode.Expand();
            }
        }

        internal Dictionary<string, IReportPlugin> GetPlugins()
        {
            return Plugins;
        }

        protected static void CreateControls(TreeNodeCollection nodes, HierarchyReports<TreeNode, Panel> hierarchyReports)
        {
            foreach(var report in hierarchyReports.ChildrenReports)
            {
                if (report.Invisible)
                {
                    CreateControls(nodes, report);
                }
                else
                {
                    var added = new TreeNode(report.Name);
                    if (report.ReportPlugin == null)
                        added.SelectAction = TreeNodeSelectAction.Expand;
                    else
                        added.ImageUrl = ((IWebReportPlugin) report.ReportPlugin).ImageUrl;
                    nodes.Add(added);
                    report.TreeControl = added;
                    CreateControls(added.ChildNodes, report);
                }
            }
        }

        protected override void CreateControls(HierarchyReports<TreeNode, Panel> hierarchyReports)
        {
            CreateControls(_treeView.Nodes, hierarchyReports);
//            foreach (TreeNode parentNode in _treeView.Nodes)
//            {
//                foreach(TreeNode node in parentNode.ChildNodes)
//                {
//                    parentNode.CollapseAll();
//                }
//            }
        }

        public override void ShowNewReport(StiReport report, ParameterText[] parametersText, params DataSet[] dataSets)
        {
            _dataSets = dataSets;
            _report = report;
            foreach(DataSet dataSet in dataSets)
                _report.RegData(dataSet);
            _requireNewReport = true;
//            _report.Render();
        }

        public override void ShowNewReport(string reportUrl, QueryConditionList conditionList)
        {
            var parameters = conditionList.GetFilterParameters();
            var sb = new StringBuilder();
            sb.Append(ReportInitializerSection.GetReportInitializerSection().ReportingServicesPageViewer);
            sb.Append("?");
            sb.Append(reportUrl);
            foreach (var parameter in parameters)
            {
                sb.Append("&");
                sb.Append(parameter.Key);
                sb.Append("=");
                sb.Append(parameter.Value);
            }
            HttpContext.Current.Response.Redirect(sb.ToString());
        }

        protected override void AddControls(BaseReportCondition[] conditions)
        {
            foreach(var condition in conditions)
            {
                var supportSessionWorker = condition.ColumnFilter as ISupportSessionWorker;
                if (supportSessionWorker == null) continue;

                var dataSource = GetDataTable(condition.ColumnFilter.GetStorage());
                if (dataSource == null || _session == null)
                    supportSessionWorker.SessionWorker = _sessionWorker;
                else
                {
                    if (_dataSetSessions.ContainsKey(dataSource.DataSet))
                        supportSessionWorker.SessionWorker = _dataSetSessions[dataSource.DataSet];
                    else
                    {
                        string key = Plugin.GetType().Name + "_" + dataSource.DataSet.DataSetName;
                        int i = 0;
                        while (_sessionKeys.Contains(key))
                            key = Plugin.GetType().Name + "_" + dataSource.DataSet.DataSetName + i++;

                        var sWorker = new SessionWorker();
                        sWorker.SetSession(_session);
                        sWorker.Key = key;
                        if (sWorker.Object == null || sWorker.Object.GetType() != dataSource.DataSet.GetType())
                            sWorker.Object = dataSource.DataSet;
                        _sessionKeys.Add(key);
                        _dataSetSessions.Add(dataSource.DataSet, sWorker);
                        supportSessionWorker.SessionWorker = sWorker;
                    }
                }
            }
//            _filters.AddRange(conditions);
            _requireNewControls = true;
        }

        protected override DataTable GetDataTable(ColumnFilterStorage storage)
        {
            return DataSourceHelper.GetDataTable(storage.RefDataSource);
        }

        public override void CreateView()
        {
            _filters.Clear();
            switch(Plugin.ViewType)
            {
//                case ViewType.CustomView:
//                    {
//                        Control control = (Control)Plugin.Control;
//                        _baseControl.Controls.Add(control);
//                    }
//                    break;
                case ViewType.DefaultView:
                    GenerateControls();
                    break;
                case ViewType.UnderDevelopment:
                    break;
                default:
                    throw new NotImplementedException(string.Format("ViewType {0} doesn't supported", Plugin.ViewType));
            }
        }

        public void InitValues(StorageValues storageValues, bool initInvisible)
        {
            if (Plugin == null) throw new Exception("Свойство Plugin не может быть равен null");

            foreach (var filter in Plugin.Conditions)
            {
                if(!filter.Visible && !initInvisible) continue;
                var storage = filter.ColumnFilter.GetStorage();
                storageValues.SetStorage(storage);
                filter.ColumnFilter.SetStorage(storage);
            }
            Plugin.SetCountCircleFillConditions(storageValues.CountListValues, true);
            for (int i = 0; i < storageValues.CountListValues; i++)
            {
                foreach (var filter in Plugin.CircleFillConditions[i])
                {
                    if (!filter.Visible && !initInvisible) continue;
                    var storage = filter.ColumnFilter.GetStorage();
                    storageValues.SetListStorage(storage, i);
                    filter.ColumnFilter.SetStorage(storage);
                }
            }
        }

        public StorageValues GetValues()
        {
            if (Plugin == null) throw new Exception("Свойство Plugin не может быть равен null");

            var storageValues = new StorageValues(new Hashtable(), new List<Hashtable>());

            foreach (var filter in Plugin.Conditions)
            {
                var storage = filter.ColumnFilter.GetStorage();
                var text = filter.Visible ? filter.ColumnFilter.GetTexts() : null;
                storageValues.AddStorage(storage, text);
            }
            if (Plugin.CircleFillConditions != null)
            {
                for (int i = 0; i < Plugin.CircleFillConditions.Count; i++)
                {
                    foreach (var filter in Plugin.CircleFillConditions[i])
                    {
                        var storage = filter.ColumnFilter.GetStorage();
                        var text = filter.Visible ? filter.ColumnFilter.GetTexts() : null;
                        storageValues.AddListStorage(storage, i, text);
                    }
                }
            }
            return storageValues;
        }

        public LogMessageType GetLogCode(IReportPlugin plugin)
        {
            return _typeReportLists[plugin.GetType()].LogMessageType;
        }

        public override ParameterText[] GetTextOfParameters()
        {
            return new ParameterText[0];
        }

        /// <summary>
        /// Сформировать ссылку на отчет
        /// </summary>
        /// <param name="idrec">Передача параметров атчету</param>
        /// <param name="pluginType">Тип плагина отчета</param>
        /// <param name="backText">Текст для кнопки вернуться</param>
        /// <param name="backUrl">Адрес кнопки "вернуться"</param>
        /// <param name="export">Сразу выполнить экспорт</param>
        /// <returns>Адрес отчета</returns>
        public static string GetReportUrl(string idrec, Type pluginType, string backText, string backUrl, bool export)
        {
            return GetReportUrl(idrec, pluginType.FullName, backText, backUrl, export);
        }

        /// <summary>
        /// Сформировать ссылку на отчет
        /// </summary>
        /// <param name="idrec">Передача параметров атчету</param>
        /// <param name="pluginType">Тип плагина отчета</param>
        /// <param name="backText">Текст для кнопки вернуться</param>
        /// <param name="backUrl">Адрес кнопки "вернуться"</param>
        /// <param name="export">Сразу выполнить экспорт</param>
        /// <returns>Адрес отчета</returns>
        public static string GetReportUrl(string idrec, string pluginType, string backText, string backUrl, bool export)
        {
            var reportUrl = ReportInitializerSection.GetReportInitializerSection().ReportPageViewer;
            idrec = idrec == "{0}" ? idrec : HttpUtility.UrlEncode(idrec);
            pluginType = HttpUtility.UrlEncode(pluginType);
            if (export)
                return $@"{reportUrl}?expword=1&idrec={idrec}&ClassName={pluginType}";
            return $@"{reportUrl}?idrec={idrec}&ClassName={pluginType}&text={HttpUtility.UrlEncode(backText)}&backPath={HttpUtility.UrlEncode(backUrl)}&open=true";
        }

        /// <summary>
        /// Сформировать ссылку на отчет
        /// </summary>
        /// <param name="idrec">Передача параметров атчету</param>
        /// <param name="pluginType">Тип плагина отчета</param>
        /// <param name="backText">Текст для кнопки вернуться</param>
        /// <param name="backUrl">Адрес кнопки "вернуться"</param>
        /// <param name="export">Сразу выполнить экспорт</param>
        /// <returns>Адрес отчета</returns>
        public static string GetReportUrl(string idrec, string pluginType, string backText, string backUrl, string culture, bool export)
        {
            var reportPageViewer = ReportInitializerSection.GetReportInitializerSection().ReportPageViewer;
            idrec = idrec == "{0}" ? idrec : HttpUtility.UrlEncode(idrec);
            pluginType = HttpUtility.UrlEncode(pluginType);
            if (export)
                return $@"{reportPageViewer}?expword=1&idrec={idrec}&ClassName={pluginType}&culture={culture}";
            return string.Format(
                @"{0}?idrec={1}&ClassName={2}&text={3}&culture={5}&backPath={4}&open=true",
                reportPageViewer,
                idrec,
                pluginType,
                HttpUtility.UrlEncode(backText),
                HttpUtility.UrlEncode(backUrl),
                culture);
        }

        private static object[] BuildConstants(BaseReportCondition condition, ColumnFilterStorage storage, bool addFilterType)
        {
            List<object> constants;
            if (storage.IsRefBound)
            {
                if (storage.FilterType.IsBinaryFilter())
                {
                    try
                    {
                        constants = new List<object>
                            {
                                condition.ColumnFilter.GetText(0), condition.ColumnFilter.GetText(1) 
                            };
                    }
                    catch (NotImplementedException)
                    {
                        constants = new List<object> { storage.Value1, storage.Value2 };
                    }
                }
                else
                {
                    try
                    {
                        constants = new List<object> { condition.ColumnFilter.GetText(0) };
                    }
                    catch (NotImplementedException)
                    {
                        constants = new List<object> { storage.Value1 };
                    }
                }
            }
            else
            {
                constants = storage.FilterType.IsBinaryFilter()
                           ? new List<object> { storage.Value1, storage.Value2 }
                               : new List<object> { storage.Value1 };
            }

            if (addFilterType)
            {
                constants.Insert(0, storage.FilterType);
            }

            return constants.ToArray();
        }

        public static Dictionary<string, object> CreateConstants(List<BaseReportCondition> conditions, List<List<BaseReportCondition>> circleFillConditions, bool addFilterType)
        {
            var contants = new Dictionary<string, object>();
            foreach (BaseReportCondition condition in conditions)
            {
                ColumnFilterStorage storage = condition.ColumnFilter.GetStorage();
                contants[storage.Name] = BuildConstants(condition, storage, addFilterType);
            }
            
            if (circleFillConditions != null)
            {
                var i = 0;
                foreach (List<BaseReportCondition> circleFillCondition in circleFillConditions)
                {
                    foreach (BaseReportCondition condition in circleFillCondition)
                    {
                        ColumnFilterStorage storage = condition.ColumnFilter.GetStorage();
                        if (i == 0)
                            contants.Add(storage.Name, new object[circleFillConditions.Count]);
                        var m = (object[])contants[storage.Name];
                        m[i] = BuildConstants(condition, storage, addFilterType);
                    }

                    i++;
                }
            }

            return contants;
        }

        public static IReportPlugin GetPlugin(string reportPluginName)
        {
            var section = ReportInitializerSection.GetReportInitializerSection();
            var errors = new StringBuilder();
            var tPlugin = typeof(IWebReportPlugin);
            IWebReportPlugin reportPlugin = null;
            try
            {
                var types = section.ReprotPlugins.GetReportPlugins();
                foreach (var type in types.Where(tPlugin.IsAssignableFrom))
                {
                    try
                    {
                        if (type.FullName.Equals(reportPluginName))
                        {
                            reportPlugin = (IWebReportPlugin)Activator.CreateInstance(type);
                            break;
                        }
                    }
                    catch (Exception e)
                    {
                        errors.AppendLine("Can't create '" + type.FullName + "':");
                        errors.AppendLine(e.ToString());
                        errors.AppendLine();
                        errors.AppendLine();
                    }
                }
            }
            catch (Exception e)
            {
                errors.AppendLine(e.ToString());
                errors.AppendLine();
                errors.AppendLine();
            }

            if (errors.Length > 0)
            {
                var errorsStr = errors.ToString();
                var sid = HttpContext.Current.User == null ? "Nat.Initializer" : User.GetSID();
                var logMonitor = InitializerSection.GetSection().LogMonitor;
                logMonitor.Init();
                logMonitor.Log(
                    LogConstants.SystemErrorInApp,
                    () => new LogMessageEntry(sid, LogMessageType.SystemErrorInApp, errorsStr));
                TraceContextExt.WarnExt(HttpContext.Current.Trace, errorsStr);
            }

            return reportPlugin;
        }

        public bool DoesHaveUserPermission(string pluginName)
        {
            var plugin = (IWebReportPlugin)GetPlugin(pluginName);
            if (plugin == null)
                return false;

            return UserRoles.IsInAnyRoles(plugin.Roles());
        }

        public static IEnumerable<StiExportFormat> GetAvailableStiFormat(object plugin)
        {
            var exportPermissions = plugin as IExportPermission;
            if (exportPermissions == null)
                return new [] {StiExportFormat.Pdf, StiExportFormat.Word2007, StiExportFormat.Excel2007};
            
            var list = new List<StiExportFormat>(3);

            var roles = exportPermissions.GetWordRoles();
            if (roles == null || roles.Length == 0 || UserRoles.IsInAnyRoles(roles))
                list.Add(StiExportFormat.Word2007);
            
            roles = exportPermissions.GetPdfRoles();
            if (roles == null || roles.Length == 0 || UserRoles.IsInAnyRoles(roles))
                list.Add(StiExportFormat.Pdf);

            roles = exportPermissions.GetExcelRoles();
            if (roles == null || roles.Length == 0 || UserRoles.IsInAnyRoles(roles))
                list.Add(StiExportFormat.Excel2007);

            return list;
        }

        public static IEnumerable<ExportFormat> GetAvailableFormat(object plugin)
        {
            var exportPermissions = plugin as IExportPermission;
            var roles = GetExportWithWatermarkRole();
            if (exportPermissions == null)
            {
                if (UserRoles.IsInAnyRoles(roles))
                {
                    return new[] { ExportFormat.Pdf, ExportFormat.Excel, ExportFormat.Word };
                }
                return new[] {ExportFormat.Pdf};
            }
            
            var list = new List<ExportFormat>(3);

            var w = exportPermissions.GetWordRoles()?.ToList();
            w?.Add(GetExportWithWatermarkRole()[0]);
            roles = w?.ToArray();
            if (roles == null || roles.Length == 0 || UserRoles.IsInAnyRoles(roles))
                list.Add(ExportFormat.Word);

            roles = exportPermissions.GetPdfRoles();
            if (roles == null || roles.Length == 0 || UserRoles.IsInAnyRoles(roles))
                list.Add(ExportFormat.Pdf);

            var x = exportPermissions.GetExcelRoles()?.ToList();
            x?.Add(GetExportWithWatermarkRole()[0]);
            roles = x?.ToArray();
            if (roles == null || roles.Length == 0 || UserRoles.IsInAnyRoles(roles))
                list.Add(ExportFormat.Excel);

            return list;
        }

        private static string[] GetExportWithWatermarkRole()
        {
            return new []{ "kvv Export_WordExcel_WithWatermark" };
        }
    }
}