/*
 * Created by: Sergey V. Shpakovskiy
 * Created: 15  этрЁ  2009 у.
 * Copyright й JSC New Age Technologies 2009
 */

using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Nat.Web.Controls.Properties;
using System;

namespace Nat.Web.Controls.GenerationClasses
{
    public class ChildrenLinksGenerator
    {
        private int id;

        public ChildrenLinksGenerator()
        {
            Groups = new List<Group>();
        }

        public IList<Group> Groups { get; private set; }
        public Dictionary<string, Row> Rows { get; set; }

        public void InitListRows()
        {
            var rows =
                from g in Groups
                from r in g.Rows
                where !string.IsNullOrEmpty(r.Name)
                select r;
            Rows = rows.ToDictionary(p => p.Name);
        }

        public void WriteText(StringBuilder sb)
        {
            WriteText(sb, false, null);
        }

        public void WriteText(StringBuilder sb, bool isAddingNewRow)
        {
            WriteText(sb, isAddingNewRow, null);
        }

        public void WriteText(StringBuilder sb, ExtenderAjaxControl extenderAjaxControl)
        {
            WriteText(sb, false, extenderAjaxControl);
        }

        public void WriteText(StringBuilder sb, bool isAddingNewRow, ExtenderAjaxControl extenderAjaxControl)
        {
            if (Groups.Count(p => p.Visible) == 0) return;
            sb.Append("<div class='childlinks_cssform'>");
            foreach (Group group in Groups.Where(p => p.Visible))
            {
                if (!string.IsNullOrEmpty(group.Text))
                {
                    if (extenderAjaxControl == null)
                        sb.AppendFormat("<FIELDSET style=\"min-width: 800px;\"><LEGEND>{0}</LEGEND>", group.Text);
                    else
                    {
                        var cliendID = "childRefsGroup" + id++;
                        sb.AppendFormat("<FIELDSET style=\"min-width: 800px;\"><LEGEND id=\"l_{0}\" style=\"cursor: pointer;\" title=\"{2}\">{1}</LEGEND><div id=\"{0}\">", cliendID, group.Text, Resources.PressToOpenCloseGroup);
                        HtmlGenerator.AddCollapsiblePanel(extenderAjaxControl, cliendID, "l_" + cliendID, "l_" + cliendID);
                    }
                }

                foreach (Row row in group.Rows.Where(p => p.Visible))
                {
                    sb.Append(@"<p><span>");
                    var items = row.Items.Where(p => p.Visible);
                    if (isAddingNewRow) items = items.Where(p => p.Name != "new");
                    foreach (Item item in items)
                        sb.Append(item.Text);
                    sb.Append(@"</span>");
                    sb.Append(row.Text);
                    sb.Append(@"</p>");
                }
                if (!string.IsNullOrEmpty(group.Text))
                {
                    if (extenderAjaxControl == null)
                        sb.Append("</FIELDSET>");
                    else
                        sb.Append("</div></FIELDSET>");
                }
            }
            sb.Append("</div>");
        }


        public void SetRowVisible(bool visible, params string[] rowNames)
        {
            if (Rows == null) InitListRows();

            foreach (var rowName in rowNames.Where(Rows.ContainsKey))
                Rows[rowName].Visible = visible;
        }

        public void SetRowVisible(string rowName, bool visible)
        {
            if (Rows == null) InitListRows();
            if (!Rows.ContainsKey(rowName)) return;
            var row = Rows[rowName];
            row.Visible = visible;
        }

        public void SetAllRowVisible(bool visible)
        {
            if (Rows == null) InitListRows();
            foreach (var row in Rows.Values)
                row.Visible = visible;
        }

        public void SetRowText(string rowName, string text)
        {
            if (Rows == null) InitListRows();
            if (!Rows.ContainsKey(rowName)) return;
            var row = Rows[rowName];
            row.Text = text;
        }

        public void SetJournalVisible(string rowName, bool visible)
        {
            if (Rows == null) InitListRows();
            if (!Rows.ContainsKey(rowName)) return;
            var row = Rows[rowName];
            if (row != null && row.JournalLink != null)
                row.JournalLink.Visible = visible;
        }

        public void SetJournalAdditionalParameters(string rowName, string additionalParameters)
        {
            if (Rows == null) InitListRows();
            if (!Rows.ContainsKey(rowName)) return;
            var row = Rows[rowName];
            if (row != null && row.JournalLink != null)
                row.JournalLink.SetAdditionalParameters(additionalParameters);
        }

