/*
 * Created by: Arman K. Karibaev
 * Created: 07.11.2007
 * Copyright © JSC NAT Kazakhstan 2007
 */

using System;
using System.ComponentModel;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace Nat.Web.Controls.CommandFieldExt
{
    [Serializable]
    public class AdditionalButton : ButtonField
    {
        private string controlID;

        # region Browsable = false

        [Browsable(false)]
        public override string DataTextField
        {
            get { return base.DataTextField; }
            set { base.DataTextField = value; }
        }

        [Browsable(false)]
        public override string DataTextFormatString
        {
            get { return base.DataTextFormatString; }
            set { base.DataTextFormatString = value; }
        }

        [Browsable(false)]
        public override bool CausesValidation
        {
            get { return base.CausesValidation; }
            set { base.CausesValidation = value; }
        }

        [Browsable(false)]
        public override bool ShowHeader
        {
            get { return base.ShowHeader; }
            set { base.ShowHeader = value; }
        }

        [Browsable(false)]
        public override string ValidationGroup
        {
            get { return base.ValidationGroup; }
            set { base.ValidationGroup = value; }
        }

        [Browsable(false)]
        public override string AccessibleHeaderText
        {
            get { return base.AccessibleHeaderText; }
            set { base.AccessibleHeaderText = value; }
        }

        [Browsable(false)]
        public override string FooterText
        {
            get { return base.FooterText; }
            set { base.FooterText = value; }
        }

        [Browsable(false)]
        public override string HeaderImageUrl
        {
            get { return base.HeaderImageUrl; }
            set { base.HeaderImageUrl = value; }
        }

        [Browsable(false)]
        public override string HeaderText
        {
            get { return base.HeaderText; }
            set { base.HeaderText = value; }
        }

        [Browsable(false)]
        public override bool InsertVisible
        {
            get { return base.InsertVisible; }
            set { base.InsertVisible = value; }
        }

        [Browsable(false)]
        public override string SortExpression
        {
            get { return base.SortExpression; }
            set { base.SortExpression = value; }
        }

        [Browsable(false)]
        public new Style ControlStyle
        {
            get { return base.ControlStyle; }
        }

        [Browsable(false)]
        public new TableItemStyle FooterStyle
        {
            get { return base.FooterStyle; }
        }

        [Browsable(false)]
        public new TableItemStyle HeaderStyle
        {
            get { return base.HeaderStyle; }
        }

        [Browsable(false)]
        public new TableItemStyle ItemStyle
        {
            get { return base.ItemStyle; }
        }

        [Browsable(false)]
        public override ButtonType ButtonType
        {
            get { return base.ButtonType; }
            set { base.ButtonType = value; }
        }

        # endregion

        # region перекрытые методы

        protected override DataControlField CreateField()
        {
            return new AdditionalButton();
        }

        public override string ToString()
        {
            return base.Text;
        }

        # endregion

        [Themeable(false)] 
        [ParenthesizePropertyName(true)] 
        [MergableProperty(false)] 
        [Filterable(false)]
        public virtual string ControlID
        {
            get { return controlID; }
            set { controlID = value; }
        }
    }
}