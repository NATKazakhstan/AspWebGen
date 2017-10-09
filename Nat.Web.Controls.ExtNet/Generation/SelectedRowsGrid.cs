namespace Nat.Web.Controls.ExtNet.Generation
{
    using Ext.Net;

    public class SelectedRowsGrid : GridPanel
    {
        public SelectedRowsGrid()
        {
            SortableColumns = true;
            HideHeaders = true;
            AutoScroll = true;
            ColumnModel.Columns.Add(
                new Column
                    {
                        ID = "RecordName",
                        DataIndex = "RecordName",
                        Flex = 1,
                    });
            var store = new Store
                {
                    ID = "selectedUserValuesStore",
                    RemotePaging = false,
                    AutoDataBind = true,
                    RemoteSort = false,
                    RemoteFilter = false,
                };
            Store.Add(store);
            var model = new Model
                {
                    ID = "selectedUserValuesModel",
                    IDProperty = "id",
                };
            model.Fields.Add("id", ModelFieldType.Int);
            model.Fields.Add("RecordName", ModelFieldType.String);
            store.Model.Add(model);
            Listeners.ItemClick.Handler = "function(grid, rowIndex, colIndex) { this.store.remove(rowIndex); }";
        }
    }
}
