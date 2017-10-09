namespace Nat.Web.Tools.ExtNet.ExtNetConfig
{
    using System.Collections.Generic;

    using Ext.Net;

    public interface IListConfig
    {
        BoundList GetListConfig();
        IList<IListConfigItem> Columns { get; }
        string ServerMappingPerfix { get; set; }
        void AddModelFields(ModelFieldCollection fields);
    }
}
