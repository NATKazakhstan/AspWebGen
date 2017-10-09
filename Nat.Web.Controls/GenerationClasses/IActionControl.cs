using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Nat.Web.Controls.GenerationClasses
{
    public interface IActionControl
    {
        bool AsActionControl { get; set; }
        bool IsFirstCreation { get; set; }
        void ResultActionValues(ActionControlResults result);
    }
}
