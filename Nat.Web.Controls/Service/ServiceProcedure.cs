/*
 * Created by: Sergey V. Shpakovskiy
 * Created: 15 ���� 2009 �.
 * Copyright � JSC New Age Technologies 2009
 */

using System;
using System.Linq;
using System.Collections.Generic;
using System.Security.Principal;
using System.Web;
using Nat.Web.Controls.Properties;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using Nat.Web.Tools;
using Nat.Web.Tools.Security;

namespace Nat.Web.Controls.Service
{
    public class ServiceProcedure
    {
        private static object _lock = new object();
        private UserInfo _user;

        /// <summary>
        /// �������� ����� �� ������������ ������� ��������
        /// </summary>
        /// <returns></returns>
        public bool MayOpenSite()
        {
            //�������� ������� �� ����� ������������, ���� ��� �� ����� ���������
            if (!Enabled) return true;
            var userId = Tools.Security.User.GetPersonInfo()?.id ?? 0;
            var page = HttpContext.Current.Request.Url.AbsolutePath;
            //� ����� ����� ���� ����� ��������, ���� �� �� ����� ���������
            if (UserMayWork(userId))
                return true;

            //�������� ��������� �� ���� � ��� ��� ���� �������� ������, ���� �� �� �������� ���������
            if (UserIsExit(userId, page)) return false;
            //���� ��� ������ �� �������� ��������, �� ���� �������� ������
            if (HttpContext.Current.Request.RequestType == "GET")
            {
                SetUserIsExit(userId, page);
                return false;
            }
            //�������� �� ��������� ������� ������ � ��������
            if (TimeIsEnd(userId, page))
            {
                SetUserIsExit(userId, page);
                return false;
            }
            SetUserPostBack();
            return true;
        }

        /// <summary>
        /// �������� �� ������������� �������� ����� ������������, �.�. ������ �����
        /// </summary>
        public void EnsureOnEnabled()
        {
            Initialize();
            var timeToEnable = TimeToEnable;
            if (timeToEnable != null && timeToEnable.Value <= DateTime.Now)
            {
                TimeToEnable = null;
                Enabled = true;
            }
        }

        public void InitializeCauses()
        {
            var config = ServiceProcedureConfig.Load();
            CauseMessageRu = UppercaseFirst(config.CauseMessageRu);
            CauseMessageKz = UppercaseFirst(config.CauseMessageKz);
        }

        protected void Initialize()
        {
            var config = ServiceProcedureConfig.Load();
            lock (_lock)
                if (!IsInited)
                {
                    IsInited = true;
                    InitializeByConfig(config);
                }
        }

        protected void InitializeByConfig(ServiceProcedureConfig config)
        {
            if (config != null && config.InitializeEnabled)
            {
                Enabled = config.Enabled;
                ServicePage = config.ServicePage;
                TimeBetweenRequest = config.TimeBetweenRequest;
                TimeToShutDown = config.TimeToShutDown;
                TimeToEnable = config.TimeToEnable;
                TimeOfEndService = config.TimeOfEndService;
                UsersMayWork = LoadUsersMayWork();
                CauseMessageRu = config.CauseMessageRu;
                CauseMessageKz = config.CauseMessageKz;
            }
            else
            {
                Enabled = false;
                TimeToEnable = null;
            }
        }

        /// <summary>
        /// �������� ���� ��� ��� ����� ����� �� �������, �� ��������� ������� ��� ������ ����� ��� �����
        /// </summary>
        /// <returns></returns>
        public bool EnsureAllUsersExit()
        {
            bool allUsersExit = true;
            foreach (var user in Users.Values)
            {
                if (!user.IsWorking) continue;
                if (UsersMayWork.Contains(user.id))
                {
                    allUsersExit = false;
                    continue;
                }
                foreach (var page in user.Pages.Values)
                {
                    if (TimeIsEnd(user.id, page.Page))
                        SetUserIsExit(user.id, page.Page);
                    else
                        allUsersExit = false;
                }
            }
            return allUsersExit;
        }

        /// <summary>
        /// �������� ����� �� ������������ �� ��������
        /// </summary>
        /// <param name="sid"></param>
        /// <param name="page"></param>
        /// <returns></returns>
        private bool UserIsExit(long sid, string page)
        {
            //if (Users.ContainsKey(sid))
            return !Users[sid][page].IsWorking;
            //return false;
        }

