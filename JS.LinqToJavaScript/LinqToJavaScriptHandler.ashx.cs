using Nat.Web.Tools;

namespace JS.LinqToJavaScript
{
    using System;
    using System.Collections.Generic;
    using System.Reflection;
    using System.Web;
    using System.Web.Compilation;

    public class LinqToJavaScriptHandler : IHttpHandler
    {
        MethodInfo method;
        
        #region IHttpHandler Members
        
        public bool IsReusable => false;

        public void ProcessRequest(HttpContext context)
        {
            LocalizationHelper.SetThreadCulture();
            var typeStr = context.Request["type"];
            if (typeStr == null)
                throw new ArgumentNullException(string.Empty, "Необходимо передать параметр type");
            var type = BuildManager.GetType(typeStr, true, false);
            var obj = Activator.CreateInstance(type);
            
            var activityController = obj as ActivityController;
            if (activityController != null)
                activityController.Initialize(null, new Dictionary<string, object>());

            var provider = new LinqToJavaScriptProvider();
            context.Response.ContentType = "text/javascript";
            context.Response.AddHeader("Cache-Control", "private;max-age=32400");
            if (method == null)
                method = provider.GetType().GetMethod("GetDeclareAllClassScript");
            var execMethod = method.MakeGenericMethod(type);
            context.Response.Write(
                execMethod.Invoke(provider, new[] { obj }));
        }

        #endregion
    }
}
