namespace Nat.Web.Controls.GenerationClasses
{
    public interface IHyperLink : IRenderComponent
    {
        string ValueOfLink { get; set; }
        string ToolTip { get; set; }
        string Url { get; set; }
        string ImgUrl { get; set; }
        string OnClick { get; set; }
        string OnClickQuestion { get; set; }
        string Target { get; set; }
        bool RenderAsButton { get; set; }
    }
}