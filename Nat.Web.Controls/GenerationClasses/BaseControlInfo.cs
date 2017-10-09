/*
 * Created by: Sergey V. Shpakovskiy
 * Created: 21 мая 2009 г.
 * Copyright © JSC New Age Technologies 2009
 */

using System.Collections.Generic;
using System.Web.UI.WebControls;
using System;
using Nat.Web.Controls.DateTimeControls;
using System.Web.UI;
namespace Nat.Web.Controls.GenerationClasses
{
    public class BaseControlInfo
    {
        private Dictionary<string, object> _savedValues;
        private readonly Dictionary<string, Field> _fields = new Dictionary<string, Field>();
        private bool _isEdit;
        private bool _isInsert;
        private bool _isRead;

        public BaseControlInfo()
        {
        }

        public BaseControlInfo(FormView formView)
        {
            FormView = formView;
            FormControl = formView;
            FormView.DataBound += FormView_OnDataBound;
        }

        public BaseControlInfo(Control control)
        {
            FormControl = control;
        }

        private void FormView_OnDataBound(object sender, EventArgs e)
        {
            ClearRefToControls();
            WireToChangesEvent();
            WireToEditFieldEvents();
        }

        public virtual void WireToChangesEvent()
        {
        }

        public virtual void WireToEditFieldEvents()
        {
        }

        public virtual void ClearRefToControls()
        {
            //_isWireToChangesEvents = false;
            _fields.Clear();
        }

        public virtual bool HasSavedValues
        {
            get { return _savedValues != null; }
        }

        public virtual void SaveValues()
        {
            if (_savedValues == null)
                _savedValues = new Dictionary<string, object>();
            else
                _savedValues.Clear();
        }

        public virtual void LoadValues()
        {
            if (_savedValues == null)
                throw new NullReferenceException("Before execute LoadValues need execute SaveValues");
        }

        public virtual void SetValue(string property, object value, string text, string alternativeText)
        {
        }

        public FormView FormView { get; protected set; }

        public Control FormControl { get; protected set; }

        public Func<string> GetSelectedValue { get; set; }

        public bool IsRead
        {
            get { return _isRead; }
            set
            {
                _isRead = value;
                if (value)
                {
                    _isEdit = false;
                    _isInsert = false;
                }
            }
        }

        public bool IsEdit
        {
            get { return _isEdit; }
            set
            {
                _isEdit = value;
                if (value)
                {
                    _isRead = false;
                    _isInsert = false;
                }
            }
        }

        public bool IsInsert
        {
            get { return _isInsert; }
            set
            {
                _isInsert = value;
                if (value)
                {
                    _isEdit = false;
                    _isRead = false;
                }
            }
        }

        public bool IsNew
        { 
            get { return IsInsert; } 
        }

        public virtual string SelectedValue
        {
            get
            {
                if (GetSelectedValue != null)
                    return GetSelectedValue();
                return IsNew || FormView == null || FormView.SelectedValue == null
                           ? null
                           : Convert.ToString(FormView.SelectedValue);
            }
        }

        public virtual Control FindControl(string id)
        {
            return FormControl.FindControl(id);
        }

        public virtual void ComputeReadOnlyFields(bool executeBeforeLogic)
        {
        }

        public Field this[string fieldName, FieldType fieldType]
        {
            get
            {
                if (_fields.ContainsKey(fieldName))
                    return _fields[fieldName];
                return _fields[fieldName] = new Field(fieldName, FormView, fieldType);
            }
        }

        public Control GetControl(string fieldName, FieldType fieldType)
        {
            var field = this[fieldName, fieldType];
            switch (fieldType)
            {
                case FieldType.Text:
                    return (Control)field.TextBox ?? field.Label;
                case FieldType.Date:
                    return (Control)field.DatePicker ?? field.Label;
                case FieldType.Numeric:
                    return (Control)field.NullableTextBox ?? field.Label;
                case FieldType.DropDownList:
                    return (Control)field.DropDownList ?? field.HyperLink;
                case FieldType.CodeDropDownList:
                    return (Control)field.CodeDropDownList ?? field.HyperLink;
                case FieldType.Lookup:
                    return (Control)field.TextBoxText ?? field.HyperLink;
                case FieldType.Checked:
                    return (Control)field.CheckBox ?? field.Label;
                case FieldType.BoolDropDownList:
                    return (Control)field.DropDownList ?? field.Label;
                case FieldType.File:
                    return (Control)field.FileUpload ?? field.Label;
                default:
                    return null;
            }
        }

