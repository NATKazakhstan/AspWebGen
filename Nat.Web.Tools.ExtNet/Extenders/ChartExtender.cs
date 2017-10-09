namespace Nat.Web.Tools.ExtNet.Extenders
{
    using System.Linq;

    using Ext.Net;

    using Nat.Web.Controls.GenerationClasses;

    public static class ChartExtender
    {
        public static void InitializeStoreColumns(this Chart chart, BaseGridColumns gridColumns)
        {
            var columns = gridColumns.GetExtNetGridColumns().Where(r => r.ChartColumn).ToList();
            var store = chart.GetStore();
            store.InitializeStoreModel(columns);
        }
    }
}