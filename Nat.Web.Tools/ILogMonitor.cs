/*
 * Created by: Serik Z. Zhunussov
 * Created: 01 сентября 2010 г.
 * Copyright © JSC New Age Technologies 2008
 */

using System;

namespace Nat.Web.Tools
{
    
    public interface ILogMonitor
    {
        string Sid { get; set; }
        void Init();
        void Log(ILogMessageEntry logMessageEntry);
        void Log(long messageCode, Func<ILogMessageEntry> log);
        void FieldChanged(string rowEntity, string fieldName, object oldValue, object newValue);

        long? WriteLog(ILogMessageEntry logMessageEntry);
        long? WriteLog(long messageCode, Func<ILogMessageEntry> log);
        void WriteFieldChanged(long refMessage, string rowEntity, string fieldName, object oldValue, object newValue);
        void WriteFieldChanged(long refMessage);

        /// <summary>
        /// Результат логирования ссылка на лог в журнале событий.
        /// </summary>
        /// <param name="e">Произошедшее исключение, которое нужно поместить в журнал событий.</param>
        /// <returns>Ссылка на запись журнала событий.</returns>
        string LogException(Exception e);

        /// <summary>
        /// Результат логирования ссылка на лог в журнале событий.
        /// </summary>
        /// <param name="e">Произошедшее исключение, которое нужно поместить в журнал событий.</param>
        /// <param name="sid">SID пользователя от имени которого нужно произвести логирование.</param>
        /// <returns>Ссылка на запись журнала событий.</returns>
        string LogException(Exception e, string sid);
    }
}
