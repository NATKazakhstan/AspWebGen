using System;
using System.Data.SqlTypes;
using Nat.Tools.Specific;

namespace Nat.SqlDbInitializer
{
    public class SqlDbConstants : IDbConstants
    {
        public static readonly SqlDbConstants Instance;

        private static readonly DateTime maxDateTime = SqlDateTime.MaxValue.Value;
        private static readonly DateTime minDateTime = SqlDateTime.MinValue.Value;

        static SqlDbConstants()
        {
            Instance = new SqlDbConstants();
        }


        #region IDbConstants Members

        public DateTime MaxDate
        {
            get { return maxDateTime; }
        }

        public string StringConcatChar
        {
            get { return "+"; }
        }

        public DateTime MaxDateTime
        {
            get { return maxDateTime; }
        }

        public DateTime MinDate
        {
            get { return minDateTime; }
        }

        public DateTime NullDateTime
        {
            get { return minDateTime; }
        }


        public string SqlParameterPrefix
        {
            get { return "@"; }
        }
        
		public string CatalogSeparator
        {
            get { return "."; }
        }

        #endregion
    }
}