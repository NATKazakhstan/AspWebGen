using System;
using System.Collections.Generic;
using System.Data.Linq;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Security.Policy;
using System.Security.Principal;
using System.Web;
using System.Web.Compilation;
using System.Web.UI;
using System.Web.UI.WebControls;
using Nat.ReportManager;
using Nat.ReportManager.QueryGeneration;
using Nat.ReportManager.ReportGeneration;
using Nat.ReportManager.ReportGeneration.SqlReportingServices;
using Nat.ReportManager.ReportGeneration.StimulSoft;
using Nat.Tools.Filtering;
using Nat.Web.Controls.DataBinding.Extended;
using Nat.Web.ReportManager.ReportGeneration;
using Nat.Web.Tools;
using Nat.Web.Controls;
using Nat.Web.Controls.GenerationClasses;
using Stimulsoft.Report;
using System.Linq;

namespace Nat.Web.ReportManager.UserControls
{
    using System.Text;

    using Nat.Tools.Specific;
    using Nat.Web.ReportManager.Properties;
    using Nat.Web.Tools.Security;

    using DBDataContext = Nat.Web.ReportManager.Data.DBDataContext;

    public partial class ReportManagerControl : UserControl
    {
        private Dictionary<string, IReportPlugin> _plugins;
        private bool _webControlsCreated;
        private bool inited;
        private ReportConditionControls reportCircle;
        private WebReportManager webReportManager;
        private int _countModelFillConditions = -1;
        private bool _isSubscription;

        private DBDataContext _db;

        protected DBDataContext DB
        {
            get
            {
                if (_db == null) _db = new DBDataContext(SpecificInstances.DbFactory.CreateConnection());
                return _db;
            }
        }

        public bool WebConditionControlsCreated
        {
            get { return _webControlsCreated; }
            set
            {
                if(!value && _webControlsCreated)
                {
                    ph.Controls.Clear();
                    phCircle.Controls.Clear();
                }
                _webControlsCreated = value;
            }
        }

        private bool IsSubscription
        {
            get { return "true".Equals(Request.QueryString["isSubscription"], StringComparison.OrdinalIgnoreCase); }
            set { _isSubscription = value; }
        }


        public WebReportManager WebReportManager
        {
            get { return webReportManager; }
        }

        public ReportConditionControls ReportCircle
        {
            get { return reportCircle; }
        }

        private Dictionary<string, IReportPlugin> Plugins
        {
            get { return _plugins; }
            set { _plugins = value; }
        }

        private string CurrentPlugin
        {
            get { return (string)ViewState["CurrentPlugin"]; }
            set { ViewState["CurrentPlugin"] = value; }
        }

        public override ControlCollection Controls
        {
            get
            {
                if(inited) EnsureWebConditionControls();
                return base.Controls;
            }
        }

        protected override void OnInit(EventArgs e)
        {
            base.OnInit(e);
            webReportManager = new WebReportManager(TreeView1);
            webReportManager.GenerateReportModuleList();
            Plugins = webReportManager.GetPlugins();
            inited = true;
        }

        private void EnsureWebConditionControls()
        {
            if(!WebConditionControlsCreated)
            {
                CreateWebConditionControls();
                WebConditionControlsCreated = true;
            }
        }

