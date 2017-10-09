using System;

namespace Nat.Web.Controls
{
    public interface IGridColumn : IColumnName
    {
        Array GetValues(GridViewExt grid);
    }
}