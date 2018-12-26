/*
 * Created by: Sergey V. Shpakovskiy
 * Created: 13 января 2009 г.
 * Copyright © JSC New Age Technologies 2009
 */

using System;
using System.Collections.Generic;
using Nat.Tools.Filtering;
using Nat.Web.Controls.GenerationClasses.Filter;

namespace Nat.Web.Controls.GenerationClasses
{
    using System.Linq;

    public class DefaultFilters
    {
        #region BooleanFilter enum

        public enum BooleanFilter
        {
            /// <summary>
            /// Не установленно
            /// </summary>
            Non,
            /// <summary>
            /// True
            /// </summary>
            Equals,
            /// <summary>
            /// False
            /// </summary>
            NotEquals,
            /// <summary>
            /// Равно null
            /// </summary>
            IsNotNull,
            /// <summary>
            /// Не равно null
            /// </summary>
            IsNull,
        }

        #endregion

        #region NumericFilter enum

        public enum NumericFilter
        {
            /// <summary>
            /// Не установленно
            /// </summary>
            Non,
            /// <summary>
            /// Значения равны
            /// </summary>
            Equals,
            /// <summary>
            /// Значения не равны
            /// </summary>
            NotEquals,
            /// <summary>
            /// Равно null
            /// </summary>
            IsNotNull,
            /// <summary>
            /// Не равно null
            /// </summary>
            IsNull,
            /// <summary>
            /// Значение поля больше параметра
            /// </summary>
            More,
            /// <summary>
            /// Значение поля меньше параметра
            /// </summary>
            Less,
            /// <summary>
            /// Значение поля между значениями параметров
            /// </summary>
            Between,
            /// <summary>
            /// Фильтр по периоду. Между двумя полями записи.
            /// </summary>
            BetweenColumns,
            /// <summary>
            /// Прошло N дней и более. Расчет всегда от текущей даты.
            /// </summary>
            DaysAgoAndMore,
            /// <summary>
            /// Осталось N дней и более. Расчет всегда от текущей даты.
            /// </summary>
            DaysLeftAndMore,
            /// <summary>
            /// Текущий день.
            /// </summary>
            ToDay,
            /// <summary>
            /// Значение поля больше параметра
            /// </summary>
            MoreOrEqual,
            /// <summary>
            /// Значение поля меньше параметра
            /// </summary>
            LessOrEqual,
        }

        #endregion

        #region ReferenceFilter enum

        public enum ReferenceFilter
        {
            /// <summary>
            /// Не установленно
            /// </summary>
            Non,
            /// <summary>
            /// Значения равны
            /// </summary>
            Equals,
            /// <summary>
            /// Значения не равны
            /// </summary>
            NotEquals,
            /// <summary>
            /// Равно null
            /// </summary>
            IsNotNull,
            /// <summary>
            /// Не равно null
            /// </summary>
            IsNull,
            /// <summary>
            /// Текст поля по ссылке содержит строку
            /// </summary>
            ContainsByRef,
            /// <summary>
            /// Текст поля по ссылке начинается со строки
            /// </summary>
            StartsWithByRef,
            /// <summary>
            /// Текст поля по ссылке оканчивается на строку
            /// </summary>
            EndsWithByRef,
            /// <summary>
            /// Текст поля по ссылке содержит слова.
            /// </summary>
            ContainsWordsByRef,
            /// <summary>
            /// Значения начинается с
            /// </summary>
            StartsWithCode,
            /// <summary>
            /// Значения не начинается с
            /// </summary>
            NotStartsWithCode,
            /// <summary>
            /// Текст поля по ссылке содержит любые слова.
            /// </summary>
            ContainsAnyWordByRef,
            /// <summary>
            /// Значение поля больше или равно параметра
            /// </summary>
            MoreOrEqual,
            /// <summary>
            /// Значение поля меньше или равно параметра
            /// </summary>
            LessOrEqual
        }

        #endregion

        #region TextFilter enum

