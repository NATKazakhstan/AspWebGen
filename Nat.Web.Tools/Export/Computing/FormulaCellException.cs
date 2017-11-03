namespace Nat.Web.Tools.Export.Computing
{
    using System;
    using System.Runtime.Serialization;

    public class FormulaCellException : Exception
    {
        public FormulaCellException()
        {
        }

        public FormulaCellException(string message)
            : base(message)
        {
        }

        public FormulaCellException(string message, Exception innerException)
            : base(message, innerException)
        {
        }

        protected FormulaCellException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }
    }
}