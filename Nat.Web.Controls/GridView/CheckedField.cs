using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Web.UI;
using System.Web.UI.WebControls;
using AjaxControlToolkit;

[assembly: WebResource("Nat.Web.Controls.GridView.CheckedField.js", "text/javascript")]

namespace Nat.Web.Controls
{
    [ClientScriptResource("Nat.Web.Controls.GridView.CheckedField", "Nat.Web.Controls.GridView.CheckedField.js")]
    public class CheckedField : DataControlField, IGridColumn, IScriptControl
    {
        [DefaultValue("")]
        [Description("Имя колонки")]
        public string ColumnName
        {
            get { return (string)ViewState["_columnName"] ?? ""; }
            set { ViewState["_columnName"] = value; }
        }

        [DefaultValue(false)]
        [Description("Значение по умолчанию у CheckBox-а")]
        public bool DefaultValue
        {
            get { return (bool?)ViewState["_defaultValue"] ?? false; }
            set { ViewState["_defaultValue"] = value; }
        }

        public string OnChanged { get; set; }
        public string OnChangedAll { get; set; }

        public Array GetValues(GridViewExt grid)
        {
            return GetCheckedValues(grid);
        }

        public bool[] GetCheckedValues(GridViewExt grid)
        {
            int columnIndex = grid.Columns.IndexOf(this);
            bool[] values = new bool[grid.Rows.Count];
            for (int i = 0; i < values.Length; i++)
            {
                foreach (Control control in grid.Rows[i].Cells[columnIndex].Controls)
                {
                    CheckBox chk = control as CheckBox;
                    if(chk != null)
                    {
                        values[i] = chk.Checked;
                        break;
                    }
                }
            }
            return values;
        }

        protected override DataControlField CreateField()
        {
            return new CheckedField();
        }

        protected override void CopyProperties(DataControlField newField)
        {
            CheckedField newCheckedField = (CheckedField)newField;
            newCheckedField.ColumnName = ColumnName;
            newCheckedField.DefaultValue = DefaultValue;
            base.CopyProperties(newField);
        }

        public override void InitializeCell(DataControlFieldCell cell, DataControlCellType cellType, DataControlRowState rowState, int rowIndex)
        {
            base.InitializeCell(cell, cellType, rowState, rowIndex);
            CheckBox checkBox = new CheckBox();
            checkBox.ID = "checkBox_ID";
            checkBox.InputAttributes["column"] = ColumnName;
            switch (cellType)
            {
                case DataControlCellType.DataCell:
                {
                        if (((GridViewExt)Control).ShowAsTree)
                            checkBox.Attributes["onclick"] = @"checkedField(this);" + OnChanged;
                        else
                            checkBox.Attributes["onclick"] = @"checkFieldForItems(this);" + OnChanged;

                        cell.Controls.Add(checkBox);
                        break;
                    }
                case DataControlCellType.Footer:
                    break;
                case DataControlCellType.Header:
                    checkBox.Attributes["onclick"] = @"mainCheckedField(this);" + OnChangedAll;
                    checkBox.Text = HeaderText;
                    cell.Controls.AddAt(0, checkBox);
                    break;
                default:
                    break;
            }
        }

        public IEnumerable<ScriptDescriptor> GetScriptDescriptors()
        {
             throw new NotImplementedException();
        }

        public IEnumerable<ScriptReference> GetScriptReferences()
        {
            var references = new List<ScriptReference>();
            references.AddRange(ScriptObjectBuilder.GetScriptReferences(GetType()));
            return references;
        }
    }
}