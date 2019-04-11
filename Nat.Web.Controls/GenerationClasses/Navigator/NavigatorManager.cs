namespace Nat.Web.Controls.GenerationClasses.Navigator
{
    using System;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Reflection;
    using System.Web.Compilation;

    public class NavigatorManager
    {
        public static string GetReadUrlForRecord(
            string projectName,
            string tableName,
            string id,
            string destinationTable,
            string publicKeyToken = "11c252a207597415",
            bool showHistory = false)
        {
            if (string.IsNullOrEmpty(projectName))
                throw new ArgumentNullException("projectName");

            if (string.IsNullOrEmpty(tableName))
                throw new ArgumentNullException("tableName");

            var typeName = string.Format(
                "{0}.{1}NavigatorInfo, {0}, Version=1.4.0.0, Culture=neutral, PublicKeyToken={2}",
                projectName,
                tableName,
                publicKeyToken);
            var type = BuildManager.GetType(typeName, false, true);
            if (type == null && projectName.Contains("."))
            {
                typeName = string.Format(
                    "{0}.{1}NavigatorInfo, {3}, Version=1.4.0.0, Culture=neutral, PublicKeyToken={2}",
                    projectName.Substring(0, projectName.LastIndexOf('.')),
                    tableName,
                    publicKeyToken,
                    projectName);
                type = BuildManager.GetType(typeName, false, true);
            }

            if (type == null && projectName.Contains("_"))
            {
                typeName = string.Format(
                    "{0}.{1}NavigatorInfo, {3}, Version=1.4.0.0, Culture=neutral, PublicKeyToken={2}",
                    projectName.Substring(0, projectName.LastIndexOf('_')),
                    tableName,
                    publicKeyToken,
                    projectName);
                type = BuildManager.GetType(typeName, false, true);
            }

            if (type == null)
                throw new ArgumentOutOfRangeException("tableName");

            var nav = (BaseNavigatorInfo)Activator.CreateInstance(type);
            nav.Initialize();
            var navigateUrl = nav.GetUrlForNavigateTo(id, destinationTable);
            if (!string.IsNullOrEmpty(navigateUrl))
                return navigateUrl;

            var resultUrl = GetReadUrlForRecord(nav, id, destinationTable);
            resultUrl.ShowHistory = showHistory;
            return resultUrl.CreateUrl();
        }

        public static MainPageUrlBuilder GetReadUrlForRecord(BaseNavigatorInfo nav, string id, string destinationTable)
        {
            var url = new MainPageUrlBuilder
                {
                    IsDataControl = true,
                    IsRead = true,
                    UserControl = nav.TableName + MainPageUrlBuilder.UserControlTypeEdit,
                };
            RecurciveParentUrl(string.Empty, url, nav, id, destinationTable);
            return url;
        }

        private static void RecurciveParentUrl(string reference, MainPageUrlBuilder url, BaseNavigatorInfo nav, string id, string destinationTable)
        {
            url.QueryParameters[MainPageUrlBuilder.ReferencIDPrefix + nav.TableName] = id;
            if (!string.IsNullOrEmpty(reference))
                url.QueryParameters[reference + ".id"] = id;

            var parentNav = nav.ParentNavigators.FirstOrDefault(r => r.TableName.Equals(destinationTable, StringComparison.OrdinalIgnoreCase))
                            ?? nav.ParentNavigators.FirstOrDefault(r => !r.HideIfEmpty);

            if (parentNav == null)
                return;

            var methodInfo = typeof(NavigatorManager).GetMethod("RecurciveParentUrlID", BindingFlags.Static | BindingFlags.NonPublic);
            var genericMethod = methodInfo.MakeGenericMethod(nav.DataSourceViewGetName.KeyType, nav.TableType, parentNav.TableType);
            var nextId = (string)genericMethod.Invoke(null, new[] { nav, nav.DataSourceViewGetName.GetKey(id), parentNav });

            if (string.IsNullOrEmpty(nextId))
                return;

            RecurciveParentUrl(
                string.IsNullOrEmpty(reference) ? parentNav.ReferenceName : reference + "." + parentNav.ReferenceName,
                url,
                parentNav,
                nextId,
                destinationTable);
        }

        internal static string RecurciveParentUrlID<TKey, TTableFrom, TTableTo>(BaseNavigatorInfo nav, TKey id, BaseNavigatorInfo parentNav)
            where TKey: struct
            where TTableFrom : class
            where TTableTo : class
        {
            var view = (BaseDataSourceView<TKey>)nav.DataSource;
            var data = view.GetSelectRow(id).Select(GetSelectItemExpression<TTableFrom>(view.RowType)).Select(GetSelectExpression<TTableFrom, TTableTo>(parentNav.ReferenceName));
            if (typeof(long) == parentNav.DataSourceViewGetName.KeyType)
            {
                return data.Select(GetIDExpression<TTableTo>()).FirstOrDefault().ToString();
            }

            throw new NotSupportedException("MultipleKey is not supported");
        }

        private static Expression<Func<BaseRow, TTableFrom>> GetSelectItemExpression<TTableFrom>(Type rowType)
            where TTableFrom : class
        {
            var param = Expression.Parameter(typeof(BaseRow), "r");
            Expression exp = Expression.Convert(param, rowType);
            exp = Expression.Property(exp, "Item");
            return Expression.Lambda<Func<BaseRow, TTableFrom>>(exp, param);
        }

        private static Expression<Func<TTableTo, long?>> GetIDExpression<TTableTo>()
            where TTableTo : class
        {
            var param = Expression.Parameter(typeof(TTableTo), "r");
            var refExp = Expression.Convert(Expression.Property(param, "id"), typeof(long?));
            return Expression.Lambda<Func<TTableTo, long?>>(refExp, param);
        }

        private static Expression<Func<TTableFrom, TTableTo>> GetSelectExpression<TTableFrom, TTableTo>(string reference)
            where TTableFrom : class
            where TTableTo : class
        {
            var param = Expression.Parameter(typeof(TTableFrom), "r");
            var refExp = Expression.Property(param, reference);
            return Expression.Lambda<Func<TTableFrom, TTableTo>>(refExp, param);
        }
    }
}