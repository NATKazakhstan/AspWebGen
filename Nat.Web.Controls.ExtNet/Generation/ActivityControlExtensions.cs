namespace Nat.Web.Controls.ExtNet.Generation
{
    using System;
    using System.Web.UI;

    using Ext.Net;

    using JS.LinqToJavaScript;

    public class ActivityControlExtensions
    {
        public static EventHandler<ActivityChangedEventArgs> AllowRequiredValidateChanged(Control control)
        {
            return delegate(object sender, ActivityChangedEventArgs args)
                {
                    if (control is Field field && ":".Equals(field.LabelSeparator))
                    {
                        field.LabelSeparator =
                            args.Value
                                ? "<span class='requiredFieldMark' data-qtip='Required'>*</span>:"
                                : "<span class='requiredFieldMark' style='display:none' data-qtip='Required'>*</span>:";
                    }
                    else if (control is GridPanel grid)
                        grid.CustomConfig.Add(new ConfigItem("allowBlank", (!args.Value).ToString().ToLower(), ParameterMode.Raw));
                };
        }

        public static EventHandler<ActivityChangedEventArgs> ReadOnlyChanged(Control control)
        {
            return delegate(object sender, ActivityChangedEventArgs args)
                {
                    if (control is Field field)
                        field.ReadOnly = args.Value;
                    else if (control is GridPanel grid)
                        grid.CustomConfig.Add(new ConfigItem("readOnly", args.Value.ToString().ToLower(), ParameterMode.Raw));
                };
        }
    }
}
