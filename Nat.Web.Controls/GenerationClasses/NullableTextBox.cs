/*
 * Created by: Evgeny P. Kolesnikov
 * Created: 19.11.2008
 * Copyright © JSC NAT Kazakhstan 2008
 */

using System.ComponentModel;
using System.Globalization;
using System.Web.UI.WebControls;

namespace Nat.Web.Controls.GenerationClasses
{
    public class NullableTextBox : TextBox
    {
        [DefaultValue(false)]
        public bool IsNumeric { get; set; }

        [Bindable(BindableSupport.Default, BindingDirection.TwoWay)]
        public string Value
        {
            get
            {
                var text = base.Text;
                if (text == "") return null;
                if (IsNumeric)
                    return CorrectNumericValue(text);
                return text;
            }
            set { base.Text = value; }
        }

        public static string CorrectNumericValue(string text)
        {
            if (string.IsNullOrEmpty(text)) return text;
            if (text.Contains("."))
            {
                if (CultureInfo.CurrentCulture.NumberFormat.NumberDecimalSeparator != ".")
                    return text.Replace(".", CultureInfo.CurrentCulture.NumberFormat.NumberDecimalSeparator);
            }
            else if (text.Contains(","))
            {
                if (CultureInfo.CurrentCulture.NumberFormat.NumberDecimalSeparator != ",")
                    return text.Replace(",", CultureInfo.CurrentCulture.NumberFormat.NumberDecimalSeparator);
            }
            return text;
        }
    }
}
