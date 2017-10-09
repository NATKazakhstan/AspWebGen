using System.ComponentModel;
using System.Web.UI.WebControls;

namespace Nat.Web.Controls
{
    public class TemplateFieldExt : TemplateField, IColumnName
    {
        [DefaultValue("")]
        public string ColumnName
        {
            get { return (string)ViewState["columnName"] ?? ""; }
            set { ViewState["columnName"] = value; }
        }

        protected override void CopyProperties(DataControlField newField)
        {
            base.CopyProperties(newField);
            TemplateFieldExt fieldExt = (TemplateFieldExt)newField;
            fieldExt.ColumnName = ColumnName;
        }

        protected override DataControlField CreateField()
        {
            return new TemplateFieldExt();
        }
    }
}