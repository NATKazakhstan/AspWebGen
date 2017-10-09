namespace Nat.Web.Tools.ExtNet.ExtNetConfig
{
    using Nat.Web.Controls.GenerationClasses;

    public class ListConfigItem : IListConfigItem
    {
        public string Width { get; set; }
        public string ColumnName { get; set; }
        public string ServerMaping { get; set; }
        public string Header { get; set; }
        public SelectParameters.SelectInfo SelectInfo { get; set; }
        public ExtNetSelectColumnParameters.ExtNetFieldInfo FieldInfo { get; set; }
    }
}