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
        /// ��� ������� �� CustomConditions, ��� ���������� �������� ������ �����. ��� ����� ������� ��������� CheckBox.
        /// </summary>
        string CheckedFilterCondition { get; set; }

        /// <summary>
        /// ����������� ��������� ��� CheckBox, ������������ �� ��������� �������� CheckedFilterCondition.
        /// </summary>
        string CheckedFilterConditionTooltip { get; set; }
    }
}