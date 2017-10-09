namespace Nat.Web.Tools.ExtNet.ExtNetConfig
{
    using System.Collections.Generic;
    using System.Text;

    using Ext.Net;

    public abstract class BaseListConfig : IListConfig
    {
        private IList<IListConfigItem> propertyColumns;

        protected BaseListConfig(BaseListConfigContextEnum context)
        {
            Context = context;
            ItemSelectorCls = "x-boundlist-item";
            BoundListCls = context == BaseListConfigContextEnum.Grid
                               ? "Default-ListConfig-BoundListCls-Grid"
                               : "Default-ListConfig-BoundListCls";
            TableCls = "Default-ListConfig-TableCls";
        }

        protected abstract IList<IListConfigItem> GetColumns();

        public BoundList GetListConfig()
        {
            return new BoundList
                {
                    LoadingText = Controls.Properties.Resources.SSearchLoading,
                    ItemSelector = "." + ItemSelectorCls,
                    Cls = BoundListCls,
                    Tpl = GetTemplate(),
                };
        }

        public IList<IListConfigItem> Columns
        {
            get { return propertyColumns ?? (propertyColumns = GetColumns()); }
        }

        public void AddModelFields(ModelFieldCollection fields)
        {
            foreach (var column in Columns)
            {
                var field = new ModelField(column.ColumnName);
                if (!string.IsNullOrEmpty(column.ServerMaping))
                    field.ServerMapping = ServerMappingPerfix + column.ServerMaping;
                fields.Add(field);
            }
        }

        private XTemplate GetTemplate()
        {
            return new XTemplate
                {
                    Html = GetHtmlTemplate(),
                };
        }

        private string GetHtmlTemplate()
        {
            var sb = new StringBuilder();
            sb.AppendFormat(@"
<tpl for='.'>
    <tpl if='[xindex] == 1'>
        <table class='{0}'>
", TableCls);
            GetColumnsSettings(sb, 3);
            sb.AppendLine("            <thead>");
            GetHeaderRows(sb, 4);
            sb.AppendLine(@"            </thead>
    </tpl>");
            GetDataRows(sb, 1);
            sb.AppendLine(@"
    <tpl if='[xcount-xindex]==0'>
        </table>
    </tpl>
</tpl>");
            return sb.ToString();
        }

        private void GetColumnsSettings(StringBuilder sb, int spaces)
        {
            var spacesCount = spaces * 4;
            foreach (var column in Columns)
                sb.Append(' ', spacesCount).AppendFormat("<col width=\"{0}\" />\r\n", column.Width);
        }

        private void GetHeaderRows(StringBuilder sb, int spaces)
        {
            var spacesTh = (spaces + 1) * 4;
            sb.Append(' ', spaces * 4).AppendLine("<tr>");
            foreach (var column in Columns)
                sb.Append(' ', spacesTh).AppendFormat("<th class='{1}'>{0}</th>\r\n", column.Header, ThCls);
            sb.Append(' ', spaces * 4).AppendLine("</tr>");
        }

        private void GetDataRows(StringBuilder sb, int spaces)
        {
            var spacesTd = (spaces + 1) * 4;
            sb.Append(' ', spaces * 4).AppendFormat("<tr class='{0}'>\r\n", ItemSelectorCls);
            foreach (var column in Columns)
                sb.Append(' ', spacesTd).AppendFormat("<td>{{{0}}}</td>\r\n", column.ColumnName);
            sb.Append(' ', spaces * 4).AppendLine("</tr>");
        }

        public string BoundListCls { get; set; }
        public string TableCls { get; set; }
        public string ThCls { get; set; }
        public string ItemSelectorCls { get; set; }
        public string ServerMappingPerfix { get; set; }
        public BaseListConfigContextEnum Context { get; private set; }
    }
}