namespace Nat.Web.Controls.GenerationClasses
{
    public interface ICheckBox : IRenderComponent
    {
        bool Checked { get; set; }
        string TrueText { get; set; }
        string FalseText { get; set; }
        string OnChange { get; set; }
        string OnClick { get; set; }
        string Label { get; set; }
        string ToolTip { get; set; }
    }
}