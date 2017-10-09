using System.Collections;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace Nat.Web.Controls.GenerationClasses
{
    public interface IDropDownList : IRenderComponent
    {
        IEnumerable GetData();
        string NamePropertyName { get; set; }
        string TitlePropertyName { get; set; }
        Unit Width { get; set; }
        bool AllowValueNotSet { get; set; }
        string TextOfValueNotSet { get; set; }
        object SelectedValue { get; set; }
        string IDPropertyName { get; set; }
    }
}