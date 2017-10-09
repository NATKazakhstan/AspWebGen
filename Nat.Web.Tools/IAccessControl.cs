/*
 * Created by: Sergey.Shpakovsiy
 * Created: 29.01.2009
 * Copyright � JSC New Age Technologies 2009
 */

using System.Web.UI;

namespace Nat.Web.Controls
{
    public interface IAccessControl
    {
       /// <summary>
       /// �������� ���� ������� ��� ��������� ��������
       /// </summary>
       /// <returns>���� ���� ����� �� true, ���� ��� �� false</returns>
        bool CheckPermit(Page page);
    }
}