using System;
using System.ComponentModel;
using System.ComponentModel.Design;
using System.Web.Compilation;
using System.Web.Configuration;
using System.Web.Security;

namespace Nat.Web.Controls.Security
{
    public class ActionConverter : StringConverter
    {
        public override StandardValuesCollection GetStandardValues(ITypeDescriptorContext context)
        {
            if (context == null) 
                return new StandardValuesCollection(new string[0]);
            var service = (IDesignerHost)context.GetService(typeof(IDesignerHost));
            if (service == null) 
                return new StandardValuesCollection(new string[0]);

            var roleManager = (RoleManagerSection)WebConfigurationManager.GetSection("system.web/roleManager");
            var roleProviderSection = roleManager.Providers[roleManager.DefaultProvider];
            var roleProvider = (RoleProvider)Activator.CreateInstance(BuildManager.GetType(roleProviderSection.Type, true, true));
            
            return new StandardValuesCollection(roleProvider.GetAllRoles());
        }

        public override bool GetStandardValuesExclusive(ITypeDescriptorContext context)
        {
            if (context == null) return false;
            var service = (IDesignerHost)context.GetService(typeof(IDesignerHost));
            if (service == null) return false;
            return context.Instance is ActionBind;
        }

        public override bool GetStandardValuesSupported(ITypeDescriptorContext context)
        {
            return true;
        }

    }
}