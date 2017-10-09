/*
 * Created by: Sergey V. Shpakovskiy
 * Created: 10 ������ 2009 �.
 * Copyright � JSC New Age Technologies 2009
 */

using System.Collections.Generic;

namespace Nat.Web.Controls.GenerationClasses
{
    public interface IAdditionalFields
    {
        /// <summary>
        /// �������� ���� �� ��������� ������.
        /// </summary>
        /// <returns></returns>
        bool CheckPermit();

        /// <summary>
        /// ��������� 
        /// </summary>
        /// <param name="idValue">�������� ����� ������� ������</param>
        /// <param name="value">�������� ����</param>
        /// <param name="nameOfColumn">������������ ������ ��� ������� �������������� �������� �������������� �����</param>
        /// <param name="isSecond">���� false ������ ������� ����� �����, ����� �������� ��������� �� �����</param>
        /// <param name="isKz">�� ����� ����� ������������� ������</param>
        /// <return>������ �������� �������������� �����</return></returns>
        IEnumerable<string> GetNameOfValue(long? idValue, string value, string nameOfColumn, bool isSecond, bool isKz);
    }
}