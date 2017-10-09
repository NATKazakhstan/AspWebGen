/*
 * Created by: Sergey V. Shpakovskiy
 * Created: 13 мая 2009 г.
 * Copyright © JSC New Age Technologies 2009
 */

namespace Nat.Web.Controls.GenerationClasses
{
    public interface ICodeRow : IRow
    {
        /// <summary>
        /// Код записи.
        /// </summary>
        string code { get; }
    }
}