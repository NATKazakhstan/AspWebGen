namespace JS.LinqToJavaScript
{
    using System;

    [Flags]
    public enum ActivityType
    {
        None = 0,
        Enabled = 1,
        Visible = 2,
        ReadOnly = 4,
        AllowValidate = 8,
        AllowRequiredValidate = 16,
    }
}