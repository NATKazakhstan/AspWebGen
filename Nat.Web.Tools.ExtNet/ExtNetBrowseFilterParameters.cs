namespace Nat.Web.Tools.ExtNet
{
    using System;
    using System.Web.UI;

    using Ext.Net;

    using Nat.Web.Controls;
    using Nat.Web.Controls.GenerationClasses;

    public class ExtNetBrowseFilterParameters : BrowseFilterParameters
    {
        public void AddJsParamerter(string propertyFilter, string getValueJavaScript)
        {
            PropertyValues.Add(propertyFilter, new SimpleJSControl()
            {
                GetValue = () => getValueJavaScript
            });
        }

        protected override ControlParameter CreateControlParameter(Control control, string key)
        {
            var baseControl = (BaseControl)control;
            if (baseControl != null)
                return new ExtNetControlParameter(baseControl, key);

            return base.CreateControlParameter(control, key);
        }

        protected override ControlParameter CreateControlParameter(IControl control, string key)
        {
            return new ExtNetControlParameter(control, key);
        }

        protected override ControlParameter CreateGridColumnParameter(GridHtmlGenerator.Column control, string key)
        {
            var gridColumn = control as GridColumn;
            if (gridColumn != null)
                return new ExtNetGridControlParameter(gridColumn, key);

            return base.CreateGridColumnParameter(control, key);
        }

        protected override string GetValueFromControl(Control control)
        {
            var combobox = control as ComboBoxBase;
            if (combobox != null)
                return combobox.SelectedItem == null ? string.Empty : combobox.SelectedItem.Value;

            var field = control as Field;
            if (field != null && field.IsEmpty)
                return string.Empty;

            var dateField = control as DateField;
            if (dateField != null)
                return Convert.ToString(dateField.RawValue);

            var numberField = control as NumberField;
            if (numberField != null)
                return Convert.ToString(numberField.Number);

            var checkbox = control as Checkbox;
            if (checkbox != null)
                return checkbox.Checked.ToString();

            return base.GetValueFromControl(control);
        }

        public class SimpleJSControl : SimpleIDControl
        {
        }

        protected class ExtNetControlParameter : ControlParameter
        {
            public ExtNetControlParameter(BaseControl control, string property)
            {
                TypeComponent = TypeComponent.ValueControl;
                Property = property;
                if (control is DateField)
                    GetValueScript = control.ClientID + ".getValue() ? " + control.ClientID +
                                     ".getValue().toJSON() : null";
                else
                    GetValueScript = control.ClientID + ".getValue()";
            }

            public ExtNetControlParameter(IControl control, string property)
            {
                TypeComponent = TypeComponent.ValueControl;
                Property = property;
                GetValueScript = control is SimpleJSControl ? control.GetValue() : control.ClientID + ".getValue()";
            }

            public string GetValueScript { get; set; }
        }

        protected class ExtNetGridControlParameter : ControlParameter
        {
            public ExtNetGridControlParameter(GridColumn control, string property)
            {
                TypeComponent = TypeComponent.ValueControl;
                Property = property;
                GetValueScript = "record.data." + control.ColumnNameIndex;
            }

            public string GetValueScript { get; set; }
        }
    }
}
