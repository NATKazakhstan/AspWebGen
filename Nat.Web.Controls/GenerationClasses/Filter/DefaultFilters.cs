/*
 * Created by: Sergey V. Shpakovskiy
 * Created: 13 ������ 2009 �.
 * Copyright � JSC New Age Technologies 2009
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
            /// �� ������������
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
            /// ����� null
            /// </summary>
            IsNotNull,
            /// <summary>
            /// �� ����� null
            /// </summary>
            IsNull,
        }

        #endregion

        #region NumericFilter enum

        public enum NumericFilter
        {
            /// <summary>
            /// �� ������������
            /// </summary>
            Non,
            /// <summary>
            /// �������� �����
            /// </summary>
            Equals,
            /// <summary>
            /// �������� �� �����
            /// </summary>
            NotEquals,
            /// <summary>
            /// ����� null
            /// </summary>
            IsNotNull,
            /// <summary>
            /// �� ����� null
            /// </summary>
            IsNull,
            /// <summary>
            /// �������� ���� ������ ���������
            /// </summary>
            More,
            /// <summary>
            /// �������� ���� ������ ���������
            /// </summary>
            Less,
            /// <summary>
            /// �������� ���� ����� ���������� ����������
            /// </summary>
            Between,
            /// <summary>
            /// ������ �� �������. ����� ����� ������ ������.
            /// </summary>
            BetweenColumns,
            /// <summary>
            /// ������ N ���� � �����. ������ ������ �� ������� ����.
            /// </summary>
            DaysAgoAndMore,
            /// <summary>
            /// �������� N ���� � �����. ������ ������ �� ������� ����.
            /// </summary>
            DaysLeftAndMore,
            /// <summary>
            /// ������� ����.
            /// </summary>
            ToDay,
            /// <summary>
            /// �������� ���� ������ ���������
            /// </summary>
            MoreOrEqual,
            /// <summary>
            /// �������� ���� ������ ���������
            /// </summary>
            LessOrEqual,
        }

        #endregion

        #region ReferenceFilter enum

        public enum ReferenceFilter
        {
            /// <summary>
            /// �� ������������
            /// </summary>
            Non,
            /// <summary>
            /// �������� �����
            /// </summary>
            Equals,
            /// <summary>
            /// �������� �� �����
            /// </summary>
            NotEquals,
            /// <summary>
            /// ����� null
            /// </summary>
            IsNotNull,
            /// <summary>
            /// �� ����� null
            /// </summary>
            IsNull,
            /// <summary>
            /// ����� ���� �� ������ �������� ������
            /// </summary>
            ContainsByRef,
            /// <summary>
            /// ����� ���� �� ������ ���������� �� ������
            /// </summary>
            StartsWithByRef,
            /// <summary>
            /// ����� ���� �� ������ ������������ �� ������
            /// </summary>
            EndsWithByRef,
            /// <summary>
            /// ����� ���� �� ������ �������� �����.
            /// </summary>
            ContainsWordsByRef,
            /// <summary>
            /// �������� ���������� �
            /// </summary>
            StartsWithCode,
            /// <summary>
            /// �������� �� ���������� �
            /// </summary>
            NotStartsWithCode,
            /// <summary>
            /// ����� ���� �� ������ �������� ����� �����.
            /// </summary>
            ContainsAnyWordByRef,
            /// <summary>
            /// �������� ���� ������ ��� ����� ���������
            /// </summary>
            MoreOrEqual,
            /// <summary>
            /// �������� ���� ������ ��� ����� ���������
            /// </summary>
            LessOrEqual
        }

        #endregion

        #region TextFilter enum

        public enum TextFilter
        {
            /// <summary>
            /// �� ������������
            /// </summary>
            Non,
            /// <summary>
            /// �������� �����
            /// </summary>
            Equals,
            /// <summary>
            /// �������� �� �����
            /// </summary>
            NotEquals,
            /// <summary>
            /// ����� null
            /// </summary>
            IsNotNull,
            /// <summary>
            /// �� ����� null
            /// </summary>
            IsNull,
            /// <summary>
            /// ����� ���� �������� ������
            /// </summary>
            Contains,
            /// <summary>
            /// ����� ���� �� �������� ������
            /// </summary>
            NotContains,
            /// <summary>
            /// ����� ���� ���������� �� ������
            /// </summary>
            StartsWith,
            /// <summary>
            /// ����� ���� ������������ �� ������
            /// </summary>
            EndsWith,
            /// <summary>
            /// ����� �������� �����.
            /// </summary>
            ContainsWords,
            /// <summary>
            /// ����� ���� �� ������ �������� ����� �����.
            /// </summary>
            ContainsAnyWord,
            /// <summary>
            /// ����� �� �������� �����.
            /// </summary>
            NotContainsWords,
            /// <summary>
            /// ����� ������ ������ ���������� ��������.
            /// </summary>
            LengthMore,
            /// <summary>
            /// ����� ������ ������ ���������� ��������.
            /// </summary>
            LengthLess,
        }

        #endregion
        
        #region FullTextSearchFilter enum

        public enum FullTextSearchFilter
        {
            /// <summary>
            /// ����� ���� �������� ������
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