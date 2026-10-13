using System;
using AwesomeAssertions;
using Fasterflect;
using VectoInputManager.Core;
using VectoInputManager.Tests.Utilities;
using VectoObjects;
using VectoObjects.Helpers;

namespace VectoInputManager.Tests.Core;

public class XmlUtils_Tests
{
    [Test]
     public void SyntheticVehiclesToJsonAndXml_Test()
    {
        var outputDir = TestUtils.CombinePath(TestUtils.OutputPath,
            nameof(SyntheticVehiclesToJsonAndXml_Test));

        foreach (var vt in Data.GetVehicleTypes())
        {
            var vehicle = (IVehicleDeclaration)vt.FindType().CreateInstance();

            if (vt.TypeName != nameof(VehicleDeclarationType_v1_0))
                vehicle.SetExampleValues();

            // Output Json
            var outputFileJson = Path.Combine(outputDir, vt.TypeName + ".json");
            JsonDeclarationUtils.SerializeToFile(vehicle, outputFileJson, true);

            // Can we read it back?
            var deserializedVehicle = JsonDeclarationUtils.DeserializeFromFile(outputFileJson);

            // Output XML
            var outputFile = Path.Combine(outputDir, vt.TypeName + ".xml");
            using var fs = File.Create(outputFile);

            var job = VectoXml.CreateVehicleJobDeclaration(vehicle);

            XmlUtils.HashAndSerializeJob(fs, job);
        }
    }

    [Test]
    public void XMLSignatureRead_Test()
    {
        var inDir = Path.Combine(TestUtils.InputPath, "Signatures");

        var file = Path.Combine(inDir, "Vehicle-Valid.xml");
        using var fs = File.OpenRead(file);
        VectoXml.TryGetSerializer(fs, out var serializer, out var rootType, out var error);

        var vehicleJob = (IVehicleJobDeclaration)serializer.Deserialize(fs)!;

        var components = TypeUtils.FindComponentDelarations(vehicleJob.Vehicle);
        foreach (var component in components)
        {
            var sig = component.component.GetSignature();
            sig.Should().NotBeNull();
        }
    }

    [Test]
    public void XMLHashValidation()
    {
        var inDir = Path.Combine(TestUtils.InputPath, "Validation");

        {
            var file = Path.Combine(inDir, "2.4-PEV_heavyLorry_E4.xml");
            using var fs = File.OpenRead(file);
            VectoXml.TryGetSerializer(fs, out var serializer, out var rootType, out var error);
            var vehicleJob = (IVehicleJobDeclaration)serializer.Deserialize(fs)!;
            var errors = XmlUtils.ValidateVehicleJobHashes(vehicleJob);
            errors.Where(c => c.result != SignatureValidation.Valid).Should().HaveCount(2);
        }
        {
            var file = Path.Combine(inDir, "2.7-Multiple_FCHV_F2_IEPC_HeavyLorry.xml");
            using var fs = File.OpenRead(file);
            VectoXml.TryGetSerializer(fs, out var serializer, out var rootType, out var error);
            var vehicleJob = (IVehicleJobDeclaration)serializer.Deserialize(fs)!;
            var errors = XmlUtils.ValidateVehicleJobHashes(vehicleJob);
            errors.Where(c => c.result != SignatureValidation.Valid).Should().HaveCount(11);
        }
    }

}