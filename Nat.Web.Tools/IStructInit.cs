namespace Nat.Web.Tools
{
    using System.Collections.Generic;

    public interface IStructInit
    {
        object StructInit(string parseValue);

        Dictionary<string, object> GetDic();
    }
}