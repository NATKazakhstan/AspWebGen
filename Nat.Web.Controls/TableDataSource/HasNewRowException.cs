using System;
using System.Runtime.Serialization;

namespace Nat.Web.Controls
{
    public class HasNewRowException : Exception
    {
        public HasNewRowException()            
        {
        }

        public HasNewRowException(string message) : base(message)
        {
        }

        public HasNewRowException(string message, Exception innerException) : base(message, innerException)
        {
        }

        public HasNewRowException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}