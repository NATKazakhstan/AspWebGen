using System.IO;

namespace Nat.ExportInExcel
{
    public class FileStreamReader : FileStream
    {
        private readonly string _fileName;

        public FileStreamReader(string path, bool deleteFileAfterDispose)
            : base(path, FileMode.Open, FileAccess.Read)
        {
            _fileName = path;
            DeleteFileAfterDispose = deleteFileAfterDispose;
        }

        public bool DeleteFileAfterDispose { get; private set; }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
            if (disposing && DeleteFileAfterDispose)
                File.Delete(_fileName);
        }
    }
}