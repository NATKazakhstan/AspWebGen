using System;
using System.Data;
using System.Linq;
using System.Globalization;
using System.Text;
using Nat.Tools.Specific;
using Nat.Web.Controls;
using System.Web;

namespace Nat.Web.ReportManager
{
    public static class ReportUtils
    {
        public const string ExtPropEmptyIfDbNull = "emptyIfDbNull";
        public const string ExtPropFormat = "format";

        /// <summary>
        /// Собрать данные из таблицы в строчку.
        /// </summary>
        /// <param name="view">Данные которой нужно поместить в строку текста</param>
        /// <param name="separator">Разделитель между строками</param>
        /// <param name="format">Формат (такой же как string.Foramt)</param>
        /// <param name="columns">Колонки данных которые необходимо вывести</param>
        /// <returns>Строка соддержащая данные таблицы</returns>
        /// <remarks>Также поддерживаются расширинные свойства колонок:
        /// - ExtPropFormat: формат формирования значения колонки (такой же как string.Foramt);
        /// - ExtPropEmptyIfDbNull: значение будет пустое, если значение поля равно DBNull.Value, имеет смысл использовать с ExtPropFormat.
        /// </remarks>
        public static string FetchItemsArray(DataView view, string separator, string format,
                                             params DataColumn[] columns)
        {
            var res = new StringBuilder();
            var values = new object[columns.Length];
            var formats = new string[columns.Length];
            var emptyIfDbNull = new bool[columns.Length];
            for (int i = 0; i < columns.Length; i++)
            {
                formats[i] = (string) columns[i].ExtendedProperties[ExtPropFormat];
                emptyIfDbNull[i] = (bool?) columns[i].ExtendedProperties[ExtPropEmptyIfDbNull] ?? false;
            }

            foreach (DataRowView rowView in view)
            {
                DataRow row = rowView.Row;
                bool isEmpty = true;
                for (int i = 0; i < columns.Length; i++)
                {
                    object value = row[columns[i]];
                    bool dbNull = value == DBNull.Value;
                    isEmpty &= dbNull;
                    if (emptyIfDbNull[i] && dbNull)
                        values[i] = "";
                    else if (string.IsNullOrEmpty(formats[i]))
                        values[i] = value;
                    else
                        values[i] = string.Format(formats[i], value);
                }

                if (!isEmpty)
                {
                    string value = string.Format(format, values);
                    if (!string.IsNullOrEmpty(value))
                    {
                        res.Append(value);
                        res.Append(separator);
                    }
                }
            }
            if (res.Length == 0) return "";
            return res.ToString(0, res.Length - separator.Length);
        }

        /// <summary>
        /// Собрать данные из таблицы в строчку.
        /// </summary>
        /// <param name="row">Строка дочернии записи, которой нужно поместить в строку текста</param>
        /// <param name="relationName">Связь, по которой можно получить дочернии записи</param>
        /// <param name="separator">Разделитель между строками</param>
        /// <param name="format">Формат (такой же как string.Foramt)</param>
        /// <param name="filter">Строка фильтра, для фильтрации дочерних записей</param>
        /// <param name="columns">Колонки данных которые необходимо вывести</param>
        /// <returns>Строка соддержащая данные таблицы</returns>
        /// <remarks>Также поддерживаются расширинные свойства колонок:
        /// - ExtPropFormat: формат формирования значения колонки (такой же как string.Foramt);
        /// - ExtPropEmptyIfDbNull: значение будет пустое, если значение поля равно DBNull.Value, имеет смысл использовать с ExtPropFormat.
        /// </remarks>
        public static string FetchItemsArray(DataRowView row, string relationName, string separator, string format, string filter,
                                             params DataColumn[] columns)
        {
            DataView childView = row.CreateChildView(relationName);
            if (!string.IsNullOrEmpty(filter)) childView.RowFilter = filter;
            return FetchItemsArray(childView, separator, format, columns);
        }

