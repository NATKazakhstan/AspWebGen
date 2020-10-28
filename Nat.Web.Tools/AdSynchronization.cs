using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.Common;
using System.DirectoryServices;
using System.Linq;
using System.Security.Principal;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;
using Nat.Tools.Specific;

namespace Nat.Web.Tools
{
    public delegate void AdSynchronizationFunction<T>(T value);

    public class AdSynchronization
    {
        private static readonly Regex RegexDomain = new Regex(@"(?<domain>.+?)\\.+|.+?@(?<domain>.+?)(\.\w+)?$");
        private static readonly Regex RegexDC = new Regex(@"DC=(?<domain>\w+)");
        private const string DisplayName = "DisplayName";
        private const string ObjectsId = "objectSID";
        private const string EMail = "mail";
        private const string UAC = "userAccountControl";
        private const string AccountName = "sAMAccountName";
        private const string AdsPath = "adsPath";
        
        public List<ConnectionStringSettings> ConnectionStrings { get; set; }
        public AdSynchronizationFunction<string> LogFunction { get; set; }
        public AdSynchronizationFunction<decimal> SetProgressFunction { get; set; }
        public AdSynchronizationFunction<string> ErrorFunction { get; set; }
        public int CountUpdates { get; set; }
        public int CountInserts { get; set; }
        public int Count { get; set; }
        private StringBuilder _sbInserts;

        public void ExecuteSinc()
        {
            ExecuteSinc("(objectClass=user)");
        }

        public void ExecuteSinc(string filter)
        {
            int count = ConnectionStrings.Count;
            int i = 0;
            foreach (var connectionString in ConnectionStrings)
            {
                _sbInserts = new StringBuilder();
                var countStart = Count;
                var countInsertsStart = CountInserts;
                var countUpdatesStart = CountUpdates;
                i++;
                if (LogFunction != null) 
                    LogFunction("Выполнение синхронизации SID (AdSynchronization) соединение: " + connectionString.Name);
                try
                {
                    SyncSidAndNames(connectionString.ConnectionString, filter, i * 100 / count);
                    if (LogFunction != null)
                        LogFunction(string.Format(
                            "Выполнение синхронизации SID (AdSynchronization) ({4}) обработано записей/добавлено/обнавлено: {0}/{1}/{2}\r\nДобавлены: {3}",
                            Count - countStart, CountInserts - countInsertsStart, CountUpdates - countUpdatesStart, _sbInserts, connectionString.Name));
                }
                catch (Exception exception)
                {
                    var error = WriteError(exception);
                    if (ErrorFunction != null) ErrorFunction(error);
                    if (LogFunction != null) LogFunction(error);
                    if (LogFunction != null)
                        LogFunction(string.Format(
                            "Выполнение синхронизации SID (AdSynchronization) ({4}) обработано записей/добавлено/обнавлено: {0}/{1}/{2}\r\nДобавлены: {3}",
                            Count - countStart, CountInserts - countInsertsStart, CountUpdates - countUpdatesStart, _sbInserts, connectionString.Name));
                }
            }
        }

        protected static string WriteError(Exception exception)
        {
            if (exception == null) return "";
            return WriteError(exception.InnerException) + "\r\n" + exception.Message + "\r\n" + exception.StackTrace;
        }

        private static string GetProperty(SearchResult searchResult, string propertyName)
        {
            if (searchResult.Properties.Contains(propertyName))
                return searchResult.Properties[propertyName][0].ToString();
            return string.Empty;
        }

        public IEnumerable<string> GetNames(string filter)
        {
            var names = new List<string>();
            foreach (var connectionString in ConnectionStrings)
                names.AddRange(GetNames(connectionString.ConnectionString, filter));
            return names;
        }

        internal IEnumerable<string> GetNames(string actveDirectoryConnectionString, string filter)
        {
            var parent = new DirectoryEntry(actveDirectoryConnectionString);
            var searcher = new DirectorySearcher(parent, filter);
            searcher.PropertiesToLoad.Add(ObjectsId);
            searcher.PropertiesToLoad.Add(DisplayName);
            searcher.PropertiesToLoad.Add(EMail);
            searcher.PropertiesToLoad.Add(AccountName);
            searcher.PropertiesToLoad.Add(UAC);
            searcher.PageSize = 1000;
            var collection = searcher.FindAll();

            return collection.Cast<SearchResult>().
                Where(r => r.Properties.Contains(DisplayName) || r.Properties.Contains(AccountName)).
                Select(r => r.Properties.Contains(DisplayName)
                                   ? GetProperty(r, DisplayName)
                                   : GetProperty(r, AccountName)).
                ToList();
        }

