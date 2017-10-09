namespace Nat.SqlDbInitializer
{
    using System;
    using System.Configuration;
    using System.Data.Common;
    using System.Data.SqlClient;
    using System.Security;
    using System.Security.Permissions;
    using System.Web.Configuration;

    using Nat.Web.Tools;

    public class SqlClientFactoryConnectionSpecified : DbProviderFactory, IWebConfiguration
    {
        public static readonly SqlClientFactoryConnectionSpecified Instance;
        private static readonly object _lock = new object();
        private Configuration webConfiguration;

        static SqlClientFactoryConnectionSpecified()
        {
            Instance = new SqlClientFactoryConnectionSpecified();
        }

        public override bool CanCreateDataSourceEnumerator
        {
            get { return SqlClientFactory.Instance.CanCreateDataSourceEnumerator; }
        }

        public Configuration WebConfiguration
        {
            get
            {
                lock (_lock)
                    if (webConfiguration == null)
                        webConfiguration = WebConfigurationManager.OpenWebConfiguration("~/");
                return webConfiguration;
            }
            set { webConfiguration = value; }
        }

        public string ConnectionString { get; set; }

        private string GetConnectionString()
        {
            string connectionString = ConnectionString;
            if (connectionString == null)
            {
                var section = DbInitializerSection.GetSection(WebConfiguration);
                connectionString = WebConfiguration.ConnectionStrings.ConnectionStrings[section.ConnectionStringName].ConnectionString;
            }

            return connectionString;
        }

        public override DbCommand CreateCommand()
        {
            var command = SqlClientFactory.Instance.CreateCommand();
            command.CommandTimeout = 45;
            return command;
        }

        public override DbConnection CreateConnection()
        {
            DbConnection con = SqlClientFactory.Instance.CreateConnection();
            con.ConnectionString = GetConnectionString();
            try
            {
                con.Open();
            }
            catch
            {
                throw new Exception("DbInitializer. Could not open connection");
            }
            finally
            {
                con.Close();
            }
            return con;
        }

        public override DbCommandBuilder CreateCommandBuilder()
        {
            return SqlClientFactory.Instance.CreateCommandBuilder();
        }

        public override DbConnectionStringBuilder CreateConnectionStringBuilder()
        {
            var builder = SqlClientFactory.Instance.CreateConnectionStringBuilder();
            builder.ConnectionString = GetConnectionString();
            return builder;
        }

        public override DbDataAdapter CreateDataAdapter()
        {
            return SqlClientFactory.Instance.CreateDataAdapter();
        }

        public override DbDataSourceEnumerator CreateDataSourceEnumerator()
        {
            return SqlClientFactory.Instance.CreateDataSourceEnumerator();
        }

        public override DbParameter CreateParameter()
        {
            return SqlClientFactory.Instance.CreateParameter();
        }

        public override CodeAccessPermission CreatePermission(PermissionState state)
        {
            return SqlClientFactory.Instance.CreatePermission(state);
        }
    }
}