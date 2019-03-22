/*
* Created by: Sergey V. Shpakovskiy
* Created: 2012.08.29
* Copyright © JSC NAT Kazakhstan 2012
*/

namespace Nat.Web.Tools.Export
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Text;

    using Nat.Web.Controls;
    using Nat.Web.Tools.Export.Formatting;

    public class JournalExportEventArgs
    {
        /// <summary>
        /// Заголовок отчета.
        /// </summary>
        public string Header { get; set; }

        /// <summary>
        /// Список примененных фильтров или другие строки перед таблицей данных.
        /// </summary>
        public List<string> FilterValues { get; set; }

        /// <summary>
        /// Набор данных.
        /// </summary>
        public ICollection Data { get; set; }

        /// <summary>
        /// Проверка доступа.
        /// </summary>
        public IAccessControl Control { get; set; }

        /// <summary>
        /// Описание колонок.
        /// </summary>
        public IEnumerable<IExportColumn> Columns { get; set; }

        /// <summary>
        /// Формат выходных данных. Сейчас только xlsx.
        /// </summary>
        public string Format { get; set; }
        
        /// <summary>
        /// Класс для логирования.
        /// </summary>
        public ILogMonitor LogMonitor { get; set; }
        
        /// <summary>
        /// Флаг о необходимости проверять права. True значит вызывается проверка прав.
        /// </summary>
        public bool CheckPermit { get; set; }

        /// <summary>
        /// Расширение выходного файла.
        /// </summary>
        public string FileNameExtention { get; set; }

        /// <summary>
        /// Вызывется метод в начале рендера новой строки.
        /// </summary>
        public Action<object> StartRenderRow { get; set; }

        /// <summary>
        /// Ссылка на просомотр журнала.
        /// </summary>
        public StringBuilder ViewJournalUrl { get; set; }

        /// <summary>
        /// Код лога эеспорта.
        /// </summary>
        public long ExportLog { get; set; }

        /// <summary>
        /// Описание форматирования.
        /// </summary>
        public List<ConditionalFormatting> ConditionalFormatting { get; set; }

        /// <summary>
        /// Количество фиксированных колонок.
        /// </summary>
        public int FixedColumnsCount { get; set; }

        /// <summary>
        /// Количество фиксированных строк.
        /// </summary>
        public int FixedRowsCount { get; set; }

        /// <summary>
        /// Добавить колонку.
        /// </summary>
        /// <param name="exportColumn"></param>
        public void AddColumn(IExportColumn exportColumn)
        {
            if (Columns == null) Columns = new List<IExportColumn>();
            ((ICollection<IExportColumn>)Columns).Add(exportColumn);
        }

        /// <summary>
        /// Добавить значение в параметр FilterValues.
        /// </summary>
        /// <param name="filterValue"></param>
        public void AddFilterValue(string filterValue)
        {
            if (FilterValues == null)
                FilterValues = new List<string>();
            FilterValues.Add(filterValue);
        }

        /// <summary>
        /// Текст для группировки строк (выводится одна строка на всю таблицу с текстом).
        /// Первый параметр строка, вернуть текст группы.
        /// </summary>
        public Func<object, string> GetGroupText;

        /// <summary>
        /// Необходимо выполнить расчет данных по колонкам.
        /// Первый параметр строка, второй текст группы, третий колнка.
        /// </summary>
        public Action<object, string, IExportColumn> ComputeTotalValue;

        /// <summary>
        /// Получение значения для ячейки строки итога.
        /// Первый параметр текст группы, второй колнка, вернуть текст ячейки Excel.
        /// </summary>
        public Func<string, IExportColumn, string> GetTotalValue;

        /// <summary>
        /// Экспорт без обобщающей итоговой строки.
        /// </summary>
        public bool WithOutGroupTotalRow { get; set; }
    }
}