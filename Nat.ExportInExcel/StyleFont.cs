using System.Drawing;

namespace Nat.ExportInExcel
{
    public struct StyleFont
    {
        public int Size { get; set; }
        public bool Bold { get; set; }
        public bool Italic { get; set; }
        public Color? Color { get; set; }
    }
}
