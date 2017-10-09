using System.Collections.Specialized;
using System.ComponentModel;
using System.Web.UI;
using Nat.Web.Controls.GenerationClasses.BaseJournal;
using Nat.Web.Controls.Properties;

namespace Nat.Web.Controls.GenerationClasses
{
    [DefaultProperty("Checked")]
    [DefaultBindingProperty("Checked")]
    [ValidationProperty("Checked")]
    public class BaseRadioButton : BaseEditControl, IRadioButton, IPostBackDataHandler
    {
        public override object Value
        {
            get { return Checked; }
            set { Checked = ((value ?? "").ToString() == ID || true.Equals(value)); }
        }

        public override string Text
        {
            get { return Checked ? Resources.SYes : Resources.SNo; }
            set { }
        }

        public bool Checked { get; set; }
        public string GroupName { get; set; }
        public bool GroupNameUsUniqueID { get; set; }

        protected override void RenderContents(HtmlTextWriter writer)
        {
            writer.AddAttribute(HtmlTextWriterAttribute.Value, ValueAttribute);
            if (Checked)
                writer.AddAttribute(HtmlTextWriterAttribute.Checked, "checked");
            writer.AddAttribute(HtmlTextWriterAttribute.Id, ((IRenderComponent)this).ClientID);
            var uniqueID = GetUniqueID();
            if (uniqueID != null)
                writer.AddAttribute(HtmlTextWriterAttribute.Name, uniqueID);
            AddAttributes(writer);
            writer.AddAttribute(HtmlTextWriterAttribute.Type, "radio");
            writer.RenderBeginTag(HtmlTextWriterTag.Input);
            writer.RenderEndTag();
        }

        private string GetUniqueID()
        {
            if (GroupNameUsUniqueID)
                return GroupName;

            var uniqueID = ((IRenderComponent) this).UniqueID;
            var lengthName = uniqueID.LastIndexOf(IdSeparator);
            if (lengthName > 0)
            {
                var cell = Parent as BaseJournalCell;
                if (cell != null)
                {
                    uniqueID = uniqueID.Substring(0, lengthName);
                    lengthName = uniqueID.LastIndexOf(IdSeparator);
                    return uniqueID.Substring(0, lengthName + 1) + cell.Column.GetRadioButtonCellID(cell.RenderContext) + IdSeparator + GroupName;
                }

                return uniqueID.Substring(0, lengthName + 1) + GroupName;
            }

            return null;
        }

        protected override void AddMandatoryValidator()
        {
        }

        public bool LoadPostData(string postDataKey, NameValueCollection postCollection)
        {
            var eventArgument = postCollection[GetUniqueID()];
            if ((eventArgument != null) && eventArgument.Equals(ValueAttribute))
            {
                if (!Checked)
                {
                    Checked = true;
                    return true;
                }
            }
            else if (Checked) 
                Checked = false;
            return false;
        }

        protected string ValueAttribute
        {
            get { return ID; }
        }

        public void RaisePostDataChangedEvent()
        {
        }
    }
}