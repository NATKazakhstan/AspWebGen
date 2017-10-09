/*
 * Created by: Daniil A. Kovalev
 * Created: 21.09.2007
 */

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data.SqlTypes;
using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using System.Drawing.Design;
using System.Globalization;
using System.Text.RegularExpressions;
using System.Web.UI;
using System.Web.UI.WebControls;
using AjaxControlToolkit;
using Nat.Tools.Classes;
using Nat.Tools.Specific;
using Nat.Web.Tools;


#region Resources

[assembly: WebResource("Nat.Web.Controls.DateTimeControls.calendar.png", "image/png")]
[assembly: WebResource("Nat.Web.Controls.DateTimeControls.DatePicker.js", "text/javascript")]

#endregion


namespace Nat.Web.Controls.DateTimeControls
{
    [ToolboxBitmap(typeof(CalendarExtender), "calendar.png")]
    [ValidationProperty("Text")]
    [ControlValuePropertyAttribute("Date")]
    [RequiredScript(typeof(CalendarExtender))]
    [RequiredScript(typeof(MaskedEditExtender))]
    [ClientScriptResource("Nat.Web.Controls.DateTimeControls.DatePicker", "Nat.Web.Controls.DateTimeControls.DatePicker.js")]
    public class DatePicker : CompositeControl, IEditableTextControl, IClientElementProvider, IScriptControl
    {
        #region Fields

        private CalendarExtender calendarExtender;
        private ImageButton imageButton;
        private TextBox textBox;
        private Table table;
        private Object date;
        private MaskedEditExtender maskedEditExtender;
        private string _caption = string.Empty;
        private RegularExpressionValidator validator;
        private CustomValidator cv;

        #endregion        

        #region Methods

        private string getRegExFromFormatTime(string format)
        {
            if (string.IsNullOrEmpty(format))
                format = "hh:mm";

            var exp = format.ToLower();
            exp = exp.Replace("hh", @"([0-1]\d|2[0-3])");
            exp = exp.Replace("mm", @"[0-5]\d");
            exp = exp.Replace("ss", @"[0-5]\d");
            return exp;
        }

        protected override void OnInit(EventArgs e)
        {
            base.OnInit(e);
            table = new Table();

            table.Width = Width;
            table.CellPadding = 0;
            table.CellSpacing = 0;
            table.Rows.Add(new TableRow());
            table.Rows[0].Cells.Add(new TableCell());
            table.Rows[0].Cells.Add(new TableCell());

            textBox = new TextBox();
            textBox.ID = "textBoxID";
            table.Rows[0].Cells[0].Controls.Add(textBox);
            textBox.AutoPostBack = AutoPostBack;
            textBox.ReadOnly = ReadOnly;
            textBox.TextChanged += TextBox_OnTextChanged;
            SetCurrentDateTime(date);

            if (Width != Unit.Empty)
            {
                textBox.Width = Unit.Percentage(100);
                table.Rows[0].Cells[0].Style["padding-right"] = "6px";
                table.Rows[0].Cells[1].Width = 1;
            }

            imageButton = new ImageButton();
            imageButton.ID = "imageButtonID";
            table.Rows[0].Cells[1].Controls.Add(imageButton);
            imageButton.OnClientClick = string.Format("return false;");
            imageButton.Enabled = !ReadOnly;

            // MaskedEditExtender 
            maskedEditExtender = new MaskedEditExtender();
            maskedEditExtender.TargetControlID = textBox.ID;

            // CalendarExtender
            calendarExtender = new CalendarExtender();
            calendarExtender.ID = "calendarExtenderID";
            calendarExtender.TargetControlID = textBox.ID;
            calendarExtender.PopupButtonID = imageButton.ID;

            InitializeExtenders(maskedEditExtender, calendarExtender, Format, Mode, Mask);

            Controls.Add(table);
            Controls.Add(calendarExtender);
            Controls.Add(maskedEditExtender);

            if (Mode == DatePickerMode.Time)
            {
                validator = new RegularExpressionValidator
                    {
                        ValidationExpression = getRegExFromFormatTime(Format),
                        ValidationGroup = ValidationGroup,
                        Text = Properties.Resources.SInvalidBindingInput,
                        ControlToValidate = textBox.ID,
                        Display = ValidatorDisplay.Dynamic,
                        EnableViewState = false,
                    };
                Controls.Add(validator);
            }
            else
            {
                cv = new CustomValidator
                    {
                        ValidationGroup = ValidationGroup,
                        Text = Properties.Resources.SInvalidBindingInput,
                        ControlToValidate = textBox.ID,
                        Display = ValidatorDisplay.Dynamic,
                        EnableViewState = false,
                    };
                cv.ServerValidate += Cv_OnServerValidate;
                Controls.Add(cv);
            }
        }

