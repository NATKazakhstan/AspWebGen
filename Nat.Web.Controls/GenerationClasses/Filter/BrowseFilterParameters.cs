/*
 * Created by: Sergey V. Shpakovskiy
 * Created: 10 ноября 2008 г.
 * Copyright © JSC New Age Technologies 2008
 */

using System;
using System.Collections.Generic;
using System.Web.Script.Serialization;
using System.Web.UI;
using System.Web.UI.WebControls;
using Nat.Web.Controls.DateTimeControls;

namespace Nat.Web.Controls.GenerationClasses
{
    using System.Linq.Expressions;

    using Nat.Tools.Filtering;
    using Nat.Web.Controls.GenerationClasses.Filter;
    using System.Linq;

    using Nat.Web.Controls.SelectValues;

    public class BrowseFilterParameters : EventArgs
    {
        public const string IdIsNull = "id.IsNull";

        protected readonly Dictionary<string, object> PropertyValues = new Dictionary<string, object>();
        private string _clientControlParameters;
        private string _clientValueParameters;

        public string ClientControlParameters
        {
            get
            {
                if (_clientControlParameters == null)
                    CreateClientParameters();
                return _clientControlParameters;
            }
        }

        public string ClientValueParameters
        {
            get
            {
                if (_clientValueParameters == null)
                    CreateClientParameters();
                return _clientValueParameters;
            }
        }

        private void CreateClientParameters()
        {
            _clientControlParameters = string.Empty;
            _clientValueParameters = string.Empty;
            var controls = new List<ControlParameter>();
            var values = new List<ValueParameter>();

            foreach (KeyValuePair<string, object> pair in PropertyValues)
            {
                var control = pair.Value as Control;
                if (control != null)
                {
                    controls.Add(CreateControlParameter(control, pair.Key));
                    continue;
                }

                var icontrol = pair.Value as IControl;
                if (icontrol != null)
                {
                    controls.Add(CreateControlParameter(icontrol, pair.Key));
                    continue;
                }

                var column = pair.Value as GridHtmlGenerator.Column;
                if (column != null)
                {
                    var gridColumnParameter = CreateGridColumnParameter(column, pair.Key);
                    if (gridColumnParameter != null)
                    {
                        controls.Add(gridColumnParameter);
                        continue;
                    }
                }

                values.Add(CreateValueParameter(pair.Key, (string)pair.Value));
            }

            var jss = new JavaScriptSerializer();
            _clientControlParameters = jss.Serialize(controls);
            _clientValueParameters = jss.Serialize(values);
        }

        protected virtual ControlParameter CreateControlParameter(Control control, string key)
        {
            return new ControlParameter(control, key);
        }

        protected virtual ControlParameter CreateControlParameter(IControl control, string key)
        {
            return new ControlParameter(control, key);
        }

        protected virtual ControlParameter CreateGridColumnParameter(GridHtmlGenerator.Column control, string key)
        {
            return null;
        }

        protected virtual ValueParameter CreateValueParameter(string key, string value)
        {
            return new ValueParameter { Property = key, Value = value };
        }

        public void AddControlParamerter(string propertyFilter, Control control)
        {
            PropertyValues.Add(propertyFilter, control);
        }

        public void AddControlParamerter(string propertyFilter, IControl control)
        {
            PropertyValues.Add(propertyFilter, control);
        }

        public void AddControlParamerter(string propertyFilter, GridHtmlGenerator.Column column)
        {
            PropertyValues.Add(propertyFilter, column);
        }

        public void AddControlParamerter(string propertyFilter, string value)
        {
            PropertyValues.Add(propertyFilter, value);            
        }

        public void AddControlParamerter<TSource, TResult>(
            Expression<Func<TSource, TResult>> getValueExpression,
            ColumnFilterType filterType,
            object value)
        {
            var parameterName = getValueExpression.Body.ToString();
            parameterName = parameterName.Remove(0, parameterName.IndexOf(".", StringComparison.InvariantCulture) + 1)
                            + "."
                            + FilterItem.ConvertToFilterType(filterType);
            if (value == null || value is IControl || value is Control || value is string || value is GridHtmlGenerator.Column) 
                PropertyValues.Add(parameterName, value);
            else
                PropertyValues.Add(parameterName, value.ToString());
        }

