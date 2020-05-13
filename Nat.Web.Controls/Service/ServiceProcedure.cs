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
        /// Проверка может ли пользователь открыть страницу
        /// </summary>
        /// <returns></returns>
        public bool MayOpenSite()
        {
            //проверка включен ли режим обслуживания, если нет то можно открывать
            if (!Enabled) return true;
            var userId = Tools.Security.User.GetPersonInfo()?.id ?? 0;
            var page = HttpContext.Current.Request.Url.AbsolutePath;
            //у юзера стоит флаг может работать, если да то можно открывать
            if (UserMayWork(userId))
                return true;

            //проверка поставлен ли флаг о том что юзер закончил работу, если да то открытие запрещено
            if (UserIsExit(userId, page)) return false;
            //если это запрос на открытие страницы, то юзер завершил работу
            if (HttpContext.Current.Request.RequestType == "GET")
            {
                SetUserIsExit(userId, page);
                return false;
            }
            //проверка на истечение времени работы с системой
            if (TimeIsEnd(userId, page))
            {
                SetUserIsExit(userId, page);
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
            else
            {
                Enabled = false;
                TimeToEnable = null;
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
        /// Проверка вышел ли пользователь со страницы
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
        /// Проверка истечения времени
        /// </summary>
        /// <returns></returns>
        protected bool TimeIsEnd(long sid, string page)
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
        /// Текущий юзер.
        /// </summary>
        protected UserInfo User => Users[Tools.Security.User.GetPersonInfoRequired().id];

        /// <summary>
        /// Получить флаг того что юзер, может работать не смотря на ограничения
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
        /// Установить что юзер выполнил выход из страницы
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
            public long id { get; set; }
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
        /// Сообщение для пользователя, о том что включен режим обслуживания.
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
        /// Сообщение о проведении технических работ.
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
        /// Сообщение юзеру, о том что планируется проводить технические работы.
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
        /// Первую буквы переводим в Uppercase
        /// </summary>
        public string UppercaseFirst(string str)
        {
            if (string.IsNullOrEmpty(str))
                return string.Empty;

            return char.ToUpper(str[0]) + str.Substring(1);
        }
    }
}