        /// <summary>
        /// �������� ��������� �������
        /// </summary>
        /// <returns></returns>
        protected bool TimeIsEnd(long sid, string page)
        {
            var time = TimeToShutDown;
            //����� ������ �����������
            if (time <= DateTime.Now) return true;
            var timeBetweenRequest = TimeBetweenRequest;
            //����������� ����� ��������� ����
            if (timeBetweenRequest == null) return false;
            var timeRequest = Users[sid][page].TimeRequest;
            //����� ����� ��������� ����� � ���� �������� �������
            if (timeRequest.Add(timeBetweenRequest.Value) < DateTime.Now) return true;
            return false;
        }

        /// <summary>
        /// ������� ��� ���� �������� �������
        /// </summary>
        public void SetUserPostBack()
        {
            User.TimeRequest = DateTime.Now;
            User[HttpContext.Current.Request.Url.AbsolutePath].TimeRequest = DateTime.Now;
        }

        /// <summary>
        /// ������� ����.
        /// </summary>
        protected UserInfo User => Users[Tools.Security.User.GetPersonInfoRequired().id];

        /// <summary>
        /// �������� ���� ���� ��� ����, ����� �������� �� ������ �� �����������
        /// </summary>
        /// <param name="sid"></param>
        /// <returns></returns>
        protected bool UserMayWork(long id)
        {
            if (UserRoles.IsInAnyRoles(UserRoles.ADMIN, UserRoles.ServiceProcedureConfig))
                return true;

            return UsersMayWork.Contains(id);
        }

        /// <summary>
        /// ���������� ��� ���� �������� ����� �� ��������
        /// </summary>
        /// <param name="userSID"></param>
        /// <param name="page"></param>
        public void SetUserIsExit(long userSID, string page)
        {
            //if (Users.ContainsKey(userSID))
            {
                var user = Users[userSID];
                user[page].IsWorking = false;
                user[page].TimeExit = DateTime.Now;
                user.IsWorking = user.Pages.Values.Count(p => p.IsWorking) > 0;
                if (!user.IsWorking) user.TimeExit = DateTime.Now;
            }
        }

        /// <summary>
        /// ������������ ������������ � �������.
        /// </summary>
        protected UsersDic Users
        {
            get
            {
                var users = HttpContext.Current.Application["ServiceProcedure_Users"] as UsersDic;
                if (users == null)
                {
                    users = new UsersDic();
                    HttpContext.Current.Application["ServiceProcedure_Users"] = users;
                }
                return users;
            }
        }

        /// <summary>
        /// ��������� ������������.
        /// </summary>
        public class UserInfo
        {
            public bool IsWorking { get; set; }
            public long id { get; set; }
            public string Name { get; set; }
            public DateTime TimeRequest { get; set; }
            public DateTime? TimeExit { get; set; }
            /// <summary>
            /// ���������� � ������������� �������.
            /// </summary>
            public Dictionary<string, ItemPage> Pages { get; set; }

            public ItemPage this[string page]
            {
                get
                {
                    ItemPage itemPage;
                    if (Pages.ContainsKey(page))
                        itemPage = Pages[page];
                    else
                    {
                        itemPage = Pages[page] =
                            new ItemPage
                            {
                                IsWorking = true,
                                Page = page,
                                TimeRequest = DateTime.Now,
                            };
                        IsWorking = true;
                    }
                    return itemPage;
                }
            }
        }

        /// <summary>
        /// ���������� � ������������� ��������
        /// </summary>
        public class ItemPage
        {
            public string Page { get; set; }
            public bool IsWorking { get; set; }
            public DateTime TimeRequest { get; set; }
            public DateTime? TimeExit { get; set; }
        }

        /// <summary>
        /// ���������� ������ ������������.
        /// </summary>
        public bool Enabled
        {
            get
            {
                var enabled = "ON".Equals(HttpContext.Current.Application["ServiceProcedure_Enabled"]);
                //�������� �� ������������� �������� ����� ������������
                if (!enabled) EnsureOnEnabled();
                return enabled;
            }
            set
            {
                if (value)
                    HttpContext.Current.Application["ServiceProcedure_Enabled"] = "ON";
                else
                    HttpContext.Current.Application.Remove("ServiceProcedure_Enabled");
            }
        }

