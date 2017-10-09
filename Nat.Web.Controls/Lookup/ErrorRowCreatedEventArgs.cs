/*
 * Created by: Sergey V. Shpakovskiy
 * Created: 25 сентября 2008 г.
 * Copyright © JSC New Age Technologies 2008
 */

using System;
using System.Collections.Generic;
using System.Web.UI.WebControls;

namespace Nat.Web.Controls
{
    public class ErrorRowCreatedEventArgs : EventArgs
    {
        public ErrorRowCreatedEventArgs(TableCell cell, int index, long? id)
        {
            Cell = cell;
            Index = index;
            Id = id;
            ErrorIconUrls = new List<string>();
        }

        public TableCell Cell { get; private set; }
        public int Index { get; private set; }
        public long? Id { get; private set; }
        public List<string> ErrorIconUrls { get; private set; }
    }
}