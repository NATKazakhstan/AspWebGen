/*
* Created by: Sergey V. Shpakovskiy
* Created: 2012.08.28
* Copyright © JSC NAT Kazakhstan 2012
*/

namespace Nat.Web.Tools.Export.Formatting
{
    using System.Drawing;

    public class ConditionalFormattingColorScale : ConditionalFormattingBaseItem
    {
        public ConditionalFormattingVO Min { get; set; }
        
        public ConditionalFormattingVO Avg { get; set; }

        public ConditionalFormattingVO Max { get; set; }

        public Color ColorMin { get; set; }

        public Color ColorAvg { get; set; }

        public Color ColorMax { get; set; }

        public override Color? GetBackgroundColor(decimal value, ConditionalFormatting conditionalFormatting)
        {
            var minValue = conditionalFormatting.GetMinValue();
            var maxValue = conditionalFormatting.GetMaxValue();
            var avgValue = Avg.GetValue(minValue, maxValue);

            var newMinValue = Min.GetValue(minValue, maxValue);
            if (newMinValue <= avgValue)
                minValue = newMinValue;

            var newMaxValue = Max.GetValue(minValue, maxValue);
            if (newMaxValue >= avgValue)
                maxValue = newMaxValue;

            if (maxValue == minValue)
            {
                if (value <= newMinValue && Min.Type == ConditionalFormattingVOType.Num)
                    return ColorMin;

                if (value >= newMaxValue && Max.Type == ConditionalFormattingVOType.Num)
                    return ColorMax;

                return value < 0 ? ColorMin : ColorMax;
            }

            if (value <= minValue)
                return ColorMin;

            if (value >= maxValue)
                return ColorMax;

            if (value == avgValue)
                return ColorAvg;

            if (value < avgValue)
                return MergeColors(value, minValue ?? 0, avgValue, ColorMin, ColorAvg);

            return MergeColors(value, avgValue, maxValue ?? 0, ColorAvg, ColorMax);
        }

        private Color MergeColors(decimal value, decimal value1, decimal value2, Color color1, Color color2)
        {
            var full = value2 - value1;
            var k1 = (value - value1) / full;
            var k2 = (value2 - value) / full;
            return Color.FromArgb(
                (int)(color1.A * k2 + color2.A * k1),
                (int)(color1.R * k2 + color2.R * k1),
                (int)(color1.G * k2 + color2.G * k1),
                (int)(color1.B * k2 + color2.B * k1));
        }
    }
}