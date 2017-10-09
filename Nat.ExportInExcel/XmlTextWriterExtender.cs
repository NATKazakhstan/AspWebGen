using System.Xml;

namespace Nat.ExportInExcel
{
    public static class XmlTextWriterExtender
    {
        
        public static void WriteEmptyElementExt(this XmlTextWriter writer, string elementName)
        {
            writer.WriteStartElement(elementName);
            writer.WriteEndElement();            
        }

        public static void WriteElementStringExt(this XmlTextWriter writer, string elementName, string elementValue, string attrName1, string attrValue1)
        {
            writer.WriteStartElement(elementName);
            writer.WriteAttributeString(attrName1, attrValue1);
            if (!string.IsNullOrEmpty(elementValue))
                writer.WriteString(elementValue);
            writer.WriteEndElement();
        }

        public static void WriteElementStringExt(this XmlTextWriter writer, string elementName, string elementValue, string attrName1, string attrValue1, string attrName2, string attrValue2)
        {
            writer.WriteStartElement(elementName);
            writer.WriteAttributeString(attrName1, attrValue1);
            writer.WriteAttributeString(attrName2, attrValue2);
            if (!string.IsNullOrEmpty(elementValue))
                writer.WriteString(elementValue);
            writer.WriteEndElement();
        }

        public static void WriteElementStringExt(this XmlTextWriter writer, string elementName, string elementValue, string attrName1, string attrValue1, string attrName2, string attrValue2, string attrName3, string attrValue3)
        {
            writer.WriteStartElement(elementName);
            writer.WriteAttributeString(attrName1, attrValue1);
            writer.WriteAttributeString(attrName2, attrValue2);
            writer.WriteAttributeString(attrName3, attrValue3);
            if (!string.IsNullOrEmpty(elementValue))
                writer.WriteString(elementValue);
            writer.WriteEndElement();
        }

        public static void WriteElementStringExt(this XmlTextWriter writer, string elementName, string elementValue, string attrName1, string attrValue1, string attrName2, string attrValue2, string attrName3, string attrValue3, string attrName4, string attrValue4)
        {
            writer.WriteStartElement(elementName);
            writer.WriteAttributeString(attrName1, attrValue1);
            writer.WriteAttributeString(attrName2, attrValue2);
            writer.WriteAttributeString(attrName3, attrValue3);
            writer.WriteAttributeString(attrName4, attrValue4);
            if (!string.IsNullOrEmpty(elementValue))
                writer.WriteString(elementValue);
            writer.WriteEndElement();
        }

        public static void WriteElementStringExt(this XmlTextWriter writer, string elementName, string elementValue, string attrName1, string attrValue1, string attrName2, string attrValue2, string attrName3, string attrValue3, string attrName4, string attrValue4, string attrName5, string attrValue5, string attrName6, string attrValue6, string attrName7, string attrValue7, string attrName8, string attrValue8, string attrName9, string attrValue9, string attrName10, string attrValue10)
        {
            writer.WriteStartElement(elementName);
            writer.WriteAttributeString(attrName1, attrValue1);
            writer.WriteAttributeString(attrName2, attrValue2);
            writer.WriteAttributeString(attrName3, attrValue3);
            writer.WriteAttributeString(attrName4, attrValue4);
            writer.WriteAttributeString(attrName5, attrValue5);
            if (!string.IsNullOrEmpty(attrName6))
                writer.WriteAttributeString(attrName6, attrValue6);
            if (!string.IsNullOrEmpty(attrName7))
                writer.WriteAttributeString(attrName7, attrValue7);
            if (!string.IsNullOrEmpty(attrName8))
                writer.WriteAttributeString(attrName8, attrValue8);
            if (!string.IsNullOrEmpty(attrName9))
                writer.WriteAttributeString(attrName9, attrValue9);
            if (!string.IsNullOrEmpty(attrName10))
                writer.WriteAttributeString(attrName10, attrValue10);
            if (!string.IsNullOrEmpty(elementValue))
                writer.WriteString(elementValue);
            writer.WriteEndElement();
        }

        public static void WriteDoubleElementStringExt(this XmlTextWriter writer, string elementName, string attrName1, string attrValue1, 
            string element2Name, string element2Value, string attr2Name1, string attr2Value1)
        {
            writer.WriteStartElement(elementName);
            if (!string.IsNullOrEmpty(attrName1))
                writer.WriteAttributeString(attrName1, attrValue1);

            #region element2Name
            writer.WriteStartElement(element2Name);
            if (!string.IsNullOrEmpty(attr2Name1))
                writer.WriteAttributeString(attr2Name1, attr2Value1);
            if (!string.IsNullOrEmpty(element2Value))
                writer.WriteString(element2Value);

            writer.WriteEndElement();
            #endregion

            writer.WriteEndElement();
        }

        public static void WriteStartElementExt(this XmlTextWriter writer, string elementName, string attrName1, string attrValue1)
        {
            writer.WriteStartElement(elementName);
            writer.WriteAttributeString(attrName1, attrValue1);
        }

        public static void WriteStartElementExt(this XmlTextWriter writer, string elementName, string attrName1, string attrValue1, string attrName2, string attrValue2)
        {
            writer.WriteStartElement(elementName);
            writer.WriteAttributeString(attrName1, attrValue1);
            writer.WriteAttributeString(attrName2, attrValue2);
        }

        public static void WriteStartElementExt(this XmlTextWriter writer, string elementName, string attrName1, string attrValue1, string attrName2, string attrValue2, string attrName3, string attrValue3)
        {
            writer.WriteStartElement(elementName);
            writer.WriteAttributeString(attrName1, attrValue1);
            writer.WriteAttributeString(attrName2, attrValue2);
            writer.WriteAttributeString(attrName3, attrValue3);
        }

        public static void WriteStartElementExt(this XmlTextWriter writer, string elementName, string attrName1, string attrValue1, string attrName2, string attrValue2, string attrName3, string attrValue3, string attrName4, string attrValue4)
        {
            writer.WriteStartElement(elementName);
            writer.WriteAttributeString(attrName1, attrValue1);
            writer.WriteAttributeString(attrName2, attrValue2);
            writer.WriteAttributeString(attrName3, attrValue3);
            writer.WriteAttributeString(attrName4, attrValue4);
        }

    }
}
