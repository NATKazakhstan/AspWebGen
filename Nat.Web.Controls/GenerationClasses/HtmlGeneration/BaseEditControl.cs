using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Web.UI;
using System.Web.UI.WebControls;
using Nat.Web.Controls.GenerationClasses.BaseJournal;
using Nat.Web.Controls.Properties;

namespace Nat.Web.Controls.GenerationClasses
{
    using System.IO;
    using System.Linq;
    using System.Text;

    [DefaultProperty("Value")]
    [DefaultBindingProperty("Value")]
    [ValidationPropertyAttribute("Value")]
    public abstract class BaseEditControl : WebControl, IRenderComponent
    {
        private bool _inited;

        protected BaseEditControl()
        {
            ValidatorProperties = new List<ValidatorProperties>();
            EnableViewState = false;
        }

        string IRenderComponent.ClientID
        {
            get { return ClientID; }
            set { }
        }

        string IRenderComponent.UniqueID
        {
            get { return UniqueID; }
            set { }
        }

        public string ValidationGroup { get; set; }
        protected List<ValidatorProperties> ValidatorProperties { get; private set; }

        public abstract object Value { get; set; }
        public abstract string Text { get; set; }
        public virtual bool NoWrap { get; set; }
        public virtual bool Mandatory { get; set; }

        protected override void OnInit(EventArgs e)
        {
            base.OnInit(e);
            InitValidatorProperties();
            foreach (var validatorPropertiese in ValidatorProperties)
                Controls.Add(validatorPropertiese.CreateValidator(ID, ValidationGroup));
            _inited = true;
        }

        public override void RenderBeginTag(HtmlTextWriter writer)
        {
            if (NoWrap)
            {
                writer.AddAttribute(HtmlTextWriterAttribute.Wrap, "none");
                writer.RenderBeginTag(HtmlTextWriterTag.Span); //span
            }
        }
        
        protected override void Render(HtmlTextWriter writer)
        {
            RenderBeginTag(writer);
            RenderContents(writer);
            RenderChildren(writer);
            RenderEndTag(writer);
        }

        protected virtual void AddAttributes(HtmlTextWriter writer)
        {
            foreach (var key in Attributes.Keys.Cast<string>())
                writer.AddAttribute(key, Attributes[key]);

            foreach (string key in Style.Keys)
                writer.AddStyleAttribute(key, Style[key]);
        }

        public override void RenderEndTag(HtmlTextWriter writer)
        {
            if (NoWrap)
                writer.RenderEndTag(); //span
        }

        public virtual void AddValidators(IEnumerable<ValidatorProperties> validators)
        {
            ValidatorProperties.AddRange(validators);
            if (_inited)
                foreach (var validator in validators)
                    Controls.Add(validator.CreateValidator(ID, ValidationGroup));
        }

        public virtual void AddValidator(ValidatorProperties validator)
        {
            ValidatorProperties.Add(validator);
            if (_inited)
                Controls.Add(validator.CreateValidator(ID, ValidationGroup));
        }

        public bool ValidateValue(string value, RenderContext renderContext)
        {
            var isValid = true;
            foreach (var validator in ValidatorProperties)
                if (!validator.ValidateValue(value))
                {
                    renderContext.AddErrorMessage(string.Format(validator.ErrorMessageInSummary, renderContext.Column.Header));
                    isValid = false;
                }
            return isValid;
        }

        protected virtual void InitValidatorProperties()
        {
            if (Mandatory)
                AddMandatoryValidator();
        }

        protected virtual void AddMandatoryValidator()
        {
            var validator = new RequiredValidatorProperties
                                 {
                                     ErrorMessage = "*",
                                     ErrorMessageInSummary = Resources.SRequiredFieldMessage,
                                 };
            AddValidator(validator);
        }

        public virtual void Render(StringBuilder sb, ExtenderAjaxControl extenderAjaxControl)
        {
             using (var textWriter = new StringWriter(sb))
             using (var writer = new HtmlTextWriter(textWriter))
                 Render(writer, extenderAjaxControl);
        }

        public virtual void Render(HtmlTextWriter writer, ExtenderAjaxControl extenderAjaxControl)
        {
            throw new NotSupportedException();
        }

        public virtual void InitAjax(Control control, ExtenderAjaxControl extenderAjaxControl)
        {
            throw new NotSupportedException();
        }
    }
}