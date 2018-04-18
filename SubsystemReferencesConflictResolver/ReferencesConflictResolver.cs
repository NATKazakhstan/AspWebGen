namespace SubsystemReferencesConflictResolver
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Data.SqlClient;
    using System.Linq;
    using System.Reflection;
    using System.Text.RegularExpressions;
    using System.Web;
    using System.Web.Compilation;

    using Nat.Tools.Specific;
    using Nat.Web.Controls.GenerationClasses;
    using Nat.Web.Controls.GenerationClasses.Data;
    using Nat.Web.Tools;

    using SubsystemReferencesConflictResolver.Properties;

    public class ReferencesConflictResolver
    {
        private const string TableGroup = "Table";
        private const string ColumnGroup = "Column";
        private const string ReferenceGroup = "Reference";
        private const string ConstraintGroup = "Constraint";
        private static readonly Dictionary<string, string> ProjectCodesCache = new Dictionary<string, string>();
        private static readonly Dictionary<string, string> ProjectHeadersCache = new Dictionary<string, string>();
        private static readonly Dictionary<string, string> ProjectFieldHeadersCache = new Dictionary<string, string>();

        private static readonly Regex DeleteRegex = new Regex(@"DELETE.*?REFERENCE.*?""(?<Reference>.*?)"".*?""(?<Catalog>.*?)"".*?""(\w+\.)?(?<Table>.*?)"".*?'(?<Column>.*?)'", RegexOptions.Compiled);
        private static readonly Regex ConstraintRegex = new Regex(@"CHECK constraint.*?""(?<Constraint>.*?)"".*?""(?<Catalog>.*?)"".*?""(\w+\.)?(?<Table>.*?)""", RegexOptions.Compiled);

        private readonly DBDataContext db;

        private ReferencesConflictResolver(DBDataContext db)
        {
            this.db = db;
        }

        public static bool AddErrorMessage(SqlException exception, Action<string> addErrorMessage, long id, string originalTableName)
        {
            using (var db = new DBDataContext(SpecificInstances.DbFactory.CreateConnection()))
            {
                var resolver = new ReferencesConflictResolver(db);
                return resolver.Resolve(exception, addErrorMessage, id, originalTableName);
            }
        }

        private bool Resolve(SqlException exception, Action<string> addErrorMessage, long id, string originalTableName)
        {
            var resolve = false;
            for (int i = 0; i < exception.Errors.Count; i++)
            {
                if (exception.Errors[i].Number == 547)
                    resolve |= Resolve(exception, exception.Errors[i], addErrorMessage, id, originalTableName);
                else
                    resolve |= CustomErrorResolve(exception, exception.Errors[i], addErrorMessage, id, originalTableName);
            }

            if (!resolve)
            {
                var cantDeleteWrited = false;
                for (int i = 0; i < exception.Errors.Count; i++)
                {
                    if (exception.Errors[i].Number == 547)
                    {
                        if (cantDeleteWrited)
                            continue;

                        addErrorMessage(Nat.Web.Controls.Properties.Resources.ECanNotDeleteUseInSystem);
                        cantDeleteWrited = true;
                    }
                    else if (i == 0 || exception.Errors[i].Number != 3621)
                    {
                        addErrorMessage(exception.Errors[i].Message);
                        cantDeleteWrited = true;
                    }
                }
            }

            return resolve;
        }

        private bool CustomErrorResolve(SqlException exception, SqlError error, Action<string> addErrorMessage, long id, string originalTableName)
        {
            var resolve = false;
            var data = db.SYS_ReferencesConflictResolvers.Where(r => r.RegexSearch != null).ToList();
            foreach (var resolverRow in data)
            {
                var match = Regex.Match(error.Message, resolverRow.RegexSearch);
                if (match.Success)
                    resolve |= RowResolve(resolverRow, match, exception, error, addErrorMessage, id, originalTableName);
            }

            return resolve;
        }

        private bool Resolve(SqlException exception, SqlError error, Action<string> addErrorMessage, long id, string originalTableName)
        {
            var match = DeleteRegex.Match(error.Message);
            if (match.Success)
                return DeleteResolve(match, exception, error, addErrorMessage, id, originalTableName);
            match = ConstraintRegex.Match(error.Message);
            if (match.Success)
                return ConstraintResolve(match, exception, error, addErrorMessage, id, originalTableName);
            return false;
        }

        private bool DeleteResolve(Match match, SqlException exception, SqlError error, Action<string> addErrorMessage, long id, string originalTableName)
        { 
            var tableName = match.Groups[TableGroup].Value;
            var columnName = match.Groups[ColumnGroup].Value;
            var referenceName = match.Groups[ReferenceGroup].Value;

            if (tableName.Contains("[") || tableName.Contains("]") || columnName.Contains("[") || columnName.Contains("]"))
                return false;
            
            var referencedTableName = GetReferencedTableName(referenceName);
            if (originalTableName.Equals(referencedTableName, StringComparison.OrdinalIgnoreCase))
                SimpleResolve(addErrorMessage, id, tableName, columnName, originalTableName);
            else if (!DependentResolve(addErrorMessage, id, tableName, columnName, originalTableName, exception, error))
                return false;

            return true;
        }

        private bool ConstraintResolve(Match match, SqlException exception, SqlError error, Action<string> addErrorMessage, long id, string originalTableName)
        {
            var tableName = match.Groups[TableGroup].Value;
            var constraintName = match.Groups[ConstraintGroup].Value;
            var resolverRow = db.SYS_ReferencesConflictResolvers.FirstOrDefault(r => r.ConstraintName == constraintName && r.TableName == tableName);
            if (resolverRow == null)
                return false;

            return RowResolve(resolverRow, match, exception, error, addErrorMessage, id, originalTableName);
        }

        private bool RowResolve(SYS_ReferencesConflictResolver resolverRow, Match match, SqlException exception, SqlError error, Action<string> addErrorMessage, long id, string originalTableName)
        {
            if (LocalizationHelper.IsCultureKZ && !string.IsNullOrEmpty(resolverRow.ErrorMessageKz))
            {
                addErrorMessage(resolverRow.ErrorMessageKz);
                return true;
            }

            if (!LocalizationHelper.IsCultureKZ && !string.IsNullOrEmpty(resolverRow.ErrorMessageRu))
            {
                addErrorMessage(resolverRow.ErrorMessageRu);
                return true;
            }

            return false;
        }

        private bool DependentResolve(Action<string> addErrorMessage, long id, string tableName, string columnName, string originalTableName, SqlException exception, SqlError error)
        {
            var args = new TableParametersArgs
                {
                    ConflictedTableCode = tableName,
                    ConflictedColumnCode = columnName,
                    DeleteID = id,
                    DeleteTable = originalTableName,
                    Exception = exception,
                    SqlError = error,
                };
            SetParametersByCustomCode(args, GetProjectCode(args.ConflictedTableCode));

            var result = false;
            foreach (var dependentTable in args.DependentTables)
            {
                if (!string.IsNullOrEmpty(dependentTable.Url))
                    result = true;
                else if (dependentTable.ID.HasValue)
                {
                    dependentTable.Url = GetUrl(dependentTable);
                    result = true;
                }
            }

            AddErrorMessage(args, addErrorMessage);

            return result;
        }

        private void SimpleResolve(Action<string> addErrorMessage, long id, string tableName, string columnName, string originalTableName)
        {
            var keys = GetPrimaryKeys(tableName, columnName, id);
            foreach (var key in keys.Take(10))
            {
                var args = new TableParametersArgs
                    {
                        ConflictedTableCode = tableName,
                        ConflictedColumnCode = columnName,
                        DeleteID = id,
                        DeleteTable = originalTableName,
                    };
                var projectCode = GetProjectCode(tableName);
                args.DependentTables.Add(
                    new DependentTable
                        {
                            TableCode = tableName,
                            ProjectCode = projectCode,
                            ID = key,
                        });
                SetParametersByCustomCode(args, projectCode);

                foreach (var dependentTable in args.DependentTables)
                {
                    if (!string.IsNullOrEmpty(dependentTable.ProjectCode))
                    {
                        if (string.IsNullOrEmpty(dependentTable.TableHeader))
                            dependentTable.TableHeader = GetTableHeader(dependentTable.ProjectCode, dependentTable.TableCode);

                        if (string.IsNullOrEmpty(dependentTable.RowName))
                            dependentTable.RowName = GetRowName(dependentTable);
                    }

                    if (string.IsNullOrEmpty(dependentTable.Url))
                        dependentTable.Url = GetUrl(dependentTable);
                }

                AddErrorMessage(args, addErrorMessage);
            }
            if (keys.Count == 11)
                addErrorMessage("...");
        }

        private string GetReferencedTableName(string referenceName)
        {
            return db.ExecuteQuery<string>("select OBJECT_NAME(fk.referenced_object_id) from sys.foreign_keys fk where object_id = OBJECT_ID({0})", referenceName).First();
        }

        private static string GetUrl(DependentTable args)
        {
            return string.Format("/MainPage.aspx/data/{0}Edit/read?ref{0}={1}", args.TableCode, args.ID);
        }

        private void AddErrorMessage(TableParametersArgs args, Action<string> addErrorMessage)
        {
            foreach (var dependentTable in args.DependentTables)
            {
                var link = string.Format(
                    "<a href='{0}' title=\"{1}\" target='_blank'>{2}</a>",
                    HttpUtility.HtmlAttributeEncode(dependentTable.Url),
                    HttpUtility.HtmlAttributeEncode(dependentTable.RowName ?? dependentTable.ID.ToString()),
                    HttpUtility.HtmlEncode(dependentTable.TableHeader ?? dependentTable.TableCode));
                var text = string.Format(Resources.SErrorMessage, link);
                addErrorMessage(text);
            }
        }

        private string GetRowName(DependentTable args)
        {
            var type = BuildManager.GetType(
               string.Format("{1}.{0}JournalDataSourceView, {1}, Version=1.0.0.0, Culture=neutral, PublicKeyToken=55f6c56e6ab9709a", args.TableCode, args.ProjectCode),
               false,
               true);

            if (type != null)
            {
                var source = (IDataSourceViewGetName)Activator.CreateInstance(type);
                return source.GetName(args.ID);
            }

            return null;
        }

        private void SetParametersByCustomCode(TableParametersArgs args, string projectCode)
        {
            var type = BuildManager.GetType(
              string.Format("{0}.ReferencesConflictResolver, {0}, Version=1.0.0.0, Culture=neutral, PublicKeyToken=55f6c56e6ab9709a", projectCode),
              false,
              true);

            if (type == null) return;

            var source = Activator.CreateInstance(type) as IReferencesConflictResolver;
            if (source != null)
                source.OnReferenceConflictResolving(args);
        }

        public static string GetProjectCode(string tableName)
        {
            if (ProjectCodesCache.ContainsKey(tableName))
                return ProjectCodesCache[tableName];

            string projectCode;
            using (var db = new DBDataContext(SpecificInstances.DbFactory.CreateConnection()))
            {
                projectCode = CacheQueries.ExecuteFunction<MON_RequestTable, string, string>(
                    db,
                    tableName,
                    (r, value) => r.TableCode == value,
                    r => r.PackageCode);
            }

            return ProjectCodesCache[tableName] = projectCode;
        }

        public static string GetTableHeader(string projectCode, string tableName)
        {
            var cacheKey = tableName + ":" + LocalizationHelper.IsCultureKZ;
            if (ProjectHeadersCache.ContainsKey(cacheKey))
                return ProjectHeadersCache[cacheKey];

            string header = null;
            var type = BuildManager.GetType(
                string.Format("{1}.Properties.{0}Resources, {1}, Version=1.0.0.0, Culture=neutral, PublicKeyToken=55f6c56e6ab9709a", tableName, projectCode),
                false,
                true);

            var property = type?.GetProperty("Header", BindingFlags.Static | BindingFlags.Public);
            if (property != null)
                header = (string)property.GetValue(null, new object[0]);

            if (string.IsNullOrEmpty(header))
            {
                using (var db = new DBDataContext(SpecificInstances.DbFactory.CreateConnection()))
                {
                    header = CacheQueries.GetNameCached<MON_RequestTable, string, string>(
                        db,
                        tableName,
                        (r, value) => r.TableCode == value,
                        r => r.TableName);
                }
            }

            if (string.IsNullOrEmpty(header))
                header = tableName;

            return ProjectHeadersCache[cacheKey] = header;
        }

        public static string GetTableFieldHeader(string projectCode, string tableName, string fieldName)
        {
            var cacheKey = $"{tableName}.{fieldName}:{LocalizationHelper.IsCultureKZ}";
            if (ProjectFieldHeadersCache.ContainsKey(cacheKey))
                return ProjectFieldHeadersCache[cacheKey];

            string header = null;
            var type = BuildManager.GetType(
                string.Format("{1}.Properties.{0}Resources, {1}, Version=1.0.0.0, Culture=neutral, PublicKeyToken=55f6c56e6ab9709a", tableName, projectCode),
                false,
                true);

            var property = type?.GetProperty(fieldName + "__Header", BindingFlags.Static | BindingFlags.Public);
            if (property != null)
                header = (string)property.GetValue(null, new object[0]);

            if (string.IsNullOrEmpty(header))
                header = tableName + "." + fieldName;

            return ProjectFieldHeadersCache[cacheKey] = header;
        }

        private List<long> GetPrimaryKeys(string tableName, string columnName, long id)
        {
            return db.ExecuteQuery<long>("select top 11 id from [" + tableName + "] where [" + columnName + "] = {0}", id).ToList();
        }
    }
}
