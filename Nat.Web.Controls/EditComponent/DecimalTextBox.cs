/*
 * Created by: Evgeny P. Kolesnikov
 * Created: 08.01.2009
 * Copyright © JSC NAT Kazakhstan 2009
 */

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Web.UI;
using System.Web.UI.WebControls;
using Nat.Tools.Classes;
using Nat.Web.Tools;

namespace Nat.Web.Controls
{
    public class DecimalTextBox : TextBox, IClientElementProvider
    {
        #region Implementation of IClientElementProvider

        public ICollection<string> GetInputElements()
        {
            return new[] {ClientID };
        }

        public ICollection<Control> GetInputControls()
        {
            return new Control[] {this};
        }

        public ICollection<Pair<string, IFormatProvider>> GetInputFormatProviders()
        {
            return new[]
                {
                    new Pair<string, IFormatProvider>(ClientID, NumberFormatInfo.CurrentInfo)
                };
        }

        #endregion
    }
}