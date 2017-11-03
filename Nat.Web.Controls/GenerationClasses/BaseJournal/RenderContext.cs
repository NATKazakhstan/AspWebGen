using System.Collections.Generic;
using System.Linq;
using System.Web.UI;
using Nat.Web.Controls.GenerationClasses.HierarchyFields;

namespace Nat.Web.Controls.GenerationClasses.BaseJournal
{
    using System;

    using Nat.Web.Tools.Export.Computing;
    using Nat.Web.Tools.Export.Formatting;

    public class RenderContext
    {
        private object[] _groupValues;

        private bool computeAggregateExecuted;

        private bool crossTableLoadedData;

        public RenderContext()
        {
            OtherRows = new List<object>();
            Tags = new List<string>();
        }

        public BaseJournalControl Journal { get; set; }
        public BaseJournalRow JournalRow { get; set; }
        public BaseColumn Column { get; set; }
        public BaseRow DataRow { get; set; }
        public ColumnHierarchy ColumnHierarchy { get; set; }
        public object AggregateDataRow { get; set; }
        public object CrossDataItemRow { get; set; }
        public List<object> OtherRows { get; set; }
        public string CrossColumnId { get; set; }
        public object CrossColumnIdObject { get; set; }
        public object CrossColumnHeaderRow { get; set; }
        public BaseColumn.GroupKeys CrossDataItemKey { get; set; }
        public int RowIndex { get; set; }
        public int ColumnIndex { get; set; }
        public bool AllowEdit { get; set; }
        public string EditClientID { get; set; }
        public string ClientID { get; set; }
        public string UniqueID { get; set; }
        public object[] TotalGroupValues { get; set; }
        public List<string> Errors { get; set; }
        public Control Control { get; set; }
        public string ValidationGroup { get; set; }
        public Dictionary<string, RenderContext> OtherColumns { get; set; }
        public List<string> Tags { get; set; }
        public ConditionalFormatting ConditionalFormatting { get; set; }

        public object[] GroupValues
        {
            get { return _groupValues ?? TotalGroupValues; }
            set { _groupValues = value; }
        }

        public TRow GetDataRow<TRow>() where TRow : class
        {
            var row = DataRow as TRow
                ?? AggregateDataRow as TRow
                ?? CrossDataItemRow as TRow
                ?? CrossColumnHeaderRow as TRow;
            if (row != null || OtherRows.Count == 0) return row;
            return OtherRows.OfType<TRow>().FirstOrDefault();
        }

        public TRow GetCrossDataItemRow<TRow>() where TRow : class
        {
            return CrossDataItemRow as TRow;
        }

        public void AddErrorMessage(string message)
        {
            if (Errors == null)
                Errors = new List<string>();
            Errors.Add(message);
        }

        public void AddErrorMessage(string format, params object[] values)
        {
            if (Errors == null)
                Errors = new List<string>();
            Errors.Add(string.Format(format, values));
        }

        public void AddErrorMessage(string format, object value1)
        {
            if (Errors == null)
                Errors = new List<string>();
            Errors.Add(string.Format(format, value1));
        }

        public void AddErrorMessage(string format, object value1, object value2)
        {
            if (Errors == null)
                Errors = new List<string>();
            Errors.Add(string.Format(format, value1, value2));
        }

        public void AddErrorMessage(string format, object value1, object value2, object value3)
        {
            if (Errors == null)
                Errors = new List<string>();
            Errors.Add(string.Format(format, value1, value2, value3));
        }

        public void AddErrorMessage(IEnumerable<string> messages)
        {
            if (Errors == null)
                Errors = new List<string>();
            Errors.AddRange(messages);
        }

        public void AddErrorMessage(IEnumerable<string> messages, params object[] values)
        {
            if (Errors == null)
                Errors = new List<string>();
            Errors.AddRange(messages.Select(r => string.Format(r, values)));
        }

