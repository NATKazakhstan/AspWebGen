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
                    var field = control as Field;
                    if (field != null && ":".Equals(field.LabelSeparator))
                    {
                        field.LabelSeparator =
                            args.Value
                                ? "<span class='requiredFieldMark' data-qtip='Required'>*</span>:"
                                : "<span class='requiredFieldMark' style='display:none' data-qtip='Required'>*</span>:";
                    }
                };
        }
    }
}
