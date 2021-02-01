using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Script.Serialization;
using System.Linq.Expressions;
using System.Web.Compilation;
using System.Web.UI;
using System.Web.UI.WebControls;
using Nat.Web.Controls.DateTimeControls;

namespace Nat.Web.Controls.GenerationClasses
{
    [Serializable]
    public class SelectColumnParameters
    {
        public SelectColumnParameters()
        {
            UserControls = new List<ControlInfo>();
            SelectParameters = new SelectParameters();
        }

        public string FieldName
        {
            get { return SelectParameters.FieldName; }
            set { SelectParameters.FieldName = value; }
        }
        public string SessionKey 
        {
            get { return SelectParameters.SessionKey; }
            set { SelectParameters.SessionKey = value; }
        }
        public FieldInfo[] FieldInfoItems { get; set; }
        public List<ControlInfo> UserControls { get; set; }
        [ScriptIgnore]
        public SelectParameters SelectParameters { get; set; }

        public string Serialize()
        {
            var ser = new JavaScriptSerializer();
            return ser.Serialize(this);
        }

        [Serializable]
        public class FieldInfo
        {
            public FieldInfo()
            {
            }

            public FieldInfo(string fieldName, Control control)
            {
                FieldName = fieldName;
                ControlID = control.ClientID;
                TypeComponent = TypeComponent.ValueControl;
                Type controlType = control.GetType();

                if (typeof(CheckBox).IsAssignableFrom(controlType))
                    TypeComponent = TypeComponent.CheckedControl;
                else if (typeof(DatePicker).IsAssignableFrom(controlType))
                    ControlID = control.FindControl("textBoxID").ClientID;
            }

            public string FieldName { get; set; }
            public string ControlID { get; set; }
            public TypeComponent TypeComponent { get; set; }
        }

        [Serializable]
        public class ControlInfo
        {
            public string hfID { get; set; }
            public string LabelID { get; set; }
            public string FileLinkID { get; set; }
        }
    }

    [Serializable]
    public class SelectParameters
    {
        public SelectParameters()
        {
            SelectValues = new List<SelectInfo>();
            FieldValues = new Dictionary<string, string>();
            TableParameters = new Dictionary<string, TableParameterInfo>();
        }

        public string SessionKey { get; set; }
        public Dictionary<string, string> FieldValues { get; set; }
        public string FieldName { get; set; }
        [ScriptIgnore]
        public List<SelectInfo> SelectValues { get; set; }
        [ScriptIgnore]
        public Dictionary<string, TableParameterInfo> TableParameters { get; set; }

        public bool LoadFromSession()
        {
            var values = (List<SelectInfo>)HttpContext.Current.Session[SessionKey + "." + FieldName + ".SelectValues"];
            var parameters = (Dictionary<string, TableParameterInfo>)HttpContext.Current.Session[SessionKey + "." + FieldName + ".TableParameters"];
            if (values == null || parameters == null) return false;
            SelectValues = values;
            TableParameters = parameters;
            return true;
        }

        public void SaveToSession()
        {
            var keys = (List<string>)HttpContext.Current.Session[SessionKey];
            if (keys == null)
                HttpContext.Current.Session[SessionKey] = keys = new List<string>();
            var key1 = SessionKey + "." + FieldName + ".SelectValues";
            var key2 = SessionKey + "." + FieldName + ".TableParameters";
            HttpContext.Current.Session[key1] = SelectValues;
            HttpContext.Current.Session[key2] = TableParameters;
            if (!keys.Contains(key1))
                keys.Add(key1);
            if (!keys.Contains(key2))
                keys.Add(key2);
        }

        public static void RemoveSessionKey(string sessionKey)
        {
            var keys = (List<string>)HttpContext.Current.Session[sessionKey];
            if (keys == null) return;

            foreach (var key in keys)
                HttpContext.Current.Session.Remove(key);
        }

        public Expression GetExpression(string parameter)
        {
            if (TableParameters.ContainsKey(parameter))
                return TableParameters[parameter].GetExpression(FieldValues);
            else
                return Expression.Constant(null);
        }

        [Serializable]
        public class SelectInfo
        {
            private string format;

            public SelectInfoType Type { get; set; }
            
            public string Name { get; set; }
            
            public List<string> Parameters { get; set; }


            public string Format
            {
                get
                {
                    if (string.IsNullOrEmpty(this.format)) return "{0}";
                    return this.format;
                }

                set
                {
                    this.format = value;
                }
            }
        }

        public enum SelectInfoType
        {
            Column = 1,
            TableParameter = 2,
            RowInfo = 3,
        }

        [Serializable]
        public class TableParameterInfo
        {
            public string DataSourceType { get; set; }
            public string ParentTableParameterName { get; set; }
            public string Name { get; set; }
            public string FieldName { get; set; }
            public string ParameterValue { get; set; }
            public KeyValuePair<string, object> ParameterValueCache { get; set; }
            public Type ResultDataType { get; set; }
            public Type NullableResultDataType { get; set; }
            public Type FieldDataType { get; set; }

            public Expression GetExpression(Dictionary<string, string> fieldValues)
            {
                object value = null;
                if (!string.IsNullOrEmpty(DataSourceType))
                {
                    var key = fieldValues.ContainsKey(FieldName) ? fieldValues[FieldName] : "";
                    if (key.Equals(ParameterValueCache.Key))
                        value = ParameterValueCache.Value;
                    else if(!string.IsNullOrEmpty(key))
                    {
                        var sourceType = BuildManager.GetType(DataSourceType, true, false);
                        var dataSource = (IDataSourceViewGetName)Activator.CreateInstance(sourceType);
                        value = dataSource.GetTableParameterValue(key, ParentTableParameterName);
                        ParameterValueCache = new KeyValuePair<string, object>(key, value);
                    }
                }
                else
                {
                    string newValue = null;
                    if (string.IsNullOrEmpty(FieldName))
                        newValue = ParameterValue;
                    else if (fieldValues.ContainsKey(FieldName) && !string.IsNullOrEmpty(fieldValues[FieldName]))
                        newValue = fieldValues[FieldName];

                    try
                    {
                        value = newValue == null ? null : Convert.ChangeType(newValue, ResultDataType);
                    }
                    catch (FormatException)
                    {
                        value = null;
                    }
                }
                return Expression.Constant(value, NullableResultDataType);
            }
        }
    }
}
