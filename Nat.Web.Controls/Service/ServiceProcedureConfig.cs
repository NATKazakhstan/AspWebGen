/*
 * Created by: Sergey V. Shpakovskiy
 * Created: 15 июня 2009 г.
 * Copyright © JSC New Age Technologies 2009
 */

using System;
using System.IO;
using System.Web;
using System.Xml.Serialization;
using System.Collections.Generic;
using Microsoft.SharePoint;

namespace Nat.Web.Controls.Service
{
    [Serializable]
    public class ServiceProcedureConfig
    {
        public static ServiceProcedureConfig Load()
        {
            var ser = new XmlSerializer(typeof(ServiceProcedureConfig));
            var fileName = HttpContext.Current.Request.MapPath("ServiceProcedureConfig.xml");
            if (!File.Exists(fileName)) return new ServiceProcedureConfig { Enabled = false, InitializeEnabled = false, ServicePage = "/ServiceProcedure.aspx", TimeBetweenRequest = new TimeSpan(0, 5, 0) };
            using (var stream = new FileStream(fileName, FileMode.Open, FileAccess.Read))
            {
                var config = (ServiceProcedureConfig)ser.Deserialize(stream);
                if (config._TimeBetweenRequest == null)
                    config.TimeBetweenRequest = null;
                else
                    config.TimeBetweenRequest = new TimeSpan(config._TimeBetweenRequest.Value);
                return config;
            }
        }

        public void Save()
        {
            ServiceProcedure.IsInited = false;
            if (TimeBetweenRequest == null)
                _TimeBetweenRequest = null;
            else
                _TimeBetweenRequest = TimeBetweenRequest.Value.Ticks;
            var ser = new XmlSerializer(typeof(ServiceProcedureConfig));
            SPSecurity.RunWithElevatedPrivileges(
                delegate
                    {
                        var fileName = HttpContext.Current.Request.MapPath("ServiceProcedureConfig.xml");
                        using (var stream = new FileStream(fileName, FileMode.Create, FileAccess.Write))
                            ser.Serialize(stream, this);
                    });
        }

        /// <summary>
        /// Страница, режима обслуживания.
        /// </summary>
        public string ServicePage { get; set; }
        /// <summary>
        /// Активность режима обслуживания.
        /// </summary>
        public bool Enabled { get; set; }
        /// <summary>
        /// Применить настройки конфигурации.
        /// </summary>
        public bool InitializeEnabled { get; set;}
        /// <summary>
        /// Время ожидания между запросами пользователей.
        /// </summary>
        public TimeSpan? TimeBetweenRequest { get; set; }
        /// <summary>
        /// Время с котрого система не доступна пользователям.
        /// </summary>
        public DateTime? TimeToShutDown { get; set; }
        /// <summary>
        /// Включение режима обслуживания.
        /// </summary>
        public DateTime? TimeToEnable { get; set; }
        /// <summary>
        /// Время завершения обслуживания.
        /// </summary>
        public DateTime? TimeOfEndService { get; set; }
        /// <summary>
        /// Причина отключения системы РУС.
        /// </summary>
        public string CauseMessageRu { get; set; }

        /// <summary>
        /// Причина отключения системы КАЗ.
        /// </summary>
        public string CauseMessageKz { get; set; }

        public long? _TimeBetweenRequest;
    }
}