/*
 * Created by : Daniil Kovalev
 * Created    : 20.03.2008
 */

using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;
using System.Security.Principal;
using System.Web.Security;
using Nat.Tools.Constants;
using Nat.Tools.ResourceTools;
using Nat.Tools.Specific;
using Nat.Web.Controls.Properties;
using Nat.Web.Tools;
using Nat.Web.Tools.Initialization;
using System.Web;
using Nat.Web.Tools.Security;

namespace Nat.Web.Controls
{
    using System.Data.Linq;
    using System.IO;
    using System.Xml.Linq;

    using Nat.Web.Controls.Log;

    public delegate LogMessageEntry LogDelegate();

    public class LogMonitor : ILogMonitor
    {
        #region Fields

        private readonly List<LogChangedFieldEntry> _changedFieldList;
        private DbCommand _cmdLog;
        private DbCommand _cmdLogFields;

        #endregion

        #region Constructor

        public LogMonitor()
        {
            _changedFieldList = new List<LogChangedFieldEntry>();
        }

        #endregion

        #region Enumerations

        [Flags]
        private enum LogResult
        {
            /// <summary>
            /// Operation has done successfully
            /// </summary>
            Succeed = 0x01,

            /// <summary>
            /// Could not find user SID in ULS_RecordCards
            /// </summary>
            UserNotFound = 0x02,

            /// <summary>
            /// Log is disabled for this message code
            /// </summary>
            LogDisabled = 0x04,

            /// <summary>
            /// Could not find ID associated with given code
            /// </summary>
            CodeNotFound = 0x08,

            /// <summary>
            /// There are more than one RecordCard which correspond to one SID
            /// </summary>
            ManyRecordCardToSID = 0x10
        }

        #endregion

        #region Methods

        public void Init()
        {
            _cmdLog = SpecificInstances.DbFactory.CreateCommand();
            if (_cmdLog == null)
                throw new NullReferenceException("Method 'SpecificInstances.DbFactory.CreateCommand()' return null");
            if (Transaction == null)
            {
                _cmdLog.Connection = SpecificInstances.DbFactory.CreateConnection();
            }
            else
            {
                _cmdLog.Connection = Transaction.Connection;
                _cmdLog.Transaction = Transaction;
            }
            _cmdLog.CommandType = CommandType.StoredProcedure;
            _cmdLog.CommandText = "log";
            _cmdLog.Parameters.Add(new SqlParameter("@code", null));
            _cmdLog.Parameters.Add(new SqlParameter("@SID", null));
            //_cmdLog.Parameters.Add(new SqlParameter("@refRecordCard", null));
            _cmdLog.Parameters.Add(new SqlParameter("@content", SqlDbType.NVarChar));
            _cmdLog.Parameters.Add(new SqlParameter("@ClientIPAddress", SqlDbType.NVarChar));
            _cmdLog.Parameters.Add(new SqlParameter("@refRVSProperties", SqlDbType.BigInt));
            
            var parameter = new SqlParameter
                                {
                                    ParameterName = "@result",
                                    Direction = ParameterDirection.ReturnValue,
                                };
            _cmdLog.Parameters.Add(parameter);

            _cmdLogFields = SpecificInstances.DbFactory.CreateCommand();
            if (_cmdLogFields == null)
                throw new NullReferenceException("Method 'SpecificInstances.DbFactory.CreateCommand()' return null");
            if(Transaction == null)
                _cmdLogFields.Connection = SpecificInstances.DbFactory.CreateConnection();
            else
            {
                _cmdLogFields.Connection = Transaction.Connection;
                _cmdLogFields.Transaction = Transaction;
            }
            _cmdLogFields.CommandType = CommandType.Text;
            _cmdLogFields.CommandText =
                @"
                INSERT INTO LOG_CHANGED_FIELDS(tableName, fieldName, oldValue, newValue, refMessage)
                VALUES(@tableName, @fieldName, @oldValue, @newValue, @refMessage)
            ";
            _cmdLogFields.Parameters.Add(new SqlParameter("@tableName", SqlDbType.NVarChar));
            _cmdLogFields.Parameters.Add(new SqlParameter("@fieldName", SqlDbType.NVarChar));
            _cmdLogFields.Parameters.Add(new SqlParameter("@oldValue", null));
            _cmdLogFields.Parameters.Add(new SqlParameter("@newValue", null));
            _cmdLogFields.Parameters.Add(new SqlParameter("@refMessage", SqlDbType.BigInt));
        }

