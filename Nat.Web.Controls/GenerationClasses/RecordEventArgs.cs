/*
 * Created by: Sergey V. Shpakovskiy
 * Created: 20 декабря 2008 г.
 * Copyright © JSC New Age Technologies 2008
 */

using System.Collections.Generic;
using System.ComponentModel;
using System.Web;

namespace Nat.Web.Controls.GenerationClasses
{
    public class RecordEventArgs : CancelEventArgs
    {
        public RecordEventArgs()
        {
            ErrorMessages = new List<string>();
        }

        public List<string> ErrorMessages { get; set; }

        public void AddErrorMessage(string errorMessage)
        {
            Cancel = true;
            ErrorMessages.Add(errorMessage);
        }

        public string GetFullErrorMessage()
        {
            string errorMessage = "";
            foreach (var msg in ErrorMessages)
                errorMessage += HttpUtility.HtmlAttributeEncode(msg) + "<br/>";
            if (!string.IsNullOrEmpty(errorMessage))
                return errorMessage.Substring(0, errorMessage.Length - 5);
            return errorMessage;
        }

        public string GetFullErrorMessageOneLine()
        {
            string errorMessage = "";
            foreach (var msg in ErrorMessages)
                errorMessage += HttpUtility.HtmlAttributeEncode(msg) + ", ";
            if (!string.IsNullOrEmpty(errorMessage))
                return errorMessage.Substring(0, errorMessage.Length - 2);
            return errorMessage;
        }
    }
}