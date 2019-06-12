using System;
using System.Data.Common;
using System.Data;

namespace Nat.SqlDbInitializer.Wrappers
{
    /// <summary>
    /// Обертка для DbConnection с возможностью использования обернутого DbCommand (DbCommandWrapper)
    /// </summary>
    public class DbConnectionWrapper : DbConnection
    {
        private DbConnection _dbConn = null;
        public DbConnectionWrapper()
        {
            _dbConn = Nat.Tools.Specific.SpecificInstances.DbFactory.CreateConnection();
        }

        public string CommandTextAddStr { get; set; }

        #region wrap overrides

        private StateChangeEventHandler _stateChangeEventHandler;
        public override event StateChangeEventHandler StateChange
        {
            add
            {
                this._stateChangeEventHandler = (StateChangeEventHandler)Delegate.Combine(this._stateChangeEventHandler, value);
                _dbConn.StateChange += value;
            }
            remove
            {
                this._stateChangeEventHandler = (StateChangeEventHandler)Delegate.Remove(this._stateChangeEventHandler, value);
                _dbConn.StateChange -= value;
            }
        }

        public override string ServerVersion => _dbConn.ServerVersion;
        public override string DataSource => _dbConn.DataSource;
        public override string Database => _dbConn.Database;
        public override int ConnectionTimeout => _dbConn.ConnectionTimeout;
        public override string ConnectionString
        {
            get { return _dbConn.ConnectionString; }
            set { _dbConn.ConnectionString = value; }
        }
        public override ConnectionState State => _dbConn.State;

        public override void ChangeDatabase(string databaseName)
        {
            _dbConn.ChangeDatabase(databaseName);
        }

        public override void Close()
        {
            _dbConn.Close();
        }

        public override void Open()
        {
            _dbConn.Open();
        }

        protected override DbTransaction BeginDbTransaction(IsolationLevel isolationLevel)
        {
            return _dbConn.BeginTransaction(isolationLevel);
        }

        protected override void OnStateChange(StateChangeEventArgs stateChange)
        {
            StateChangeEventHandler stateChangeEventHandler = this._stateChangeEventHandler;
            if (stateChangeEventHandler != null)
            {
                stateChangeEventHandler(this, stateChange);
            }
        }

        #endregion

        protected override DbCommand CreateDbCommand()
        {
            return string.IsNullOrEmpty(CommandTextAddStr) ? _dbConn.CreateCommand() : new DbCommandWrapper(_dbConn, CommandTextAddStr);
        }
    }
}