        [Obsolete("Устарело - используйте другие методы FetchItemsArray.")]
        public static string FetchItemsArray(DataTable table, params string[] columnName)
        {
            var result = new StringBuilder();

            for (int rowID = 0; rowID < table.Rows.Count; rowID++)
            {
                for (int cnID = 0; cnID < columnName.Length; cnID++)
                {
                    string first = string.Empty;
                    string last = string.Empty;
                    string clearColumnName = columnName[cnID];
                    int firstID = columnName[cnID].IndexOf('|');
                    if (firstID > -1)
                    {
                        first = clearColumnName.Substring(firstID + 1, 1);
                        clearColumnName = clearColumnName.Remove(firstID, 2);
                    }
                    int lastID = clearColumnName.IndexOf('|');
                    if (firstID > -1)
                    {
                        last = clearColumnName.Substring(lastID + 1, 1);
                        clearColumnName = clearColumnName.Remove(lastID, 2);
                    }

                    for (int columnID = 0; columnID < table.Columns.Count; columnID++)
                    {
                        if (table.Columns[columnID].ColumnName.Equals(clearColumnName))
                        {
                            bool isEmpty = table.Rows[rowID].ItemArray[columnID].ToString().Equals(string.Empty);

                            if (table.Rows[rowID].ItemArray[columnID] is DateTime)
                            {
                                var dateColumn = (DateTime)table.Rows[rowID].ItemArray[columnID];
                                result.Append(first);
                                result.Append(dateColumn.ToShortDateString());
                                result.Append(last);
                            }
                            else
                            {
                                if (!isEmpty)
                                {
                                    result.Append(first);
                                    result.Append(table.Rows[rowID].ItemArray[columnID]);
                                    result.Append(last);
                                }
                            }

                            if (isEmpty && result.Length > 2)
                                result.Remove(result.Length - 2, 2);

                            result.Append(cnID == columnName.Length - 1 ? "; " : ", ");
                            break;
                        }
                    }
                }
            }
            return result.ToString();
        }

        /// <summary>
        /// Построение списка значений дочерней таблицы
        /// </summary>
        /// <param name="row">Родительская строка</param>
        /// <param name="RelationName">Наименование связи</param>
        /// <param name="Column">Имя колонки, из которй беруться данные</param>
        /// <param name="Separator">Разделитель</param>
        /// <returns>Возвращает список значений дочерней таблицы</returns>
        [Obsolete("Устарело - используйте другие методы FetchItemsArray.")]
        public static string FetchItemsArray(DataRow row, string RelationName, DataColumn Column, string Separator)
        {
            DataRow[] childRows = row.GetChildRows(RelationName);
            string valueList = string.Empty;
            if (childRows.Length == 0) return string.Empty;

            foreach (DataRow dataRow in childRows)
            {
                valueList = String.Format("{0}{1}{2}\n", valueList, dataRow[Column], Separator);
            }
            return valueList;
        }

        [Obsolete("Устарело - используйте другие методы FetchItemsArray.")]
        public static string FetchItemsArrayExt(DataRow row, string RelationName, DataColumn Column, string Separator,
                                                string AddingWord)
        {
            DataRow[] childRows = row.GetChildRows(RelationName);
            string valueList = string.Empty;
            if (childRows.Length == 0) return string.Empty;

            foreach (DataRow dataRow in childRows)
            {
                valueList = String.Format("{0}{1}{2}\n", valueList, AddingWord + dataRow[Column], Separator);
            }
            return valueList;
        }

        [Obsolete("Устарело - используйте структуру Nat.Tools.System.DateSpan.")]
        public static void CorrectValues(ref int years, ref int months, ref int days)
        {
            years += (months + days/30)/12;
            months = (months + days/30)%12;
            days = days%30;
            
        }

        /// <summary>
        /// Привести к верхнему регистру первый символ строк таблицы, указаных колонок.
        /// </summary>
        /// <param name="table">таблица с данными</param>
        /// <param name="columns">колонки, которые надо преоброзовать</param>
        public static void ToUpperFirstChar(this DataTable table, params DataColumn[] columns)
        {
            foreach (DataRow row in table.Rows)
            {
                foreach (DataColumn column in columns)
                {
                    if (row[column] != DBNull.Value)
                        row[column] = ToUpperFirstChar((string)row[column]);
                }
            }
        }

        /// <summary>
        /// Приведение первого символа строки к верхнему регистру.
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static string ToUpperFirstChar(this string value)
        {
            if (string.IsNullOrEmpty(value)) return value;
            if (value.Length == 1) return value.ToUpper();
            return value.Substring(0, 1).ToUpper() + value.Substring(1);
        }

        /// <summary>
        /// Приведение первого символа строки к нижнему регистру.
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static string ToLowerFirstChar(this string value)
        {
            if (string.IsNullOrEmpty(value)) return value;
            if (value.Length == 1) return value.ToLower();
            return value.Substring(0, 1).ToLower() + value.Substring(1);
        }

