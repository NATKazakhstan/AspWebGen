using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Compilation;
using System.Web.Script.Serialization;
using System.Web.Script.Services;
using System.Web.Services;
using System.Web.UI;
using Microsoft.JScript;
using Nat.Web.Controls.GenerationClasses;
using Convert=System.Convert;

namespace Nat.Web.Controls
{
    using Nat.Tools;
    using Nat.Web.Tools;
    using Nat.Web.Tools.Initialization;

    /// <summary>
    /// Summary description for WebServiceAutoComplete
    /// </summary>
    [WebService(Namespace = "http://tempuri.org/")]
    [WebServiceBinding(ConformsTo = WsiProfiles.BasicProfile1_1)]
    [System.ComponentModel.ToolboxItem(false)]
    [ScriptService]
    public class WebServiceAutoComplete : WebService
    {
        [ScriptMethod]
        [WebMethod(EnableSession=true)]
        public string[] GetCompletionList(string prefixText, int count, string contextKey)
        {
            string[] split = contextKey.Split(',');
            if (split.Length < 4)
            {
                //todo: log
                return null;
            }

            bool isKz = string.IsNullOrEmpty(split[0]) ? false : Convert.ToBoolean(split[0]);
            bool isCode = string.IsNullOrEmpty(split[1]) ? false : Convert.ToBoolean(split[1]);
            var sourceObj = Activator.CreateInstance(BuildManager.GetType(split[2], true, true), null);
            string value = split[3];

            //todo: переключение языка потока
            var dataSourse2 = sourceObj as IDataSourceView2;
            if (dataSourse2 != null)
                return GetData(prefixText, count, isKz, isCode, dataSourse2, sourceObj as IDataSourceView3, value);

            var dataSourse1 = (IDataSourceView)sourceObj;
            return GetData(prefixText, count, isKz, isCode, dataSourse1, value);
        }

        [ScriptMethod(UseHttpGet = true, ResponseFormat = ResponseFormat.Json)]
        [WebMethod(EnableSession = true)]
        public string[] GetCompletionList2(string prefixText, int count, string contextKey)
        {
            return GetCompletionList(prefixText, count, contextKey);
        }

        [ScriptMethod(UseHttpGet = true, ResponseFormat = ResponseFormat.Json)]
        [WebMethod(EnableSession = true)]
        public ResultRows GetSourceData(int startIndex, int count, string dataSource, bool isKz, string parameters, string filter, string sid)
        {
            var source = (IDataSourceView)Activator.CreateInstance(BuildManager.GetType(dataSource, true, true), null);
            if (!source.CheckPermit())
                return new ResultRows { Total = 0, Data = new BaseRow[0] };

            LocalizationHelper.SetThreadCulture(isKz ? "kk-kz" : "ru-ru", null);
            if (!string.IsNullOrEmpty(sid))
                Tools.Security.User.SetSID(sid);

            var selectParameters = parameters;
            if (!string.IsNullOrEmpty(filter))
            {
                var filters = new List<MainPageUrlBuilder.FilterParameter>
                    {
                        new MainPageUrlBuilder.FilterParameter
                            {
                                Value = filter,
                                Key = source.TableName,
                            }
                    };

                var jss = new JavaScriptSerializer();
                var serializedFilter = "__filters=" + GlobalObject.encodeURIComponent(jss.Serialize(filters));
                selectParameters = string.IsNullOrEmpty(selectParameters) ? serializedFilter : "&" + serializedFilter;
            }

            var query = source.GetSelectIRow(selectParameters);
            var total = query.Count();
            var result = query.Skip(startIndex).Take(count).Cast<BaseRow>().ToArray();

            if (source.LogViewData != null)
            {
                var logMonitor = InitializerSection.GetSection().LogMonitor;
                logMonitor.Init();
                logMonitor.Log(source.LogViewData.Value,
                    delegate
                        {
                            const string Message = "Выполнен запрос данных через сервис WebServiceAutoComplete.GetSourceData";
                            logMonitor.FieldChanged(string.Empty, "startIndex", string.Empty, startIndex);
                            logMonitor.FieldChanged(string.Empty, "count", string.Empty, count);
                            logMonitor.FieldChanged(string.Empty, "dataSource", string.Empty, dataSource);
                            logMonitor.FieldChanged(string.Empty, "isKz", string.Empty, isKz.ToString());
                            logMonitor.FieldChanged(string.Empty, "parameters", string.Empty, parameters);
                            logMonitor.FieldChanged(string.Empty, "filter", string.Empty, filter);
                            if (!string.IsNullOrEmpty(sid))
                            {
                                var fio = Tools.Security.User.GetPersonInfo(sid);
                                if (fio != null)
                                    sid += " - " + fio.Fio_Ru;
                                
                                var originalSid = Tools.Security.User.GetSID(false);
                                fio = Tools.Security.User.GetPersonInfo(originalSid);
                                if (fio != null)
                                    originalSid += " - " + fio.Fio_Ru;
                             
                                logMonitor.FieldChanged(string.Empty, "sid", originalSid, sid);
                            }

                            logMonitor.FieldChanged(
                                string.Empty,
                                "Предоставлены записи",
                                string.Empty,
                                string.Join("; ", result.Select(r => r.id + ": " + r.nameRu).ToArray()));
                            return new LogMessageEntry
                                {
                                    Message = Message,
                                    MessageCodeAsLong = source.LogViewData.Value,
                                };
                        });
            }

            return new ResultRows
                {
                    Data = result,
                    Total = total,
                };
        }

