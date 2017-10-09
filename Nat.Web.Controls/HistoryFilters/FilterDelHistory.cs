using System;
using System.Collections.Generic;
using System.Data.Linq;
using System.Linq;
using System.Linq.Expressions;
using System.Web;

namespace Nat.Web.Controls.HistoryFilters
{
    /// <summary>
    /// Фильтрация историчного журнала по флагу удаленых. Т.е. не нужные запси помечаны флагом удаленные.
    /// </summary>
    public static class FilterDelHistory
    {
        /// <summary>
        /// Фильтрация истории, исключая запись с ключом id. Если поле showHistory == true, то фильтрация не используется
        /// </summary>
        /// <typeparam name="TSource">Тип</typeparam>
        /// <param name="source">Журнал для фильтрации</param>
        /// <param name="showHistory">Показывать ли историчные данные</param>
        /// <param name="id">Ключ</param>
        /// <returns>Результат фильтрации</returns>
        public static IQueryable<TSource> FDelHistory<TSource>(this IQueryable<TSource> source, bool showHistory, long id) where TSource : class, IFilterDelHistory
        {
            if (showHistory) return source;

            return source.Where(r => !r.isDel || r.id == id);
        }

        /// <summary>
        /// Фильтрация истории, исключая запись с ключом id.
        /// </summary>
        /// <typeparam name="TSource">Тип</typeparam>
        /// <param name="source">Журнал для фильтрации</param>
        /// <param name="id">Ключ</param>
        /// <returns>Результат фильтрации</returns>
        public static IQueryable<TSource> FDelHistory<TSource>(this IQueryable<TSource> source, long id) where TSource : class, IFilterDelHistory
        {

            return source.Where(r => !r.isDel || r.id == id);
        }

        /// <summary>
        /// Фильтрация истории, исключая запись с ключей ids. Если поле showHistory == true, то фильтрация не используется
        /// </summary>
        /// <typeparam name="TSource">Тип</typeparam>
        /// <param name="source">Журнал для фильтрации</param>
        /// <param name="showHistory">Показывать ли историчные данные</param>
        /// <param name="ids">Ключи</param>
        /// <returns>Результат фильтрации</returns>
        public static IQueryable<TSource> FDelHistory<TSource>(this IQueryable<TSource> source, bool showHistory, params long[] ids) where TSource : class, IFilterDelHistory
        {
            if (showHistory) return source;

            return source.Where(r => !r.isDel || ids.Contains(r.id));
        }

        /// <summary>
        /// Фильтрация истории, исключая запись с ключей ids.
        /// </summary>
        /// <typeparam name="TSource">Тип</typeparam>
        /// <param name="source">Журнал для фильтрации</param>
        /// <param name="ids">Ключи</param>
        /// <returns>Результат фильтрации</returns>
        public static IQueryable<TSource> FDelHistory<TSource>(this IQueryable<TSource> source, params long[] ids) where TSource : class, IFilterDelHistory
        {

            return source.Where(r => !r.isDel || ids.Contains(r.id));
        }
        
        /// <summary>
        /// Фильтрация истории. Если поле showHistory == true, то фильтрация не используется
        /// </summary>
        /// <typeparam name="TSource">Тип</typeparam>
        /// <param name="source">Журнал для фильтрации</param>
        /// <param name="showHistory">Показывать ли историчные данные</param>
        /// <returns>Результат фильтрации</returns>
        public static IQueryable<TSource> FDelHistory<TSource>(this IQueryable<TSource> source, bool showHistory) where TSource : class, IFilterDelHistory
        {
            if (showHistory) return source;

            return source.Where(r => !r.isDel);
        }

        /// <summary>
        /// Фильтрация истории.
        /// </summary>
        /// <typeparam name="TSource">Тип</typeparam>
        /// <param name="source">Журнал для фильтрации</param>
        /// <returns>Результат фильтрации</returns>
        public static IQueryable<TSource> FDelHistory<TSource>(this IQueryable<TSource> source) where TSource : class, IFilterDelHistory
        {
            return source.Where(r => !r.isDel);
        }

