/*
 * Created by: Sergey V. Shpakovskiy
 * Created: 20 декабря 2008 г.
 * Copyright © JSC New Age Technologies 2008
 */

using System;
using System.Collections.Generic;
using System.Web.UI;
using System.Linq;
using System.Data.Linq;
using System.Linq.Expressions;
using Nat.Web.Controls.GenerationClasses.Navigator;

namespace Nat.Web.Controls.GenerationClasses
{
    public class BaseRecordEvents
    {
        public BaseRecordEvents()
        {
            SelectedValues = new Dictionary<Type, long>();
        }

        public Page Page { get; set; }
        public Control Control { get; set; }

        public IDictionary<Type, long> SelectedValues { get; private set; }

        public virtual void NewRecordAdding(RecordEventArgs args)
        {}
        public virtual void RecordEditing(RecordEventArgs args, long selectedValue)
        {}
        public virtual void RecordDeleting(RecordEventArgs args, long selectedValue)
        {}
        public virtual void RecordEditing(RecordEventArgs args, IDictionary<string,object> selectedValue)
        { }
        public virtual void RecordDeleting(RecordEventArgs args, IDictionary<string, object> selectedValue)
        { }
    }

    public class BaseRecordEvents<T>
    {
        public BaseRecordEvents()
        {
            SelectedValues = new Dictionary<Type, long>();
        }

        public Page Page { get; set; }
        public Control Control { get; set; }

        public IDictionary<Type, long> SelectedValues { get; private set; }

        public virtual void NewRecordAdding(RecordEventArgs args)
        { }
        public virtual void RecordEditing(RecordEventArgs args, T selectedValue)
        { }
        public virtual void RecordDeleting(RecordEventArgs args, T selectedValue)
        { }
    }

    public class BaseRecordEvents<T, TableType> where T : struct where TableType : class
    {
        private static object _lock = new object();
        private static List<RecordEventItem<T, TableType>> expressions;
        public Dictionary<Type, object> Values { get; private set; }

        public BaseRecordEvents()
        {
            SelectedValues = new Dictionary<Type, long>();
            Values = new Dictionary<Type, object>();
        }

        public Page Page { get; set; }
        public Control Control { get; set; }

        public void InitializeValues(IDictionary<Type, object> values)
        {
            foreach (var item in values)
            {
                Values[item.Key] = item.Value;
                var value = item.Value as long?;
                if (value != null)
                    SelectedValues[item.Key] = value.Value;
            }
        }

        /// <summary>
        /// Колекция выбранных записей вверх по иерархии журналов.
        /// </summary>
        public IDictionary<Type, long> SelectedValues { get; private set; }

        /// <summary>
        /// Добавление записи
        /// </summary>
        /// <param name="args"></param>
        public virtual void NewRecordAdding(RecordEventArgs args)
        { }

        /// <summary>
        /// Добавление записи в древовидных таблицах
        /// </summary>
        /// <param name="args"></param>
        /// <param name="refParent">Ссылка на родителя</param>
        public virtual void NewRecordAdding(RecordEventArgs args, T? refParent)
        {
            NewRecordAdding(args);
        }

        /// <summary>
        /// Редактирование записи
        /// </summary>
        /// <param name="args"></param>
        /// <param name="selectedValue">Ключ записи</param>
        public virtual void RecordEditing(RecordEventArgs args, T selectedValue)
        { }

        /// <summary>
        /// Удаление записи
        /// </summary>
        /// <param name="args"></param>
        /// <param name="selectedValue">Ключ записи</param>
        public virtual void RecordDeleting(RecordEventArgs args, T selectedValue)
        { }

        private void InitExpressions()
        {
            lock(_lock)
            {
                if (expressions == null)
                {
                    expressions = new List<RecordEventItem<T, TableType>>();
                    InitializeExpressions();
                    long i = 1;
                    foreach (var item in expressions)
                    {
                        item.Index = i;
                        i = i << 1;
                    }
                }
            }
        }

        protected virtual void InitializeExpressions()
        { }
    }

    public delegate void BaseRecordEventsGetMessageHandler(BaseInformationValues info);

    public class RecordEventItem<T, TableType>
        where T : struct
        where TableType : class
    {
        public BaseRecordEventsGetMessageHandler Handler { get; set; }
        public RecordEventsDeniedMode Mode { get; set; }
        public Expression<Func<TableType, bool>> Expression { get; set; }
        public bool UseInAllChilds { get; set; }
        public long Index { get; set; }
    }

    /// <summary>
    /// Какие запреты нужны по выполнению условий
    /// </summary>
    [Flags]
    public enum RecordEventsDeniedMode
    {
        /// <summary>
        /// Запись не валидна, нелья выбрать из справочника
        /// </summary>
        NotValid = 1,
        /// <summary>
        /// Запись запрещено редактировать
        /// </summary>
        CanNotEdit = 2,
        /// <summary>
        /// Запись запрещено удалять
        /// </summary>
        CanNotDelete = 4,
        /// <summary>
        /// Прятать кнопки на редактирование и удаление
        /// </summary>
        HideButtons = 8,
    }
}