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
using Microsoft.SharePoint;
using Nat.Web.Controls.Properties;
using System.Xml.Serialization;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using Nat.Web.Tools;

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
            var userSID = ((WindowsIdentity)HttpContext.Current.User.Identity).User.Value;
            var page = HttpContext.Current.Request.Url.AbsolutePath;
            //� ����� ����� ���� ����� ��������, ���� �� �� ����� ���������
            if (UserMayWork(userSID)) return true;
            //�������� ��������� �� ���� � ��� ��� ���� �������� ������, ���� �� �� �������� ���������
            if (UserIsExit(userSID, page)) return false;
            //���� ��� ������ �� �������� ��������, �� ���� �������� ������
            if (HttpContext.Current.Request.RequestType == "GET")
            {
                SetUserIsExit(userSID, page);
                return false;
            }
            //�������� �� ��������� ������� ������ � ��������
            if (TimeIsEnd(userSID, page))
            {
                SetUserIsExit(userSID, page);
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
                if (UsersMayWork.ContainsKey(user.SID))
                {
                    allUsersExit = false;
                    continue;
                }
                foreach (var page in user.Pages.Values)
                {
                    if (TimeIsEnd(user.SID, page.Page))
                        SetUserIsExit(user.SID, page.Page);
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
        private bool UserIsExit(string sid, string page)
        {
            //if (Users.ContainsKey(sid))
            return !Users[sid][page].IsWorking;
            //return false;
        }

        /// <summary>
        /// �������� ��������� �������
        /// </summary>
        /// <returns></returns>
        protected bool TimeIsEnd(string sid, string page)
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
        /// ���������� ����������� �������� MayWork
        /// </summary>
        /// <param name="sid"></param>
        /// <param name="mayWork"></param>
        public void SetUserMayWork(string sid, bool mayWork)
        {
            if (!UsersMayWork.ContainsKey(sid) && mayWork)
                UsersMayWork.Add(sid, Users[sid].Name);
            else if (UsersMayWork.ContainsKey(sid))
                UsersMayWork.Remove(sid);
        }

        /// <summary>
        /// ������� ����.
        /// </summary>
        protected UserInfo User
        {
            get
            {
                var userSID = ((WindowsIdentity)HttpContext.Current.User.Identity).User.Value;
                return Users[userSID];
            }
        }

        /// <summary>
        /// �������� ���� ���� ��� ����, ����� �������� �� ������ �� �����������
        /// </summary>
        /// <param name="sid"></param>
        /// <returns></returns>
        protected bool UserMayWork(string sid)
        {
            if (Users.ContainsKey(sid)) return UsersMayWork.ContainsKey(sid);
            return false;
        }

        /// <summary>
        /// ���������� ��� ���� �������� ����� �� ��������
        /// </summary>
        /// <param name="userSID"></param>
        /// <param name="page"></param>
        public void SetUserIsExit(string userSID, string page)
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
            public string SID { get; set; }
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
        public static Dictionary<string, string> UsersMayWork
        {
            get
            {
                var users = (Dictionary<string, string>)HttpContext.Current.Application["ServiceProcedure_UsersMayWork"];
                if (users == null)
                    UsersMayWork = users = new Dictionary<string, string>();
                return users;
            }
            set
            {
                HttpContext.Current.Application["ServiceProcedure_UsersMayWork"] = value;
            }
        }

        public void SaveUsersMayWork()
        {
            SPSecurity.RunWithElevatedPrivileges(
                delegate
                {
                    var ser = new BinaryFormatter();
                    string path = HttpContext.Current.Request.MapPath("ServiceProcedureUsersMayWork.xml");
                    using (var stream = new FileStream(path, FileMode.Create, FileAccess.Write))
                        ser.Serialize(stream, UsersMayWork);
                });
        }

        protected static Dictionary<string, string> LoadUsersMayWork()
        {
            var ser = new BinaryFormatter();
            var fileName = HttpContext.Current.Request.MapPath("ServiceProcedureUsersMayWork.xml");
            if (!File.Exists(fileName)) return new Dictionary<string, string>();
            using (var stream = new FileStream(fileName, FileMode.Open, FileAccess.Read))
                return (Dictionary<string, string>)ser.Deserialize(stream);
        }

        /// <summary>
        /// ��������� ��� ������������, � ��� ��� ������� ����� ������������.
        /// </summary>
        public string EnabledMessage
        {
            get
            {
                var causeMessage = LocalizationHelper.IsCultureKZ ? CauseMessageKz : CauseMessageRu;
                string causeName = UppercaseFirst(causeMessage);
                causeMessage = (string.IsNullOrEmpty(causeMessage)) ? string.Empty : string.Format(Resources.SServiceProcedure_CauseMessage, causeName);
                //string message = Resources.SServiceProcedure_EnabledMessage;
                string message = (string.IsNullOrEmpty(causeMessage)) ? Resources.SServiceProcedure_EnabledMessage : causeMessage;
                if (TimeBetweenRequest != null)
                    message += string.Format(Resources.SServiceProcedure_TimeBetweenRequest, TimeBetweenRequest.Value);
                if (TimeToShutDown != null)
                    message += string.Format(Resources.SServiceProcedure_TimeToShutDown, TimeToShutDown.Value);
                if (TimeOfEndService != null)
                {
                    if (string.IsNullOrEmpty(causeName))
                    {
                        message += string.Format(Resources.SServiceProcedure_TimeOfEndService, TimeOfEndService.Value);
                    }
                    else
                    {
                        message += string.Format(Resources.SServiceProcedure_TimeOfEndCause, TimeOfEndService.Value);
                    }
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
                if (!Enabled) return BeforeEnabledMessage;
                var causeMessage = LocalizationHelper.IsCultureKZ ? CauseMessageKz : CauseMessageRu;
                string causeName = causeMessage;
                causeMessage = (string.IsNullOrEmpty(causeMessage)) ? string.Empty : string.Format(Resources.SServiceProcedure_ServiceProcedureMessage, causeMessage);
                string message = (string.IsNullOrEmpty(causeMessage)) ? Resources.SServiceProcedure : causeMessage;
                if (TimeOfEndService != null)
                {
                    if (string.IsNullOrEmpty(causeName))
                    {
                        message += string.Format(Resources.SServiceProcedure_TimeOfEndService, TimeOfEndService.Value);
                    }
                    else
                    {
                        message += string.Format(Resources.SServiceProcedure_TimeOfEndCause, TimeOfEndService.Value);
                    }
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
                if (TimeToEnable == null) return "";
                //var message = string.Format(Resources.SServiceProcedure_TimeToEnableMessage, TimeToEnable.Value);
                var causeMessage = LocalizationHelper.IsCultureKZ ? CauseMessageKz : CauseMessageRu;
                string causeName = causeMessage;
                causeMessage = (string.IsNullOrEmpty(causeMessage)) ? string.Empty : string.Format(Resources.SServiceProcedure_BeforeEnabledMessage, TimeToEnable.Value, causeMessage);
                var message = (string.IsNullOrEmpty(causeMessage)) ? string.Format(Resources.SServiceProcedure_TimeToEnableMessage, TimeToEnable.Value) : causeMessage;
                if (TimeOfEndService != null)
                {
                    if (string.IsNullOrEmpty(causeName))
                    {
                        message += string.Format(Resources.SServiceProcedure_TimeOfEndService, TimeOfEndService.Value);
                    }
                    else
                    {
                        message += string.Format(Resources.SServiceProcedure_TimeOfEndCause, TimeOfEndService.Value);
                    }
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

        protected class UsersDic : Dictionary<string, UserInfo>
        {
            public new UserInfo this[string sid]
            {
                get
                {
                    if (!base.ContainsKey(sid))
                    {
                        var identity = ((WindowsIdentity)HttpContext.Current.User.Identity);
                        base[sid] = new UserInfo
                        {
                            SID = sid,
                            IsWorking = true,
                            TimeRequest = DateTime.Now,
                            Pages = new Dictionary<string, ItemPage>(),
                            Name = identity.User.Value == sid ? identity.Name : "",
                        };
                    }
                    return base[sid];
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
            {
                return string.Empty;
            }
            return char.ToUpper(str[0]) + str.Substring(1);
        }
    }
}