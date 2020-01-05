/*
 * Created by : Daniil Kovalev
 * Created    : 07.02.2008
 */

using System;
using System.Collections.Generic;
using System.Data;
using System.Web.UI;
using Nat.Web.Tools;
using Nat.Web.Tools.Security;

// ReSharper disable once CheckNamespace
namespace Nat.Web.Controls
{
    public class LogMonitorWeb : Control, ILogMonitor
    {
        #region Fields

        private LogMonitor logMonitor;

        #endregion

        public LogMonitor LogMonitor
        {
            get { return logMonitor; }
        }

        #region Methods

        protected override void OnInit(EventArgs e)
        {
            base.OnInit(e);
            logMonitor = new LogMonitor();
            logMonitor.Init();
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            logMonitor.ChangedFieldList.AddRange(ChangedFieldList);
        }

        protected override void OnPreRender(EventArgs e)
        {
            base.OnPreRender(e);
            ChangedFieldList = logMonitor.ChangedFieldList;
        }

        public void RowChanged(DataTable table, DataColumnCollection columns, Object[] oldValues, Object[] newValues)
        {
            logMonitor.RowChanged(table, columns, oldValues, newValues);
        }

        public void DefineRowEntity(String tableName, String rowEntity)
        {
            logMonitor.DefineRowEntity(tableName, rowEntity);
        }

        public string Sid
        {
            get { return LogMonitor.Sid; }
            set { LogMonitor.Sid = value; }
        }

        public void Log(ILogMessageEntry logMessageEntry)
        {
            logMessageEntry.Sid = User.GetSID();
            logMonitor.Log(logMessageEntry);
        }

        public void Log(long messageCode, Func<ILogMessageEntry> log)
        {
            logMonitor.Log(messageCode,
                           delegate
                               {
                                   var logMessageEntry = log();
                                   logMessageEntry.Sid = User.GetSID();
                                   return logMessageEntry;
                               });
        }
        
        public void Log(LogMessageType messageCode, LogDelegate log)
        {
            logMonitor.Log(messageCode,
                           delegate
                               {
                                   var logMessageEntry = log();
                                   logMessageEntry.Sid = User.GetSID();
                                   return logMessageEntry;
                               });
        }

        public long? WriteLog(ILogMessageEntry logMessageEntry)
        {
            return logMonitor.WriteLog(logMessageEntry);
        }

        public long? WriteLog(long messageCode, Func<ILogMessageEntry> log)
        {
            return logMonitor.WriteLog(messageCode, log);
        }

        public void FieldChanged(string rowEntity, string fieldName, object oldValue, object newValue)
        {
            logMonitor.FieldChanged(rowEntity, fieldName, oldValue, newValue);
        }

        public void FieldChanged(String tableName, String rowEntity, String fieldName, Object oldValue, Object newValue)
        {
            logMonitor.FieldChanged(tableName, rowEntity, fieldName, oldValue, newValue);
        }

        public void WriteFieldChanged(long refMessage, string rowEntity, string fieldName, object oldValue, object newValue)
        {
            logMonitor.WriteFieldChanged(refMessage, rowEntity, fieldName, oldValue, newValue);
        }

        public void WriteFieldChanged(Int64 refMessage)
        {
            logMonitor.WriteFieldChanged(refMessage);
        }

        public void ClearChangedFieldList()
        {
            logMonitor.ClearChangedFieldList();
        }

        public string LogException(Exception e)
        {
            return logMonitor.LogException(e);
        }

        public string LogException(Exception e, string sid)
        {
            return logMonitor.LogException(e, sid);
        }

        public void MessageSourceLink(long messageSourceId, long recordId)
        {
            logMonitor.MessageSourceLink(messageSourceId, recordId);
        }

        public void WriteMessageSourceLink(long refMessage)
        {
            logMonitor.WriteMessageSourceLink(refMessage);
        }

        #endregion


        #region Properties

        private List<LogChangedFieldEntry> ChangedFieldList
        {
            get
            {
                if(ViewState["ChangedFieldList"] == null)
                    ViewState["ChangedFieldList"] = new List<LogChangedFieldEntry>();

                return (List<LogChangedFieldEntry>)ViewState["ChangedFieldList"];
            }
            set { ViewState["ChangedFieldList"] = value; }
        }

        #endregion

        #region ILogMonitor Members

        public new void Init()
        {
            logMonitor = new LogMonitor();
            logMonitor.Init();
        }

        #endregion
    }
}