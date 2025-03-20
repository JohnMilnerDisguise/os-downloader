using System;
using Newtonsoft.Json;

namespace OSDownloader.Models.JSON
{

    public class TimeSpanConverter : JsonConverter<TimeSpan>
    {
        // Serialize TimeSpan to "hh:mm:ss" format
        public override void WriteJson(JsonWriter writer, TimeSpan value, JsonSerializer serializer)
        {
            writer.WriteValue(value.ToString(@"hh\:mm\:ss"));
        }

        // Deserialize "hh:mm:ss" format to TimeSpan
        public override TimeSpan ReadJson(JsonReader reader, Type objectType, TimeSpan existingValue, bool hasExistingValue, JsonSerializer serializer)
        {
            if (reader.Value == null)
            {
                return TimeSpan.Zero;
            }

            // Parse the string value into a TimeSpan
            if (TimeSpan.TryParse((string)reader.Value, out TimeSpan result))
            {
                return result;
            }

            throw new JsonSerializationException("Invalid TimeSpan format. Expected 'hh:mm:ss'.");
        }
    }
}
