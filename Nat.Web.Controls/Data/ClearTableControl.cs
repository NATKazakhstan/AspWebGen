using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Web.UI;
using Nat.Web.Tools;

namespace Nat.Web.Controls
{
    [ParseChildren(true)]
    [ProvideProperty("Tables", typeof (Component))]
    [PersistChildren(false)]
    public class ClearTableControl : Control, ISessionWorkerContainer
    {
        private readonly ListTableItems<ClearTableItem> tables;
        private ClearType clearType = ClearType.All;
        private bool generateSessionWorker = false;
        private SessionWorker sessionWorker = null;
        private string sessionWorkerControl = "";
        private bool allowClearTables = true;

        public ClearTableControl()
        {
            tables = new ListTableItems<ClearTableItem>(this);
        }

        /// <summary>
        /// “ип чистки примен€етс€ по умолчанию ко всем таблицам.
        /// </summary>
        [DefaultValue(ClearType.All)]
        public ClearType ClearType
        {
            get { return clearType; }
            set { clearType = value; }
        }

        /// <summary>
        /// ¬ыполн€ть очистку.
        /// </summary>
        [Browsable(false)]
        [DefaultValue(true)]
        public bool AllowClearTables
        {
            get { return allowClearTables; }
            set { allowClearTables = value; }
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
        [PersistenceMode(PersistenceMode.InnerProperty)]
        [NotifyParentProperty(true)]
        public ListTableItems<ClearTableItem> Tables
        {
            get { return tables; }
        }

        [Browsable(false)]
        public SessionWorker SessionWorker
        {
            get
            {
                if (!generateSessionWorker) return sessionWorker;
                generateSessionWorker = false;
                Control control = Page.FindControl(sessionWorkerControl);
                if (control == null && Parent != null)
                    control = Parent.FindControl(sessionWorkerControl);
                SessionWorkerControl swc = control as SessionWorkerControl;
                if (swc != null) return sessionWorker = swc.SessionWorker;
                return sessionWorker;
            }
            set { sessionWorker = value; }
        }

        #region ISessionWorkerContainer Members

        [IDReferencePropertyAttribute(typeof (SessionWorkerControl))]
        [Category("Behavior")]
        [DefaultValue("")]
        public string SessionWorkerControl
        {
            get { return sessionWorkerControl; }
            set
            {
                sessionWorkerControl = value;
                generateSessionWorker = true;
            }
        }

        #endregion

        protected override void OnLoad(EventArgs e)
        {
            Page.Unload += Page_OnUnload;
            base.OnLoad(e);
        }


        protected override void Render(HtmlTextWriter writer)
        {
            if (DesignMode) writer.Write(ID);
        }

        private void Page_OnUnload(object sender, EventArgs e)
        {
            ClearTables();
        }

        public void ClearTables() 
        {
            if (!allowClearTables) return;
            if (SessionWorker == null) 
                throw new Exception(string.Format("SessionWorker не может быть null ({0})", ID));
            DataSet dataSet = SessionWorker.Object as DataSet;
            if (dataSet == null)
                throw new Exception(string.Format("SessionWorker.Object должен содержать DataSet ({0})", ID));

            Dictionary<string, ClearType> dic = new Dictionary<string, ClearType>();
            if (clearType == ClearType.Not || clearType == ClearType.NotSet)
            {
                foreach (ClearTableItem item in tables)
                {
                    if (item.ClearType == ClearType.NotSet || item.ClearType == ClearType.Not)
                        continue;
                    else dic.Add(item.TableName, item.ClearType);
                }
            }
            else
            {
                foreach (DataTable table in dataSet.Tables)
                    dic.Add(table.TableName, clearType);
                foreach (ClearTableItem item in tables)
                {
                    if (item.ClearType == ClearType.Not)
                    {
                        if (dic.ContainsKey(item.TableName))
                            dic.Remove(item.TableName);
                        continue;
                    }
                    if (item.ClearType == ClearType.NotSet)
                        continue;
                    if (dic.ContainsKey(item.TableName))
                        dic[item.TableName] = item.ClearType;
                    else dic.Add(item.TableName, item.ClearType);
                }
            }
            ClearTable.Clear(dataSet, dic);
        }
    }
}