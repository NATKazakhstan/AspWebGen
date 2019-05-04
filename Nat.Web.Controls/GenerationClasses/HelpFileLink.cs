using System.Web;
using System.Web.UI;
using System.IO;
using Nat.Web.Controls.Properties;

using System.Threading;

namespace Nat.Web.Controls.GenerationClasses
{
    using System.Web.Configuration;

    public class HelpFileLink : Control
    {
        public string NavigateUrl { get; set; }
        public string NavigateUrlDefault { get; set; }
        public string NavigateUrlOnAdd { get; set; }
        public string NavigateUrlOnRead { get; set; }

        protected override void Render(HtmlTextWriter writer)
        {
            if (!DesignMode)
            {
                var culture = Thread.CurrentThread.CurrentUICulture.Name;
                var helpSite = WebConfigurationManager.AppSettings["OpenHelpSite"];
                if (!string.IsNullOrEmpty(helpSite))
                {
                    RenderHyperLink(writer, string.Format(helpSite, culture));
                    return;
                }

                if (MainPageUrlBuilder.Current.IsNew && TryRenderHyperLink(writer, NavigateUrlOnAdd, culture))
                    return;
                if (MainPageUrlBuilder.Current.IsRead && TryRenderHyperLink(writer, NavigateUrlOnRead, culture))
                    return;
                if (TryRenderHyperLink(writer, NavigateUrl, culture))
                    return;
                if (TryRenderHyperLink(writer, NavigateUrlDefault, culture))
                    return;
            }
        }

        private static bool TryRenderHyperLink(HtmlTextWriter writer, string url, string culture)
        {
            string cultureUrl;
            if (!FileExists(url, culture, out cultureUrl))
                return false;
            cultureUrl = cultureUrl.Replace("\\", "/");
            RenderHyperLink(writer, cultureUrl);
            return true;
        }

        private static bool FileExists(string url, string culture, out string cultureUrl)
        {
            cultureUrl = "";
            if (string.IsNullOrEmpty(url)) return false;
            var filePath = HttpContext.Current.Request.MapPath(url);
            var fileDirCulture = Path.Combine(Path.GetDirectoryName(filePath), culture);
            var filePathCulture = Path.Combine(fileDirCulture, Path.GetFileName(filePath));
            if (File.Exists(filePathCulture))
            {
                fileDirCulture = Path.Combine(Path.GetDirectoryName(url), culture);
                cultureUrl = Path.Combine(fileDirCulture, Path.GetFileName(url));
                return true;
            }
            if (File.Exists(filePath))
            {
                cultureUrl = url;
                return true;
            }
            var fileLink = Path.GetDirectoryName(filePath);
            if (fileLink == null) return false;
            if (fileLink.EndsWith("\\"))
                fileLink = fileLink.Substring(0, fileLink.Length - 1) + ".link";
            else
                fileLink = fileLink + ".link";
            var dir = Path.GetDirectoryName(url);
            dir = Path.GetDirectoryName(dir);
            var fileName = Path.GetFileName(filePath);
            if (File.Exists(fileLink) && dir != null && fileName != null)
            {
                foreach (var line in File.ReadAllLines(fileLink))
                    if (FileExists(Path.Combine(Path.Combine(dir, line), fileName), culture, out cultureUrl))
                        return true;
            }
            return false;
        }

        private static void RenderHyperLink(HtmlTextWriter writer, string helpFileLink)
        {
            #region Render HyperLink

            writer.AddAttribute(HtmlTextWriterAttribute.Title, Resources.SHelp);
            writer.AddAttribute(HtmlTextWriterAttribute.Href, helpFileLink);
            writer.AddAttribute(HtmlTextWriterAttribute.Onclick,
                string.Format("javascript:var r = window.showModelessDialog(\"{0}\", '', 'resizable:yes;dialogHeight=' + (window.screen.height - window.screen.height/4) + 'px;dialogWidth=' + (window.screen.width - window.screen.width/4) +'px;');return false;",
                              HttpUtility.HtmlAttributeEncode(helpFileLink)));
            //writer.AddAttribute(HtmlTextWriterAttribute.Target, "_new");
            writer.RenderBeginTag(HtmlTextWriterTag.A);

            #region Render Image

            writer.AddAttribute(HtmlTextWriterAttribute.Border, "0");
            writer.AddAttribute(HtmlTextWriterAttribute.Alt, Resources.SHelp);
            writer.AddAttribute(HtmlTextWriterAttribute.Src, Themes.IconUrlHelpIcon);
            writer.RenderBeginTag(HtmlTextWriterTag.Img);
            writer.RenderEndTag();

            #endregion

            writer.RenderEndTag();

            #endregion
        }
    }
}
