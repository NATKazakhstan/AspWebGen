namespace Nat.Web.ReportManager.Modules
{
    using System;
    using System.Collections.Generic;
    using System.Web;

    using Nat.Web.Tools.Security;

    public class SqlReportViewerLogExportModule : IHttpModule
    {
        private const string ControlIDField = "ControlID";

        private const string OpTypeField = "OpType";

        private const string OpTypeExport = "Export";

        private const string SessionList = "SqlReportViewerLogExportModule.PluginList";

        public static void SetPlugin(string controlKey, Type plugin)
        {
            var dic = (Dictionary<string, Type>)HttpContext.Current.Session[SessionList];
            if (dic == null)
                HttpContext.Current.Session[SessionList] = dic = new Dictionary<string, Type>();

            dic[controlKey] = plugin;
        }

        public void Init(HttpApplication context)
        {
            context.PostAcquireRequestState += ContextOnPostAcquireRequestState;
        }

        public void Dispose()
        {
        }

        private void ContextOnPostAcquireRequestState(object sender, EventArgs eventArgs)
        {
            var context = (HttpApplication)sender;

            if (string.IsNullOrEmpty(context.Request.QueryString[ControlIDField])
                || string.IsNullOrEmpty(context.Request.QueryString[OpTypeField])
                || !OpTypeExport.Equals(context.Request.QueryString[OpTypeField], StringComparison.OrdinalIgnoreCase)
                || !context.Request.Path.Equals("/Reserved.ReportViewerWebControl.axd", StringComparison.OrdinalIgnoreCase))
            {
                return;
            }

            var controlID = context.Request.QueryString[ControlIDField];
            var dic = (Dictionary<string, Type>)context.Session[SessionList];
            if (dic == null || !dic.ContainsKey(controlID))
                return;

            var plugin = dic[controlID];

            DBDataContext.AddViewReports(
                User.GetSID(),
                HttpContext.Current.User.Identity.Name,
                HttpContext.Current.User.Identity.Name,
                ReportInitializerSection.GetReportInitializerSection().ReportPageViewer + "?ClassName=" + plugin.FullName,
                HttpContext.Current.Request.Url.GetLeftPart(UriPartial.Authority),
                Environment.MachineName,
                true,
                plugin);
        }
    }
}