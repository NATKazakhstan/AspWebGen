using System.Collections;
using System.ComponentModel;

namespace Nat.Web.Tools.Security
{
    using System;
    using System.Linq;
    using System.Security.Principal;
    using System.Threading;
    using System.Web;
    using System.Web.Configuration;
    using System.Web.Security;

    using Nat.Tools.Specific;
    using Initialization;

    using System.Web.Compilation;
    using System.Runtime.InteropServices;
    using System.Net;
    using System.Web.Caching;

    public class User
    {
        [DllImport("iphlpapi.dll", ExactSpelling = true)]
        public static extern int SendARP(int DestIP, int SrcIP, [Out] byte[] pMacAddr, ref int PhyAddrLen);

        private const string PersonInfoBySid = "Nat.Web.Tools.Security.User.PersonInfoBySid<";

        public static void SetSID(string sid)
        {
            var section = (RoleManagerSection)WebConfigurationManager.GetSection("system.web/roleManager");
            if (section.Providers.Count == 0) 
                return;

            var sectionRoleManager = section.Providers[section.DefaultProvider];
            if (sectionRoleManager == null) 
                return;

            var roleManagerType = BuildManager.GetType(sectionRoleManager.Type, true, false);
            var roleProvider = (RoleProvider)Activator.CreateInstance(roleManagerType);
            if (roleProvider.IsUserInRole(GetSID(false), UserRoles.WorkAsOtherUser))
            {
                try
                {
                    var base64 = Convert.FromBase64String(sid);
                    var sidValue = new SecurityIdentifier(base64, 0);
                    HttpContext.Current.Items["Nat.Web.Tools.Security.UserSID"] = sidValue.Value;
                }
                catch (FormatException)
                {
                    HttpContext.Current.Items["Nat.Web.Tools.Security.UserSID"] = sid;
                }
            }
        }

        public static string GetSID()
        {
            return GetSID(true);
        }

        public static string GetDelegationSID()
        {
            if (!InitializerSection.PermissionsDelegationEnabled || HttpContext.Current == null || HttpContext.Current.User == null)
                return null;

            var cookie = HttpContext.Current.Request.Cookies["duser"];
            if (cookie == null)
                return null;

            if (!string.IsNullOrEmpty(cookie.Value) && int.TryParse(cookie.Value, out var userId))
            {
                var available = GetAvailableDelegations().FirstOrDefault(r => r.id == userId);
                return available?.Sid;
            }

            return null;
        }

        public static long? GetDelegationID()
        {
            if (!InitializerSection.PermissionsDelegationEnabled || HttpContext.Current == null || HttpContext.Current.User == null)
                return null;

            var cookie = HttpContext.Current.Request.Cookies["duser"];
            if (cookie == null)
                return null;

            if (!string.IsNullOrEmpty(cookie.Value) && int.TryParse(cookie.Value, out var userId))
            {
                var available = GetAvailableDelegations().FirstOrDefault(r => r.id == userId);
                return available?.refPermissionDelegation;
            }

            return null;
        }

        internal static string GetSID(bool checkWorkAsOtherUser)
        {
            if (checkWorkAsOtherUser && HttpContext.Current != null && HttpContext.Current.Items["Nat.Web.Tools.Security.UserSIDCheckRoles"] == null)
            {
                HttpContext.Current.Items["Nat.Web.Tools.Security.UserSIDCheckRoles"] = true;
                try
                {
                    if (UserRoles.IsInRole(UserRoles.WorkAsOtherUser))
                    {
                        var userSid = (string)HttpContext.Current.Items["Nat.Web.Tools.Security.UserSID"];
                        if (!string.IsNullOrEmpty(userSid))
                            return userSid;
                    }
                }
                finally
                {
                    HttpContext.Current.Items.Remove("Nat.Web.Tools.Security.UserSIDCheckRoles");
                }
            }

            string userName;

            if (HttpContext.Current == null)
            {
                userName = GetUserName(Thread.CurrentPrincipal.Identity);
                if (!string.IsNullOrEmpty(userName)) return userName;

                var name = Thread.CurrentPrincipal.Identity.Name;
                if (string.IsNullOrEmpty(name))
                    return "anonymous";
                return name;
            }

            if (HttpContext.Current.User == null)
                return "anonymous";
                
            userName = GetUserName(HttpContext.Current.User.Identity);
            if (!string.IsNullOrEmpty(userName)) return userName;

            var sid = HttpContext.Current.User.Identity.Name;

            if (string.IsNullOrEmpty(sid)) return "anonymous";
            return sid;
        }

