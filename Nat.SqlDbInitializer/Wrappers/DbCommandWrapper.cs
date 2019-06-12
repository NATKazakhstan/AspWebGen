using System;
using System.Data.Common;
using System.Data;

namespace Nat.SqlDbInitializer.Wrappers
{
    /// <summary>
    /// Обертка для DbCommand с возможностью модификации SQL запроса перед ExecuteReader()
    /// </summary>
    public class DbCommandWrapper : DbCommand
    {
        private DbCommand _command = null;
        private DbConnection _connection = null;
        private DbTransaction _transaction = null;

        private string _commandTextAddStr { get; set; }

        public DbCommandWrapper(DbConnection conn, string commandTextAddStr) : base()
        {
            _commandTextAddStr = commandTextAddStr;
            _command = conn.CreateCommand();
            Connection = _command.Connection;
            _connection = _command.Connection;
            Transaction = _command.Transaction;
            _transaction = _command.Transaction;
        }

        public new DbParameterCollection Parameters => _command.Parameters;

        #region wrap overrides

        public override bool DesignTimeVisible
        {
            get { return _command.DesignTimeVisible; }
            set { _command.DesignTimeVisible = value; }
        }

        public override CommandType CommandType
        {
            get { return _command.CommandType; }
            set { _command.CommandType = value; }
        }

        public override int CommandTimeout
        {
            get { return _command.CommandTimeout; }
            set { _command.CommandTimeout = value; }
        }

        public override string CommandText
        {
            get { return _command.CommandText; }
            set { _command.CommandText = value; }
        }

        public override UpdateRowSource UpdatedRowSource
        {
            get { return _command.UpdatedRowSource; }
            set { _command.UpdatedRowSource = value; }
        }

        protected override DbConnection DbConnection
        {
            get { return _connection; }
            set { _connection = value; }
        }

        protected override DbParameterCollection DbParameterCollection => Parameters;

        protected override DbTransaction DbTransaction
        {
            get { return _transaction; }
            set { _transaction = value; }
        }

        public override void Cancel()
        {
            _command.Cancel();
        }

        public override int ExecuteNonQuery()
        {
            return _command.ExecuteNonQuery();
        }

        public override object ExecuteScalar()
        {
            return _command.ExecuteScalar();
        }

        public override void Prepare()
        {
            _command.Prepare();
        }

        protected override DbParameter CreateDbParameter()
        {
            return _command.CreateParameter();
        }

        #endregion

        protected override DbDataReader ExecuteDbDataReader(CommandBehavior behavior)
        {
            if (!string.IsNullOrEmpty(_command.CommandText) && !string.IsNullOrEmpty(_commandTextAddStr))
                _command.CommandText += Environment.NewLine + _commandTextAddStr;
            return _command.ExecuteReader(behavior);
        }
    }
}
