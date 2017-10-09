using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Nat.Web.Controls.GenerationClasses.Navigator
{
    public class BaseNavigatorInfoVisibleEventArgs : EventArgs
    {
        public BaseNavigatorInfoVisibleEventArgs()
        {
            Visible = true;
            VisibleLookButton = true;
            VisibleToJournalButton = true;
            VisibleAddButton = true;
            FilterByParent = true;
        }

        public object SelectedValue { get; set; }

        public BaseNavigatorValues Values { get; set; }

        /// <summary>
        /// Видимость ссылки.
        /// </summary>
        public bool Visible { get; set; }
        
        /// <summary>
        /// Видимость ссылки на просмотр записи. 
        /// Для ссылки на родителя или для дочки 1-1
        /// </summary>
        public bool VisibleLookButton { get; set; }
        
        /// <summary>
        /// Видимость на просмотр журнала.
        /// Для ссылки на родителя и на дочки.
        /// </summary>
        public bool VisibleToJournalButton { get; set; }

        /// <summary>
        /// Видимость на просмотр добавления.
        /// Для ссылки на дочки.
        /// </summary>
        public bool VisibleAddButton { get; set; }

        /// <summary>
        /// Фильтровать ли дочернии данные по данной ссылки.
        /// </summary>
        public bool FilterByParent { get; set; }
    }
}
