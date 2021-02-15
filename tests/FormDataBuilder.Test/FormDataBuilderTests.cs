using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;
using Xunit;

namespace FormDataBuilder.Test
{
    public class FormDataBuilderTests
    {
        [Fact]
        public void TestFormDataBuilder()
        {
            NestedClass nestedClass = new NestedClass();
            nestedClass.NestedClass2 = new NestedClass2();
            nestedClass.NestedClass2.Count = 123;

            TestClass @class = 
                new TestClass(
                    new Address("123 Xenost Street", 
                        "", 
                        "Moyun", 
                        "AZ", 
                        "USA", 
                        "85203"),
                "SomeoneName", 
                    "$35000",
                    new Dictionary<string, string>() { { "key1", "value1"},
                        {"key2", "value2"} },
                    nestedClass
                    );

            Dictionary<string, string> outputDict = new Dictionary<string, string>();

            FormDataBuilder.CreateFormData(@class, ref outputDict, "");

            Assert.Equal(11, outputDict.Count);

            Assert.True(outputDict.ContainsKey("address[line1]"));
            Assert.True(outputDict.ContainsKey("address[line2]"));
            Assert.True(outputDict.ContainsKey("address[city]"));
            Assert.True(outputDict.ContainsKey("address[state]"));
            Assert.True(outputDict.ContainsKey("address[country]"));
            Assert.True(outputDict.ContainsKey("address[zip]"));
            Assert.True(outputDict.ContainsKey("name"));
            Assert.True(outputDict.ContainsKey("balance"));
            Assert.True(outputDict.ContainsKey("metadata[key1]"));
            Assert.True(outputDict.ContainsKey("metadata[key2]"));
            Assert.True(outputDict.ContainsKey("nested[nestedclass2[count]]"));

            Assert.Equal("123 Xenost Street", outputDict["address[line1]"]);
            Assert.Equal("", outputDict["address[line2]"]);
            Assert.Equal("Moyun", outputDict["address[city]"]);
            Assert.Equal("AZ", outputDict["address[state]"]);
            Assert.Equal("USA", outputDict["address[country]"]);
            Assert.Equal("85203", outputDict["address[zip]"]);
            Assert.Equal("SomeoneName", outputDict["name"]);
            Assert.Equal("$35000", outputDict["balance"]);
            Assert.Equal("value1", outputDict["metadata[key1]"]);
            Assert.Equal("value2", outputDict["metadata[key2]"]);
            Assert.Equal("123", outputDict["nested[nestedclass2[count]]"]);
        }

        [Fact]
        public void TestFormDataBuilderWithAttributes()
        {
            SomeClass @class =
                new SomeClass();

            @class.Name = "TestName";
            @class.Age = 21;
            @class.BigNumber = 100;

            Dictionary<string, string> outputDict = new Dictionary<string, string>();

            FormDataBuilder.CreateFormData(@class, ref outputDict, "");

            Assert.Equal(3, outputDict.Count);

            Assert.True(outputDict.ContainsKey("name"));
            Assert.True(outputDict.ContainsKey("test_age"));
            Assert.True(outputDict.ContainsKey("bignumber"));

            Assert.Equal("TestName", outputDict["name"]);
            Assert.Equal("21", outputDict["test_age"]);
            Assert.Equal("100", outputDict["bignumber"]);
        }

        [Fact]
        public void TestFormDataBuilderWithNullData()
        {
            Dictionary<string, string> outputDict = new Dictionary<string, string>();

            FormDataBuilder.CreateFormData(null, ref outputDict, "");

            Assert.Empty(outputDict);
        }

        [Fact]
        public void TestFormDataBuilderWithNullProperty()
        {
            SomeClass @class =
                new SomeClass();

            @class.Name = "TestName";
            @class.Age = 21;
            @class.BigNumber = null;

            Dictionary<string, string> outputDict = new Dictionary<string, string>();

            FormDataBuilder.CreateFormData(@class, ref outputDict, "");

            Assert.Equal(2, outputDict.Count);

            Assert.True(outputDict.ContainsKey("name"));
            Assert.True(outputDict.ContainsKey("test_age"));
            Assert.False(outputDict.ContainsKey("bignumber"));

            Assert.Equal("TestName", outputDict["name"]);
            Assert.Equal("21", outputDict["test_age"]);
        }

        [Fact]
        public void TestFormDataBuilderWithNullRequiredProperty()
        {
            SomeClass @class =
                new SomeClass();

            @class.Name = null;
            @class.Age = 21;
            @class.BigNumber = null;

            Dictionary<string, string> outputDict = new Dictionary<string, string>();

            Assert.Throws<Exception>(() => FormDataBuilder.CreateFormData(@class, ref outputDict, ""));
        }

        public record TestClass(Address Address, string Name, string Balance, Dictionary<string, string> MetaData, NestedClass Nested);

        public record Address (string Line1, string Line2, string City, string State, string Country, string Zip);

        public class SomeClass
        {
            [Required]
            public string Name { get; set; }

            [JsonPropertyAttribute("test_age")]
            public int Age { get; set; }

            public int? BigNumber { get; set; }
        }

        public class NestedClass
        {
            public NestedClass2 NestedClass2 { get; set; }
        }

        public class NestedClass2
        {
            public int Count { get; set; }
        }
    }
}
