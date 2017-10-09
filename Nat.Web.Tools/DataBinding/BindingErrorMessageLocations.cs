/*
 * Created by: Denis M. Silkov
 * Created: 9 октября 2007 г.
 * Copyright © JSC New Age Technologies 2007
 */

namespace Nat.Web.Controls.DataBinding
{
    /// <summary>
    /// Enumeration for the various binding error message locations possible
    /// that determine where the error messages are rendered in relation to the
    /// control.
    /// </summary>
    public enum BindingErrorMessageLocations
    {
        /// <summary>
        /// Применяет стиль ошибки для контрола.
        /// </summary>
        ErrorCssClass,
        /// <summary>
        /// Displays an image icon to the right of the control
        /// </summary>
        WarningIconRight,
        /// <summary>
        /// Displays a text ! next to the control 
        /// </summary>
        TextExclamationRight,
        /// <summary>
        /// Displays the error message as text below the control
        /// </summary>
        RedTextBelow,
        /// <summary>
        /// Displays an icon and the text of the message below the control.
        /// </summary>
        RedTextAndIconBelow,
        None
    }
}