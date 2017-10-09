using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Nat.Web.Controls.GenerationClasses.Data
{
    public class ParentDataSourceViewInfo
    {
        public IParentDataSourceViews DataSource { get; set; }
        public string ReferenceName { get; set; }
        public bool Mandatory { get; set; }
        public string FieldName { get; set; }
        public Type TableType { get; set; }
    }
}
