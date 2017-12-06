namespace Nat.ExportInExcel
{
    using System;
    using System.IO;

    public class ExportResultArgs
    {
        public Stream Stream { get; set; }
        public string ErrorMessage { get; set; }
        public Exception Exception { get; set; }
    }
}