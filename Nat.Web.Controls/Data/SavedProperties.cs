using System;
using System.Collections.Generic;
using System.Data.Linq;
using System.Linq;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Serialization;
using Nat.Tools.Specific;
using Nat.Web.Controls.GenerationClasses;
using Nat.Web.Controls.GenerationClasses.BaseJournal;
using Nat.Web.Controls.GenerationClasses.HierarchyFields;
using Nat.Web.Tools;
using System.Data.Common;
using Nat.Web.Tools.Security;
using System.Web;

namespace Nat.Web.Controls.Data
{
    public class RvsSavedProperties
    {
        public string Format { get; set; }
        public string Culture { get; set; }
        public List<GroupColumn> Grouping { get; set; }
        public bool IsFixedHeader { get; set; }
        public int FixedRowsCount { get; set; }
        public int FixedColumnsCount { get; set; }
        public string PageUrl { get; set; }
        public string ReportPluginName { get; set; }
        public string OrderByColumns { get; set; }
        public StorageValues StorageValues { get; set; }
        public List<ColumnHierarchy> ColumnHierarchy { get; set; }
        public List<RowProperties> HeaderRowsProperties { get; set; }
        public List<RowProperties> DataRowsProperties { get; set; }
        public List<CellProperties> DataCellProperties { get; set; }
        public List<ConcatenateColumnTransporter> ConcatenateColumns { get; set; }
        public string NameRu { get; set; }
        public string NameKz { get; set; }
        public string JournalTypeName { get; set; }

        #region load

        public static RvsSavedProperties LoadFrom(long id, ILogMonitor logMonitor)
        {
            using (var db = new DB_RvsSettingsDataContext(LogMonitor.CreateConnection()))
            {
                return LoadFrom(db, db.RVS_Properties.FirstOrDefault(r => r.id == id));
            }
        }

        private static RvsSavedProperties LoadFrom(DB_RvsSettingsDataContext db, RVS_Property row)
        {
            if (row == null) return null;
            var properties = new RvsSavedProperties
                             {
                                 Grouping = row.Grouping.Elements("GroupColumn").Select(r => (GroupColumn)r.Value).ToList(),
                                 OrderByColumns = row.OrderByColumns == null ? null : string.Join(",", row.OrderByColumns.Elements("OrderColumn").Select(r => r.Value).ToArray()),
                                 PageUrl = row.Filter == null ? null : row.Filter.Value,
                                 ReportPluginName = row.ReportPluginName,
                                 NameRu = row.nameRu,
                                 NameKz = row.nameKz,
                                 JournalTypeName = row.JournalTypeName,
                             };
            ReadFixedHeader(row, properties);
            ReadColumnHierarchy(row, properties);
            ReadRowsProperties(row, properties);
            ReadStorageValues(row, properties);
            ReadCellsProperties(row, properties);
            ReadOtherParameters(row, properties);
            return properties;
        }

        public static RvsSavedProperties LoadBySavedViewSettings(long idSavedProperty, ILogMonitor logMonitor)
        {
            using (var db = new DB_RvsSettingsDataContext(LogMonitor.CreateConnection()))
            {
                var row = db.RVS_SavedProperties.
                    Where(r => r.id == idSavedProperty
                               && (r.isSharedView || r.UserSID == User.GetSID())).
                    Select(r => r.RVS_Property).
                    FirstOrDefault();
                return LoadFrom(db, row);
            }
        }

        #endregion

        #region save 

        public long Save()
        {
            using (var db = new DB_RvsSettingsDataContext(LogMonitor.CreateConnection()))
            {
                var row = SaveProperties(db);
                db.SubmitChanges();
                return row.id;
            }
        }

