using System.Configuration;

namespace Nat.Web.Tools.Initialization
{
    public class InitializerSectionGroup : ConfigurationSectionGroup
    {
        [ConfigurationProperty("Initializer")]
        public InitializerSection Initializers
        {
            get { return (InitializerSection)base.Sections["Initializer"]; }
        }
    }
}