        private static string GetUserName(IIdentity identity)
        {
            var userName = GetByClaimsIdentity(identity) ?? (identity.AuthenticationType == "Forms" ? GetUserNameByFormIdentity(identity) : GetUserNameByWindowsIdentity(identity));

            return !string.IsNullOrEmpty(userName) ? userName : string.Empty;
        }

        private static string GetByClaimsIdentity(IIdentity identity)
        {
            var type = identity.GetType();
            if (!type.Name.Equals("ClaimsIdentity"))
                return null;

            var data = (IEnumerable) TypeDescriptor.GetProperties(type).Find("Claims", false).GetValue(identity);
            if (data == null)
                return null;

            Func<object, object> getType = null;
            foreach (var claim in data)
            {
                if (getType == null)
                    getType = TypeDescriptor.GetProperties(claim).Find("Type", false).GetValue;
                if ((string)getType.Invoke(claim) == "LoginName")
                    return (string)TypeDescriptor.GetProperties(claim).Find("Value", false).GetValue(claim);
            }

            return null;
        }

        private static string GetUserNameByFormIdentity(IIdentity identity)
        {
            var formsIdentity = identity as FormsIdentity;
            if (formsIdentity != null && !string.IsNullOrEmpty(formsIdentity.Name)) 
                return formsIdentity.Name;
            return string.Empty;
        }

        private static string GetUserNameByWindowsIdentity(IIdentity identity)
        {
            var windowsIdentity = identity as WindowsIdentity;
            if (windowsIdentity != null && windowsIdentity.User != null)
                return windowsIdentity.User.Value;
            return string.Empty;
        }
        
        public static string[] GetGroups()
        {
            var config = InitializerSection.GetSection();
            var groups = new string[] { };

            if (!string.IsNullOrEmpty(config.GroupProviderType))
            {
                var type = BuildManager.GetType(config.GroupProviderType, true, true);

                var simpleGroupProvider = (IGroupProvider)Activator.CreateInstance(type);
                var userName = GetSID();
                var formGroups = simpleGroupProvider.GetGroupsForUser(userName);
                return formGroups.ToArray();
            }

            var windowsIdentity = HttpContext.Current.User.Identity as WindowsIdentity;
            if (windowsIdentity != null)
            {
                var windowsGroups = windowsIdentity.Groups;

                if (windowsGroups == null) return new string[0];
                groups = windowsGroups.Select(r => r.Value).ToArray();
            }

            return groups;
        }

        public static string GetSubdivisionKSP()
        {
            var config = InitializerSection.GetSection();
            var typeName = config.TypeOfMethodGetSubdivisionKSP;
            if (string.IsNullOrEmpty(typeName))
                return string.Empty;

            var type = BuildManager.GetType(typeName, true, true);
            var mehtod = type.GetMethod("GetSubdivisionKSP", new Type[0]);
            return (string)mehtod.Invoke(null, new object[0]);
        }

        public static GetPersonInfoBySidResult GetPersonInfoRequired()
        {
            var value = GetPersonInfo();
            if (value == null)
            {
                if (AccessDeniedException.IsInService())
                    throw new AccessDeniedException("User.GetPersonInfoRequired() is null");
                HttpContext.Current.Response.Redirect("/MainPage.aspx/data/NoPermit");
			}

            return value;
        }

        public static TResult GetPersonInfoRequired<TResult>()
        {
            var value = GetPersonInfo<TResult>(GetSID());
            if (value == null)
            {
                if (AccessDeniedException.IsInService())
                    throw new AccessDeniedException("User.GetPersonInfoRequired() is null");
                HttpContext.Current.Response.Redirect("/MainPage.aspx/data/NoPermit");
			}

            return value;
        }

        public static GetPersonInfoBySidResult GetPersonInfo()
        {
            return GetPersonInfo(GetSID());
        }

        public static GetPersonInfoBySidResult GetPersonInfoOnUseDelegation()
        {
            var sid = GetDelegationSID();
            if (string.IsNullOrEmpty(sid))
                return null;
            return GetPersonInfo(sid);
        }

        public static TResult GetPersonInfoOnUseDelegation<TResult>()
        {
            var sid = GetDelegationSID();
            if (string.IsNullOrEmpty(sid))
                return default(TResult);
            return GetPersonInfo<TResult>(sid);
        }

