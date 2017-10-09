// (c) Copyright Microsoft Corporation.
// This source is subject to the Microsoft Permissive License.
// See http://www.microsoft.com/resources/sharedsource/licensingbasics/sharedsourcelicenses.mspx.
// All other rights reserved.

using System;

namespace AjaxControlToolkit
{
    /// <summary>
    /// Specifies this property is an element reference and should be converted during serialization.
    /// The default (e.g. cases without this attribute) will generate the element's ID
    /// </summary>
    [AttributeUsage(AttributeTargets.Property)]
    public sealed class ElementReferenceAttribute : Attribute
    {
        /// <summary>
        /// Constructs a new ElementReferenceAttribute
        /// </summary>
        public ElementReferenceAttribute()
        {
        }
    }
}
