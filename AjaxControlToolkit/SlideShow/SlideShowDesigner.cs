// (c) Copyright Microsoft Corporation.
// This source is subject to the Microsoft Permissive License.
// See http://www.microsoft.com/resources/sharedsource/licensingbasics/sharedsourcelicenses.mspx.
// All other rights reserved.


using AjaxControlToolkit.Design;

namespace AjaxControlToolkit
{
    /// <summary>
    /// SlideShow designer
    /// </summary>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Security", "CA2117:AptcaTypesShouldOnlyExtendAptcaBaseTypes", Justification = "Security handled by base class")]
    public class SlideShowDesigner : ExtenderControlBaseDesigner<SlideShowExtender>
    {
        /// <summary>
        /// Signature of the page method for SlideShow's web service that
        /// is used to support adding/navigating to the page method from the designer
        /// </summary>
        /// <param name="contextKey">Optional user specific context</param>
        /// <returns>Slides to display</returns>
        [PageMethodSignature("SlideShow", "SlideShowServicePath", "SlideShowServiceMethod", "UseContextKey")]
        private delegate Slide[] GetSlides(string contextKey);
    }
}