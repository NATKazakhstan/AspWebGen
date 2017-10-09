using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Nat.Web.Controls.GenerationClasses
{
    public class DeleteEventArgs<TKey> : EventArgs
        where TKey : struct
    {
        /// <summary>
        /// Влаг о том что запись удалена и стандартный механизм удаления не отрабатывает.
        /// </summary>
        public bool IsDeleted { get; set; }
        /// <summary>
        /// Запись на которую нужно перейти после удаления записи.
        /// </summary>
        public TKey? NewSelectedValue { get; set; }
        /// <summary>
        /// Если журнал древовидный, и данный флаг True, то запись будет развернута в дереве.
        /// </summary>
        public bool IsParentValue { get; set; }
    }
}
