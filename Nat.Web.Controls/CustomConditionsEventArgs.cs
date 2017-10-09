using System;
using System.Collections.Generic;
using System.Text;
using System.Web.UI.WebControls;

namespace Nat.Web.Controls
{
    public class CustomConditionsEventArgs : EventArgs
    {
        public Dictionary<string, Parameter> Parameters { get; set; }
        public StringBuilder Where { get; set; }
    }
}