using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Nat.Web.Controls.GenerationClasses.Buttons
{
    public interface IButton
    {
        /// <summary>
        /// Уникальное имя кнопки
        /// </summary>
        string Name { get; }
        /// <summary>
        /// Добавление кнопки
        /// </summary>
        /// <param name="buttons"></param>
        void AddButton(AdditionalButtons buttons, object context);
        /// <summary>
        /// Выполнение действий кнопки
        /// </summary>
        /// <param name="value"></param>
        void Execute(string value, object context);
    }
}
