using System;
using System.Collections.Generic;
using System.Data.Linq;
using System.IO;
using System.Linq;
using System.Security.Principal;
using System.Text;
using System.Web;
using System.Web.Script.Serialization;
using System.Web.Services;
using Nat.Web.Controls.Data;
using Nat.Web.Tools.Initialization;

namespace Nat.Web.Controls.SelectValues
{
    /// <summary>
    /// Summary description for WebServiceUploadFiles
    /// </summary>
    [WebService(Namespace = "http://tempuri.org/")]
    [WebServiceBinding(ConformsTo = WsiProfiles.BasicProfile1_1)]
    [System.ComponentModel.ToolboxItem(false)]
    [System.Web.Script.Services.ScriptService]
    public class WebServiceUploadFiles : WebService
    {
        [WebMethod]
        public string UploadTextFile(string text, string fileName, string subsystem, string tableId)
        {
            fileName = Path.GetFileNameWithoutExtension(fileName) + ".txt";
            using (var stream = new MemoryStream())
            using (var writer = new StreamWriter(stream))
            {
                writer.Write(text);
                writer.Flush();
                return UploadFile(stream.ToArray(), fileName, subsystem, tableId);
            }
        }

        [WebMethod]
        public string UploadImageFile(string imageBase64, string fileName, string subsystem, string tableId)
        {
            fileName = Path.GetFileNameWithoutExtension(fileName) + ".jpg";
            return UploadFile(Convert.FromBase64String(imageBase64), fileName, subsystem, tableId);
        }

        [WebMethod]
        public void DeleteUploadedFile(long id)
        {
            WebInitializer.Initialize();
            using (var db = new DBUploadFilesDataContext(WebSpecificInstances.DbFactory.CreateConnection()))
            {
                var row = db.SYS_FileUploads.FirstOrDefault(r => r.id == id && r.PersonSID == Tools.Security.User.GetSID());
                if (row != null)
                {
                    db.SYS_FileUploads.DeleteOnSubmit(row);
                    db.SubmitChanges();
                }
            }
        }

        [WebMethod]
        public string UploadFile(string bytesBase64, string fileName, string subsystem, string tableId)
        {
            return UploadFile(Convert.FromBase64String(bytesBase64), fileName, subsystem, tableId);
        }

        private string UploadFile(byte[] buffer, string fileName, string subsystem, string tableId)
        {
            WebInitializer.Initialize();
            using (var db = new DBUploadFilesDataContext(WebSpecificInstances.DbFactory.CreateConnection()))
            {
                var sfu = new SYS_FileUpload
                              {
                                  data = buffer,
                                  UploadDate = DateTime.Now,
                                  PersonSID = Tools.Security.User.GetSID(),
                                  SubSystemName = subsystem,
                                  dataFileName = fileName,
                              };
                db.SYS_FileUploads.InsertOnSubmit(sfu);
                db.SubmitChanges();
                var result = GetResult(fileName, tableId, sfu);
                return new JavaScriptSerializer().Serialize(result);
            }
        }

        private static UploadInfo GetResult(string fileName, string tableId, SYS_FileUpload sfu)
        {
            var fs = new FileSelector {ID = tableId,};
            var item = new ListControlItem {Value = sfu.id.ToString(), Text = fileName, Selected = true, Enabled = true,};
            return new UploadInfo
                       {
                           FileName = fileName,
                           ID = sfu.id,
                           TableID = tableId,
                           Html = fs.Render(item),
                           Item = item,
                       };
        }

        public class UploadInfo
        {
            public string FileName { get; set; }
            public long ID { get; set; }
            public string TableID { get; set; }
            public string Html { get; set; }
            public ListControlItem Item { get; set; }
        }
    }
}