        public string GetValue(string fieldName, FieldType fieldType)
        {
            var field = this[fieldName, fieldType];
            switch (fieldType)
            {
                case FieldType.Text:
                    return field.TextBox != null ? field.TextBox.Text : field.Label.Text;
                case FieldType.Date:
                    return field.DatePicker != null ? (field.DatePicker.Date ?? "").ToString() : field.Label.Attributes["value"];
                case FieldType.Numeric:
                    return field.NullableTextBox != null ? field.NullableTextBox.Text : field.Label.Attributes["value"];
                case FieldType.DropDownList:
                    return field.DropDownList != null ? (field.DropDownList.SelectedValue ?? "").ToString() : field.HyperLink.Attributes["value"];
                case FieldType.CodeDropDownList:
                    return field.CodeDropDownList != null ? (field.CodeDropDownList.SelectedValue ?? "") : field.HyperLink.Attributes["value"];
                case FieldType.Lookup:
                    return field.TextBox != null ? field.TextBox.Text : field.HyperLink.Attributes["value"];
                case FieldType.Checked:
                    return field.CheckBox != null ? field.CheckBox.Checked.ToString() : field.Label.Attributes["checked"];
                case FieldType.BoolDropDownList:
                    return field.DropDownList != null ? (field.DropDownList.SelectedValue ?? "").ToString() : field.Label.Attributes["checked"];
                case FieldType.File:
                    return field.HiddenField != null ? field.HiddenField.Value : field.HyperLink.Attributes["value"];
                default:
                    return null;
            }
        }

        public bool IsNull(string fieldName, FieldType fieldType)
        {
            return string.IsNullOrEmpty(GetValue(fieldName, fieldType));
        }

        public class Field
        {
            public Field(string fieldName, FormView formView, FieldType fieldType)
            {
                switch (fieldType)
                {
                    case FieldType.Text:
                        TextBox = (TextBox)formView.FindControl("tb" + fieldName);
                        Label = (Label)formView.FindControl("l" + fieldName);
                        break;
                    case FieldType.Date:
                        DatePicker = (DatePicker)formView.FindControl("dp" + fieldName);
                        Label = (Label)formView.FindControl("l" + fieldName);
                        break;
                    case FieldType.Numeric:
                        NullableTextBox = (NullableTextBox)formView.FindControl("tb" + fieldName);
                        Label = (Label)formView.FindControl("l" + fieldName);
                        break;
                    case FieldType.CodeDropDownList:
                        CodeDropDownList = (CodeDropDownListExt)formView.FindControl("ddl" + fieldName);
                        HyperLink = (HyperLink)formView.FindControl("hl" + fieldName);
                        break;
                    case FieldType.DropDownList:
                        DropDownList = (DropDownListExt)formView.FindControl("ddl" + fieldName);
                        HyperLink = (HyperLink)formView.FindControl("hl" + fieldName);
                        break;
                    case FieldType.BoolDropDownList:
                        DropDownList = (DropDownListExt)formView.FindControl("ddl" + fieldName);
                        Label = (Label)formView.FindControl("l" + fieldName);
                        break;
                    case FieldType.Lookup:
                        TextBox = (TextBox)formView.FindControl("tbID" + fieldName);
                        TextBoxText = (TextBox)formView.FindControl("tbT" + fieldName);
                        ImageButtonBrowse = (ImageButton)formView.FindControl("ibBrowse" + fieldName);
                        ImageButtonDelete = (ImageButton)formView.FindControl("bNull" + fieldName);
                        HyperLinkInfo = (HyperLink)formView.FindControl("hlInfo" + fieldName);
                        HyperLink = (HyperLink)formView.FindControl("hl" + fieldName);
                        TextBoxOtherText = (TextBox)formView.FindControl("tbA" + fieldName);
                        break;
                    case FieldType.Checked:
                        CheckBox = (CheckBox)formView.FindControl("cb" + fieldName);
                        Label = (Label)formView.FindControl("l" + fieldName);
                        break;
                    case FieldType.File:
                        HiddenField = (HiddenField)formView.FindControl("hf" + fieldName);
                        HiddenField = (HiddenField)formView.FindControl("hfOriginal" + fieldName);
                        FileUpload = (FileUpload)formView.FindControl("fu" + fieldName);
                        Label = (Label)formView.FindControl("lFileName" + fieldName);
                        HyperLink = (HyperLink)formView.FindControl("hl" + fieldName);
                        break;
                }
            }

