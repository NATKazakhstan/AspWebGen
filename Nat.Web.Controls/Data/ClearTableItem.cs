using System.Collections.Generic;
using System.ComponentModel;
using Nat.Web.Tools;

namespace Nat.Web.Controls
{
    public class ClearTableItem : TableItem, IComparer<ClearTableItem>
    {
        private ClearType clearType = ClearType.Not;

        [DefaultValue(ClearType.Not)]
        public ClearType ClearType
        {
            get { return clearType; }
            set { clearType = value; }
        }

        public int Compare(ClearTableItem x, ClearTableItem y)
        {
            if (x == null)
            {
                if (y == null) return 0;
                return -1;
            }
            if (y == null) return 1;
            return string.Compare(x.TableName, y.TableName);
        }

    }
}