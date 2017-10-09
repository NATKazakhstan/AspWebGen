namespace Nat.Web.Controls
{
    using System;

    public interface IColumnFilterStorageChanged
    {
        event EventHandler<EventArgs> ColumnFilterStorageChanged;
    }
}