        public void AddControlParamerter<TSource, TResult>(
            Expression<Func<TSource, TResult?>> getValueExpression,
            ColumnFilterType filterType,
            object value)
            where TResult : struct
        {
            var parameterName = getValueExpression.Body.ToString();
            parameterName = parameterName.Remove(0, parameterName.IndexOf(".", StringComparison.InvariantCulture) + 1)
                            + "."
                            + FilterItem.ConvertToFilterType(filterType);
            if (value == null || value is IControl || value is Control || value is string || value is GridHtmlGenerator.Column)
                PropertyValues.Add(parameterName, value);
            else
                PropertyValues.Add(parameterName, value.ToString());
        }

        public void RemoveParameter(string propertyFilter)
        {
            PropertyValues.Remove(propertyFilter);
        }

        public void AddControlParamerter(string propertyFilter, params string[] valuesCollection)
        {
            PropertyValues.Add(propertyFilter, string.Join(",", valuesCollection));
        }

        public string GetClientParameters()
        {
            var jss = new JavaScriptSerializer();
            return jss.Serialize(new Pair(ClientControlParameters, ClientValueParameters));
        }

        public object GetParameterValue(string propertyFilter)
        {
            if (!PropertyValues.ContainsKey(propertyFilter))
                return null;
            return PropertyValues[propertyFilter];
        }

        public IEnumerable<KeyValuePair<string, string>> GetParameterValues()
        {
            return PropertyValues.Select(
                pair =>
                {
                    var column = pair.Value as GridHtmlGenerator.Column;
                    if (column != null)
                    {
                        throw new NotSupportedException("В данном фильтре нельзя указывать GridHtmlGenerator.Column");
                    }

                    var control = pair.Value as Control;
                    if (control != null)
                    {
                        return new KeyValuePair<string, string>(pair.Key, GetValueFromControl(control));
                    }

                    var icontrol = pair.Value as IControl;
                    if (icontrol != null)
                    {
                        return new KeyValuePair<string, string>(pair.Key, icontrol.GetValue());
                    }

                    return new KeyValuePair<string, string>(pair.Key, (string)pair.Value);
                });
        }

        public void Clear()
        {
            PropertyValues.Clear();
        }

        protected class ControlParameter
        {
            public ControlParameter()
            {
            }

            public ControlParameter(Control control, string property)
            {
                ID = control.ClientID;
                Property = property;
                TypeComponent = TypeComponent.ValueControl;
                Type controlType = control.GetType();

                if (typeof(CheckBox).IsAssignableFrom(controlType))
                    TypeComponent = TypeComponent.CheckedControl;
                else if (typeof(MultipleSelect).IsAssignableFrom(controlType))
                    TypeComponent = TypeComponent.CheckedListItems;
                else if (typeof(DatePicker).IsAssignableFrom(controlType))
                    ID = control.FindControl("textBoxID").ClientID;
                else if (typeof(CodeDropDownListExt).IsAssignableFrom(controlType))
                    ID = ((CodeDropDownListExt)control).DropDownListExtText.ClientID;
            }

            public ControlParameter(IControl control, string property)
            {
                ID = control.ClientID;
                Property = property;
                TypeComponent = control.TypeComponent;
            }

            public string ID;

            public string Property;

            public TypeComponent TypeComponent;
        }

        protected class ValueParameter
        {
            public string Property;

            public string Value;
        }

        public void SetQueryParameters(MainPageUrlBuilder builder)
        {
            foreach (var pair in GetParameterValues())
            {
                if ("mode".Equals(pair.Key))
                    builder.SelectMode = pair.Value;
                else
                    builder.QueryParameters.Add(pair.Key, pair.Value);
            }
        }

        protected virtual string GetValueFromControl(Control control)
        {
            var dropDownList = control as DropDownListExt;
            if (dropDownList != null)
            {
                return dropDownList.SelectedValue == null
                           ? string.Empty
                           : dropDownList.SelectedValue.ToString();
            }

            var attributes = control.GetType().GetCustomAttributes(typeof(ControlValuePropertyAttribute), true);
            if (attributes.Length > 0)
            {
                var property = control.GetType().GetProperty(((ControlValuePropertyAttribute)attributes[0]).Name);
                var objValue = property.GetValue(control, null);
                return objValue == null ? string.Empty : objValue.ToString();
            }

            var webControl = control as WebControl;
            if (webControl != null)
                return webControl.Attributes["value"];

            return string.Empty;
        }

        public interface IControl
        {
            string ClientID { get; }
            TypeComponent TypeComponent { get; }
            Func<string> GetValue { get; }
        }

        public class SimpleIDControl : IControl
        {
            public SimpleIDControl()
            {
                TypeComponent = TypeComponent.ValueControl;
            }

            #region IControl Members

            public string ClientID { get; set; }
            
            public TypeComponent TypeComponent { get; set; }

            public Func<string> GetValue { get; set; }

            #endregion

        }
    }
}