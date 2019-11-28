using System;
using System.Web.Script.Serialization;
using System.Xml.Serialization;
using Nat.Tools.Filtering;

namespace Nat.Web.Controls.GenerationClasses.Filter
{
    [Serializable]
    public class FilterItem
    {
        public FilterItem()
        {
        }

        public FilterItem(string filterName, ColumnFilterType? columnFilterType, object[] values)
        {
            ColumnFilterType = columnFilterType;
            FilterName = filterName;
            Value1 = values == null || values.Length < 1 || values[0] == null
                         ? null
                         : values[0].ToString();
            Value2 = values == null || values.Length < 2 || values[1] == null
                         ? null
                         : values[1].ToString();
            IsDisabled = true;
        }

        public string FilterName { get; set; }
        public string FilterType { get; set; }
        public string Value1 { get; set; }
        public string Value2 { get; set; }
        public bool IsDisabled { get; set; }

        [ScriptIgnore]
        [XmlIgnore]
        public ColumnFilterType? ColumnFilterType
        {
            get
            {
                if (string.IsNullOrEmpty(FilterType)) return null;
                switch (FilterType)
                {
                    case "Non":
                        return Nat.Tools.Filtering.ColumnFilterType.None;
                    case "Equals":
                        return Nat.Tools.Filtering.ColumnFilterType.Equal;
                    case "NotEquals":
                        return Nat.Tools.Filtering.ColumnFilterType.NotEqual;
                    case "IsNotNull":
                        return Nat.Tools.Filtering.ColumnFilterType.NotNull;
                    case "StartsWith":
                        return Nat.Tools.Filtering.ColumnFilterType.StartWith;
                    case "EndsWith":
                        return Nat.Tools.Filtering.ColumnFilterType.EndWith;
                    case "EqualsCollection":
                        return Nat.Tools.Filtering.ColumnFilterType.In;
                    default:
                        return (ColumnFilterType)Enum.Parse(typeof(ColumnFilterType), FilterType);
                }
            }
            set
            {
                if (value == null)
                {
                    FilterType = null;
                    return;
                }

                FilterType = ConvertToFilterType(value.Value);
            }
        }

        internal static string ConvertToFilterType(ColumnFilterType value)
        {
            switch (value)
            {
                case Nat.Tools.Filtering.ColumnFilterType.None:
                case Nat.Tools.Filtering.ColumnFilterType.NotSet:
                    return "Non";
                case Nat.Tools.Filtering.ColumnFilterType.Equal:
                    return "Equals";
                case Nat.Tools.Filtering.ColumnFilterType.NotEqual:
                    return "NotEquals";
                case Nat.Tools.Filtering.ColumnFilterType.NotNull:
                    return "IsNotNull";
                case Nat.Tools.Filtering.ColumnFilterType.Custom:
                    return "Equals";
                case Nat.Tools.Filtering.ColumnFilterType.StartWith:
                    return "StartsWith";
                case Nat.Tools.Filtering.ColumnFilterType.EndWith:
                    return "EndsWith";
                case Nat.Tools.Filtering.ColumnFilterType.In:
                    return "EqualsCollection";
                case Nat.Tools.Filtering.ColumnFilterType.OutOf:
                    return "NotEqualsCollection";
                default:
                    return value.ToString();
            }
        }
    }
}
