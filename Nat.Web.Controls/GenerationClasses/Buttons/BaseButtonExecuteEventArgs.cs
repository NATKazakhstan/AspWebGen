using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Nat.Web.Controls.GenerationClasses.Buttons
{
    public class BaseButtonExecuteEventArgs<T> : EventArgs
        where T : struct
    {
        public BaseButtonExecuteEventArgs(T? value)
        {
            Value = value;
        }

        public T? Value { get; private set; }
    }
}
