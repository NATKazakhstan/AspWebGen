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
            var userName = identity.AuthenticationType == "Forms" ? GetUserNameByFormIdentity(identity) : GetUserNameByWindowsIdentity(identity);

            return !string.IsNullOrEmpty(userName) ? userName : string.Empty;
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
                HttpContext.Current.Response.Redirect("/MainPage.aspx/data/NoPermit");
            return value;
        }

        public static TResult GetPersonInfoRequired<TResult>()
        {
            var value = GetPersonInfo<TResult>(GetSID());
            if (value == null)
                HttpContext.Current.Response.Redirect("/MainPage.aspx/data/NoPermit");
            return value;
        }

        public static GetPersonInfoBySidResult GetPersonInfo()
        {
            return GetPersonInfo(GetSID());
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

        public static string GetMacAddress()
        {
            var ip = IPAddress.Parse(HttpContext.Current.Request.UserHostAddress);
            var address = ip.GetAddressBytes();
            byte[] buffer = new byte[6];
            int len = buffer.Length;
            int r = SendARP((int)ip.Address, 0, buffer, ref len);
            return BitConverter.ToString(buffer, 0, 6);
        }
    }
}