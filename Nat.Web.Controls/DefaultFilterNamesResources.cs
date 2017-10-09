namespace Nat.Web.Controls
{
    using System.Collections.Generic;
    using System.Web.UI.WebControls;

    using Nat.Web.Controls.Properties;
    using Nat.Web.Tools.Initialization;

    public class DefaultFilterNamesResources : IFilterNamesResources
    {
        public string SSelectCondition
        {
            get
            {
                return Resources.SSelectCondition;
            }
        }

        public string SIsFilled
        {
            get
            {
                return Resources.SIsFilled;
            }
        }

        public string SIsNotFilled
        {
            get
            {
                return Resources.SIsNotFilled;
            }
        }

        public string SEquals
        {
            get
            {
                return Resources.SEquals;
            }
        }

        public string SNotEquals
        {
            get
            {
                return Resources.SNotEquals;
            }
        }

        public string SMore
        {
            get
            {
                return Resources.SMore;
            }
        }

        public string SLess
        {
            get
            {
                return Resources.SLess;
            }
        }

        public string SBetween
        {
            get
            {
                return Resources.SBetween;
            }
        }

        public string SDateMore
        {
            get
            {
                return Resources.SDateMore;
            }
        }

        public string SDateLess
        {
            get
            {
                return Resources.SDateLess;
            }
        }

        public string SDateBetween
        {
            get
            {
                return Resources.SDateBetween;
            }
        }

        public string SStartsWith
        {
            get
            {
                return Resources.SStartsWith;
            }
        }

        public string SEndsWith
        {
            get
            {
                return Resources.SEndsWith;
            }
        }

        public string SContains
        {
            get
            {
                return Resources.SContains;
            }
        }

        public string SContainsWords
        {
            get
            {
                return Resources.SContainsWords;
            }
        }

        public string SNotContains
        {
            get
            {
                return Resources.SNotContains;
            }
        }

        public string SNotContainsWords
        {
            get
            {
                return Resources.SNotContainsWords;
            }
        }

        public string SContainsAnyWord
        {
            get
            {
                return Resources.SContainsAnyWord;
            }
        }

        public string SDaysAgoAndMore
        {
            get
            {
                return Resources.SDaysAgoAndMore;
            }
        }

        public string SDaysLeftAndMore
        {
            get
            {
                return Resources.SDaysLeftAndMore;
            }
        }


        public string SLengthMore
        {
            get
            {
                return Resources.SLengthMore;
            }
        }

        public string SLengthLess
        {
            get
            {
                return Resources.SLengthLess;
            }
        }


        public string SToDay
        {
            get
            {
                return Resources.SToDay;
            }
        }

        public Unit SelectFilterTypeWidth
        {
            get
            {
                return new Unit("100%");
            }
        }

        public string SFilterByPeriod
        {
            get
            {
                return Resources.SFilterByPeriod;
            }
        }

        public IEnumerable<KeyValuePair<object, object>> ReferencesFilterTypes
        {
            get
            {
                return null;
            }
        }

        public IEnumerable<KeyValuePair<object, object>> NotMadatoryFilterTypes
        {
            get
            {
                return null;
            }
        }

        public IEnumerable<KeyValuePair<object, object>> NumericFilterTypes
        {
            get
            {
                return null;
            }
        }

        public IEnumerable<KeyValuePair<object, object>> DatetimeFilterTypes
        {
            get
            {
                return null;
            }
        }

        public IEnumerable<KeyValuePair<object, object>> TextFilterTypes
        {
            get
            {
                return null;
            }
        }

        public IEnumerable<KeyValuePair<object, object>> ReferenceFilterTypes
        {
            get
            {
                return null;
            }
        }

        public IEnumerable<KeyValuePair<object, object>> ReferenceListWithTextFilterForCodeTypes
        {
            get
            {
                return null;
            }
        }
    }
}