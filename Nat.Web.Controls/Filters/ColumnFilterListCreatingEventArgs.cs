/*
 * Created by: Sergey V. Shpakovskiy
 * Created: 25 сентября 2008 г.
 * Copyright © JSC New Age Technologies 2008
 */

using System;
using System.Collections.Generic;
using Nat.Controls.DataGridViewTools;

namespace Nat.Web.Controls.Filters
{
    public class ColumnFilterListCreatingEventArgs : EventArgs
    {
        public ColumnFilterListCreatingEventArgs()
        {
            ListStorages = new List<ColumnFilterStorage>();
        }

        public List<ColumnFilterStorage> ListStorages { get; private set; }
    }
}