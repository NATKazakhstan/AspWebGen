/*
 * Created by: Sergey V. Shpakovskiy
 * Created: 8 но€бр€ 2008 г.
 * Copyright © JSC New Age Technologies 2008
 */

using System;
using System.Diagnostics;
using System.Linq;
using System.Security.Principal;
using System.Web;
using System.Web.Compilation;
using System.Web.UI;
using System.Web.UI.WebControls;
using Nat.Web.Controls.GenerationClasses;
using Nat.Web.Controls.Preview;
using Nat.Web.Controls.Trace;
using Nat.Web.Tools;
using System.Web.UI.HtmlControls;
using System.IO;
using System.Collections.Generic;
using Nat.Web.Tools.Initialization;
using System.Data.SqlClient;
using Nat.Web.Controls.Properties;
using Nat.Web.Tools.Security;
#if !LOCAL && ForSharepoint
using Microsoft.SharePoint;
#endif

[assembly: WebResource("Nat.Web.Controls.GenerationClasses.MainScripts.js", "text/javascript")]

namespace Nat.Web.Controls
{
    
 //todo: попробовать заюзать Microsoft.SharePoint.Portal.WebControls.PersonalWebPartPage
    using System.Text.RegularExpressions;

    using Nat.Web.Controls.GenerationClasses.BaseJournal;
    using Nat.Web.Controls.GenerationClasses.Navigator;

#if !LOCAL && ForSharepoint
    public abstract class BaseMainPage : Microsoft.SharePoint.WebPartPages.WebPartPage
#else
    public abstract class BaseMainPage : Page
#endif
    {
        private Control _control;
        private Control _filterControl;
        private HiddenField hiddenfieldForUrl;
        UpdateProgressBar _updateProgressBar;
        private PicturePreview _picturePreview;
        protected virtual string DefaultControl { get { return null; } }
        private Regex regexPublicKeyToken = new Regex("PublicKeyToken=(.+)", RegexOptions.Compiled | RegexOptions.IgnoreCase);

        private ScriptManager _ScriptManager;

        public ScriptManager ScriptManager
        {
            get
            {
                if (_ScriptManager == null)
                    _ScriptManager = ScriptManager.GetCurrent(Page);
                return _ScriptManager;
            }
        }
/*
 * а€ксовые постбаки не работают
 *
#if DEBUG //write time of creating page
        private DateTime time;

        protected override void OnPreInit(EventArgs e)
        {
            time = DateTime.Now;
            base.OnPreInit(e);
        }

        protected override void Render(HtmlTextWriter writer)
        {
            base.Render(writer);
            writer.Write("Time to create Page: ");
            writer.Write((DateTime.Now - time).TotalSeconds.ToString("#0.000"));
            writer.Write(" of seconds");
        }
#endif
//*/

        protected virtual bool EmptyPage { get; set; }

        protected override void OnPreInit(EventArgs e)
        {
            WebInitializer.Initialize();
            base.OnPreInit(e);
            HttpContext.Current.Items["CurrentPage"] = this;
        }

        public static byte[] GetFile(string uri, Page page)
        {
            var url = new MainPageUrlBuilder(uri);
            if (!url.IsDownload)
                throw new ArgumentException("is incorrect url");
            var type = BuildManager.GetType(url.QueryParameters["ManagerType"], false, true);
            var fileManager = (IFileManager2) Activator.CreateInstance(type);
            if (!fileManager.CheckPermit(page)) return null;
            return fileManager.GetFile(Convert.ToInt64(url.QueryParameters["ID"]), url.QueryParameters["fieldName"]);
        }

        protected override void OnInit(EventArgs e)
        {
            if (File.Exists(HttpContext.Current.Server.MapPath("WebServiceFilters.asmx")))
            {
                ScriptManager?.Services.Add(
                    new ServiceReference
                        {
                            Path = "/WebServiceFilters.asmx",
                        });
            }

            Trace.WriteExt("BaseMainPage.BeginOnInit");
            if (PlaceHolderInUpInternal != null)
            {
                hiddenfieldForUrl = new HiddenField();
                hiddenfieldForUrl.ID = "hfCUrl";
                PlaceHolderInUpInternal.Controls.Add(hiddenfieldForUrl);
                var value = Request.Params[hiddenfieldForUrl.UniqueID];
                if (!string.IsNullOrEmpty(value))
                    MainPageUrlBuilder.Current = new MainPageUrlBuilder(value);
            }

            var url = MainPageUrlBuilder.Current;
            if (url.IsDownload)
            {
                if (url.QueryParameters.ContainsKey("ManagerType"))
                {
                    var type = BuildManager.GetType(url.QueryParameters["ManagerType"], false, true);
                    var fileManager = (IFileManager)Activator.CreateInstance(type);
                    if (fileManager.CheckPermit(Page))
                    {
                        fileManager.DownloadFile(Convert.ToInt64(url.QueryParameters["ID"]), url.QueryParameters["fieldName"], Response);
                        _control = Page.LoadControl("/UserControls/FileNotExists.ascx");
                    }
                    else
                        _control = Page.LoadControl("/UserControls/NoPermit.ascx");
                    PlaceHolder.Controls.Add(_control);
                }
            }

            if (url.IsExecute)
            {
                if (url.QueryParameters.ContainsKey("ManagerType"))
                {
                    var type = BuildManager.GetType(url.QueryParameters["ManagerType"], false, true);
                    var executeManager = (IExecuteManager)Activator.CreateInstance(type);
                    if (executeManager.CheckPermit(Page))
                        executeManager.Execute(url.QueryParameters, Page);
                    else
                        _control = Page.LoadControl("/UserControls/NoPermit.ascx");
                    PlaceHolder.Controls.Add(_control);
                }
            }
            else if (!string.IsNullOrEmpty(url.UserControl))
            {
                if (!EmptyPage)
                {
                    _updateProgressBar = new UpdateProgressBar
                        {
                            ModalPopupBehaviorID = "UpdateProgressBar"
                        };
                    _picturePreview = (PicturePreview)LoadControl("/Preview/PicturePreview.ascx");
                    Form.Controls.Add(_updateProgressBar);
                    Form.Controls.Add(_picturePreview);
                }

                string redirectUrl = GetRedirectUrl(url.UserControl);
                if(!string.IsNullOrEmpty(redirectUrl))
                    Response.Redirect(redirectUrl);
                _control = BaseMainPage.LoadControl(Page, url.UserControl);
                //проверка прав, если нет прав, то загрузить контрол "Ќет прав"
                var access = _control as IAccessControl;
                Trace.WriteExt("BaseMainPage.OnInit.BeginCheckPermit");
                if ((access != null && !access.CheckPermit(this)))
                    _control = Page.LoadControl("/UserControls/NoPermit.ascx");
                Trace.WriteExt("BaseMainPage.OnInit.EndCheckPermit");
                if (!UserRoles.IsInRole(UserRoles.ADMIN))
                {
                    Trace.WriteExt("BaseMainPage.OnInit.BeginEnsureRecordCardCorrect");
                    var userControl = EnsureRecordCardCorrect();
                    Trace.WriteExt("BaseMainPage.OnInit.EndEnsureRecordCardCorrect");
                    if (userControl != null) _control = userControl;
                }

                if (url.NavigateTo)
                {
                    var navControl = (AbstractUserControl)_control;
                    var projectName = navControl.ProjectName;
                    var tableName = navControl.TableName;
                    var baseType = navControl.GetType().BaseType;

                    if (string.IsNullOrEmpty(projectName))
                        projectName = baseType.Assembly.FullName.Split(',')[0];

                    if (string.IsNullOrEmpty(tableName) && baseType.Name.EndsWith(MainPageUrlBuilder.UserControlTypeEdit))
                    {
                        tableName = baseType.Name;
                        tableName = tableName.Substring(0, tableName.Length - MainPageUrlBuilder.UserControlTypeEdit.Length);
                    }

                    var navigateUrl = NavigatorManager.GetReadUrlForRecord(
                        projectName,
                        tableName,
                        url.QueryParameters[MainPageUrlBuilder.ReferencIDPrefix + tableName],
                        url.QueryParameters.ContainsKey(MainPageUrlBuilder.NavigateToDestinationParentTableName)
                            ? url.QueryParameters[MainPageUrlBuilder.NavigateToDestinationParentTableName]
                            : string.Empty,
                        regexPublicKeyToken.Match(baseType.Assembly.FullName).Groups[1].Value);
                    Response.Redirect(navigateUrl);
                }

                var selected = _control as ISelectedValue;
                _control.ID = "item";
                var placeHolder = GetMainPlaceHolder();
                if (url.TimeoutInSQL)
                {
                    placeHolder.Controls.Add(new Literal
                                                 {
                                                     Text = "<span class=\"font14\" style=\"color:red\">"
                                                            + Resources.SFilterInSqlTimeOutException
                                                            + "</span>",
                                                 });
                }

                if (_control is IFilterSupport fSupport && InitializerSection.AddFilterInMainPageInternal && (url.IsSelect || url.ShowFilter))
                {
                    _filterControl = LoadControlFilter(fSupport.GetDefaultFilterControl());
                    _filterControl.ID = "filter";
                    placeHolder.Controls.Add(_filterControl);
                    placeHolder.Controls.Add(new Literal { Text = "<br /><br />" });
                    fSupport.FilterControl = _filterControl.ID;
                }

                if (selected != null)
                {
                    selected.ShowHistory = url.ShowHistory;
                    selected.IsNew = url.IsNew;
                    selected.IsRead = url.IsRead;
                    selected.IsSelect = url.IsSelect;
                }

                placeHolder.Controls.Add(_control);
            }
            else if (!string.IsNullOrEmpty(DefaultControl))
            {
                _control = Page.LoadControl(DefaultControl);
                if (_control != null) PlaceHolder.Controls.Add(_control);
            }
            Trace.WriteExt("BaseMainPage.EndOnInit");
            base.OnInit(e);
            PreRenderComplete += Page_PreRenderComplete;
        }

        protected override void OnPreLoad(EventArgs e)
        {
            base.OnPreLoad(e);
            CreateActionControls();
        }

        public Control EnsureRecordCardCorrect()
        {
            UserControl control = null;
            var message = UserRoles.CheckPersonInfo();
            if(string.IsNullOrEmpty(message)) return null;
            control = (UserControl)Page.LoadControl("/UserControls/NoPermit.ascx");
            control.Attributes["ErrorMessage"] = message;
            return control;
        }

        public static Control LoadControl(Page page, string controlName)
        {
            var watch = new Stopwatch();
            watch.Start();
            try
            {
                var userControlFile = controlName + ".ascx";
                var control = GetControl(page, controlName, userControlFile, "Journal");
                if (control != null) return control;
                control = GetControl(page, controlName, userControlFile, "Edit");
                if (control != null) return control;
                control = GetControl(page, controlName, userControlFile, "Filter");
                if (control != null) return control;
                return page.LoadControl("/UserControls/" + userControlFile);
            }
            finally
            {
                TraceLoadControl(controlName, watch.ElapsedMilliseconds);
            }
        }

        public static TFilterType LoadFilterControl<TFilterType>(Page page, string controlName)
            where TFilterType: UserControl, new()
        {
            var watch = new Stopwatch();
            watch.Start();
            try
            {
                if (page == null)
                    return new TFilterType();
                var userControlFile = controlName + ".ascx";
                var control = GetControl(page, controlName, userControlFile, "Filter");
                if (control != null) return (TFilterType)control;
                return (TFilterType)page.LoadControl("/UserControls/" + userControlFile);
            }
            finally
            {
                TraceLoadControl(controlName, watch.ElapsedMilliseconds);
            }
        }

        private static void TraceLoadControl(string controlName, long ms)
        {
            // если загрузка больше чем 50 мс, то логируем эту информацию
            if (ms <= 50) return;
            string message = string.Format("Load control '{0}'; time {1} ms", controlName, ms);
            HttpContext.Current.Trace.WriteExt("LoadControl", message);
        }

        public Control LoadControlFilter(string controlName)
        {
            var timeStart = DateTime.Now.Ticks;
            try
            {
                var path = Page.Request.MapPath("/");
                var userControlFile = controlName + ".ascx";
                var control = GetControl(Page, controlName, userControlFile, "Filter");
                if (control != null) return control;
                return Page.LoadControl("/UserControls/" + userControlFile);
            }
            finally
            {
                TraceLoadControl(controlName, DateTime.Now.Ticks - timeStart);
            }
        }

        private static Control GetControl(Page page, string controlName, string userControlFile, string endsWith)
        {
            string file;
            if (controlName.Length > endsWith.Length && controlName.EndsWith(endsWith, StringComparison.OrdinalIgnoreCase))
            {
                var dir = controlName.Substring(0, controlName.Length - endsWith.Length);
                if (dir.StartsWith("Wuc", StringComparison.OrdinalIgnoreCase)) dir = dir.Substring(3);
                file = Path.Combine("/UserControls/" + dir, userControlFile);
                if (File.Exists(HttpContext.Current.Request.MapPath(file)))
                    return page.LoadControl("/" + file);
            }

            return null;
        }

        protected virtual Control CheckUser()
        {
            return null;
        }

        protected override void InitializeCulture()
        {
            if (string.IsNullOrEmpty(MainPageUrlBuilder.Current.Culture))
                LocalizationHelper.SetCulture(Page);
            else
                LocalizationHelper.SetCulture(MainPageUrlBuilder.Current.Culture, this);
            base.InitializeCulture();
        }

        protected override void OnPreRender(EventArgs e)
        {
            base.OnPreRender(e);

            // ѕриходитс€ делать SetCulture, т.к. есть подозрение что Sharepoint в какой то момент устанавливает свою Culture
            LocalizationHelper.SetCulture(Page);
            if (_updateProgressBar != null)
                RegisterStartupScript(Page, GetType(), _updateProgressBar.ClientID);
            if (_picturePreview != null)
                RegisterPicturePreviewStartupScript(Page, GetType(), _picturePreview.ControlID);
            if (InitializerSection.RegisterMainScriptsInternal)
                ClientScript.RegisterClientScriptResource(typeof(BaseMainPage), "Nat.Web.Controls.GenerationClasses.MainScripts.js");
            if (MainPageUrlBuilder.Current.TimeoutInSQL && MainPageUrlBuilder.Current.IsDataControl)
            {
                var url = MainPageUrlBuilder.Current.Clone();
                url.TimeoutInSQL = false;
                url.IsFilterWindow = false;
                url.UserControl = url.UserControl.Substring(0, url.UserControl.Length - 6) + "Journal"; //-"Filter".Length
                url.ControlFilterParameters.Clear();
                Page.ClientScript.RegisterStartupScript(GetType(), 
                    "SetApplyFilterUrl",
                    string.Format("\r\napplyFilterUrl = '{0}';", url.CreateUrl()),
                    true);
            }

            if (!UserRoles.IsInRole(UserRoles.AllowCopyTextFromWebBrowser))
            {
                Page.ClientScript.RegisterStartupScript(GetType(), "disableSelectText", "document.body.oncopy = disableSelectText;", true);
                Form.Attributes["oncopy"] = "return false;";
            }
        }

        public static void RegisterStartupScript(Page page, Type type, string progeressBarID)
        {
            page.ClientScript.RegisterStartupScript(type, "RequestToServerSetModalPopup", "\r\nfunction UpdateRefModalPopap(){ modalPopup = $get('" + progeressBarID + "');}\r\n", true);
        }

        public static void RegisterPicturePreviewStartupScript(Page page, Type type, string previewControlID)
        {
            page.ClientScript.RegisterStartupScript(type, "OpenPreviewPicture", "\r\nfunction OpenPreviewPicture(url, fileName){ _OpenPreviewPicture('" + previewControlID + "', url, fileName);}\r\n", true);
        }

        private void Page_PreRenderComplete(object sender, EventArgs e)
        {
            var headerControl = _control as IHeaderControl;
            if (headerControl != null)
            {
                Title = PageTitle = headerControl.Header;
                Page.Header.Title = headerControl.Header;
            }

            if (Form != null && (MainPageUrlBuilder.Current.IsDataControl || MainPageUrlBuilder.Current.IsCustomUserControl))
            {
                Form.Action = MainPageUrlBuilder.Current.CreateUrl(true, true);
                if (hiddenfieldForUrl != null)
                    hiddenfieldForUrl.Value = Form.Action;
                ChangeFormSubmit(Form);
            }
        }

        public static void ChangeFormSubmit(HtmlForm form)
        {
            form.Attributes["onsubmit"] = "javascript:return __doPostBackInMainPage();";
        }

        protected void smp_ItemCreated(object sender, SiteMapNodeItemEventArgs e)
        {
            //if (e.Item.SiteMapNode != null) _title = e.Item.SiteMapNode.Title;
        }

        protected abstract PlaceHolder PlaceHolder { get; }
        protected abstract PlaceHolder PlaceHolderInUP { get; }

        protected PlaceHolder GetMainPlaceHolder()
        {
            return MainPageUrlBuilder.Current.IsSelect ? PlaceHolderInUP : PlaceHolder;
        }

        protected PlaceHolder GetActionPlaceHolder()
        {
            return MainPageUrlBuilder.Current.IsSelect
                ? PlaceHolderInUP
                : (PlaceHolderForActions ?? PlaceHolder);

        }

        protected virtual PlaceHolder PlaceHolderInUpInternal
        { 
            get { return null; } 
        }

        protected abstract string GetRedirectUrl(string userControl);

        protected string PageTitle { get; set; }

        protected override void OnError(EventArgs e)
        {
            base.OnError(e);

            if (!InitializerSection.RedirectOnSQLTimeoutInternal)
                return;

            var sqlException = HttpContext.Current.Error as SqlException;
            var url = MainPageUrlBuilder.Current;
            if (sqlException != null && sqlException.Number == 2
                && url.IsDataControl
                && url.UserControl.EndsWith("Journal"))
            {
                url.UserControl = url.UserControl.Substring(0, url.UserControl.Length - 7) + "Filter"; //"Journal".Length
                url.IsFilterWindow = true;
                url.TimeoutInSQL = true;
                HttpContext.Current.Response.Redirect(url.CreateUrl());
            }
        }
        
        #region AddControlForAction

        protected virtual UpdatePanel UpdatePanelForActions
        {
            get { return null; }
        }

        protected virtual PlaceHolder PlaceHolderForActions
        {
            get { return null; }
        }

        private List<ActionControlParameters> ActionControlNames 
        {
            get
            {
                var controls = (List<ActionControlParameters>)ViewState["ActionControls"];
                if (controls == null)
                    ViewState["ActionControls"] = controls = new List<ActionControlParameters>();
                return controls;
            }
        }

        private List<Control> ActionControls { get; set; }

        public void AddActionControl(ActionControlParameters parameter)
        {
            var control = LoadControl(Page, parameter.UserControl);
            if (control != null)
            {
                var access = control as IAccessControl;
                Trace.WriteExt("BaseMainPage.AddActionControl.BeginCheckPermit");
                if (access != null && !access.CheckPermit(this))
                    return;
                Trace.WriteExt("BaseMainPage.AddActionControl.EndCheckPermit");

                var actionControl = (IActionControl)control;
                actionControl.AsActionControl = true;
                actionControl.IsFirstCreation = true;
                ActionControlNames.Add(parameter);

                SetActionType(parameter, (ISelectedValue)control);

                AddActionControl(control);

                if (ActionControls == null)
                    ActionControls = new List<Control>();
                ActionControls.Add(control);
                SetVisibleForActionControls();
            }
        }

        private void SetActionType(ActionControlParameters parameter, ISelectedValue control)
        {
            switch (parameter.ActionType)
            {
                case ActionControlType.AddNewItem:
                    control.IsNew = true;
                    break;
                case ActionControlType.EditItem:
                    control.IsRead = false;
                    control.SetParentValue(parameter.Value);
                    break;
                case ActionControlType.LookItem:
                    control.IsRead = true;
                    control.SetParentValue(parameter.Value);
                    break;
                default:
                    break;
            }
        }

        private void SetVisibleForActionControls()
        {
            if (_control != null)
                _control.Visible = ActionControls.Count == 0;
            if (_filterControl != null)
                _filterControl.Visible = ActionControls.Count == 0;
            foreach (var control in ActionControls.Take(ActionControls.Count - 1))
            {
                control.Visible = false;
                var updatePanel = ControlHelper.FindControl<UpdatePanel>(control);
                if (updatePanel != null) updatePanel.Update();
            }
            var lastControl = ActionControls.LastOrDefault();
            if (lastControl != null)
            {
                lastControl.Visible = true;
                var updatePanel = ControlHelper.FindControl<UpdatePanel>(lastControl);
                if (updatePanel != null) updatePanel.Update();
            }
        }

        public void CreateActionControls()
        {
            if (ActionControlNames.Count == 0) return;
            if (ActionControls == null)
                ActionControls = new List<Control>();
            foreach (var parameter in ActionControlNames)
            {
                var control = LoadControl(Page, parameter.UserControl);

                var access = control as IAccessControl;
                Trace.WriteExt("BaseMainPage.CreateActionControls.BeginCheckPermit");
                if (access != null && !access.CheckPermit(this))
                    return;
                Trace.WriteExt("BaseMainPage.CreateActionControls.EndCheckPermit");

                var actionControl = (IActionControl)control;
                actionControl.AsActionControl = true;
                SetActionType(parameter, (ISelectedValue)control);
                AddActionControl(control);
                ActionControls.Add(control);
            }
            SetVisibleForActionControls();
        }

        public void RemoveActionControl(Control control, params object[] backValues)
        {
            if (ActionControls == null) return;
            var index = ActionControls.IndexOf(control);
            if (index < 0) return;
            var actionParameter = ActionControlNames[index];
            
            ActionControls.RemoveAt(index);
            ActionControlNames.RemoveAt(index);
            RemoveActionControl(control);

            SetVisibleForActionControls();
            SetBackValues(new ActionControlResults { ActionParameter = actionParameter, ResultValues = backValues });
        }

        public void RemoveActionControl(Control control, ActionControlResults result)
        {
            if (ActionControls == null) return;
            var index = ActionControls.IndexOf(control);
            if (index < 0) return;
            result.ActionParameter = ActionControlNames[index];

            ActionControls.RemoveAt(index);
            ActionControlNames.RemoveAt(index);
            RemoveActionControl(control);
            //placeHolder.Controls.Remove(control);
            SetVisibleForActionControls();
            SetBackValues(result);
        }

        private void SetBackValues(ActionControlResults result)
        {
            var lastControl = (ActionControls.LastOrDefault() ?? _control) as IActionControl;
            if (lastControl != null)
                lastControl.ResultActionValues(result);
        }

        private void AddActionControl(Control control)
        {
            UpdatePanel updatePanel = new UpdatePanel
                {
                    UpdateMode = UpdatePanelUpdateMode.Conditional,
                };
            var placeHolder = GetActionPlaceHolder();
            updatePanel.ContentTemplateContainer.Controls.Add(control);
            placeHolder.Controls.Add(updatePanel);

            updatePanel = ControlHelper.FindControl<UpdatePanel>(placeHolder);
            if (updatePanel != null) updatePanel.Update();
        }

        private void RemoveActionControl(Control control)
        {
            var placeHolder = GetActionPlaceHolder();
            var updatePanel = ControlHelper.FindControl<UpdatePanel>(control);
            placeHolder.Controls.Remove(updatePanel);
            updatePanel = ControlHelper.FindControl<UpdatePanel>(placeHolder);
            if (updatePanel != null) updatePanel.Update();
        }

        #endregion
    }
}