        public static ADM_P_GetAvailableDelegationsResult[] GetAvailableDelegations(bool extended = false)
        {
            if (!InitializerSection.PermissionsDelegationEnabled || HttpContext.Current == null || HttpContext.Current.User == null)
                return new ADM_P_GetAvailableDelegationsResult[0];

            if (HttpContext.Current.Items["availableDelegation.extended"] != null)
                return (ADM_P_GetAvailableDelegationsResult[])HttpContext.Current.Items["availableDelegation.extended"];

            if (!extended && HttpContext.Current.Items["availableDelegation"] != null)
                return (ADM_P_GetAvailableDelegationsResult[])HttpContext.Current.Items["availableDelegation"];

            using (var db = new DBDataContext(SpecificInstances.DbFactory.CreateConnection()))
            using (var query = db.ADM_P_GetAvailableDelegations(GetSID(), extended))
            {
                var data = query.ToArray();
                if (extended) HttpContext.Current.Items["availableDelegation.extended"] = data;
                HttpContext.Current.Items["availableDelegation"] = data;
                return data;
            }
        }

        public static ADM_P_GetMyAvailableDelegationsResult[] GetMyAvailableDelegations()
        {
            if (!InitializerSection.PermissionsDelegationEnabled || HttpContext.Current == null || HttpContext.Current.User == null)
                return new ADM_P_GetMyAvailableDelegationsResult[0];

            if (HttpContext.Current.Items["myAvailableDelegation"] != null)
                return (ADM_P_GetMyAvailableDelegationsResult[])HttpContext.Current.Items["myAvailableDelegation"];

            using (var db = new DBDataContext(SpecificInstances.DbFactory.CreateConnection()))
            using (var query = db.ADM_P_GetMyAvailableDelegations(GetSID()))
            {
                var data = query.ToArray();
                HttpContext.Current.Items["myAvailableDelegation"] = data;
                return data;
            }
        }

        public static TResult GetPersonInfo<TResult>()
        {
            return GetPersonInfo<TResult>(GetSID());
        }

        public static GetPersonInfoBySidResult GetPersonInfo(string sid)
        {
            return GetPersonInfoInternal<GetPersonInfoBySidResult>(sid);
        }

        public static TResult GetPersonInfo<TResult>(string sid)
        {
            return GetPersonInfoInternal<TResult>(sid);
        }

        private static TResult GetPersonInfoInternal<TResult>(string sid)
        {
            if (string.IsNullOrEmpty(sid)) return default(TResult);

            try
            {
                if(InitializerSection.GetSection().IsConvertToSSDL)
                if (!sid.StartsWith("S-") && !"anonymous".Equals(sid))
                {
                    var base64 = Convert.FromBase64String(sid);
                    var sidValue = new SecurityIdentifier(base64, 0);
                    sid = sidValue.Value;
                }
            }
            catch (FormatException)
            {
            }

            var key = PersonInfoBySid + typeof(TResult).FullName + ">:" + sid;
            TResult value;
            if (HttpContext.Current != null && HttpContext.Current.Cache[key] != null)
                value = (TResult)HttpContext.Current.Cache[key];
            else
            {
                WebInitializer.Initialize();
                using (var db = new DBDataContext(SpecificInstances.DbFactory.CreateConnection()))
                {
                    value = db.GetPersonInfoBySid<TResult>(sid).FirstOrDefault();
                    if (value != null && HttpContext.Current != null)
                    {
                        HttpContext.Current.Cache.Add(
                            key,
                            value,
                            null,
                            DateTime.Now.AddMinutes(2),
                            Cache.NoSlidingExpiration,
                            CacheItemPriority.Normal,
                            null);
                    }
                }
            }

            return value;
        }

        public static void ClearCache<TResult>(string sid)
        {
            var key = PersonInfoBySid + typeof(TResult).FullName + ">:" + sid;
            HttpContext.Current.Cache.Remove(key);
        }

        public static string GetMacAddress()
        {
            var ip = IPAddress.Parse(HttpContext.Current.Request.UserHostAddress);
            var address = ip.GetAddressBytes();
            byte[] buffer = new byte[6];
            int len = buffer.Length;
            int r = SendARP((int)ip.Address, 0, buffer, ref len);
            return BitConverter.ToString(buffer, 0, 6);
        }

        public static void SetUserActivityTime(string sid, DateTime time)
        {
            WebInitializer.Initialize();
            using (var db = new DBDataContext(SpecificInstances.DbFactory.CreateConnection()))
            {
                db.ADM_P_SetUserActivityTime(sid, time);
            }
        }
    }
}