namespace SyncScriptManager
{
    using System.Collections.Generic;

    public class ScriptItemList
    {
        private List<ScriptItem> list = new List<ScriptItem>();

        public ScriptItemList(ScriptItem item)
        {
            Add(item);
        }

        public ScriptItem Primary { get; set; }

        public void Add(ScriptItem item)
        {
            list.Add(item);
            item.Script.MustHave = false;
            item.Script.SkipExecution = true;
        }
    }
}