        internal IEnumerable<string> GetLoginNames(string actveDirectoryConnectionString, string filter)
        {
            var parent = new DirectoryEntry(actveDirectoryConnectionString);
            var searcher = new DirectorySearcher(parent, filter);
            searcher.PropertiesToLoad.Add(ObjectsId);
            searcher.PropertiesToLoad.Add(DisplayName);
            searcher.PropertiesToLoad.Add(EMail);
            searcher.PropertiesToLoad.Add(AccountName);
            searcher.PropertiesToLoad.Add(UAC);
            searcher.PageSize = 1000;
            var collection = searcher.FindAll();

            return collection.Cast<SearchResult>().
                Where(r => r.Properties.Contains(DisplayName) || r.Properties.Contains(AccountName)).
                Select(r => GetLoginName(GetProperty(r, AccountName), GetProperty(r, AdsPath))).
                ToList();
        }
        /*internal static string GetLoginName(string accountName, string currentUserName)
        {
            if (string.IsNullOrEmpty(accountName))
                return accountName;
            var match = RegexDomain.Match(currentUserName);
            if (match.Success && match.Groups["domain"].Success)
                return match.Groups["domain"].Value + "\\" + accountName;
            return accountName;
        }*/

        internal static string GetLoginName(string accountName, string adsPath)
        {
            if (string.IsNullOrEmpty(accountName))
                return accountName;
            var match = RegexDC.Match(adsPath);
            if (match.Success && match.Groups["domain"].Success)
                return match.Groups["domain"].Value + "\\" + accountName;
            return accountName;
        }