            public Label Label { get; set; }
            public TextBox TextBox { get; set; }
            public CheckBox CheckBox { get; set; }
            public HyperLink HyperLink { get; set; }
            public DropDownListExt DropDownList { get; set; }
            public CodeDropDownListExt CodeDropDownList { get; set; }
            public DatePicker DatePicker { get; set; }
            public NullableTextBox NullableTextBox { get; set; }

            public HyperLink HyperLinkInfo { get; set; }
            public TextBox TextBoxText { get; set; }
            public TextBox TextBoxOtherText { get; set; }
            public ImageButton ImageButtonBrowse { get; set; }
            public ImageButton ImageButtonDelete { get; set; }

            public HiddenField HiddenField { get; set; }
            public HiddenField HiddenFieldOriginal { get; set; }
            public FileUpload FileUpload { get; set; }
        }

        public abstract class BaseField
        {
            public string FieldName { get; set; }
            public BaseControlInfo Info { get; set; }
            public virtual void Clear()
            {
            }
            public abstract Control Control { get; }
        }

        public class LableField : BaseField
        {
            static readonly Label Empty = new Label();
            Label _control;

            public Label Label 
            {
                get 
                {
                    if (_control == null)
                        _control = (Label)Info.FormView.FindControl("l" + FieldName) ?? Empty;
                    return _control == Empty ? null : _control;
                }
            }

            public override Control Control
            {
                get { return Label; }
            }

            public override void Clear()
            {
                base.Clear();
                _control = null;
            }
        }

        public class HyperLinkField : BaseField
        {
            static readonly HyperLink Empty = new HyperLink();
            HyperLink _control;

            public HyperLink HyperLink
            {
                get
                {
                    if (_control == null)
                        _control = (HyperLink)Info.FormView.FindControl("hl" + FieldName) ?? Empty;
                    return _control == Empty ? null : _control;
                }
            }

            public override Control Control
            {
                get { return HyperLink; }
            }

            public override void Clear()
            {
                base.Clear();
                _control = null;
            }
        }

        public class TextField : LableField
        {
            static readonly TextBox Empty = new TextBox();
            TextBox _control;

            public TextBox TextBox
            {
                get
                {
                    if (_control == null)
                        _control = (TextBox)Info.FormView.FindControl("tb" + FieldName) ?? Empty;
                    return _control == Empty ? null : _control;
                }
            }

            public override Control Control
            {
                get { return (Control)TextBox ?? Label; }
            }

            public override void Clear()
            {
                base.Clear();
                _control = null;
            }
        }

        public class DatePickerField : LableField
        {
            static readonly DatePicker Empty = new DatePicker();
            DatePicker _control;

            public DatePicker DatePicker
            {
                get
                {
                    if (_control == null)
                        _control = (DatePicker)Info.FormView.FindControl("dp" + FieldName) ?? Empty;
                    return _control == Empty ? null : _control;
                }
            }

            public override Control Control
            {
                get { return (Control)DatePicker ?? Label; }
            }

            public override void Clear()
            {
                base.Clear();
                _control = null;
            }
        }

        public enum FieldType
        {
            Text,
            Date,
            Numeric,
            DropDownList,
            CodeDropDownList,
            Lookup,
            Checked,
            BoolDropDownList,
            File,
        }
    }
}