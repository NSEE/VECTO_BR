using System;
using AwesomeAssertions;
using VectoInputManager.Core;
using VectoObjects;

namespace VectoInputManager.Tests.Core;

public class JsonDeclarationUtils_Tests
{
    [Test]
    public void Serialize_Deserialize_CustomComponentDeclaration_WithComplexProperties()
    {
        // Arrange
        var component = new TestComponentDeclaration
        {
            Signature = new SignatureType_v2_0
            {
                Reference = new ReferenceType()
            },

            DeltaCdxA_CFD = 1.25m,
            Value = 2.50m,
        };

        var json = JsonDeclarationUtils.Serialize(component, addSchema: false);

        json.Should().NotBeNullOrEmpty();
        json.Should().Contain("\"DeltaCdxA_CFD\": 1.25");
        json.Should().NotContain("DeltaCdxA_CFDValueSpecified");
        json.Should().NotContain("DeltaCdxA_CFDValue");
        json.Should().Contain("\"Value\": 2.5");
        json.Should().Contain("\"$type\"");

        var deserializedComponent = JsonDeclarationUtils.Deserialize(json) as TestComponentDeclaration;
        deserializedComponent.Should().NotBeNull();
        deserializedComponent.DeltaCdxA_CFD.Should().Be(1.25m);
        deserializedComponent.DeltaCdxA_CFDValue.Should().Be(1.25m);
        deserializedComponent.DeltaCdxA_CFDValueSpecified.Should().BeTrue();
        deserializedComponent.Value.Should().Be(2.50m);
    }

    [Test]
    public void SerializeToFile_DeserializeFromFile_CustomComponentDeclaration()
    {
        // Arrange
        var component = new TestComponentDeclaration
        {
            Signature = new SignatureType_v2_0
            {
                Reference = new ReferenceType()
            },
            DeltaCdxA_CFDValueSpecified = false,
            Value = 3.75m,
        };

        var json = JsonDeclarationUtils.Serialize(component, addSchema: false);
        json.Should().NotContain("DeltaCdxA_CFD");
        json.Should().Contain("\"Value\": 3.75");

        var deserializedComponent = JsonDeclarationUtils.Deserialize(json) as TestComponentDeclaration;

        deserializedComponent!.DeltaCdxA_CFDValueSpecified.Should().BeFalse();
        deserializedComponent!.DeltaCdxA_CFDValue.Should().Be(0);
        deserializedComponent!.DeltaCdxA_CFD.Should().Be(null);
        deserializedComponent!.Value.Should().Be(3.75m);
    }

}