        public void SaveWithViewSettings(SavingJournalSettings.SaveArgument argument, string conext)
        {
            if (!argument.saveFilters)
                PageUrl = null;
            using (var db = new DB_RvsSettingsDataContext(LogMonitor.CreateConnection()))
            {
                db.Connection.Open();
                DbTransaction transaction = null;
                try
                {
                    transaction = db.Connection.BeginTransaction();
                    db.Transaction = transaction;
                    var row = SaveProperties(db);
                    db.SubmitChanges();

                    RVS_SavedProperty viewRow = null;
                    if (argument.id != null)
                    {
                        var data = db.RVS_SavedProperties.
                            Where(r => r.id == argument.id);
                        if (UserRoles.IsInRole(UserRoles.AllowChangeOrDeleteJournalSettingsAsShared))
                            data = data.Where(r => r.isSharedView || r.UserSID == User.GetSID());
                        else
                            data = data.Where(r => r.UserSID == User.GetSID());
                        viewRow = data.FirstOrDefault();
                    }
                    if (viewRow == null)
                    {
                        viewRow = new RVS_SavedProperty();
                        db.RVS_SavedProperties.InsertOnSubmit(viewRow);
                    }

                    viewRow.context = conext;
                    viewRow.dateTime = DateTime.Now;
                    viewRow.isDefaultView = false;
                    viewRow.isSharedView = argument.saveAsShared && UserRoles.IsInRole(UserRoles.AllowSaveJournalSettingsAsShared);
                    viewRow.JournalTypeName = JournalTypeName;
                    viewRow.nameRu = argument.nameRu;
                    viewRow.nameKz = argument.nameKz;
                    viewRow.UserSID = User.GetSID();
                    viewRow.refProperties = row.id;

                    db.SubmitChanges();
                    db.Transaction.Commit();
                }
                catch
                {
                    if (transaction != null)
                        transaction.Rollback();
                }
                finally
                {
                    if (transaction != null)
                        transaction.Dispose();
                    db.Connection.Close();
                }
            }
        }

        private RVS_Property SaveProperties(DB_RvsSettingsDataContext db)
        {
            var row = new RVS_Property
                {
                    Grouping = new XElement("Root", Grouping.Select(r => new XElement("GroupColumn", r.ToString()))),
                    OrderByColumns = new XElement("Root", (OrderByColumns ?? "").Split(',').Select(r => new XElement("OrderColumn", r.ToString()))),
                    Filter = string.IsNullOrEmpty(PageUrl) ? null : new XElement("PageUrl", PageUrl),
                    ReportPluginName = ReportPluginName,
                    nameRu = NameRu,
                    nameKz = NameKz,
                    JournalTypeName = JournalTypeName,
                };
            WriteFixedHeader(row, this);
            WriteColumnHierarchy(row, this);
            WriteRowsProperties(row, this);
            WriteStorageValues(row, this);
            WriteCellsProperties(row, this);
            WriteOtherParameters(row, this);
            db.RVS_Properties.InsertOnSubmit(row);
            return row;
        }

        #endregion

        public static string GetGuidForLoadParameter(long id)
        {
            var dic = (Dictionary<long, string>)HttpContext.Current.Session["LogViewReports"];
            var dic2 = (Dictionary<string, long>)HttpContext.Current.Session["LogViewReports2"];
            if (dic == null)
                HttpContext.Current.Session["LogViewReports"] = dic = new Dictionary<long, string>();
            if (dic2 == null)
                HttpContext.Current.Session["LogViewReports2"] = dic2 = new Dictionary<string, long>();
            if (dic.ContainsKey(id))
                return dic[id];
            var guid = Guid.NewGuid().ToString();
            dic[id] = guid;
            dic2[guid] = id;
            return guid;
        }

