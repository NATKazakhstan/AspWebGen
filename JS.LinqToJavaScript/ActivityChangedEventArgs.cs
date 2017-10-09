namespace JS.LinqToJavaScript
{
    using System;

    public class ActivityChangedEventArgs : EventArgs
    {
        public ActivityChangedEventArgs(ActivityType activityType, bool value)
        {
            ActivityType = activityType;
            Value = value;
        }

        public ActivityType ActivityType { get; set; }

        public bool Value { get; set; }
    }
}
