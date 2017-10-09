namespace JS.LinqToJavaScript.ExpressionVisitorsArgs
{
    using System.Linq.Expressions;

    public class JavaScriptMethodArgs
    {
        #region Public Properties

        public Expression Expression { get; set; }
        public string MethodName { get; set; }
        public string ReturnMethodType { get; set; }

        #endregion
    }
}