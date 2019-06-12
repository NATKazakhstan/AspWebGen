using System.Data.Common;

namespace Nat.SqlDbInitializer.Wrappers
{
    public static class WrapperExtensions
    {
        public static void SetCommandTextAddToWrappedConn(this DbConnection conn, string commandTextAddStr)
        {
            if (conn is DbConnectionWrapper)
            {
                (conn as DbConnectionWrapper).CommandTextAddStr = commandTextAddStr;
            }
        }
    }
}
