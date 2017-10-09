using System;
using System.Collections.Generic;
using System.Web.UI;
using System.Web.UI.WebControls;
using Nat.ReportManager.ReportGeneration;
using Nat.Web.ReportManager.Properties;

[assembly: WebResource("Nat.Web.ReportManager.DeleteHS.png", "image/png")]

namespace Nat.Web.ReportManager
{
    public class ReportConditionControls : WebControl
    {
        private bool _inited = false;
        private bool _reportControlsCreated = false;
        private Table _table;
        private Button _btnAdd;
        private Button _btnDelete;
        private TextBox _countAdd;
        private readonly IWebReportPlugin _plugin;

        public ReportConditionControls() {}

        public ReportConditionControls(IWebReportPlugin plugin)
        {
            _plugin = plugin;
        }

        protected override void OnInit(EventArgs e)
        {
            base.OnInit(e);

            Controls.Add(new LiteralControl("&nbsp"));
            _countAdd = new TextBox();
            _countAdd.Text = "1";
            _countAdd.Width = 20;
            _countAdd.MaxLength = 1;
            _countAdd.ID = "addTextBox";
            Controls.Add(_countAdd);

            Controls.Add(new LiteralControl("&nbsp"));
            _btnAdd = new Button();
            _btnAdd.Click += AddButton_OnClick;
            _btnAdd.Text = Resources.SAddCondition;
            _btnAdd.ID = "addButton";
            Controls.Add(_btnAdd);

            Controls.Add(new LiteralControl("&nbsp"));
            _btnDelete = new Button();
            _btnDelete.Click += DeleteAllButton_OnClick;
            _btnDelete.Text = Resources.SDeleteCondition;
            _btnDelete.ID = "deleteButton";
            Controls.Add(_btnDelete);

            Controls.Add(new LiteralControl("&nbsp"));
            _table = new Table();
            _table.BorderWidth = 1;
            Controls.Add(_table);

            _inited = true;
        }

        private void DeleteAllButton_OnClick(object sender, EventArgs e)
        {
            _table.Rows.Clear();
            CountRows = 0;
        }

        private void AddButton_OnClick(object sender, EventArgs e)
        {
            int count;
            if(int.TryParse(_countAdd.Text, out count) && count < 10)
            {
                _plugin.SetCountCircleFillConditions(CountRows + count, false);
                while (count > 0)
                {
                    TableRow row = CreateNewRow(CountRows, _plugin.CircleFillConditions[CountRows]);
                    _table.Rows.Add(row);
                    CountRows++;
                    count--;
                }
            }
        }

        public override ControlCollection Controls
        {
            get
            {
                if (_inited) EnsureReportControls();
                return base.Controls;
            }
        }

        protected bool ReportControlsCreated
        {
            get { return _reportControlsCreated; }
            set
            {
                //todo: нужна чистка если приходит false;
                _reportControlsCreated = value;
            }
        }

        protected void EnsureReportControls()
        {
            if(!ReportControlsCreated)
            {
                CreateReportControls();
                ReportControlsCreated = true;
            }
        }

        protected void CreateReportControls()
        {
            _plugin.SetCountCircleFillConditions(CountRows, false);
            for (int i = 0; i < CountRows; i++)
            {
                TableRow row = CreateNewRow(i, _plugin.CircleFillConditions[i]);
                _table.Rows.Add(row);
            }
        }


        protected override void OnPreRender(EventArgs e)
        {
            EnsureReportControls();
            base.OnPreRender(e);
        }

        protected ReportTableRow CreateNewRow(int index, List<BaseReportCondition> reportConditions)
        {
            ReportTableRow row = new ReportTableRow();
            TableCell cell;
            List<BaseReportCondition> items = new List<BaseReportCondition>();
            int i = 0;
            foreach (BaseReportCondition condition in reportConditions)
            {
                items.Add(condition);

                foreach (BaseReportCondition reportCondition in condition.GetVisualReportConditions())
                {
                    if (!reportCondition.Visible) continue;
                    cell = new TableCell();
                    cell.BorderWidth = 1;
                    Control columnFilter = (Control)reportCondition.ColumnFilter;
                    columnFilter.ID = string.Format("column_{0}_{1}", index, i++);
                    cell.Controls.Add(columnFilter);
                    row.Cells.Add(cell);
                }
            }
            cell = new TableCell();
            cell.BorderWidth = 1;
            ImageButton button = new ImageButton();
            button.ID = string.Format("delButton_{0}", index);
            button.Click += DeleteButton_OnClick;
            button.Attributes["index"] = index.ToString();
            button.ImageUrl = Page.ClientScript.GetWebResourceUrl(GetType(), "Nat.Web.ReportManager.DeleteHS.png");

            cell.Controls.Add(button);
            row.Cells.Add(cell);
            row.ReportConditions = items;
            row.ImageButton = button;
            return row;
        }

        private void DeleteButton_OnClick(object sender, EventArgs e)
        {
            ImageButton btn = sender as ImageButton;
            if(btn != null)
            {
                int index = Convert.ToInt32(btn.Attributes["index"]);
                _table.Rows.RemoveAt(index);
                CountRows--;
                for (int rowIndex = index; rowIndex < _table.Rows.Count; rowIndex++)
                {
                    int cellIndex = 0;
                    ReportTableRow row = (ReportTableRow)_table.Rows[rowIndex];
                    row.ImageButton.ID = string.Format("delButton_{0}", rowIndex);
                    foreach (BaseReportCondition item in row.ReportConditions)
                    {
                        foreach (BaseReportCondition reportCondition in item.GetVisualReportConditions())
                            ((Control)reportCondition.ColumnFilter).ID = string.Format("column_{0}_{1}", rowIndex, cellIndex);
                    }
                }
            }
        }

        public int CountRows
        {
            get { return (int?)ViewState["CountRows"] ?? 0; }
            set { ViewState["CountRows"] = value; }
        }

        public List<List<BaseReportCondition>> GetFillCircleConditions()
        {
            List<List<BaseReportCondition>> lists = new List<List<BaseReportCondition>>();
            foreach (ReportTableRow tableRow in _table.Rows)
            {
                lists.Add(tableRow.ReportConditions);
            }
            return lists;
        }
    }
}