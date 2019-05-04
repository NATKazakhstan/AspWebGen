/*
* Created by: Sergey V. Shpakovskiy
* Created: 2012.06.01
* Copyright © JSC NAT Kazakhstan 2012
*/

namespace Nat.Web.Controls.Trace
{
    using System;
    using System.Collections.Generic;
    using System.Data;
    using System.Reflection;
    using System.Text.RegularExpressions;
    using System.Web;
    using System.Web.Configuration;
    using System.Web.UI;
    using System.Xml.Linq;

    using Nat.Tools.Specific;
    using Nat.Web.Controls.Data;
    using Nat.Web.Tools.Initialization;
    using Nat.Web.Tools.Security;

    public class TraceTimeOfDestinationUserControl : Control
    {
        private const string ScriptFormat =
            @"
$(document).ready(function() 
{{
    Nat.Web.Controls.Trace.WebServiceTraceTimeOfDestinationUser.TraceDestination('{0}');
}});";

        private static readonly Regex pageRegex = new Regex(@"\w+\.aspx", RegexOptions.Compiled | RegexOptions.IgnoreCase);
        private static readonly Regex tableNameAndPageTypeRegex = new Regex(@"/data/(?<TableName>\w+)(?<PageType>Edit/read|Edit/new|Edit|Journal/select|Journal|Filter)", RegexOptions.Compiled | RegexOptions.IgnoreCase);

        private ScriptManager scriptManager;
        private DateTime dateTimeOnInit;

        public ScriptManager ScriptManager
        {
            get { return this.scriptManager ?? (this.scriptManager = ScriptManager.GetCurrent(this.Page)); }
        }
        
        protected override void OnPreRender(EventArgs e)
        {
            var trace = WebConfigurationManager.AppSettings["NatTraceTimingReqeusts"];
            if (string.IsNullOrEmpty(trace) || HttpContext.Current.Items["TraceTimingRegeusts.TraceKey"] != null)
                return;
            var guid = Guid.NewGuid();
            var script = string.Format(ScriptFormat, guid);
            Page.ClientScript.RegisterClientScriptBlock(GetType(), "TraceTimeOfDestinationUserControl", script, true);
            HttpContext.Current.Items["TraceTimingRegeusts.TraceKey"] = guid;
            HttpContext.Current.Session[guid.ToString("N")] = HttpContext.Current.Timestamp;
            base.OnPreRender(e);
        }

        protected override void OnInit(EventArgs e)
        {
            this.ScriptManager.Services.Add(new ServiceReference("/WebServiceTraceTimeOfDestinationUser.asmx"));
            base.OnInit(e);
            if (Context != null && HttpContext.Current.Request.Url.AbsolutePath.Contains(".aspx"))
            {
                var trace = WebConfigurationManager.AppSettings["NatTraceTimingReqeusts"];
                if (!string.IsNullOrEmpty(trace))
                {
                    if (HttpContext.Current.Trace.IsEnabled)
                        HttpContext.Current.Trace.TraceFinished += TraceTimingRegeusts;
                    else
                    {
                        var app = HttpContext.Current.ApplicationInstance as Global;
                        if (app != null)
                            app.CustomEndRequest += ApplicationInstanceOnEndRequest;
                    }

                    this.dateTimeOnInit = DateTime.Now;
                }
            }
        }

        private void ApplicationInstanceOnEndRequest(object sender, EventArgs eventArgs)
        {
            var traceXml = new XElement("Root");
            WriteTrace(HttpContext.Current.TraceExt().TraceInfoExt, traceXml);
            TraceRequest(traceXml);
        }

        private void TraceTimingRegeusts(object sender, TraceContextEventArgs traceContextEventArgs)
        {
            HttpContext.Current.Trace.TraceFinished -= TraceTimingRegeusts;
            var traceXml = new XElement("Root");
            WriteTrace(traceContextEventArgs, traceXml);
            WriteTrace(HttpContext.Current.TraceExt().TraceInfoExt, traceXml);
            TraceRequest(traceXml);
        }

        private void WriteTrace(IEnumerable<TraceContextInfo> traceContextInfos, XElement traceXml)
        {
            if (traceContextInfos == null) return;

            foreach (TraceContextInfo traceRecord in traceContextInfos)
            {
                traceXml.Add(
                    new XElement(
                        "TraceExtItem",
                        new XAttribute("Category", traceRecord.Category ?? string.Empty),
                        new XAttribute("IsWarning", traceRecord.IsWarning),
                        new XElement("ErrorInfo", traceRecord.Exception == null ? string.Empty : traceRecord.Exception.ToString()),
                        new XElement("Message", traceRecord.Message ?? string.Empty),
                        new XElement("LongFormFirst", traceRecord.LongFormFirst),
                        new XElement("LongFormLast", traceRecord.LongFormLast)));
            }

            if (HttpContext.Current.AllErrors != null && HttpContext.Current.AllErrors.Length > 0)
            {
                foreach (var error in HttpContext.Current.AllErrors)
                {
                    traceXml.Add(
                    new XElement(
                        "TraceExtItem",
                        new XAttribute("Category", string.Empty),
                        new XAttribute("IsWarning", "true"),
                        new XElement("ErrorInfo", error.ToString()),
                        new XElement("Message", error.Message),
                        new XElement("LongFormFirst", string.Empty),
                        new XElement("LongFormLast", string.Empty)));
                }
            }
        }

