using System.Configuration;
using System.Web.Configuration;

namespace Nat.Web.Tools.MailMessageContent
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.IO.Packaging;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Net.Mail;
    using System.Web.UI;

    using Nat.Web.Tools.Initialization;
    using Nat.Web.Tools.Security;

    public abstract class BaseEMailNotification
    {
        #region Constructors and Destructors

        protected BaseEMailNotification(long refPerson)
        {
            this.refPerson = refPerson;
            refCurrentPerson = User.GetPersonInfo()?.id ?? 0;
        }

        protected BaseEMailNotification(long currentPerson, long person)
        {
            refCurrentPerson = currentPerson;
            refPerson = person;
        }

        #endregion

        #region Properties

        /// <summary>
        /// Свойство для отписки от уведомления.
        /// </summary>
        public long? refModuleSend { get; set; }

        protected long refCurrentPerson { get; set; }

        protected long refPerson { get; set; }

        public bool ThrowException { get; set; }

        public string SubsystemName { get; set; }

        public string CurrentPersonText { get; set; } = "Изменения внес:";
        public string PersonText { get; set; }

        public EMailNotificationAggregator EMailAggregator { get; set; }

        public List<MyAttachment> Attachments { get; } = new List<MyAttachment>();

        #endregion

        #region Public Methods and Operators

        public abstract void Notify();

        #endregion

        #region Methods

        protected abstract string GetCurrentUserEMail();

        protected abstract RowPerson GetPerson(long refPerson);

        protected abstract BaseSendEmailsDetector GetSendEmailsDetector();

        protected abstract string GetSubject();

        protected internal abstract void SendEMail(
            string currentMailAddress, 
            HtmlTextWriter html, 
            string subject, 
            IEnumerable<string> listEmails, 
            IEnumerable<string> listEmailsCopy, 
            IEnumerable<MyAttachment> attachments);
        
        #endregion

        protected class Changes
        {
            #region Public Properties

            public RowPerson CurrentPerson { get; set; }

            public RowPerson Person { get; set; }

            #endregion
        }

        protected class Changes<TRowState> : Changes
        {
            #region Public Properties

            public TRowState NewValues { get; set; }

            public TRowState OldValues { get; set; }

            #endregion
        }

        public class RowPerson
        {
            #region Public Properties

            public string Fio { get; set; }

            public string Position { get; set; }

            public string Rank { get; set; }

            #endregion
        }
        
        public class MyAttachment
        {
            public MyAttachment()
            {
            }

            public MyAttachment(string filePath, string fileName)
            {
                FileName = fileName;
                FilePath = filePath;
            }

            public string FileName { get; set; }
            public string FilePath { get; set; }
            public string MediaType { get; set; }
        }
    }

    public abstract class BaseEMailNotification<TRowState> : BaseEMailNotification
    {
        #region Constructors and Destructors

        protected BaseEMailNotification(long refPerson)
            : base(refPerson)
        {
        }

        protected BaseEMailNotification(long currentPerson, long person)
            : base(currentPerson, person)
        {
        }

        #endregion

        #region Public Methods and Operators

        public abstract TRowState GetNewValues();

        public abstract TRowState GetOldValues();

        public override void Notify()
        {
            var mailsDetector = GetSendEmailsDetector();
            if (!mailsDetector.Detect())
            {
                OnDetectToEmailsEmpty();
                return;
            }

            refModuleSend = mailsDetector.refModuleSend;

            using (var stringWriter = new StringWriter())
            using (var htmlWriter = new HtmlTextWriter(stringWriter))
            {
                var subject = GetSubject();
                var row = GetData();
                RenderEMail(htmlWriter, subject, row);
                if (EMailAggregator != null)
                    EMailAggregator.Add(
                        this,
                        GetCurrentUserEMail(),
                        htmlWriter,
                        subject,
                        mailsDetector.EMailsTo,
                        mailsDetector.EMailsCopy,
                        Attachments);
                else
                    SendEMail(
                        GetCurrentUserEMail(),
                        htmlWriter,
                        subject,
                        mailsDetector.EMailsTo,
                        mailsDetector.EMailsCopy,
                        Attachments);
            }
        }

        protected virtual bool CustomHtml => false;

        #endregion

        #region Methods

        protected virtual void OnDetectToEmailsEmpty()
        {
        }

        protected virtual Changes<TRowState> GetData()
        {
            WebInitializer.Initialize();

            var changes = new Changes<TRowState>
                {
                    Person = GetPerson(refPerson),
                    CurrentPerson = GetPerson(refCurrentPerson),
                    OldValues = GetOldValues(),
                    NewValues = GetNewValues()
                };

            return changes;
        }

        protected abstract void RenderEMail(EmailMessage email, Changes<TRowState> row);

        protected virtual void AddAttributesForMainTable(HtmlTextWriter writer)
        {
            writer.AddAttribute(HtmlTextWriterAttribute.Border, "1");
            writer.AddStyleAttribute(HtmlTextWriterStyle.Width, "450px");
        }

        protected virtual void RenderEMail(HtmlTextWriter htmlWriter, string subject, Changes<TRowState> row)
        {
            using (var email = new EmailMessage(htmlWriter))
            {
                email.BeginMessage();
                if (CustomHtml)
                {
                    RenderEMail(email, row);
                    email.EndMessage();
                    return;
                }

                email.BeginTable(
                    subject,
                    SubsystemName, 
                    AddAttributesForMainTable, 
                    writer => { writer.AddAttribute(HtmlTextWriterAttribute.Class, "boldTD"); }, 
                    string.Empty, 
                    string.Empty);
                
                if (row.Person != null)
                {
                    if (!string.IsNullOrEmpty(PersonText))
                    {
                        email.HtmlWriter.AddAttribute(HtmlTextWriterAttribute.Class, "tableHead");
                        email.AddRow(PersonText);
                    }

                    if (!string.IsNullOrEmpty(row.Person.Rank))
                        email.AddRow("В/зв", row.Person.Rank);
                    email.AddRow("ФИО", row.Person.Fio);
                    if (!string.IsNullOrEmpty(row.Person.Position))
                        email.AddRow("Должность", row.Person.Position);
                }

                var baseRender = false;
                if (row.Person != null && row.CurrentPerson != null)
                {
                    baseRender = true;
                    RenderEMail(email, row);
                }

                if (row.CurrentPerson != null)
                {
                    email.AddRow(string.Empty);
                    email.HtmlWriter.AddAttribute(HtmlTextWriterAttribute.Class, "tableHead");
                    email.AddRow(CurrentPersonText);
                    if (!string.IsNullOrEmpty(row.CurrentPerson.Rank))
                        email.AddRow("В/зв", row.CurrentPerson.Rank);
                    email.AddRow("ФИО", row.CurrentPerson.Fio);
                    if (!string.IsNullOrEmpty(row.CurrentPerson.Position))
                        email.AddRow("Должность", row.CurrentPerson.Position);
                }

                if (!baseRender)
                    RenderEMail(email, row);
                
                email.EndTable();

                if (refModuleSend != null)
                {
                    email.HtmlWriter.AddAttribute(HtmlTextWriterAttribute.Href, ConfigurationManager.AppSettings["MaxatServer"] + "/MSC/Unsubscribe?refSend=" + refModuleSend);
                    email.HtmlWriter.RenderBeginTag(HtmlTextWriterTag.A);
                    email.HtmlWriter.Write("Отписаться");
                    email.HtmlWriter.RenderEndTag();
                }

                email.EndMessage();
            }
        }

        #endregion
    }
}