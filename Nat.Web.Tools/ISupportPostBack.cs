/*
 * Created by: Sergey V. Shpakovskiy
 * Created: 18 апреля 2008 г.
 * Copyright © JSC New Age Technologies 2008
 */

namespace Nat.Web.Tools
{
    /// <summary>
    /// Интерфей обозначающий поддержку свойства PostBack
    /// </summary>
    public interface ISupportPostBack
    {
        bool PostBack { get; set; }
    }
}