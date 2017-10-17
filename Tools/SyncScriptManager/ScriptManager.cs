namespace SyncScriptManager
{
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Xml.Serialization;

    public class ScriptManager
    {
        public Dictionary<string, ScriptItemList> Scripts = new Dictionary<string, ScriptItemList>();

        public void AddScripts<TScript, TScriptItem>(IEnumerable<string> scriptFileNames)
            where TScript : IScript<TScriptItem>
            where TScriptItem : class, IScriptName
        {
            var ser = new XmlSerializer(typeof(TScript));
            foreach (var scriptFileName in scriptFileNames)
            {
                using (var stream = new FileStream(scriptFileName, FileMode.Open, FileAccess.Read))
                {
                    Merge<TScript, TScriptItem>((TScript)ser.Deserialize(stream), Path.GetFileNameWithoutExtension(scriptFileName));
                }
            }
        }

        private void Merge<TScript, TScriptItem>(TScript script, string scriptFileName)
            where TScript : IScript<TScriptItem>
            where TScriptItem : class, IScriptName
        {
            foreach (var scriptItem in script.Scripts)
            {
                var name = scriptItem.GetName();

                var item = new ScriptItem
                    {
                        FileName = scriptFileName,
                        Script = scriptItem,
                    };
                ScriptItemList scriptList;
                if (!Scripts.ContainsKey(name))
                {
                    Scripts[name] = scriptList = new ScriptItemList(item);
                }
                else
                {
                    scriptList = Scripts[name];
                    scriptList.Add(item);
                }

                var isPrimary = name.Contains(scriptFileName);
                if (isPrimary) scriptList.Primary = item;
            }
        }

        public void MergeScripts<TScript, TScriptItem>(TScript script)
            where TScript : IScript<TScriptItem>
            where TScriptItem : class, IScriptName
        {
            var exists = script.Scripts.ToLookup(r => r.GetName());

            foreach (var list in Scripts.Where(r => r.Value.Primary != null).Where(r => !exists.Contains(r.Key)))
                script.Scripts.Add((TScriptItem)list.Value.Primary.Script);
        }
    }
}