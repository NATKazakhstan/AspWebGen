/*
 * Created by: Denis M. Silkov
 * Created: 14 сентября 2007 г.
 */

using System;

namespace Nat.Web.Controls
{
    /// <summary>
    /// Тип заполнения TableDataSource.
    /// </summary>
    [Serializable]
    public enum TableDataSourceFillType
    {
        /// <summary>
        /// Заполнять, если только изменились параметры выборки.
        /// </summary>
        ParametersNotChanged,
        /// <summary>
        /// Всегда заполнять.
        /// </summary>
        Always,
        /// <summary>
        /// Никогда не заполнять.
        /// </summary>
        Never
    }
}