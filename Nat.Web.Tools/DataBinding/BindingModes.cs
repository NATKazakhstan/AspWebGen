/*
 * Created by: Denis M. Silkov
 * Created: 9 октября 2007 г.
 * Copyright © JSC New Age Technologies 2007
 */

namespace Nat.Web.Controls.DataBinding
{
    /// <summary>
    /// Determines how databinding is performed for the target control. Note that 
    /// if a Bindings is  marked for None or OneWay, the control will not 
    /// be unbound or in the case of None bound even when an explicit call to 
    /// ReadValue() or WriteValue() is made.
    /// </summary>
    public enum BindingModes
    {
        /// <summary>
        /// Databinding occurs for ReadValue() and WriteValue()
        /// </summary>
        TwoWay,
        /// <summary>
        /// DataBinding occurs for ReadValue() only
        /// </summary>
        OneWay,
        /// <summary>
        /// No binding occurs
        /// </summary>
        None
    }
}