/*
 * Created by: Sergey.Shpakovsiy
 * Created: 29.01.2009
 * Copyright © JSC New Age Technologies 2009
 */

using System.Web.UI;

namespace Nat.Web.Controls
{
    public interface IAccessControl
    {
       /// <summary>
       /// ѕроверка прав доступа дл€ просмотра контрола
       /// </summary>
       /// <returns>если есть права то true, елси нет то false</returns>
        bool CheckPermit(Page page);
    }
}