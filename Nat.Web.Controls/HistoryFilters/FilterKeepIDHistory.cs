using System;
using System.Collections.Generic;
using System.Data.Linq;
using System.Linq;
using System.Linq.Expressions;
using System.Web;

namespace Nat.Web.Controls.HistoryFilters
{
    /// <summary>
    /// Фильтрация историчного журнала по принцыпу, что для истории создаются копии записи, а текущая запись имеет ссылку на себя. 
    /// Также используется флаг об удалении.
    /// </summary>
    public static class FilterKeepIDHistory
    {
        /// <summary>
        /// Фильтрация истории, исключая запись с ключом id. Если поле showHistory == true, то фильтрация не используется
        /// </summary>
        /// <typeparam name="TSource">Тип</typeparam>
        /// <param name="source">Журнал для фильтрации</param>
        /// <param name="showHistory">Показывать ли историчные данные</param>
        /// <param name="id">Ключ</param>
        /// <returns>Результат фильтрации</returns>
        public static IQueryable<TSource> FKeepIDHistory<TSource>(this IQueryable<TSource> source, bool showHistory, long id) where TSource : class, IFilterKeepIDHistory
        {
            if (showHistory) return source;
            return source.Where(r => (r.refHistory == r.id && !r.isDel) || r.id == id);
        }

        /// <summary>
        /// Фильтрация истории, исключая запись с ключом id.
        /// </summary>
        /// <typeparam name="TSource">Тип</typeparam>
        /// <param name="source">Журнал для фильтрации</param>
        /// <param name="id">Ключ</param>
        /// <returns>Результат фильтрации</returns>
        public static IQueryable<TSource> FKeepIDHistory<TSource>(this IQueryable<TSource> source, long id) where TSource : class, IFilterKeepIDHistory
        {
            return source.Where(r => (r.refHistory == r.id && !r.isDel) || r.id == id);
        }

        /// <summary>
        /// Фильтрация истории, исключая запись с ключей ids. Если поле showHistory == true, то фильтрация не используется
        /// </summary>
        /// <typeparam name="TSource">Тип</typeparam>
        /// <param name="source">Журнал для фильтрации</param>
        /// <param name="showHistory">Показывать ли историчные данные</param>
        /// <param name="ids">Ключи</param>
        /// <returns>Результат фильтрации</returns>
        public static IQueryable<TSource> FKeepIDHistory<TSource>(this IQueryable<TSource> source, bool showHistory, params long[] ids) where TSource : class, IFilterKeepIDHistory
        {
            if (showHistory) return source;
            return source.Where(r => (r.refHistory == r.id && !r.isDel) || ids.Contains(r.id));
        }

        /// <summary>
        /// Фильтрация истории, исключая запись с ключей ids.
        /// </summary>
        /// <typeparam name="TSource">Тип</typeparam>
        /// <param name="source">Журнал для фильтрации</param>
        /// <param name="ids">Ключи</param>
        /// <returns>Результат фильтрации</returns>
        public static IQueryable<TSource> FKeepIDHistory<TSource>(this IQueryable<TSource> source, params long[] ids) where TSource : class, IFilterKeepIDHistory
        {
            return source.Where(r => (r.refHistory == r.id && !r.isDel) || ids.Contains(r.id));
        }

        /// <summary>
        /// Фильтрация истории.
        /// </summary>
        /// <typeparam name="TSource">Тип</typeparam>
        /// <param name="source">Журнал для фильтрации</param>
        /// <returns>Результат фильтрации</returns>
        public static IQueryable<TSource> FKeepIDHistory<TSource>(this IQueryable<TSource> source) where TSource : class, IFilterKeepIDHistory
        {
            return source.Where(r => r.refHistory == r.id && !r.isDel);
        }

        /// <summary>
        /// Фильтрация истории. Если поле showHistory == true, то фильтрация не используется
        /// </summary>
        /// <typeparam name="TSource">Тип</typeparam>
        /// <param name="source">Журнал для фильтрации</param>
        /// <param name="showHistory">Показывать ли историчные данные</param>
        /// <returns>Результат фильтрации</returns>
        public static IQueryable<TSource> FDelHistory<TSource>(this IQueryable<TSource> source, bool showHistory) where TSource : class, IFilterKeepIDHistory
        {
            if (showHistory) return source;
            return source.Where(r => r.refHistory == r.id && !r.isDel);
        }

        /// <summary>
        /// Найти ключ текущей записи, по историцной записи
        /// </summary>
        /// <typeparam name="TSource">Тип</typeparam>
        /// <param name="source">Журнал для фильтрации</param>
        /// <param name="row">Строка оригинал</param>
        /// <returns>Идентификатор записи</returns>
        public static long? FKeepIDHistoryGetKey<TSource>(this IQueryable<TSource> source, TSource row) where TSource : class, IFilterKeepIDHistory
        {
            return row.refHistory ?? row.id;
        }

        /// <summary>
        /// Получение историчных записей по ключу
        /// </summary>
        /// <typeparam name="TSource">Тип</typeparam>
        /// <param name="source">Журнал для фильтрации</param>
        /// <param name="db"></param>
        /// <param name="id">Ключ</param>
        /// <returns></returns>
        public static IQueryable<TSource> FKeepIDHistoryGetHistory<TSource>(this IQueryable<TSource> source, DataContext db, long id) where TSource : class, IFilterKeepIDHistory
        {
            var result =
                from r in source
                where r.refHistory == db.GetTable<TSource>().Where(k => k.id == id).Select(k => k.refHistory).First()
                select r;
            return result;
        }

        /// <summary>
        /// Фильтрация истории.
        /// </summary>
        /// <typeparam name="TSource">Тип</typeparam>
        /// <param name="source">Журнал для фильтрации</param>
        /// <returns>Результат фильтрации</returns>
        public static Expression<Func<TSource, bool>> FKeepIDHistoryGetExpression<TSource>() where TSource : class, IFilterKeepIDHistory
        {
            return r => r.refHistory == r.id && !r.isDel;
        }

        /// <summary>
        /// Фильтрация истории.
        /// </summary>
        /// <typeparam name="TSource">Тип</typeparam>
        /// <param name="ids">Ключи</param>
        /// <returns>Результат фильтрации</returns>
        public static Expression<Func<TSource, bool>> FKeepIDHistoryGetExpression<TSource>(params long[] ids) where TSource : class, IFilterKeepIDHistory
        {
            return r => (r.refHistory == r.id && !r.isDel) || ids.Contains(r.id);
        }

        /// <summary>
        /// Фильтрация истории.
        /// </summary>
        /// <typeparam name="TSource">Тип</typeparam>
        /// <param name="id">Ключ</param>
        /// <returns>Результат фильтрации</returns>
        public static Expression<Func<TSource, bool>> FKeepIDHistoryGetExpression<TSource>(long id) where TSource : class, IFilterKeepIDHistory
        {
            return r => (r.refHistory == r.id && !r.isDel) || r.id == id;
        }

    }

    /// <summary>
    /// Интерфейс, для таблицы с историей.
    /// </summary>
    public interface IFilterKeepIDHistory
    {
        long? refHistory { get; }
        bool isDel { get; }
        long id { get; }
    }
}