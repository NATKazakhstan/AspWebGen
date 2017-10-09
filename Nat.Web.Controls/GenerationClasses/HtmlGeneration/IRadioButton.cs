namespace Nat.Web.Controls.GenerationClasses
{
    public interface IRadioButton : IRenderComponent
    {
        bool Checked { get; set; }
        string GroupName { get; set; }
    }
}