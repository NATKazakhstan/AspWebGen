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
    public static class FilterIsDel
    {
        /// <summary>
        /// Фильтрация истории, исключая запись с ключом id.
        /// </summary>
        /// <typeparam name="TSource">Тип</typeparam>
        /// <param name="source">Журнал для фильтрации</param>
        /// <param name="id">Ключ</param>
        /// <returns>Результат фильтрации</returns>
        public static IQueryable<TSource> FIsDel<TSource>(this IQueryable<TSource> source, long id) where TSource : class, IFilterIsDel
        {
            return source.Where(r => !r.isDel || r.id == id);
        }
        
        /// <summary>
        /// Фильтрация истории, исключая запись с ключей ids.
        /// </summary>
        /// <typeparam name="TSource">Тип</typeparam>
        /// <param name="source">Журнал для фильтрации</param>
        /// <param name="ids">Ключи</param>
        /// <returns>Результат фильтрации</returns>
        public static IQueryable<TSource> FIsDel<TSource>(this IQueryable<TSource> source, params long[] ids) where TSource : class, IFilterIsDel
        {
            return source.Where(r => !r.isDel || ids.Contains(r.id));
        }
       
        /// <summary>
        /// Фильтрация истории.
        /// </summary>
        /// <typeparam name="TSource">Тип</typeparam>
        /// <param name="source">Журнал для фильтрации</param>
        /// <returns>Результат фильтрации</returns>
        public static IQueryable<TSource> FIsDel<TSource>(this IQueryable<TSource> source) where TSource : class, IFilterIsDel
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
        public static long? FIsDelGetKey<TSource>(this IQueryable<TSource> source, TSource row) where TSource : class, IFilterIsDel
        {
            return row.id;
        }

        /// <summary>
        /// Фильтрация истории.
        /// </summary>
        /// <typeparam name="TSource">Тип</typeparam>
        /// <param name="source">Журнал для фильтрации</param>
        /// <returns>Результат фильтрации</returns>
        public static Expression<Func<TSource, bool>> FIsDelGetExpression<TSource>() where TSource : class, IFilterIsDel
        {
            return r => !r.isDel;
        }

        /// <summary>
        /// Фильтрация истории.
        /// </summary>
        /// <typeparam name="TSource">Тип</typeparam>
        /// <param name="source">Журнал для фильтрации</param>
        /// <returns>Результат фильтрации</returns>
        public static Expression<Func<TSource, bool>> FIsDelGetExpression<TSource>(long id) where TSource : class, IFilterIsDel
        {
            return r => !r.isDel || r.id == id;
        }

        /// <summary>
        /// Фильтрация истории.
        /// </summary>
        /// <typeparam name="TSource">Тип</typeparam>
        /// <param name="source">Журнал для фильтрации</param>
        /// <returns>Результат фильтрации</returns>
        public static Expression<Func<TSource, bool>> FIsDelGetExpression<TSource>(params long[] ids) where TSource : class, IFilterIsDel
        {
            return r => !r.isDel || ids.Contains(r.id);
        }

        /// <summary>
        /// Фильтрация истории.
        /// </summary>
        /// <typeparam name="TSource">Тип</typeparam>
        /// <param name="source">Журнал для фильтрации</param>
        /// <returns>Результат фильтрации</returns>
        public static Expression<Func<TSource, bool>> FIsDelHistoryGetExpression<TSource>() where TSource : class, IFilterIsDel
        {
            return r => !r.isDel;
        }

        /// <summary>
        /// Фильтрация истории.
        /// </summary>
        /// <typeparam name="TSource">Тип</typeparam>
        /// <param name="ids">Ключи</param>
        /// <returns>Результат фильтрации</returns>
        public static Expression<Func<TSource, bool>> FIsDelHistoryGetExpression<TSource>(params long[] ids) where TSource : class, IFilterIsDel
        {
            return r => !r.isDel || ids.Contains(r.id);
        }

        /// <summary>
        /// Фильтрация истории.
        /// </summary>
        /// <typeparam name="TSource">Тип</typeparam>
        /// <param name="id">Ключ</param>
        /// <returns>Результат фильтрации</returns>
        public static Expression<Func<TSource, bool>> FIsDelHistoryGetExpression<TSource>(long id) where TSource : class, IFilterIsDel
        {
            return r => !r.isDel || r.id == id;
        }

        public static Expression<Func<TSource, bool>> FIsDelCanEditDeleteExpression<TSource>()
            where TSource : class, IFilterIsDel
        {
            return item => !item.isDel;
        }
    }

    /// <summary>
    /// Интерфейс, для таблицы с историей.
    /// </summary>
    public interface IFilterIsDel
    {
        long id { get; }
        bool isDel { get; }
    }
}