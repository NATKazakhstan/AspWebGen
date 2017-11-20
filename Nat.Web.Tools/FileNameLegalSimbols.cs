using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Nat.Tools;

namespace Nat.Web.Tools
{
    using System.IO;

    /// <summary>
    /// Автозамена не конкретных символов в имене файла
    /// </summary>
    public class FileNameLegalSimbols
    {
        public static string Correct(string fileName)
        {
            if (fileName.IsNullOrEmpty()) return fileName;
            char[] chars = { '"', '*', '<', '>', '?', '/', '\\', '|', '+' };
            fileName = Path.GetFileName(fileName) ?? fileName;
            return new string(fileName.Select(c => chars.Contains(c) ? '_' : c).ToArray());
        }
    }
}