using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI.WebControls;
using System.Data.Linq;
using System.ComponentModel;
using System.Xml.Linq;
using System.Globalization;

namespace Nat.Web.Controls.GenerationClasses.Data
{
    using System.Data.Linq.Mapping;

    public class LinqDataSourceViewExt : LinqDataSourceView
    {
        public LinqDataSourceViewExt(LinqDataSource owner, string name, HttpContext context)
            : base (owner, name, context)
        {
        }

        public bool UpdateNewObject { get; set; }

        protected override void UpdateDataObject(object dataContext, object table, object oldDataObject, object newDataObject)
        {
            ((ITable)table).Attach(oldDataObject);
            Dictionary<string, Exception> innerExceptions = this.SetDataObjectProperties(oldDataObject, newDataObject);
            if (innerExceptions != null)
                throw new LinqDataSourceValidationException(string.Format(CultureInfo.InvariantCulture, "AtlasWeb.LinqDataSourceView_ValidationFailed error message: {1} for type: {0} ", new object[] { oldDataObject.GetType(), innerExceptions.Values.First<Exception>().Message }), innerExceptions);
            ((DataContext)dataContext).SubmitChanges();
            
            if (UpdateNewObject)
            {
                UpdateNewDataObjectProperties(oldDataObject, newDataObject);
            }
        }

        private Dictionary<string, Exception> SetDataObjectProperties(object oldDataObject, object newDataObject)
        {
            Dictionary<string, Exception> dictionary = null;
            foreach (PropertyDescriptor descriptor in TypeDescriptor.GetProperties(oldDataObject))
            {
                if (!descriptor.IsReadOnly && 
                        (descriptor.PropertyType.IsSerializable 
                         || descriptor.PropertyType == typeof(XElement)
                         || descriptor.PropertyType == typeof(XDocument)))
                {
                    object obj2 = descriptor.GetValue(newDataObject);
                    try
                    {
                        descriptor.SetValue(oldDataObject, obj2);
                    }
                    catch (Exception exception)
                    {
                        if (dictionary == null)
                            dictionary = new Dictionary<string, Exception>(StringComparer.OrdinalIgnoreCase);
                        dictionary[descriptor.Name] = exception;
                    }
                }
            }
            return dictionary;
        }


        private Dictionary<string, Exception> UpdateNewDataObjectProperties(object oldDataObject, object newDataObject)
        {
            Dictionary<string, Exception> dictionary = null;
            foreach (PropertyDescriptor descriptor in TypeDescriptor.GetProperties(oldDataObject))
            {
                if (!descriptor.IsReadOnly &&
                        (descriptor.PropertyType.IsSerializable
                         || descriptor.PropertyType == typeof(XElement)
                         || descriptor.PropertyType == typeof(XDocument))
                    && descriptor.Attributes[typeof(ColumnAttribute)] != null)
                {
                    var attribute = ((ColumnAttribute)descriptor.Attributes[typeof(ColumnAttribute)]);
                    if (attribute.AutoSync != AutoSync.Always && attribute.AutoSync != AutoSync.OnUpdate)
                        continue;

                    var obj2 = descriptor.GetValue(oldDataObject);
                    try
                    {
                        descriptor.SetValue(newDataObject, obj2);
                    }
                    catch (Exception exception)
                    {
                        if (dictionary == null)
                            dictionary = new Dictionary<string, Exception>(StringComparer.OrdinalIgnoreCase);
                        dictionary[descriptor.Name] = exception;
                    }
                }
            }

            return dictionary;
        }
    }
}
