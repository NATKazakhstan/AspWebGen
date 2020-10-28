using System;
using System.Web;
using System.Web.Configuration;
using Nat.Tools.Specific;
using Nat.Web.Tools.Initialization;
using System.Data.SqlClient;
using Nat.Web.Controls.Data;

namespace Nat.Web.Controls
{
    using System.Collections.Generic;

    using Nat.Web.Controls.Trace;

#if LOCAL || !ForSharepoint
    public class Global : HttpApplication
#else
    public class Global : Microsoft.SharePoint.ApplicationRuntime.SPHttpApplication
#endif
    {
        public event EventHandler CustomEndRequest;

        protected virtual void Initialize()
        {
            WebInitializer.Initialize();
        }

        protected virtual void OnApplication_Start()
        {
        }

        protected virtual void OnApplication_AuthenticateRequest()
        {
        }

        protected void Application_Start(object sender, EventArgs e)
        {
            Initialize();
            OnApplication_Start();
        }
        
        protected void Application_BeginRequest(object sender, EventArgs e)
        {
            if (Context != null && Context.Request.Url.AbsolutePath.Contains(".aspx"))
            {
                var trace = WebConfigurationManager.AppSettings["NatTraceTimingReqeustsEnableOnBeginRequest"];
                if (!string.IsNullOrEmpty(trace))
                    Context.Trace.IsEnabled = true;
            }

            OnBeginRequest(sender, e);
        }

        protected virtual void OnBeginRequest(object sender, EventArgs e)
        {
        }

        protected virtual void OnApplication_PreRequestHandlerExecute()
        {
        }

        protected void Application_PreRequestHandlerExecute(object sender, EventArgs e)
        {
            try
            {
                bool enabled = WebConfigurationManager.AppSettings["ADM.SetUserActivityTime"] != null
                               && Convert.ToBoolean(WebConfigurationManager.AppSettings["ADM.SetUserActivityTime"]);
                if (enabled && User.Identity.IsAuthenticated && Context.Session != null && !Session.IsReadOnly)
                {
                    Session["Global.LastActivityDateTime"] = DateTime.Now;
                    Session["Global.LastActivitySID"] = Tools.Security.User.GetSID(false);
                    var lastTime = (DateTime?)Session["Global.UserActivityTime"];
                    if (lastTime == null || lastTime.Value.AddMinutes(1) < DateTime.Now)
                    {
                        Session["Global.UserActivityTime"] = DateTime.Now;
                        SetUserActivityTime();
                    }
                }
            }
            catch (HttpException httpException)
            {
            }

            OnApplication_PreRequestHandlerExecute();
        }

        protected void Application_EndRequest(object sender, EventArgs e)
        {
            OnCustomEndRequest(sender, e);
            OnEndRequest(sender, e);
        }

        protected virtual void OnEndRequest(object sender, EventArgs e)
        {
        }

        private void OnCustomEndRequest(object sender, EventArgs e)
        {
            if (CustomEndRequest != null)
            {
                CustomEndRequest(sender, e);
                CustomEndRequest = null;
            }
        }

        protected void Application_AuthenticateRequest(object sender, EventArgs e)
        {
            OnApplication_AuthenticateRequest();
        }

        protected virtual void OnApplication_Error()
        {
        }

        protected void Application_Error(object sender, EventArgs e)
        {
            OnCustomEndRequest(sender, e);
            try
            {
                if (Context != null && Context.AllErrors != null)
                {
                    WebInitializer.Initialize();
                    var monitor = new LogMonitor();
                    monitor.Init();
                    var str2 = Tools.Security.User.GetSID();
                    var db = new DBFilterValuesDataContext(SpecificInstances.DbFactory.CreateConnection());
                    
                    // присвоение выбранного фильтра, для его пометки, как опасного
                    long? refUserFilter = null;
                    if (MainPageUrlBuilder.Current.IsDataControl
                        && MainPageUrlBuilder.Current.UserControl != null
                        && MainPageUrlBuilder.Current.UserControl.EndsWith("Journal"))
                    {
                        var tableName = MainPageUrlBuilder.Current.UserControl.Substring(0, MainPageUrlBuilder.Current.UserControl.Length - 7);
                        var filterValues = MainPageUrlBuilder.Current.GetFilterItemsDic(tableName);
                        if (filterValues != null && filterValues.ContainsKey("__refUserFilterValues")
                            && filterValues["__refUserFilterValues"].Count > 0
                            && !string.IsNullOrEmpty(filterValues["__refUserFilterValues"][0].Value1))
                        {
                            refUserFilter = Convert.ToInt64(filterValues["__refUserFilterValues"][0].Value1);
                        }
                    }

                    foreach (Exception exception in Context.AllErrors)
                    {
                        var message = string.Format("{0}: {1}", Context.Request.Url.PathAndQuery, exception);
                        var entry2 = new LogMessageEntry(LogMessageType.SystemErrorInApp, message) { Sid = str2 };
                        var logMessageEntry = entry2;
                        monitor.Log(logMessageEntry);
                        var sqlException = exception as SqlException ?? exception.InnerException as SqlException;
                        if (sqlException != null && refUserFilter != null)
                        {
                            foreach (SqlError sqlError in sqlException.Errors)
                                if (sqlError.Number == -2)
                                {
                                    db.SYS_SetIsDangerousUserFilter(refUserFilter);
                                    break;
                                }
                        }
                    }
                }
            }
            catch (Exception)
            {
            }
            OnApplication_Error();
        }

        protected void Session_OnEnd(object sender, EventArgs e)
        {
            SetUserActivityTime();
        }

        private void SetUserActivityTime()
        {
            if (!string.IsNullOrEmpty((string)Session["Global.LastActivitySID"]))
                Tools.Security.User.SetUserActivityTime((string)Session["Global.LastActivitySID"], (DateTime)Session["Global.LastActivityDateTime"]);
        }
    }
}
