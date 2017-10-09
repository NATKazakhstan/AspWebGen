using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI.WebControls;

namespace Nat.Web.Controls.GenerationClasses
{
    public interface ITextBox : IRenderComponent
    {
        string TextValue { get; set; }
        string ToolTip { get; set; }
        Unit Width { get; set; }
        Unit Height { get; set; }
        bool IsMultipleLines { get; set; }
        int? Columns { get; set; }
        int? Rows { get; set; }
        int? MaxLength { get; set; }
    }
}
