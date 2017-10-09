/*
 * Created by: Sergey V. Shpakovskiy
 * Created: 10 апреля 2009 г.
 * Copyright © JSC New Age Technologies 2009
 */

using System.Collections.Generic;

namespace Nat.Web.Controls.GenerationClasses
{
    public interface IAdditionalFields
    {
        /// <summary>
        /// Проверка прав на получение данных.
        /// </summary>
        /// <returns></returns>
        bool CheckPermit();

        /// <summary>
        /// Получение 
        /// </summary>
        /// <param name="idValue">Значение ключа текущей строки</param>
        /// <param name="value">Значение поля</param>
        /// <param name="nameOfColumn">Наименование колнки для которой запрашиваються значения дополнительных полей</param>
        /// <param name="isSecond">Если false значит значния перед полем, иначе значения следующих за полем</param>
        /// <param name="isKz">На каком языке запрашиваются данные</param>
        /// <return>Список значений дополнительных полей</return></returns>
        IEnumerable<string> GetNameOfValue(long? idValue, string value, string nameOfColumn, bool isSecond, bool isKz);
    }
}