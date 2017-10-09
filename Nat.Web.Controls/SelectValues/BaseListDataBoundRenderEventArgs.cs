using System;

namespace Nat.Web.Controls.SelectValues
{
    public class BaseListDataBoundRenderEventArgs : EventArgs
    {
        public string Width { get; set; }
        public int RowsCount { get; set; }

        public BaseListDataBoundRenderEventArgs Clone()
        {
            return new BaseListDataBoundRenderEventArgs
                       {
                           RowsCount = RowsCount,
                           Width = Width,
                       };
        }
    }
}