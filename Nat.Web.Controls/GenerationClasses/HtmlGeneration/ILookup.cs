using System.Web.UI.WebControls;

namespace Nat.Web.Controls.GenerationClasses
{
    public interface ILookup : IRenderComponent
    {
        Unit Width { get; set; }
        string SelectMode { get; set; }
        string ViewMode { get; set; }
        string ProjectName { get; set; }
        string TableName { get; set; }
        string AlternativeCellWidth { get; set; }
        BrowseFilterParameters BrowseFilterParameters { get; set; }
        object AlternativeColumnValue { get; set; }
        string AlternateText { get; set; }
        int MinimumPrefixLength { get; set; }
        string SelectKeyValueColumn { get; set; }
        bool IsMultipleSelect { get; set; }
        string OnChangedValue { get; set; }
        SelectColumnParameters SelectInfo { get; set; }
    }
}
