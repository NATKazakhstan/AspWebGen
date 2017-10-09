using System;
using System.ComponentModel;
using System.Web.UI;

namespace Nat.Web.Controls.SelectValues
{
    [Serializable]
    [ParseChildren(true)]
    [PersistChildren(false)]
    public class GroupFieldItem
    {
        [Localizable(true)]
        public string GroupField { get; set; }
        [Localizable(true)]
        public string GroupFieldFormatString { get; set; }

        /// <summary>
        /// Текст, если значение null или String.Empty
        /// </summary>
        [Localizable(true)]
        public string NullText { get; set; }

        /// <summary>
        /// Сортировка по возврастанию
        /// </summary>
        public bool Ascending { get; set; }
        /// <summary>
        /// Если группа сварачиваемая, то true
        /// </summary>
        public bool Collapsible { get; set; }
        /// <summary>
        /// Можно ли в данной группе выбирать все вложенные
        /// </summary>
        public bool Selectable { get; set; }
        /// <summary>
        /// По значение по умолчанию, свернута или развернута группа
        /// </summary>
        public bool DefaultCollapced { get; set; }

        /// <summary>
        /// Не показывать группу (контент оказывается на уровень выше), если значение null или String.Empty.
        /// </summary>
        public bool HideGroupIfValueNull { get; set; }
    }
}