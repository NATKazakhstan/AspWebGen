/*
 * Created by: Sergey V. Shpakovskiy
 * Created: 8 ������ 2008 �.
 * Copyright � JSC New Age Technologies 2008
 */

using System.Web;
using Nat.Web.Controls;

namespace Nat.Web.Tools
{
    public interface IFileManager : IAccessControl
    {
        void DownloadFile(long ID, string fieldName, HttpResponse Response);
    }
}