using System;
using System.Collections.Generic;
using System.Data.Linq;
using System.Linq;
using System.Linq.Expressions;
using System.Web;

namespace Nat.Web.Controls.HistoryFilters
{
    /// <summary>
    /// Фильтрация историчного журнала по dateStart и dateEnd, текушая дата должна попадать в данный период
    /// </summary>
    public static class FilterPeriodHistory
    {
        /// <summary>
        /// Фильтрация истории, исключая запись с ключом id. Если поле showHistory == true, то фильтрация не используется
        /// </summary>
        /// <typeparam name="TSource">Тип</typeparam>
        /// <param name="source">Журнал для фильтрации</param>
        /// <param name="showHistory">Показывать ли историчные данные</param>
        /// <param name="id">Ключ</param>
        /// <returns>Результат фильтрации</returns>
        public static IQueryable<TSource> FPeriodHistory<TSource>(this IQueryable<TSource> source, bool showHistory, long id) where TSource : class, IFilterPeriodHistory
        {
            if (showHistory) return source;
            return source.Where(FPeriodHistoryGetExpression<TSource>(id));
        }

        /// <summary>
        /// Фильтрация истории, исключая запись с ключом id.
        /// </summary>
        /// <typeparam name="TSource">Тип</typeparam>
        /// <param name="source">Журнал для фильтрации</param>
        /// <param name="id">Ключ</param>
        /// <returns>Результат фильтрации</returns>
        public static IQueryable<TSource> FPeriodHistory<TSource>(this IQueryable<TSource> source, long id) where TSource : class, IFilterPeriodHistory
        {
            return source.Where(FPeriodHistoryGetExpression<TSource>(id));
        }

        /// <summary>
        /// Фильтрация истории, исключая запись с ключей ids. Если поле showHistory == true, то фильтрация не используется
        /// </summary>
        /// <typeparam name="TSource">Тип</typeparam>
        /// <param name="source">Журнал для фильтрации</param>
        /// <param name="showHistory">Показывать ли историчные данные</param>
        /// <param name="ids">Ключи</param>
        /// <returns>Результат фильтрации</returns>
        public static IQueryable<TSource> FPeriodHistory<TSource>(this IQueryable<TSource> source, bool showHistory, params long[] ids) where TSource : class, IFilterPeriodHistory
        {
            if (showHistory) return source;
            return source.Where(FPeriodHistoryGetExpression<TSource>(ids));
        }

        /// <summary>
        /// Фильтрация истории, исключая запись с ключей ids.
        /// </summary>
        /// <typeparam name="TSource">Тип</typeparam>
        /// <param name="source">Журнал для фильтрации</param>
        /// <param name="ids">Ключи</param>
        /// <returns>Результат фильтрации</returns>
        public static IQueryable<TSource> FPeriodHistory<TSource>(this IQueryable<TSource> source, params long[] ids) where TSource : class, IFilterPeriodHistory
        {
            return source.Where(FPeriodHistoryGetExpression<TSource>(ids));
        }

        /// <summary>
        /// Фильтрация истории.
        /// </summary>
        /// <typeparam name="TSource">Тип</typeparam>
        /// <param name="source">Журнал для фильтрации</param>
        /// <returns>Результат фильтрации</returns>
        public static IQueryable<TSource> FPeriodHistory<TSource>(this IQueryable<TSource> source) where TSource : class, IFilterPeriodHistory
        {
            return source.Where(FPeriodHistoryGetExpression<TSource>());
        }

        /// <summary>
        /// Фильтрация истории.
        /// </summary>
        /// <typeparam name="TSource">Тип</typeparam>
        /// <param name="source">Журнал для фильтрации</param>
        /// <returns>Результат фильтрации</returns>
        public static IQueryable<TSource> FPeriodHistory2<TSource>(this IQueryable<TSource> source) where TSource : class, IFilterPeriodHistory2
        {
            return source.Where(FPeriodHistoryGetExpression2<TSource>());
        }

