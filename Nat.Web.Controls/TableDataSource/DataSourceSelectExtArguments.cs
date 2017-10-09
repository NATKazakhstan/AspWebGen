using System;

namespace Nat.Web.Controls
{
    public class DataSourceSelectExtArguments
    {
        private bool allowSelectTop = true;
        private int topCount = 15;

        public DataSourceSelectExtArguments() { }


        public DataSourceSelectExtArguments(int topCount)
        {
            TopCount = topCount;
        }

        public DataSourceSelectExtArguments(bool allowSelectTop, int topCount)
        {
            this.allowSelectTop = allowSelectTop;
            this.topCount = topCount;
        }

        public int TopCount
        {
            get { return topCount; }
            set
            {
                if(topCount < 1) throw new ArgumentOutOfRangeException("value", "не может быть меньше 1");
                topCount = value;
            }
        }

        public bool AllowSelectTop
        {
            get { return allowSelectTop; }
            set { allowSelectTop = value; }
        }
    }
}