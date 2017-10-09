namespace Nat.Web.Controls
{
    public enum LogMessageType : long
    {
        None = 0,

        /// <summary>
        /// Системные логи, Данные серилизации
        /// Запрос страницы
        /// </summary>
        SystemSerializationPageRequest = 149,

        /// <summary>
        /// Система
        /// Ошибка в ПО
        /// </summary>
        SystemErrorInApp = 150,

        /// <summary>
        /// Системные, Задачи
        /// Действие
        /// </summary>
        SystemJobsActions = 700,

        /// <summary>
        /// Системные, Табличные представления
        /// Сохранение настроек
        /// </summary>
        SystemRVSSettingsSaveSettings = 701,

        /// <summary>
        /// Системные, Табличные представления
        /// Открытие сохраненных настроек
        /// </summary>
        SystemRVSSettingsLoadSavedSettings = 702,

        /// <summary>
        /// Системные, Табличные представления
        /// Экспорт
        /// </summary>
        SystemRVSSettingsExport = 703,

        /// <summary>
        /// Системные, Табличные представления
        /// Отсутствуют права
        /// </summary>
        SystemRVSSettingsDeniedAccess = 704,

        /// <summary>
        /// Системные, Табличные представления
        /// Просмотр журнала
        /// </summary>
        SystemRVSSettingsView = 705,
        
        /// <summary>
        /// Системные, Сообщения об отправке почты
        /// Отправка почты
        /// SendMail (720)
        /// </summary>
        SystemMailSendMailInformation = 720,

        /// <summary>
        /// Системные, Сообщения об отправке почты
        /// Отправка почты
        /// ErrorOnSend (721)
        /// </summary>
        SystemMailSendMailError = 721,
    }
}