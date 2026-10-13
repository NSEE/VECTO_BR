using AwesomeAssertions;
using Microsoft.Extensions.DependencyInjection;
using VectoInputManager.Commands;
using VectoInputManager.Core;
using VectoObjects;
using VectoObjects.Helpers;

namespace VectoInputManager.Tests.Core;

public class CorpusBuilder_Tests
{
    [TestCase("Vehicle1")]
    [TestCase("Vehicle2")]
    [TestCase("Vehicle3")]
    public void CreateVehicleFromComponents_Vehicle(string folder)
    {
        var provider = TestUtils.TestServiceProvider();
        var corpus = provider.GetRequiredService<CorpusProcessor>();

        var directory = Path.Combine(TestUtils.InputPath, folder);
        var outDir = TestUtils.CombinePath(TestUtils.OutputPath,
            nameof(CreateVehicleFromComponents_Vehicle) + "-" + folder);

        var outputVehicleFile = Path.Combine(outDir, "vehicle.xml");

        foreach (var file in Directory.GetFiles(directory, "*.*"))
        {
            corpus.AddCorpusFile(file);
        }

        corpus.Vehicle.Should().NotBeNull();

        var builder = provider.GetRequiredService<VehicleBuilder>();
        builder.TemplateObject = corpus.Vehicle;

        builder.PromptHandler = action =>
        {
            var path = action.Property;
            var (found, value) = corpus.FindComponentByPath(path);

            if (found)
            {
                action.Answer = value;
            }
            else
                action.Answer = action.TemplateValue;
        };
        builder.Run(null);


        var job = VectoXml.CreateVehicleJobDeclaration(
            builder.Object as IVehicleDeclaration);

        corpus.EnforceSignatureAction((IVehicleDeclaration)job.Vehicle!,
            BrokenSignatureAction.ReSign);

        using (var stream = File.Create(outputVehicleFile))
        {
            XmlUtils.HashAndSerializeJob(stream, job);
        }

        using (var writer = new StreamWriter(Path.Combine(outDir, "Report.txt")))
        {
            foreach (var line in corpus.WriteReport((IVehicleDeclaration)job.Vehicle!, outputVehicleFile))
                writer.WriteLine(line);
        }

        TestUtils.AssertTextFilesAreEquivalent(
           Path.Combine(outDir, "Report.txt"), Path.Combine(directory, "Report.txt"));
    }

    [Test()]
    public void CorpusBuilder_LoadVehicles_Vehicles()
    {
        var provider = TestUtils.TestServiceProvider();
        var corpus = provider.GetRequiredService<CorpusProcessor>();

        var files = ConsoleCommon.ScanFiles(
            [Path.Combine(TestUtils.InputPath, "Vehicles/*.*")], null);

        foreach (var file in files)
        {
            corpus.AddCorpusFile(file);
        }

        corpus.ComponentObjects.Count.Should().Be(7);
    }
}