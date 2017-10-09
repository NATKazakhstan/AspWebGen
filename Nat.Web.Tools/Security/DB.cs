namespace Nat.Web.Tools.Security
{
    using System;
    using System.Globalization;
    using System.Threading;

    using Nat.ReportManager.QueryGeneration;
    using Nat.Tools.Specific;

    partial class DBDataContext
    {
        public static void AddViewReports(string sid, string loginName, string name, string docLocation, string siteUrl, string hostName, bool export, Type plugin)
        {
            var oldCulture = Thread.CurrentThread.CurrentUICulture;
            Thread.CurrentThread.CurrentUICulture = new CultureInfo("ru-ru");
            var pluginRu = (IReportPlugin)Activator.CreateInstance(plugin);
            var titleRu = pluginRu.Description;
            Thread.CurrentThread.CurrentUICulture = new CultureInfo("kk-kz");
            var pluginKz = (IReportPlugin)Activator.CreateInstance(plugin);
            var titleKz = pluginKz.Description;
            Thread.CurrentThread.CurrentUICulture = oldCulture;

            AddViewReports(
                sid,
                loginName,
                name,
                docLocation,
                siteUrl,
                hostName,
                export,
                plugin.FullName,
                titleRu,
                titleKz);
        }

        public static void AddViewReports(
            string sid,
            string loginName,
            string name,
            string docLocation,
            string siteUrl,
            string hostName,
            bool export,
            string plugin,
            string titleRu,
            string titleKz)
        {
            using (var db = new DBDataContext(SpecificInstances.DbFactory.CreateConnection()))
            {
                if (siteUrl.EndsWith("/") || siteUrl.EndsWith("\\"))
                    siteUrl = siteUrl.Substring(0, siteUrl.Length - 1);

                if (docLocation.StartsWith(siteUrl))
                    docLocation = docLocation.Substring(siteUrl.Length);

                if (docLocation.StartsWith("/") || docLocation.StartsWith("\\"))
                    docLocation = docLocation.Substring(1);

                if (plugin.StartsWith("/") || plugin.StartsWith("\\"))
                    plugin = plugin.Substring(1);

                db.MSD_P_AddViewReports(
                    sid,
                    loginName,
                    name,
                    DateTime.UtcNow,
                    docLocation,
                    siteUrl,
                    hostName,
                    new Guid("{B85A2C1A-E75F-47CD-84DA-CA3295CC7BFB}"),
                    export,
                    plugin,
                    titleRu,
                    titleKz);
            }
        }
    }
}