        /// <summary>
        /// ��������, ������ ������������.
        /// </summary>
        protected string ServicePage
        {
            get
            {
                return (string)HttpContext.Current.Application["ServiceProcedure_ServicePage"];
            }
            set
            {
                HttpContext.Current.Application["ServiceProcedure_ServicePage"] = value;
            }
        }

        /// <summary>
        /// ����� �������� ����� ��������� �������������.
        /// </summary>
        public TimeSpan? TimeBetweenRequest
        {
            get
            {
                return (TimeSpan?)HttpContext.Current.Application["ServiceProcedure_TimeBetweenRequest"];
            }
            set
            {
                HttpContext.Current.Application["ServiceProcedure_TimeBetweenRequest"] = value;
            }
        }

        /// <summary>
        /// ����� � ������� ������� �� �������� �������������.
        /// </summary>
        public DateTime? TimeToShutDown
        {
            get
            {
                return (DateTime?)HttpContext.Current.Application["ServiceProcedure_TimeToShutDown"];
            }
            set
            {
                HttpContext.Current.Application["ServiceProcedure_TimeToShutDown"] = value;
            }
        }

        /// <summary>
        /// ��������� ������ ������������.
        /// </summary>
        public DateTime? TimeToEnable
        {
            get
            {
                return (DateTime?)HttpContext.Current.Application["ServiceProcedure_TimeToEnable"];
            }
            set
            {
                HttpContext.Current.Application["ServiceProcedure_TimeToEnable"] = value;
            }
        }

        /// <summary>
        /// ����� ���������� ������������.
        /// </summary>
        public DateTime? TimeOfEndService
        {
            get
            {
                return (DateTime?)HttpContext.Current.Application["ServiceProcedure_TimeOfEndService"];
            }
            set
            {
                HttpContext.Current.Application["ServiceProcedure_TimeOfEndService"] = value;
            }
        }

        /// <summary>
        /// ������ SID-��, ��� ������ ��������� ������������ �������� �� �������� �� ������.
        /// </summary>
        public static List<long> UsersMayWork
        {
            get
            {
                var users = (List<long>)HttpContext.Current.Application["ServiceProcedure_UsersMayWork"];
                if (users == null) UsersMayWork = users = new List<long>();
                return users;
            }
            set => HttpContext.Current.Application["ServiceProcedure_UsersMayWork"] = value;
        }

        public static void SaveUsersMayWork(List<long> ids)
        {
            var ser = new BinaryFormatter();
            string path = HttpContext.Current.Request.MapPath("~/App_Data/ServiceProcedureUsersMayWork.xml");
            using (var stream = new FileStream(path, FileMode.Create, FileAccess.Write))
                ser.Serialize(stream, ids);
        }

        protected static List<long> LoadUsersMayWork()
        {
            var ser = new BinaryFormatter();
            var fileName = HttpContext.Current.Request.MapPath("~/App_Data/ServiceProcedureUsersMayWork.xml");
            if (!File.Exists(fileName)) return new List<long>();
            using (var stream = new FileStream(fileName, FileMode.Open, FileAccess.Read))
                return ser.Deserialize(stream) as List<long> ?? new List<long>();
        }

        /// <summary>
        /// ��������� ��� ������������, � ��� ��� ������� ����� ������������.
        /// </summary>
        public string EnabledMessage
        {
            get
            {
                var causeMessage = LocalizationHelper.IsCultureKZ ? CauseMessageKz : CauseMessageRu;
                var message = string.IsNullOrEmpty(causeMessage)
                    ? Resources.SServiceProcedure_EnabledMessage
                    : string.Format(Resources.SServiceProcedure_CauseMessage, UppercaseFirst(causeMessage));
                message += " ";
                if (TimeToShutDown != null && TimeToShutDown.Value > DateTime.Now)
                {
                    if (TimeBetweenRequest != null)
                        message += " " + string.Format(Resources.SServiceProcedure_TimeBetweenRequest, TimeBetweenRequest.Value);

                    message += " " + string.Format(Resources.SServiceProcedure_TimeToShutDown, TimeToShutDown.Value.ToString("dd.MM.yyyy HH:mm"));
                }
                if (TimeOfEndService != null)
                {
                    message += " " + string.Format(string.IsNullOrEmpty(causeMessage)
                                       ? Resources.SServiceProcedure_TimeOfEndService
                                       : Resources.SServiceProcedure_TimeOfEndCause,
                                   TimeOfEndService.Value.ToString("dd.MM.yyyy HH:mm"));
                }

                return message;
            }
        }

