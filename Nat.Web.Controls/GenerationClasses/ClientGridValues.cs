/*
 * Created by: Sergey V. Shpakovskiy
 * Created: 27 декабря 2008 г.
 * Copyright © JSC New Age Technologies 2008
 */

using System;

namespace Nat.Web.Controls.GenerationClasses
{
    public class ClientGridValues
    {
        public long ID 
        { 
            get
            {
                return Convert.ToInt64(StringID);
            }
        }
        public string Value { get; set; }
        public bool Checked { get; set; }
        public string Key { get; set; }
        public string StringID { get; set; }
    }
}