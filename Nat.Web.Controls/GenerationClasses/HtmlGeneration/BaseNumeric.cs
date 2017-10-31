using System;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Threading;
using System.Web.UI;
using System.Web.UI.WebControls;
using Nat.Web.Controls.Properties;

namespace Nat.Web.Controls.GenerationClasses
{
    [DefaultProperty("ValueD")]
    [DefaultBindingProperty("ValueD")]
    [ValidationPropertyAttribute("ValueD")]
    public class BaseNumeric : BaseEditControl, INumeric, IPostBackDataHandler
    {
        private object _value;
        public string Format { get; set; }
        public int Length { get; set; }
        public int Precision { get; set; }
        public decimal? MinValue { get; set; }
        public decimal? MaxValue { get; set; }
        private decimal? _valueD;

        public decimal? ValueD
        {
            get { return _valueD; }
            set 
            { 
                _valueD = value;
                _value = value;
            }
        }

        public override object Value
        {
            get { return ValueD ?? _value; }
            set
            {
                if ("".Equals(value))
                    value = null;
                _valueD = null;
                _value = value;
                try
                {
                    if (value != null)
                        _valueD = (decimal?)Convert.ChangeType(value, typeof(decimal));
                    else
                        _valueD = null;
                }
                catch (Exception)
                {
                    if (value != null)
                        AddValidator(new RequiredValidatorProperties
                                         {
                                             InitialValue = value.ToString(),
                                             ErrorMessage = Resources.SNotCorrectNumber,
                                         });
                    throw;
                }
            }
        }

        public override string Text
        {
            get { return ValueD == null ? string.Empty : string.Format(GetFormat(), ValueD); }
            set {  }
        }

        public bool HideEditor { get; set; }

        protected override void InitValidatorProperties()
        {
            base.InitValidatorProperties();

            RegexValidatorProperties regexValidator;

            #region Length

            if (Length > 0)
            {
                regexValidator = new RegexValidatorProperties();
                var length = Length + (Precision > 0 ? 1 : 0);
                regexValidator.RegularExpression = @"-?.{0," + length + "}";
                regexValidator.ErrorMessage = string.Format("<span title='{0}'>&gt;{1}</span>",
                                                            string.Format(Resources.EMaxLength, length), length);
                regexValidator.ErrorMessageInSummary = string.Format(Resources.SMaxLengthOfField, "{0}", length);
                AddValidator(regexValidator);
            }

            #endregion

            #region Format
            regexValidator = new RegexValidatorProperties();
            regexValidator.RegularExpression = @"-?\d*" + (Precision > 0 ? @"(\.|,)?\d*" : "");
            string format = Length == 0 ? "123" : "";
            for (int i = 0; i < Length; i++)
            {
                if (Length > Precision)
                    format += i.ToString()[0];
                else if(Length == Precision)
                    format += "." + i.ToString()[0];
                else
                    format += i.ToString()[0];
            }
            regexValidator.ErrorMessage = string.Format("<span title='{0}'>{1}</span>", Resources.SNotCorrectNumber, format);
            regexValidator.ErrorMessageInSummary = Resources.SNotCorrectNumberOfField;
            AddValidator(regexValidator);
            #endregion

            #region Min Max Value
            if (Precision == 0 && (MinValue != null || MaxValue != null))
            {
                var message = string.Format(Web.Controls.Properties.Resources.SRangeValidatorErrorMessage,
                                            MinValue, MaxValue);
                var summaryMessage = string.Format(Web.Controls.Properties.Resources.SRangeValidatorErrorMessageInSummary,
                                            MinValue, MaxValue, "{0}");
                var rangeValidator =
                    new RangeValidatorProperties
                        {
                            MinimumValue = MinValue.ToString(),
                            MaximumValue = MaxValue.ToString(),
                            Type = ValidationDataType.Integer,
                            ErrorMessage = string.Format("<span title='{0}'>{1} &lt;= X &gt;= {2}</span>",
                                                         message, MinValue, MaxValue),
                            ErrorMessageInSummary = summaryMessage,
                        };
                AddValidator(rangeValidator);
            }
            #endregion
        }

