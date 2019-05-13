/*
 * Created by: Roman V. Kurbangaliev
 * Created: 28 мая 2008 г.
 * Copyright © JSC New Age Technologies 2008
 */
using System;
using System.Data;
using System.Globalization;
using System.Threading;
using System.Web.UI;
using Nat.Tools.QueryGeneration;
using Nat.Tools.Specific;
using Nat.Web.Tools.Initialization;
using System.Web;

namespace Nat.Web.Tools
{
    public class LocalizationHelper
    {
        /// <summary>
        /// Изменить колонкам Expression, в соответствии с текущей культурой
        /// </summary>
        /// <param name="table">таблица с колонками</param>
        public static void ChangeColumnsExpression(DataTable table)
        {
            WebInitializer.Initialize();
            var key = "userCulture";

            var userCulture = CultureInfo.CurrentUICulture.Name;

            string serverCulture = null;
            if (table.ExtendedProperties.Contains(key) && table.ExtendedProperties[key] != null)
                serverCulture = table.ExtendedProperties[key].ToString();

            if (userCulture == serverCulture)
                return;

            foreach (DataColumn column in table.Columns)
            {
                if (!String.IsNullOrEmpty(column.Expression))
                    QueryCulture.ChangeColumnExpression(column, SpecificInstances.QueryCulture.AliaseCultures,
                        userCulture);
            }

            table.ExtendedProperties[key] = userCulture;
        }

        /// <summary>
        /// Установить культуру
        /// </summary>
        public static void SetCulture()
        {
            var culture = GetCulture(out var lcid);
            SetThreadCulture(culture, lcid);
        }

        public static void SetCulture(string culture)
        {
            SetThreadCulture(culture, null);
        }

        private static string GetCulture(out string lcid)
        {
            var culture = "";
            lcid = "1049";
            if (HttpContext.Current.Items["Culture"] != null
                && !string.IsNullOrEmpty((string) HttpContext.Current.Items["Culture"]))
                culture = (string) HttpContext.Current.Items["Culture"];
            else if (HttpContext.Current.Request.Cookies["lcid"] != null
                     && !string.IsNullOrEmpty(HttpContext.Current.Request.Cookies["lcid"].Value))
                lcid = HttpContext.Current.Request.Cookies["lcid"].Value;
            return culture;
        }

        public static bool IsCultureRU =>
            CultureInfo.CurrentUICulture.Name.Equals("ru-ru", StringComparison.OrdinalIgnoreCase);

        public static bool IsCultureKZ
        {
            get
            {
                if (CultureInfo.CurrentUICulture.Name.Equals("kk-kz", StringComparison.OrdinalIgnoreCase))
                    return true;
                return false;
            }
        }

        public static string Culture => CultureInfo.CurrentUICulture.Name;

        public static string GetGroupPart(string message, int index)
        {
            var values = message.Split('#');
            if (values.Length <= index || index < 0) return "[Текст группы не определен]";
            var value = values[index];
            if (value.StartsWith("..._"))
                return value.Substring(4);
            if (value.StartsWith("..."))
                return value.Substring(3);
            if (value.StartsWith("_"))
                return value.Substring(1);
            return value;
        }

        public static void SetThreadCulture()
        {
            var culture = GetCulture(out var lcid);
            SetThreadCulture(culture, lcid);
        }

        public static CultureInfo SetThreadCulture(string culture, string lcid)
        {
            if (culture == "ru")
                culture = "ru-ru";
            else if (culture == "kz")
                culture = "kk-kz";
            else if (culture == "kk")
                culture = "kk-kz";
            CultureInfo ci;
            if (!string.IsNullOrEmpty(culture))
            {
                if (HttpContext.Current != null)
                    HttpContext.Current.Items["Culture"] = culture;
                ci = new CultureInfo(culture);
            }
            else
            {
                var LCID = int.Parse(lcid);
                ci = new CultureInfo(LCID);
            }

            ci.DateTimeFormat.ShortDatePattern = "dd.MM.yyyy";
            ci.DateTimeFormat.FullDateTimePattern = "dd.MM.yyyy HH:mm:ss";
            ci.DateTimeFormat.LongTimePattern = "HH:mm:ss";
            ci.DateTimeFormat.LongDatePattern = "j F Y 'j.'";

            if (ci.Name == "kk-KZ")
            {
                ci.DateTimeFormat.MonthNames = ci.DateTimeFormat.MonthGenitiveNames = new[]
                {
                    "Qańtar", "Aqpan", "Naýryz", "Sáýіr", "Mamyr", "Maýsym", "Shіlde", "Tamyz", "Qyrkúıek", "Qazan",
                    "Qarasha", "Jeltoqsan", ""
                };

                ci.DateTimeFormat.AbbreviatedMonthNames = ci.DateTimeFormat.AbbreviatedMonthGenitiveNames = new[]
                {
                    "Qań", "Aqp", "Naý", "Sáý", "Mam", "Maý", "Shіl", "Tam", "Qyr", "Qaz", "Qar", "Jel", ""
                };

                ci.DateTimeFormat.DayNames = new[]
                {
                    "Jeksenbі", "Dúısenbі", "Seısenbі", "Sársenbі", "Beısenbі", "Juma", "Senbі"
                };

                ci.DateTimeFormat.ShortestDayNames = new[]
                {
                    "Jk", "Ds", "Ss", "Sr", "Bs", "Jm", "Sn"
                };

                ci.DateTimeFormat.AbbreviatedDayNames = new[]
                {
                    "Jek", "Dúı", "Seı", "Sár", "Beı", "Jum", "Sen"
                };
            }

            Thread.CurrentThread.CurrentUICulture = ci;
            Thread.CurrentThread.CurrentCulture = ci;
            return ci;
        }

        public static void SetCulture(Page culture)
        {
            SetCulture();
        }

        public static void SetCulture(string cultureName, Page page)
        {
            SetCulture(cultureName);
        }
    }
}