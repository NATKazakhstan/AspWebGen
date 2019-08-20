namespace Nat.Web.Tools.MailMessageContent
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Text;
    using System.Web.UI;

    using Nat.Tools;

    public class EMailNotificationAggregator : IDisposable
    {
        private readonly Dictionary<string, Item> _emails = new Dictionary<string,Item>();

        private bool _requiredNotify;

        public bool ForAttachmentSeparateEMails { get; set; }
        public bool Distinct { get; set; }
        public string MessageSeparator { get; set; } = "\r\n<br/><hr/><br/>\r\n";

        public void Add(
            BaseEMailNotification notification,
            string from,
            HtmlTextWriter htmlWriter,
            string unsubscribe,
            string subject,
            IEnumerable<string> emailsTo,
            IEnumerable<string> emailsCopy,
            IEnumerable<BaseEMailNotification.MyAttachment> attachments)
        {
            _requiredNotify = true;
            var value = htmlWriter.InnerWriter.ToString();
            var item = new Item
                {
                    Html = new StringBuilder(value),
                    Attachments = attachments.ToList(),
                    From = from,
                    Subject = subject,
                    Unsubscribe = unsubscribe,
                    EMailsTo = emailsTo.ToList(),
                    EMailsCopy = emailsCopy.ToList(),
                    Notification = notification
                };
            item.AddedContent.Add(value);
            var key = item.GetKey(ForAttachmentSeparateEMails);
            if (_emails.ContainsKey(key))
                _emails[key].Union(item, MessageSeparator, Distinct);
            else
                _emails[key] = item;
        }

        public void Notify()
        {
            _requiredNotify = false;
            _emails.ToArray().ForEach(
                r =>
                    {
                        r.Value.Notify();
                        _emails.Remove(r.Key);
                    });
        }

        public void Dispose()
        {
            if (_requiredNotify)
                Notify();

            _emails.Clear();
        }

        private class Item
        {
            private readonly Guid _guid = Guid.NewGuid();

            private string _key;

            public string Subject { get; set; }
            public string From { get; set; }
            public string Unsubscribe { get; set; }
            public List<string> EMailsTo { get; set; }
            public List<string> EMailsCopy { get; set; }
            public List<string> AddedContent { get; } = new List<string>();
            public StringBuilder Html { get; set; }
            public List<BaseEMailNotification.MyAttachment> Attachments { get; set; }
            public BaseEMailNotification Notification { get; set; }

            public string GetKey(bool forAttachmentSeparateEMails)
            {
                if (forAttachmentSeparateEMails && Attachments != null && Attachments.Count > 0)
                    return _guid.ToString();
                
                if (_key != null) 
                    return _key;

                return _key = Notification.GetType().AssemblyQualifiedName
                              + " & " + From
                              + " & " + Subject
                              + " & " + string.Join(",", EMailsTo.ToArray())
                              + " & " + string.Join(",", EMailsCopy?.ToArray() ?? new string[0]);
            }

            public void Union(Item item, string messageSeparator, bool distinct)
            {
                if (!distinct || !AddedContent.Contains(item.AddedContent[0]))
                {
                    var searchBody = Html.Length > 64 ? Html.ToString(Html.Length - 64, 64) : Html.ToString();
                    var index = searchBody.LastIndexOf("</body>", StringComparison.Ordinal);
                    if (Html.Length > 64) index += Html.Length - 64;
                    Html.Insert(index, messageSeparator);

                    var unionHtml = item.AddedContent[0];
                    var startBodyIndex = unionHtml.IndexOf("<body", StringComparison.Ordinal);
                    var startTableIndex = unionHtml.IndexOf("<table", StringComparison.Ordinal);
                    var endStyleIndex = unionHtml.LastIndexOf("</style>", StringComparison.Ordinal);
                    int startIndex;

                    if (endStyleIndex < startTableIndex && endStyleIndex > -1)
                        startIndex = endStyleIndex + 8;
                    else if (startTableIndex > -1)
                        startIndex = startTableIndex;
                    else
                        startIndex = unionHtml.IndexOf(">", startBodyIndex, StringComparison.Ordinal) + 1;

                    var endIndex = unionHtml.LastIndexOf("</body>", StringComparison.Ordinal);
                    Html.Insert(index + messageSeparator.Length, item.Html.ToString(startIndex, endIndex - startIndex));

                    if (distinct)
                        AddedContent.Add(item.AddedContent[0]);
                }

                if (Attachments == null)
                    Attachments = item.Attachments;
                else if (item.Attachments != null)
                    Attachments.AddRange(item.Attachments);
            }

            public void Notify()
            {
                if (!string.IsNullOrEmpty(Unsubscribe))
                    Html.Insert(Html.Length - 16, Unsubscribe);
                using (var stringWriter = new StringWriter(Html))
                using (var writer = new HtmlTextWriter(stringWriter))
                {
                    Notification.SendEMail(From, writer, Subject, EMailsTo, EMailsCopy, Attachments);
                }
            }
        }
    }
}