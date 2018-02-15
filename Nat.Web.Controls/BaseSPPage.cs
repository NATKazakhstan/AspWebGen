using System;
using System.Data;
using System.Data.Common;
using System.Security.Principal;
using System.Web.UI;
using Nat.Tools.Specific;
using Nat.Web.Tools;
using Nat.Web.Tools.Initialization;
using Nat.Web.Tools.Security;

namespace Nat.Web.Controls
{
    public class BaseSPPage : Page, IPage
    {
        #region поля

        private LogMonitorWeb logMonitor;
        private Guid? pageGuid;
        UpdateProgressBar _updateProgressBar;
        private ScriptManager _ScriptManager;

        #endregion

        #region Methods

        protected override void OnInit(EventArgs e)
        {
            WebInitializer.Initialize();
            //ScriptManager.Services.Add(new ServiceReference { Path = "/WebServiceFilters.asmx", });
            _updateProgressBar = new UpdateProgressBar { ID = "modalUpdateProgressBar", ModalPopupBehaviorID = "UpdateProgressBar" };
            Form.Controls.Add(_updateProgressBar);

            logMonitor = new LogMonitorWeb();
            Form.Controls.Add(logMonitor);
//            Form.Controls.Add(new ServiceProcedureControl());

            Page.Title = ResourceName;
            base.OnInit(e);
            PreRenderComplete += Page_PreRenderComplete;
        }

        protected override void OnPreRender(EventArgs e)
        {
            base.OnPreRender(e);
            //Приходится делать SetCulture, т.к. есть подозрение что Sharepoint в какой то момент устанавливает свою Culture
            ClientScript.RegisterClientScriptResource(typeof(BaseMainPage), "Nat.Web.Controls.GenerationClasses.MainScripts.js");
            LocalizationHelper.SetCulture(Page);
            BaseMainPage.RegisterStartupScript(Page, GetType(), _updateProgressBar.ClientID);
        }

        private void Page_PreRenderComplete(object sender, EventArgs e)
        {
            BaseMainPage.ChangeFormSubmit(Form);
        }

        protected override void InitializeCulture()
        {
            if (string.IsNullOrEmpty(MainPageUrlBuilder.Current.Culture))
                LocalizationHelper.SetCulture(Page);
            else
                LocalizationHelper.SetCulture(MainPageUrlBuilder.Current.Culture, this);
        }

        protected override void OnPreLoad(EventArgs e)
        {
            if (!Page.Request.AppRelativeCurrentExecutionFilePath.Equals(@"~/NoPermit.aspx") &&
                !UserRoles.IsInAnyRoles(UserRoles.ADMIN) && (!ExistUserInPersonalCard() || !CheckPermit()))
                Page.Response.Redirect("/NoPermit.aspx");
            base.OnPreLoad(e);
        }

        public virtual bool CheckPermit()
        {
            return true;
        }

        protected override void CreateChildControls()
        {
            base.CreateChildControls();
            //EnsureUpdatePanelFixups();
        }

        private bool ExistUserInPersonalCard()
        {
            UserRoles.EnsurePersonInfoCorrect();
            return true;
            /*
            string SID = ((WindowsIdentity)Context.User.Identity).User.Value;
            DbCommand dcCheckUser;
            dcCheckUser = WebSpecificInstances.DbFactory.CreateCommand();
            dcCheckUser.CommandText = String.Format(@"SELECT count(*) as Count,r.refPosition
                                                      FROM ULS_RecordCards r
                                                      WHERE (login = '{0}')
                                                      GROUP BY r.refPosition ", SID);
            dcCheckUser.Connection = WebSpecificInstances.DbFactory.CreateConnection();
            dcCheckUser.Connection.Open();
            bool queryResult = true;
            DbDataReader executeResult = null;
            try
            {
                executeResult = dcCheckUser.ExecuteReader();

                if(executeResult != null)
                {
                    if(executeResult.Read() && !executeResult.GetValue(0).Equals(0))
                    {
                            queryResult =
                                executeResult.GetValue(1) != DBNull.Value;
                    }
                    else queryResult = false;
                }
            }
            finally
            {
                if (executeResult != null) executeResult.Close();
                if(dcCheckUser.Connection.State == ConnectionState.Open)
                    dcCheckUser.Connection.Close();
                
            }
            return queryResult;*/
        }

        private void EnsureUpdatePanelFixups()
        {
            if (Page.Form != null)
            {
                string formOnSubmitAtt = Page.Form.Attributes["onsubmit"];
                if (formOnSubmitAtt == "return _spFormOnSubmitWrapper();")
                    Page.Form.Attributes["onsubmit"] = "_spFormOnSubmitWrapper();";
            }
            string script =
                string.Format(
                    "_spOriginalFormAction = document.forms[0].action; _spSuppressFormOnSubmitWrapper=true;document.title='{0}';",
                    Page.Title);
            ScriptManager.RegisterStartupScript(this, GetType(), "UpdatePanelFixup", script, true);
        }

        protected override object LoadPageStateFromPersistenceMedium()
        {
            if(SaveViewStateInSession)
            {
                object obj = base.LoadPageStateFromPersistenceMedium();
                Pair pair = (Pair)obj;
                pageGuid = (Guid)pair.Second;
                Pair viewState = (Pair)Session[pageGuid.ToString()];
                if (viewState == null)
                    Response.Redirect(OnSessionEmptyRedirect);
                PageStatePersister.ControlState = viewState.First;
                PageStatePersister.ViewState = viewState.Second;
                return viewState;
            }
            return base.LoadPageStateFromPersistenceMedium();
        }

        protected virtual string OnSessionEmptyRedirect => "/";

        protected override void SavePageStateToPersistenceMedium(object state)
        {
            if(SaveViewStateInSession)
            {
                Pair pair = (Pair)state;
                Session[PageGuid.ToString()] = pair;
                pair = new Pair(null, PageGuid);
                base.SavePageStateToPersistenceMedium(pair);
            }
            else
                base.SavePageStateToPersistenceMedium(state);
        }


        #endregion

        #region Properties

        public LogMonitorWeb LogMonitor
        {
            get { return logMonitor; }
        }

        public virtual string ResourceName
        {
            get { return string.Empty; }
        }

        protected virtual bool SaveViewStateInSession
        {
            get { return true; }
        }

        public Guid PageGuid
        {
            get
            {
                if (IsPostBack && !pageGuid.HasValue)
                    throw new Exception("ViewState is not been initialized");
                if (!pageGuid.HasValue)
                    pageGuid = Guid.NewGuid();
                return pageGuid.Value;
            }
        }

        public ScriptManager ScriptManager
        {
            get
            {
                if (_ScriptManager == null)
                    _ScriptManager = ScriptManager.GetCurrent(Page);
                return _ScriptManager;
            }
        }

        #endregion
    }
}