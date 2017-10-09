/*
 * Created by: Denis M. Silkov
 * Created: 14 �������� 2007 �.
 */

using System;

namespace Nat.Web.Controls
{
    /// <summary>
    /// ��� ���������� TableDataSource.
    /// </summary>
    [Serializable]
    public enum TableDataSourceFillType
    {
        /// <summary>
        /// ���������, ���� ������ ���������� ��������� �������.
        /// </summary>
        ParametersNotChanged,
        /// <summary>
        /// ������ ���������.
        /// </summary>
        Always,
        /// <summary>
        /// ������� �� ���������.
        /// </summary>
        Never
    }
}