        public void RowChanged(DataTable table, DataColumnCollection columns, object[] oldValues, object[] newValues)
        {
            Boolean isRowEntityFound = false;
            String rowId = null;

            for (int i = 0; i < columns.Count; i++)
            {
                var isRowEntity =
                    (Boolean)
                    (DataSetResourceManager.GetColumnExtProperty(columns[i], ColumnExtProperties.LOG_ENTITY) ?? false);
                if (isRowEntity)
                {
                    rowId = Convert.ToString(oldValues[i]);
                    isRowEntityFound = true;
                }
            }

            if (!isRowEntityFound)
            {
                DataColumn identityColumn = columns["id"];
                if (identityColumn != null)
                    rowId = Convert.ToString(oldValues[identityColumn.Ordinal]);
            }

            String tableCaption = DataSetResourceManager.GetTableExtProperty(table, TableExtProperties.CAPTION,
                                                                             table.TableName);

            String rowEntity = String.Format("{0}, {1}", tableCaption, rowId);

            for (int i = 0; i < columns.Count; i++)
            {
                var logEnabled =
                    (Boolean)
                    (DataSetResourceManager.GetColumnExtProperty(columns[i], ColumnExtProperties.LOG_ENABLED) ?? true);
                object oldValue;
                object newValue;
                if (columns[i].MaxLength < 255)
                {
                    oldValue = oldValues[i];
                    newValue = newValues[i];
                }
                else
                {
                    oldValue = Resources.SViewNotAvailable;
                    newValue = Resources.SViewNotAvailable;
                }
                if (logEnabled && !oldValue.Equals(newValue))
                {
                    var caption =
                        (String)
                        (DataSetResourceManager.GetColumnExtProperty(columns[i], ColumnExtProperties.CAPTION) ??
                         columns[i].Caption);
                    FieldChanged(table.TableName, rowEntity, caption, oldValue, newValue);
                }
            }
        }

        public void DefineRowEntity(string tableName, string rowEntity)
        {
            foreach (LogChangedFieldEntry changedFieldEntry in _changedFieldList)
            {
                if (changedFieldEntry.TableName.Equals(tableName))
                {
                    changedFieldEntry.RowEntity = rowEntity;
                }
            }
        }

        public void Log(ILogMessageEntry logMessageEntry)
        {
            WriteLog(logMessageEntry);
        }

        public long? WriteLog(ILogMessageEntry logMessageEntry)
        {
            long? refMessage = null;
            if (EnabledLogTable.EnabledLog(logMessageEntry.MessageCodeAsLong, Transaction))
                refMessage = WriteToDataBase(logMessageEntry);
            ChangedFieldList.Clear();
            
            return refMessage;
        }

        public void Log(long messageCode, Func<ILogMessageEntry> log)
        {
            WriteLog(messageCode, log);
        }

        public long? WriteLog(long messageCode, Func<ILogMessageEntry> log)
        {
            long? refMessage = null;
            if (EnabledLogTable.EnabledLog(messageCode, Transaction))
            {
                var logMessageEntry = log();
                if (logMessageEntry.MessageCodeAsLong == 0)
                    logMessageEntry.MessageCodeAsLong = messageCode;
                refMessage = WriteToDataBase(logMessageEntry);
            }

            ChangedFieldList.Clear();
            
            return refMessage;
        }

        public long? WriteLog(LogMessageType messageCode, Func<ILogMessageEntry> log)
        {
            long? refMessage = null;
            if (EnabledLogTable.EnabledLog((long)messageCode, Transaction))
            {
                var logMessageEntry = log();
                if (logMessageEntry.MessageCodeAsLong == 0)
                    logMessageEntry.MessageCodeAsLong = (long)messageCode;
                refMessage = WriteToDataBase(logMessageEntry);
            }

            ChangedFieldList.Clear();
            
            return refMessage;
        }

        public void Log(LogMessageType messageCode, LogDelegate log)
        {
            Log((long) messageCode, () => log());
        }

        public void FieldChanged(string tableName, string rowEntity, string fieldName, object oldValue, object newValue)
        {
            if (oldValue is Binary || newValue is Binary)
                return;

            var logChangedFieldEntry = new LogChangedFieldEntry(
                tableName, rowEntity, fieldName, oldValue, newValue);
            ChangedFieldList.Add(logChangedFieldEntry);
        }