        public static void ToUpper(this DataTable table, params DataColumn[] columns)
        {
            foreach (DataRow row in table.Rows)
            {
                foreach (DataColumn column in columns)
                {
                    if (row[column] != DBNull.Value && ((string)row[column]) != null)
                        row[column] = ((string)row[column]).ToUpper();
                }
            }
        }

        /// <summary>
        /// Разобрать значени разделителем splitSeparator, и объеденить каждую часть в соответсвии с phrases, используя форматирование format
        /// </summary>
        /// <param name="value">Значени. Например код статьи 100.5.1</param>
        /// <param name="splitSeparator">Разделитель значения. Например "."</param>
        /// <param name="options">Настройка разделения</param>
        /// <param name="phrases">Фразы для составления текста по значениям. Например "по статье", "части", "пункту"</param>
        /// <param name="format">Формат для объединения текста значения ({0}) и текста фразы ({1}). Например "{1} {0}"</param>
        /// <param name="joinSeparator">Разделитель объединения элементов. Например " "</param>
        /// <returns>Например получится "по статье 100 части 5 пункту 1"</returns>
        public static string ParseString(string value, string[] splitSeparator, StringSplitOptions options, string joinSeparator, string format, params string[] phrases)
        {
            var array = value.
                Split(splitSeparator, options).
                Select((s, index) => string.Format(format, s.Trim(), phrases[index])).
                ToArray();
            return string.Join(joinSeparator, array);
        }

        /// <summary>
        /// Разобрать значения в коллекцию используя splitSeparatorArray, optionsArray. Результаты разбора другим методом ParseString объеденить по joinSeparatorArray.
        /// Разобрать значени разделителем splitSeparator, и объеденить каждую часть в соответсвии с phrases, используя форматирование format
        /// </summary>
        /// <param name="value">Значени. Например код статьи 100.5.1</param>
        /// <param name="splitSeparatorArray"></param>
        /// <param name="optionsArray"></param>
        /// <param name="joinSeparatorArray"></param>
        /// <param name="splitSeparator">Разделитель значения. Например "."</param>
        /// <param name="options">Настройка разделения</param>
        /// <param name="phrases">Фразы для составления текста по значениям. Например "по статье", "части", "пункту"</param>
        /// <param name="format">Формат для объединения текста значения ({0}) и текста фразы ({1}). Например "{1} {0}"</param>
        /// <param name="joinSeparator">Разделитель объединения элементов. Например " "</param>
        /// <returns>Например получится "по статье 100 части 5 пункту 1"</returns>
        public static string ParseString(string values, string[] splitSeparatorArray, StringSplitOptions optionsArray, string joinSeparatorArray, string[] splitSeparator, StringSplitOptions options, string joinSeparator, string format, params string[] phrases)
        {
            var array = values.
                Split(splitSeparatorArray, optionsArray).
                Select(value => ParseString(value, splitSeparator, options, joinSeparator, format, phrases)).
                ToArray();
            return string.Join(joinSeparatorArray, array);
        }

        public static T[] NewArray<T>(params T[] values)
        {
            return values;
        }

