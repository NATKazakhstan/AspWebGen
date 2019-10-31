namespace Nat.Web.Controls
{
    using System;

    public interface IColumnFilterStorageChanged
    {
        event EventHandler<EventArgs> ColumnFilterStorageChanged;
    }

    public interface ICheckedFilterCondition
    {
        /// <summary>
        /// Имя условия из CustomConditions, для фильтрации текущего списка полей. Для этого фильтра создается CheckBox.
        /// </summary>
        string CheckedFilterCondition { get; set; }

        /// <summary>
        /// Всплывающая подсказка для CheckBox, создаваемого на основании свойства CheckedFilterCondition.
        /// </summary>
        string CheckedFilterConditionTooltip { get; set; }
    }
}