        internal static void InitializeExtenders(MaskedEditExtender maskedEditExtender, CalendarExtender calendarExtender, string format, DatePickerMode mode, string mask)
        {
            maskedEditExtender.AcceptAMPM = true;
            maskedEditExtender.ClearMaskOnLostFocus = true;
            maskedEditExtender.CultureName = CultureInfo.CurrentCulture.Name;

            // Setup format string
            if (string.IsNullOrEmpty(format))
            {
                if (mode == DatePickerMode.Date)
                {
                    calendarExtender.Format = CultureInfo.CurrentCulture.DateTimeFormat.ShortDatePattern;
                    maskedEditExtender.MaskType = MaskedEditType.Date;
                    var dateMask = CultureInfo.CurrentCulture.DateTimeFormat.ShortDatePattern;
                    dateMask = Regex.Replace(dateMask, "\\w", "9");
                    dateMask = Regex.Replace(dateMask, "\\W", "/");
                    maskedEditExtender.Mask = dateMask;
                }
                else if (mode == DatePickerMode.Time)
                {
                    calendarExtender.Format = CultureInfo.CurrentCulture.DateTimeFormat.ShortTimePattern;
                    maskedEditExtender.MaskType = MaskedEditType.Time;
                    var timeMask = CultureInfo.CurrentCulture.DateTimeFormat.ShortTimePattern;

                    timeMask = Regex.Replace(timeMask, @"(?<1>\w)(?<!\k<1>\k<1>)(?!\k<1>)", @"$1$1");
                    timeMask = Regex.Replace(timeMask, "\\w", @"9");
                    timeMask = Regex.Replace(timeMask, "\\W", @":");
                    maskedEditExtender.Mask = timeMask;
                }
                else
                {
                    calendarExtender.Format = CultureInfo.CurrentCulture.DateTimeFormat.ShortDatePattern + " " +
                                              CultureInfo.CurrentCulture.DateTimeFormat.ShortTimePattern;
                    maskedEditExtender.MaskType = MaskedEditType.DateTime;

                    var dateMask = CultureInfo.CurrentCulture.DateTimeFormat.ShortDatePattern;
                    dateMask = Regex.Replace(dateMask, "\\w", "9");
                    dateMask = Regex.Replace(dateMask, "\\W", "/");

                    var timeMask = CultureInfo.CurrentCulture.DateTimeFormat.ShortTimePattern;
                    timeMask = Regex.Replace(timeMask, @"(?<1>\w)(?<!\k<1>\k<1>)(?!\k<1>)", @"$1$1");
                    timeMask = Regex.Replace(timeMask, "\\w", @"9");
                    timeMask = Regex.Replace(timeMask, "\\W", @":");

                    maskedEditExtender.Mask = string.Format("{0} {1}", dateMask, timeMask);
                }
            }
            else
            {
                calendarExtender.Format = format;

                if (mode == DatePickerMode.Date)
                    maskedEditExtender.MaskType = MaskedEditType.Date;
                else if (mode == DatePickerMode.Time)
                    maskedEditExtender.MaskType = MaskedEditType.Time;
                else
                    maskedEditExtender.MaskType = MaskedEditType.DateTime;

                if (string.IsNullOrEmpty(mask))
                    maskedEditExtender.Enabled = false;
                else
                    maskedEditExtender.Mask = mask;
            }
        }