        private static string[] GetData(string prefixText, int count, bool isKz, bool isCode, IDataSourceView dataSourse, string value)
        {
            if (!dataSourse.CheckPermit())
            {
                //todo: log
                return null;
            }

            var reslut = new List<string>(count);
            var jss = new JavaScriptSerializer();
            if (dataSourse.SupportSelectICodeRow)
            {
                var queryable = dataSourse.GetSelectICodeRow(GlobalObject.decodeURIComponent(value));
                if (isKz)
                {
                    queryable = isCode
                                    ? queryable.Where(q => q.code.StartsWith(prefixText)).Take(count)
                                    : queryable.Where(q => q.nameKz.StartsWith(prefixText)).Take(count);
                    foreach (var row in queryable)
                        reslut.Add(jss.Serialize(new Pair(isCode ? row.code : row.nameKz, new Pair(isCode ? row.nameKz : row.code, row.id))));
                }
                else
                {
                    queryable = isCode
                                    ? queryable.Where(q => q.code.StartsWith(prefixText)).Take(count)
                                    : queryable.Where(q => q.nameRu.StartsWith(prefixText)).Take(count);
                    foreach (var row in queryable)
                        reslut.Add(jss.Serialize(new Pair(isCode ? row.code : row.nameRu, new Pair(isCode ? row.nameRu : row.code, row.id))));
                }
            }
            else
            {
                var queryable = dataSourse.GetSelectIRow(GlobalObject.decodeURIComponent(value));
                if (isKz)
                {
                    queryable = queryable.Where(q => q.nameKz.StartsWith(prefixText)).Take(count);
                    foreach (var row in queryable)
                        reslut.Add(jss.Serialize(new Pair(row.nameKz, new Pair("", row.id))));
                }
                else
                {
                    queryable = queryable.Where(q => q.nameRu.StartsWith(prefixText)).Take(count);
                    foreach (var row in queryable)
                        reslut.Add(jss.Serialize(new Pair(row.nameRu, new Pair("", row.id))));
                }
            }

            return reslut.ToArray();
        }

        private static string[] GetData(string prefixText, int count, bool isKz, bool isCode, IDataSourceView2 dataSourse, IDataSourceView3 ds3, string value)
        {
            if (!dataSourse.CheckPermit())
            {
                //todo: log
                return null;
            }

            MainPageUrlBuilder.Current.IsDataControl = true;
            var reslut = new List<string>(count);
            var jss = new JavaScriptSerializer();
            if (dataSourse.SupportSelectICodeRow)
            {
                var queryable = dataSourse.
                    GetSelectICodeRow(GlobalObject.decodeURIComponent(value));
                if (ds3 != null && ds3.SupportFlagCanAddChild)
                    queryable = queryable.Where(q => q.CanAddChild);

                if (isKz)
                {
                    queryable = isCode
                                    ? queryable.Where(q => q.code.StartsWith(prefixText)).OrderBy(q => q.code).Take(count)
                                    : queryable.Where(q => q.nameKz.StartsWith(prefixText)).Take(count);
                    foreach (var row in queryable)
                        reslut.Add(jss.Serialize(new Pair(isCode ? row.code : row.nameKz, new Triplet(isCode ? row.nameKz : row.code, row.Value, ds3 != null ? row.GetAdditionalValues(ds3.SelectParameters) : null))));
                }
                else
                {
                    queryable = isCode
                                    ? queryable.Where(q => q.code.StartsWith(prefixText)).OrderBy(q => q.code).Take(count)
                                    : queryable.Where(q => q.nameRu.StartsWith(prefixText)).Take(count);
                    foreach (var row in queryable)
                        reslut.Add(jss.Serialize(new Pair(isCode ? row.code : row.nameRu, new Triplet(isCode ? row.nameRu : row.code, row.Value, ds3 != null ? row.GetAdditionalValues(ds3.SelectParameters) : null))));
                }
            }
            else
            {
                var queryable = dataSourse.
                    GetSelectIRow(GlobalObject.decodeURIComponent(value));
                if (ds3 != null && ds3.SupportFlagCanAddChild)
                    queryable = queryable.Where(q => q.CanAddChild);
                if (isKz)
                {
                    queryable = queryable.Where(q => q.nameKz.StartsWith(prefixText)).Take(count);
                    foreach (var row in queryable)
                        reslut.Add(jss.Serialize(new Pair(row.nameKz, new Triplet("", row.Value, ds3 != null ? row.GetAdditionalValues(ds3.SelectParameters) : null))));
                }
                else
                {
                    queryable = queryable.Where(q => q.nameRu.StartsWith(prefixText)).Take(count);
                    foreach (var row in queryable)
                        reslut.Add(jss.Serialize(new Pair(row.nameRu, new Triplet("", row.Value, ds3 != null ? row.GetAdditionalValues(ds3.SelectParameters) : null))));
                }
            }

            return reslut.ToArray();
        }

        public class ResultRows
        {
            public int Total;
            public BaseRow[] Data;
        }
    }
}
