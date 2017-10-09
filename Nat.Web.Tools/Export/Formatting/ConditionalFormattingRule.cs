/*
* Created by: Sergey V. Shpakovskiy
* Created: 2012.08.28
* Copyright © JSC NAT Kazakhstan 2012
*/

namespace Nat.Web.Tools.Export.Formatting
{
    using System.Collections.Generic;
    using System.Drawing;
    using System.Linq;

    public class ConditionalFormattingRule
    {
        public ConditionalFormattingRule()
        {
            Items = new List<ConditionalFormattingBaseItem>();
        }

        public ConditionalFormattingRule(ConditionalFormattingRuleType type, List<ConditionalFormattingBaseItem> items)
        {
            Items = items;
            Type = type;
        }

        public List<ConditionalFormattingBaseItem> Items { get; }

        public ConditionalFormattingRuleType Type { get; set; }

        public int Priority { get; set; }

        public Color? GetBackgroundColor(decimal value, ConditionalFormatting conditionalFormatting)
        {
            return Items.Select(r => r.GetBackgroundColor(value, conditionalFormatting)).FirstOrDefault(r => r != null);
        }
    }
}