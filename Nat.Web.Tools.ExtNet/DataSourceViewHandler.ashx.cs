namespace Nat.Web.Tools.ExtNet
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Web;
    using System.Web.Compilation;
    using System.Web.SessionState;

    using Ext.Net;

    using Microsoft.JScript;

    using Nat.Web.Controls;
    using Nat.Web.Controls.GenerationClasses;
    using Nat.Web.Controls.GenerationClasses.BaseJournal;
    using Nat.Web.Tools.ExtNet.Data;
    using Nat.Web.Tools.Initialization;

    using Convert = System.Convert;

    public class DataSourceViewHandler : IHttpHandler, IReadOnlySessionState
    {
        private const string Limit = "limit";
        private const string Start = "start";
        private const string DataSourceType = "dataSourceType";

        #region IHttpHandler Members

        public bool IsReusable
        {
            get { return true; }
        }

        public void ProcessRequest(HttpContext context)
        {
            WebInitializer.Initialize();
            context.Response.ContentType = "text/json";
            int total;
            var data = GetData(context, out total);
            context.Response.Write(string.Format("{{Total:{1},'Data':{0}}}", JSON.Serialize(data), total));
        }

        #endregion

        private static IEnumerable<IDataRow> GetData(HttpContext context, out int total)
        {
            var start = 0;
            var limit = 0;
            var isKz = false;
            var showHistory = false;
            var dataSourceType = string.Empty;
            var parameters = string.Empty;
            var refParent = string.Empty;
            var search = string.Empty;

            if (!string.IsNullOrEmpty(context.Request[Start]))
                start = Convert.ToInt32(context.Request[Start]);

            if (!string.IsNullOrEmpty(context.Request[Limit]))
                limit = Convert.ToInt32(context.Request[Limit]);

            if (!string.IsNullOrEmpty(context.Request[DataSourceType]))
                dataSourceType = context.Request[DataSourceType];

            if (!string.IsNullOrEmpty(context.Request["isKz"]))
                isKz = Convert.ToBoolean(context.Request["isKz"]);

            if (!string.IsNullOrEmpty(context.Request["showHistory"]))
                showHistory = Convert.ToBoolean(context.Request["showHistory"]);

            if (!string.IsNullOrEmpty(context.Request["parameters"]))
                parameters = context.Request["parameters"];

            if (!string.IsNullOrEmpty(context.Request["node"]))
                refParent = context.Request["node"];

            if (!string.IsNullOrEmpty(context.Request["search"]))
                search = context.Request["search"];

            if (isKz)
                LocalizationHelper.SetThreadCulture("kk-kz", null);

            var sourceObj = Activator.CreateInstance(BuildManager.GetType(dataSourceType, true, true), null);

            return GetData(start, limit, (IDataSourceViewExtNet)sourceObj, parameters, refParent, search, showHistory, out total);
        }

        private static IEnumerable<IDataRow> GetData(
            int start,
            int limit,
            IDataSourceViewExtNet dataSource,
            string parameters,
            string refParent,
            string search,
            bool showHistory,
            out int total)
        {
            total = 0;

            if (!dataSource.CheckPermit())
            {
                // todo: log
                return null;
            }

            MainPageUrlBuilder.Current.IsDataControl = true;
            IQueryable<IDataRow> queryable;
            var queryParameters = GlobalObject.decodeURIComponent(parameters);
            if (showHistory)
                queryParameters = "/showhistory?" + queryParameters;
            if (string.IsNullOrEmpty(queryParameters) || queryParameters[queryParameters.Length - 1] == '?')
                queryParameters += BaseFilterParameterSearch<object>.SearchQueryParameter + "=" + search;
            else
                queryParameters += "&" + BaseFilterParameterSearch<object>.SearchQueryParameter + "=" + search;

            if (string.IsNullOrEmpty(refParent))
                queryable = dataSource.GetFullModelData(queryParameters);
            else
            {
                var refParentValue = "NaN".Equals(refParent) || "Root".Equals(refParent)
                                         ? null
                                         : refParent;
                queryable = dataSource.GetFullModelData(queryParameters, refParentValue);
            }

            total = queryable.Count();

            if (limit > 0)
                queryable = queryable.Skip(start).Take(limit);

            return queryable.ToList();
        }
    }
}
