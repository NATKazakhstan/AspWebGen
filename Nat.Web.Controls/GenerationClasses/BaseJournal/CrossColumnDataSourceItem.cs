/*
* Created by: Sergey V. Shpakovskiy
* Created: 2012.08.27
* Copyright © JSC NAT Kazakhstan 2012
*/

namespace Nat.Web.Controls.GenerationClasses.BaseJournal
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Web.UI;

    public abstract class CrossColumnDataSourceItem
    {
        public string Header { get; set; }
        public string HeaderRu { get; set; }
        public string HeaderKz { get; set; }
        public string ToolTipRu { get; set; }
        public string ToolTipKz { get; set; }
        public string HyperLinkUrl { get; set; }
        public string HyperLinkTarget { get; set; }
        public string HyperLinkOnClick { get; set; }
        public bool HideInHeader { get; set; }
        public bool IsVerticalHeader { get; set; }
        public int CountColumnValues { get; set; }
        public abstract int ColSpan { get; }
        public abstract int Level { get; }
        public abstract IEnumerable<CrossColumnDataSourceItem> ChildItems { get; }
        public bool IdIsNull { get; set; }
        public abstract string StringColumnId { get; }
        public abstract BaseColumn BaseColumn { get; }
        public abstract object GetColumnID();
        public object Row { get; set; }
        public abstract object ColumnIdObject { get; }
        public object Tag { get; set; }
        public string BaseColumnName { get; set; }

        public string CreatedColumnName { get; set; }
        public string CreatedColumnKey { get; set; }

        public abstract string GetColumnName();
    }

    public class CrossColumnDataSourceItem<THeaderKey, TRow, THeaderTable> : CrossColumnDataSourceItem
        where TRow : BaseRow
        where THeaderTable : class, ICrossTable<THeaderKey>
    {
        private int level;
        private int colSpan;
        private THeaderKey columnID;

        public CrossColumnDataSourceItem()
        {
            CountColumnValues = 1;
            Visible = true;
            LastCells = new Dictionary<BaseJournalRow<TRow>, Control>();
        }

        public THeaderKey ColumnId
        {
            get
            {
                return columnID;
            }
            set
            {
                columnID = value;
                ColumnIdSeted = value != null;
            }
        }
        
        public bool ColumnIdSeted { get; private set; }

        public bool Visible { get; set; }

        public Dictionary<BaseJournalRow<TRow>, Control> LastCells { get; set; }

        public BaseColumn Column { get; set; }

        public List<CrossColumnDataSourceItem<THeaderKey, TRow, THeaderTable>> Childs { get; set; }

        public override string StringColumnId
        {
            get { return ColumnId == null || IdIsNull ? null : ColumnId.ToString(); }
        }

        public override BaseColumn BaseColumn
        {
            get { return Column; }
        }

        public override object ColumnIdObject
        {
            get
            {
                if (IdIsNull) return null;
                return ColumnId;
            }
        }

        public override IEnumerable<CrossColumnDataSourceItem> ChildItems
        {
            get
            {
                if (Childs == null) return new CrossColumnDataSourceItem[0];
                return Childs.Cast<CrossColumnDataSourceItem>();
            }
        }

        public override int ColSpan
        {
            get
            {
                if (colSpan == 0)
                {
                    if (Childs != null && Childs.Count > 0)
                        colSpan = Childs.Sum(c => c.ColSpan + (c.CountColumnValues - 1));
                    else
                        colSpan = 1;
                }
                return colSpan;
            }
        }
        
        public override int Level
        {
            get
            {
                if (level == 0)
                {
                    int plusLevel;
                    if (BaseColumn != null && BaseColumn.IsCrossColumn)
                        plusLevel = BaseColumn.BaseCrossColumnDataSource.MaxLevel;
                    else
                        plusLevel = HideInHeader ? 0 : 1;
                    if (Childs != null && Childs.Count > 0)
                        level = Childs.Max(c => c.Level) + plusLevel;
                    else
                        level = plusLevel;
                }

                return level;
            }
        }

        public override object GetColumnID()
        {
            if (IdIsNull) return null;
            return ColumnId;
        }

        public override string GetColumnName()
        {
            return BaseColumn != null ? BaseColumnName + "_" + BaseColumn.ColumnName + "_" + StringColumnId : null;
        }
    }
}