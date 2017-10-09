/*
 * Created by: Igor A. Kovalev
 * Created: 12 декабря 2008 г.
 * Copyright © JSC New Age Technologies 2008
 */

using System.Collections.Generic;
using System.Web.UI;
using Nat.Web.Controls;

namespace Nat.Web.Tools
{
    public interface IExecuteManager : IAccessControl
    {
        void Execute(IDictionary<string, string> queryParameters, Page page);
    }
}