        private void Cv_OnServerValidate(object source, ServerValidateEventArgs args)
        {
            args.IsValid = true;
            var yyyy = 0;
            var MM = 0;
            var dd = 0;
            var hh = 0;
            var mm = 0;
            var ss = 0;
            var maxDay = 0;

            var ds = CultureInfo.CurrentCulture.DateTimeFormat.DateSeparator;
            var r = Mode == DatePickerMode.Date ? new Regex(@"(?<dd>\d\d?)?(" + ds + @"(?<MM>\d\d?))?(" + ds + @"(?<yyyy>\d\d\d\d))") : 
                new Regex(@"(?<dd>\d\d?)?(" + ds + @"(?<MM>\d\d?))?(" + ds + @"(?<yyyy>\d\d\d\d)) " + @"(?<hh>\d\d?)?(:(?<mm>\d\d?))?(:(?<ss>\d\d?))");

            var match = r.Match(args.Value);

            if (match.Success)
            {
                if (match.Groups["yyyy"].Success)
                    yyyy = int.Parse(match.Groups["yyyy"].Value);
                if (match.Groups["MM"].Success)
                    MM = int.Parse(match.Groups["MM"].Value);
                if (match.Groups["dd"].Success)
                    dd = int.Parse(match.Groups["dd"].Value);
                if (match.Groups["hh"].Success)
                    hh = int.Parse(match.Groups["hh"].Value);
                if (match.Groups["mm"].Success)
                    mm = int.Parse(match.Groups["mm"].Value);
                if (match.Groups["ss"].Success)
                    ss = int.Parse(match.Groups["ss"].Value);
            }

            if (yyyy < 1800 || yyyy > 2200)
            {
                args.IsValid = false;
            }
            else if (MM < 1 || MM > 12)
            {
                args.IsValid = false;
            }
            else
            {
                maxDay = DateTime.DaysInMonth(yyyy, MM);
                if (dd < 1 || dd > maxDay)
                    args.IsValid = false;
            }
            if (Mode == DatePickerMode.DateTime)
            {
                if (hh < 0 || hh >= 24)
                {
                    args.IsValid = false;
                }
                else if (mm < 0 || mm >= 60)
                {
                    args.IsValid = false;
                }
                else if (ss < 0 || ss >= 60)
                {
                    args.IsValid = false;
                }
            }
        }

        protected override void OnPreRender(EventArgs e)
        {
            base.OnPreRender(e);

            if (Mode == DatePickerMode.Time)
            {
                table.Rows[0].Cells.RemoveAt(1);
                Controls.Remove(calendarExtender);
                calendarExtender.Enabled = false;
            }

            if(String.IsNullOrEmpty(ImageUrl))
                imageButton.ImageUrl = Page.ClientScript.GetWebResourceUrl(GetType(), "Nat.Web.Controls.DateTimeControls.calendar.png");
            else
                imageButton.ImageUrl = ImageUrl;

            if (!DesignMode)
            {
                ScriptObjectBuilder.RegisterCssReferences(this);
                ScriptManager.GetCurrent(Page).RegisterScriptControl(this);
            }

        }

        protected override void Render(HtmlTextWriter writer)
        {
            if (!DesignMode)
                ScriptManager.GetCurrent(Page).RegisterScriptDescriptors(this);

            imageButton.Style["filter"] = Enabled ? "" : "gray";

            base.Render(writer);
        }

        private object GetCurrentDateTime()
        {
            Boolean hasDigits = textBox.Text.IndexOfAny("0123456789".ToCharArray()) != -1;
            DateTime dateTime;
            if (string.IsNullOrEmpty(textBox.Text) || !hasDigits)
                return null;

            if (Mode != DatePickerMode.Time && !cv.IsValid)
                return null;
            try
            {
                if (String.IsNullOrEmpty(Format))
                {
                    dateTime = Convert.ToDateTime(textBox.Text);
                }
                else
                {
                    DateTimeFormatInfo formatInfo = new DateTimeFormatInfo();
                    if (Mode == DatePickerMode.Time)
                    {
                        Regex r = new Regex(@"(?<hh>\d\d?)?(:(?<mm>\d\d?))?(:(?<ss>\d\d?))");
                        var hh = 0;
                        var mm = 0;
                        var ss = 0;
                        var match = r.Match(textBox.Text);
                        if (match.Success)
                        {
                            if (match.Groups["hh"].Success)
                                hh = int.Parse(match.Groups["hh"].Value);
                            if (match.Groups["mm"].Success)
                                mm = int.Parse(match.Groups["mm"].Value);
                            if (match.Groups["ss"].Success)
                                ss = int.Parse(match.Groups["ss"].Value);
                        }
                        dateTime = DateTime.Now.Date.Add(new TimeSpan(hh, mm, ss));
                    }
                    else
                    {
                        formatInfo.ShortDatePattern = Format;
                        dateTime = Convert.ToDateTime(textBox.Text, formatInfo);
                    }
                }
                if (Mode == DatePickerMode.Date)
                    return dateTime.Date;
                if (Mode == DatePickerMode.Time)
                {
                    return new DateTime(SpecificInstances.DbConstants.MinDate.Year,
                                        SpecificInstances.DbConstants.MinDate.Month, SpecificInstances.DbConstants.MinDate.Day,
                                        dateTime.Hour, dateTime.Minute, dateTime.Second, dateTime.Millisecond);
                }
                return dateTime;
            }
            catch (Exception e)
            {
            }
            return null;
        }