        /// <summary>
        /// ��������� � ���������� ����������� �����.
        /// </summary>
        public string ServiceProcedureMessage
        {
            get
            {
                if (!Enabled) 
                    return BeforeEnabledMessage;

                var causeMessage = LocalizationHelper.IsCultureKZ ? CauseMessageKz : CauseMessageRu;
                var message = string.IsNullOrEmpty(causeMessage)
                    ? Resources.SServiceProcedure
                    : string.Format(Resources.SServiceProcedure_ServiceProcedureMessage, causeMessage);
                if (TimeOfEndService != null)
                {
                    message += " " + string.Format(
                        string.IsNullOrEmpty(causeMessage)
                            ? Resources.SServiceProcedure_TimeOfEndService
                            : Resources.SServiceProcedure_TimeOfEndCause, TimeOfEndService.Value.ToString("dd.MM.yyyy HH:mm"));
                }

                return message;
            }
        }

        /// <summary>
        /// ��������� �����, � ��� ��� ����������� ��������� ����������� ������.
        /// </summary>
        public string BeforeEnabledMessage
        {
            get
            {
                if (TimeToEnable == null) 
                    return string.Empty;

                var causeMessage = LocalizationHelper.IsCultureKZ ? CauseMessageKz : CauseMessageRu;
                var message = string.IsNullOrEmpty(causeMessage)
                    ? string.Format(Resources.SServiceProcedure_TimeToEnableMessage, TimeToEnable.Value.ToString("dd.MM.yyyy HH:mm"))
                    : string.Format(Resources.SServiceProcedure_BeforeEnabledMessage, TimeToEnable.Value.ToString("dd.MM.yyyy HH:mm"), causeMessage);
               
                if (TimeOfEndService != null)
                {
                    message += " " + string.Format(
                        string.IsNullOrEmpty(causeMessage)
                            ? Resources.SServiceProcedure_TimeOfEndService
                            : Resources.SServiceProcedure_TimeOfEndCause, TimeOfEndService.Value.ToString("dd.MM.yyyy HH:mm"));
                }

                return message;
            }
        }

        public static bool IsInited
        {
            get { return (bool?)HttpContext.Current.Application["ServiceProcedure_IsInited"] ?? false; }
            set
            {
                HttpContext.Current.Application["ServiceProcedure_IsInited"] = value;
                if (value == false)
                {
                    HttpContext.Current.Application.Remove("ServiceProcedure_Enabled");
                    HttpContext.Current.Application.Remove("ServiceProcedure_Users");
                    //var c = new ServiceProcedure();
                    //c.Enabled = false;
                    //c.EnsureOnEnabled();
                }
            }
        }

        public void Redirect()
        {
            HttpContext.Current.Response.Redirect(ServicePage ?? "ServiceProcedure.aspx");
        }

        protected class UsersDic : Dictionary<long, UserInfo>
        {
            private object _lock = new object();

            public new UserInfo this[long id]
            {
                get
                {
                    if (!ContainsKey(id))
                    {
                        var userInfo = new UserInfo
                        {
                            id = id,
                            IsWorking = true,
                            TimeRequest = DateTime.Now,
                            Pages = new Dictionary<string, ItemPage>(),
                        };

                        lock(_lock)
                            return base[id] = userInfo;
                    }

                    lock (_lock)
                        return base[id];
                }
            }
        }

        public IEnumerable<UserInfo> GetUsers()
        {
            return Users.Values;
        }

        public string CauseMessageRu { get; set; }

        public string CauseMessageKz { get; set; }

        /// <summary>
        /// ������ ����� ��������� � Uppercase
        /// </summary>
        public string UppercaseFirst(string str)
        {
            if (string.IsNullOrEmpty(str))
                return string.Empty;

            return char.ToUpper(str[0]) + str.Substring(1);
        }
    }
}