        protected override void OnPreRender(EventArgs e)
        {
            base.OnPreRender(e);
            foreach (var item in ValidatorProperties)
                item.RegisterClientValidator(Page);
        }

        protected override void RenderContents(HtmlTextWriter writer)
        {
            if (HideEditor)
            {
                writer.AddAttribute(HtmlTextWriterAttribute.Class, "nat-hideEditorDIV");
                writer.RenderBeginTag(HtmlTextWriterTag.Div);

                writer.AddAttribute(HtmlTextWriterAttribute.Href, "javascript: void(0)");
                writer.AddAttribute(HtmlTextWriterAttribute.Title, Resources.SEditText);
                writer.AddStyleAttribute("min-width", Width.ToString());
                writer.RenderBeginTag(HtmlTextWriterTag.A);
                writer.Write(Text);
                writer.RenderEndTag();

                writer.AddStyleAttribute(HtmlTextWriterStyle.Display, "none");
                writer.RenderBeginTag(HtmlTextWriterTag.Div);
            }

            string uniqueID = ((IRenderComponent)this).UniqueID;
            string clientID = ((IRenderComponent)this).ClientID;
            if (!string.IsNullOrEmpty(uniqueID)) writer.AddAttribute(HtmlTextWriterAttribute.Name, uniqueID);
            if (!string.IsNullOrEmpty(clientID)) writer.AddAttribute(HtmlTextWriterAttribute.Id, clientID);
            if (!Width.IsEmpty) 
                writer.AddStyleAttribute(HtmlTextWriterStyle.Width, Width.ToString());
            else if (Length > 0)
                writer.AddAttribute(HtmlTextWriterAttribute.Cols, (Length + 3).ToString());
            else
                writer.AddAttribute(HtmlTextWriterAttribute.Cols, "15");
            if (!Enabled) writer.AddAttribute(HtmlTextWriterAttribute.Disabled, "disabled");
            if (Length > 0)
                writer.AddAttribute(HtmlTextWriterAttribute.Maxlength,
                                    (Length + 1 + (Precision == 0 ? 0 : 1)).ToString());
            writer.AddAttribute(HtmlTextWriterAttribute.Type, "text");
            writer.AddAttribute(HtmlTextWriterAttribute.Value, Text);
            writer.RenderBeginTag(HtmlTextWriterTag.Input);
            writer.RenderEndTag();


            if (HideEditor)
            {
                writer.RenderEndTag();
                writer.RenderEndTag();
            }
        }

        private string GetFormat()
        {
            if (!string.IsNullOrEmpty(Format))
                return Format;

            var format = Length == 0 ? "{0" : "{0:";
            for (int i = 0; i < Length; i++)
            {
                if (Length > Precision)
                    format += "#";
                else if (Length == Precision)
                    format += ".#";
                else
                    format += "#";
            }
            format += "}";
            return format;
        }

        public override void Render(HtmlTextWriter writer, ExtenderAjaxControl extenderAjaxControl)
        {
            RenderContents(writer);
        }

        public bool LoadPostData(string postDataKey, NameValueCollection postCollection)
        {
            var valueD = ValueD;
            var newValueStr = postCollection[postDataKey];
            var decimalSeparator = Thread.CurrentThread.CurrentUICulture.NumberFormat.NumberDecimalSeparator;
            if (!string.IsNullOrEmpty(newValueStr))
                newValueStr = decimalSeparator == "."
                                  ? newValueStr.Replace(",", ".")
                                  : newValueStr.Replace(".", ",");
            if (!valueD.ToString().Equals(newValueStr))
            {
                Value = newValueStr;
                return true;
            }
            return false;
        }

        public void RaisePostDataChangedEvent()
        {
        }
    }
}