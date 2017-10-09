using System;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;

namespace Nat.Web.Controls
{
    public class ExpandTableCell : DataControlFieldCell
    {
        private HtmlInputHidden _expand;
        private HtmlInputHidden _item;
        private HtmlInputHidden _parent;
        private HtmlAnchor _text;
        private HtmlInputHidden _visible;

        public ExpandTableCell(DataControlField containingField) : base(containingField)
        {
        }

        public ExpandTableCell(HtmlTextWriterTag tagKey, DataControlField containingField) : base(tagKey, containingField)
        {
        }

        public bool RowVisible
        {
            get { return !string.IsNullOrEmpty(_visible.Value); }
            set
            {
                if(value) _visible.Value = "1";
                else _visible.Value = "";
            }
        }

        public HtmlInputHidden Item
        {
            get { return _item; }
        }

        public HtmlInputHidden ParentItem
        {
            get { return _parent; }
        }

        public bool Collapsed
        {
            get { return string.IsNullOrEmpty(_expand.Value); }
            set
            {
                if(value) _expand.Value = "";
                else _expand.Value = "1";
            }
        }

        public string LevelText
        {
            get { return _text.InnerHtml; }
            set { _text.InnerHtml = value; }
        }

        public string LevelTab
        {
            get { return _text.Attributes["levelTab"]; }
            set { _text.Attributes["levelTab"] = value; }
        }

        public HtmlInputHidden Expand
        {
            get { return _expand; }
        }

        private void Initialize()
        {
            _expand = new HtmlInputHidden();
            _parent = new HtmlInputHidden();
            _item = new HtmlInputHidden();
            _visible = new HtmlInputHidden();
            _text = new HtmlAnchor();
            _expand.Value = "";
            _parent.Value = "";
            _item.Value = "";
            _visible.Value = "";
        }

        protected override void OnInit(EventArgs e)
        {
            Initialize();

            Controls.Add(_parent);
            Controls.Add(_expand);
            Controls.Add(_item);
            Controls.Add(_visible);
            Controls.Add(_text);

            base.OnInit(e);
        }
    }
}