        /// <summary>
        /// Фильтрация истории. Если поле showHistory == true, то фильтрация не используется
        /// </summary>
        /// <typeparam name="TSource">Тип</typeparam>
        /// <param name="source">Журнал для фильтрации</param>
        /// <param name="showHistory">Показывать ли историчные данные</param>
        /// <returns>Результат фильтрации</returns>
        public static IQueryable<TSource> FDelHistory<TSource>(this IQueryable<TSource> source, bool showHistory) where TSource : class, IFilterPeriodHistory
        {
            if (showHistory) return source;
            return source.Where(FPeriodHistoryGetExpression<TSource>());
        }

        /// <summary>
        /// Найти ключ текущей записи, по историцной записи
        /// </summary>
        /// <typeparam name="TSource">Тип</typeparam>
        /// <param name="source">Журнал для фильтрации</param>
        /// <param name="row">Строка оригинал</param>
        /// <returns>Идентификатор записи</returns>
        public static long? FPeriodHistoryGetKey<TSource>(this IQueryable<TSource> source, TSource row) where TSource : class, IFilterPeriodHistory
        {
            var time = DateTime.Now;
            var key = source.Where(r => r.dateStart < time && (r.dateEnd == null || r.dateEnd > time) && r.refHistory == (row.refHistory ?? row.id)).
                Select(r => (long?)r.id).
                FirstOrDefault();
            return key;
        }

        /// <summary>
        /// Получение историчных записей по ключу
        /// </summary>
        /// <typeparam name="TSource">Тип</typeparam>
        /// <param name="source">Журнал для фильтрации</param>
        /// <param name="db"></param>
        /// <param name="id">Ключ</param>
        /// <returns></returns>
        public static IQueryable<TSource> FPeriodHistoryGetHistory<TSource>(this IQueryable<TSource> source, DataContext db, long id) where TSource : class, IFilterPeriodHistory
        {
            var result =
                from r in source
                let key = db.GetTable<TSource>().Where(k => k.id == id).Select(k => k.refHistory ?? k.id).First()
                where r.id == key || r.refHistory == key
                select r;
            return result;
        }

        /// <summary>
        /// Получение историчных записей по ключу
        /// </summary>
        /// <typeparam name="TSource">Тип</typeparam>
        /// <param name="source">Журнал для фильтрации</param>
        /// <param name="db"></param>
        /// <param name="id">Ключ</param>
        /// <returns></returns>
        public static Expression<Func<TSource, bool>> FPeriodHistoryGetHistory<TSource>(DataContext db, long id) where TSource : class, IFilterPeriodHistory
        {
            Expression<Func<TSource, long, bool>> exp = (r, key) => r.id == key || r.refHistory == key;
            var keyExp = db.GetTable<TSource>().Where(k => k.id == id).Select(k => k.refHistory ?? k.id).Expression;
            keyExp = Expression.Call(typeof(Queryable), "First", new[] { typeof(long) }, keyExp);
            var param = Expression.Parameter(typeof(TSource), "r");
            return (Expression<Func<TSource, bool>>)Expression.Lambda(Expression.Invoke(exp, param, keyExp), param);
        }

        /// <summary>
        /// Фильтрация истории.
        /// </summary>
        /// <typeparam name="TSource">Тип</typeparam>
        /// <param name="source">Журнал для фильтрации</param>
        /// <returns>Результат фильтрации</returns>
        public static Expression<Func<TSource, bool>> FPeriodHistoryGetExpression<TSource>() where TSource : class, IFilterPeriodHistory
        {
            var time = DateTime.Now;
            return r => r.dateStart < time && (r.dateEnd == null || r.dateEnd > time);
        }

