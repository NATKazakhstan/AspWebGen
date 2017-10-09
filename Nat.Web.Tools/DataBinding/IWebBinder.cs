/*
 * Created by: Denis M. Silkov
 * Created: 9 октября 2007 г.
 * Copyright © JSC New Age Technologies 2007
 */

using System.Collections.Generic;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace Nat.Web.Tools.DataBinding
{
    /// <summary>
    /// Интерфейс веб-байндера.
    /// </summary>
    public interface IWebBinder
    {
        IList<IWebBinding> Bindings { get; }

        string ErrorCssClass { get; }

        void InitBindings(Control root);

        bool ReadValues();

        bool ReadValues(Control Container);

        bool WriteValues();

        bool WriteValues(Control Container);

        BindingControlInfo LoadControlInfo(WebControl control);
    }
}