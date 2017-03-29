using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using static Axis.Luna.Extensions.EnumerableExtensions;
using System.Linq;
using System.Collections.Generic;
using Axis.Luna;

namespace UnitTestProject1
{
    [TestClass]
    public class UnitTest1
    {
        [TestMethod]
        public void TestMethod1()
        {
            var jsonSetting = new JsonSerializerSettings
            {
                Converters = Enumerate<JsonConverter>()
                    .Append(new Axis.Apollo.Json.TimeSpanConverter())
                    .Append(new Axis.Apollo.Json.DateTimeConverter())
                    .Append(new Axis.Apollo.Json.LazyOperationConverter())
                    .ToList(),
                Formatting = Formatting.Indented
            };

            var op = Operation.TryLazily(() => "stuff");
            var fop = Operation.FailLazily<string>(new Exception());

            var json = JsonConvert.SerializeObject(op, jsonSetting);
            Console.WriteLine(json);

            var fjson = JsonConvert.SerializeObject(fop, jsonSetting);
            Console.WriteLine(fjson);

            var start = DateTime.Now;
            var dop = JsonConvert.DeserializeObject<LazyOperation<string>>(json, jsonSetting);
            Console.WriteLine($"1. Deserialized in {DateTime.Now - start}");

            start = DateTime.Now;
            dop = JsonConvert.DeserializeObject<LazyOperation<string>>(fjson, jsonSetting);
            Console.WriteLine($"2. Deserialized in {DateTime.Now - start}");
        }

        [TestMethod]
        public void TestMethod2()
        {
            var x = JsonConvert.SerializeObject(new Dictionary<string, object> { { "stuff", 5 }, { "otherStuff", true } });
            Console.WriteLine(x);
        }
    }

    public class Obj
    {
        public DateTime BirthDay { get; set; } = DateTime.Now;
        public TimeSpan Duration { get; set; } = TimeSpan.FromDays(0.65);
    }
}