        public static string TranslateIntToText(int integer, int cultureId)
        {
            #region translateTable
            var translateTable = new string[2, 4, 10];
            translateTable[0, 0, 0] = "ноль";
            translateTable[0, 0, 1] = "один";
            translateTable[0, 0, 2] = "два";
            translateTable[0, 0, 3] = "три";
            translateTable[0, 0, 4] = "четыре";
            translateTable[0, 0, 5] = "пять";
            translateTable[0, 0, 6] = "шесть";
            translateTable[0, 0, 7] = "семь";
            translateTable[0, 0, 8] = "восемь";
            translateTable[0, 0, 9] = "девять";
            translateTable[0, 1, 0] = "десять";
            translateTable[0, 1, 1] = "одиннадцать";
            translateTable[0, 1, 2] = "двенадцать";
            translateTable[0, 1, 3] = "тринадцать";
            translateTable[0, 1, 4] = "четырнадцать";
            translateTable[0, 1, 5] = "пятнадцать";
            translateTable[0, 1, 6] = "шестьнадцать";
            translateTable[0, 1, 7] = "семьнадцать";
            translateTable[0, 1, 8] = "восемьнадцать";
            translateTable[0, 1, 9] = "девятнадцать";
            translateTable[0, 2, 2] = "двадцать";
            translateTable[0, 2, 3] = "тридцать";
            translateTable[0, 2, 4] = "сорок";
            translateTable[0, 2, 5] = "пятьдесят";
            translateTable[0, 2, 6] = "шестьдесят";
            translateTable[0, 2, 7] = "семьдесят";
            translateTable[0, 2, 8] = "восемьдесят";
            translateTable[0, 2, 9] = "девяносто";
            translateTable[0, 3, 1] = "сто";
            translateTable[0, 3, 2] = "двести";
            translateTable[0, 3, 3] = "триста";
            translateTable[0, 3, 4] = "четыреста";
            translateTable[0, 3, 5] = "пятьсот";
            translateTable[0, 3, 6] = "шестьсот";
            translateTable[0, 3, 7] = "семьсот";
            translateTable[0, 3, 8] = "восемьсот";
            translateTable[0, 3, 9] = "девятьсот";

            translateTable[1, 0, 0] = "нөль";
            translateTable[1, 0, 1] = "бір";
            translateTable[1, 0, 2] = "екі";
            translateTable[1, 0, 3] = "үш";
            translateTable[1, 0, 4] = "төрт";
            translateTable[1, 0, 5] = "бес";
            translateTable[1, 0, 6] = "алты";
            translateTable[1, 0, 7] = "жеті";
            translateTable[1, 0, 8] = "сегіз";
            translateTable[1, 0, 9] = "тоғыз";
            translateTable[1, 1, 0] = "он";
            translateTable[1, 1, 1] = "он бір";
            translateTable[1, 1, 2] = "он екі";
            translateTable[1, 1, 3] = "он үш";
            translateTable[1, 1, 4] = "он төрт";
            translateTable[1, 1, 5] = "он бес";
            translateTable[1, 1, 6] = "он алты";
            translateTable[1, 1, 7] = "он жеті";
            translateTable[1, 1, 8] = "он сегіз";
            translateTable[1, 1, 9] = "он тоғыз";
            translateTable[1, 2, 2] = "жиырма";
            translateTable[1, 2, 3] = "отыз";
            translateTable[1, 2, 4] = "қырық";
            translateTable[1, 2, 5] = "елу";
            translateTable[1, 2, 6] = "алпыс";
            translateTable[1, 2, 7] = "жетпіс";
            translateTable[1, 2, 8] = "сексен";
            translateTable[1, 2, 9] = "тоқсан";
            translateTable[1, 3, 1] = "жүз";
            translateTable[1, 3, 2] = "екі жұз";
            translateTable[1, 3, 3] = "үш жүз";
            translateTable[1, 3, 4] = "төрт жүз";
            translateTable[1, 3, 5] = "бес жүз";
            translateTable[1, 3, 6] = "алты жүз";
            translateTable[1, 3, 7] = "жеті жүз";
            translateTable[1, 3, 8] = "сегіз жүз";
            translateTable[1, 3, 9] = "тоғыз жүз";
            #endregion

            var result = "";
            var digit = integer / 100;
            if (digit > 9)
            {
                throw new NotSupportedException("Supported range of integer is: [0..1000]");
            }
            if (0 < digit && digit < 10)
            {
                result += translateTable[cultureId, 3, digit];
            }
            integer -= digit * 100;
            digit = integer / 10;
            if (1 < digit && digit <= 9)
            {
                result += " " + translateTable[cultureId, 2, digit];
            }
            else if (digit == 1)
            {
                var unit = integer % 10;
                result += " " + translateTable[cultureId, 1, unit];
            }
            if (digit != 1)
            {
                integer -= digit * 10;
                digit = integer;
                if (0 < digit && digit < 10)
                {
                    result += " " + translateTable[cultureId, 0, digit];
                }
            }
            result = result.Trim();
            return result;
        }

        public static string GetDateLongString(DateTime date, bool isKz)
        {
            var culture = CultureInfo.GetCultureInfo(isKz ? "kk-kz" : "ru-ru");
            if (isKz)
                return date.ToString("yyyy ж. «dd» ") + culture.DateTimeFormat.GetMonthName(date.Month);
            return date.ToString("«dd» ") + GetMonthName(date.Month, false, 0) + " " + date.Year;
        }