        public enum TextFilter
        {
            /// <summary>
            /// Не установленно
            /// </summary>
            Non,
            /// <summary>
            /// Значения равны
            /// </summary>
            Equals,
            /// <summary>
            /// Значения не равны
            /// </summary>
            NotEquals,
            /// <summary>
            /// Равно null
            /// </summary>
            IsNotNull,
            /// <summary>
            /// Не равно null
            /// </summary>
            IsNull,
            /// <summary>
            /// Текст поля содержит строку
            /// </summary>
            Contains,
            /// <summary>
            /// Текст поля не содержит строку
            /// </summary>
            NotContains,
            /// <summary>
            /// Текст поля начинается со строки
            /// </summary>
            StartsWith,
            /// <summary>
            /// Текст поля оканчивается на строку
            /// </summary>
            EndsWith,
            /// <summary>
            /// Текст содержит слова.
            /// </summary>
            ContainsWords,
            /// <summary>
            /// Текст поля по ссылке содержит любые слова.
            /// </summary>
            ContainsAnyWord,
            /// <summary>
            /// Текст не содержит слова.
            /// </summary>
            NotContainsWords,
            /// <summary>
            /// Длина строки больше указанного значения.
            /// </summary>
            LengthMore,
            /// <summary>
            /// Длина строки меньше указанного значения.
            /// </summary>
            LengthLess,
        }

        #endregion
        
        #region FullTextSearchFilter enum

        public enum FullTextSearchFilter
        {
            /// <summary>
            /// Текст поля содержит строку
            /// </summary>
            Contains,
        }

        #endregion
        
        private Dictionary<string, string[]> _filterValues = new Dictionary<string, string[]>();

        public Dictionary<string, string[]> FilterValues
        {
            get { return _filterValues; }
            set { _filterValues = value; }
        }

        private Dictionary<string, List<FilterItem>> _filterItems = new Dictionary<string, List<FilterItem>>();

        public Dictionary<string, List<FilterItem>> FilterItems
        {
            get { return _filterItems; }
            set { _filterItems = value; }
        }

        internal void RemoveFilters(IEnumerable<string> fieldNames)
        {
            foreach (var fieldName in fieldNames.Where(_filterItems.ContainsKey))
            {
                _filterItems[fieldName].Clear();
            }
        }

        public void SetFilter(string fieldName, BooleanFilter filterType)
        {
            SetFilter(fieldName, filterType, true, null);
        }

        public void SetFilter(string fieldName, ReferenceFilter filterType, object value, string text)
        {
            SetFilter(fieldName, filterType, value, (object)text);
        }

        public void SetFilter(string fieldName, TextFilter filterType, string value)
        {
            SetFilter(fieldName, filterType, value, null);
        }

        public void SetFilter(string fieldName, NumericFilter filterType, object value)
        {
            SetFilter(fieldName, (Enum) filterType, value, null);
        }

        public void SetFilter(string fieldName, NumericFilter filterType, object value1, object value2)
        {
            SetFilter(fieldName, (Enum) filterType, value1, value2);
        }

        public void SetFilter(string fieldName, ColumnFilterType? filterType, string value1, string value2)
        {
            List<FilterItem> items;
            if (!_filterItems.ContainsKey(fieldName))
                items = _filterItems[fieldName] = new List<FilterItem>();
            else
                items = _filterItems[fieldName];
            items.Add(new FilterItem
                          {
                              FilterName = fieldName,
                              ColumnFilterType = filterType,
                              Value1 = value1,
                              Value2 = value2,
                              IsDisabled = true,
                          });
        }

        private void SetFilter(string fieldName, Enum filterType, object value1, object value2)
        {
            if (!_filterValues.ContainsKey(fieldName))
                _filterValues.Add(fieldName, new string[3]);
            _filterValues[fieldName][0] = filterType.ToString();
            _filterValues[fieldName][1] = (value1 ?? "").ToString();
            _filterValues[fieldName][2] = (value2 ?? "").ToString();

            List<FilterItem> items;
            if (!_filterItems.ContainsKey(fieldName))
                items = _filterItems[fieldName] = new List<FilterItem>();
            else
                items = _filterItems[fieldName];
            items.Add(new FilterItem
                          {
                              FilterName = fieldName,
                              FilterType = filterType.ToString(),
                              Value1 = (value1 ?? "").ToString(),
                              Value2 = (value2 ?? "").ToString(),
                          });
        }
    }
}