using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using static Axis.Luna.Extensions.EnumerableExtensions;
using System.Linq;

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
                    .ToList(),
                Formatting = Formatting.Indented
            };

            var json = JsonConvert.SerializeObject(new Obj(), jsonSetting);
            Console.WriteLine(json);
        }
    }

    public class Obj
    {
        public DateTime BirthDay { get; set; } = DateTime.Now;
        public TimeSpan Duration { get; set; } = TimeSpan.FromDays(0.65);
    }
}