        private void SetCurrentDateTime(Object value)
        {
            if (value == DBNull.Value || value == null)
                textBox.Text = string.Empty;
            else
            {
                DateTime date;

                if (String.IsNullOrEmpty(Format))
                    date = Convert.ToDateTime(value);
                else
                {
                    DateTimeFormatInfo formatInfo = new DateTimeFormatInfo();
                    formatInfo.ShortDatePattern = Format;
                    date = Convert.ToDateTime(value, formatInfo);
                }

                if (String.IsNullOrEmpty(Format))
                {
                    if (Mode == DatePickerMode.Date)
                        textBox.Text = date.ToShortDateString();
                    else if (Mode == DatePickerMode.Time)
                        textBox.Text = date.ToShortTimeString();
                    else
                        textBox.Text = date.ToString(
                            String.Format("{0} {1}",
                                          CultureInfo.CurrentCulture.DateTimeFormat.ShortDatePattern,
                                          CultureInfo.CurrentCulture.DateTimeFormat.ShortTimePattern));
                }
                else
                    textBox.Text = date.ToString(Format);
            }
        }

        private void TextBox_OnTextChanged(object sender, EventArgs e)
        {
            OnDateChanged(e);
        }

        #endregion

        #region Events

        public event EventHandler DateChanged;

        protected virtual void OnDateChanged(EventArgs e)
        {
            if (DateChanged != null)
                DateChanged(this, e);

            if (TextChanged != null)
                TextChanged(this, e);
        }

        #endregion

        #region Properties

        [DefaultValue(DatePickerMode.Date)]
        [Themeable(false)]
        [Category("Behavior")]
        public DatePickerMode Mode
        {
            get { return GetPropertyValue("Mode", DatePickerMode.Date); }
            set { SetPropertyValue("Mode", value); }
        }

        [DefaultValue(null)]
        [Bindable(true, BindingDirection.TwoWay)]
        public Object Date
        {
            get
            {   
                if (textBox != null)
                    return GetCurrentDateTime();
                return date;
            }
            set
            {
                date = value;
                if (textBox != null)
                    SetCurrentDateTime(value);
            }
        }

        [DefaultValue(null)]
        [Bindable(true, BindingDirection.TwoWay)]
        public Object Value
        {
            get { return Date; }
            set { Date = value; }
        }

        [DefaultValue(null)]
        [Bindable(true, BindingDirection.TwoWay)]
        public Object SelectedValue
        {
            get { return Date; }
            set { Date = value; }
        }

        [DefaultValue(false)]
        [Themeable(false)]
        [Category("Behavior")]
        public virtual bool AutoPostBack
        {
            get { return GetPropertyValue("AutoPostBack", false); }
            set
            {
                if(textBox != null) textBox.AutoPostBack = value;
                SetPropertyValue("AutoPostBack", value);
            }
        }
        
        [DefaultValue(false)]
        [Themeable(false)]
        [Category("Behavior")]
        public virtual bool ReadOnly
        {
            get { return GetPropertyValue("ReadOnly", false); }
            set
            {
                if (textBox != null) textBox.ReadOnly = value;
                SetPropertyValue("ReadOnly", value);
            }
        }

        [DefaultValue("")]
        [UrlProperty]
        [Bindable(true)]
        [SuppressMessage("Microsoft.Design", "CA1056:UriPropertiesShouldNotBeStrings", Justification = "Using string to avoid Uri complications")]
        [Editor("System.Web.UI.Design.ImageUrlEditor, System.Design, Version=2.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a", typeof(UITypeEditor))]
        public string ImageUrl
        {
            get { return GetPropertyValue("ImageUrl", (String)null); }
            set { SetPropertyValue("ImageUrl", value); }
        }