        public void FieldChanged(string rowEntity, string fieldName, object oldValue, object newValue)
        {
            if (oldValue is Binary || newValue is Binary)
                return;
            
            var logChangedFieldEntry = new LogChangedFieldEntry(
                null, rowEntity, fieldName, oldValue, newValue);
            ChangedFieldList.Add(logChangedFieldEntry);
        }

        public void ClearChangedFieldList()
        {
            ChangedFieldList.Clear();
        }

        private long? WriteToDataBase(ILogMessageEntry logMessageEntry)
        {
            var openConnection = false;
            try
            {
                if (_cmdLog.Connection.State != ConnectionState.Open)
                {
                    openConnection = true;
                    _cmdLog.Connection.Open();
                    if (_cmdLog.Connection != _cmdLogFields.Connection)
                        _cmdLogFields.Connection.Open();
                }

                _cmdLog.Parameters["@code"].Value = (LogMessageType)logMessageEntry.MessageCodeAsLong;
                _cmdLog.Parameters["@SID"].Value = logMessageEntry.Sid ?? Sid ?? User.GetSID(false);
                _cmdLog.Parameters["@content"].Value = logMessageEntry.Message;

                try
                {
                    _cmdLog.Parameters["@ClientIPAddress"].Value = HttpContext.Current == null ? "local" : HttpContext.Current.Request.UserHostAddress;
                }
                catch (HttpException e)
                {
                    _cmdLog.Parameters["@ClientIPAddress"].Value = "local";
                }
                catch (ArgumentException e)
                {
                    _cmdLog.Parameters["@ClientIPAddress"].Value = "local";
                }
                _cmdLog.Parameters["@refRVSProperties"].Value = logMessageEntry.RefRVSProperties;
                
                var value = _cmdLog.ExecuteScalar();
                var result = (LogResult) _cmdLog.Parameters["@result"].Value;

                if ((result & LogResult.CodeNotFound) != 0)
                    throw new Exception(String.Format("Could not write log entry. Message code '{0}' not found",
                                                      (LogMessageType)logMessageEntry.MessageCodeAsLong));

                if ((result & LogResult.ManyRecordCardToSID) != 0)
                    //throw new Exception(String.Format("Could not write log entry. There are more than one record cards which correspond to SID '{0}'", logMessageEntry.Sid));
                    throw new Exception(String.Format(Resources.SLogErrorMultipleUsers, logMessageEntry.Sid));

                if ((result & LogResult.Succeed) != 0)
                {
                    foreach (LogChangedFieldEntry entry in ChangedFieldList)
                        WriteFieldChanged(Convert.ToInt64(value), entry.RowEntity, entry.FieldName, entry.OldValue, entry.NewValue);

                    return Convert.ToInt64(value);
                }
            }
            finally
            {
                if (openConnection)
                {
                    _cmdLog.Connection.Close();
                    if (_cmdLog.Connection != _cmdLogFields.Connection)
                        _cmdLogFields.Connection.Close();
                }
            }

            return null;
        }

        public void WriteFieldChanged(long refMessage, string rowEntity, string fieldName, object oldValue, object newValue)
        {
            var openConnection = false;
            try
            {
                if (_cmdLogFields.Connection.State != ConnectionState.Open)
                {
                    openConnection = true;
                    _cmdLogFields.Connection.Open();
                }

                _cmdLogFields.Parameters["@tableName"].Value = rowEntity;
                _cmdLogFields.Parameters["@fieldName"].Value = fieldName;
                _cmdLogFields.Parameters["@oldValue"].Value = GetValue(oldValue);
                _cmdLogFields.Parameters["@newValue"].Value = GetValue(newValue);
                _cmdLogFields.Parameters["@refMessage"].Value = refMessage;
                _cmdLogFields.ExecuteNonQuery();
            }
            finally
            {
                if (openConnection)
                    _cmdLogFields.Connection.Close();
            }
        }

