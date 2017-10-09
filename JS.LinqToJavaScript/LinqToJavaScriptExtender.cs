namespace JS.LinqToJavaScript
{
    public static class LinqToJavaScriptExtender
    {
        public static T CreateClassScript<T>(this T source)
        {
            return source;
        }

        public static T DeclareClassScript<T>(this T source)
        {
            return source;
        }

        public static T DeclareAllClassScript<T>(this T source)
        {
            return source;
        }

        public static object JQueryFindById(this string clientId)
        {
            return clientId;
        }

        public static object JQueryFind(this string query)
        {
            return query;
        }

        public static object JQueryGetValue<T>(this T source)
        {
            return source;
        }

        public static TResult JQueryGetValue<T, TResult>(this T source)
        {
            return default(TResult);
        }

        public static T ExecuteJavaScriptFunction<T>(this T obj, string functionName, params object[] values)
        {
            return obj;
        }
    }
}