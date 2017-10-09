/*
 * Created by: Sergey V. Shpakovskiy
 * Created: 10 декабря 2008 г.
 * Copyright © JSC New Age Technologies 2008
 */

using System;
using System.Data.Linq;

namespace Nat.Web.Controls.GenerationClasses
{
    public class DataSourceSelectingEventArgs : EventArgs
    {
        public IFilterControl FilterControl { get; set; }
        public DataContext DB { get; set; }
    }
}