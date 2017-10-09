using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Nat.Tools.Filtering;

namespace Nat.Web.Controls.GenerationClasses.Filter
{
    public static class FilterItemExtensions
    {
        public static void AddFilter(this List<FilterItem> filterItems, string filterName, ColumnFilterType? columnFilterType, object[] values)
        {
            filterItems.Add(new FilterItem(filterName, columnFilterType, values));
        }

        public static void AddEquals(this List<FilterItem> filterItems, string filterName, object value)
        {
            filterItems.Add(new FilterItem(filterName, ColumnFilterType.Equal, new[] {value, ""}));
        }

        public static void AddEquals(this List<FilterItem> filterItems, string filterName, object value1, object value2)
        {
            filterItems.Add(new FilterItem(filterName, ColumnFilterType.Equal, new[] { value1, value2 }));
        }

        public static void AddIsFalse(this List<FilterItem> filterItems, string filterName)
        {
            filterItems.Add(new FilterItem(filterName, ColumnFilterType.NotEqual, new object[] { true }));
        }

        public static void AddIsTrue(this List<FilterItem> filterItems, string filterName)
        {
            filterItems.Add(new FilterItem(filterName, ColumnFilterType.Equal, new object[] { true }));
        }
    }
}