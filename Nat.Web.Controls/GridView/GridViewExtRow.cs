using System;
using System.Web.UI;
using System.Web.UI.WebControls;
using Nat.Web.Tools;

namespace Nat.Web.Controls
{
    public class GridViewExtRow : GridViewRow
    {
        private bool collapsed = true;
        private bool? rowVisible = null;
        private string itemValue;
        private string parentValue;
//        private ExpandTableCell tableCell;
        private TemplateExpand tableCell;
        public const string expandConstPostBack = "Expand";

        /// <summary>
        /// Constructor for ExtGridViewRow
        /// </summary>
        /// <param name="rowIndex"></param>
        /// <param name="dataItemIndex"></param>
        /// <param name="rowType"></param>
        /// <param name="rowState"></param>
        public GridViewExtRow(int rowIndex, int dataItemIndex, DataControlRowType rowType, DataControlRowState rowState)
            : base(rowIndex, dataItemIndex, rowType, rowState) {}

        /// <summary>
        /// Gets or sets a value which specifies if the expand boutton should be displayed or not for the current row.
        /// </summary>
        public bool ShowExpand
        {
            get { return (bool?)ViewState["ShowExpand"] ?? false; }
            set { ViewState["ShowExpand"] = value; }
        }

        public int Level
        {
            get { return (int?)ViewState["Level"] ?? -1; }
            set { ViewState["Level"] = value; }
        }

        public bool Collapsed
        {
            get { return collapsed; }
            set { collapsed = value; }
        }

        public bool? RowVisible
        {
            get { return rowVisible; }
            set { rowVisible = value; }
        }

        public TemplateExpand TableCell
        {
            get { return tableCell; }
        }

        private GridViewExt GridView
        {
            get { return (GridViewExt)Parent.Parent; }
        }

        public string ItemValue
        {
            get 
            {
                if (tableCell == null || tableCell.Item == null || string.IsNullOrEmpty(tableCell.Item.Value))
                    return itemValue; 
                return tableCell.Item.Value; 
            }
            set
            {
                if (tableCell == null || tableCell.Item == null || string.IsNullOrEmpty(tableCell.Item.Value))
                    itemValue = value;
                else
                    tableCell.Item.Value = value; 
            }
        }

        public string ParentValue
        {
            get 
            {
                if (tableCell == null || tableCell.ParentItem == null || string.IsNullOrEmpty(tableCell.ParentItem.Value))
                    return parentValue;
                return tableCell.Item.Value;
            }
            set 
            {
                if (tableCell == null || tableCell.ParentItem == null || string.IsNullOrEmpty(tableCell.ParentItem.Value))
                    parentValue = value;
                else
                    tableCell.ParentItem.Value = value; 
            }
        }

        /// <summary>
        /// Overrides GridViewRow.OnInit to perform custom initialization of the row.
        /// </summary>
        /// <param name="e">event args</param>
        protected override void OnInit(EventArgs e)
        {
            base.OnInit(e);
            Initialize();
        }

        private void Initialize()
        {
//            TableCell cell = null;

//            if(RowType == DataControlRowType.Header)
//                cell = new TableHeaderCell();
//            else 
            if(RowType == DataControlRowType.DataRow)
            {
//                cell = tableCell = new ExpandTableCell(null);
                tableCell = new TemplateExpand(Cells[GridView.GridColumnIndexes["fTree"]]);
            }

//            if(cell != null)
//            {
//                Cells.RemoveAt(0);
//                Cells.AddAt(0, cell);
//            }

            if(RowType == DataControlRowType.DataRow)
            {
                if(!string.IsNullOrEmpty(ParentValue)) tableCell.ParentItem.Value = parentValue;
                if(!string.IsNullOrEmpty(ItemValue)) tableCell.Item.Value = itemValue;
                if (RowVisible != null) tableCell.RowVisible = (bool)RowVisible;
                if(!Collapsed) tableCell.Collapsed = false;

                string levelTab = "";
                if(Level > 0)
                {
                    for(int i = 0; i < Level; i++)
                        levelTab += "…";
                    tableCell.LevelTab = levelTab;
                }
                if(ShowExpand)
                {
                    if (GridView.AllowPaging)
                        tableCell.Cell.Attributes["onclick"] = string.Format(@"{0};", Page.ClientScript.GetPostBackEventReference(GridView, expandConstPostBack + tableCell.Item.Value));
                    else if (GridView.PostBackOnExpnad)
                        tableCell.Cell.Attributes["onclick"] = string.Format(@"if(gverChange(this)) {0};", Page.ClientScript.GetPostBackEventReference(GridView, expandConstPostBack));
                    else
                        tableCell.Cell.Attributes["onclick"] = string.Format(@"gverChange(this);");
                    tableCell.Cell.Attributes["onmouseover"] = "this.style.cursor=\"hand\";";
                    tableCell.Cell.Attributes["onmouseout"] = "this.style.cursor=\"\";";
                }
            }
        }

        /// <summary>
        /// Overrides GridViewRow.Render to perform custom rendering of the row.
        /// </summary>
        /// <param name="writer">the HtmlTextWrite object in which the row is rendered</param>
        protected override void Render(HtmlTextWriter writer)
        {
            if(DesignMode)
            {
                base.Render(writer);
                return;
            }

            if (RowType == DataControlRowType.DataRow && GridView.ShowAsTree)
            {
                string expandText;
                if (string.IsNullOrEmpty(tableCell.Cell.Attributes["onclick"]))
                {
                    expandText = "#";
                }
                else if (tableCell.Collapsed)
                {
                    expandText = GridView.ExpandButtonText;
                    tableCell.Cell.CssClass = GridView.ExpandButtonCssClass;
                }
                else
                {
                    expandText = GridView.CollapseButtonText;
                    tableCell.Cell.CssClass = GridView.CollapseButtonCssClass;
                }
                tableCell.LevelText = expandText + tableCell.LevelTab;

                if (!tableCell.RowVisible) Attributes["Style"] = "display:none";
                else Attributes["Style"] = "";

                if(GridView.MaximumLevelNewRow > -1)
                    if(Level > GridView.MaximumLevelNewRow)
                    {
                        Control control = ControlHelper.FindControlRecursive(this, TemplateFieldNew.btnnewtreecolumn);
                        if(control != null) control.Visible = false;
                    }
                if(GridView.EditIndex == this.RowIndex)
                {
                    Control control = ControlHelper.FindControlRecursive(this, TemplateFieldNew.btnnewtreecolumn);
                    if (control != null) control.Visible = false;
                }
                    
            }
            base.Render(writer);
        }
    }
}