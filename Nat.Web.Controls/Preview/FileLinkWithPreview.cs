using System;
using System.ComponentModel;
using System.Text;
using System.Web;
using System.Web.Script.Serialization;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace Nat.Web.Controls.Preview
{
    [DefaultProperty("FileName")]
    public class FileLinkWithPreview : WebControl
    {
        private readonly HiddenField _hf = new HiddenField();

        public string NavigateUrl { get; set; }
        public string FileName { get; set; }
        
        public FileLinkBuilder FileLinkBuilder { get; set; }
        public string FileManager { get; set; }
        public string FieldName { get; set; }

        public string HiddenFieldClientID
        {
            get { return _hf.ClientID; }
        }

        protected override void OnInit(EventArgs e)
        {
            base.OnInit(e);
            Controls.Add(_hf);
            if (Page.IsPostBack && !string.IsNullOrEmpty(HttpContext.Current.Request.Form[_hf.UniqueID]))
            {
                var str = HttpContext.Current.Request.Form[_hf.UniqueID];
                var jss = new JavaScriptSerializer();
                FileLinkBuilder = jss.Deserialize<FileLinkBuilder>(str);
            }
        }

        public override bool EnableViewState
        {
            get
            {
                return false;
            }
        }

        protected override void Render(HtmlTextWriter writer)
        {
            if (!Enabled) return;
            if (FileLinkBuilder != null && string.IsNullOrEmpty(NavigateUrl))
                NavigateUrl = FileLinkBuilder.GetFileUrl(FileManager, FieldName);
            RenderFileLink(writer, FileLinkBuilder == null ? FileName : FileLinkBuilder.FileName, NavigateUrl);
        }

        public static void RenderFileLink(StringBuilder sb, FileLinkBuilder fileLinkBuilder, string fileManager, string fieldName)
        {
            if (fileLinkBuilder == null) return;
            RenderFileLink(sb, fileLinkBuilder.FileName, fileLinkBuilder.GetFileUrl(fileManager, fieldName));
        }

        public static string GetFileLink(FileLinkBuilder fileLinkBuilder, string fileManager, string fieldName)
        {
            if (fileLinkBuilder == null) return null;
            return fileLinkBuilder.GetFileUrl(fileManager, fieldName);
        }

        public static void RenderFileLink(StringBuilder sb, string fileName, string navigateUrl)
        {
            if (string.IsNullOrEmpty(fileName)) return;

            sb.Append("<a ");
            if (IsPictureFileName(fileName))
                sb.AppendFormat("onclick=\"OpenPreviewPicture('{0}', $(this).attr('value')); return false;\" ", navigateUrl);
            sb.AppendFormat("value=\"{0}\" target=\"_blank\" href=\"{1}\">{2}</a>", HttpUtility.HtmlAttributeEncode(fileName), navigateUrl, HttpUtility.HtmlEncode(fileName));
        }

        private static bool IsPictureFileName(string fileName)
        {
            return fileName.EndsWith(".jpg", StringComparison.OrdinalIgnoreCase)
                   || fileName.EndsWith(".jpeg", StringComparison.OrdinalIgnoreCase)
                   || fileName.EndsWith(".jpe", StringComparison.OrdinalIgnoreCase)
                   || fileName.EndsWith(".jfif", StringComparison.OrdinalIgnoreCase)
                   || fileName.EndsWith(".tif", StringComparison.OrdinalIgnoreCase)
                   || fileName.EndsWith(".tiff", StringComparison.OrdinalIgnoreCase)
                   || fileName.EndsWith(".gif", StringComparison.OrdinalIgnoreCase)
                   || fileName.EndsWith(".png", StringComparison.OrdinalIgnoreCase)
                   || fileName.EndsWith(".bmp", StringComparison.OrdinalIgnoreCase);
        }

        protected void RenderFileLink(HtmlTextWriter writer, string fileName, string navigateUrl)
        {
            if (!string.IsNullOrEmpty(fileName) && IsPictureFileName(fileName))
            {
                writer.AddAttribute(HtmlTextWriterAttribute.Onclick,
                                    string.Format("OpenPreviewPicture('{0}', $(this).attr('value')); return false;", navigateUrl));
            }
            writer.AddAttribute(HtmlTextWriterAttribute.Id, ClientID);
            writer.AddAttribute(HtmlTextWriterAttribute.Value, HttpUtility.HtmlAttributeEncode(fileName));
            writer.AddAttribute(HtmlTextWriterAttribute.Href, navigateUrl);
            writer.AddAttribute(HtmlTextWriterAttribute.Target, "_blank");
            writer.AddAttribute("FileManager", FileManager);
            writer.AddAttribute("FieldName", FieldName);
            writer.RenderBeginTag(HtmlTextWriterTag.A);
            writer.Write(HttpUtility.HtmlEncode(fileName));
            writer.RenderEndTag();
        }
        
        protected override void OnPreRender(EventArgs e)
        {
            base.OnPreRender(e);
            var jss = new JavaScriptSerializer();
            _hf.Value = jss.Serialize(FileLinkBuilder);
        }

        protected override object SaveViewState()
        {
            return new SerializeInfo(FileName, NavigateUrl, FileLinkBuilder);
        }

        protected override void LoadViewState(object savedState)
        {
            var pair = (SerializeInfo)savedState;
            if (pair != null)
            {
                FileName = pair.FileName;
                NavigateUrl = pair.NavigateUrl;
                if (FileLinkBuilder == null)
                    FileLinkBuilder = pair.FileLinkBuilder;
            }
        }

        [Serializable]
        private class SerializeInfo
        {
            public SerializeInfo()
            {
            }

            public SerializeInfo(string fileName, string navigateUrl, FileLinkBuilder fileLinkBuilder)
            {
                FileName = fileName;
                NavigateUrl = navigateUrl;
                FileLinkBuilder = fileLinkBuilder;
            }

            public string FileName { get; set; }
            public string NavigateUrl { get; set; }
            public FileLinkBuilder FileLinkBuilder { get; set; }
        }
    }
}