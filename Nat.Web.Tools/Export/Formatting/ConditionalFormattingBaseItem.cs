/*
* Created by: Sergey V. Shpakovskiy
* Created: 2012.08.28
* Copyright © JSC NAT Kazakhstan 2012
*/

namespace Nat.Web.Tools.Export.Formatting
{
    using System.Drawing;

    public abstract class ConditionalFormattingBaseItem
    {
        public abstract Color? GetBackgroundColor(decimal value, ConditionalFormatting conditionalFormatting);
    }
}