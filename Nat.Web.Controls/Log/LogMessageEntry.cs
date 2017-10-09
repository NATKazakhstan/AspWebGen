/*
 * Created by : Daniil Kovalev
 * Created    : 07.02.2008
 */

using System;
using Nat.Web.Tools;
using Nat.Web.Controls.Data;

namespace Nat.Web.Controls
{
    [Serializable]
    public class LogMessageEntry : ILogMessageEntry
    {
        private long? _refRvsProperties;

        #region Constructors

        public LogMessageEntry()
        {
            DateTime = DateTime.Now;
        }

        public LogMessageEntry(string sid, LogMessageType messageCode, String message, long refRVSProperties)
            : this()
        {
            Sid = sid;
            MessageCode = messageCode;
            Message = message;
            RefRVSProperties = refRVSProperties;
        }

        public LogMessageEntry(string sid, LogMessageType messageCode, String message, RvsSavedProperties savedProperties)
            : this()
        {
            Sid = sid;
            MessageCode = messageCode;
            Message = message;
            SavedProperties = savedProperties;
        }

        public LogMessageEntry(LogMessageType messageCode, String message, RvsSavedProperties savedProperties)
            : this()
        {
            MessageCode = messageCode;
            Message = message;
            SavedProperties = savedProperties;
        }

        public LogMessageEntry(string sid, LogMessageType messageCode, String message)
            : this()
        {
            Sid = sid;
            MessageCode = messageCode;
            Message = message;
        }

        public LogMessageEntry(LogMessageType messageCode, String message)
            : this()
        {
            MessageCode = messageCode;
            Message = message;
        }


        public LogMessageEntry(string sid, long messageCode, String message)
            : this()
        {
            Sid = sid;
            MessageCodeAsLong = messageCode;
            Message = message;
        }

        #endregion

        #region Methods

        public override String ToString()
        {
            return String.Format("{0} {1} {2} {3} {4}\n",
                                 MessageCode, Message, DateTime, Sid, RefRVSProperties);
        }

        #endregion

        #region Properties

        protected RvsSavedProperties SavedProperties { get; set; }

        #endregion

        #region ILogMessageEntry Members

        public string Message { get; set; }
        public long MessageCodeAsLong { get; set; }
        public DateTime DateTime { get; set; }
        public string Sid { get; set; }

        public LogMessageType MessageCode
        {
            get { return (LogMessageType)MessageCodeAsLong; }
            set { MessageCodeAsLong = (long)value; }
        }

        public long? RefRVSProperties
        {
            get
            {
                if (_refRvsProperties == null && SavedProperties != null)
                    _refRvsProperties = SavedProperties.Save();
                return _refRvsProperties;
            }
            set { _refRvsProperties = value; }
        }


        #endregion
    }
}