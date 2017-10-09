using System;
using System.Web.UI;
using System.Web.UI.WebControls;
using Nat.Web.Controls.Properties;

namespace Nat.Web.Controls
{
    public class TemplateFieldNew : ITemplate
    {
        private readonly string _commandName = "new";
        private ButtonType _buttonType;
        private string _text = Resources.SAdd;
        private Button btNew;
        private LinkButton lbNew;
        private ImageButton ibNew;
        private string _url;
        internal const string btnnewtreecolumn = "btnNewTreeColumn";


        public TemplateFieldNew(string _commandName)
        {
            this._commandName = _commandName;
        }

        public TemplateFieldNew() {}

        public string Text
        {
            get { return _text; }
            set { _text = value; }
        }

        public ButtonType ButtonType
        {
            get { return _buttonType; }
            set { _buttonType = value; }
        }

        public string Url
        {
            get { return _url; }
            set { _url = value; }
        }

        #region ITemplate Members

        public void InstantiateIn(Control container)
        {
            switch(_buttonType)
            {
                case ButtonType.Link:
                    lbNew = new LinkButton();
                    lbNew.ID = btnnewtreecolumn;
                    lbNew.CommandName = _commandName;
                    lbNew.Text = _text;
                    lbNew.CausesValidation = true;
                    container.Controls.Add(lbNew);
                    break;
                case ButtonType.Button:
                    btNew = new Button();
                    btNew.ID = btnnewtreecolumn;
                    btNew.CommandName = _commandName;
                    btNew.Text = _text;
                    btNew.CausesValidation = true;
                    container.Controls.Add(btNew);
                    break;
                case ButtonType.Image:
                    ibNew = new ImageButton();
                    ibNew.ID = btnnewtreecolumn;
                    ibNew.CommandName = _commandName;
                    ibNew.ToolTip = _text;
                    ibNew.Init += delegate(object sender, EventArgs e)
                                      {
                                          string url = _url;
                                          if (string.IsNullOrEmpty(url))
                                          {
                                              url = ibNew.Page.ClientScript.GetWebResourceUrl(typeof(CommandFieldExt.CommandFieldExt),
                                                                                                  "Nat.Web.Controls.CommandFieldExt.Images.new.gif");
                                          }
                                          ibNew.ImageUrl = url;
                                      };
                    ibNew.CausesValidation = true;
                    container.Controls.Add(ibNew);
                    break;
            }
        }

        #endregion
    }
}