        public static RvsSavedProperties GetFromJournal(BaseJournalUserControl journal)
        {
            journal.PrepareSettings();
            var properties =
                new RvsSavedProperties
                    {
                        Grouping = journal.BaseJournal.GroupColumns.ToList(),
                        PageUrl = journal.Url.CreateUrl(true, true, true),
                        ReportPluginName = journal.ReportPluginName,
                        IsFixedHeader = journal.FixedHeader,
                        FixedColumnsCount = journal.FixedColumnsCount,
                        FixedRowsCount = journal.FixedRowsCount,
                        ColumnHierarchy = journal.BaseJournal.BaseInnerHeader.ColumnHierarchy,
                        StorageValues = journal.StorageValues,
                        HeaderRowsProperties = journal.BaseJournal.BaseInnerHeader.RowsProperties,
                        DataRowsProperties = journal.BaseJournal.RowsProperties,
                        DataCellProperties = journal.BaseJournal.CellsProperties,
                        ConcatenateColumns = journal.BaseJournal.SelectingColumnControl != null
                                                 ? journal.BaseJournal.SelectingColumnControl.GetConcatenateColumnTransporters()
                                                 : null,
                        NameRu = journal.HeaderRu,
                        NameKz = journal.HeaderKz,
                        JournalTypeName = GetJournalTypeName(journal),
                        OrderByColumns = journal.BaseJournal.DefaultOrder,
                    };
            return properties;
        }

        public static string GetJournalTypeName(BaseJournalUserControl journal)
        {
            var type = journal.GetType();
            if (type.BaseType?.IsGenericType ?? true)
                return type.FullName + "," + type.Assembly.FullName;
            return type.BaseType.FullName + "," + type.BaseType.Assembly.FullName;
        }

        public void SetToJournal(BaseJournalUserControl journal)
        {
            SetToJournal(journal, false, false);
        }

        public void SetToJournal(BaseJournalUserControl journal, bool skipFilters, bool skipNavigators)
        {
            journal.BaseJournal.GroupColumns.Clear();
            journal.BaseJournal.GroupColumns.AddRange(Grouping);
            journal.StorageValues = StorageValues;
            journal.ValuesLoaded = true;
            if (!string.IsNullOrEmpty(PageUrl))
            {
                if (!skipNavigators && !skipFilters)
                {
                    journal.Url = new MainPageUrlBuilder(PageUrl);
                    journal.Url.ReportPluginName = ReportPluginName;
                    journal.Url.CheckUseSession();
                    if (journal.Url.UserControl.Equals(MainPageUrlBuilder.Current.UserControl))
                    {
                        MainPageUrlBuilder.Current = journal.Url;
                        MainPageUrlBuilder.ChangedUrl();
                    }
                }
                else
                {
                    var url = journal.Url;
                    var lUrl = new MainPageUrlBuilder(PageUrl);
                    if (!skipNavigators)
                    {
                        foreach (var key in url.QueryParameters.Keys.ToList())
                        {
                            if (key.Contains(".")) url.QueryParameters.Remove(key);
                        }
                        foreach (var queryParameter in lUrl.QueryParameters)
                        {
                            if (queryParameter.Key.Contains("."))
                                url.QueryParameters[queryParameter.Key] = queryParameter.Value;
                        }
                    }
                    if (!skipFilters)
                    {
                        url.SetFilter(journal.BaseFilter.TableName, lUrl.GetFilterItemsDic(journal.BaseFilter.TableName));
                        /*foreach (var key in url.QueryParameters.Keys.ToList())
                        {
                            if (!key.Contains(".")) url.QueryParameters.Remove(key);
                        }
                        foreach (var queryParameter in lUrl.QueryParameters)
                        {
                            if (!queryParameter.Key.Contains("."))
                                url.QueryParameters[queryParameter.Key] = queryParameter.Value;
                        }*/
                    }
                }
            }
            journal.FixedHeader = IsFixedHeader;
            journal.FixedColumnsCount = FixedColumnsCount;
            journal.FixedRowsCount = FixedRowsCount;
            journal.BaseJournal.BaseInnerHeader.ColumnHierarchy = ColumnHierarchy;
            journal.StorageValues = StorageValues;
            journal.BaseJournal.BaseInnerHeader.RowsProperties = HeaderRowsProperties;
            journal.BaseJournal.RowsProperties = DataRowsProperties;
            journal.BaseJournal.CellsProperties = DataCellProperties;
            journal.BaseJournal.ConcatenateColumns = ConcatenateColumns;
            if (journal.BaseJournal.SelectingColumnControl != null && ConcatenateColumns != null)
                journal.BaseJournal.SelectingColumnControl.SetConcatenateColumnProperties(ConcatenateColumns);
            if (OrderByColumns != null)
                journal.BaseJournal.DefaultOrder = OrderByColumns;
        }

