using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Reflection;

namespace Nat.Web.Controls
{
    public class SessionWorkerTypeNameTypeEditor : StringConverter
    {
        private static string[] GetDataSetTypes()
        {
            List<string> list = new List<string>();
            Type tDataSet = typeof (DataSet);
            try
            {
                Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();
                foreach (Assembly assembly in assemblies)
                {
                    if (assembly.GlobalAssemblyCache) continue;
                    if (assembly.GetName().FullName.ToLower().Contains("resharper") ||
                        assembly.GetName().FullName.ToLower().Contains("ankh")) continue;
                    try
                    {
                        Type[] types = assembly.GetTypes();
                        foreach (Type type in types)
                        {
                            if (type.IsSubclassOf(tDataSet))
                                list.Add(type.FullName);
                        }
                    }
                    catch
                    {
                    }
                }
            }
            catch
            {
            }
            return list.ToArray();
        }

        public override StandardValuesCollection GetStandardValues(ITypeDescriptorContext context)
        {
            return new StandardValuesCollection(GetDataSetTypes());
        }

        public override bool GetStandardValuesExclusive(ITypeDescriptorContext context)
        {
            return true;
        }

        public override bool GetStandardValuesSupported(ITypeDescriptorContext context)
        {
            return true;
        }
    }
}