        private void TraceRequest(XElement traceXml)
        {
            var parametersXml = new XElement("Root");
            this.WriteParameters(parametersXml);

            WebInitializer.Initialize();
            using (var db = new DBTraceTimingRequestsDataContext(LogMonitor.CreateConnection()))
            {
                var page = string.Empty;
                var tableName = string.Empty;
                var pageType = string.Empty;
                var traceKey = (Guid?)HttpContext.Current.Items["TraceTimingRegeusts.TraceKey"];
                if (traceKey == null)
                    traceKey = Guid.NewGuid();
                long? id = null;
                var match = pageRegex.Match(HttpContext.Current.Request.Url.AbsolutePath);
                if (match.Success) page = match.Value;
                match = tableNameAndPageTypeRegex.Match(HttpContext.Current.Request.Url.AbsolutePath);
                if (match.Success)
                {
                    if (match.Groups["TableName"].Success)
                        tableName = match.Groups["TableName"].Value;
                    if (match.Groups["PageType"].Success)
                        pageType = match.Groups["PageType"].Value;
                }

                var personInfo = User.GetPersonInfo();
                db.P_LOG_InsertTraceTimingRequest(
                    page,
                    HttpContext.Current.Request.Url.ToString(),
                    HttpContext.Current.Timestamp,
                    (DateTime.Now.Ticks - HttpContext.Current.Timestamp.Ticks) / 10000,
                    User.GetSID(),
                    personInfo?.refRegion,
                    tableName,
                    HttpContext.Current.Request.QueryString["mode"],
                    pageType,
                    parametersXml,
                    traceXml,
                    traceKey,
                    ref id);
            }
        }

        private void WriteParameters(XElement parametersXml)
        {
            foreach (var key in HttpContext.Current.Request.QueryString.AllKeys)
            {
                parametersXml.Add(
                    new XElement("Url", new XAttribute("Key", key ?? string.Empty), new XAttribute("Value", HttpContext.Current.Request.QueryString[key] ?? string.Empty)));
            }

            if ("POST".Equals(HttpContext.Current.Request.RequestType, StringComparison.OrdinalIgnoreCase))
            {
                parametersXml.Add(
                    new XElement(
                        "Form",
                        new XAttribute("Key", "__EVENTTARGET"),
                        new XAttribute("Value", HttpContext.Current.Request.Form["__EVENTTARGET"] ?? string.Empty)));
                parametersXml.Add(
                    new XElement(
                        "Form",
                        new XAttribute("Key", "__EVENTARGUMENT"),
                        new XAttribute("Value", HttpContext.Current.Request.Form["__EVENTARGUMENT"] ?? string.Empty)));
            }

            if (HttpContext.Current.Request.UserHostAddress != null)
            {
                parametersXml.Add(
                    new XElement(
                        "Request", new XAttribute("Key", "IP"), new XAttribute("Value", HttpContext.Current.Request.UserHostAddress)));
            }

            parametersXml.Add(
                new XElement(
                    "PageEvents",
                    new XAttribute("Key", "OnInitTime"),
                    new XAttribute("Value", (this.dateTimeOnInit.Ticks - HttpContext.Current.Timestamp.Ticks) / 10000)));
        }

        private void WriteTrace(TraceContextEventArgs traceContextEventArgs, XElement traceXml)
        {
            foreach (TraceContextRecord traceRecord in traceContextEventArgs.TraceRecords)
            {
                traceXml.Add(
                    new XElement(
                        "TraceItem",
                        new XAttribute("Category", traceRecord.Category),
                        new XAttribute("IsWarning", traceRecord.IsWarning),
                        new XElement("ErrorInfo", traceRecord.ErrorInfo),
                        new XElement("Message", traceRecord.Message)));
            }

            var getData = HttpContext.Current.Trace.GetType().GetMethod("GetData", BindingFlags.NonPublic | BindingFlags.Instance);
            if (getData != null)
            {
                var data = getData.Invoke(HttpContext.Current.Trace, new object[0]);
                var ds = data as DataSet;
                if (ds != null && ds.Tables.Contains("Trace_Trace_Information"))
                {
                    var table = ds.Tables["Trace_Trace_Information"];
                    var traceData = new XElement("TraceData");
                    foreach (DataRow row in table.Rows)
                    {
                        var xmlRow = new XElement("Row");
                        foreach (DataColumn column in table.Columns)
                            xmlRow.Add(new XElement(column.ColumnName, row[column]));
                        traceData.Add(xmlRow);
                    }

                    traceXml.Add(traceData);
                }
            }
        }
    }
}