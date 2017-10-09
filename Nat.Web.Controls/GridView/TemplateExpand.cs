using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;

namespace Nat.Web.Controls
{
    public class TemplateExpand : ITemplate
    {
        private readonly TableCell cell;

        public TemplateExpand() {}

        public TemplateExpand(TableCell cell) 
        {
            this.cell = cell;
            _parent = (HtmlInputHidden)cell.Controls[0];
            _expand = (HtmlInputHidden)cell.Controls[1];
            _item = (HtmlInputHidden)cell.Controls[2];
            _visible = (HtmlInputHidden)cell.Controls[3];
            _text = (HtmlAnchor)cell.Controls[4];
        }

        public void InstantiateIn(Control container)
        {
            Initialize();

            container.Controls.Add(_parent);
            container.Controls.Add(_expand);
            container.Controls.Add(_item);
            container.Controls.Add(_visible);
            container.Controls.Add(_text);
        }

        private HtmlInputHidden _expand;
        private HtmlInputHidden _item;
        private HtmlInputHidden _parent;
        private HtmlAnchor _text;
        private HtmlInputHidden _visible;

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
            _item.Attributes["role"] = "row-id";
            _visible.Value = "";
            _visible.Attributes["role"] = "row-visible";
        }

        public TableCell Cell
        {
            get { return cell; }
        }

    }
}