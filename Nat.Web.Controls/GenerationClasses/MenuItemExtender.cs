namespace Nat.Web.Controls.GenerationClasses
{
    using System;

    public static class MenuItemExtender
    {
        public static string IdRecParameter = "@idrec";

        /// <summary>
        /// Добавить в меню элемент с именем menuName, в который будет добавлено два элемента "На русском" и "На казахском".
        /// Url у кнопок формируется вызовом функции параметра getUrlForKz.
        /// </summary>
        /// <param name="buttons">Куда добавить кнопки.</param>
        /// <param name="menuName">Название меню.</param>
        /// <param name="getUrlForKz">Функция возвращающая url для кнопок, при параметре true для казахского языка, false для русского языка.</param>
        public static void AddMenuRuKz(this AdditionalButtons buttons, string menuName, Func<bool, string> getUrlForKz)
        {
            var menuItem = new MenuItem { Text = menuName };
            menuItem.Items.Add(
                new MenuItem
                    {
                        Url = getUrlForKz(false),
                        Text = Properties.Resources.SOnRu,
                    });
            menuItem.Items.Add(
                new MenuItem
                    {
                        Url = getUrlForKz(true),
                        Text = Properties.Resources.SOnKz,
                    });
            buttons.AddMenuItem(menuItem);
        }

        /// <summary>
        /// Добавить в меню элемент с именем menuName, в который будет добавлено два элемента "На русском" и "На казахском".
        /// Url у кнопок формируется вызовом функции параметра getUrlForKz.
        /// Будет формироваться url для формирования печатной формы по нескольким выбранным записям.
        /// В url добавить «@idrec» в то место где должны подставиться id выбранных записей.
        /// </summary>
        /// <param name="buttons">Куда добавить кнопки.</param>
        /// <param name="menuName">Название меню.</param>
        /// <param name="getUrlForKz">Функция возвращающая url для кнопок, при параметре true для казахского языка, false для русского языка. Для передачи выбранных id использовать строку «@idrec».</param>
        public static void AddDownloadMenuRuKzForSelected(this AdditionalButtons buttons, string menuName, Func<bool, string> getUrlForKz)
        {
            var menuItem = new MenuItem { Text = menuName };
            var onclick = string.Format(
                "DownloadBySelected($(this).attr('url'), '{1}', '{0}'); return false; ",
                Properties.Resources.SNoOneSelectedRow,
                IdRecParameter);
            menuItem.Items.Add(
                new MenuItem
                    {
                        Url = "javascript:void(0);",
                        AttributeUrl = getUrlForKz(false),
                        OnClick = onclick,
                        Text = Properties.Resources.SOnRu,
                    });
            menuItem.Items.Add(
                new MenuItem
                    {
                        Url = "javascript:void(0);",
                        AttributeUrl = getUrlForKz(true),
                        OnClick = onclick,
                        Text = Properties.Resources.SOnKz,
                    });
            buttons.AddMenuItem(menuItem);
        }
    }
}