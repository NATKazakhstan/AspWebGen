namespace Nat.Web.Controls.ExtNet
{
    using Ext.Net;

    public static class TextFieldExtender
    {
        /// <summary>
        /// Шаблон формирования ComboBox с многострочным полем ввода.
        /// </summary>
        public const string MultilineComboboxTemplate = @"<div class=""{hiddenDataCls}"" role=""presentation""></div>
<textarea id=""{id}"" class=""{fieldCls} {typeCls} {editableCls}"" name=""{name}"" <tpl if=""readOnly""> readonly=""readonly""</tpl><tpl if=""disabled""> disabled=""disabled""</tpl><tpl if=""tabIdx""> tabIndex=""{tabIdx}""</tpl><tpl if=""fieldStyle""> style=""{fieldStyle}""</tpl> ><tpl if=""value"">{[Ext.util.Format.htmlEncode(values.value)]}</tpl></textarea>";

        /// <summary>
        /// Сделать ComboBox с многострочным полем ввода и высотой height;
        /// </summary>
        /// <param name="combo">ComboBox, который нужно сделать с многострочным вводом.</param>
        /// <param name="height">Настройка высоты ComboBox.</param>
        /// <param name="width">Настройка ширины ComboBox.</param>
        public static void Multiline(this TextField combo, int height, int? width = null)
        {
            combo.Height = height;
            if (width != null) combo.InputWidth = (int) width;
            combo.FieldSubTpl = new XTemplate
            {
                Html = MultilineComboboxTemplate,
            };
        }
    }
}