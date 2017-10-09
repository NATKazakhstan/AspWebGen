using System;
using System.Web.Script.Serialization;

namespace Nat.Web.Controls.Preview
{
    [Serializable]
    public class FileLinkBuilder
    {
        private Type _fileManagerType;

        public FileLinkBuilder()
        {
        }

        public FileLinkBuilder(string fileName, string keyValue)
        {
            FileName = fileName;
            KeyValue = keyValue;
        }

        public string FileName { get; set; }
        public string FileManager { get; set; }
        public string FieldName { get; set; }

        [ScriptIgnore]
        public Type FileManagerType
        {
            get { return _fileManagerType; }
            set
            {
                _fileManagerType = value;
                if (value != null)
                    FileManager = value.FullName;
            }
        }

        public string KeyValue { get; set; }

        public string GetFileUrl(string fileManager, string fieldName)
        {
            if (string.IsNullOrEmpty(fileManager))
                fileManager = FileManager;
            if (string.IsNullOrEmpty(fieldName))
                fieldName = FieldName;
            return string.Format("/MainPage.aspx/download?ManagerType={0}&fieldName={1}&ID={2}", fileManager, fieldName, KeyValue);
        }

        public override string ToString()
        {
            var jss = new JavaScriptSerializer();
            return jss.Serialize(this);
        }
    }
}