        private void CreateWebConditionControls()
        {
            if (!string.IsNullOrEmpty(CurrentPlugin) && Plugins.ContainsKey(CurrentPlugin))
            {
                webReportManager.Plugin = Plugins[CurrentPlugin];
                webReportManager.Plugin.InitializeReportCulture(webReportManager.Plugin.SupportCulture[0]);
                ((IWebReportPlugin)webReportManager.Plugin).Page = Page;
                webReportManager.SessionWorker = swReport.SessionWorker;
                webReportManager.Session = Session;
                webReportManager.CreateView();
            }
            if (webReportManager.Plugin != null)
            {
                int i = 0;
                foreach (BaseReportCondition condition in webReportManager.Filters)
                {
                    if (!condition.Visible) continue;
                    Control columnFilter = (Control)condition.ColumnFilter;
                    if (string.IsNullOrEmpty(columnFilter.ID))
                        columnFilter.ID = string.Format("column_{0}", i);
                    ph.Controls.Add(columnFilter);
                    i++;
                }
                //                rcc.SetReportCondition(webReportManager.Plugin.ModelFillConditions);
                if (webReportManager.Plugin.CreateModelFillConditions().Count > 0)
                {
                    reportCircle = new ReportConditionControls((IWebReportPlugin)webReportManager.Plugin);
                    reportCircle.ID = "rcc";
                    if (_countModelFillConditions != -1)
                        reportCircle.CountRows = _countModelFillConditions;
                    phCircle.Controls.Add(reportCircle);
                    //todo: кастыль, количество после постбака не востанавливается
                    if (_countModelFillConditions != -1)
                    {
                        reportCircle.CountRows = -1;
                        reportCircle.CountRows = _countModelFillConditions;
                    }
                }
            }
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            if(!Page.IsPostBack)
            {
                string reportName = Request.QueryString["ClassName"];
                if(!string.IsNullOrEmpty(reportName) && Plugins.ContainsKey(reportName))
                {
                    if (!webReportManager.HierarchyReports.Plugins.ContainsKey(Plugins[reportName]))
                        System.Web.HttpContext.Current.Response.Redirect("/MainPage.aspx/NoPermit");
                    HierarchyReports<TreeNode, Panel> hierarchyReports;
                    hierarchyReports = webReportManager.HierarchyReports.Plugins[Plugins[reportName]];
                    if(TreeView1.SelectedNode != null)
                        TreeView1.SelectedNode.Selected = false;
                    if (hierarchyReports.TreeControl != null)
                        hierarchyReports.TreeControl.Selected = true;
                    Page.LoadComplete +=
                        delegate
                            {
                                WebConditionControlsCreated = false;
                                SetCurrentPlugin(Plugins[reportName],
                                                 string.IsNullOrEmpty(Request.QueryString["setDefaultParams"]));
                            };
                    
                    Page.PreRenderComplete += delegate //(object sender, EventArgs e)
                                                  {
                                                      var culture = Request.QueryString["culture"] ?? "ru";
                                                      if (string.IsNullOrEmpty(Request.QueryString["open"])
                                                          || Request.QueryString["open"].Equals("on"))
                                                      {
                                                          CreateReport(culture,false);
                                                      }
                                                  };
                }
            }
            EnsureWebConditionControls();
            if(reportCircle != null)
            {
                webReportManager.Plugin.CircleFillConditions = reportCircle.GetFillCircleConditions();
//                foreach (List<BaseReportCondition> filters in reportCircle.GetFillCircleConditions())
//                {
//                    webReportManager.Filters.AddRange(filters);
//                }
            }
        }

        protected void btnCreateReportRu_Click(object sender, EventArgs e)
        {
            CreateReport("ru", false);
        }

        protected void btnCreateReportKz_Click(object sender, EventArgs e)
        {
            CreateReport("kz",false);
        }

