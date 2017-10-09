using System;
using System.Web.SessionState;
using System.Web.UI;

namespace Nat.Web.Tools
{
    public class SessionWorker
    {
        private string key;
        private HttpSessionState session;
        public const string KeyConst = "SessionWorker$";

        /// <summary>
        /// Конструктор.
        /// </summary>
        public SessionWorker()
        {
        }

        /// <summary>
        /// Конструктор.
        /// </summary>
        /// <param name="page">Страница, используетсья для получения параметров.</param>
        /// <param name="key">Ключ сессии.</param>
        public SessionWorker(Page page, string key)
        {
            session = page.Session;
//            IPage ipage = page as IPage;
//            if (ipage != null)
//                key += "_" + ipage.PageGuid;
            if (!string.IsNullOrEmpty(page.Request.QueryString["sessionKey"]))
                key += page.Request.QueryString["sessionKey"];
            this.key = key;
        }

        /// <summary>
        /// Объект данных из сессии.
        /// </summary>
        public virtual object Object
        {
            get { return session[GetSessionKey()]; }
            set { session[GetSessionKey()] = value; }
        }

        public virtual T GetObject<T>() where T : new()
        {
            if (Object == null) Object = new T();
            return (T) Object;
        }

        public HttpSessionState Session
        {
            get { return session; }
        }

        public string GetSessionKey()
        {
            return KeyConst + key;
        }

        public void SetSession(HttpSessionState ses)
        {
            if (session != null) throw new Exception("Session can set only once");
            session = ses;
        }

        public virtual void RemoveObject()
        {
            if (session[GetSessionKey()] != null)
                session.Remove(GetSessionKey());
        }

        public string Key
        {
            get { return key; }
            set { key = value; }
        }
    }

    public class SessionWorker<T> : SessionWorker where T : new()
    {
        public SessionWorker()
        {
        }

        public SessionWorker(Page page, string key) : base(page, key)
        {
        }

        public override object Object
        {
            get
            {
                if (base.Object == null) base.Object = new T();
                return base.Object;
            }
            set { base.Object = (T) value; }
        }

        public virtual T GObject
        {
            get { return (T)Object; }
            set { base.Object = value; }
        }
    }
}