using AwesomeAssertions;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json.Linq;
using VectoInputManager.Core;
using VectoObjects;

namespace VectoInputManager.Tests.Core;

public class ObjectBuilder_JObject_Tests
{
    private TestBuilder builder;

    [SetUp]
    public void Setup()
    {
        var provider = TestUtils.TestServiceProvider();
        builder = provider.GetRequiredService<TestBuilder>();
    }

    private T Execute<T>(T template, JObject source)
        where T : new()
    {
        builder.TemplateObject = template;
        builder.PromptHandler = action =>
        {
            var path = action.Property;

            var (found, value) = PathUtils.FindJObject(source, action, null);

            if (found)
            {
                action.Answer = value;
            }
            else
            {
                action.Answer = action.TemplateValue;
            }
        };

        var final = new T();
        builder.Run(final);
        return final;
    }

    [Test]
    public void MergeIntoTarget_ObjectList_CreatesObjectInstances()
    {
        // Arrange
        var target = new Test_Object { ObjectList = [] };

        var source = JObject.Parse(@"{
        ""ObjectList"": [
         { ""Name"": ""First"", ""Value"": 10 },
         { ""Name"": ""Second"", ""Value"": 20 }
        ] }");

        // Act
        Test_Object final = Execute(target, source);

        // Assert
        final.ObjectList.Should().NotBeNull();
        final.ObjectList.Count.Should().Be(2);
        final.ObjectList[0].Name.Should().Be("First");
        final.ObjectList[0].Value.Should().Be(10);
        final.ObjectList[1].Name.Should().Be("Second");
        final.ObjectList[1].Value.Should().Be(20);
    }

    [Test]
    public void MergeIntoTarget_ObjectListWithValuesConvention_CreatesObjects()
    {
        // Arrange
        var target = new Test_Object { ObjectList = [] };

        var source = JObject.Parse(@"{
         ""ObjectList"": {
           ""$values"": [
              { ""Name"": ""Alpha"", ""Value"": 1 },
              { ""Name"": ""Beta"", ""Value"": 2 }
            ]
         } }");

        // Act
        Test_Object final = Execute(target, source);

        // Assert
        final.ObjectList.Should().NotBeNull();
        final.ObjectList.Count.Should().Be(2);
        final.ObjectList[0].Name.Should().Be("Alpha");
        final.ObjectList[1].Name.Should().Be("Beta");
    }

    [Test]
    public void MergeIntoTarget_SimpleProperties_UpdatesValues()
    {
        // Arrange
        var target = new Test_Object
        {
            StringProp = "original",
            IntProp = 10,
            DecimalProp = 5.5m,
            BoolProp = false
        };

        var source = JObject.Parse(@"{
            ""StringProp"": ""updated"",
            ""IntProp"": 20,
            ""DecimalProp"": 10.5,
            ""BoolProp"": true
        }");

        Test_Object final = Execute(target, source);

        // Assert
        final.StringProp.Should().Be("updated");
        final.IntProp.Should().Be(20);
        final.DecimalProp.Should().Be(10.5m);
        final.BoolProp.Should().BeTrue();
    }

    [Test]
    public void MergeIntoTarget_PartialUpdate_OnlyUpdatesSpecifiedProperties()
    {
        // Arrange
        var target = new Test_Object
        {
            StringProp = "original",
            IntProp = 10,
            DecimalProp = 5.5m
        };

        var source = JObject.Parse(@"{
            ""StringProp"": ""updated""
        }");

        // Act
        Test_Object final = Execute(target, source);

        // Assert
        final.StringProp.Should().Be("updated");
        final.IntProp.Should().Be(10); // Unchanged
        final.DecimalProp.Should().Be(5.5m); // Unchanged
    }

    [Test]
    public void MergeIntoTarget_SkipsSpecialProperties()
    {
        // Arrange
        var target = new Test_Object { StringProp = "original" };

        var source = JObject.Parse(@"{
            ""$type"": ""SomeType"",
            ""$schema"": ""SomeSchema"",
            ""StringProp"": ""updated""
        }");

        // Act
        Test_Object final = Execute(target, source);

        // Assert
        final.StringProp.Should().Be("updated");
        // No exception should be thrown for $type and $schema
    }

    [Test]
    public void MergeIntoTarget_NestedObject_UpdatesNestedProperties()
    {
        // Arrange
        var target = new Test_Object
        {
            NestedObject = new VectoObjects.Test_Nested
            {
                Name = "original",
                Value = 100
            }
        };

        var source = JObject.Parse(@"{
        ""NestedObject"": {
                ""Name"": ""updated"",
                ""Value"": 200
        } }");

        // Act
        Test_Object final = Execute(target, source);

        // Assert
        final.NestedObject.Should().NotBeNull();
        final.NestedObject.Name.Should().Be("updated");
        final.NestedObject.Value.Should().Be(200);
    }

    [Test]
    public void MergeIntoTarget_NestedObject_CreatesIfNull()
    {
        // Arrange
        var target = new Test_Object { NestedObject = null };
        var source = JObject.Parse(@"{
            ""NestedObject"": {
                ""Name"": ""created"",
                ""Value"": 300
         } }");

        // Act
        Test_Object final = Execute(target, source);

        // Assert
        final.NestedObject.Should().NotBeNull();
        final.NestedObject.Name.Should().Be("created");
        final.NestedObject.Value.Should().Be(300);
    }

    [Test]
    public void MergeIntoTarget_PartialNestedUpdate_PreservesOtherProperties()
    {
        // Arrange
        var target = new Test_Object
        {
            NestedObject = new VectoObjects.Test_Nested
            {
                Name = "original",
                Value = 100
            }
        };

        var source = JObject.Parse(@"{
        ""NestedObject"": {
            ""Name"": ""updated""
    } }");

        // Act
        Test_Object final = Execute(target, source);

        // Assert
        final.NestedObject.Name.Should().Be("updated");
        final.NestedObject.Value.Should().Be(100); // Unchanged
    }

    [Test]
    public void MergeIntoTarget_ComplexScenario_UpdatesAllLevels()
    {
        // Arrange
        var target = new Test_Object
        {
            StringProp = "original",
            IntProp = 10,
            NestedObject = new VectoObjects.Test_Nested
            {
                Name = "nested original",
                Value = 50
            },
            ObjectList =
            [
                new VectoObjects.Test_Nested { Name = "item1", Value = 10 }
      ]
        };

        var source = JObject.Parse(@"{
        ""StringProp"": ""updated"",
        ""NestedObject"": {
            ""Value"": 999
          },
            ""ObjectList"": [
                { ""Name"": ""new1"", ""Value"": 100 },
                { ""Name"": ""new2"", ""Value"": 200 }
            ]
        }");

        // Act
        Test_Object final = Execute(target, source);

        // Assert
        final.StringProp.Should().Be("updated");
        final.IntProp.Should().Be(10); // Unchanged
        final.NestedObject.Name.Should().Be("nested original"); // Unchanged
        final.NestedObject.Value.Should().Be(999); // Updated
        final.ObjectList.Count.Should().Be(2);
        final.ObjectList[0].Name.Should().Be("new1");
        final.ObjectList[1].Name.Should().Be("new2");
    }

    [Test]
    public void MergeIntoTarget_NullValues()
    {
        // Arrange
        var target = new Test_Object
        {
            StringProp = "original",
            NestedObject = new VectoObjects.Test_Nested { Name = "test" }
        };

        var source = JObject.Parse(@"{
            ""StringProp"": null
        }");

        // Act
        Test_Object final = Execute(target, source);

        // Assert
        final.StringProp.Should().Be(null);
    }
}

