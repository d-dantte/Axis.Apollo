using Axis.Apollo.Json.Data;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;

using Axis.Luna.Extensions;

namespace Axis.Apollo.Json
{

    public class CustomTimeSpanConverter : JsonConverter
    {
        public override bool CanConvert(Type objectType) => objectType == typeof(TimeSpan);

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
            => serializer.Deserialize<TimeSpanObject>(reader).TimeSpan();


        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
            => JToken.FromObject(new TimeSpanObject(value.As<TimeSpan>())).WriteTo(writer);
    }
}
