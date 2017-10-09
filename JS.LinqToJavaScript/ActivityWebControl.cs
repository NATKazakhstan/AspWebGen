namespace JS.LinqToJavaScript
{
    using System.Web.UI;

    public class ActivityCheckBoxControl<TActivityController> : ActivityControl<TActivityController, bool>
        where TActivityController : ActivityController
    {
        #region Constructors and Destructors

        public ActivityCheckBoxControl(TActivityController activityController, string controlID, string controlName)
            : base(activityController, controlID, controlName)
        {
        }

        #endregion

        #region Public Properties

        protected override object ControlValue
        {
            get { return SavedValue; }
            set { SavedValue = (bool)value; }
        }

        protected override bool? GetControlValue()
        {
            var chk = CheckBoxControl;
            if (chk == null)
                return null;

            return chk.Checked;
        }
        
        public ICheckBoxControl CheckBoxControl
        {
            get { return (ICheckBoxControl)Control; }
            set { Control = (Control)value; }
        }

        #endregion
    }
}