using Axis.Apollo.Json.Data;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Axis.Luna.Extensions;

namespace Axis.Apollo.Json
{
    public class MomentJsDateConverter : JsonConverter
    {
        public DateTimeKind DefaultDateTimeKind = DateTimeKind.Utc;

        public override bool CanConvert(Type objectType) => objectType == typeof(DateTime);

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
            => serializer.Deserialize<MomentDateTime>(reader).ToDateTime(DefaultDateTimeKind);


        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
            => JToken.FromObject(new MomentDateTime(value.As<DateTime>())).WriteTo(writer);
    }
}
