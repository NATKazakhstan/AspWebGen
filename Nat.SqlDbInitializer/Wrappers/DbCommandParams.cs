using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nat.SqlDbInitializer.Wrappers
{
    public class DbCommandParams
    {
        public string CommandTextAddStr { get; set; }
        public string CommandTextReplaceFrom { get; set; }
        public string CommandTextReplaceTo { get; set; }
        public Dictionary<string, string> CommandReplaceValuesDic { get; set; }
    }
}