        protected void CreateReport(string culture, bool isSubscriptions)
        {
            if(webReportManager.Plugin == null) return;
            errorDisplay.Text = "";
            ValidateReportEventArgs args = new ValidateReportEventArgs();
            if(!webReportManager.Validate(args))
            {
                errorDisplay.ShowError(webReportManager.ErrorText);
                WriteErrors(errorDisplay, args);
                return;
            }
            //webReportManager.ShowReport();
            //StiWebViewer1.Report = webReportManager.Report;
            //StiWebViewer1.ResetCurrentPage();
            if (webReportManager.Plugin != null)
            {
                StorageValues values = webReportManager.GetValues();
                var sid = new byte[] { };
                switch (this.Context.User.Identity.AuthenticationType)
                {
                    case "Windows":
                        var windowsIdentity = (WindowsIdentity)this.Context.User.Identity;
                        sid = new byte[windowsIdentity.User.BinaryLength];
                        windowsIdentity.User.GetBinaryForm(sid, 0);
                        break;
                    case "Forms": // note: Получение сида при идентификации по формам. 
                            sid = Encoding.Default.GetBytes(User.GetSID());
                        break;
                }

                if (((IWebReportPlugin) webReportManager.Plugin).AllowSaveValuesConditions)
                    StorageValues.SetStorageValues(webReportManager.Plugin.GetType().FullName, sid, values);
                if (!isSubscriptions)
                {
                    var redirectReportPlugin = webReportManager.Plugin as IRedirectReportPlugin;
                    var backPath = Request.QueryString["backPath"];
                    var backText = Request.QueryString["text"];

                    if (string.IsNullOrEmpty(backPath) && webReportManager.Plugin.Visible)
                    {
                        backPath = WebReportManager.GetReportUrl(
                                       string.Empty,
                                       webReportManager.Plugin.GetType().FullName,
                                       string.Empty,
                                       string.Empty,
                                       false) + "&open=false&setDefaultParams=true";
                    }

                    if (string.IsNullOrEmpty(backText))
                        backText = Resources.SBack;

                    if (webReportManager.Plugin.Visible)
                    {
                        RememberReports(
                            WebReportManager.GetReportUrl(
                                string.Empty,
                                webReportManager.Plugin.GetType().FullName,
                                string.Empty,
                                string.Empty,
                                false) + "&open=false&setDefaultParams=true",
                            webReportManager.Plugin);
                    }

                    if (redirectReportPlugin != null)
                    {
                        if (redirectReportPlugin.LogViewReport)
                        {
                            Tools.Security.DBDataContext.AddViewReports(
                                Tools.Security.User.GetSID(),
                                HttpContext.Current.User.Identity.Name,
                                HttpContext.Current.User.Identity.Name,
                                ReportInitializerSection.GetReportInitializerSection().ReportPageViewer + "?ClassName="
                                + redirectReportPlugin.GetType().FullName,
                                HttpContext.Current.Request.Url.GetLeftPart(UriPartial.Authority),
                                Environment.MachineName,
                                false,
                                redirectReportPlugin.GetType());
                        }

                        redirectReportPlugin.OpenReport(
                            webReportManager,
                            values,
                            Request.QueryString["rs:format"],
                            culture, HttpUtility.UrlEncode(backPath),
                            backText, Request.QueryString["rs:command"]);
                    }
                    else
                    {
                        var guid = SetSession(values, webReportManager);
                        Response.Redirect(
                            string.Format(
                                "{6}?reportName={0}&expword={4}&values={1}&text={2}&culture={5}&backPath={3}",
                                webReportManager.Plugin.GetType().FullName,
                                guid,
                                backText,
                                HttpUtility.UrlEncode(backPath),
                                Request.QueryString["expword"],
                                culture,
                                ReportInitializerSection.GetReportInitializerSection().ReportingStiReportResultPage));
                    }

                    //                    ScriptManager.RegisterStartupScript(btnCreateReport,
                    //                                                        GetType(),
                    //                                                        "open new window",
                    //                                                        string.Format("window.open('ReportResultPage.aspx?reportName={0}&refPerson={1}', '');",
                    //                                                                      webReportManager.Plugin.GetType().FullName,
                    //                                                                      refPerson,
                    //                                                                      Guid.NewGuid().ToString().Replace("-", "")),
                    //                                                        true);
                }
                else
                {
                    var guid = SetSession(values, webReportManager);
                    var url = new MainPageUrlBuilder
                                  {
                                      UserControl = "ReportSubscriptionsEdit",
                                      IsDataControl = true,
                                      IsNew = true
                                  };
                    url.CustomQueryParameters.Add("guid", guid);
                    /*Определение типа отчета*/
                    var isSqlReportingServices = Convert.ToByte(webReportManager.Plugin is ISqlReportingServicesPlugin);                    
                    url.CustomQueryParameters.Add("isSqlReportingServices", isSqlReportingServices.ToString());
                    url.CustomQueryParameters.Add("reportName", webReportManager.Plugin.GetType().FullName);
                    url.CustomQueryParameters.Add("qscommand", Request.QueryString["rs:command"]);
                    url.CustomQueryParameters.Add("culture", culture);
                    /*Определениe формата выгрузки отчета в зависимости от типа отчета*/
                    string format="";
                    if (isSqlReportingServices==0)
                    {
                        var webReportPlugin = (IWebReportPlugin)webReportManager.Plugin;
                        var stiPlugin = (IStimulsoftReportPlugin)webReportPlugin;
                        var expToWord = !string.IsNullOrEmpty(Request.QueryString["expword"]);
                        if (!expToWord && stiPlugin.AutoExportTo == null)
                        {
                            format = Request.QueryString["expword"];
                        }
                        else if (stiPlugin.AutoExportTo != null)
                            format = stiPlugin.AutoExportTo.Value.ToString();
                    } else format = Request.QueryString["rs:format"];                   
                    /**/
                    url.CustomQueryParameters.Add("format", format);
                    Page.Response.Redirect(url.CreateUrl(false, true));
                }
            }
        }

        private void RememberReports(string path, IReportPlugin plugin)
        {
            var typeStr = ReportInitializerSection.GetReportInitializerSection().RememberLastReportType;
            if (string.IsNullOrEmpty(typeStr))
                return;

            var type = BuildManager.GetType(typeStr, true, true);
            var obj = (IRememberReports)Activator.CreateInstance(type);
            obj.CreateReport(path, plugin);
        }

