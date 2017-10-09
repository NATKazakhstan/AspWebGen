namespace JS.LinqToJavaScript
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Web.Script.Serialization;

    using LinqToJavaScript.ExpressionVisitors;

    public class LinqToJavaScriptConvert : JavaScriptConverter
    {
        private const string Dot = ".";

        #region Public Properties

        public Dictionary<Type, object> JavaScriptChildrenClasses { get; set; }

        public override IEnumerable<Type> SupportedTypes
        {
            get { return JavaScriptChildrenClasses.Keys; }
        }

        #endregion

        #region Public Methods and Operators

        public override object Deserialize(
            IDictionary<string, object> dictionary, Type type, JavaScriptSerializer serializer)
        {
            throw new NotImplementedException();
        }

        public override IDictionary<string, object> Serialize(object obj, JavaScriptSerializer serializer)
        {
            var javaScriptProperties = LinqToJavaScriptExpressionVisitor.GetJavaScriptProperties(
                obj, JavaScriptChildrenClasses);
            var dic = new Dictionary<string, object>();

            var properties = javaScriptProperties
                .Where(r => r.ExpressionGetProperty == null || r.ExpressionSetProperty != null);

            foreach (var property in properties.Where(r => !r.ReadOnly))
            {
                if (property.ChildObject != null)
                {
                    // todo: убрать костыль. нужен другой сериализатор
                    var sb = new StringBuilder();
                    sb.Append("$create(")
                        .Append(property.JavaScriptClassAttribute.Namespace)
                        .Append(Dot)
                        .Append(property.JavaScriptClassAttribute.ClassName)
                        .Append(", ");
                    dic[property.PropertyName + "__" + property.JavaScriptClassAttribute.ClassName + "__"] = sb.ToString();
                    dic[property.JavaScriptClassAttribute.ClassName + "__"] = Serialize(property.ChildObject, serializer);
                    dic[property.JavaScriptClassAttribute.ClassName + "____"] = string.Empty;
                }
                else
                {
                    object value = property.PropertyDescriptor.GetValue(obj);
                    if (value != null)
                        dic[property.PropertyName] = value;
                }
            }

            return dic;
        }

        #endregion
    }
}