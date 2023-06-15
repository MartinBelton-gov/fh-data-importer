using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System.Globalization;
using System;

namespace HounslowconnectImporter.Services
{
    public class CustomDateTimeConverter : DateTimeConverterBase
    {
#pragma warning disable CS8765
        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            if (reader.Value == null)
            {
                return default!;
            }
            DateTime result;
            if (DateTime.TryParseExact(reader.Value.ToString(),
                       "yyyy-dd-MM hh:mm tt",
                       CultureInfo.InvariantCulture,
                       DateTimeStyles.None,
                       out result))
            {
                return result;
            }

            if (DateTime.TryParseExact(reader.Value.ToString(),
                       "dd/MM/yyyy HH:mm:ss",
                       CultureInfo.InvariantCulture,
                       DateTimeStyles.None,
                       out result))
            {
                return result;
            }

            if (DateTime.TryParseExact(reader.Value.ToString(),
                       "yyyy-MM-dd'T'HH:mm:sszzz",
                       CultureInfo.InvariantCulture,
                       DateTimeStyles.None,
                       out result))
            {
                return result;
            }

            return default!;
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            writer.WriteValue(((DateTime)value).ToString("yyyy-MM-dd'T'HH:mm:sszzz"));
        }
#pragma warning restore CS8765
    }
}