        #region Read, Write RVS_Property

        private static void ReadFixedHeader(RVS_Property row, RvsSavedProperties properties)
        {
            properties.IsFixedHeader = row.FixedHeader.GetElementValue<bool>("IsFixedHeader") ?? true;
            properties.FixedRowsCount = row.FixedHeader.GetElementValue<int>("FixedRowsCount") ?? 0;
            properties.FixedColumnsCount = row.FixedHeader.GetElementValue<int>("FixedColumnsCount") ?? 1;
        }

        private static void WriteFixedHeader(RVS_Property row, RvsSavedProperties properties)
        {
            row.FixedHeader = new XElement("Root",
                new XElement("IsFixedHeader", properties.IsFixedHeader.ToString()),
                new XElement("FixedRowsCount", properties.FixedRowsCount.ToString()),
                new XElement("FixedColumnsCount", properties.FixedColumnsCount.ToString()));
        }

        private static void ReadColumnHierarchy(RVS_Property row, RvsSavedProperties properties)
        {
            var serializer = new XmlSerializer(typeof(List<ColumnHierarchy>));
            List<ColumnHierarchy> colH;
            using (var stream = row.ColumnsStyle.CreateReader())
                colH = (List<ColumnHierarchy>) serializer.Deserialize(stream);
            properties.ColumnHierarchy = colH;
        }

        private static void WriteColumnHierarchy(RVS_Property row, RvsSavedProperties properties)
        {
            var serializer = new XmlSerializer(typeof(List<ColumnHierarchy>));
            var xmlDoc = new XDocument();
            using(var writer = xmlDoc.CreateWriter())
            {
                serializer.Serialize(writer, properties.ColumnHierarchy);
                writer.Flush();
                writer.Close();
                row.ColumnsStyle = xmlDoc.Root;
            }
        }

        private static void WriteRowsProperties(RVS_Property row, RvsSavedProperties properties)
        {
            var serializer = new XmlSerializer(typeof(List<RowProperties>));
            var xmlDoc = new XDocument();
            XElement eHR;
            XElement eDR;
            using (var writer = xmlDoc.CreateWriter())
            {
                serializer.Serialize(writer, properties.HeaderRowsProperties);
                writer.Flush();
                writer.Close();
                eHR = new XElement("HeaderRowsProperties", xmlDoc.Root);
            }
            xmlDoc = new XDocument();
            using (var writer = xmlDoc.CreateWriter())
            {
                serializer.Serialize(writer, properties.DataRowsProperties);
                writer.Flush();
                writer.Close();
                eDR = new XElement("DataRowsProperties", xmlDoc.Root);
            }
            row.RowsStyle = new XElement("Root", eHR, eDR);
        }

        private static void ReadRowsProperties(RVS_Property row, RvsSavedProperties properties)
        {
            var serializer = new XmlSerializer(typeof(List<RowProperties>));
            var eHR = row.RowsStyle.Element("HeaderRowsProperties");
            if (eHR != null)
            {
                using (var stream = eHR.Elements().First().CreateReader())
                    properties.HeaderRowsProperties = (List<RowProperties>) serializer.Deserialize(stream);
            }
            var eDR = row.RowsStyle.Element("DataRowsProperties");
            if (eDR != null)
            {
                using (var stream = eDR.Elements().First().CreateReader())
                    properties.DataRowsProperties = (List<RowProperties>)serializer.Deserialize(stream);
            }
        }

