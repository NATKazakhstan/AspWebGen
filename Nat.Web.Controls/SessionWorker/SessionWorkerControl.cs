using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.IO;
using System.Reflection;
using System.Runtime.Serialization.Formatters.Binary;
using System.Web.Compilation;
using System.Web.UI;
using System.Linq;
using Nat.Web.Tools;

namespace Nat.Web.Controls
{
    public class SessionWorkerControl : Control
    {
        private bool clearSessionOnNotIsPostBack;
        private string key = "";
        private SessionWorker sessionWorker;
        private string typeName = "";


        /// <summary>
        /// Ёкземпл€р SessionWorker.
        /// </summary>
        [Browsable(false)]
        public SessionWorker SessionWorker
        {
            get
            {
                if (sessionWorker == null) sessionWorker = InitSessionWorker();
                return sessionWorker;
            }
        }

        /// <summary>
        /// ќчистка данных сессии, если страница загружена не после PostBack.
        /// </summary>
        [DefaultValue(false)]
        [Category("Behavior")]
        public bool ClearSessionOnNotIsPostBack
        {
            get { return clearSessionOnNotIsPostBack; }
            set { clearSessionOnNotIsPostBack = value; }
        }

        [DefaultValue("")]
        [TypeConverter(typeof (SessionWorkerTypeNameTypeEditor))]
        [Category("Behavior")]
        public string TypeName
        {
            get { return typeName; }
            set { typeName = value; }
        }

        [DefaultValue("")]
        [Category("Behavior")]
        public string Key
        {
            get { return key; }
            set { key = value; }
        }

        protected override void OnInit(EventArgs e)
        {
            base.OnInit(e);
            Page.PreLoad += Page_OnPreLoad;
        }

        private void Page_OnPreLoad(object sender, EventArgs e)
        {
            if (clearSessionOnNotIsPostBack && !Page.IsPostBack)
            {
                SessionWorker.RemoveObject();
                sessionWorker = null;
            }
        }

        public event EventHandler<SessionWorkerArgs> SessionWorkerInit;

        protected SessionWorker InitSessionWorker()
        {
            SessionWorkerArgs args = new SessionWorkerArgs();
            OnSessionWorkerInit(args);
            if (args.SessionWorker == null)
            {
                SessionWorker worker = new SessionWorker(Page, Key);
                if (!string.IsNullOrEmpty(typeName))
                {
                    Type type = BuildManager.GetType(typeName, false, true);
                    if (type != null)
                    {
                        ConstructorInfo info = type.GetConstructor(new Type[] {});
                        if (info == null)
                            throw new Exception(string.Format("“ип '{0}' не содержит пустого конструктора. ({1})",
                                                              type.FullName, ID));
                        worker.Object = info.Invoke(new object[0]);
                    }
                }
                else throw new Exception("SessionWorker не инициализирован");
                return worker;
            }

            if (string.IsNullOrEmpty(args.SessionWorker.Key))
                args.SessionWorker.Key = Key;
            if (args.SessionWorker.Session == null)
                args.SessionWorker.SetSession(Page.Session);
            return args.SessionWorker;
        }

        protected virtual void OnSessionWorkerInit(SessionWorkerArgs args)
        {
            if (SessionWorkerInit != null) SessionWorkerInit(this, args);
        }

        public override void RenderControl(HtmlTextWriter writer)
        {
            if (DesignMode) writer.Write(ID);
        }

        public void RefreshSessionWorker()
        {
            sessionWorker = null;
            InitSessionWorker();
        }

        protected override void OnUnload(EventArgs e)
        {
            base.OnUnload(e);
            var page = Page as BaseSPPage;
            if (page != null && page.LogMonitor != null)
            {
                page.LogMonitor.Log(LogMessageType.SystemSerializationPageRequest, LogDataSet);
            }
        }

        private LogMessageEntry LogDataSet()
        {
            if (sessionWorker == null)
                return new LogMessageEntry(LogMessageType.SystemSerializationPageRequest, string.Format("sessionWorker == null in '{0}'", ID));
            var ds = sessionWorker.Object as DataSet;
            if (ds == null)
                return new LogMessageEntry(LogMessageType.SystemSerializationPageRequest, string.Format("sessionWorker.Object is not DataSet in '{0}'", ID));

            var ser = new BinaryFormatter();
            long length;
            string typeObject = null;
            using (var stream = new MemoryStream())
            {
                ser.Serialize(stream, ds);
                length = stream.Length;
                stream.Position = 0;
                var o = ser.Deserialize(stream);
                if (o != null) typeObject = o.GetType().FullName;
            }
            int tablesCount = 0;
            string tableLengthes = null;
            foreach (DataTable table in ds.Tables)
            {
                if (table.Rows.Count <= 0) continue;
                tablesCount++;
                using (var serializationStream = new MemoryStream())
                {
                    ser.Serialize(serializationStream, table);
                    tableLengthes += string.Format("{0} [Rows: {1}, Size: {2}], ", table.TableName, table.Rows.Count, serializationStream.Length);
                }
            }
            var format = "Session={0}, DataSet ({1}).Size={2}, TableCount={3}, FilledTablesCount={4} ({5}), Deserilized={6}";
            var message = string.Format(format, sessionWorker.Key, ds.DataSetName, length,
                                        ds.Tables.Count, tablesCount, tableLengthes, typeObject);
            return new LogMessageEntry(LogMessageType.SystemSerializationPageRequest, message);
        }

//        public virtual string GetSessionKey(bool full)
//        {
//            string pageKey = GetPageKey();
//            return GetSessionKey(full, pageKey, Key);
//        }
//
//        public virtual string GetPageKey()
//        {
//            BaseSPPage page = Page as BaseSPPage;
//            if (page != null) return page.PageGuid.ToString();
//            return null;
//        }
//
//        public static string GetSessionKey(bool full, string pageKey, string key)
//        {
//            string sessionKey = full ? SessionWorker.KeyConst + key : key;
//            if (string.IsNullOrEmpty(pageKey)) return sessionKey;
//            return sessionKey + "_" + pageKey;
//        }
//
//        public static string GetSessionKey(bool full, Page page, string key)
//        {
//            string pageKey = null;
//            BaseSPPage baseSPPage = page as BaseSPPage;
//            if (baseSPPage != null)
//                pageKey = baseSPPage.PageGuid.ToString();
//
//            string sessionKey = full ? SessionWorker.KeyConst + key : key;
//            if (string.IsNullOrEmpty(pageKey)) return sessionKey;
//            return sessionKey + "_" + pageKey;
//        }
    }
}