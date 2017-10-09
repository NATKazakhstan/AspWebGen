using System;
using System.Xml.Linq;

namespace Nat.Web.Controls.Data
{
    public static class XContainerExtender
    {
        public static TValue? GetElementValue<TValue>(this XContainer parentElement, string elementName)
            where TValue : struct
        {
            XElement element = parentElement.Element(elementName);
            if (element == null) return null;
            return (TValue)Convert.ChangeType(element.Value, typeof(TValue));
        }

        public static string GetElementValue(this XContainer parentElement, string elementName)
        {
            XElement element = parentElement.Element(elementName);
            if (element == null) return null;
            return element.Value;
        }
        /*
        public static void SetElementValue(this XContainer parentElement, string value, string elementName)
        {
            XElement element = parentElement.Element(elementName);
            if (element == null)
            {
                element = new XElement(elementName);
                parentElement.Add(element);
            }
            element.Value = value;
        }*/
    }
}
