using Axis.Apollo.Json.Data;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;

using Axis.Luna.Extensions;

namespace Axis.Apollo.Json
{
    public class DateTimeConverter : JsonConverter
    {
        public DateTimeKind DefaultDateTimeKind { get; set; } = DateTimeKind.Utc;

        public override bool CanConvert(Type objectType) => objectType == typeof(DateTime) || objectType == typeof(DateTime?);

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
            => serializer.Deserialize<JsonDateTime>(reader)?.ToDateTime(DefaultDateTimeKind);


        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
            => JToken.FromObject(value!= null? new JsonDateTime(value.As<DateTime>()) : null).WriteTo(writer);
    }
}