        internal static string SetSession(StorageValues values, WebReportManager webReportManager)
        {
            var guid = Guid.NewGuid().ToString();
            HttpContext.Current.Session[guid] = values;
            HttpContext.Current.Session["logmsg" + guid] = webReportManager.GetLogInformation().Replace("\r\n", "<br/>");
            HttpContext.Current.Session["logcode" + guid] = webReportManager.GetLogCode(webReportManager.Plugin);
            HttpContext.Current.Session["constants" + guid] = ((IWebReportPlugin)webReportManager.Plugin).Constants;
            return guid;
        }

        public static void WriteErrors(ErrorDisplay errorDisplay, ValidateReportEventArgs args)
        {
            foreach(IColumnFilter columnFilter in args.Errors)
                errorDisplay.ShowError(columnFilter.GetStorage().Caption + " - " + columnFilter.ErrorText);
        }

        protected void TreeView1_SelectedNodeChanged(object sender, EventArgs e)
        {
            ImageCheckBox.Checked = false;
            IReportPlugin plugin = webReportManager.HierarchyReports.Controls[TreeView1.SelectedNode].ReportPlugin;
            if (webReportManager.Plugin == plugin) return;
            webReportManager.CloseReports();
            swReport.SessionWorker.RemoveObject();
            WebConditionControlsCreated = false;
//            bool isRedirect = sender == this;

            SetCurrentPlugin(plugin, false);
        }

        private void SetCurrentPlugin(IReportPlugin plugin, bool isRedirect)
        {
            webReportManager.Plugin = plugin;
            plugin.InitializeReportCulture(plugin.SupportCulture[0]);
            IWebReportPlugin webPlugin = plugin as IWebReportPlugin;
            if (webPlugin != null) webPlugin.Page = Page;
            swReport.RefreshSessionWorker();
            if (webReportManager.Plugin != null)
            {
                CurrentPlugin = webReportManager.Plugin.GetType().FullName;
                if (isRedirect)
                    ((IWebReportPlugin)webReportManager.Plugin).DefaultValue = Request.QueryString["idrec"];
                webReportManager.Plugin.OnReportOpening();
            }
            else
                CurrentPlugin = "";
            webReportManager.SessionWorker = swReport.SessionWorker;
            if (!isRedirect)
            {
                StorageValues values = null;
                IWebReportPlugin webReportPlugin = (IWebReportPlugin)webReportManager.Plugin;
                if (webReportPlugin != null && webReportPlugin.AllowSaveValuesConditions)
                {
                    byte[] sid = new byte[] { };
                    switch (this.Context.User.Identity.AuthenticationType)
                    {
                        case "Windows":
                            var windowsIdentity = (WindowsIdentity)this.Context.User.Identity;
                            sid = new byte[windowsIdentity.User.BinaryLength];
                            windowsIdentity.User.GetBinaryForm(sid, 0);
                            break;
                        case "Forms": // note: Получение сида при идентификации по формам. 
                            sid = Encoding.Default.GetBytes(User.GetSID());
                            break;
                    }
                    if (sid != null && sid.Length > 0)
                        values = StorageValues.GetStorageValues(webReportManager.Plugin.GetType().FullName, sid);
                    if (values != null && values.CountListValues > 0)
                        _countModelFillConditions = values.CountListValues;
                }
                if (values != null)
                    webReportManager.InitValues(values, webReportPlugin.InitSavedValuesInvisibleConditions);
            }
            else if (IsSubscription)
            {
                IWebReportPlugin webReportPlugin = (IWebReportPlugin)webReportManager.Plugin;
                if (webReportPlugin != null)
                {
                    tdTreeView.Visible = false;
                    TreeView1.Visible = false;
                    btnCreateReportKz.Visible = false;
                    btnCreateReportRu.Visible = false;
                    ImageCheckBox.Visible = false;                    
                    btnSubscriptionsSaveParams.Visible = true;
                    StorageValues values = null;
                    values = (StorageValues) Session[Request.QueryString["idStorageValues"]];
                    if (values != null && values.CountListValues > 0)
                        _countModelFillConditions = values.CountListValues;
                    if (values != null)
                        webReportManager.InitValues(values, webReportPlugin.InitSavedValuesInvisibleConditions);
                }
            }
            EnsureWebConditionControls();
        }

