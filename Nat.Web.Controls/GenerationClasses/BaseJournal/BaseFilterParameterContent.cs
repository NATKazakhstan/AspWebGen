using System;
using System.Linq;
using System.Linq.Expressions;
using System.Data.Linq;

using Nat.Web.Controls.GenerationClasses.Filter;

namespace Nat.Web.Controls.GenerationClasses.BaseJournal
{
    /// <summary>
    /// Абстрактный класс для полнотекстового поиска используется в BaseDataSourceView
    /// </summary>
    /// <typeparam name="TTable">Основная таблица выборки</typeparam>
    public abstract class BaseFilterParameterContent<TTable> : BaseFilterParameter<TTable>
        where TTable : class 
    {
        protected override Type FieldType
        {
            get { return typeof (string); }
        }
        /// <summary>
        /// составляет выражение для полнотекстового поиска со связкой с основной таблицей
        /// </summary>
        /// <param name="query">выражение выборки основной таблицы</param>
        /// <param name="qParams">параметры окружения</param>
        /// <param name="searchText">Текст для поиска</param>
        /// <returns>поисковая выборка со связкой с основной таблицей</returns>
        public abstract Expression GetJoinExpression(Expression query, QueryParameters qParams, String searchText);

    }
    /// <summary>
    /// Вспомогательный класс для объединения запросов при полнотекстовом поиске 
    /// </summary>
    /// <typeparam name="TTable">Основная таблица</typeparam>
    /// <typeparam name="TJoinTable">Таблица с результатами поиска</typeparam>
    public class BaseFilterParameterHolder<TTable, TJoinTable>
    {
        public TTable master;
        public TJoinTable joined;

        public BaseFilterParameterHolder(){}
        public BaseFilterParameterHolder(TTable master, TJoinTable joined)
        {
            this.master = master;
            this.joined = joined;
        }
    }
    /// <summary>
    /// Класс для параметра фильтра поиск по телу документа
    /// </summary>
    /// <typeparam name="TTable">Основная таблица</typeparam>
    /// <typeparam name="TJoinTable">Результирующая таблица для сцепления</typeparam>
    /// <typeparam name="TField">Тип полей для сцепления</typeparam>
    /// <typeparam name="TDataContext">Для определения коннекта с бд</typeparam>
    /// <typeparam name="TSortField">Для определения коннекта с бд</typeparam>
    public class BaseFilterParameterContent<TDataContext, TTable, TJoinTable, TField, TSortField> : BaseFilterParameterContent<TTable>
        where TTable : class 
        where TJoinTable : class
        where TField : struct
        where TSortField : struct
        where TDataContext : DataContext
    {
        public BaseFilterParameterContent()
        {
        }

        public BaseFilterParameterContent(Expression<Func<TDataContext, String, IQueryable<TJoinTable>>> funcName, Expression<Func<TTable, TField?>> valueExprMain, Expression<Func<TJoinTable, TField?>> valueExprJoin, Expression<Func<BaseFilterParameterHolder<TTable, TJoinTable>, TSortField?>> sortExpression)
        {
            FuncName = funcName;
            ValueExprMain = valueExprMain;
            ValueExprJoin = valueExprJoin;
            SortExpression = sortExpression;
            //JoinFilter.FilterData
        }
        /// <summary>
        /// переопределяем функцию чтоб не работали стандартный фильтры, используется связка таблиц, а не фильтрация полей
        /// </summary>
        /// <param name="filtertype"></param>
        /// <param name="filterItem"></param>
        /// <param name="qParams"></param>
        /// <returns></returns>
        protected internal override Expression OnFilter(Enum filtertype, FilterItem filterItem, QueryParameters qParams)
        {
            return null;
        }
        /// <summary>
        /// Поле для основной таблицы для связки
        /// </summary>
        public Expression<Func<TTable, TField?>> ValueExprMain{ get; private set; }
        /// <summary>
        /// Поле поисковой функции для связки с основной таблицей
        /// </summary>
        public Expression<Func<TJoinTable, TField?>> ValueExprJoin { get; private set; }
        /// <summary>
        /// ссылка на функцию, которая обеспечивает полнотекстовый поиск
        /// </summary>
        public Expression<Func<TDataContext, String, IQueryable<TJoinTable>>> FuncName { get; private set; }
        /// <summary>
        /// поле для сортировка по релевантности полнотекстового поиска
        /// </summary>
        public Expression<Func<BaseFilterParameterHolder<TTable, TJoinTable>, TSortField?>> SortExpression { get; private set; }

        /// <summary>
        /// составляет выражение для полнотекстового поиска со связкой с основной таблицей
        /// </summary>
        /// <param name="query">выражение выборки основной таблицы</param>
        /// <param name="qParams">параметры окружения</param>
        /// <param name="searchText">Текст для поиска</param>
        /// <returns>поисковая выборка со связкой с основной таблицей</returns>
        public override Expression GetJoinExpression(Expression query, QueryParameters qParams, String searchText)
        {
            /* ASM_ConclusionCards.Join (
                  Func(s), 
                  c => c.ID, 
                  p => p.card_id, 
                  (c, p) => new {c,p}
               ).OrderByDescending(t=>t.p.rank).Select(t=>t.c)
             */
            //var dbParameterExpression = qParams.DBParameterExpression;
            // проставляем информацию о коннекте с бд
            var dbParameterExpression = qParams.GetDBExpression<TDataContext>();
            //проставляем текст для поиска
            var searchExpression = qParams.GetExpression("searchText", searchText);
            // обеспечиваем вызов функции полнтекстового поиска
            var searchFuncExpression = Expression.Invoke(FuncName, dbParameterExpression, searchExpression);
            // оборачиваем выборку в промежуточный тип, для обеспечения сортировки
            //Expression<Func<TTable, TJoinTable, BaseFilterParameterHolder<TTable,TJoinTable>>> resultSelector = (t, it) => new BaseFilterParameterHolder<TTable, TJoinTable>(t,it);
            Expression<Func<TTable, TJoinTable, BaseFilterParameterHolder<TTable,TJoinTable>>> resultSelector = (t, it) => new BaseFilterParameterHolder<TTable, TJoinTable>{ master = t, joined = it};
            //выражение для возвращения оригинального запроса
            Expression<Func<BaseFilterParameterHolder<TTable, TJoinTable>,TTable>> selectResultSelector = t => t.master;
            //соединяем основную выборку с полнотекстовым поиском
            var joinExpression =  Expression.Call(typeof(Queryable), "Join"
                , new[] { typeof(TTable), typeof(TJoinTable), typeof(TField?), typeof(BaseFilterParameterHolder<TTable, TJoinTable>) }
                , query, searchFuncExpression, ValueExprMain, ValueExprJoin, resultSelector);
            // сортируем полученную выборку по релевантности
            var callSortExpression = Expression.Call(
                typeof (Queryable),
                "OrderByDescending",
                new[] {typeof (BaseFilterParameterHolder<TTable, TJoinTable>), typeof (TSortField?)},
                joinExpression,
                SortExpression);
            //возвращаем основную таблицу
            var selectExpression = Expression.Call(
                typeof(Queryable),
                "Select",
                new[] { typeof(BaseFilterParameterHolder<TTable, TJoinTable>), typeof(TTable) },
                callSortExpression,
                selectResultSelector);
            return selectExpression;
        }
    }
}