using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.UI;
using System.Web.UI.WebControls;
using Nat.Web.Controls.GenerationClasses.HierarchyFields;

namespace Nat.Web.Controls.GenerationClasses.BaseJournal
{
    using System.Collections;
    using System.ComponentModel;
    using System.Drawing;
    using System.Web;

    public abstract class BaseJournalCell : WebControl, INamingContainer
    {
        protected BaseJournalCell()
        {
            ColSpan = 1;
            RowSpan = 1;
            ValidatorProperties = new List<ValidatorProperties>();
        }

        public object Item { get; set; }
        public BaseColumn Column { get; set; }
        public bool IsEmpty { get; set; }
        public int ColSpan { get; set; }
        public int RowSpan { get; set; }
        public int RowIndex { get; set; }
        public string CrossColumnId { get; set; }
        public ColumnHierarchy ColumnHierarchy { get; set; }
        public abstract string GetValue();
        public abstract bool IsVertical();
        public abstract string GetCellKey();
        protected abstract object GetColumnValue();
        protected abstract string GetColumnName();
        public string RowNumber { get; set; }
        public RenderContext RenderContext { get; set; }
        protected List<ValidatorProperties> ValidatorProperties { get; private set; }

        private Control _readColtrol;
        public Control ReadColtrol
        {
            get { return _readColtrol; }
            set
            {
                if (value != null)
                    Controls.Add(value);
                if (_readColtrol != null)
                    Controls.Remove(_readColtrol);
                _readColtrol = value;
            }
        }

        private Control _editColtrol;
        public Control EditColtrol
        {
            get { return _editColtrol; }
            set
            {
                if (_editColtrol != null)
                    Controls.Remove(_editColtrol);
                if (value != null)
                {
                    InitEditControlValues(value);
                    Controls.Add(value);
                    InitEditControlIDs(value);
                }
                _editColtrol = value;
            }
        }

        protected virtual void InitEditControlValues(Control editControl)
        {
            var renderComponent = editControl as IRenderComponent;
            if (renderComponent != null)
            {
                renderComponent.ClientID = RenderContext.ClientID;
                renderComponent.UniqueID = RenderContext.UniqueID;
                renderComponent.ValidationGroup = RenderContext.ValidationGroup;
                renderComponent.Mandatory |= Column.EditMandatory;
                renderComponent.Value = GetColumnValue();
                renderComponent.Text = GetColumnName();
            }
        }

        protected virtual void InitEditControlIDs(Control editControl)
        {
            var renderComponent = editControl as IRenderComponent;
            if (renderComponent != null)
            {
                RenderContext.EditClientID = renderComponent.ClientID;
                RenderContext.UniqueID = renderComponent.UniqueID;
            }
            else
            {
                RenderContext.EditClientID = editControl.ClientID;
                RenderContext.UniqueID = editControl.UniqueID;
            }
        }

        public override bool EnableViewState
        {
            get
            {
                return false;
            }
        }

        protected override void OnPreRender(EventArgs e)
        {
            base.OnPreRender(e);
            foreach (var item in ValidatorProperties)
                item.RegisterClientValidator(Page);
        }

        public virtual void AddValidators(IEnumerable<ValidatorProperties> validators)
        {
            var editComponent = EditColtrol as IRenderComponent;
            if (editComponent != null)
                editComponent.AddValidators(validators);
            else
            {
                foreach (var validator in validators)
                {
                    ValidatorProperties.Add(validator);
                    AddClientValidator(validator, ValidatorProperties.Count); 
                }
            }
        }

        public virtual void AddValidator(ValidatorProperties validator)
        {
            var editComponent = EditColtrol as IRenderComponent;
            if (editComponent != null)
                editComponent.AddValidator(validator);
            else
            {
                ValidatorProperties.Add(validator);
                AddClientValidator(validator, ValidatorProperties.Count);
            }
        }

        protected void AddClientValidator(ValidatorProperties validator, int validatorIndex)
        {
            var sb = new StringBuilder();
            validator.CreateClientValidator(Page, sb,
                                             ID + "_" + validatorIndex,
                                             RenderContext.EditClientID,
                                             ValidatorDisplay.Dynamic,
                                             RenderContext.ValidationGroup,
                                             GetColumnValue());
            Controls.Add(new Literal { Text = sb.ToString() });
        }

        public bool ValidateValue(string value)
        {
            var isValid = true;
            foreach (var validator in ValidatorProperties)
                if (!validator.ValidateValue(value))
                {
                    RenderContext.AddErrorMessage(validator.ErrorMessageInSummary);
                    isValid = false;
                }
            var editComponent = EditColtrol as IRenderComponent;
            if (editComponent != null && !editComponent.ValidateValue(value, RenderContext))
                isValid = false;
            return isValid;
        }
    }

