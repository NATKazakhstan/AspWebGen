namespace Nat.Web.Tools.ExtNet.ExtNetConfig
{
    using Nat.Web.Controls.GenerationClasses;

    public interface IListConfigItem
    {
        string ColumnName { get; set; }
        string Width { get; set; }
        string ServerMaping { get; set; }
        string Header { get; set; }
        SelectParameters.SelectInfo SelectInfo { get; set; }
        ExtNetSelectColumnParameters.ExtNetFieldInfo FieldInfo { get; set; }
    }
}