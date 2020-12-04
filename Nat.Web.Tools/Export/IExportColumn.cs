/*
* Created by: Sergey V. Shpakovskiy
* Created: 2012.08.29
* Copyright © JSC NAT Kazakhstan 2012
*/

namespace Nat.Web.Tools.Export
{
    using System.Collections.Generic;

    public interface IExportColumn
    {
        /// <summary>
        /// Заголовок колонки.
        /// </summary>
        string Header { get; }

        /// <summary>
        /// Уникальное наименование колонки
        /// </summary>
        string ColumnName { get; }

        /// <summary>
        /// Текст заголовок выводить вертикально.
        /// </summary>
        bool IsVerticalHeaderText { get; }

        /// <summary>
        /// Текст данных выводить вертикально.
        /// </summary>
        bool IsVerticalDataText { get; }
        
        /// <summary>
        /// Выводить текст, если значение null.
        /// </summary>
        string NullItemText { get; }

        /// <summary>
        /// Видимость колонки, если false, то колонка не рендерится.
        /// </summary>
        bool Visible { get; }

        /// <summary>
        /// Кол-во соединенных колонок в строке, для заголовка.
        /// </summary>
        int ColSpan { get; }

        /// <summary>
        /// Кол-во соединенных строк в колонке, для заголовка.
        /// </summary>
        int RowSpan { get; }

        /// <summary>
        /// True, если имеются вложенные элементы заголовка.
        /// </summary>
        bool HasChild { get; }

        /// <summary>
        /// Ширина ячейки.
        /// </summary>
        decimal Width { get; }

        /// <summary>
        /// Колонка имеет числовые значения, выводится соответствующим типом.
        /// </summary>
        bool IsNumericColumn { get; }

        /// <summary>
        /// Метод возвращает значение колонки, для переданной строки.
        /// </summary>
        /// <param name="row"></param>
        /// <returns></returns>
        string GetValue(object row);

        /// <summary>
        /// Метод возвращает ссылку.
        /// </summary>
        /// <param name="row"></param>
        /// <returns></returns>
        string GetHyperLink(object row);

        /// <summary>
        /// Список дочерних колонок.
        /// </summary>
        /// <returns></returns>
        IEnumerable<IExportColumn> GetChilds();

        /// <summary>
        /// Кол-во соединеннных строк в колонке, для формирования данных
        /// </summary>
        /// <param name="row"></param>
        /// <returns></returns>
        int? GetDataRowSpan(object row);
    }
}