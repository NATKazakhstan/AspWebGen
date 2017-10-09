namespace Nat.Web.ReportManager
{
    using System;
    using System.Globalization;

    public static class DateTimeHelper
    {
        // Year
        public static DateTime GetFirstDayYear(DateTime dateIn)
        {
            return dateIn.AddDays(1 - dateIn.DayOfYear);
        }

        public static DateTime GetLastDayYear(DateTime dateIn)
        {
            return
                DateTime.Parse(
                    string.Format(
                        "{0}.{1}.{2} {3}",
                        "31",
                        "12",
                        dateIn.Year,
                        dateIn.ToLongTimeString()),
                    CultureInfo.CreateSpecificCulture("ru-RU"));
        }

        // HalfYear
        public static DateTime GetFirstDayHalfYear(DateTime dateIn)
        {
            return dateIn.Month <= 6
                       ? GetFirstDayYear(dateIn)
                       : DateTime.Parse(
                           string.Format(
                               "{0}.{1}.{2} {3}",
                               "01",
                               "07",
                               dateIn.Year,
                               dateIn.ToLongTimeString()),
                           CultureInfo.CreateSpecificCulture("ru-RU"));
        }

        public static DateTime GetLastDayHalfYear(DateTime dateIn)
        {
            return dateIn.Month > 6
                       ? GetLastDayYear(dateIn)
                       : GetLastDayMonths(
                           DateTime.Parse(
                               string.Format(
                                   "{0}.{1}.{2} {3}",
                                   "01",
                                   "06",
                                   dateIn.Year,
                                   dateIn.ToLongTimeString()),
                               CultureInfo.CreateSpecificCulture("ru-RU")));
        }

        // Months
        public static DateTime GetFirstDayMonths(DateTime dateIn)
        {
            return dateIn.AddDays(1 - dateIn.Day);
        }

        public static DateTime GetLastDayMonths(DateTime dateIn)
        {
            return dateIn.AddMonths(1).AddDays(-dateIn.Day);
        }

        // Quarter
        public static DateTime GetFirstDayQuarter(DateTime dateIn)
        {
            return
                DateTime.Parse(
                    string.Format(
                        "{0}.{1}.{2} {3}",
                        "01",
                        (dateIn.Month - 1) / 3 * 3 + 1,
                        dateIn.Year,
                        dateIn.ToLongTimeString()),
                    CultureInfo.CreateSpecificCulture("ru-RU"));
        }

        public static DateTime GetLastDayQuarter(DateTime dateIn)
        {
            return
                DateTime.Parse(
                    string.Format(
                        "{0}.{1}.{2} {3}",
                        "01",
                        (dateIn.Month - 1) / 3 * 3 + 1,
                        dateIn.Year,
                        dateIn.ToLongTimeString()),
                    CultureInfo.CreateSpecificCulture("ru-RU")).AddMonths(3).AddDays(-1);
        }

        // Week
        public static DateTime GetFirstDayWeek(DateTime dateIn)
        {
            return dateIn.AddDays(1 - Convert.ToInt32(dateIn.DayOfWeek));
        }

        public static DateTime GetLastDayWeek(DateTime dateIn)
        {
            return dateIn.AddDays(7 - Convert.ToInt32(dateIn.DayOfWeek));
        }
    }
}