        public void AddErrorMessage(IEnumerable<string> messages, object value1)
        {
            if (Errors == null)
                Errors = new List<string>();
            Errors.AddRange(messages.Select(r => string.Format(r, value1)));
        }

        public void AddErrorMessage(IEnumerable<string> messages, object value1, object value2)
        {
            if (Errors == null)
                Errors = new List<string>();
            Errors.AddRange(messages.Select(r => string.Format(r, value1, value2)));
        }

        public void AddErrorMessage(IEnumerable<string> messages, object value1, object value2, object value3)
        {
            if (Errors == null)
                Errors = new List<string>();
            Errors.AddRange(messages.Select(r => string.Format(r, value1, value2, value3)));
        }

        public void ComputeAggregates()
        {
            if (computeAggregateExecuted || GroupValues != null || TotalGroupValues != null) return;
            computeAggregateExecuted = true;
            Column.ComputeAggregates(this, ColumnHierarchy.AggregateType);
            ConditionalFormatting?.AddValue(GetValue());
        }

        public void LoadData(BaseJournalCrossTable crossTable)
        {
            if (!crossTableLoadedData)
            {
                if (crossTable != Column.BaseJournalCrossTable && Column.BaseJournalCrossTable != null)
                {
                    Column.BaseJournalCrossTable.LoadData(this);
                    crossTableLoadedData = true;
                }
                if (crossTable != null)
                {
                    crossTable.LoadData(this);
                    crossTableLoadedData = true;
                }
            }
        }

        public void LoadData()
        {
            if (!crossTableLoadedData && Column.BaseJournalCrossTable != null)
            {
                Column.BaseJournalCrossTable.LoadData(this);
                crossTableLoadedData = true;
            }
        }

        public bool IsVertical()
        {
            return Column.IsVerticalCell
                   && (TotalGroupValues == null
                       || Column.AggregateType == ColumnAggregateType.GroupText
                       || Column.AggregateType == ColumnAggregateType.GroupTextWithTotalPhrase);
        }

        public RenderContext Clone()
        {
            return new RenderContext
                {
                    CrossDataItemRow = CrossDataItemRow,
                    Journal = Journal,
                    Tags = Tags,
                    CrossDataItemKey = CrossDataItemKey,
                    AggregateDataRow = AggregateDataRow,
                    AllowEdit = AllowEdit,
                    ClientID = ClientID,
                    Column = Column,
                    ColumnHierarchy = ColumnHierarchy,
                    ColumnIndex = ColumnIndex,
                    Control = Control,
                    CrossColumnHeaderRow = CrossColumnHeaderRow,
                    CrossColumnId = CrossColumnId,
                    CrossColumnIdObject = CrossColumnIdObject,
                    DataRow = DataRow,
                    EditClientID = EditClientID,
                    Errors = Errors,
                    GroupValues = _groupValues,
                    JournalRow = JournalRow,
                    OtherColumns = OtherColumns,
                    OtherRows = OtherRows,
                    RowIndex = RowIndex,
                    TotalGroupValues = TotalGroupValues,
                    UniqueID = UniqueID,
                    ValidationGroup = ValidationGroup,
                };
        }

        public object GetValue()
        {
            return Column.GetValue(this);
        }

        public RenderContext GetRenderContextFor(BaseColumn column)
        {
            string columnName;
            if (string.IsNullOrEmpty(CrossColumnId))
                columnName = column.ColumnName;
            else
            {
                var length = ColumnHierarchy.ColumnKey.Length - Column.ColumnName.Length - CrossColumnId.Length - 1;
                columnName = ColumnHierarchy.ColumnKey.Substring(0, length) + column.ColumnName + "_" + CrossColumnId;
            }

            if (!OtherColumns.ContainsKey(columnName))
                throw new ArgumentException("RenderContext does not contain column with name " + columnName);

            return OtherColumns[columnName];
        }

        public Formula GetFormula()
        {
            return Column.GetFormulaHandler?.Invoke(this);
        }
    }
}