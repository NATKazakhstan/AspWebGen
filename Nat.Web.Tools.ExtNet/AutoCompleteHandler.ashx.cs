/*
 * Created by: Eugene P. Kolesnikov
 * Created: 2012.10.02
 * Copyright © JSC NAT Kazakhstan 2012
 */

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
    using Nat.Web.Tools.ExtNet.Extenders;
    using Nat.Web.Tools.ExtNet.ExtNetConfig;
    using Nat.Web.Tools.Initialization;

    using Convert = System.Convert;

    /// <summary>
    /// Summary description for AutoCompleteHandler
    /// </summary>
    public class AutoCompleteHandler : IHttpHandler, IReadOnlySessionState
    {
        public const string PrefixText = "prefixText";
        public const string Limit = "limit";
        public const string Start = "start";
        public const string DataSourceType = "dataSourceType";
        public const string Parameters = "parameters";
        public const string ComboBoxView = "comboBoxView";

        public bool IsReusable
        {
            get
            {
                return true;
            }
        }

        public void ProcessRequest(HttpContext context)
        {
            WebInitializer.Initialize();
            context.Response.ContentType = "text/json";
            IDataSourceView3 ds3;
            int countRows;
            var data = GetCompletionList(context, out ds3, out countRows);
            
            if (data == null) return;

            var dataRows = data.ToList()
                .Select(
                    r => new
                        {
                            id = r.Value,
                            RowName = r.Name,
                            Row = r,
                            AdditionalValues = JSON.Serialize(r.GetAdditionalValues(ds3.SelectParameters)),
                        })
                .ToList();

            var store = new Store();
            store.Model.Add(new Model());
            store.Model.Primary.Fields.Add(new ModelField("id", ModelFieldType.String) { ServerMapping = "id" });
            store.Model.Primary.Fields.Add(new ModelField("RowName", ModelFieldType.String) { ServerMapping = "RowName" });
            store.Model.Primary.Fields.Add(new ModelField("AdditionalValues", ModelFieldType.String) { ServerMapping = "AdditionalValues" });

            var comboBoxView = GetComboBoxView(context);
            if (comboBoxView != null)
            {
                comboBoxView.ServerMappingPerfix = "Row.";
                store.InitializeListConfig(comboBoxView);
            }

            store.DataSource = dataRows;
            store.DataBind();
            context.Response.Write(string.Format("{{Total:{1},'Data':{0}}}", store.JsonData, countRows));
        }

        private IListConfig GetComboBoxView(HttpContext context)
        {
            if (string.IsNullOrEmpty(context.Request[ComboBoxView]))
                return null;
            var typeStr = context.Request[ComboBoxView];
            var obj = Activator.CreateInstance(BuildManager.GetType(typeStr, true, true), null);
            return (IListConfig)obj;
        }

        private static IEnumerable<IDataRow> GetCompletionList(HttpContext context, out IDataSourceView3 ds3, out int countRows)
        {
            var start = 0;
            var limit = 10;
            var prefixTextValue = string.Empty;
            var isKz = false;
            var dataSourceType = string.Empty;
            var parameters = string.Empty;
            var refParent = string.Empty;
            var fullModelData = false;

            if (!string.IsNullOrEmpty(context.Request[Start]))
                start = Convert.ToInt32(context.Request[Start]);

            if (!string.IsNullOrEmpty(context.Request[Limit]))
                limit = Convert.ToInt32(context.Request[Limit]);

            if (!string.IsNullOrEmpty(context.Request[PrefixText]))
                prefixTextValue = context.Request[PrefixText];

            if (!string.IsNullOrEmpty(context.Request[DataSourceType]))
                dataSourceType = context.Request[DataSourceType];

            if (!string.IsNullOrEmpty(context.Request["isKz"]))
                isKz = Convert.ToBoolean(context.Request["isKz"]);

            if (!string.IsNullOrEmpty(context.Request["parameters"]))
                parameters = context.Request["parameters"];

            if (!string.IsNullOrEmpty(context.Request["node"]))
                refParent = context.Request["node"];

            var sourceObj = Activator.CreateInstance(BuildManager.GetType(dataSourceType, true, true), null);
            ds3 = (IDataSourceView3)sourceObj;
            return GetData(prefixTextValue, start, limit, isKz, (IDataSourceView2)sourceObj, ds3, parameters, refParent, out countRows);
        }

        private static IEnumerable<IDataRow> GetData(
            string prefixText,
            int start,
            int limit,
            bool isKz,
            IDataSourceView2 dataSourse,
            IDataSourceView3 ds3,
            string parameters,
            string refParent,
            out int countRows)
        {
            if (!dataSourse.CheckPermit())
            {
                // todo: log
                countRows = 0;
                return null;
            }

            if (isKz)
                LocalizationHelper.SetThreadCulture("kk-kz", null);

            MainPageUrlBuilder.Current.IsDataControl = true;
           
            IQueryable<IDataRow> queryable;
            if (string.IsNullOrEmpty(refParent))
                queryable = dataSourse.GetSelectIRow(GlobalObject.decodeURIComponent(parameters));
            else
            {
                var queryParameters = GlobalObject.decodeURIComponent(parameters)
                                      + "&refParent=" + ("NaN".Equals(refParent) ? null : refParent);
                queryable = dataSourse.GetSelectIRow(queryParameters);
            }

            if (ds3 != null && ds3.SupportFlagCanAddChild)
                queryable = queryable.Where(q => q.CanAddChild);

            queryable = isKz
                            ? queryable.Where(q => q.nameKz.Contains(prefixText))
                            : queryable.Where(q => q.nameRu.Contains(prefixText));
            

            if (!parameters.Contains("RemoveDefaultSortOnAutocompleetRequset"))
                queryable = isKz ? queryable.OrderBy(q => q.nameKz) : queryable.OrderBy(q => q.nameRu);

            countRows = queryable.Count();
            return queryable.Skip(start).Take(limit);
        }
    }
}