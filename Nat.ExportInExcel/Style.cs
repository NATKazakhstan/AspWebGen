using System;
using Nat.Web.Controls.GenerationClasses.HierarchyFields;

namespace Nat.ExportInExcel
{
    public struct Style
    {
        public int? FillId { get; set; }
        public int? FontId { get; set; }
        public int? BorderId { get; set; }
        public int TextRotation { get; set; }
        public Aligment HorizontalAlignment { get; set; }
        public Aligment VerticalAlignment { get; set; }
        public bool WrapText { get; set; }

        public Style Clone()
        {
            return new Style
                       {
                           FillId = FillId,
                           BorderId = BorderId,
                           FontId = FontId,
                           HorizontalAlignment = HorizontalAlignment,
                           TextRotation = TextRotation,
                           VerticalAlignment = VerticalAlignment,
                           WrapText = WrapText,
                       };
        }

        public Style Clone(int fillId, int fontId)
        {
            return new Style
                       {
                           FillId = fillId,
                           FontId = fontId,
                           BorderId = BorderId,
                           HorizontalAlignment = HorizontalAlignment,
                           TextRotation = TextRotation,
                           VerticalAlignment = VerticalAlignment,
                           WrapText = WrapText,
                       };
        }

        public Style Clone(int textRotation)
        {
            return new Style
            {
                FillId = FillId,
                BorderId = BorderId,
                FontId = FontId,
                HorizontalAlignment = textRotation == 90 ? RotationAligment90(VerticalAlignment) : HorizontalAlignment,
                TextRotation = textRotation,
                VerticalAlignment = textRotation == 90 ? RotationAligment90(HorizontalAlignment) : VerticalAlignment,
                WrapText = WrapText,
            };
        }

        public Style Clone(Aligment horizontalAlignment)
        {
            return new Style
            {
                FillId = FillId,
                BorderId = BorderId,
                FontId = FontId,
                HorizontalAlignment = horizontalAlignment,
                TextRotation = TextRotation,
                VerticalAlignment = VerticalAlignment,
                WrapText = WrapText,
            };
        }

        public Style Clone(int textRotation, Aligment horizontalAlignment)
        {
            return new Style
            {
                FillId = FillId,
                BorderId = BorderId,
                FontId = FontId,
                HorizontalAlignment = textRotation == 90 ? RotationAligment90(VerticalAlignment) : horizontalAlignment,
                TextRotation = textRotation,
                VerticalAlignment = textRotation == 90 ? RotationAligment90(horizontalAlignment) : VerticalAlignment,
                WrapText = WrapText,
            };
        }

        private Aligment RotationAligment90(Aligment aligment)
        {
            switch (aligment)
            {
                case Aligment.Left:
                    return Aligment.Bottom;
                case Aligment.Right:
                    return Aligment.Top;
                case Aligment.Top:
                    return Aligment.Left;
                case Aligment.Bottom:
                    return Aligment.Right;
                case Aligment.Center:
                    return Aligment.Center;
                default:
                    throw new ArgumentOutOfRangeException("aligment");
            }
        }
    }
}