        public void WriteFieldChanged(long refMessage)
        {
            var openConnection = false;
            try
            {
                if (_cmdLogFields.Connection.State != ConnectionState.Open)
                {
                    openConnection = true;
                    _cmdLogFields.Connection.Open();
                }
                
                var data = new DS.LogFieldChangedDataTable();
                var table = new DataTable(data.TableName);
                using (var stream = new MemoryStream())
                {
                    data.WriteXmlSchema(stream);
                    stream.Flush();
                    stream.Position = 0;
                    table.ReadXmlSchema(stream);
                }

                foreach (var row in ChangedFieldList)
                    data.AddLogFieldChangedRow(refMessage, row.RowEntity, row.FieldName, (string)GetValue(row.OldValue), (string)GetValue(row.NewValue));

                foreach (var row in data)
                    table.Rows.Add(row.ItemArray);

                using (var command = _cmdLogFields.Connection.CreateCommand())
                {
                    var parameter = command.CreateParameter();
                    parameter.ParameterName = "@data";
                    parameter.Value = table;
                    command.Parameters.Add(parameter);
                    command.CommandText = "LOG_P_BulkInsertLogFieldChanged";
                    command.CommandType = CommandType.StoredProcedure;
                    command.ExecuteNonQuery();
                }

                ChangedFieldList.Clear();
            }
            finally
            {
                if (openConnection)
                    _cmdLogFields.Connection.Close();
            }
        }

        private static object GetValue(object oldValue)
        {
            var xmlElement = oldValue as XElement;
            if (xmlElement != null)
                return xmlElement.ToString();
            return oldValue ?? string.Empty;
        }

        public static void SyncSidAndNames()
        {
            WebInitializer.Initialize();
            #region Create connections, commands
            
            var connectionSelect = SpecificInstances.DbFactory.CreateConnection();
            var connectionUpdate = SpecificInstances.DbFactory.CreateConnection();
            var cSelect = SpecificInstances.DbFactory.CreateCommand();
            var cUpdate = SpecificInstances.DbFactory.CreateCommand();
            var nameParameter = SpecificInstances.DbFactory.CreateParameter();
            var idParameter = SpecificInstances.DbFactory.CreateParameter();

            if (cSelect == null || cUpdate == null)
                throw new NullReferenceException("Method 'SpecificInstances.DbFactory.CreateCommand()' return null");
            if (connectionSelect == null || connectionUpdate == null)
                throw new NullReferenceException("Method 'SpecificInstances.DbFactory.CreateConnection()' return null");
            if (nameParameter == null || idParameter == null)
                throw new NullReferenceException("Method 'SpecificInstances.DbFactory.CreateParameter()' return null");

            #endregion

            cSelect.CommandType = CommandType.Text;
            cSelect.CommandText = "select id, sid from LOG_SidIdentification";
            cSelect.Connection = connectionSelect;

            cUpdate.CommandType = CommandType.Text;
            cUpdate.CommandText = "update LOG_SidIdentification set name = @name where id = @id";
            cUpdate.Connection = connectionUpdate;
            nameParameter.DbType = DbType.String;
            nameParameter.ParameterName = "name";
            idParameter.DbType = DbType.Int64;
            idParameter.ParameterName = "id";
            cUpdate.Parameters.Add(idParameter);
            cUpdate.Parameters.Add(nameParameter);

            var provider = Membership.Providers["LogMonitorADProvider"];
            if (provider == null) throw new NullReferenceException("LogMonitorADProvider is not set");
            try
            {
                connectionSelect.Open();
                connectionUpdate.Open();
                var reader = cSelect.ExecuteReader();
                while (reader.Read())
                {
                    var sid = reader.GetString(1);
                    var user = provider.GetUser(new SecurityIdentifier(sid), false);
                    if (user != null)
                    {
                        nameParameter.Value = user.UserName;
                        idParameter.Value = reader.GetInt64(0);
                        cUpdate.ExecuteNonQuery();
                    }
                }
            }
            finally
            {
                connectionSelect.Close();
                connectionUpdate.Close();
            }
        }

        public static void EnabledChanged()
        {
            EnabledLogTable.NeedReload();
        }

        /// <summary>
        /// Результат логирования ссылка на лог в журнале событий.
        /// </summary>
        /// <param name="e">Произошедшее исключение, которое нужно поместить в журнал событий.</param>
        /// <returns>Ссылка на запись журнала событий.</returns>
        public string LogException(Exception e)
        {
            var logid = WriteLog(
                new BaseLogMessageEntry
                    {
                        MessageCodeAsLong = (long)LogMessageType.SystemErrorInApp,
                        Message = e.ToString(),
                        DateTime = DateTime.Now,
                    });

            return string.Format("<a href=\"/MainPage.aspx/data/LOG_MESSAGESEdit/read?refLOG_MESSAGES={0}\">{1}</a>", logid, e.Message);
        }

