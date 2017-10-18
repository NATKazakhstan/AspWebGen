namespace Nat.Web.Tools.Security
{
    using System;
    using System.Runtime.Serialization;
    using System.Web;

    public class AccessDeniedException : Exception
    {
        public AccessDeniedException()
        {
        }

        public AccessDeniedException(string message)
            : base(message)
        {
        }

        public AccessDeniedException(string message, Exception innerException)
            : base(message, innerException)
        {
        }

        protected AccessDeniedException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }

        public static bool IsInService()
        {
            return HttpContext.Current?.Handler?.GetType().FullName?.StartsWith("System.Web.Script.Services.ScriptHandlerFactory") ?? false;
        }
    }
}