        /// <summary>
        /// Фильтрация истории.
        /// </summary>
        /// <typeparam name="TSource">Тип</typeparam>
        /// <param name="source">Журнал для фильтрации</param>
        /// <returns>Результат фильтрации</returns>
        public static Expression<Func<TSource, bool>> FPeriodHistoryGetExpression2<TSource>() where TSource : class, IFilterPeriodHistory2
        {
            var time = DateTime.Now;
            return r => r.DateStart < time && (r.DateEnd == null || r.DateEnd > time);
        }

        /// <summary>
        /// Фильтрация истории.
        /// </summary>
        /// <typeparam name="TSource">Тип</typeparam>
        /// <param name="source">Журнал для фильтрации</param>
        /// <returns>Результат фильтрации</returns>
        public static Expression<Func<TSource, bool>> FPeriodHistoryGetExpression3<TSource>() where TSource : class, IFilterPeriodHistory3
        {
            var time = DateTime.Now;
            return r => r.DateStart < time && (r.DateEnd == null || r.DateEnd > time);
        }
        /// <summary>
        /// Фильтрация истории.
        /// </summary>
        /// <typeparam name="TSource">Тип</typeparam>
        /// <param name="ids">Ключи</param>
        /// <returns>Результат фильтрации</returns>
        public static Expression<Func<TSource, bool>> FPeriodHistoryGetExpression<TSource>(params long[] ids) where TSource : class, IFilterPeriodHistory
        {
            var time = DateTime.Now;
            return r => (r.dateStart < time && (r.dateEnd == null || r.dateEnd > time)) || ids.Contains(r.id);
        }

        /// <summary>
        /// Фильтрация истории.
        /// </summary>
        /// <typeparam name="TSource">Тип</typeparam>
        /// <param name="ids">Ключи</param>
        /// <returns>Результат фильтрации</returns>
        public static Expression<Func<TSource, bool>> FPeriodHistoryGetExpression2<TSource>(params long[] ids) where TSource : class, IFilterPeriodHistory2
        {
            var time = DateTime.Now;
            return r => (r.DateStart < time && (r.DateEnd == null || r.DateEnd > time)) || ids.Contains(r.id);
        }

        /// <summary>
        /// Фильтрация истории.
        /// </summary>
        /// <typeparam name="TSource">Тип</typeparam>
        /// <param name="ids">Ключи</param>
        /// <returns>Результат фильтрации</returns>
        public static Expression<Func<TSource, bool>> FPeriodHistoryGetExpression3<TSource>(params long[] ids) where TSource : class, IFilterPeriodHistory3
        {
            var time = DateTime.Now;
            return r => (r.DateStart < time && (r.DateEnd == null || r.DateEnd > time)) || ids.Contains(r.id);
        }

        /// <summary>
        /// Фильтрация истории.
        /// </summary>
        /// <typeparam name="TSource">Тип</typeparam>
        /// <param name="id">Ключ</param>
        /// <returns>Результат фильтрации</returns>
        public static Expression<Func<TSource, bool>> FPeriodHistoryGetExpression<TSource>(long id) where TSource : class, IFilterPeriodHistory
        {
            var time = DateTime.Now;
            return r => (r.dateStart < time && (r.dateEnd == null || r.dateEnd > time)) || r.id == id;
        }
    }

    /// <summary>
    /// Интерфейс, для таблицы с историей.
    /// </summary>
    public interface IFilterPeriodHistory
    {
        DateTime dateStart { get; }
        DateTime? dateEnd { get; }
        long? refHistory { get; }
        long id { get; }
    }

    /// <summary>
    /// Интерфейс, для таблицы с историей.
    /// </summary>
    public interface IFilterPeriodHistory2
    {
        DateTime DateStart { get; }
        DateTime? DateEnd { get; }
        long? refHistory { get; }
        long id { get; }
    }

    /// <summary>
    /// Интерфейс, для таблицы с историей.
    /// </summary>
    public interface IFilterPeriodHistory3
    {
        DateTime DateStart { get; }
        DateTime DateEnd { get; }
        long? refHistory { get; }
        long id { get; }
    }
}