// --------------------------------------------------------------------------------------------------------------------
// <copyright file="WebServiceTraceTimeOfDestinationUser.asmx.cs" company="NAT Software">
//   NAT Software
// </copyright>
// <summary>
//   Сервис для получения ответа от клиента о получении им страницы, используется для трасировки времени.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Nat.Web.Controls.Trace
{
    using System;
    using System.Web;
    using System.Web.Services;

    using Nat.Tools.Specific;
    using Nat.Web.Controls.Data;
    using Nat.Web.Tools.Initialization;

    /// <summary>
    /// Сервис для получения ответа от клиента о получении им страницы, используется для трасировки времени.
    /// </summary>
    [WebService(Namespace = "http://tempuri.org/")]
    [WebServiceBinding(ConformsTo = WsiProfiles.BasicProfile1_1)]
    [System.ComponentModel.ToolboxItem(false)]
     [System.Web.Script.Services.ScriptService]
    public class WebServiceTraceTimeOfDestinationUser : WebService
    {
        [WebMethod(EnableSession = true)]
        public void TraceDestination(string key)
        {
            if (string.IsNullOrEmpty(key)) return;
            var guid = new Guid(key);
            if (HttpContext.Current.Session[guid.ToString("N")] == null)
                return;
            var dateTime = (DateTime)HttpContext.Current.Session[guid.ToString("N")];
            WebInitializer.Initialize();
            using (var db = new DBTraceTimingRequestsDataContext(SpecificInstances.DbFactory.CreateConnection()))
                db.P_LOG_UpdateTraceTimingRequest(guid, (DateTime.Now.Ticks - dateTime.Ticks) / 10000);
            HttpContext.Current.Session.Remove(guid.ToString("N"));
        }
    }
}
