/*
* Created by: Sergey V. Shpakovskiy
* Created: 2012.08.28
* Copyright © JSC NAT Kazakhstan 2012
*/

namespace Nat.Web.Tools.Export.Formatting
{
    using System;

    public class ConditionalFormattingVO
    {
        public ConditionalFormattingVO()
        {
        }

        public ConditionalFormattingVO(ConditionalFormattingVOType type)
        {
            Type = type;
        }

        public ConditionalFormattingVO(ConditionalFormattingVOType type, string value)
        {
            Type = type;
            Value = value;
        }

        public ConditionalFormattingVOType Type { get; set; }

        public string Value { get; set; }

        public decimal GetValue(decimal? minValue, decimal? maxValue)
        {
            switch (Type)
            {
                case ConditionalFormattingVOType.Num:
                    return Convert.ToDecimal(Value);
                case ConditionalFormattingVOType.AutoMax:
                case ConditionalFormattingVOType.Max:
                    return maxValue ?? 0;
                case ConditionalFormattingVOType.AutoMin:
                case ConditionalFormattingVOType.Min:
                    return minValue ?? 0;
                case ConditionalFormattingVOType.Percent:
                    return (maxValue - minValue) * Convert.ToDecimal(Value) / 100M + minValue ?? 0;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }
}