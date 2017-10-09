using System;

namespace Nat.Web.Controls
{
    /// <summary>
    /// ����� �������� ������.
    /// </summary>
    [Serializable]    
    public enum TableDataSourceDataMode
    {
        /// <summary>
        /// ��� ������� ������, ������ ������������ ��� ������.
        /// </summary>
        All,
        /// <summary>
        /// ��� ������� ������, ������������ ������ ������� ������.
        /// </summary>
        OnlyCurrent,
        /// <summary>
        /// ��� ������� ������, ������������ ������� ������, ���� �� ���, �� ��� ������.
        /// </summary>
        CurrentOrAll
    }
}