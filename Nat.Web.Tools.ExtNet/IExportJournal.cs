namespace Nat.Web.Tools.ExtNet
{
    using System.Collections.Generic;

    using Nat.Web.Controls;
    using Nat.Web.Controls.GenerationClasses;

    public interface IExportJournal : IJournal
    {
        void PrepareExportData(List<IDataRow> exportData);
        List<string> GetFilterValues();
    }

    public interface IJournal
    {
        string TableHeader { get; }
        string TableName { get; }
        MainPageUrlBuilder Url { get; set; }
        IEnumerable<GridHtmlGenerator.Column> GetColumns();
    }
}