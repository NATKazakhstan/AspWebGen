using System.Web.UI.WebControls;

namespace Nat.Web.Controls.GenerationClasses
{
    public interface INumeric : IRenderComponent
    {
        decimal? ValueD { get; set; }
        string Format { get; set; }
        Unit Width { get; set; }
        int Length { get; set; }
        int Precision { get; set; }
        decimal? MinValue { get; set; }
        decimal? MaxValue { get; set; }
    }
}