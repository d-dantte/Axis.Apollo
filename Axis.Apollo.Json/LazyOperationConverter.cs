using Axis.Luna;
using Axis.Luna.Extensions;
using Axis.Luna.MetaTypes;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;

namespace Axis.Apollo.Json
{
    public class LazyOperationConverter : JsonConverter
    {
        private static ConcurrentDictionary<string, Func<object, object>> _lazyOperationFactory = new ConcurrentDictionary<string, Func<object, object>>();

        public override bool CanConvert(Type objectType) => objectType.IsGenericType && objectType.GetGenericTypeDefinition() == typeof(LazyOperation<>);

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            var jobj = JObject.Load(reader);
            bool? succeeded = jobj["Succeeded"]?.As<JValue>().Value.As<bool>();
            if (succeeded == null) throw new InvalidOperationException("cannot deserialize an unresolved LazyOperation");

            else if (succeeded == true) return _lazyOperationFactory.GetOrAdd(objectType.MinimalAQName() + "##", _k =>
            {
                //Expression: _o => Operation.ResolvedFrom<T>((T)_o)
                var opResult = objectType.GetGenericArguments()[0];
                var factoryMethod = typeof(Operation)
                    .GetMethod(nameof(Operation.ResolvedFrom))
                    .MakeGenericMethod(opResult);

                var paramExp = Expression.Parameter(typeof(object), "_o");
                var castExp = Expression.Convert(paramExp, opResult); // (T)_o
                var callExp = Expression.Call(factoryMethod, castExp); //Operation.ResolvedFrom<T>((T)_o);
                var lambda = Expression.Lambda(typeof(Func<object, object>), callExp, paramExp); //_o => Operation.ResolvedFrom<T>((T)_o)

                return (Func<object, object>)lambda.Compile();
            })
            .Invoke(jobj["Result"].As<JValue>().Value);

            else return _lazyOperationFactory.GetOrAdd(objectType.MinimalAQName() + "$$", _k =>
            {
                //Expression: _o => Operatio.FailLazily<T>((string)_o)
                var opResult = objectType.GetGenericArguments()[0];
                var factoryMethod = typeof(Operation)
                    .GetMethods()
                    .Where(_m => _m.Name == nameof(Operation.FailLazily))
                    .Where(_m => _m.IsStatic)
                    .Where(_m => _m.GetParameters().Length == 1 && _m.GetParameters()[0].ParameterType == typeof(string))
                    .FirstOrDefault()
                    .MakeGenericMethod(opResult);

                var paramExp = Expression.Parameter(typeof(object), "_o");
                var castExp = Expression.Convert(paramExp, typeof(string)); // (string)_o
                var callExp = Expression.Call(factoryMethod, castExp); //Operation.FailLazily<T>((string)_o);
                var lambda = Expression.Lambda(typeof(Func<object, object>), callExp, paramExp); //_o => Operation.FailLazily<T>((string)_o)

                return (Func<object, object>)lambda.Compile();
            })
            .Invoke(jobj["Message"]?.As<JValue>().Value ?? "unknown error");
        }


        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            dynamic d = value;
            JToken.FromObject(value == null ? null : new Dictionary<string, object>
            {
                { nameof(LazyOperation<@void>.Result), d.Result},
                { nameof(LazyOperation<@void>.Succeeded), d.Succeeded},
                { nameof(LazyOperation<@void>.Message), d.Message}
            })
            .WriteTo(writer);
        }
    }
}