        /// <summary>
        /// Результат логирования ссылка на лог в журнале событий.
        /// </summary>
        /// <param name="e">Произошедшее исключение, которое нужно поместить в журнал событий.</param>
        /// <returns>Ссылка на запись журнала событий.</returns>
        /// <param name="sid">SID пользователя от имени которого нужно произвести логирование.</param>
        public string LogException(Exception e, string sid)
        {
            var logid = WriteLog(
                new BaseLogMessageEntry
                    {
                        MessageCodeAsLong = (long)LogMessageType.SystemErrorInApp,
                        Message = e.ToString(),
                        DateTime = DateTime.Now,
                        Sid = sid,
                    });

            return string.Format("<a href=\"/MainPage.aspx/data/LOG_MESSAGESEdit/read?refLOG_MESSAGES={0}\">{1}</a>", logid, e.Message);
        }

        public static string LogError(Exception e)
        {
            var monitor = InitializerSection.GetSection().LogMonitor;
            monitor.Init();
            return monitor.LogException(e);
        }

        #endregion

        #region Properties

        internal List<LogChangedFieldEntry> ChangedFieldList
        {
            get { return _changedFieldList; }
        }

        private DbTransaction _transaction;
        public DbTransaction Transaction
        {
            get { return _transaction; }
            set
            {
                _transaction = value;
                if (_cmdLog != null)
                {
                    if(Transaction != null)
                    {
                        _cmdLog.Transaction = Transaction;
                        _cmdLog.Connection = Transaction.Connection;
                        _cmdLogFields.Transaction = Transaction;
                        _cmdLogFields.Connection = Transaction.Connection;
                    }
                    else
                    {
                        _cmdLog.Transaction = null;
                        _cmdLogFields.Transaction = null;
                        _cmdLog.Connection = SpecificInstances.DbFactory.CreateConnection();
                        _cmdLogFields.Connection = SpecificInstances.DbFactory.CreateConnection();
                    }
                }
            }
        }

        public string Sid { get; set; }

        #endregion

        private class EnabledLogTable : DataTable
        {
            private const string CodeColumn = "code";
            private const string EnabledColumn = "enabled";
            private static readonly EnabledLogTable Table = new EnabledLogTable();
            private bool _needReload = true;

            private EnabledLogTable()
            {
                Columns.Add(new DataColumn(CodeColumn, typeof (long)) {Unique = true});
                Columns.Add(new DataColumn(EnabledColumn, typeof(bool)));
                PrimaryKey = new [] {Columns[CodeColumn]};
            }

            public static bool EnabledLog(long code, DbTransaction transaction = null)
            {
                lock (Table)
                    if (Table._needReload) Table.ReloadData(transaction);
                var find = Table.Rows.Find(code);
                if (find != null) return (bool)find[EnabledColumn];
                return true;
            }

            public static void NeedReload()
            {
                lock (Table) Table._needReload = true;
            }

            private void ReloadData(DbTransaction transaction)
            {
                WebInitializer.Initialize();
                DbConnection connection;
                var command = SpecificInstances.DbFactory.CreateCommand();
                var adapter = SpecificInstances.DbFactory.CreateDataAdapter();
                if (command == null)
                    throw new NullReferenceException("Method 'SpecificInstances.DbFactory.CreateCommand()' return null");
                if (adapter == null)
                    throw new NullReferenceException("Method 'SpecificInstances.DbFactory.CreateDataAdapter()' return null");
                if (transaction == null)
                {
                    connection = SpecificInstances.DbFactory.CreateConnection();
                    if (connection == null)
                        throw new NullReferenceException("Method 'SpecificInstances.DbFactory.CreateConnection()' return null");
                }
                else
                {
                    connection = transaction.Connection;
                    command.Transaction = transaction;
                }
                command.Connection = connection;
                command.CommandText = "select code, enabled from DIC_LOG_MESSAGE_SOURCE_TO_TYPE";
                adapter.SelectCommand = command;
                if(transaction == null)
                    connection.Open();
                try
                {
                    Clear();
                    adapter.Fill(this);
                    _needReload = false;
                }
                finally
                {
                    if (transaction == null)
                        connection.Close();
                }
            }
        }
    }
}