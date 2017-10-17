using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Serialization;
using Nat.Update;
using SyncDbmlByScript.Properties;
using System.Diagnostics;
using System.Threading;

namespace SyncDbmlByScript
{
    using System.Collections.Specialized;

    using SyncScriptManager;

    class Program
    {
        public static readonly int Version = 2;

        static void Main(string[] args)
        {
//            Thread.Sleep(10000);
            if (AutoUpdater.SuggestAndUpdate(false))
            {
                return;
            }
            //Thread.Sleep(3000);
            if(args == null || args.Length == 0 || args[0].ToUpper() == "HELP" || args[0] == "?" )
            {
                Console.Write(Resources.Help);
                return;
            }

            string scriptFile = args[0];
            string dbmlFile = args.Length > 1 ? args[1] : "";
            ScriptList scripts;

            var ser = new XmlSerializer(typeof(ScriptList));
            if (dbmlFile.ToUpper() == "CSS")
            {
                using (var stream = new FileStream(scriptFile, FileMode.Create, FileAccess.Write))
                {
                    var obj = new ScriptList(new BaseSync[]
                                                         {
                                                             new SyncAssociation {AssociationName = "AssociationName"},
                                                             new SyncColumn {TableName = "TableName"},
                                                             new SyncTable {TableName="TableName"},
                                                         });
                    obj.Version = Version;
                    ser.Serialize(stream, obj);
                }
                return;
            }
            var isSdbml = Path.GetExtension(scriptFile).Equals(".sdbml", StringComparison.OrdinalIgnoreCase);
            if (isSdbml && dbmlFile == "")
            {
                dbmlFile = Path.GetFullPath(Path.Combine(Path.GetDirectoryName(scriptFile), "DB.dbml"));
                if (!File.Exists(dbmlFile))
                {
                    dbmlFile = Path.GetFullPath(Path.Combine(Path.GetDirectoryName(scriptFile), "../DB.dbml"));
                    if (!File.Exists(dbmlFile))
                        dbmlFile = Path.GetFullPath(Path.Combine(Path.GetDirectoryName(scriptFile), "../../DB.dbml"));
                }
            }
            var doc = XDocument.Load(dbmlFile);
            using (var stream = new FileStream(scriptFile, FileMode.Open, FileAccess.Read))
                scripts = (ScriptList)ser.Deserialize(stream);
            if (scripts.Version != Version)
            {
                Console.WriteLine("Version of file '{0}' not equals version of program '{1}'", scripts.Version, Version);
                return;
            }

            var syncManager = new ScriptManager();
            foreach (var publicSyncFile in Settings.Default.PublicSyncFiles)
            {
                var files = Directory.GetFiles(publicSyncFile, "*.sdbml", SearchOption.AllDirectories);
                syncManager.AddScripts<ScriptList, BaseSync>(files);
            }

            syncManager.MergeScripts<ScriptList, BaseSync>(scripts);

            var ignoreTables = Settings.Default.IgnoreTables.Cast<string>().ToDictionary(s => s);
            var modifyOnlyTables = Settings.Default.ModifyOnlyTables.Cast<string>().ToDictionary(s => s);
            scripts.IgnoreTables = ignoreTables;
            scripts.ModifyOnlyTables = modifyOnlyTables;
            foreach (var script in scripts.Scripts.Where(r => !r.SkipExecution))
            {
//                Console.WriteLine("Execute " + script.Command + " from " + script.GetType().FullName);
                if (script.IsChangeTables(ignoreTables))
                {
                    Console.WriteLine("Skip " + script.GetName());
                    continue;
                }
                if (modifyOnlyTables.Count > 0 && !script.IsChangeTables(modifyOnlyTables))
                {
                    Console.WriteLine("Skip " + script.GetName());
                    continue;
                }
                if ((!script.IsExecuted && !script.Execute(doc, scripts)) || (script.IsExecuted && !script.Success && script.MustHave))
                {
                    Console.WriteLine();
                    Console.WriteLine("Failed sync");
                    if (isSdbml)
                    {
                        Console.WriteLine("Do you want to ignore error? Y/N");
                        if (Console.ReadKey().Key == ConsoleKey.Y)
                            continue;
                        Console.WriteLine("Execution canceled. Press any key.");
                        Console.ReadKey();
                    } 
                    return;
                }
            }
            doc.Save(dbmlFile + ".new");
            File.Replace(dbmlFile + ".new", dbmlFile, dbmlFile + ".bak");
            Console.WriteLine();
            Console.WriteLine("Successful sync");
            
            var codeFile = Path.Combine(Path.GetDirectoryName(dbmlFile), "DB.designer.cs");
            var ns = Path.GetFileName(Path.GetDirectoryName(dbmlFile));
            var arguments = "/namespace:" + ns + " \"/code:" + codeFile + "\" \"" + dbmlFile + "\"";
            var startInfo = new ProcessStartInfo(Settings.Default.SqlMetal, arguments);
            startInfo.RedirectStandardOutput = true;
            startInfo.UseShellExecute = false;
            var process = Process.Start(startInfo);
            Console.WriteLine(process.StandardOutput.ReadToEnd());
            process.WaitForExit();
            
            if (isSdbml)
            {
                Console.WriteLine("Press any key");
                Console.Read();
            } 
        }

        public void Temp()
        {
            
        }
    }
}
