/*
 * Created by: Sergey V. Shpakovskiy
 * Created: 7 апреля 2008 г.
 * Copyright © JSC New Age Technologies 2008
 */

using System.Collections.Generic;
using System.ComponentModel;
using System.Web.Script.Serialization;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace Nat.Web.Controls
{
    [ParseChildren(true)]
    [PersistChildren(false)]
    public class EnableList : IEnableControl
    {
        [DefaultValue(EnableListMode.And), PersistenceMode(PersistenceMode.Attribute)]
        public EnableListMode ListMode { get; set; }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        [Browsable(false)]
        public bool Disable
        {
            get { return false; }
            set { throw new System.NotImplementedException(); }
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        [Browsable(false)]
        public string ControlID
        {
            get { return null; }
            set { throw new System.NotImplementedException(); }
        }

        public string GetClientID()
        {
            return null;
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        [ScriptIgnore]
        [Browsable(false)]
        public EnableControls EnableControls { get; set; }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        [Browsable(false)]
        [ScriptIgnore]
        public Control Owner { get; set; }

        public void RefreshValues()
        {
        }

        public TypeComponent TypeComponent
        {
            get { return TypeComponent.ListItems; }
        }

        public bool IsEquals()
        {
            throw new System.NotImplementedException();
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Visible), PersistenceMode(PersistenceMode.InnerProperty)]
        public List<IEnableControl> Items { get; set; }

        public EnableList()
        {
            ListMode = EnableListMode.And;
            Items = new List<IEnableControl>();
        }

        public IEnableControl Clone()
        {
            var clone = new EnableList(){ListMode = ListMode};
            foreach (var item in Items)
                clone.Items.Add(item.Clone());
            return clone;
        }
    }
}