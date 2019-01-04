using System;
using System.Data.Linq;
using System.Drawing;
using System.Drawing.Imaging;
using System.Globalization;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Web;
using System.Web.UI;

namespace Nat.Web.Tools
{
    public class PageHelper
    {
        public static Control GetPostBackControl(Page page)
        {
            Control control = null;

            string ctrlname = page.Request.Params.Get("__EVENTTARGET");
            if (!string.IsNullOrEmpty(ctrlname))
            {
                control = page.FindControl(ctrlname);
            }
            else
            {
                foreach (string ctl in page.Request.Form)
                {
                    Control c = page.FindControl(ctl);
                    if (c is System.Web.UI.WebControls.Button)
                    {
                        control = c;
                        break;
                    }
                }
            }
            return control;
        }

        /// <summary>
        /// Показывает Warning сообщение на клиенте (Alert)
        /// </summary>
        /// <param name="control">контрол</param>
        /// <param name="type">тип</param>
        /// <param name="message">текст сообщения</param>
        public static void ShowMessage(Control control, Type type, string message)
        {
            ScriptManager.RegisterStartupScript(control, type, "warning", "alert('" + message + "');", true);
        }

        /// <summary>
        /// Загрузка файла на клиент
        /// </summary>
        /// <param name="sFileName">Имя файла на сервере</param>
        /// <param name="Response">Page.Response</param>
        /// <param name="Request">Page.Request</param>
        public static void DownloadFile(string fileName, HttpResponse Response)
        {
            fileName = fileName.Replace("\r\n", " ");
            Response.Clear();
            Response.ClearContent();
            var request = HttpContext.Current.Request;
            string contentDisposition;
            if (request.Browser.Browser == "IE" && (request.Browser.Version == "7.0" || request.Browser.Version == "8.0"))
                contentDisposition = "attachment; filename=" + Uri.EscapeDataString(fileName);
            else if (request.Browser.Browser == "Safari")
                contentDisposition = "attachment; filename=" + fileName;
            else
                contentDisposition = "attachment; filename*=UTF-8''" + Uri.EscapeDataString(fileName);            
            Response.AddHeader("Content-Disposition", contentDisposition);

            var fileStream = new FileStream(fileName, FileMode.OpenOrCreate, FileAccess.Read);
            var fileSize = fileStream.Length;
            try
            {
                Response.ContentType = "application/octet-stream";
                Response.AppendHeader("Connection", "keep-alive");
                Response.AppendHeader("Content-Length", fileSize.ToString());
//                Response.TransmitFile(sFileName);
                Response.WriteFile(fileName, 0, fileSize);
                Response.Flush();
            }
            finally
            {
                fileStream.Close();
                Response.End();
            }
        }

        public static void DownloadFile(byte[] buffer, string fileName, HttpResponse Response)
        {
            DownloadFile(buffer, fileName, Response, "application/octet-stream");
        }

        public static void DownloadFile(byte[] buffer, string fileName, HttpResponse Response, string contentType)
        {
            fileName = fileName.Replace("\r\n", " ");
            //Изменение размеров изображения и рисование уголка для фотографий на документы
            buffer = ImageUtils.ResizingGraphicsFile(buffer, HttpContext.Current.Request);
            //
            Response.Clear();
            Response.ClearContent();
            var request = HttpContext.Current.Request;
            string contentDisposition;
            if (request.Browser.Browser == "IE" && (request.Browser.Version == "7.0" || request.Browser.Version == "8.0"))
                contentDisposition = "attachment; filename=" + Uri.EscapeDataString(fileName);
            else if (request.Browser.Browser == "Safari")
                contentDisposition = "attachment; filename=" + fileName;
            else
                contentDisposition = "attachment; filename*=UTF-8''" + Uri.EscapeDataString(fileName);
            Response.AddHeader("Content-Disposition", contentDisposition);

            try
            {
                Response.ContentType = contentType;
                Response.AppendHeader("Connection", "keep-alive");
                Response.AppendHeader("Content-Length", buffer.Length.ToString());
                Response.OutputStream.Write(buffer, 0, buffer.Length);
                Response.Flush();
            }
            finally
            {
                Response.End();
            }
        }

        public static void DownloadFile(Stream stream, string fileName, HttpResponse Response)
        {
            try
            {
                fileName = fileName.Replace("\r\n", " ");

                Response.Clear();
                Response.ClearContent();
                var request = HttpContext.Current.Request;
                string contentDisposition;
                if (request.Browser.Browser == "IE" && (request.Browser.Version == "7.0" || request.Browser.Version == "8.0"))
                    contentDisposition = "attachment; filename=" + Uri.EscapeDataString(fileName);
                else if (request.Browser.Browser == "Safari")
                    contentDisposition = "attachment; filename=" + fileName;
                else
                    contentDisposition = "attachment; filename*=UTF-8''" + Uri.EscapeDataString(fileName);
                Response.AddHeader("Content-Disposition", contentDisposition);
                Response.ContentType = "application/octet-stream";
                Response.AppendHeader("Connection", "keep-alive");
                Response.AppendHeader("Content-Length", stream.Length.ToString());
                var buffer = new byte[4096];
                while (stream.Read(buffer, 0, 4096) > 0)
                    Response.OutputStream.Write(buffer, 0, buffer.Length);
                Response.Flush();
            }
            finally
            {
                stream.Close();
                Response.End();
            }
        }
    }
}