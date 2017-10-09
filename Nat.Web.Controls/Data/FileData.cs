using System.Data.Linq;

namespace Nat.Web.Controls.Data
{
    public class FileData
    {
        public string FileName { get; set; }
        public Binary Binary { get; set; }
        public byte[] Data { get; set; }

        public byte[] GetData()
        {
            return Binary != null ? Binary.ToArray() : Data;
        }
    }
}