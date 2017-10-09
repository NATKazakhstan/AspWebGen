using System;
using System.ComponentModel;
using System.ComponentModel.Design;
using System.Data;
using System.Web.Compilation;
using Nat.Web.Controls;
using Nat.Web.Tools;

namespace Nat.Web.Controls
{
    public class TableTypeConverter : StringConverter
    {
        private static DataSet GetDataSet(ITypeDescriptorContext context)
        {
            if (context == null) return null;
            TableItem tableItem = context.Instance as TableItem;
            if (tableItem == null) return null;
            ISessionWorkerContainer sessionWorkerContainer = tableItem.SessionWorkerContainer;
            if (sessionWorkerContainer == null) return null;
            IDesignerHost service = (IDesignerHost)context.GetService(typeof(IDesignerHost));
            if (service == null) return null;
            foreach (IComponent component in service.Container.Components)
            {
                SessionWorkerControl control = component as SessionWorkerControl;
                if (control != null && sessionWorkerContainer.SessionWorkerControl != null && 
                    sessionWorkerContainer.SessionWorkerControl.Equals(control.ID, StringComparison.OrdinalIgnoreCase) && 
                    !string.IsNullOrEmpty(control.TypeName))
                {
                    try
                    {
                        Type tDataSet = BuildManager.GetType(control.TypeName, false, true);
                        if (tDataSet != null)
                            return Activator.CreateInstance(tDataSet) as DataSet;
                    }
                    catch 
                    {
                        return null;
                    }
                }
            }
            return null;
        }

        public override StandardValuesCollection GetStandardValues(ITypeDescriptorContext context)
        {
            DataSet dataSet = GetDataSet(context);
            if (dataSet != null)
            {
                string [] list = new string[dataSet.Tables.Count];
                for (int i = 0; i < list.Length; i++)
                    list[i] = dataSet.Tables[i].TableName;            
                return new StandardValuesCollection(list);
            }
            return new StandardValuesCollection(new string[0]);
        }

        public override bool GetStandardValuesExclusive(ITypeDescriptorContext context)
        {
            return true;
        }

        public override bool GetStandardValuesSupported(ITypeDescriptorContext context)
        {
            return GetDataSet(context) != null;
        }
    }
}