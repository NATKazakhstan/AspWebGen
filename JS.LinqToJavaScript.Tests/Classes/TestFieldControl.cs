namespace JS.LinqToJavaScript.Tests.Classes
{
    using LinqToJavaScript.Attributes;

    [JavaScriptClass(ClassName = "TestFieldControl", Namespace = "JS.LinqToJavaScript.Tests", BaseClassName = "Nat.Web.JSControls.ActivityControl")]
    public class TestFieldControl<TActivityController, T> : ActivityControl<TActivityController, T>
        where TActivityController : ActivityController
        where T : struct 
    {
        #region Constructors and Destructors

        public TestFieldControl(TActivityController activityController, string controlID, string controlName)
            : base(activityController, controlID, controlName)
        {
        }

        #endregion

        protected override T? GetControlValue()
        {
            return default(T?);
        }
    }

    [JavaScriptClass(ClassName = "TestFieldControlString", Namespace = "JS.LinqToJavaScript.Tests", BaseClassName = "Nat.Web.JSControls.ActivityControl")]
    public class TestFieldControlClassValue<TActivityController, T> : ActivityControlClassValue<TActivityController, T>
        where TActivityController : ActivityController
        where T : class
    {
        #region Constructors and Destructors

        public TestFieldControlClassValue(TActivityController activityController, string controlID, string controlName)
            : base(activityController, controlID, controlName)
        {
        }

        #endregion

        protected override T GetControlValue()
        {
            return default(T);
        }
    }
}