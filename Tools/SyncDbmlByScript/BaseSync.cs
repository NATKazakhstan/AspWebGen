/*
 * Created by: Sergey V. Shpakovskiy
 * Created: 27 мая 2009 г.
 * Copyright © JSC New Age Technologies 2009
 */

using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using System.Xml.Linq;
using System.Xml.Serialization;

namespace SyncDbmlByScript
{
    using SyncScriptManager;

    [Serializable]
//    [XmlInclude(typeof(SyncAssociation))]
//    [XmlInclude(typeof(SyncColumn))]
//    [XmlInclude(typeof(SyncTable))]
    public abstract class BaseSync : IScriptName
    {
        private StringBuilder _sb;
        private bool _isValidation;
        private XElement _cacheLastTable;
        private string _cacheLastTableName;

        protected BaseSync()
        {
            Scheme = "dbo";
            MustHave = true;
        }

        public bool SkipExecution { get; set; }

        ILookup<string, BaseSync> lookupOfNames;
        private XElement _dataBase;
        private IEnumerable<XElement> _tables;

        private Dictionary<string, XElement> _tablesCache;
        protected XElement DataBase
        {
            get
            {
                if(_dataBase == null)
//                    _dataBase = Doc.Element("Database");
                    _dataBase = Doc.Element("{http://schemas.microsoft.com/linqtosql/dbml/2007}Database");
                return _dataBase;
            }
        }
        protected IEnumerable<XElement> Tables
        {
            get
            {
                if (_tables == null)
//                    _tables = DataBase.Elements("Table");
                    _tables = DataBase.Elements("{http://schemas.microsoft.com/linqtosql/dbml/2007}Table");
                return _tables;
            }
        }

        protected XElement GetTableType(string tableName)
        {
            var table = GetTable(tableName);
            if (table == null) return null;
            return table.Element("{http://schemas.microsoft.com/linqtosql/dbml/2007}Type");
        }

        protected XElement GetTable(string tableName)
        {
            if (tableName.Equals(_cacheLastTableName, StringComparison.OrdinalIgnoreCase) && _cacheLastTable != null)
                return _cacheLastTable;
            var dbFullName = (Scheme + "." + tableName).ToLower();
            _cacheLastTableName = tableName;

            if (_tablesCache == null)
            {
                var lookup = Tables
                    .Where(t => t.Attribute("Name") != null)
                    .Where(t => !string.IsNullOrEmpty(t.Attribute("Name").Value))
                    .ToLookup(t => t.Attribute("Name").Value.ToLower());
                _tablesCache = lookup.Where(r => r.Count() == 1).SelectMany(r => r).ToDictionary(r => r.Attribute("Name").Value.ToLower());
            }

            if (_tablesCache.ContainsKey(dbFullName))
                return _tablesCache[dbFullName];

            return null;
        }

        protected XElement EnsureExistsColumn(XElement table, string fullColumnName)
        {
            var column = GetColumn(table, fullColumnName);
            if (column == null)
                column = ExecuteScript(typeof(SyncColumn), fullColumnName);
            if (column == null && MustHave)
                Error("не была найдена колонка '{0}'", fullColumnName);
            return column;
        }

        protected XElement EnsureExistsTable(string tableName)
        {
            var table = GetTable(tableName);
            if (table == null)
                table = ExecuteScript(typeof(SyncTable), tableName);
            if (table == null && MustHave)
                Error("не была найдена таблица '{0}'", tableName);
            return table;
        }

        protected XElement ExecuteScript(Type type, string name)
        {
            if (lookupOfNames == null)
                lookupOfNames = Scripts.Scripts.ToLookup(r => r.GetName());
            
            if (!lookupOfNames.Contains(name))
                return null;

            foreach (var script in lookupOfNames[name])
            {
                if (script.GetType() == type)
                {
                    if (script.IsChangeTables(Scripts.IgnoreTables))
                    {
                        Console.WriteLine("Skip " + script.GetName());
                        return null;
                    }
                    if (Scripts.ModifyOnlyTables.Count > 0 && !script.IsChangeTables(Scripts.ModifyOnlyTables))
                    {
                        Console.WriteLine("Skip " + script.GetName());
                        continue;
                    }
                    if (MustHave) script.MustHave = true;
                    if (script.Execute(Doc, Scripts))
                        return script.GetObject();
                }
            }

            return null;
        }

