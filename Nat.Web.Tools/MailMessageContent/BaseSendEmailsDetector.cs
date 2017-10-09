namespace Nat.Web.Tools.MailMessageContent
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Linq.Expressions;

    using Nat.Tools.Specific;

    /// <summary>
    /// Базовый класс для определения адресов кому нужно отправить письмо и кого поставить в копию.
    /// </summary>
    public abstract class BaseSendEmailsDetector
    {
        protected BaseSendEmailsDetector(long refUser, string moduleCode)
        {
            if (string.IsNullOrEmpty(moduleCode))
                throw new ArgumentNullException(nameof(moduleCode));

            ModuleCode = moduleCode;
            this.refUser = refUser;
            EMailsTo = new List<string>();
            EMailsCopy = new List<string>();
        }

        public static ILookup<string, ModuleConfiguration> GetConfigurations(string moduleCode)
        {
            using (var connection = SpecificInstances.DbFactory.CreateConnection())
            using (var db = new DBDataContext(connection))
            {
                return db.MSC_Module_Configurations
                    .Where(r => r.MSC_Module_refModule.Code == moduleCode)
                    .Where(r => r.Enabled)
                    .ToLookup(
                        r => r.MSC_Module_ConfigurationField_refField.Code,
                        r => new ModuleConfiguration
                            {
                                ContentKz = r.ContentKz,
                                ContentRu = r.ContentRu,
                            });
            }
        }

        public enum EventType
        {
            Non,
            Add,
            Change,
            Delete
        }

        public interface ICustomEvent
        {
            /// <summary>
            /// Набор событий. Каждое событие между знаками меньше больше.
            /// </summary>
            string OnEvents { get; }
        }

        public interface IModuleCode
        {
            /// <summary>
            /// Код модуля.
            /// </summary>
            string ModuleCode { get; }
        }

        public interface IEMail
        {
            /// <summary>
            /// Почтовый адрес указать в поле Кому, если указано истина, иначе адрес указывается в поле Копия.
            /// </summary>
            bool ToOrCopy { get; }

            /// <summary>
            /// Почтовый адрес.
            /// </summary>
            string EMail { get; }
        }

        public interface IEvents
        {
            /// <summary>
            /// Отправка почты на событие добавления.
            /// </summary>
            bool OnAdd { get; }

            /// <summary>
            /// Отправка почты на событие изменения.
            /// </summary>
            bool OnChange { get; }

            /// <summary>
            /// Отправка почты на событие удаления.
            /// </summary>
            bool OnDelete { get; }
        }

        /// <summary>
        /// Ключ пользователя.
        /// </summary>
        public long refUser { get; private set; }

        /// <summary>
        /// Код модуля.
        /// </summary>
        public string ModuleCode { get; private set; }

        /// <summary>
        /// Произошедшее событие.
        /// </summary>
        public EventType Event { get; set; }

        /// <summary>
        /// Произошедшее событие особое. События перечисляются через запятую или точку с запятой.
        /// </summary>
        public string CustomEvent { get; set; }

        /// <summary>
        /// Адреса кому нужно отправить письмо.
        /// </summary>
        public List<string> EMailsTo { get; private set; }

        /// <summary>
        /// Адреса кого нужно поставить в копию, при отправке письма.
        /// </summary>
        public List<string> EMailsCopy { get; private set; }

        /// <summary>
        /// Метод определяющий список адресов кому нужно отправить письмо и кого поставить в копию.
        /// </summary>
        /// <returns>Истина, если список кому отправить письмо заполнен, иначе ложь.</returns>
        public abstract bool Detect();

        /// <summary>
        /// Заполнить адреса кому нужно отправить письмо и кого поставить в копию.
        /// </summary>
        /// <typeparam name="TSource">Тип источника данных.</typeparam>
        /// <param name="emails">Набор адресов.</param>
        /// <param name="currentUserEMail">Почтовый адрес текущего пользователя.</param>
        protected virtual void SetEmails<TSource>(List<TSource> emails, string currentUserEMail)
            where TSource : IEMail
        {
            EMailsTo.AddRange(
                emails.Where(r => r.ToOrCopy)
                      .Select(r => r.EMail)
                      .Distinct());
            EMailsCopy.AddRange(
                emails.Where(r => !r.ToOrCopy)
                      .Select(r => r.EMail)
                      .Distinct());
        }

        /// <summary>
        /// Фильтр источника адресов по полю "Произошедшее событие особое".
        /// </summary>
        /// <typeparam name="TSource">Тип источника данных.</typeparam>
        /// <param name="source">Источник данных.</param>
        /// <returns>Фильтрованный источник данных.</returns>
        protected virtual IQueryable<TSource> CustomEventFilter<TSource>(IQueryable<TSource> source)
            where TSource : ICustomEvent
        {
            if (string.IsNullOrEmpty(CustomEvent))
                return source;
            var events = CustomEvent.Split(new[] { ',', ';' }, StringSplitOptions.RemoveEmptyEntries)
                                    .Select(r => "<" + r + ">")
                                    .ToList();
            if (events.Count == 0)
                return source;

            Expression<Func<TSource, string, bool>> filter = (r, value) => r.OnEvents.Contains(value);
            Expression exp = null;
            var param = Expression.Parameter(typeof(TSource), "row");
            foreach (var item in events)
            {
                var expression = Expression.Invoke(filter, param, Expression.Constant(item));
                if (exp == null)
                    exp = expression;
                else
                    exp = Expression.Or(exp, expression);
            }

            if (exp == null)
                return source;

            var lambda = Expression.Lambda<Func<TSource, bool>>(exp, param);
            return source.Where(lambda);
        }

        /// <summary>
        /// Фильтр источника адресов по полю "Код модуля".
        /// </summary>
        /// <typeparam name="TSource">Тип источника данных.</typeparam>
        /// <param name="source">Источник данных.</param>
        /// <returns>Фильтрованный источник данных.</returns>
        protected virtual IQueryable<TSource> ModuleCodeFilter<TSource>(IQueryable<TSource> source)
            where TSource : IModuleCode
        {
            return source.Where(r => r.ModuleCode == ModuleCode);
        }

        /// <summary>
        /// Фильтр источника адресов по полю "Произошедшее событие".
        /// </summary>
        /// <typeparam name="TSource">Тип источника данных.</typeparam>
        /// <param name="source">Источник данных.</param>
        /// <returns>Фильтрованный источник данных.</returns>
        protected virtual IQueryable<TSource> EventFilter<TSource>(IQueryable<TSource> source)
            where TSource : IEvents
        {
            switch (Event)
            {
                case EventType.Non:
                    return source;
                case EventType.Add:
                    return source.Where(r => r.OnAdd);
                case EventType.Change:
                    return source.Where(r => r.OnChange);
                case EventType.Delete:
                    return source.Where(r => r.OnDelete);
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        protected class EMailInformation : IEMail
        {
            public bool ToOrCopy { get; set; }

            public string EMail { get; set; }
        }
    }
}