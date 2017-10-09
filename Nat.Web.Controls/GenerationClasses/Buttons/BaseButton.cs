using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel;
using Nat.Web.Tools.Security;

namespace Nat.Web.Controls.GenerationClasses.Buttons
{
    public class BaseButton<T, TKey, TEdit, TJournal> : IButton
        where T : struct
        where TKey : struct
        where TEdit : AbstractUserControl<TKey>
        where TJournal : AbstractUserControl<TKey>
    {
        private bool? _visible;
        private object _context;

        /// <summary>
        /// Делегат выполняющий проверку необходимости добавлять кнопку на форму
        /// </summary>
        [Description("Делегат выполняющий проверку необходимости добавлять кнопку на форму")]
        public EventHandler<BaseButtonVisibleEventArgs<TKey, TEdit, TJournal>> SetVisible { get; set; }
        /// <summary>
        /// Делегат создания кнопки
        /// </summary>
        [Description("Делегат создания кнопки")]
        public EventHandler<BaseButtonEventArgs<TKey, TEdit, TJournal>> CreateButton { get; set; }
        /// <summary>
        /// Делегат исполнения действия кнопки
        /// </summary>
        [Description("Делегат исполнения действия кнопки")]
        public EventHandler<BaseButtonExecuteEventArgs<T>> Execute { get; set; }

        #region IButton Members

        /// <summary>
        /// Уникальное имя кнопки
        /// </summary>
        [Description("Уникальное имя кнопки")]
        public string Name { get; set; }

        /// <summary>
        /// Список прав позволяющий выполнять действие кнопки
        /// </summary>
        [Description("Список прав позволяющий выполнять действие кнопки")]
        public string[] Roles { get; set; }

        /// <summary>
        /// Видимость кнопки
        /// </summary>
        [Description("Видимость кнопки")]
        public bool Visible
        {
            get { return _visible ?? GetVisible(); }
        }

        protected virtual bool GetVisible()
        {
            var args = new BaseButtonVisibleEventArgs<TKey, TEdit, TJournal>(_context);
            if (SetVisible != null) SetVisible(this, args);
            _visible = args.Visible;
            return _visible.Value;
        }

        public virtual void AddButton(AdditionalButtons buttons, object context)
        {
            _context = context;
            if (!UserRoles.IsInAnyRoles(Roles) || !Visible) return;
            if (CreateButton != null)
            {
                var args = new BaseButtonEventArgs<TKey, TEdit, TJournal>(buttons, context);
                buttons.CurrentArgument = Name + ":";
                CreateButton(this, args);
                buttons.CurrentArgument = null;
            }
        }

        void IButton.Execute(string value, object context)
        {
            _context = context;
            if (!UserRoles.IsInAnyRoles(Roles) || !Visible) return;
            BaseButtonExecuteEventArgs<T> args;
            if (string.IsNullOrEmpty(value))
            {
                args = new BaseButtonExecuteEventArgs<T>(null);
            }
            else
            {
                var t = (T)Convert.ChangeType(value, typeof(T));
                args = new BaseButtonExecuteEventArgs<T>(t);
            }
            if (Execute != null) Execute(this, args);
        }

        #endregion
    }
}
