namespace Nat.Web.Tools.Security
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using System.Web;
    using System.Web.Compilation;

    using Nat.Web.Tools.Initialization;

    public class UserRoles
    {
        private static MethodInfo methodIsInRole;

        /// <summary>
        /// Администратор.
        /// </summary>
        public const string ADMIN = "ADMIN";

        /// <summary>
        /// Право работать от имени другого пользователя.
        /// </summary>
        public const string WorkAsOtherUser = "WorkAsOtherUser";

        /// <summary>
        /// Настройка SID пользователям.
        /// </summary>
        public const string MANAGEMENT_PERSON_SID = "MANAGEMENT_PERSON_SID";

        /// <summary>
        /// Настройка режима обслуживания.
        /// </summary>
        public const string ServiceProcedureConfig = "ServiceProcedureConfig";

        /// <summary>
        /// Разрешить сохранять настройки табличного представления для всех.
        /// </summary>
        public const string AllowSaveJournalSettingsAsShared = "AllowSaveJournalSettingsAsShared";

        /// <summary>
        /// Разрешение изменять и удалять настройки табличного представления созданные другими пользователями.
        /// </summary>
        public const string AllowChangeOrDeleteJournalSettingsAsShared = "AllowChangeOrDeleteJournalSettingsAsShared";

        /// <summary>
        /// Право копировать текст из веб браузера.
        /// </summary>
        public const string AllowCopyTextFromWebBrowser = "AllowCopyTextFromWebBrowser";
        
        public static bool IsInRole(string role)
        {
            if (methodIsInRole != null)
                return (bool) methodIsInRole.Invoke(null, new object[] {role});

            var config = InitializerSection.GetSection();
            if (string.IsNullOrEmpty(config.SecurityRoles))
                return HttpContext.Current != null && HttpContext.Current.User != null
                       && HttpContext.Current.User.Identity.IsAuthenticated
                       && HttpContext.Current.User.IsInRole(role);

            var type = BuildManager.GetType(config.SecurityRoles, true, true);
            methodIsInRole = type.GetMethod("IsInRole");
            return (bool)methodIsInRole.Invoke(null, new object[] { role });
        }

        public static bool IsInAnyRoles(IEnumerable<string> roles)
        {
            return roles.Any(IsInRole);
        }

        public static bool IsInAnyRoles(params string[] roles)
        {
            return IsInAnyRoles((IEnumerable<string>)roles);
        }

        public static bool IsInAllRoles(IEnumerable<string> roles)
        {
            return roles.All(IsInRole);
        }

        public static bool IsInAllRoles(params string[] roles)
        {
            return IsInAnyRoles((IEnumerable<string>)roles);
        }

        public static bool DoesHaveUserPermissionToReport(string pluginName)
        {
            var config = InitializerSection.GetSection();
            var type = BuildManager.GetType(config.ReportAccess, true, true);
            var access = (IReportAccess)Activator.CreateInstance(type);
            return access.DoesHaveUserPermission(pluginName);
        }

        public static void EnsurePersonInfoCorrect()
        {
            var config = InitializerSection.GetSection();
            if (string.IsNullOrEmpty(config.TypeOfMethodEnsurePersonInfoCorrect))
                return;

            var type = BuildManager.GetType(config.TypeOfMethodEnsurePersonInfoCorrect, true, true);
            var mehtod = type.GetMethod("EnsurePersonInfoCorrect");
            mehtod.Invoke(null, new object[0]);
        }

        public static void UserDoesNotHavePermitions(string errorMassage)
        {
            var config = InitializerSection.GetSection();
            HttpContext.Current.Response.Redirect(config.DoesNotHavePermitionsPage + "?ErrorMessage=" + HttpUtility.UrlEncode(errorMassage));
        }

        public static string CheckPersonInfo()
        {
            var config = InitializerSection.GetSection();
            if (string.IsNullOrEmpty(config.TypeOfMethodCheckPersonInfo)) 
                return null;
            var type = BuildManager.GetType(config.TypeOfMethodCheckPersonInfo, true, true);
            var mehtod = type.GetMethod("CheckPersonInfo");
            return (string)mehtod.Invoke(null, new object[0]);
        }
    }
}