        public void SetAddAdditionalParameters(string rowName, string additionalParameters)
        {
            if (Rows == null) InitListRows();
            if (!Rows.ContainsKey(rowName)) return;
            var row = Rows[rowName];
            if (row != null && row.AddLink != null)
                row.AddLink.SetAdditionalParameters(additionalParameters);
        }

        public void SetEditVisible(string rowName, bool visible)
        {
            if (Rows == null) InitListRows();
            if (!Rows.ContainsKey(rowName)) return;
            var row = Rows[rowName];
            if (row != null && row.EditLink != null)
                row.EditLink.Visible = visible;
        }

        public void SetAddVisible(string rowName, bool visible)
        {
            if (Rows == null) InitListRows();
            if (!Rows.ContainsKey(rowName)) return;
            var row = Rows[rowName];
            if (row != null && row.AddLink != null)
                row.AddLink.Visible = visible;
        }

        public void SetLookVisible(string rowName, bool visible)
        {
            if (Rows == null) InitListRows();
            if (!Rows.ContainsKey(rowName)) return;
            var row = Rows[rowName];
            if (row != null && row.LookLink != null)
                row.LookLink.Visible = visible;
        }

        public void SetVisible(string rowName, string buttonName, bool visible)
        {
            if (Rows == null) InitListRows();
            if (!Rows.ContainsKey(rowName)) return;
            var row = Rows[rowName];
            if (row != null)
            {
                var item = row.Items.FirstOrDefault(
                    r => buttonName.Equals(r.Name, StringComparison.OrdinalIgnoreCase));
                if (item != null) item.Visible = visible;
            }
        }


        #region Nested type: Group

        public class Group
        {
            public Group()
            {
                Rows = new List<Row>();
                Visible = true;
            }

            public bool Visible { get; set; }
            public string Text { get; set; }
            public IList<Row> Rows { get; private set; }
        }

        #endregion

        #region Nested type: Item

        public class Item
        {
            public Item()
            {
                Visible = true;
            }

            public string Name { get; set; }
            public bool Visible { get; set; }
            public string Text { get; set; }

            public void SetAdditionalParameters(string additionalParameters)
            {
                var startIndex = Text.IndexOf("href=\"", StringComparison.OrdinalIgnoreCase);
                var endIndex = Text.IndexOf("\"", startIndex + 6, StringComparison.OrdinalIgnoreCase);
                var contains = Text.IndexOf("?", startIndex, endIndex - startIndex, StringComparison.OrdinalIgnoreCase) > -1;
                Text = Text.Insert(endIndex, (contains ? "&" : "?") + additionalParameters);
            }
        }

        #endregion

        #region Nested type: Row

        public class Row
        {
            public Row()
            {
                Items = new ListItems() {row = this};
                Visible = true;
            }

            public string Name { get; set; }
            public string Text { get; set; }
            public bool Visible { get; set; }
            public IList<Item> Items { get; private set; }
            public Item AddLink { get; set; }
            public Item EditLink { get; set; }
            public Item LookLink { get; set; }
            public Item JournalLink { get; set; }

            private class ListItems : List<Item>, IList<Item>
            {
                public Row row;

                void ICollection<Item>.Add(Item item)
                {
                    SetLink(item);
                    Add(item);
                }

                bool ICollection<Item>.Remove(Item item)
                {
                    SetEmptyLink(item);
                    return Remove(item);
                }

                void IList<Item>.Insert(int index, Item item)
                {
                    SetLink(item);
                    Insert(index, item);
                }

                void IList<Item>.RemoveAt(int index)
                {
                    SetEmptyLink(this[index]);
                    RemoveAt(index);
                }

                private void SetLink(Item item)
                {
                    switch (item.Name)
                    {
                        case "new":
                            row.AddLink = item;
                            break;
                        case "edit":
                            row.EditLink = item;
                            break;
                        case "look":
                            row.LookLink = item;
                            break;
                        case "journal":
                            row.JournalLink = item;
                            break;
                    }
                }
                
                private void SetEmptyLink(Item item)
                {
                    switch (item.Name)
                    {
                        case "new":
                            row.AddLink = null;
                            break;
                        case "edit":
                            row.EditLink = null;
                            break;
                        case "look":
                            row.LookLink = null;
                            break;
                        case "journal":
                            row.JournalLink = null;
                            break;
                    }
                }
            }
        }

        #endregion
    }
}