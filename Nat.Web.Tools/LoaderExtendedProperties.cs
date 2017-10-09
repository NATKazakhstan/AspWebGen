using System.Data;
using System.Data.SqlClient;
using System.Web.Configuration;
using Microsoft.SqlServer.Management.Common;
using Microsoft.SqlServer.Management.Smo;
using SMO = Microsoft.SqlServer.Management.Smo;

namespace Nat.Web.Tools
{
    public static class LoaderExtendedProperties
    {
        private static SqlConnectionStringBuilder builder;
//        private static Server server = null;
//
//        public static Server Server
//        {
//            get
//            {
//                if (server == null) server = GetServer();
//                return server;
//            }
//        }

        private static Server GetServer()
        {
            if (builder == null)
            {
                builder = new SqlConnectionStringBuilder(
                    WebConfigurationManager.ConnectionStrings["ASPNETConnectionStringLocal"].ConnectionString);                
            }
            return new Server(new ServerConnection(new SqlConnection(builder.ConnectionString)));
        }

        public static void SetExtendedProperties(DataSet ds)
        {            
            Database database = GetServer().Databases[builder.InitialCatalog];
            foreach (DataTable dataTable in ds.Tables)
                SetExtendedProperties(dataTable, database);
        }

        public static void SetExtendedProperties(DataTable dataTable)
        {
            Database database = GetServer().Databases[builder.InitialCatalog];
            SetExtendedProperties(dataTable, database);
        }

        private static void SetExtendedProperties(DataTable dataTable, Database database)
        {
            if (database.Tables.Contains(dataTable.TableName))
                SetExtendedProperies(dataTable, database.Tables[dataTable.TableName]);
            else if(database.Views.Contains(dataTable.TableName))
                SetExtendedProperies(dataTable, database.Views[dataTable.TableName]);
        }

        private static void SetExtendedProperies(DataTable dataTable, TableViewBase view)
        {
            foreach (ExtendedProperty property in view.ExtendedProperties)
                dataTable.ExtendedProperties[property.Name] = property.Value;
            foreach (DataColumn dataColumn in dataTable.Columns)
            {
                if (view.Columns.Contains(dataColumn.ColumnName))
                {
                    foreach (ExtendedProperty property in view.Columns[dataColumn.ColumnName].ExtendedProperties)
                        dataColumn.ExtendedProperties[property.Name] = property.Value;                    
                }                
            }
        }
    }
}