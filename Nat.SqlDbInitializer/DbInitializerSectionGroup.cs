using System.Configuration;

namespace Nat.SqlDbInitializer
{
    public class DbInitializerSectionGroup : ConfigurationSectionGroup
    {
        [ConfigurationProperty("DbInitializer")]
        public DbInitializerSection DbInitializer
        {
            get { return (DbInitializerSection)base.Sections["DbInitializer"]; }
        }
    }
}