        protected void swReport_SessionWorkerInit(object sender, SessionWorkerArgs e)
        {
            Type type = null;
            if (webReportManager.Plugin != null && webReportManager.Plugin.Table != null)
                type = webReportManager.Plugin.Table.DataSet.GetType();
            var sessionWorker = new SessionWorker(Page, swReport.Key);
            var obj = sessionWorker.Object;
            if(type == null) sessionWorker.RemoveObject();
            if(((obj != null && obj.GetType() != type) || obj == null) && type != null)
                sessionWorker.Object = Activator.CreateInstance(type);
            e.SessionWorker = sessionWorker;
        }

        protected override void OnPreRender(EventArgs e)
        {
            base.OnPreRender(e);

            EnsureWebConditionControls();
            var visible = webReportManager.Plugin != null && !IsSubscription;
            btnCreateReportRu.Visible = false;
            btnCreateReportKz.Visible = false;
            btnSubscriptionsJournal.Visible = false;
            var showOnlyCulture = Request.QueryString["culture"];
            if (visible)
            {
                var cultures = webReportManager.Plugin.SupportCulture;
                var cultureCaption = webReportManager.Plugin.CultureCaption;
                for (var i = 0; i < cultures.Length; i++)
                {
                    if (cultures[i].StartsWith("ru") && (string.IsNullOrEmpty(showOnlyCulture) || showOnlyCulture == cultures[i]))
                    {
                        btnCreateReportRu.Visible = true;
                        btnCreateReportRu.Text = cultureCaption[i];
                    }
                    else if (cultures[i].StartsWith("kz") && (string.IsNullOrEmpty(showOnlyCulture) || showOnlyCulture == cultures[i]))
                    {
                        btnCreateReportKz.Visible = true;
                        btnCreateReportKz.Text = cultureCaption[i];
                    }
                }

                if (!(webReportManager.Plugin is CrossJournalReportPlugin) && !(webReportManager.Plugin is IReportAutoExport))
                    btnSubscriptionsJournal.Visible = true;
            }

            StiWebViewer1.Visible = StiWebViewer1.Report != null;
            if(!string.IsNullOrEmpty(CurrentPlugin) && Plugins.ContainsKey(CurrentPlugin))
                lReport.Text = Plugins[CurrentPlugin].Description;
            else
                lReport.Text = string.Empty;

            if (!WebReportManager.IsTreeViewControlVisible)
            {
                tdTreeView.Visible = false;
                checkboxPanel.Visible = false;
                btnSubscriptionsJournal.Visible = false;
                btnClearValueConditions.Visible = visible;
            }

            SetClearFormJavaScript();
        }

        private void SetClearFormJavaScript()
        {
            if (WebReportManager.Plugin == null) return;
            var sb = new StringBuilder();
            sb.Append("var list = [");
            var listExists = false;
            var inputElements = WebReportManager.Plugin.Conditions
                .Where(r => r.Visible)
                .Select(r => r.ColumnFilter)
                .OfType<IDefaultFilterValues>()
                .SelectMany(r => r.GetInputElements());
            foreach (var inputElement in inputElements)
            {
                sb.Append("{id:'").Append(inputElement.ClientId).Append("', value:'").Append(inputElement.Value).Append("'}, ");
                listExists = true;
            }

            if (!listExists)
            {
                btnClearValueConditions.OnClientClick = "return false;";
                return;
            }

            sb.Remove(sb.Length - 2, 2);
            sb.AppendLine("];");
            sb.AppendLine("ClearFormValues(list); return false;");
            btnClearValueConditions.OnClientClick = sb.ToString();
        }

        protected void btnSubscriptionsJournal_Click(object sender, EventArgs e)
        {
            CreateReport("kz", true);
        }

        protected void btnSubscriptionsSaveParams_Click(object sender, EventArgs e)
        {
            var idSubscription = Convert.ToInt64(Request.QueryString["idSubscription"]);
            var row = this.DB.ReportSubscriptions.FirstOrDefault(q => q.id == idSubscription);
            if (row != null)
            {
                // values
                var value = webReportManager.GetValues();
                row.values = ReportSubscriptionsHelper.ObjectToBinary(value);

                // constants
                var constants = ((IWebReportPlugin)Plugins[CurrentPlugin]).Constants;
                row.constants = ReportSubscriptionsHelper.ObjectToBinary(constants);
                DB.SubmitChanges();

                ReportSubscriptionsHelper.UpdateReportSubscriptionParams(
                    DB, row.id, Page, ph, value, row.reportName);
            }

            Page.Response.Redirect(Request.QueryString["url"]);
        }
    }
}