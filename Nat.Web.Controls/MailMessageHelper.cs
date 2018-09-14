/*
 * Created by: Eugene.Kolesnikov
 * Created: 16 июня 2011 г.
 * Copyright © JSC NAT Kazakhstan 2009
 */
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Configuration;
using System.Net.Mail;
using System.Web.Configuration;
using System.Web.UI;
using Nat.Web.Tools.Security;

namespace Nat.Web.Controls
{
    using System.ComponentModel;

    using Nat.Tools.Specific;
    using Nat.Web.Tools;

    public static class MailMessageHelper
    {
        [DisplayName("Общии задачи. Отправка уведомлений на почту.")]
        public static void SendMail(
            string currentMailAddress,
            HtmlTextWriter html,
            string subject,
            IEnumerable<string> listEmails,
            IEnumerable<string> listEmailsCopy,
            IEnumerable<Attachment> attachments)
        {
            SendMail(currentMailAddress, html, subject, listEmails, listEmailsCopy, attachments, User.GetSID(), false);
        }

        [DisplayName("Общии задачи. Отправка уведомлений на почту.")]
        public static void SendMail(
            string currentMailAddress,
            HtmlTextWriter html,
            string subject,
            IEnumerable<string> listEmails,
            IEnumerable<string> listEmailsCopy,
            IEnumerable<Attachment> attachments,
            bool throwException)
        {
            SendMail(currentMailAddress, html, subject, listEmails, listEmailsCopy, attachments, User.GetSID(), throwException);
        }

        [DisplayName("Общии задачи. Отправка уведомлений на почту.")]
        public static void SendMail(
            string currentMailAddress,
            HtmlTextWriter html,
            string subject,
            IEnumerable<string> listEmails,
            IEnumerable<string> listEmailsCopy,
            IEnumerable<Attachment> attachments,
            string sid)
        {
            SendMail(currentMailAddress, html, subject, listEmails, listEmailsCopy, attachments, sid, false);
        }

        [DisplayName("Общии задачи. Отправка уведомлений на почту.")]
        public static void SendMail(
            string currentMailAddress,
            HtmlTextWriter html,
            string subject,
            IEnumerable<string> listEmails,
            IEnumerable<string> listEmailsCopy,
            IEnumerable<Attachment> attachments,
            string sid,
            bool throwException)
        {
            SendMail(
                currentMailAddress,
                html.InnerWriter.ToString(),
                subject,
                listEmails,
                listEmailsCopy,
                attachments,
                sid,
                throwException);
        }

        [DisplayName("Общии задачи. Отправка уведомлений на почту.")]
        public static void SendMail(
            string currentMailAddress,
            string html,
            string subject,
            IEnumerable<string> listEmails,
            IEnumerable<string> listEmailsCopy,
            IEnumerable<Attachment> attachments,
            string sid,
            bool throwException)
        {
            SmtpSection mailSettings;
            if (SpecificInstances.DbFactory == null)
                mailSettings = (SmtpSection)WebConfigurationManager.GetSection("system.net/mailSettings/smtp");
            else
                mailSettings =
                    (SmtpSection)
                    ((IWebConfiguration)SpecificInstances.DbFactory).WebConfiguration.GetSection(
                        "system.net/mailSettings/smtp");

            var mailServer = WebConfigurationManager.AppSettings["MailServer"];
            string emailFrom;

            if (mailSettings != null && !string.IsNullOrEmpty(mailSettings.From))
                emailFrom = mailSettings.From;
            else
                emailFrom = currentMailAddress;

            if (string.IsNullOrEmpty(emailFrom))
                throw new Exception("Not set emailFrom, e-mail can't be sended");

            var mm = new MailMessage
                         {
                             From = new MailAddress(emailFrom),
                             Subject = subject,
                             //SubjectEncoding = Encoding.UTF8,
                             Body = html,
                             IsBodyHtml = true,
                         };

            foreach (var email in listEmails.Where(r => !string.IsNullOrEmpty(r)))
                mm.To.Add(new MailAddress(email));

            if (listEmailsCopy != null)
                foreach (var email in listEmailsCopy.Where(r => !string.IsNullOrEmpty(r)))
                    mm.CC.Add(new MailAddress(email));

            SmtpClient client;
            if (mailSettings == null || string.IsNullOrEmpty(mailSettings.From))
                client = new SmtpClient(mailServer, 25)
                             {
                                 Credentials = CredentialCache.DefaultNetworkCredentials,
                             };
            else
            {
                
                var credentails = new NetworkCredential(
                    mailSettings.Network.UserName,
                    mailSettings.Network.Password,
                    mailSettings.Network.ClientDomain);
                client = new SmtpClient(mailSettings.Network.Host, mailSettings.Network.Port)
                {
                    Credentials = credentails,
                    UseDefaultCredentials = mailSettings.Network.DefaultCredentials,
                    EnableSsl = Convert.ToBoolean(WebConfigurationManager.AppSettings["Email_EnableSsl"]),
                };

                client.Credentials = credentails;
            }

            if (attachments != null)
                foreach (var attachment in attachments)
                    mm.Attachments.Add(attachment);

            var monitor = new LogMonitor();
            monitor.Init();
            try
            {
                monitor.Log(
                    LogMessageType.SystemMailSendMailInformation,
                    () =>
                        {
                            var toMailsArray = mm.To.Select(
                                r => string.IsNullOrEmpty(r.DisplayName) ? r.Address : r.DisplayName).ToArray();
                            var toMails = string.Join("; ", toMailsArray);
                            var fromEmail = string.IsNullOrEmpty(mm.From.DisplayName)
                                                ? mm.From.Address
                                                : mm.From.DisplayName;
                            var message = string.Format(
                                "От: {0}; \r\nКому: {1}; \r\nТема: {2}",
                                fromEmail,
                                toMails,
                                mm.Subject);
                            monitor.FieldChanged("EMailBody", "Содержание", "", mm.Body);
                            return new LogMessageEntry(sid, LogMessageType.SystemMailSendMailInformation, message);
                        });
                client.Send(mm);
            }
            catch (Exception e)
            {
                var messageEntry = new LogMessageEntry(sid, LogMessageType.SystemMailSendMailError, e.ToString());
                monitor.Log(messageEntry);
                if (throwException)
                    throw;
            }
        }
    }
}