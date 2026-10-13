using System;
using System.Globalization;
using AwesomeAssertions;
using Microsoft.Extensions.DependencyInjection;
using VectoInputManager.Core;
using VectoObjects;

namespace VectoInputManager.Tests.Core;

public class ObjectBuilder_Tests
{
    [Test]
    public void Run_Test()
    {
        Thread.CurrentThread.CurrentCulture = CultureInfo.GetCultureInfo("en-US");
        var provider = TestUtils.TestServiceProvider();
        var builder = provider.GetRequiredService<TestBuilder>();

        // Input object supplies default values that should appear as DefaultValue in enum PromptActions
        var template = new Test_Object()
        {
            EnumProp = Test_Enum.OptionC,
            ObjectList =
            [
                new Test_Nested { Name = "Initial", Value = 1 }
            ],
            EnumProp2 = Test_Enum.OptionB,
            DecimalProp = 123.45m,
            DecimalArray = [10.5m, 20.75m]
        };

        builder.TemplateObject = template;

        int collectionPrompts = 0;
        builder.PromptHandler = action =>
        {
            object? result()
            {
                switch (action.Type)
                {
                    case PromptActionType.Enum:
                        if (action.Property == "EnumProp") return "OptionA";
                        if (action.Property == "EnumProp2") return "OptionB";
                        break;

                    case PromptActionType.Collection:
                        if (collectionPrompts == 0)
                        {
                            collectionPrompts++;
                            return "Test_Nested";
                        }

                        return "No";

                    case PromptActionType.String:
                        if (action.Property == "ObjectList[0].Name")
                            return "Modified";

                        if (action.Property == "DecimalArray")
                            return "5.25,15.75";
                        break;

                    case PromptActionType.Integer:
                        if (action.Property == "ObjectList[0].Value")
                            return 42;
                        break;

                    case PromptActionType.Decimal:
                        if (action.Property == "DecimalProp")
                            return 99.99m;
                        break;
                }
                return null;
            }

            action.Answer = result();
        };

        var final = new Test_Object(); // Start object with default enum values (OptionA, OptionA)
        builder.Run(final);

        // Enum assertions (values applied)
        final.EnumProp.Should().Be(Test_Enum.OptionA);
        final.EnumProp2.Should().Be(Test_Enum.OptionB);

        // Collection / item assertions
        final.ObjectList.Count.Should().Be(1);
        final.ObjectList[0].Name.Should().Be("Modified");
        final.ObjectList[0].Value.Should().Be(42);

        // Decimal property assertions
        final.DecimalProp.Should().Be(99.99m);
        final.DecimalArray.Should().NotBeNull();
        final.DecimalArray.Length.Should().Be(2);
        final.DecimalArray[0].Should().Be(5.25m);
        final.DecimalArray[1].Should().Be(15.75m);

        builder.Actions.Count.Should().Be(12);

        // Verify specific actions exist
        builder.Actions.Any(a => a.Property == "EnumProp").Should().BeTrue();
        builder.Actions.Any(a => a.Property == "ObjectList[0]").Should().BeTrue();
        builder.Actions.Any(a => a.Property == "ObjectList[0].Name").Should().BeTrue();
        builder.Actions.Any(a => a.Property == "ObjectList[0].Value").Should().BeTrue();
        builder.Actions.Any(a => a.Property == "ObjectList[1]").Should().BeTrue();
        builder.Actions.Any(a => a.Property == "EnumProp2").Should().BeTrue();
        builder.Actions.Any(a => a.Property == "DecimalProp").Should().BeTrue();
        builder.Actions.Any(a => a.Property == "DecimalArray").Should().BeTrue();

        // DefaultValue checks: ensure they reflect the original input object's enum values
        var enumAction1 = builder.Actions.First(a => a.Property == "EnumProp");
        var enumAction2 = builder.Actions.First(a => a.Property == "EnumProp2");
        enumAction1.TemplateValue.Should().Be(Test_Enum.OptionC);
        enumAction2.TemplateValue.Should().Be(Test_Enum.OptionB);

        // DefaultValue checks for decimal properties
        var numberAction = builder.Actions.First(a => a.Property == "DecimalProp");
        numberAction.TemplateValue.Should().Be(123.45m);

        var arrayAction1 = builder.Actions.First(a => a.Property == "DecimalArray");
        arrayAction1.TemplateValue.Should().Be("10.5,20.75");

        var item2Action = builder.Actions.First(a => a.Property == "ObjectList[1]");
        item2Action.TemplateValue.Should().Be("No");
    }
}

