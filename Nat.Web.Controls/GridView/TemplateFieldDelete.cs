/*
 * Created by : Eugene P. Kolesnikov
 * Created    : 01.10.2007
 * Copyright © New Age Technologies
 */


using System.Web.UI;
using System.Web.UI.WebControls;

namespace Nat.Web.Controls
{
    /// <summary>
    /// Класс реализующий возможность создавать TemplateField динамически. 
    /// </summary>
    internal class TemplateFieldDelete : ITemplate
    {
        private bool _confrimDelete = true;

        #region Constants

        private const string _commandName = "Delete";
        private const string _text = "Удалить";
        private const string JScriptCode = "javascript:return confirm('Удалить запись?');";

        #endregion

        #region Private fields

        private ButtonType _buttonType;
        private Button btDelete;
        private LinkButton lbDelete;

        #endregion

        #region Properties

        public ButtonType ButtonType
        {
            get { return _buttonType; }
            set { _buttonType = value; }
        }

        public bool ConfrimDelete
        {
            get { return _confrimDelete; }
            set { _confrimDelete = value; }
        }

        #endregion

        public TemplateFieldDelete(bool _confrimDelete)
        {
            this._confrimDelete = _confrimDelete;
        }

        #region ITemplate Members

        void ITemplate.InstantiateIn(Control container)
        {
            if (_confrimDelete)
            {
                switch (_buttonType)
                {
                    case ButtonType.Link:
                        {
                            lbDelete = new LinkButton();
                            lbDelete.CommandName = _commandName;
                            lbDelete.Text = _text;
                            lbDelete.CausesValidation = false;
                            lbDelete.OnClientClick = JScriptCode;
                            container.Controls.Add(lbDelete);
                            break;
                        }
                    case ButtonType.Button:
                        {
                            btDelete = new Button();
                            btDelete.CommandName = _commandName;
                            btDelete.Text = _text;
                            btDelete.CausesValidation = false;
                            btDelete.OnClientClick = JScriptCode;
                            container.Controls.Add(btDelete);
                            break;
                        }
                    case ButtonType.Image:
                        {
                            break;
                        }
                }
            }
        }

        #endregion
    }
}