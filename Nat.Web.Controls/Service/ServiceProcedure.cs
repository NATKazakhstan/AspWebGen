/*
 * Created by: Sergey V. Shpakovskiy
 * Created: 15 июня 2009 г.
 * Copyright © JSC New Age Technologies 2009
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
        /// Проверка может ли пользователь открыть страницу
        /// </summary>
        /// <returns></returns>
        public bool MayOpenSite()
        {
            //проверка включен ли режим обслуживания, если нет то можно открывать
            if (!Enabled) return true;
            var userSID = ((WindowsIdentity)HttpContext.Current.User.Identity).User.Value;
            var page = HttpContext.Current.Request.Url.AbsolutePath;
            //у юзера стоит флаг может работать, если да то можно открывать
            if (UserMayWork(userSID)) return true;
            //проверка поставлен ли флаг о том что юзер закончил работу, если да то открытие запрещено
            if (UserIsExit(userSID, page)) return false;
            //если это запрос на открытие страницы, то юзер завершил работу
            if (HttpContext.Current.Request.RequestType == "GET")
            {
                SetUserIsExit(userSID, page);
                return false;
            }
            //проверка на истечение времени работы с системой
            if (TimeIsEnd(userSID, page))
            {
                SetUserIsExit(userSID, page);
                return false;
            }
            SetUserPostBack();
            return true;
        }

        /// <summary>
        /// Проверка на необходимость включить режим обслуживания, т.к. пришло время
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
        /// Проверка того что все юзеры вышли из системы, по критериям времени или статус стоит что вышли
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
        /// Проверка вышел ли пользователь со страницы
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
        /// Проверка истечения времени
        /// </summary>
        /// <returns></returns>
        protected bool TimeIsEnd(string sid, string page)
        {
            var time = TimeToShutDown;
            //Время работы закончилось
            if (time <= DateTime.Now) return true;
            var timeBetweenRequest = TimeBetweenRequest;
            //ограничения между запросами нету
            if (timeBetweenRequest == null) return false;
            var timeRequest = Users[sid][page].TimeRequest;
            //время между запросами юзера к этой страницы истекло
            if (timeRequest.Add(timeBetweenRequest.Value) < DateTime.Now) return true;
            return false;
        }

        /// <summary>
        /// Указать что юзер выполнил постбак
        /// </summary>
        public void SetUserPostBack()
        {
            User.TimeRequest = DateTime.Now;
            User[HttpContext.Current.Request.Url.AbsolutePath].TimeRequest = DateTime.Now;
        }

        /// <summary>
        /// Установить пользвателю значение MayWork
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
        /// Текущий юзер.
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
        /// Получить флаг того что юзер, может работать не смотря на ограничения
        /// </summary>
        /// <param name="sid"></param>
        /// <returns></returns>
        protected bool UserMayWork(string sid)
        {
            if (Users.ContainsKey(sid)) return UsersMayWork.ContainsKey(sid);
            return false;
        }

        /// <summary>
        /// Установить что юзер выполнил выход из страницы
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
        /// Пользователи обращавшиеся к системе.
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
        /// Параметры пользователя.
        /// </summary>
        public class UserInfo
        {
            public bool IsWorking { get; set; }
            public string SID { get; set; }
            public string Name { get; set; }
            public DateTime TimeRequest { get; set; }
            public DateTime? TimeExit { get; set; }
            /// <summary>
            /// Информация о использовании страниц.
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
        /// Информация о использовании страницы
        /// </summary>
        public class ItemPage
        {
            public string Page { get; set; }
            public bool IsWorking { get; set; }
            public DateTime TimeRequest { get; set; }
            public DateTime? TimeExit { get; set; }
        }

        /// <summary>
        /// Активность режима обслуживания.
        /// </summary>
        public bool Enabled
        {
            get
            {
                var enabled = "ON".Equals(HttpContext.Current.Application["ServiceProcedure_Enabled"]);
                //Проверка на необходимость включить режим обслуживания
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
        /// Страница, режима обслуживания.
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
        /// Время ожидания между запросами пользователей.
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
        /// Время с котрого система не доступна пользователям.
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
        /// Включение режима обслуживания.
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
        /// Время завершения обслуживания.
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
        /// Список SID-ов, тех юзеров разрешено пользоваться системой не зависимо от режима.
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
        /// Сообщение для пользователя, о том что включен режим обслуживания.
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
        /// Сообщение о проведении технических работ.
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
        /// Сообщение юзеру, о том что планируется проводить технические работы.
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
        /// Первую буквы переводим в Uppercase
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