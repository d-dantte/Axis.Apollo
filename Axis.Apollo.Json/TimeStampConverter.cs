using Axis.Apollo.Json.Data;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;

using Axis.Luna.Extensions;

namespace Axis.Apollo.Json
{

    public class TimeSpanConverter : JsonConverter
    {
        public override bool CanConvert(Type objectType) => objectType == typeof(TimeSpan) || objectType == typeof(TimeSpan?);

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
            => serializer.Deserialize<JsonTimeSpan>(reader)?.TimeSpan();


        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
            => JToken.FromObject(value!= null? new JsonTimeSpan(value.As<TimeSpan>()) : null).WriteTo(writer);
    }
}
