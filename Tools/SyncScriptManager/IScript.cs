namespace SyncScriptManager
{
    using System.Collections.Generic;

    public interface IScript<TScriptItem>
        where TScriptItem : class, IScriptName
    {
        List<TScriptItem> Scripts { get; set; }
    }
}