/*
 * Created by: Sergey V. Shpakovskiy
 * Created: 27 ������ 2009 �.
 * Copyright � JSC New Age Technologies 2009
 */

using System.Collections.Generic;
using System.ComponentModel;

namespace Nat.Web.Controls.GenerationClasses
{
    public class AddSaveLinkArgs : CancelEventArgs
    {
        public AddSaveLinkArgs()
        {
            Fields = new List<string>();
        }

        public AddSaveLinkArgs(IEnumerable<string> fields)
        {
            Fields = new List<string>(fields);
        }

        /// <summary>
        /// ���� �������, ������� ��������� ���������.
        /// </summary>
        public IList<string> Fields { get; private set; }
    }
}