        private static void WriteCellsProperties(RVS_Property row, RvsSavedProperties properties)
        {
            var serializer = new XmlSerializer(typeof(List<CellProperties>));
            var xmlDoc = new XDocument();
            using (var writer = xmlDoc.CreateWriter())
                serializer.Serialize(writer, properties.DataCellProperties);
            row.CellsStyle = new XElement("CellsProperties", xmlDoc.Root);
        }

        private static void ReadCellsProperties(RVS_Property row, RvsSavedProperties properties)
        {
            var serializer = new XmlSerializer(typeof(List<CellProperties>));
            if (row.CellsStyle == null) return;
            var element = row.CellsStyle.Elements().FirstOrDefault();
            if (element != null)
            {
                using (var stream = element.CreateReader())
                    properties.DataCellProperties = (List<CellProperties>)serializer.Deserialize(stream);
            }
        }

        private static void WriteStorageValues(RVS_Property row, RvsSavedProperties properties)
        {
            if (properties.StorageValues != null)
                row.StorageValues = new Binary(properties.StorageValues.Serialize());
        }

        private static void ReadStorageValues(RVS_Property row, RvsSavedProperties properties)
        {
            if (row.StorageValues != null && row.StorageValues.Length > 0)
                properties.StorageValues = StorageValues.Deserialize(row.StorageValues.ToArray());
        }

        private static void WriteOtherParameters(RVS_Property row, RvsSavedProperties properties)
        {
            var serializer = new XmlSerializer(typeof(List<ConcatenateColumnTransporter>));
            var xmlDoc = new XDocument();
            using (var writer = xmlDoc.CreateWriter())
                serializer.Serialize(writer, properties.ConcatenateColumns);
            row.OtherParameters = new XElement("Root",
                                               new XElement("ConcatenateColumns", xmlDoc.Root));
        }

        private static void ReadOtherParameters(RVS_Property row, RvsSavedProperties properties)
        {
            if (row.OtherParameters == null) return;

            var serializer = new XmlSerializer(typeof(List<ConcatenateColumnTransporter>));
            var element = row.OtherParameters.Element("ConcatenateColumns");
            if (element != null)
            {
                var doc = element.Elements().FirstOrDefault();
                if (doc != null)
                {
                    using (var stream = doc.CreateReader())
                        properties.ConcatenateColumns =
                            (List<ConcatenateColumnTransporter>) serializer.Deserialize(stream);
                }
            }
        }

        #endregion

        public static bool DeleteProperties(long id)
        {
            using (var db = new DB_RvsSettingsDataContext(LogMonitor.CreateConnection()))
            {
                var data = db.RVS_SavedProperties.
                    Where(r => r.id == id);
                if (UserRoles.IsInRole(UserRoles.AllowChangeOrDeleteJournalSettingsAsShared))
                    data = data.Where(r => r.UserSID == User.GetSID() || r.isSharedView);
                else
                    data = data.Where(r => r.UserSID == User.GetSID());

                var row = data.FirstOrDefault();
                if (row == null) return false;
                db.Connection.Open();
                DbTransaction transaction = null;
                try
                {
                    transaction = db.Connection.BeginTransaction();
                    db.Transaction = transaction;
                    db.RVS_SavedProperties.DeleteOnSubmit(row);
                    db.RVS_Properties.DeleteOnSubmit(row.RVS_Property);
                    db.SubmitChanges();
                    transaction.Commit();
                }
                catch
                {
                    if (transaction != null)
                        transaction.Rollback();
                    return false;
                }
                finally
                {
                    if (transaction != null)
                        transaction.Dispose();
                    db.Connection.Close();
                }
            }
            return true;
        }
    }
}