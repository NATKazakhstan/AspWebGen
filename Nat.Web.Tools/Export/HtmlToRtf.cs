namespace Nat.Web.Tools.Export
{
    using System;
    using System.IO;
    using System.Reflection;
    using System.Text;
    using System.Threading;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Documents;

    using MarkupConverter;

    public static class HtmlToRtf
    {
        private static readonly object LockObject = new object();

        private static string _result;
        private static Exception _exception;
        
        public static string ConvertHtmlToRtf(string html)
        {
            lock (LockObject)
            {
                _result = null;
                _exception = null;
                var thread = new Thread(ConvertRtfInSTAThread);
                thread.SetApartmentState(ApartmentState.STA);
                thread.Start(html);
                thread.Join(30000);
                if (_exception != null)
                    throw new TargetInvocationException(_exception);

                if (string.IsNullOrEmpty(_result))
                    return string.Empty;
                /*
                var indexOfPlain = _result.IndexOf(@"\plain", StringComparison.InvariantCulture);
                if (indexOfPlain > -1)
                {
                    var indexOfLtrch = _result.IndexOf(@"\ltrch", indexOfPlain, StringComparison.InvariantCulture);
                    if (indexOfLtrch > -1)
                        return _result.Remove(indexOfLtrch, 6);
                }
                */
                return _result.Replace(@"{\ltrch  }", string.Empty).Replace(@"\ltrch", string.Empty);
            }
        }

        /*
        private static void ConvertRtfInSTAThread(object html)
        {
            var wb = new System.Windows.Forms.WebBrowser();
            wb.Navigate("about:blank");
            wb.Document.Encoding = Encoding.UTF8.WebName;
            wb.Document.Write((string)html);
            wb.Document.ExecCommand("SelectAll", false, null);
            wb.Document.ExecCommand("Copy", false, null);
            _result = (string)Clipboard.GetData(DataFormats.Rtf);
        }*/

        private static void ConvertRtfInSTAThread(object html)
        {
            try
            {
                var richTextBox = new RichTextBox();
                var textRange = new TextRange(richTextBox.Document.ContentStart, richTextBox.Document.ContentEnd);
                var xaml = HtmlToXamlConverter.ConvertHtmlToXaml((string)html, false);
                using (var stream = new MemoryStream(Encoding.UTF8.GetBytes(xaml)))
                    textRange.Load(stream, DataFormats.Xaml);
                textRange = new TextRange(richTextBox.Document.ContentStart, richTextBox.Document.ContentEnd);
                using (var stream = new MemoryStream())
                {
                    textRange.Save(stream, DataFormats.Rtf);
                    stream.Position = 0;
                    using (var streamReader = new StreamReader(stream))
                        _result = streamReader.ReadToEnd();
                }
            }
            catch (Exception e)
            {
                _exception = e;
                _result = null;
            }
        }
    }
    public static class RtfToHtml
    {
        private static readonly object LockObject = new object();

        private static string _result;
        private static Exception _exception;
        
        public static string ConvertRtfToHtml(string rtfText)
        {
            lock (LockObject)
            {
                _result = null;
                _exception = null;
                var thread = new Thread(ConvertRtfInSTAThread);
                thread.SetApartmentState(ApartmentState.STA);
                thread.Start(rtfText);
                thread.Join(30000);
                if (_exception != null)
                    throw new TargetInvocationException(_exception);

                if (string.IsNullOrEmpty(_result))
                    return string.Empty;
                
                return _result;
            }
        }
        
        private static void ConvertRtfInSTAThread(object rtfText)
        {
            try
            {
                var richTextBox = new RichTextBox();
                if (string.IsNullOrEmpty((string)rtfText))
                {
                    _result = "";
                    return;
                }
                var textRange = new TextRange(richTextBox.Document.ContentStart, richTextBox.Document.ContentEnd);
                using (var rtfMemoryStream = new MemoryStream())
                {
                    using (var rtfStreamWriter = new StreamWriter(rtfMemoryStream))
                    {
                        rtfStreamWriter.Write(rtfText);
                        rtfStreamWriter.Flush();
                        rtfMemoryStream.Seek(0, SeekOrigin.Begin);
                        textRange.Load(rtfMemoryStream, DataFormats.Rtf);
                    }
                }

                string xaml;
                using (var rtfMemoryStream = new MemoryStream())
                {
                    textRange = new TextRange(richTextBox.Document.ContentStart, richTextBox.Document.ContentEnd);
                    textRange.Save(rtfMemoryStream, DataFormats.Xaml);
                    rtfMemoryStream.Seek(0, SeekOrigin.Begin);
                    using (var rtfStreamReader = new StreamReader(rtfMemoryStream))
                    {
                        xaml = rtfStreamReader.ReadToEnd();
                    }
                }

                _result = HtmlFromXamlConverter.ConvertXamlToHtml(xaml, false);
            }
            catch (Exception e)
            {
                _exception = e;
                _result = null;
            }
        }
    }
}