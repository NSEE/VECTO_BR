using AwesomeAssertions;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Schema;
using VectoInputManager.Core;
using VectoObjects;
using VectoObjects.Helpers;

namespace VectoInputManager.Tests.Samples;

public class SampleProcessing_Tests
{
    [Test]
    public void SampleVehicles_SaveToJsonAndValidateSchema()
    {
        var inputDir = Path.Combine(TestUtils.InputPath, "Samples");
        var outputDir = TestUtils.CombinePath(TestUtils.OutputPath, nameof(SampleVehicles_SaveToJsonAndValidateSchema));
        var types = TypeUtils.GetAllVehicleTypes()
            .Where(t => !t.Name.Contains("v1_0"))
            .Where(t => !t.Name.Contains("v2_0"))
            .Where(t => !t.Name.Contains("v2_1"))
            .Where(t => !t.Name.Contains("v2_2_1"))
            .Where(t => !t.Name.Contains("v2_4"))
            .ToList();

        foreach (var xmlFile in Directory.EnumerateFiles(inputDir, "*.xml"))
        {
            //if (!xmlFile.Contains("SHEV_S4_HeavyLorry.xml")) continue;

            TestUtils.AssertValidateVehicleXML(xmlFile);

            VectoXml.TryGetSerializer(xmlFile, out var serializer, out var rootType, out var error);
            var job = (IVehicleJobDeclaration)serializer.Deserialize(File.OpenRead(xmlFile))!;
            types.Remove(job.Vehicle.GetType());

            foreach (var path in XmlUtils.ValidateVehicleJobHashes(job))
            {
                System.Console.WriteLine($"Hash validation failed for file {xmlFile}: {path}");
            }

            var outputXml = Path.Combine(outputDir, Path.GetFileName(xmlFile));
            using (var stream = File.Create(outputXml))
            {
                XmlUtils.HashAndSerializeJob(stream, job);
            }

            TestUtils.AssertXmlFilesAreEquivalent(outputXml, xmlFile);
            TestUtils.AssertValidateVehicleXML(outputXml);

            var outputFileJson = Path.Combine(outputDir, Path.GetFileNameWithoutExtension(xmlFile) + ".json");

            var vehicle = job.Vehicle;

            if (vehicle != null)
            {
                JsonDeclarationUtils.SerializeToFile(job.Vehicle, outputFileJson, true);
            }

            var schemaJson = Path.Combine(TestUtils.InputPath, "../../VehicleSchemas",
                job.Vehicle.GetType().Name + ".schema.json");

            var jobject = JObject.Parse(File.ReadAllText(outputFileJson));
            JSchema schema = JSchema.Parse(File.ReadAllText(schemaJson));

            bool valid = jobject.IsValid(schema, out IList<string> messages);

            var deserializedVehicle = JsonDeclarationUtils.Deserialize(File.ReadAllText(outputFileJson))
                as IVehicleDeclaration;
            deserializedVehicle.Should().NotBeNull();

            if (!valid)
            {
                Assert.Fail($"JSON schema validation failed for {outputFileJson}:\n" + string.Join("\n", messages));
            }


            var job2 = VectoXml.CreateVehicleJobDeclaration(deserializedVehicle);

            var outputXml2 = Path.Combine(outputDir,
                Path.GetFileNameWithoutExtension(xmlFile) + "_json.xml");

            // Output XML format (always create for validation)
            using (var stream = File.Create(outputXml2))
            {
                XmlUtils.HashAndSerializeJob(stream, job2);
            }

            TestUtils.AssertXmlFilesAreEquivalent(outputXml2, xmlFile, removeMonitoringData: true);
        }

        if (types.Count > 0)
        {
            Assert.Fail("Not all vehicle types were tested: " + string.Join(", ", types.Select(t => t.Name)));
        }
    }

    [Test]
    public void SampleComponents_SaveToJsonAndValidateSchema()
    {
        var inputDir = Path.Combine(TestUtils.InputPath, "Samples");
        var outputDir = TestUtils.CombinePath(TestUtils.OutputPath, nameof(SampleComponents_SaveToJsonAndValidateSchema));

        foreach (var xmlFile in Directory.EnumerateFiles(inputDir, "*.xml"))
        {
            using var fs = File.OpenRead(xmlFile);
            VectoXml.TryGetSerializer(fs, out var serializer, out var rootType, out var error);
            var job = serializer.Deserialize(fs) as IVehicleJobDeclaration;

            var vehicle = job!.Vehicle;

            // Extract all components from the vehicle
            var components = TypeUtils.FindComponentDelarations(vehicle);

            foreach (var component in components)
            {
                var componentType = component.component.GetType();
                var outputJson = Path.Combine(outputDir,
                    $"{Path.GetFileNameWithoutExtension(xmlFile)}_{component.path}.json");
                JsonDeclarationUtils.SerializeToFile(component.component, outputJson, true);

                var schemaJson = Path.Combine(TestUtils.InputPath, "../../ComponentSchemas", componentType.Name + ".schema.json");
                var jobject = JObject.Parse(File.ReadAllText(outputJson));
                JSchema schema = JSchema.Parse(File.ReadAllText(schemaJson));

                bool valid = jobject.IsValid(schema, out IList<string> messages);

                var deserializedComponent = JsonDeclarationUtils.Deserialize(File.ReadAllText(outputJson))
                    as IComponentDeclaration;
                deserializedComponent.Should().NotBeNull();

                if (!valid)
                {
                    Assert.Fail($"JSON schema validation failed for {outputJson}:\n" + string.Join("\n", messages));
                }

                // Output XML format from original component

                var outputXml = Path.Combine(outputDir,
                    $"{Path.GetFileNameWithoutExtension(xmlFile)}_{component.path}.xml");

                var comp = new VectoInputDeclaration_v2_2();
                comp.SetComponentInVectoInputDeclaration(component.component);

                using (var stream = File.Create(outputXml))
                {
                    XmlUtils.HashAndSerialize(stream, comp, null);
                }

                // Output XML format from JSON deserialized component

                var outputXml2 = Path.Combine(outputDir,
                    $"{Path.GetFileNameWithoutExtension(xmlFile)}_{component.path}_json.xml");

                comp = new VectoInputDeclaration_v2_2();
                comp.SetComponentInVectoInputDeclaration(deserializedComponent);

                using (var stream = File.Create(outputXml2))
                {
                    XmlUtils.HashAndSerialize(stream, comp, null);
                }

                TestUtils.AssertXmlFilesAreEquivalent(outputXml2, outputXml);
            }
        }
    }
}