        /// <summary>
        /// Найти ключ текущей записи, по историцной записи
        /// </summary>
        /// <typeparam name="TSource">Тип</typeparam>
        /// <param name="source">Журнал для фильтрации</param>
        /// <param name="row">Строка оригинал</param>
        /// <returns>Идентификатор записи</returns>
        public static long? FDelHistoryGetKey<TSource>(this IQueryable<TSource> source, TSource row) where TSource : class, IFilterDelHistory
        {

            var key = source.Where(r => !r.isDel && r.refHistory == (row.refHistory ?? row.id)).
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
        public static IQueryable<TSource> FDelHistoryGetHistory<TSource>(this IQueryable<TSource> source, DataContext db, long id) where TSource : class, IFilterDelHistory
        {
            var result =
                from r in source
                let key = db.GetTable<TSource>().Where(k => k.id == id).Select(k => k.refHistory ?? k.id).First()
                where r.refHistory == key
                select r;
            return result;
        }

        /// <summary>
        /// Фильтрация истории.
        /// </summary>
        /// <typeparam name="TSource">Тип</typeparam>
        /// <param name="source">Журнал для фильтрации</param>
        /// <returns>Результат фильтрации</returns>
        public static Expression<Func<TSource, bool>> FDelHistoryGetExpression<TSource>() where TSource : class, IFilterDelHistory
        {
            return r => !r.isDel;
        }

        /// <summary>
        /// Фильтрация истории.
        /// </summary>
        /// <typeparam name="TSource">Тип</typeparam>
        /// <param name="ids">Ключи</param>
        /// <returns>Результат фильтрации</returns>
        public static Expression<Func<TSource, bool>> FDelHistoryGetExpression<TSource>(params long[] ids) where TSource : class, IFilterDelHistory
        {
            return r => !r.isDel || ids.Contains(r.id);
        }

        /// <summary>
        /// Фильтрация истории.
        /// </summary>
        /// <typeparam name="TSource">Тип</typeparam>
        /// <param name="id">Ключ</param>
        /// <returns>Результат фильтрации</returns>
        public static Expression<Func<TSource, bool>> FDelHistoryGetExpression<TSource>(long id) where TSource : class, IFilterDelHistory
        {
            return r => !r.isDel || r.id == id;
        }

        public static Expression<Func<TSource, bool>> FDelHistoryCanEditDeleteExpression<TSource>()
            where TSource : class, IFilterDelHistory
        {
            return item => !item.isDel;
        }

        public static Expression<Func<TSource, bool>> FDelHistoryGetHistoryExpression<TSource>(DataContext db, long id) where TSource : class, IFilterDelHistory
        {
            return item => db.GetTable<TSource>().Where(k => k.id == id).Select(k => k.refHistory ?? k.id).FirstOrDefault() == (item.refHistory ?? item.id);
        }

        /*
                /// <summary>
        /// Фильтрация истории, исключая запись с ключом id. Если поле showHistory == true, то фильтрация не используется
        /// </summary>
        /// <typeparam name="TSource">Тип</typeparam>
        /// <param name="source">Журнал для фильтрации</param>
        /// <param name="showHistory">Показывать ли историчные данные</param>
        /// <param name="id">Ключ</param>
        /// <returns>Результат фильтрации</returns>
        public static IQueryable<TSource> FDelHistory<TSource>(this IEnumerable<TSource> source, bool showHistory, long id) where TSource : class, IFilterDelHistory
        {
            if (showHistory) return (IQueryable<TSource>)source;

            return ((IQueryable<TSource>)source).Where(r => !r.isDel || r.id == id);
        }

        /// <summary>
        /// Фильтрация истории, исключая запись с ключом id.
        /// </summary>
        /// <typeparam name="TSource">Тип</typeparam>
        /// <param name="source">Журнал для фильтрации</param>
        /// <param name="id">Ключ</param>
        /// <returns>Результат фильтрации</returns>
        public static IQueryable<TSource> FDelHistory<TSource>(this IEnumerable<TSource> source, long id) where TSource : class, IFilterDelHistory
        {

            return ((IQueryable<TSource>)source).Where(r => !r.isDel || r.id == id);
        }

        /// <summary>
        /// Фильтрация истории, исключая запись с ключей ids. Если поле showHistory == true, то фильтрация не используется
        /// </summary>
        /// <typeparam name="TSource">Тип</typeparam>
        /// <param name="source">Журнал для фильтрации</param>
        /// <param name="showHistory">Показывать ли историчные данные</param>
        /// <param name="ids">Ключи</param>
        /// <returns>Результат фильтрации</returns>
        public static IQueryable<TSource> FDelHistory<TSource>(this IEnumerable<TSource> source, bool showHistory, params long[] ids) where TSource : class, IFilterDelHistory
        {
            if (showHistory) return (IQueryable<TSource>)source;

            return ((IQueryable<TSource>)source).Where(r => !r.isDel || ids.Contains(r.id));
        }

        /// <summary>
        /// Фильтрация истории, исключая запись с ключей ids.
        /// </summary>
        /// <typeparam name="TSource">Тип</typeparam>
        /// <param name="source">Журнал для фильтрации</param>
        /// <param name="ids">Ключи</param>
        /// <returns>Результат фильтрации</returns>
        public static IQueryable<TSource> FDelHistory<TSource>(this IEnumerable<TSource> source, params long[] ids) where TSource : class, IFilterDelHistory
        {

            return ((IQueryable<TSource>)source).Where(r => !r.isDel || ids.Contains(r.id));
        }
        
        /// <summary>
        /// Фильтрация истории. Если поле showHistory == true, то фильтрация не используется
        /// </summary>
        /// <typeparam name="TSource">Тип</typeparam>
        /// <param name="source">Журнал для фильтрации</param>
        /// <param name="showHistory">Показывать ли историчные данные</param>
        /// <returns>Результат фильтрации</returns>
        public static IQueryable<TSource> FDelHistory<TSource>(this IEnumerable<TSource> source, bool showHistory) where TSource : class, IFilterDelHistory
        {
            if (showHistory) return (IQueryable<TSource>)source;

            return ((IQueryable<TSource>)source).Where(r => !r.isDel);
        }

        /// <summary>
        /// Фильтрация истории.
        /// </summary>
        /// <typeparam name="TSource">Тип</typeparam>
        /// <param name="source">Журнал для фильтрации</param>
        /// <returns>Результат фильтрации</returns>
        public static IQueryable<TSource> FDelHistory<TSource>(this IEnumerable<TSource> source) where TSource : class, IFilterDelHistory
        {

            return ((IQueryable<TSource>)source).Where(r => !r.isDel);
        }

        /// <summary>
        /// Найти ключ текущей записи, по историцной записи
        /// </summary>
        /// <typeparam name="TSource">Тип</typeparam>
        /// <param name="source">Журнал для фильтрации</param>
        /// <param name="row">Строка оригинал</param>
        /// <returns>Идентификатор записи</returns>
        public static long? FDelHistoryGetKey<TSource>(this IEnumerable<TSource> source, TSource row) where TSource : class, IFilterDelHistory
        {

            var key = ((IQueryable<TSource>)source).Where(r => !r.isDel && r.refHistory == (row.refHistory ?? row.id)).
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
        public static IQueryable<TSource> FDelHistoryGetHistory<TSource>(this IEnumerable<TSource> source, DataContext db, long id) where TSource : class, IFilterDelHistory
        {
            var result =
                from r in source
                let key = db.GetTable<TSource>().Where(k => k.id == id).Select(k => k.refHistory ?? k.id).First()
                where r.refHistory == key
                select r;
            return (IQueryable<TSource>)result;
        }
        */
    }

    /// <summary>
    /// Интерфейс, для таблицы с историей.
    /// </summary>
    public interface IFilterDelHistory
    {
        long id { get; }
        bool isDel { get; }
        long? refHistory { get; }
    }
}