    public class BaseJournalCell<TRow> : BaseJournalCell
        where TRow : BaseRow
    {
        public BaseJournalRow<TRow> Row { get; set; }

        public override void RenderControl(HtmlTextWriter writer)
        {
            if (!Visible)
                return;

            writer.WriteLine();
            if (ColSpan > 1)
                writer.AddAttribute(HtmlTextWriterAttribute.Colspan, ColSpan.ToString());
            if (RowSpan > 1)
                writer.AddAttribute(HtmlTextWriterAttribute.Rowspan, RowSpan.ToString());
            if (IsVertical())
            {
                writer.AddStyleAttribute("writing-mode", "tb-rl");
                writer.AddStyleAttribute("filter", "flipv fliph");
                writer.AddStyleAttribute("text-align", "center");
                writer.AddAttribute(HtmlTextWriterAttribute.Align, "center");
                writer.AddAttribute(HtmlTextWriterAttribute.Valign, "center");
            }
            else
            {
                var rowProps = Row?.RowKey != null && Row.JournalControl.RowsPropertiesDic.ContainsKey(Row.RowKey)
                                ? Row.JournalControl.RowsPropertiesDic[Row.RowKey]
                                : null;
                var cellKey = GetCellKey();
                var cellProps = Row?.JournalControl.CellsPropertiesDic.ContainsKey(cellKey) ?? false
                                    ? Row.JournalControl.CellsPropertiesDic[cellKey]
                                    : null;
                if (cellProps?.HAligment != null)
                    writer.AddStyleAttribute(HtmlTextWriterStyle.TextAlign, cellProps.HAligment.ToString().ToLower());
                else if (rowProps?.HAligment != null)
                    writer.AddStyleAttribute(HtmlTextWriterStyle.TextAlign, rowProps.HAligment.ToString().ToLower());
                else if (Column.ColumnType == ColumnType.Numeric && (Row == null || !Row.JournalControl.DetailsRender))
                    writer.AddStyleAttribute(HtmlTextWriterStyle.TextAlign, "center");
                else
                    writer.AddStyleAttribute(HtmlTextWriterStyle.TextAlign, "left");
                if ((Column.GroupType & GroupType.Left) == GroupType.Left)
                    writer.AddAttribute(HtmlTextWriterAttribute.Valign, "center");
            }
            RenderCellProperties(writer);
            writer.RenderBeginTag(HtmlTextWriterTag.Td);
            if (!IsEmpty || RenderContext.AllowEdit && RenderContext.Journal.AllowEditEmptyCell)
            {
                if ((Column.GroupType & GroupType.Left) != GroupType.Left)
                {
                    for (int i = 0; i < Column.Tab; i++)
                        writer.Write("&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;");
                }

                Column.RowIndex = RowIndex;
                if(RenderContext != null && RenderContext.AllowEdit)
                {
                    RenderCellEditContent(writer);
                }
                else if (string.IsNullOrEmpty(Column.HideIfValueEquals) ||
                    !Column.HideIfValueEquals.Equals(Column.GetName(RenderContext) ?? string.Empty))
                {
                    RenderCellContent(writer);
                }
                else
                    writer.Write("&nbsp;");
                RenderChildren(writer);
            }
            else
                writer.Write(Column.GetEmptyCell(RenderContext));

            //RenderDebugInformation(writer);
            
            writer.RenderEndTag();
        }

        private void RenderDebugInformation(HtmlTextWriter writer)
        {
            writer.AddStyleAttribute(HtmlTextWriterStyle.Display, "none");
            writer.RenderBeginTag(HtmlTextWriterTag.Div);

            var properties = TypeDescriptor.GetProperties(RenderContext);
            RederValue(writer, properties["GroupValues"]);
            RederValue(writer, properties["TotalGroupValues"]);
            RederValue(writer, properties["CrossDataItemKey"]);
            writer.RenderEndTag();            
        }

        private void RederValue(HtmlTextWriter writer, PropertyDescriptor property)
        {
            RederValue(writer, property.Name, property.GetValue(RenderContext));
        }

        private void RederValue(HtmlTextWriter writer, string propertyName, object value)
        {
            var list = value as IEnumerable;
            if (list != null)
                value = string.Join(",", list.OfType<object>().Select(r => r.ToString()).ToArray());

            writer.RenderBeginTag(HtmlTextWriterTag.Div);
            writer.Write(propertyName);
            writer.Write(": ");
            writer.Write(value);
            writer.RenderEndTag();
        }

        protected override void RenderChildren(HtmlTextWriter writer)
        {
            if (ReadColtrol == null && EditColtrol == null)
                base.RenderChildren(writer);
            else
                foreach (Control control in Controls)
                {
                    if (control != ReadColtrol && control != EditColtrol)
                        control.RenderControl(writer);
                }
        }

        private void RenderCellEditContent(HtmlTextWriter writer)
        {
            if (EditColtrol != null)
            {
                var component = EditColtrol as IRenderComponent;
                if (component != null)
                    component.Enabled = Enabled;
                EditColtrol.RenderControl(writer);
                return;
            }
            var value = GetColumnValue();
            var text = GetColumnName();
            if (Column.CustomRenderEditControl(writer, this, RenderContext, value, text))
                return;

            switch (Column.TypeCell)
            {
                case BaseJournalTypeCell.Label:
                    if (value != null)
                        writer.AddAttribute(HtmlTextWriterAttribute.Value, value.ToString());
                    writer.AddAttribute(HtmlTextWriterAttribute.Type, "input");
                    writer.AddAttribute(HtmlTextWriterAttribute.Id, ClientID);
                    writer.AddAttribute(HtmlTextWriterAttribute.Name, UniqueID);
                    if (!Enabled)
                        writer.AddAttribute(HtmlTextWriterAttribute.Disabled, "disabled");
                    writer.RenderBeginTag(HtmlTextWriterTag.Input);
                    writer.RenderEndTag();
                    break;
                case BaseJournalTypeCell.HyperLink:
                    RenderCellContent(writer);
                    break;
                case BaseJournalTypeCell.Custom:
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private void RenderCellProperties(HtmlTextWriter writer)
        {
            var bcolorDraw = RenderContext.TotalGroupValues != null ? null : RenderContext?.ConditionalFormatting?.GetBackgroundColor(GetColumnValue());
            var bcolor = bcolorDraw == null ? null : ColorTranslator.ToHtml(bcolorDraw.Value);
            string cellKey = GetCellKey();
            writer.AddAttribute("cellKey", cellKey);
            if (Row.JournalControl.CellsPropertiesDic.ContainsKey(cellKey))
            {
                var props = Row.JournalControl.CellsPropertiesDic[cellKey];
                if (!string.IsNullOrEmpty(bcolor ?? props.BColor))
                    writer.AddStyleAttribute(HtmlTextWriterStyle.BackgroundColor, bcolor ?? props.BColor);
                else if (ColumnHierarchy != null && !string.IsNullOrEmpty(ColumnHierarchy.BColor) && ColSpan <= 1)
                    writer.AddStyleAttribute(HtmlTextWriterStyle.BackgroundColor, ColumnHierarchy.BColor);
                if (!string.IsNullOrEmpty(props.PColor))
                    writer.AddStyleAttribute(HtmlTextWriterStyle.Color, props.PColor);
                else if (ColumnHierarchy != null && !string.IsNullOrEmpty(ColumnHierarchy.PColor) && ColSpan <= 1)
                    writer.AddStyleAttribute(HtmlTextWriterStyle.Color, ColumnHierarchy.PColor);
            }
            else if (ColumnHierarchy != null && ColSpan <= 1)
            {
                if (!string.IsNullOrEmpty(bcolor ?? ColumnHierarchy.BColor))
                    writer.AddStyleAttribute(HtmlTextWriterStyle.BackgroundColor, bcolor ?? ColumnHierarchy.BColor);
                if (!string.IsNullOrEmpty(ColumnHierarchy.PColor))
                    writer.AddStyleAttribute(HtmlTextWriterStyle.Color, ColumnHierarchy.PColor);
            }
            else if (bcolor != null)
            {
                writer.AddStyleAttribute(HtmlTextWriterStyle.BackgroundColor, bcolor);
            }
        }

        public bool IsBold()
        {
            return !IsEmpty && (Column.UsingInGroup || RenderContext.TotalGroupValues != null);
        }

        public override bool IsVertical()
        {
            return !IsEmpty && RenderContext.IsVertical();
        }

        public override string GetCellKey()
        {
            var sb = new StringBuilder();
            sb.Append(Row.RowKey);
            sb.Append("__");
            sb.Append(Column.ColumnName);
            if (CrossColumnId != null)
            {
                sb.Append("_cc_");
                sb.Append(CrossColumnId);
            }

            if (RenderContext.CrossDataItemKey?.Count > 0)
            {
                sb.Append("_ck_");
                sb.Append(RenderContext.CrossDataItemKey);
            }

            if (RenderContext.RowIndex > 0)
            {
                sb.Append("_i_");
                sb.Append(RenderContext.RowIndex);
            }

            return sb.ToString();
        }

        private void RenderCellContent(HtmlTextWriter writer)
        {
            if (ReadColtrol != null)
            {
                ReadColtrol.RenderControl(writer);
                return;
            }

            if (Column.CustomRenderInCellBeforeText != null)
                Column.CustomRenderInCellBeforeText(this, writer);

            switch (Column.TypeCell)
            {
                case BaseJournalTypeCell.Label:
                    RenderLabel(writer);
                    break;
                case BaseJournalTypeCell.HyperLink:
                    if (RenderContext.TotalGroupValues != null)
                        RenderLabel(writer);
                    else
                        RenderHyperLink(writer);
                    break;
                case BaseJournalTypeCell.Custom:
                    Column.GetContent(Row.DataItem, Item, writer, CrossColumnId, RenderContext.TotalGroupValues);
                    break;
            }

            if (Column.CustomRenderInCellAfterText != null)
                Column.CustomRenderInCellAfterText(this, writer);
        }

        private void RenderHyperLink(HtmlTextWriter writer)
        {
            var refValue = GetColumnValue();
            if (refValue == null)
            {
                RenderNullText(writer);
                return;
            }
            var href = Column.GetHyperLink(RenderContext);
            if (string.IsNullOrEmpty(href))
                RenderLabel(writer);
            else
            {
                writer.AddAttribute(HtmlTextWriterAttribute.Href, href);
                writer.AddAttribute(HtmlTextWriterAttribute.Target, RenderContext.Column.Target ?? "_blank");
                writer.RenderBeginTag(HtmlTextWriterTag.A);
                var text = GetColumnName();
                if (!Column.DisableHtmlEncode) 
                    text = HttpUtility.HtmlEncode(text);
                writer.Write(text.Replace("\r\n", "<br/>"));
                writer.RenderEndTag();
            }
        }

        private void RenderNullText(HtmlTextWriter writer)
        {
            if (string.IsNullOrEmpty(Column.NullText) || RenderContext.TotalGroupValues != null)
                writer.Write("&nbsp;");
            else
                writer.Write(Column.NullText);
        }

        private void RenderLabel(HtmlTextWriter writer)
        {
            if (string.IsNullOrEmpty(Column.Format) || Column.Format == "{0}")
            {
                var text = GetColumnName();
                if (string.IsNullOrEmpty(text))
                    RenderNullText(writer);
                else
                {
                    if (!Column.DisableHtmlEncode)
                        text = HttpUtility.HtmlEncode(text);
                    writer.Write(text.Replace("\r\n", "<br/>"));
                }
            }
            else
            {
                if (RowNumber != null && !Column.RowNumberRight)
                    writer.Write(RowNumber);

                var value = GetColumnValue();
                if (value == null)
                    RenderNullText(writer);
                else if (!Column.DisableHtmlEncode && value is string)
                    writer.Write(Column.Format, HttpUtility.HtmlEncode((string)value));
                else
                    writer.Write(Column.Format, value);

                if (RowNumber != null && Column.RowNumberRight)
                    writer.Write(RowNumber);
            }
        }

        protected override object GetColumnValue()
        {
            return Column.GetValue(RenderContext);
        }

        protected override string GetColumnName()
        {
            return (Column.RowNumberRight ? string.Empty : RowNumber)
                   + Column.GetName(RenderContext)
                   + (Column.RowNumberRight ? RowNumber : string.Empty);
        }

        public override string GetValue()
        {
            var sb = new StringBuilder();
            if ((Column.GroupType & GroupType.Left) != GroupType.Left)
                sb.Append('\t', Column.Tab);
            Column.RowIndex = RowIndex;
            if (string.IsNullOrEmpty(Column.HideIfValueEquals) ||
                !Column.HideIfValueEquals.Equals(Column.GetName(RenderContext) ?? string.Empty))
            {
                string text;
                switch (Column.TypeCell)
                {
                    case BaseJournalTypeCell.Label:
                        if (string.IsNullOrEmpty(Column.Format) || Column.Format == "{0}")
                        {
                            text = GetColumnName();
                            if (string.IsNullOrEmpty(text) && RenderContext.TotalGroupValues == null) 
                                sb.Append(Column.NullText);
                            else
                                sb.Append(text);
                        }
                        else
                        {
                            if (RowNumber != null)
                                sb.Append(RowNumber);
                            var value = GetColumnValue();
                            if (value == null && !string.IsNullOrEmpty(Column.NullText) && RenderContext.TotalGroupValues == null)
                                sb.Append(Column.NullText);
                            else
                                sb.AppendFormat(Column.Format, value);
                        }
                        break;
                    case BaseJournalTypeCell.Custom:
                    case BaseJournalTypeCell.HyperLink:
                        text = GetColumnName();
                        if (string.IsNullOrEmpty(text) && !string.IsNullOrEmpty(Column.NullText) && RenderContext.TotalGroupValues == null)
                            sb.Append(Column.NullText);
                        else
                            sb.Append(text);
                        break;
                    default:
                        break;
                }
            }
            return sb.ToString();
        }
    }
}