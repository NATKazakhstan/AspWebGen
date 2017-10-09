namespace Nat.Web.Tools.Initialization
{
    using System.Collections.Generic;
    using System.Web.UI.WebControls;

    public interface IFilterNamesResources
    {
        string SSelectCondition { get; }

        string SIsFilled { get; }

        string SIsNotFilled { get; }

        string SEquals { get; }

        string SNotEquals { get; }

        string SMore { get; }

        string SLess { get; }

        string SBetween { get; }

        string SDateMore { get; }

        string SDateLess { get; }

        string SDateBetween { get; }

        string SStartsWith { get; }

        string SEndsWith { get; }

        string SContains { get; }

        string SContainsWords { get; }

        string SNotContains { get; }

        string SNotContainsWords { get; }
        
        string SLengthMore { get; }
        
        string SLengthLess { get; }

        string SContainsAnyWord { get; }

        string SDaysAgoAndMore { get; }

        string SDaysLeftAndMore { get; }

        string SToDay { get; }

        Unit SelectFilterTypeWidth { get; }

        string SFilterByPeriod { get; }

        IEnumerable<KeyValuePair<object, object>> ReferencesFilterTypes { get; }

        IEnumerable<KeyValuePair<object, object>> NotMadatoryFilterTypes { get; }

        IEnumerable<KeyValuePair<object, object>> NumericFilterTypes { get; }

        IEnumerable<KeyValuePair<object, object>> DatetimeFilterTypes { get; }

        IEnumerable<KeyValuePair<object, object>> TextFilterTypes { get; }

        IEnumerable<KeyValuePair<object, object>> ReferenceFilterTypes { get; }

        IEnumerable<KeyValuePair<object, object>> ReferenceListWithTextFilterForCodeTypes { get; }
    }
}