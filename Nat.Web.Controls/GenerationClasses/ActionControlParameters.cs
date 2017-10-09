using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Nat.Web.Controls.GenerationClasses
{
    [Serializable]
    public class ActionControlParameters
    {
        public string UserControl { get; set; }
        public ActionControlType ActionType { get; set; }
        public object Value { get; set; }
    }
}
