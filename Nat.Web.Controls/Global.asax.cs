using System;
using System.Web;
using System.Web.Configuration;
using Nat.Tools.Specific;
using Nat.Web.Tools.Initialization;
using System.Data.SqlClient;
using Nat.Web.Controls.Data;

namespace Nat.Web.Controls
{
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
        }

        protected void Application_EndRequest(object sender, EventArgs e)
        {
            OnCustomEndRequest(sender, e);
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
        }
    }
}
