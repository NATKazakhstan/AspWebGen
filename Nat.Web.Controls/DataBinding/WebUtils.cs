using System;
using System.Data;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Threading;
using System.Web;
using System.Web.UI;

namespace Nat.Web.Controls.DataBinding.Tools
{
    /// <summary>
    /// Summary description for WebUtils.
    /// </summary>
    public class WebUtils
    {
        #region ASP.Net Helper Functions

        /// <summary>
        /// Returns a fully qualified HTTP path from a partial path starting out with a ~.
        /// Same syntax that ASP.Net internally supports.
        /// </summary>
        /// <param name="RelativePath"></param>
        /// <returns></returns>
        public static string ResolveUrl(string RelativePath)
        {
            if(RelativePath == null)
                return null;

            if(RelativePath.IndexOf(":") != -1)
                return RelativePath;

            // *** Fix up image path for ~ root app dir directory
            if(RelativePath.StartsWith("~"))
            {
                if(HttpContext.Current != null)
                    return HttpContext.Current.Request.ApplicationPath + RelativePath.Substring(1);
                else
                    // *** Assume current directory is the base directory
                    return RelativePath.Substring(1);
            }

            return RelativePath;
        }

        /// <summary>
        /// This method returns a fully qualified server Url which includes
        /// the protocol, server, port in addition to the server relative Url.
        /// 
        /// It work like Page.ResolveUrl, but adds these to the beginning.
        /// This method is useful for generating Urls for AJAX methods
        /// </summary>
        /// <param name="ServerUrl">Any Url, either App relative or fully qualified</param>
        /// <param name="ServerControl">Required Page or Server Control so this method can call ResolveUrl.
        /// This parameter should be of the current form or a control on the form.</param>
        /// <returns></returns>
        public static string ResolveServerUrl(string ServerUrl)
        {
            if(ServerUrl.ToLower().StartsWith("http"))
                return ServerUrl;

            // *** Start by fixing up the Url an Application relative Url
            string Url = ResolveUrl(ServerUrl);

            Uri ExistingUrl = HttpContext.Current.Request.Url;
            Url = ExistingUrl.Scheme + "://" + ExistingUrl.Authority + Url;

            return Url;
        }
        
        /// <summary>
        /// Sets a user's Locale based on the browser's Locale setting. If no setting
        /// is provided the default Locale is used.
        /// </summary>
        public static void SetUserLocale(string CurrencySymbol)
        {
            HttpRequest Request = HttpContext.Current.Request;
            if(Request.UserLanguages == null)
                return;

            string Lang = Request.UserLanguages[0];
            if(Lang != null)
            {
                // *** Problems with Turkish Locale and upper/lower case
                // *** DataRow/DataTable indexes
                if(Lang.StartsWith("tr"))
                    return;

                if(Lang.Length < 3)
                    Lang = Lang + "-" + Lang.ToUpper();
                try
                {
                    Thread.CurrentThread.CurrentCulture = new CultureInfo(Lang);

                    if(CurrencySymbol != null && CurrencySymbol != "")
                    {
                        Thread.CurrentThread.CurrentCulture.NumberFormat.CurrencySymbol =
                            CurrencySymbol;
                    }
                }
                catch
                {
                    ;
                }
            }
        }

        /// <summary>
        /// Finds a Control recursively. Note finds the first match and exits
        /// </summary>
        /// <param name="root"></param>
        /// <param name="id"></param>
        /// <returns></returns>
        public static Control FindControlRecursive(Control root, string id)
        {
            if(root.ID == id)
                return root;

            Control control = root.FindControl(id);
            if (control != null && control.ID == id)
                return control;

            foreach(Control subControl in root.Controls)
            {
                control = FindControlRecursive(subControl, id);
                if(control != null)
                    return control;
            }

            return null;
        }
        
        /// <summary>
        /// Creates the headers required to force the current request to not go into 
        /// the client side cache, forcing a reload of the page.
        /// 
        /// This method can be called anywhere as part of the Response processing to 
        /// modify the headers. Use this for any non POST pages that should never be 
        /// cached.
        /// <seealso>Class WebUtils</seealso>
        /// </summary>
        /// <param name="Response"></param>
        /// <returns>Void</returns>
        public static void ForceReload()
        {
            HttpResponse Response = HttpContext.Current.Response;
            Response.Expires = 0;
            Response.AppendHeader("Pragma", "no-cache");
            Response.AppendHeader("Cache-Control", "no-cache, mustrevalidate");
        }


        /// <summary>
        /// Returns the result from an ASPX 'template' page in the /templates directory of this application.
        /// This method uses an HTTP client to call into the Web server and retrieve the result as a string.
        /// </summary>
        /// <param name="TemplatePageAndQueryString">The name of a page (ASPX, HTM etc.) in the Templates directory to retrieve plus the querystring</param>
        /// <param name="ErrorMessage">If this method returns null this message will contain the error info</param>
        /// <returns>Merged Text or null if an HTTP error occurs - note: could also return an Error page HTML result if the template page has an error.</returns>
        public static string AspTextMerge(string TemplatePageAndQueryString, ref string ErrorMessage)
        {
            string MergedText = "";

            // *** Save the current request information
            HttpContext Context = HttpContext.Current;

            // *** Fix up the path to point at the templates directory
            TemplatePageAndQueryString = Context.Request.ApplicationPath +
                                         "/templates/" + TemplatePageAndQueryString;

            // *** Now call the other page and load into StringWriter
            StringWriter sw = new StringWriter();
            try
            {
                // *** IMPORTANT: Child page's FilePath still points at current page
                //                QueryString provided is mapped into new page and then reset
                Context.Server.Execute(TemplatePageAndQueryString, sw);
                MergedText = sw.ToString();
            }
            catch(Exception ex)
            {
                MergedText = null;
                ErrorMessage = ex.Message;
            }

            return MergedText;
        }


        /// <summary>
        /// Returns just the Path of a full Url. Strips off the filename and querystring
        /// </summary>
        /// <param name="url"></param>
        /// <returns></returns>
        public static string GetUrlPath(string url)
        {
            int lnAt = url.LastIndexOf("/");
            if(lnAt > 0)
                return url.Substring(0, lnAt + 1);
            return "/";
        }

        #endregion
    }
}