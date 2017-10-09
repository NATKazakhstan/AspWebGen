using System.Collections.Generic;
using System.Drawing;
using System.Web.UI;
using Nat.ReportManager.QueryGeneration;
using Nat.Web.ReportManager.CustomExport;

namespace Nat.Web.ReportManager
{
    public interface IWebReportPlugin : IReportPlugin
    {
        /// <summary>
        /// Права доступа.
        /// </summary>
        /// <returns>Список ролей, которым доступен отчет.</returns>
        string[] Roles();

        /// <summary>
        /// Выбранное значение по умолчанию. 
        /// К примеру, отчет для персоны и переход на отчеты из персон, 
        /// то поумолчанию считается id персоны
        /// </summary>
        string DefaultValue { set; }

        /// <summary>
        /// Страница на которой генерируется отчет.
        /// </summary>
        Page Page { get; set; }

        /// <summary>
        /// Получение констант, при создании отчета.
        /// </summary>
        Dictionary<string, object> Constants { get; set; }

        /// <summary>
        /// Инициализировать значения невидимых условий отчета, сохраненными значениями с прошлого формирования отчета.
        /// </summary>
        bool InitSavedValuesInvisibleConditions { get; }

        /// <summary>
        /// Сохранять значения условий формирования отчета.
        /// </summary>
        /// <remarks>если false, то InitSavedValuesInvisibleConditions считается тоже false</remarks>
        bool AllowSaveValuesConditions { get; }

        string ImageUrl { get; }

        CustomExportType CustomExportType { get; }
    }
}