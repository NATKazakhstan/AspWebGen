using System;
using System.Linq;
using System.Collections.Generic;
using System.Web.UI.WebControls;

namespace Nat.Web.Controls.SelectValues
{
    [Serializable]
    public class ListControlItem
    {
        public ListControlItem()
        {
            Selectable = true;
        }

        /// <summary>
        /// Идентификатор сохраненной записи.
        /// </summary>
        public string SelectedKey { get; set; }

        /// <summary>
        /// Идентификатор справочной записи.
        /// </summary>
        public string Value { get; set; }
        
        public string ReferenceValue { get; set; }

        public string Text { get; set; }
        public string ToolTip { get; set; }
        
        /// <summary>
        /// Значение отмечено как выбранное для сохранения. Если false, то запись будет удалена.
        /// </summary>
        public bool Selected { get; set; }
        public bool Enabled { get; set; }
        public bool Deleted { get; set; }
        
        public bool IsGroup { get; set; }
        public bool HideGroup { get; set; }
        public bool Selectable { get; set; }
        public List<ListControlItem> Children { get; set; }

        public static implicit operator ListItem(ListControlItem controlItem)
        {
            var item = new ListItem(controlItem.Text, controlItem.Value, controlItem.Enabled);
            item.Attributes["ToolTip"] = controlItem.ToolTip;
            item.Attributes["Deleted"] = controlItem.Deleted.ToString();
            item.Selected = controlItem.Selected;
            return item;
        }

        public bool AnySelected()
        {
            return IsGroup ? Children.Any(r => r.AnySelected()) : Selected;
        }

        public bool AllSelected()
        {
            return IsGroup ? Children.All(r => r.AllSelected()) : Selected;  
        }

        public int ChildrenCount()
        {
            if (Children.Count == 1 && Children[0].IsGroup && Children[0].HideGroup)
                return Children[0].ChildrenCount();
            return Children.Count;
        }

        public IEnumerable<ListControlItem> AllChildren()
        {
            if (IsGroup) return Children.Union(Children.SelectMany(r => r.AllChildren()));
            return new ListControlItem[0];
        }
    }
}