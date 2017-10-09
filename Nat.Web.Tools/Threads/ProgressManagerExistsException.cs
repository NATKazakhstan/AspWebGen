namespace Nat.Web.Tools.Threads
{
    using System;
    using System.Runtime.Serialization;

    public class ProgressManagerExistsException : ArgumentException
    {
        public ProgressManagerExistsException()
        {
        }

        public ProgressManagerExistsException(string message)
            : base(message)
        {
        }

        public ProgressManagerExistsException(string message, Exception innerException)
            : base(message, innerException)
        {
        }

        public ProgressManagerExistsException(string message, string paramName, Exception innerException)
            : base(message, paramName, innerException)
        {
        }

        public ProgressManagerExistsException(string message, string paramName)
            : base(message, paramName)
        {
        }

        protected ProgressManagerExistsException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }
    }
}