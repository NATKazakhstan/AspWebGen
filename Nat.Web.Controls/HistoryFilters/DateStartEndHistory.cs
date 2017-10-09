using System;
using System.Linq;
using System.Linq.Expressions;

namespace Nat.Web.Controls.HistoryFilters
{
    public static class FilterDateStartEndHistory
    {
        /// <summary>
        /// Фильтрация истории, исключая запись с ключом id. Если поле showHistory == true, то фильтрация не используется
        /// </summary>
        /// <typeparam name="TSource">Тип</typeparam>
        /// <param name="source">Журнал для фильтрации</param>
        /// <param name="showHistory">Показывать ли историчные данные</param>
        /// <param name="id">Ключ</param>
        /// <returns>Результат фильтрации</returns>
        public static IQueryable<TSource> FDateStartEndHistory<TSource>(this IQueryable<TSource> source, bool showHistory, long id) where TSource : class, IFilterDateStartEndHistory
        {
            if (showHistory) return source;
            return source.Where(FDateStartEndHistoryGetExpression<TSource>(id));
        }

        /// <summary>
        /// Фильтрация истории, исключая запись с ключом id.
        /// </summary>
        /// <typeparam name="TSource">Тип</typeparam>
        /// <param name="source">Журнал для фильтрации</param>
        /// <param name="id">Ключ</param>
        /// <returns>Результат фильтрации</returns>
        public static IQueryable<TSource> FDateStartEndHistory<TSource>(this IQueryable<TSource> source, long id) where TSource : class, IFilterDateStartEndHistory
        {
            return source.Where(FDateStartEndHistoryGetExpression<TSource>(id));
        }

        /// <summary>
        /// Фильтрация истории, исключая запись с ключей ids. Если поле showHistory == true, то фильтрация не используется
        /// </summary>
        /// <typeparam name="TSource">Тип</typeparam>
        /// <param name="source">Журнал для фильтрации</param>
        /// <param name="showHistory">Показывать ли историчные данные</param>
        /// <param name="ids">Ключи</param>
        /// <returns>Результат фильтрации</returns>
        public static IQueryable<TSource> FDateStartEndHistory<TSource>(this IQueryable<TSource> source, bool showHistory, params long[] ids) where TSource : class, IFilterDateStartEndHistory
        {
            if (showHistory) return source;
            return source.Where(FDateStartEndHistoryGetExpression<TSource>(ids));
        }

        /// <summary>
        /// Фильтрация истории, исключая запись с ключей ids.
        /// </summary>
        /// <typeparam name="TSource">Тип</typeparam>
        /// <param name="source">Журнал для фильтрации</param>
        /// <param name="ids">Ключи</param>
        /// <returns>Результат фильтрации</returns>
        public static IQueryable<TSource> FDateStartEndHistory<TSource>(this IQueryable<TSource> source, params long[] ids) where TSource : class, IFilterDateStartEndHistory
        {
            return source.Where(FDateStartEndHistoryGetExpression<TSource>(ids));
        }

        /// <summary>
        /// Фильтрация истории.
        /// </summary>
        /// <typeparam name="TSource">Тип</typeparam>
        /// <param name="source">Журнал для фильтрации</param>
        /// <returns>Результат фильтрации</returns>
        public static IQueryable<TSource> FDateStartEndHistory<TSource>(this IQueryable<TSource> source) where TSource : class, IFilterDateStartEndHistory
        {
            return source.Where(FDateStartEndHistoryGetExpression<TSource>());
        }

        /// <summary>
        /// Фильтрация истории. Если поле showHistory == true, то фильтрация не используется
        /// </summary>
        /// <typeparam name="TSource">Тип</typeparam>
        /// <param name="source">Журнал для фильтрации</param>
        /// <param name="showHistory">Показывать ли историчные данные</param>
        /// <returns>Результат фильтрации</returns>
        public static IQueryable<TSource> FDelHistory<TSource>(this IQueryable<TSource> source, bool showHistory) where TSource : class, IFilterDateStartEndHistory
        {
            if (showHistory) return source;
            return source.Where(FDateStartEndHistoryGetExpression<TSource>());
        }
        
        /// <summary>
        /// Фильтрация истории.
        /// </summary>
        /// <typeparam name="TSource">Тип</typeparam>
        /// <param name="source">Журнал для фильтрации</param>
        /// <returns>Результат фильтрации</returns>
        public static Expression<Func<TSource, bool>> FDateStartEndHistoryGetExpression<TSource>() where TSource : class, IFilterDateStartEndHistory
        {
            var time = DateTime.Now;
            return r => r.dateStart < time && (r.dateEnd == null || r.dateEnd > time);
        }

        /// <summary>
        /// Фильтрация истории.
        /// </summary>
        /// <typeparam name="TSource">Тип</typeparam>
        /// <param name="ids">Ключи</param>
        /// <returns>Результат фильтрации</returns>
        public static Expression<Func<TSource, bool>> FDateStartEndHistoryGetExpression<TSource>(params long[] ids) where TSource : class, IFilterDateStartEndHistory
        {
            var time = DateTime.Now;
            return r => (r.dateStart < time && (r.dateEnd == null || r.dateEnd > time)) || ids.Contains(r.id);
        }

        /// <summary>
        /// Фильтрация истории.
        /// </summary>
        /// <typeparam name="TSource">Тип</typeparam>
        /// <param name="id">Ключ</param>
        /// <returns>Результат фильтрации</returns>
        public static Expression<Func<TSource, bool>> FDateStartEndHistoryGetExpression<TSource>(long id) where TSource : class, IFilterDateStartEndHistory
        {
            var time = DateTime.Now;
            return r => (r.dateStart < time && (r.dateEnd == null || r.dateEnd > time)) || r.id == id;
        }
    
        public static Expression<Func<TSource, bool>> FDateStartEndHistoryCanEditDeleteExpression<TSource>() where TSource : class, IFilterDateStartEndHistory
        {
            return r => true;
        }
    }

    /// <summary>
    /// Интерфейс, для таблицы с историей.
    /// </summary>
    public interface IFilterDateStartEndHistory
    {
        DateTime dateStart { get; }
        DateTime? dateEnd { get; }
        long id { get; }
    }
}