        protected IEnumerable<XElement> GetColumns(XElement tableType)
        {
            return tableType.Elements("{http://schemas.microsoft.com/linqtosql/dbml/2007}Column");
        }

        protected IEnumerable<XElement> FilterByName(IEnumerable<XElement> coll, string name)
        {
            return coll.Where(
                a =>
                a.Attribute("Name") != null &&
                (a.Attribute("Name").Value.Equals(name, StringComparison.OrdinalIgnoreCase)
                || a.Attribute("Name").Value.Equals("[" + name + "]", StringComparison.OrdinalIgnoreCase)));
        }

        protected XElement GetColumn(XElement table, string columnName)
        {
            var element = table.Element("{http://schemas.microsoft.com/linqtosql/dbml/2007}Type");
            if (element == null) return null;
            var elements = FilterByName(element.Elements("{http://schemas.microsoft.com/linqtosql/dbml/2007}Column"), columnName).ToArray();
            if (elements.Length > 1) Error("Имеется несколько одинаковых колонок");
            return elements.Length == 0 ? null : elements[0];
        }

        protected XElement AddColumn(XElement table)
        {
            var element = table.Element("{http://schemas.microsoft.com/linqtosql/dbml/2007}Type");
            if (element == null) return null;
            var column = new XElement("{http://schemas.microsoft.com/linqtosql/dbml/2007}Column");
            element.Add(column);
            EnsureSb(100);
            _sb.Append("\tnew Column('");
            if (!string.IsNullOrEmpty(ActiveObject))
            {
                _sb.Append(ActiveObject);
            }
            _sb.AppendLine("')");
            return column;
        }

        protected XElement AddTable(string TableName, string TypeName, string CollectionName)
        {
            var table = new XElement("{http://schemas.microsoft.com/linqtosql/dbml/2007}Table");
            EnsureSb(100);
            _sb.Append("\tnew Table('");
            _sb.Append(TableName);
            _sb.AppendLine("')");

            DataBase.Add(table);
            ActiveObject = TableName;
            SetAttributeValue(table, "Name", Scheme + "." + TableName);
            SetAttributeValue(table, "Member", CollectionName);
            var tableType = new XElement("{http://schemas.microsoft.com/linqtosql/dbml/2007}Type");
            ActiveObject = TableName + ".Type";
            table.Add(tableType);
            SetAttributeValue(tableType, "Name", TypeName);

            if (_tablesCache != null)
                _tablesCache.Add((Scheme + "." + TableName).ToLower(), table);

            return table;
        }

        protected XElement AddAssociation(XElement table)
        {
            var element = table.Element("{http://schemas.microsoft.com/linqtosql/dbml/2007}Type");
            if (element == null) return null;
            var association = new XElement("{http://schemas.microsoft.com/linqtosql/dbml/2007}Association");
            element.Add(association);
            EnsureSb(100);
            _sb.Append("\tnew Association('");
            if (!string.IsNullOrEmpty(ActiveObject))
            {
                _sb.Append(ActiveObject);
            }
            _sb.AppendLine("')");
            return association;
        }

        protected XElement GetAssociation(string name, XElement table, string member)
        {
            var element = table.Element("{http://schemas.microsoft.com/linqtosql/dbml/2007}Type");
            if (element == null) return null;
            var elements = element.Elements("{http://schemas.microsoft.com/linqtosql/dbml/2007}Association").
                Where(a => a.Attribute("Name") != null && a.Attribute("Name").Value.Equals(name, StringComparison.OrdinalIgnoreCase));
            if (!string.IsNullOrEmpty(member))
                elements = elements.Where(a => a.Attribute("Member") != null && member.Equals(a.Attribute("Member").Value, StringComparison.OrdinalIgnoreCase));
            return elements.Count() != 1 ? null : elements.First();
        }

