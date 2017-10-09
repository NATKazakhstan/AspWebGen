using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.Common;
using System.Web.UI;
using System.Web.UI.WebControls;
using Nat.Tools.Data;
using Nat.Tools.Data.Transactions;
using Nat.Web.Controls;
using Nat.Web.Tools;

namespace Nat.Web.Controls
{
    [ParseChildren(true)]
    [ProvideProperty("Tables", typeof(Component))]
    [PersistChildren(false)]
    public class ExecuteTransactionControl : WebControl, ISessionWorkerContainer
    {
        private TransactionManager executeTransaction;
        private bool generateSessionWorker = false;
        private SessionWorker sessionWorker = null;
        private string sessionWorkerControl = "";
        private readonly ListTableItems<TableItem> tables;
        private string textError = "";
        public event ExecuteTransactionInitConnectionEventHandler InitConnection;
        public event ExecuteTransactionErrorEventHandler Error;

        public ExecuteTransactionControl()
        {
            tables = new ListTableItems<TableItem>(this);
        }

        public ExecuteTransactionControl(HtmlTextWriterTag tag) : base(tag)
        {
            tables = new ListTableItems<TableItem>(this);
        }

        public ExecuteTransactionControl(string tag) : base(tag)
        {
            tables = new ListTableItems<TableItem>(this);
        }

        [IDReferencePropertyAttribute(typeof(SessionWorkerControl))]
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

        [Browsable(false)]
        public TransactionManager ExecuteTransaction
        {
            get { return executeTransaction; }
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
        [PersistenceMode(PersistenceMode.InnerProperty)]
        [NotifyParentProperty(true)]
        public ListTableItems<TableItem> Tables
        {
            get { return tables; }
        }

        public void UpdateData()
        {
            if (SessionWorker == null) throw new Exception("SessionWorker can not be null");
            DbConnection connection = GetConnection();

            DataSet dataSet = SessionWorker.Object as DataSet;
            if (dataSet == null) throw new Exception("Object of SessionWorker is not DataSet");
            Exception exception = null;
            List<string> skipTables = new List<string>();
            connection.Open();
            try
            {
                DbTransaction transaction = connection.BeginTransaction();
                UpdateData(transaction, dataSet, skipTables);
                transaction.Commit();
                executeTransaction.Commit();
            }
            catch (Exception e)
            {
                exception = e;
            }
            finally
            {
                connection.Close();
            }
            ThrowException(skipTables, exception);
        }

        public void ThrowException(List<string> skipTables, Exception exception) 
        {
            if(exception != null || skipTables.Count > 0)
            {
                ExecuteTransactionErrorEventArgs args = new ExecuteTransactionErrorEventArgs(exception as ExceptionExecuteTransaction, exception, skipTables.ToArray());
                OnError(args);
                if (!string.IsNullOrEmpty(args.ErrorMessage)) textError = args.ErrorMessage;
                if (args.ThrowException && exception != null) throw exception;
            }
        }

        public void UpdateData(DbTransaction transaction, DataSet dataSet, List<string> skipTables)
        {
            executeTransaction = new TransactionManager(transaction);
            foreach (TableItem item in tables)
            {
                if (dataSet.Tables.Contains(item.TableName))
                    executeTransaction.Add(new TransactionItem(dataSet.Tables[item.TableName]));
                else skipTables.Add(item.TableName);
            }
            executeTransaction.UpdateData();
        }

        public virtual DbConnection GetConnection()
        {
            ExecuteTransactionInitConnectionEventArgs args = new ExecuteTransactionInitConnectionEventArgs();
            OnGetConnection(args);
            if(args.Connection == null) throw new Exception("Connection is not initilized");
            return args.Connection;
        }

        protected virtual void OnGetConnection(ExecuteTransactionInitConnectionEventArgs e)
        {
            if (InitConnection != null) InitConnection(this, e);
        }

        protected virtual void OnError(ExecuteTransactionErrorEventArgs e)
        {
            if (Error != null) Error(this, e);
        }

        protected override void Render(HtmlTextWriter writer)
        {
            if (DesignMode) writer.Write(ID);
            else writer.Write(textError);
        }
    }
}