using System;
using System.Collections.Generic;
using System.Web.UI.WebControls;

namespace Nat.Web.Controls
{
    public class ColumnIndexer
    {
        private readonly DataControlFieldCollection _columns;
        readonly Dictionary<string, int> _gridColumnIndexes;

        public ColumnIndexer(DataControlFieldCollection columns)
        {
            _columns = columns;
            columns.FieldsChanged += Columns_OnFieldsChanged;
            _gridColumnIndexes = new Dictionary<string, int>();
            RefreshColumns();
        }

        public Dictionary<string, int> GridColumnIndexes
        {
            get { return _gridColumnIndexes; }
        }

        private void RefreshColumns()
        {
            _gridColumnIndexes.Clear();
            for (int i = 0; i < _columns.Count; i++)
            {
                IColumnName column = _columns[i] as IColumnName;
                if (column != null)
                {
                    if (_gridColumnIndexes.ContainsKey(column.ColumnName))
                        throw new Exception(string.Format("Созданы 2 колонки с одинаковым названием '{0}'", column.ColumnName));
                    _gridColumnIndexes.Add(column.ColumnName, i);
                }
                BoundField boundColumn = _columns[i] as BoundField;
                if (boundColumn != null)
                {
                    if (_gridColumnIndexes.ContainsKey(boundColumn.DataField))
                        throw new Exception(string.Format("Созданы 2 колонки с одинаковым названием '{0}'", boundColumn.DataField));
                    _gridColumnIndexes.Add(boundColumn.DataField, i);
                }
            }
        }

        private void Columns_OnFieldsChanged(object sender, EventArgs e)
        {
            RefreshColumns();
        }
    }
}