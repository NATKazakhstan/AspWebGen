using System.Collections.Generic;
using System.Web.UI;
using System.Web.UI.WebControls;
using Nat.Web.Controls.GenerationClasses.BaseJournal;

namespace Nat.Web.Controls.GenerationClasses
{
    public interface IRenderComponent
    {
        void Render(HtmlTextWriter writer, ExtenderAjaxControl extenderAjaxControl);
        void InitAjax(Control control, ExtenderAjaxControl extenderAjaxControl);
        string ClientID { get; set; }
        string UniqueID { get; set; }
        bool Enabled { get; set; }
        object Value { get; set; }
        string Text { get; set; }
        string ValidationGroup { get; set; }
        bool Mandatory { get; set; }
        //List<ValidatorProperties> ValidatorProperties { get; }
        void AddValidators(IEnumerable<ValidatorProperties> validators);
        void AddValidator(ValidatorProperties validator);
        bool ValidateValue(string value, RenderContext renderContext);
        Unit Width { get; }
        Unit Height { get; }
    }
}