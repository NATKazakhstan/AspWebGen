/*
 * Created by: Denis M. Silkov
 * Created: 1 октября 2007 г.
 * Copyright © JSC New Age Technologies 2007
 */

using System;
using System.Collections.Generic;
using System.Web.UI;
using Nat.Tools.Classes;

namespace Nat.Web.Tools
{
    /// <summary>
    /// Интерфейс контрола, создающего клиентский(е) html-элементы.
    /// </summary>
    public interface IClientElementProvider
    {
        /// <summary>
        /// Возвращает идентификаторы html-элементов, в которые осуществляется ввод данных.
        /// </summary>
        /// <returns></returns>
        ICollection<string> GetInputElements();

        /// <summary>
        /// Возвращает контролы, в которые осуществляется ввод данных.
        /// </summary>
        /// <returns></returns>
        ICollection<Control> GetInputControls();

        /// <summary>
        /// Возвращает провайдеры форматов для html-элементов, в которые осуществляется ввод данных.
        /// </summary>
        /// <returns>Тройку объектов: идентификатор элемента, провайдер формата, строковый шаблон (дополнительно).</returns>
        ICollection<Pair<string, IFormatProvider>> GetInputFormatProviders();
    }
}