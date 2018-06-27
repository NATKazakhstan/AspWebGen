namespace Nat.Web.Tools.ExtNet.Json
{
    using System;

    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;

    public class DecimalConverter : JsonConverter
    {
        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(decimal) || objectType == typeof(decimal?);
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            var token = JToken.Load(reader);
            if (token.Type == JTokenType.Float || token.Type == JTokenType.Integer)
                return token.ToObject<decimal>();

            if (token.Type == JTokenType.String)
            {
                var str = token.ToString(); /*
                if (CultureInfo.CurrentCulture.NumberFormat.NumberDecimalSeparator == ".")
                    str = str.Replace(",", ".");
                else if (CultureInfo.CurrentCulture.NumberFormat.NumberDecimalSeparator == ",")
                    str = str.Replace(".", ",");*/

                return !string.IsNullOrEmpty(str) ? Convert.ToDecimal(str) : (object) null;
            }

            if (token.Type == JTokenType.Null && objectType == typeof(decimal?))
                return null;

            throw new JsonSerializationException("Unexpected token type: " + token.Type);
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            throw new NotImplementedException();
        }
    }
}