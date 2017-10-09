/*
* Created by: Sergey V. Shpakovskiy
* Created: 2012.08.28
* Copyright © JSC NAT Kazakhstan 2012
*/

namespace Nat.Web.Tools.Export.Formatting
{
    using System;
    using System.Collections.Generic;
    using System.Drawing;

    public class ConditionalFormatting
    {
        private decimal? minValue;
        private decimal? maxValue;

        public ConditionalFormatting()
        {
            Columns = new List<string>();
        }

        public ConditionalFormatting(List<string> columns)
        {
            Columns = columns;
        }

        public ConditionalFormatting(List<string> columns, ConditionalFormattingRule rule)
        {
            Columns = columns;
            Rule = rule;
        }

        public List<string> Columns { get; private set; }

        public ConditionalFormattingRule Rule { get; set; }

        public Color? GetBackgroundColor(object value)
        {
            if (value == null || value is string || value is DateTime || value is TimeSpan || value is bool)
                return null;

            var decimalVal = Convert.ToDecimal(value);
            return Rule.GetBackgroundColor(decimalVal, this);
        }

        public void AddValue(object value)
        {
            if (value == null || value is string || value is DateTime || value is TimeSpan || value is bool)
                return;

            var decimalVal = Convert.ToDecimal(value);
            minValue = minValue == null ? decimalVal : Math.Min(minValue.Value, decimalVal);
            maxValue = maxValue == null ? decimalVal : Math.Max(maxValue.Value, decimalVal);
        }

        public decimal? GetMinValue()
        {
            return minValue;
        }

        public decimal? GetMaxValue()
        {
            return maxValue;
        }
    }
}