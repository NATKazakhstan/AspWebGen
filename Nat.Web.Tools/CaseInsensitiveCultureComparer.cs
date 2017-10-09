using System.Collections;
using System.Collections.Generic;
using System.Globalization;

namespace Nat.Web.Tools
{
    public class CaseInsensitiveCultureComparer : EqualityComparer<string>
    {
        public CaseInsensitiveComparer MyComparer;

        public CaseInsensitiveCultureComparer()
        {
            MyComparer = CaseInsensitiveComparer.DefaultInvariant;
        }

        public CaseInsensitiveCultureComparer(CultureInfo myCulture)
        {
            MyComparer = new CaseInsensitiveComparer(myCulture);
        }

        public override bool Equals(string x, string y)
        {
            if (MyComparer.Compare(x, y) == 0)
                return true;
            return false;
        }

        public override int GetHashCode(string obj)
        {
            return obj.ToLower().GetHashCode();
        }
    }
}