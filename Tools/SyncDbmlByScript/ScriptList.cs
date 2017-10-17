using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;

namespace SyncDbmlByScript
{
    using SyncScriptManager;

    [Serializable()]
    public class ScriptList : IScript<BaseSync>
    {
        public int Version { get; set; }

        [XmlArrayItem(typeof(SyncAssociation))]
        [XmlArrayItem(typeof(SyncColumn))]
        [XmlArrayItem(typeof(SyncTable))]
        public List<BaseSync> Scripts { get; set; }
        
        [XmlIgnore]
        public IDictionary<string, string> IgnoreTables { get; set; }

        [XmlIgnore]
        public IDictionary<string, string> ModifyOnlyTables { get; set; }

        public ScriptList()
        {
            Scripts = new List<BaseSync>();
        }

        public ScriptList(int capacity)
        {
            Scripts = new List<BaseSync>(capacity);
        }

        public ScriptList(IEnumerable<BaseSync> collection)
        {
            Scripts = new List<BaseSync>(collection);
        }
    }
}