        protected XElement GetAssociation(string type, string thisKey, string otherKey, XElement table, string member)
        {
            var element = table.Element("{http://schemas.microsoft.com/linqtosql/dbml/2007}Type");
            if (element == null) return null;
            var elements = element.Elements("{http://schemas.microsoft.com/linqtosql/dbml/2007}Association").
                Where(
                a => 
                a.Attribute("ThisKey") != null && a.Attribute("ThisKey").Value.Equals(thisKey, StringComparison.OrdinalIgnoreCase)
                && a.Attribute("OtherKey") != null && a.Attribute("OtherKey").Value.Equals(otherKey, StringComparison.OrdinalIgnoreCase)
                && a.Attribute("Type") != null && a.Attribute("Type").Value.Equals(type, StringComparison.OrdinalIgnoreCase)
                );
            if (!string.IsNullOrEmpty(member))
                elements = elements.Where(a => a.Attribute("Member") != null && member.Equals(a.Attribute("Member").Value, StringComparison.OrdinalIgnoreCase));
            var countElements = elements.Count();
            if (countElements > 1) Error("Имеется несколько одинаковых ассоциаций");
            return countElements == 0 ? null : elements.First();
        }

        public bool Execute(XDocument dbml, ScriptList scripts)
        {
            Doc = dbml;
            Scripts = scripts;
            bool valid = false;
            try
            {
                try
                {
                    _isValidation = true;
                    valid = Validate();
                    _isValidation = false;
                    Success = valid && Execute() || !MustHave;
                }
                catch (CustomException)
                {
                    Success = !MustHave;
                }
            }
            catch (Exception e)
            {
                Error(e);
                Success = false;
            }
            finally
            {
                IsExecuted = true;
            }
            if (_sb != null)
            {
                Console.Write(_sb.ToString());
                Console.WriteLine("End");
            }
            return Success;
        }

        private ScriptList Scripts { get; set; }
        protected XDocument Doc { get; private set; }
        public string Scheme { get; set; }
        public string Command { get; set; }
        public bool MustHave { get; set; }
        public bool IsExecuted { get; set; }
        public bool Success { get; set; }
        protected abstract bool Execute();
        protected abstract bool Validate();
        protected abstract XElement GetObject();

        public virtual string GetName()
        {
            var ser = new XmlSerializer(GetType());
            using (var stream = new MemoryStream())
            {
                ser.Serialize(stream, this);
                return Encoding.Default.GetString(stream.GetBuffer());
            }
        }

        protected void Log(string message)
        {
            EnsureSb(message.Length * 4);
            _sb.Append("\t");
            _sb.AppendLine(message);
        }

        protected void Log(string format, string message)
        {
            EnsureSb(message.Length * 4);
            _sb.Append("\t");
            _sb.AppendFormat(format, message);
            _sb.AppendLine();
        }

        private void Error(Exception e)
        {
            Log("Error in execution:");
            _sb.AppendLine();
            ErrorRecur(e);
        }
        private void ErrorRecur(Exception e)
        {
            Log(e.Message);
            Log(e.StackTrace);
            _sb.AppendLine();
            if(e.InnerException != null) ErrorRecur(e.InnerException);
        }
        
        protected void Error(string message)
        {
            message = "Error: " + message;
            if (!_isValidation || MustHave)
                Log(message);
            throw new CustomException(message);
        }

        protected void Error(string format, string message)
        {
            message = string.Format(format, message);
            if (!_isValidation || MustHave)
                Log(message);
            throw new CustomException(message);
        }

        private void EnsureSb(int length)
        {
            if(_sb == null)
            {
                _sb = new StringBuilder(length);
                _sb.Append("Execute command '");
                _sb.Append(Command);
                _sb.Append("' of ");
                _sb.AppendLine(GetType().FullName);
                _sb.Append("\tName of object: ");
                _sb.AppendLine(GetName());
            }
        }

        protected void SetAttributeValue(XElement element, string name, string value)
        {
            SetAttributeValue(element, name, value, true);
        }

        protected void SetAttributeValue(XElement element, string name, string value, bool enableLog)
        {
            var attribute = element.Attribute(name);
            if ((attribute != null && attribute.Value.Equals(value))
                || (attribute == null && value == null))
                return;
            element.SetAttributeValue(name, value);
            if (!enableLog) return;
            EnsureSb(value == null ? 50 : value.Length);
            _sb.Append("\t");
            if (!string.IsNullOrEmpty(ActiveObject))
            {
                _sb.Append(ActiveObject);
                _sb.Append(".");
            }
            _sb.Append(name);
            _sb.Append(" = ");
            _sb.AppendLine(value);
        }

        protected string ActiveObject { get; set; }

        private class CustomException : Exception
        {
            public CustomException(string message) : base(message)
            {
            }
        }

        public abstract bool IsChangeTables(IDictionary<string, string> tables);
    }
}