/*
 * Created by: Sergey V. Shpakovskiy
 * Created: 18 ������ 2008 �.
 * Copyright � JSC New Age Technologies 2008
 */

namespace Nat.Web.Tools
{
    /// <summary>
    /// �������� ������������ ��������� �������� PostBack
    /// </summary>
    public interface ISupportPostBack
    {
        bool PostBack { get; set; }
    }
}