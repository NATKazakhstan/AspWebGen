using System.Web.Script.Serialization;
using System.Xml.Serialization;

namespace Nat.Web.Controls.GenerationClasses.HierarchyFields
{
    public class CellProperties
    {
        public string Key { get; set; }
        /// <summary>
        /// BackgroundColor
        /// </summary>
        public string BColor { get; set; }
        /// <summary>
        /// Pen Color (Font Color)
        /// </summary>
        public string PColor { get; set; }

        public int? Size { get; set; }
        public bool? Italic { get; set; }
        public bool? Bold { get; set; }
        public Aligment? HAligment { get; set; }

        [XmlIgnore]
        [ScriptIgnore]
        public string StyleId { get; set; }
    }
}
