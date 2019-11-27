namespace Nat.Web.Controls.ExtNet
{
    using Ext.Net;

    public static class CheckBoxFieldExtender
    {
        /// <summary>
        /// Меняет местами CheckBox и Label
        /// </summary>
        /// <param name="checkBox"></param>
        public static void Reverse(this Checkbox checkBox)
        {
            checkBox.HideLabel = true;
            checkBox.LabelSeparator = "";
            checkBox.BoxLabel = checkBox.FieldLabel;
            checkBox.BoxLabelStyle = "width: calc(100% - 20px); padding-left: 188px;";
        }
    }
}