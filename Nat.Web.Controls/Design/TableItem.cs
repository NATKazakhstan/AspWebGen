using System;
using System.ComponentModel;
using System.Data;
using System.Configuration;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;
using System.Web.UI.HtmlControls;

namespace Nat.Web.Controls
{
    [Serializable]
    public class TableItem
    {
        private string tableName = "";
        private ISessionWorkerContainer sessionWorkerContainer;

        public TableItem()
        {
        }

        public TableItem(DataTable table)
        {
            tableName = table.TableName;
        }

        [Browsable(false)]
        [NotifyParentProperty(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public ISessionWorkerContainer SessionWorkerContainer
        {
            get { return sessionWorkerContainer; }
            set { sessionWorkerContainer = value; }
        }

        [DefaultValue("")]
        [NotifyParentProperty(true)]
        [TypeConverter(typeof(TableTypeConverter))]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
        public string TableName
        {
            get { return tableName; }
            set { tableName = value; }
        }

        public override string ToString()
        {
            if (string.IsNullOrEmpty(tableName))
                return base.ToString();
            return tableName;
        }
    }

}
