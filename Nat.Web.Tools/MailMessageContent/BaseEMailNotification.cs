namespace Nat.Web.Tools.MailMessageContent
{
    using System.Collections.Generic;
    using System.IO;
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
            refCurrentPerson = User.GetPersonInfoRequired().id;
        }

        protected BaseEMailNotification(long currentPerson, long person)
        {
            refCurrentPerson = currentPerson;
            refPerson = person;
        }

        #endregion

        #region Properties

        protected long refCurrentPerson { get; set; }

        protected long refPerson { get; set; }

        public bool ThrowException { get; set; }

        #endregion

        #region Public Methods and Operators

        public abstract void Notify();

        #endregion

        #region Methods

        protected abstract string GetCurrentUserEMail();

        protected abstract RowPerson GetPerson(long refPerson);

        protected abstract BaseSendEmailsDetector GetSendEmailsDetector();

        protected abstract string GetSubject();

        protected abstract void SendEMail(
            string currentMailAddress, 
            HtmlTextWriter html, 
            string subject, 
            IEnumerable<string> listEmails, 
            IEnumerable<string> listEmailsCopy, 
            IEnumerable<Attachment> attachments);

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

            using (var stringWriter = new StringWriter())
            using (var htmlWriter = new HtmlTextWriter(stringWriter))
            {
                string subject = GetSubject();
                var row = GetData();
                RenderEMail(htmlWriter, subject, row);
                SendEMail(
                    GetCurrentUserEMail(), 
                    htmlWriter, 
                    subject, 
                    mailsDetector.EMailsTo, 
                    mailsDetector.EMailsCopy, 
                    new List<Attachment>());
            }
        }

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
                email.BeginTable(
                    subject,
                    AddAttributesForMainTable, 
                    writer => { }, 
                    string.Empty, 
                    string.Empty);
                if (row.Person != null)
                {
                    if (!string.IsNullOrEmpty(row.Person.Rank))
                        email.AddRow("В/зв", row.Person.Rank);
                    email.AddRow("ФИО", row.Person.Fio);
                    if (!string.IsNullOrEmpty(row.Person.Position))
                        email.AddRow("Должность", row.Person.Position);
                }

                RenderEMail(email, row);

                if (row.CurrentPerson != null)
                {
                    email.AddRow(string.Empty, string.Empty);
                    email.AddRow("Изменения внес:", string.Empty);
                    if (!string.IsNullOrEmpty(row.CurrentPerson.Rank))
                        email.AddRow("В/зв", row.CurrentPerson.Rank);
                    email.AddRow("ФИО", row.CurrentPerson.Fio);
                    if (!string.IsNullOrEmpty(row.CurrentPerson.Position))
                        email.AddRow("Должность", row.CurrentPerson.Position);
                }

                email.EndTable();
                email.EndMessage();
            }
        }

        #endregion
    }
}