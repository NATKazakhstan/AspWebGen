namespace JS.LinqToJavaScript
{
    using System.Web.UI;

    public class LinqToJavaScriptDescriptor : ScriptDescriptor
    {
        public LinqToJavaScriptDescriptor(ActivityController activityController)
        {
            ActivityController = activityController;
        }

        public ActivityController ActivityController { get; private set; }

        protected override string GetScript()
        {
            var provider = new LinqToJavaScriptProvider();
            return provider.GetCreateClassScript(ActivityController);
        }
    }
}