        private void SyncSidAndNames(string actveDirectoryConnectionString, string filter, int progress)
        {
            var count = 0;
            var parent = new DirectoryEntry(actveDirectoryConnectionString);

            var searcher = new DirectorySearcher(parent, filter);
            searcher.PropertiesToLoad.Add(ObjectsId);
            searcher.PropertiesToLoad.Add(DisplayName);
            searcher.PropertiesToLoad.Add(AccountName);
            searcher.PropertiesToLoad.Add(EMail);
            searcher.PropertiesToLoad.Add(UAC);
            searcher.PageSize = 1000;
            SearchResultCollection collection = searcher.FindAll();

            #region initialize fields

            DbConnection connectionSelect = SpecificInstances.DbFactory.CreateConnection();
            DbConnection connectionUpdate = SpecificInstances.DbFactory.CreateConnection();
            DbConnection connectionInsert = SpecificInstances.DbFactory.CreateConnection();
            DbCommand cSelect = SpecificInstances.DbFactory.CreateCommand();
            DbCommand cUpdate = SpecificInstances.DbFactory.CreateCommand();
            DbCommand cInsert = SpecificInstances.DbFactory.CreateCommand();

            DbParameter idParameterUpdate = SpecificInstances.DbFactory.CreateParameter();
            DbParameter nameParameterUpdate = SpecificInstances.DbFactory.CreateParameter();
            DbParameter loginNameParameterUpdate = SpecificInstances.DbFactory.CreateParameter();
            DbParameter emailParameterUpdate = SpecificInstances.DbFactory.CreateParameter();
            DbParameter isDisabledParameterUpdate = SpecificInstances.DbFactory.CreateParameter();
            DbParameter sidInBase64ParameterUpdate = SpecificInstances.DbFactory.CreateParameter();

            DbParameter nameParameterInsert = SpecificInstances.DbFactory.CreateParameter();
            DbParameter loginNameParameterInsert = SpecificInstances.DbFactory.CreateParameter();
            DbParameter emailParameterInsert = SpecificInstances.DbFactory.CreateParameter();
            DbParameter sidInBase64ParameterInsert = SpecificInstances.DbFactory.CreateParameter();
            DbParameter isDisabledParameterInsert = SpecificInstances.DbFactory.CreateParameter();

            DbParameter sidParameterInsert = SpecificInstances.DbFactory.CreateParameter();
            DbParameter sidParameterSelect = SpecificInstances.DbFactory.CreateParameter();


            cSelect.CommandType = CommandType.Text;
            cSelect.CommandText = "select top (1) id, name, loginName, email, isDisabled, SidInBase64 from LOG_SidIdentification where sid = @sid";
            cSelect.Connection = connectionSelect;

            sidParameterSelect.DbType = DbType.String;
            sidParameterSelect.ParameterName = "sid";
            cSelect.Parameters.Add(sidParameterSelect);

            cUpdate.CommandType = CommandType.Text;
            cUpdate.CommandText = @"update LOG_SidIdentification 
                                    set name = @name, email = @email, isDisabled = @isDisabled, loginName = @loginName, SidInBase64 = @SidInBase64
                                    where id = @id";
            cUpdate.Connection = connectionUpdate;

            #region Update parameters

            nameParameterUpdate.DbType = DbType.String;
            nameParameterUpdate.ParameterName = "name";
            sidInBase64ParameterUpdate.DbType = DbType.String;
            sidInBase64ParameterUpdate.ParameterName = "SidInBase64";
            loginNameParameterUpdate.DbType = DbType.String;
            loginNameParameterUpdate.ParameterName = "loginName";
            emailParameterUpdate.DbType = DbType.String;
            emailParameterUpdate.ParameterName = "email";
            isDisabledParameterUpdate.DbType = DbType.Boolean;
            isDisabledParameterUpdate.ParameterName = "isDisabled";
            idParameterUpdate.DbType = DbType.Int64;
            idParameterUpdate.ParameterName = "id";
            cUpdate.Parameters.Add(idParameterUpdate);
            cUpdate.Parameters.Add(nameParameterUpdate);
            cUpdate.Parameters.Add(sidInBase64ParameterUpdate);
            cUpdate.Parameters.Add(loginNameParameterUpdate);
            cUpdate.Parameters.Add(emailParameterUpdate);
            cUpdate.Parameters.Add(isDisabledParameterUpdate);

            #endregion

            cInsert.CommandType = CommandType.Text;
            cInsert.CommandText = "insert into LOG_SidIdentification (sid, name, loginName, email, isDisabled, SidInBase64) values(@sid, @name, @loginName, @email, @isDisabled, @SidInBase64)";
            cInsert.Connection = connectionInsert;

            #region Insert parameters

            nameParameterInsert.DbType = DbType.String;
            nameParameterInsert.ParameterName = "name";
            loginNameParameterInsert.DbType = DbType.String;
            loginNameParameterInsert.ParameterName = "loginName";
            emailParameterInsert.DbType = DbType.String;
            emailParameterInsert.ParameterName = "email";
            isDisabledParameterInsert.DbType = DbType.Boolean;
            isDisabledParameterInsert.ParameterName = "isDisabled";
            sidParameterInsert.DbType = DbType.String;
            sidParameterInsert.ParameterName = "sid";
            sidInBase64ParameterInsert.DbType = DbType.String;
            sidInBase64ParameterInsert.ParameterName = "SidInBase64";

            cInsert.Parameters.Add(sidParameterInsert);
            cInsert.Parameters.Add(sidInBase64ParameterInsert);
            cInsert.Parameters.Add(nameParameterInsert);
            cInsert.Parameters.Add(loginNameParameterInsert);
            cInsert.Parameters.Add(emailParameterInsert);
            cInsert.Parameters.Add(isDisabledParameterInsert);

            #endregion

            #endregion

            try
            {
                connectionSelect.Open();
                connectionUpdate.Open();
                connectionInsert.Open();

                foreach (SearchResult entry in collection)
                {
                    if (entry.Properties[DisplayName].Count == 0) continue;
                    string name = GetProperty(entry, DisplayName);
                    string loginName = GetLoginName(GetProperty(entry, AccountName), GetProperty(entry, AdsPath));
                    if (!string.IsNullOrEmpty(name) && name.EndsWith("$") || !string.IsNullOrEmpty(loginName) && loginName.EndsWith("$"))
                        continue;
                    
                    count++;
                    string mail = GetProperty(entry, EMail);

                    var val = 0;
                    bool isDisabled = false;
                    if (entry.Properties[UAC].Count > 0)
                    {
                        if (entry.Properties[UAC][0] != null)
                            val = Convert.ToInt32(GetProperty(entry, UAC)); // entry.Properties[UAC][0];
                        isDisabled = val == (val | 0x2);
                    }
                    var sid = (byte[])entry.Properties[ObjectsId][0];
                    var sidStr = new SecurityIdentifier(sid, 0).Value;
                    var sidBase64 = Convert.ToBase64String(sid);
                    sidParameterSelect.Value = sidStr;
                    using (var reader = cSelect.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            object userName = reader.GetValue(1);
                            object userLoginName = reader.GetValue(2);
                            object userMail = reader.GetValue(3);//["email"]);
                            object userIsDisabled = reader.GetValue(4);//["isDisabled"]);
                            object sidInBase64 = reader.GetValue(5);//["SidInBase64"]);
                            if (userName != null && userName != DBNull.Value && ((string)userName).Equals(name)
                                && sidInBase64 != null && sidInBase64 != DBNull.Value && ((string)sidInBase64).Equals(sidBase64)
                                && userMail != null && userMail != DBNull.Value && ((string)userMail).Equals(mail)
                                && userIsDisabled != null && userIsDisabled != DBNull.Value && userIsDisabled.Equals(isDisabled)
                                && userLoginName != null && userLoginName != DBNull.Value && ((string)userLoginName).Equals(loginName))
                                continue;

                            emailParameterUpdate.Value = mail;
                            isDisabledParameterUpdate.Value = isDisabled;
                            nameParameterUpdate.Value = name;
                            sidInBase64ParameterUpdate.Value = sidBase64;
                            loginNameParameterUpdate.Value = loginName;
                            idParameterUpdate.Value = reader.GetValue(0);
                            cUpdate.ExecuteNonQuery();
                            CountUpdates++;
                        }
                        else
                        {
                            emailParameterInsert.Value = mail;
                            isDisabledParameterInsert.Value = isDisabled;
                            nameParameterInsert.Value = name;
                            loginNameParameterInsert.Value = loginName;
                            sidParameterInsert.Value = sidStr;
                            sidInBase64ParameterInsert.Value = sidBase64;
                            cInsert.ExecuteNonQuery();
                            CountInserts++;
                            if (_sbInserts.Length > 0)
                                _sbInserts.Append(", ");
                            _sbInserts.Append(name);
                        }
                    }
                }
                if (SetProgressFunction != null) SetProgressFunction(progress);
            }
            finally
            {
                connectionSelect.Close();
                connectionUpdate.Close();
                connectionInsert.Close();
            }
            Count += count;
        }
    }
}