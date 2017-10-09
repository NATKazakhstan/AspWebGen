using System;
using System.Web.UI.WebControls;

namespace Nat.Web.Controls
{
    public class TemplateFieldExpand : TemplateField, IGridColumn
    {
        public string ColumnName
        {
            get { return "fTree"; }
            set { throw new NotImplementedException(); }
        }

        public Array GetValues(GridViewExt grid)
        {
            throw new NotImplementedException();
        }
    }
}