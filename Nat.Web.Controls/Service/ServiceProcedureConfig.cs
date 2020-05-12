/*
 * Created by: Sergey V. Shpakovskiy
 * Created: 15 ���� 2009 �.
 * Copyright � JSC New Age Technologies 2009
 */

using System;
using System.IO;
using System.Web;
using System.Xml.Serialization;

namespace Nat.Web.Controls.Service
{
    [Serializable]
    public class ServiceProcedureConfig
    {
        private static ServiceProcedureConfig instanceConfig;

        public static ServiceProcedureConfig Load()
        {
            if (instanceConfig != null)
                return instanceConfig.Clone();

            var ser = new XmlSerializer(typeof(ServiceProcedureConfig));
            var fileName = HttpContext.Current.Request.MapPath("~/App_Data/ServiceProcedureConfig.xml");
            if (!File.Exists(fileName))
            {
                instanceConfig = new ServiceProcedureConfig
                {
                    Enabled = false,
                    InitializeEnabled = false,
                    ServicePage = "/ServiceProcedure.aspx",
                    TimeBetweenRequest = new TimeSpan(0, 5, 0)
                };
                return instanceConfig;
            }

            using (var stream = new FileStream(fileName, FileMode.Open, FileAccess.Read))
            {
                var config = (ServiceProcedureConfig)ser.Deserialize(stream);
                config.TimeBetweenRequest = config._TimeBetweenRequest == null
                    ? (TimeSpan?) null
                    : new TimeSpan(config._TimeBetweenRequest.Value);
                return config;
            }
        }

        public void Save()
        {
            ServiceProcedure.IsInited = false;
            _TimeBetweenRequest = TimeBetweenRequest?.Ticks;
            instanceConfig = this;
         
            var ser = new XmlSerializer(typeof(ServiceProcedureConfig));
            var fileName = HttpContext.Current.Request.MapPath("~/App_Data/ServiceProcedureConfig.xml");
            using (var stream = new FileStream(fileName, FileMode.Create, FileAccess.Write))
                ser.Serialize(stream, this);
        }
        
        private ServiceProcedureConfig Clone()
        {
            return new ServiceProcedureConfig
            {
                ServicePage = ServicePage,
                Enabled = Enabled,
                CauseMessageKz = CauseMessageKz,
                CauseMessageRu = CauseMessageRu,
                InitializeEnabled = InitializeEnabled,
                TimeBetweenRequest = TimeBetweenRequest,
                TimeOfEndService = TimeOfEndService,
                TimeToEnable = TimeToEnable,
                TimeToShutDown = TimeToShutDown,
                _TimeBetweenRequest = _TimeBetweenRequest,
            };
        }

        #region Properties
        
        /// <summary>
        /// ��������, ������ ������������.
        /// </summary>
        public string ServicePage { get; set; }
        /// <summary>
        /// ���������� ������ ������������.
        /// </summary>
        public bool Enabled { get; set; }
        /// <summary>
        /// ��������� ��������� ������������.
        /// </summary>
        public bool InitializeEnabled { get; set;}
        /// <summary>
        /// ����� �������� ����� ��������� �������������.
        /// </summary>
        public TimeSpan? TimeBetweenRequest { get; set; }
        /// <summary>
        /// ����� � ������� ������� �� �������� �������������.
        /// </summary>
        public DateTime? TimeToShutDown { get; set; }
        /// <summary>
        /// ��������� ������ ������������.
        /// </summary>
        public DateTime? TimeToEnable { get; set; }
        /// <summary>
        /// ����� ���������� ������������.
        /// </summary>
        public DateTime? TimeOfEndService { get; set; }
        /// <summary>
        /// ������� ���������� ������� ���.
        /// </summary>
        public string CauseMessageRu { get; set; }

        /// <summary>
        /// ������� ���������� ������� ���.
        /// </summary>
        public string CauseMessageKz { get; set; }

        public long? _TimeBetweenRequest;

        public long? ServiceProcedureId { get; set; }
        
        #endregion
    }
}