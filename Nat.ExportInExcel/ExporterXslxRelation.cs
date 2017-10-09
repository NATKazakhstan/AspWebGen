using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Nat.ExportInExcel
{
    public class ExporterXslxRelation
    {
        public ExporterXslxRelation(string id, string type, string target)
        {
            Id = id;
            Type = type;
            Target = target;
        }

        public string Id { get; set; }
        public string Type { get; set; }
        public string Target { get; set; }
    }
}
