using System.Data.Common;

namespace Nat.SqlDbInitializer.Wrappers
{
    public static class WrapperExtensions
    {
        public static void SetCommandParamToWrappedConn(this DbConnection conn, DbCommandParams commandParam)
        {
            if (conn is DbConnectionWrapper wrap)
            {
                wrap.DbCommandParam = commandParam;
            }
        }
    }
}