        public static string GetMonthName(int month, bool isKz, int @case)
        {
            var dataTable = (DataTable)HttpContext.Current.Cache["DIC_Months"];
            if (dataTable == null)
                using (var connection = WebSpecificInstances.DbFactory.CreateConnection())
                {
                    var command = connection.CreateCommand();
                    var adapter = WebSpecificInstances.DbFactory.CreateDataAdapter();
                    command.CommandText = "select code, isnull(nameRuRod, nameRu) as nameRuRod, isnull(nameKzDat, nameKz) as nameKzDat, isnull(nameKzIsh, nameKz) as nameKzIsh, isnull(nameKzPred, nameKz) as nameKzPred from DIC_Months where dateStart < @date and (dateEnd is null or dateEnd > @date)";
                    var parameter = command.CreateParameter();
                    parameter.ParameterName = "date";
                    parameter.Value = DateTime.Now;
                    parameter.DbType = DbType.DateTime;
                    command.Parameters.Add(parameter);
                    adapter.SelectCommand = command;
                    dataTable = new DataTable("DIC_Months");
                    dataTable.Columns.Add("code", typeof(byte));
                    dataTable.PrimaryKey = new[] { dataTable.Columns["code"] };
                    adapter.Fill(dataTable);
                    HttpContext.Current.Cache["DIC_Months"] = dataTable;
                }
            string column = "nameRuRod";
            if(isKz)
            switch (@case)
            {
                case 0:
                    column = "nameKzIsh";
                    break;
                case 1:
                    column = "nameKzDat";
                    break;
                case 2:
                    column = "nameKzPred";
                    break;
                default:
                    break;
            }
            var row = dataTable.Rows.Find((byte)month);
            if (row == null)
            {
                var culture = CultureInfo.GetCultureInfo(isKz ? "kk-kz" : "ru-ru");
                return culture.DateTimeFormat.GetMonthName(month);
            }
            if (row[column] != DBNull.Value)
                return (string)dataTable.Rows.Find((byte)month)[column];
            return (string)row["nameRu"];
        }

        public static string GetDateLongStringFrom(DateTime date, bool isKz)
        {
            var culture = CultureInfo.GetCultureInfo(isKz ? "kk-kz" : "ru-ru");
            if (isKz)
                return date.ToString("yyyy ж. «dd» ") + GetMonthName(date.Month, true, 0);
            return date.ToString("«dd» ") + GetMonthName(date.Month, false, 0) + " " + date.Year;
        }

        public static string GetDateLongStringTo(DateTime date, bool isKz)
        {
            var culture = CultureInfo.GetCultureInfo(isKz ? "kk-kz" : "ru-ru");
            if (isKz)
                return date.ToString("yyyy ж. «dd» ") + GetMonthName(date.Month, true, 1);
            return date.ToString("«dd» ") + GetMonthName(date.Month, true, 1) + " " + date.Year;
        }

        public static string GetDateLongStringPrepositionalCase(DateTime date, bool isKz)
        {
            var culture = CultureInfo.GetCultureInfo(isKz ? "kk-kz" : "ru-ru");
            if (isKz)
                return date.ToString("yyyy ж. «dd» ") + GetMonthName(date.Month, true, 2);
            return date.ToString("«dd» ") + GetMonthName(date.Month, true, 2) + " " + date.Year;
        }

        public static string GetDateLongKazIsh(DateTime date)
        {
            return string.Format("{0} жылғы {1} {2}", date.Year, date.Day, GetMonthName(date.Month, true, 0));
        }

        public static string AddDot(this string value)
        {
            value = value.TrimEnd();
            if (value.Length < 1) return value;
            string lc = value.Substring(value.Length - 1, 1);
            if (lc == ".") return value;
            if (lc == "," || lc == ":" || lc == ";")
                return value.Substring(0, value.Length - 1) + ".";
            return value + ".";
        }

        public static string AddComma(this string value)
        {
            value = value.TrimEnd();
            if (value.Length < 1) return value;
            string lc = value.Substring(value.Length - 1, 1);
            if (lc == ",") return value;
            if (lc == "." || lc == ":" || lc == ";")
                return value.Substring(0, value.Length - 1) + ",";
            return value + ",";
        }

        /// <summary>
        /// Возвращает ФИО в сокращенной форме (с инициалами)
        /// </summary>
        /// <param name="fio"></param>
        /// <returns></returns>
        public static string GetShortFio(this string fio)
        {
            if (string.IsNullOrWhiteSpace(fio))
                return fio;
            var split = fio.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            if (split.Length > 3)
                return fio;

            return split[0]
                   + (split.Length > 1 ? " " + split[1][0] + "." : "")
                   + (split.Length > 2 ? " " + split[2][0] + "." : "");
        }
    }
}