        [Category("Misc")]
        [DefaultValue("")]
        public string Format
        {
            get { return GetPropertyValue("Format", ""); }
            set
            {
                SetPropertyValue("Format", value);
                if (calendarExtender != null)
                {
                    calendarExtender.Format = value;
                    calendarExtender.Enabled = !string.IsNullOrEmpty(calendarExtender.Format);
                    if (Mode == DatePickerMode.Time)
                        validator.ValidationExpression = getRegExFromFormatTime(Format);
                }
            }
        }

        [Category("Misc")]
        [DefaultValue("")]
        public string Mask
        {
            get { return GetPropertyValue("Mask", ""); }
            set
            {
                SetPropertyValue("Mask", value);
                if (maskedEditExtender != null)
                {
                    maskedEditExtender.Mask = value;
                    maskedEditExtender.Enabled = !string.IsNullOrEmpty(maskedEditExtender.Mask);
                }
            }
        }

        [Browsable(false)]
        public string PopupBehaviorParentNode
        {
            get { return GetPropertyValue("PopupBehaviorParentNode", ""); }
            set { SetPropertyValue("PopupBehaviorParentNode", value); }
        }

        protected V GetPropertyValue<V>(string propertyName, V nullValue)
        {
            if(ViewState[propertyName] == null)
                return nullValue;
            return (V)ViewState[propertyName];
        }

        protected void SetPropertyValue<V>(string propertyName, V value)
        {
            ViewState[propertyName] = value;
        }

        [Category("Misc")]
        [DefaultValue("")]
        public string ValidationGroup
        {
            get { return GetPropertyValue("ValidationGroup", ""); }
            set { SetPropertyValue("ValidationGroup", value); }
        }

        [Category("Misc")]
        [DefaultValue("")]
        public string Caption
        {
            get { return _caption; }
            set
            {
                _caption = value;
                if (validator != null)
                    validator.ErrorMessage = string.Format(Properties.Resources.SInvalidFieldFormat, _caption);
            }
        }



        #endregion

        #region IClientElementProvider Members

        public ICollection<Control> GetInputControls()
        {
            return new Control[]{ textBox };
        }

        public ICollection<string> GetInputElements()
        {
            return new String[]{ textBox.ClientID };
        }

        public ICollection<Pair<string, IFormatProvider>> GetInputFormatProviders()
        {
            if(String.IsNullOrEmpty(Format))
            {
                return new Pair<string, IFormatProvider>[]
                    {
                        new Pair<string, IFormatProvider>(textBox.ClientID,
                                                          CultureInfo.CurrentCulture.DateTimeFormat)
                    };
            }
            else
            {
                DateTimeFormatInfo formatInfo = new DateTimeFormatInfo();
                formatInfo.ShortDatePattern = Format;
                return new Pair<string, IFormatProvider>[]
                    {
                        new Pair<string, IFormatProvider>(textBox.ClientID,
                                                          formatInfo)
                    };
            }
        }

        #endregion

        #region IEditableTextControl Members

        public event EventHandler TextChanged;


        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public string Text
        {
            get { return textBox.Text; }
            set { textBox.Text = value; }
        }

        #endregion

        #region IScriptControl Members

        public IEnumerable<ScriptDescriptor> GetScriptDescriptors()
        {
            if (Page != null && Visible)
            {
                var desc = new ScriptControlDescriptor("Nat.Web.Controls.DateTimeControls.DatePicker", ClientID);

                desc.AddProperty("calendarExtenderID", calendarExtender.BehaviorID);
                desc.AddProperty("mode", Mode);
                desc.AddProperty("popupBehaviorParentNode", PopupBehaviorParentNode);
                desc.AddProperty("imageButtonID", imageButton.ClientID);
                yield return desc;
            }
        }

        public IEnumerable<ScriptReference> GetScriptReferences()
        {
            var references = new List<ScriptReference>();
            references.AddRange(ScriptObjectBuilder.GetScriptReferences(GetType()));
            return references;
        }

        #endregion
    }
}