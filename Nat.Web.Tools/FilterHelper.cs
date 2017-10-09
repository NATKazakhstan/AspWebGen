using System;
using System.Data;
using Nat.Controls.DataGridViewTools;
using Nat.Tools.Constants;
using Nat.Tools.Filtering;
using Nat.Tools.ResourceTools;

namespace Nat.Web.Tools
{
    public class FilterHelper
    {
        #region Создание ColumnFilterStorage для выбора по текущему месяцу и году

        public struct IntervalDates
        {
            public string beginDate;
            public string endDate;
        }

        /// <summary>
        /// Получение интервала начала и конца текущего месяца и года
        /// </summary>
        /// <param name="beginDate">Начало текущего месяца</param>
        /// <param name="endDate">Конец текущего месяца</param>
        public static void GetIntervalDates(out string beginDate, out string endDate)
        {
            var beginDateFormat = "01.{0}.{1}";
            var endDateFormat = "{0}.{1}.{2}";
            beginDate = string.Format(beginDateFormat, DateTime.Now.Date.Month, DateTime.Now.Date.Year);
            endDate = string.Format(endDateFormat,
                                    DateTime.DaysInMonth(DateTime.Now.Date.Year, DateTime.Now.Date.Month),
                                    DateTime.Now.Date.Month, DateTime.Now.Date.Year);
        }

        /// <summary>
        /// Получение интервала начала и конца текущего месяца и года
        /// </summary>
        private static IntervalDates GetIntervalDates()
        {
            var intervalDates = new IntervalDates();
            GetIntervalDates(out intervalDates.beginDate, out intervalDates.endDate);
            return intervalDates;
        }

        /// <summary>
        /// Создание ColumnFilterStorage для выбора по текущему месяцу и году
        /// </summary>
        /// <param name="dataColumn">колонка</param>
        /// <returns>фильтр для колонки</returns>
        public static ColumnFilterStorage CreateFilterStorageOnCurrentMonth(DataColumn dataColumn)
        {
            var storageCaption = (string)(DataSetResourceManager.GetColumnExtProperty(dataColumn, ColumnExtProperties.CAPTION));
            ColumnFilterStorage storage = null;
            if(dataColumn != null)
            {
                storage = new ColumnFilterStorage(dataColumn.ColumnName, storageCaption, typeof(DateTime), ColumnFilterType.Between, Convert.ToDateTime(GetIntervalDates().beginDate), Convert.ToDateTime(GetIntervalDates().endDate));
                storage.AvailableFilters = storage.GetDefaultFilterTypes();
            }
            return storage;
        }

        #endregion

        #region Создание ColumnFilterStorage для дочерней таблице

        /// <summary>
        /// Создание ColumnFilterStorage для дочерней таблице
        /// </summary>
        /// <param name="attach">имя колонки для прикрипления фильтра</param>
        /// <param name="caption">заголовок фильтра</param>
        /// <param name="table">дочерняя таблица</param>
        /// <param name="displayColumn">колонка для отображения в фильтре</param>
        /// <returns>фильтр для колонки дочерней таблицы</returns>
        public static ColumnFilterStorage CreateFilterStorageForChildTable(string attach, string caption, DataTable table, DataColumn displayColumn)
        {
            return new ColumnFilterStorage(attach, caption, typeof(long), ColumnFilterType.None, null, null)
                                    {
                                        RefDataSource = table,
                                        IsRefBound = true,
                                        RefTableRolledIn = true,
                                        TableName = table.TableName,
                                        ValueColumn = "id",
                                        DisplayColumn = displayColumn.ColumnName,
                                    };
        }

        #endregion

    }
}