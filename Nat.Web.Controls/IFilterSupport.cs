/*
 * Created by: Sergey V. Shpakovskiy
 * Created: 9 октября 2008 г.
 * Copyright © JSC New Age Technologies 2008
 */

using System.Data.Linq;

namespace Nat.Web.Controls
{
    public interface IFilterSupport
    {
        string FilterControl { get; set; }
        string GetDefaultFilterControl();
    }
}