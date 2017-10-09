/*
 * Created by: Eugene P. Kolesnikov
 * Created: 2013.06.21
 * Copyright © JSC NAT Kazakhstan 2013
 */

namespace Nat.Web.Controls.ExtNet.Generation
{
    using System.IO;
    using System.Threading;
    using System.Web;

    using Ext.Net;

    public class HelpFileLinkExt : Button
    {
        public string NavigateUrl { get; set; }
        
        public string NavigateUrlDefault { get; set; }
        
        public string NavigateUrlOnAdd { get; set; }
        
        public string NavigateUrlOnRead { get; set; }

        public string WindowID { get; set; }

        public override Icon Icon
        {
            get
            {
                return Icon.Help;
            }

            set
            {
                base.Icon = value;
            }
        }

        protected override void OnPreRender(System.EventArgs e)
        {
            base.OnPreRender(e);
            if (!DesignMode)
            {
                var culture = Thread.CurrentThread.CurrentUICulture.Name;
                if (MainPageUrlBuilder.Current.IsNew && TryRenderHyperLink(this, NavigateUrlOnAdd, culture, WindowID))
                {
                    return;
                }

                if (MainPageUrlBuilder.Current.IsRead && TryRenderHyperLink(this, NavigateUrlOnRead, culture, WindowID))
                {
                    return;
                }

                if (TryRenderHyperLink(this, NavigateUrl, culture, WindowID))
                {
                    return;
                }

                if (TryRenderHyperLink(this, NavigateUrlDefault, culture, WindowID))
                {
                    return;
                }
            }
        }

        private static bool TryRenderHyperLink(Button button, string url, string culture, string windowID)
        {
            string cultureUrl;
            if (!FileExists(url, culture, out cultureUrl))
            {
                button.Visible = false;
                return false;
            }

            cultureUrl = cultureUrl.Replace("\\", "/");
            RenderHyperLink(button, cultureUrl, windowID);

            button.Visible = true;

            return true;
        }

        private static bool FileExists(string url, string culture, out string cultureUrl)
        {
            cultureUrl = string.Empty;
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

        private static void RenderHyperLink(Button component, string helpFileLink, string windowID)
        {
            component.Listeners.Click.Handler = string.Format(@"
                                var w = #{{" + windowID + @"}};
                                
                                w.loader.url = '{0}';
                                w.setTitle('{1}');
                                w.show();
                                if (!w.collapsed)
                                    w.loader.load();
", 
                                helpFileLink, 
                                Web.Controls.Properties.Resources.SHelp);
        }
    }
}