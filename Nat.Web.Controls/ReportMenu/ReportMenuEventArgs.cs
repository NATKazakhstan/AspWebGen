/*
 * Created by: Sergey V. Shpakovskiy
 * Created: 16 мая 2008 г.
 * Copyright © JSC New Age Technologies 2008
 */

using System.ComponentModel;

namespace Nat.Web.Controls.ReportMenu
{
    public class ReportMenuEventArgs : CancelEventArgs
    {
        private readonly string text;
        private readonly string value;
        private string redircet;

        public ReportMenuEventArgs(string text, string value, string redircet)
        {
            this.value = value;
            this.text = text;
            this.redircet = redircet;
        }

        public string Redircet
        {
            get { return redircet; }
            set { redircet = value; }
        }

        public string Text
        {
            get { return text; }
        }

        public string Value
        {
            get { return value; }
        }
    }
}