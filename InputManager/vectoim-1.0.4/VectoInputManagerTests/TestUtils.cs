using AwesomeAssertions;
using Microsoft.Extensions.DependencyInjection;
using Spectre.Console.Cli;
using Spectre.Console.Testing;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using System.Xml;
using TUGraz.VectoCore.Utils;
using TUGraz.VectoHashing;
using VectoInputManager.Commands;
using VectoObjects;
using VectoObjects.Helpers;

namespace VectoInputManager.Tests;

public record PromptAnswer(string Query, int Depth, object Answer);

public partial class TestUtils
{
    public static string XMLReaderDeclarationTestFilesPath =
        Path.Combine(
            GetSourceDirectory(),
            @"../../VectoCore/VectoCoreTest/TestData/XML/XMLReaderDeclaration");

    public static string XSDPath =
        Path.Combine(
            GetSourceDirectory(),
            @"../../VectoCore/VectoCore/Resources/XSD");

    public static string XSDTrailerPath =
        Path.Combine(
            GetSourceDirectory(),
            @"../VectoTrailerXSD");

    public static string OutputPath = Path.Combine(GetSourceDirectory(), "Output",
        DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss"));

    public static string InputPath = Path.Combine(GetSourceDirectory(), "Input");

    static TestUtils()
    {
        CombinePath(OutputPath);
    }

    public static CommandApp InitApp()
    {
        var console = new TestConsole();

        var services = new ServiceCollection();
        services.ConfigureServices();
        var registrar = new TypeRegistrar(services);

        var app = new CommandApp(new TypeRegistrar(services));
        app.Configure(cfg => cfg.ConfigureProgramCommands());
        return app;
    }

    public static IServiceProvider TestServiceProvider()
    {
        var services = new ServiceCollection();
        services.ConfigureServices();
        services.AddTransient<TestBuilder>();
        return services.BuildServiceProvider();
    }

    public static string CombinePath(params string[] paths)
    {
        var fullPath = Path.Combine(paths);
        if (!Directory.Exists(fullPath))
        {
            Directory.CreateDirectory(fullPath);
        }
        return fullPath;
    }

    public static void AssertValidateComponentXML(string xmlPath)
    {
        using (var stream = File.OpenRead(xmlPath))
        using (var xmlReader = XmlReader.Create(stream))
        {
            var validator = new XMLValidator(xmlReader);
            bool isValid = validator.ValidateXML(TUGraz.VectoCore.Utils.XmlDocumentType.DeclarationComponentData);
            if (!isValid)
                Assert.Fail($"XML validation failed for {xmlPath}\n" + validator.ValidationError);
        }
    }

    public static void AssertValidateVehicleXML(string xmlPath)
    {
        using (var stream = File.OpenRead(xmlPath))
        using (var xmlReader = XmlReader.Create(stream))
        {
            var validator = new XMLValidator(xmlReader);
            bool isValid = validator.ValidateXML(TUGraz.VectoCore.Utils.XmlDocumentType.DeclarationJobData);
            if (!isValid)
                Assert.Fail($"XML validation failed for {xmlPath}\n" + validator.ValidationError);
        }
    }

    public static void AssertValidateHash(string file)
    {
        using (var stream = File.OpenRead(file))
        {
            var hash = VectoHash.Load(stream);
            hash.ValidateHash().Should().BeTrue("Hash validation failed");
        }
    }

    public static void AssertTextFilesAreEquivalent(string actual, string expected)
    {
        // Prepare the file information string that will be appended to all error messages
        var fileInfoMessage = $"\nActual: {actual}\nExpected: {expected}\n";

        var actualLines = File.ReadAllLines(actual);
        var expectedLines = File.ReadAllLines(expected);

        // Compare line counts
        actualLines.Length.Should().Be(expectedLines.Length,
            $"Files have different number of lines. Actual: {actualLines.Length}, Expected: {expectedLines.Length}{fileInfoMessage}");

        // Compare each line
        for (int i = 0; i < expectedLines.Length; i++)
        {
            var actualLine = actualLines[i];
            var expectedLine = expectedLines[i];

            actualLine.Should().Be(expectedLine,
                $"Line {i + 1} differs{fileInfoMessage}Expected: \"{expectedLine}\"\nActual: \"{actualLine}\"");
        }
    }

    public static void AssertXmlFilesAreEquivalent(string actual, string expected,
        bool removeMonitoringData = false)
    {
        // Prepare the file information string that will be appended to all error messages
        var fileInfoMessage = $"\nActual: {actual}\nExpected: {expected}\n";

        var doc1 = new XmlDocument();
        var doc2 = new XmlDocument();
        doc1.Load(actual);
        doc2.Load(expected);

        if (removeMonitoringData)
        {
            RemoveMonitoringDataNodes(doc1);
            RemoveMonitoringDataNodes(doc2);
        }

        void CompareNodesRecursive(XmlNode node1, XmlNode node2, string path)
        {
            node1.LocalName.Should().Be(node2.LocalName, $"Node name mismatch at {path}{fileInfoMessage}");

            // Skip comparison for AppVersion, Date and Signature nodes.

            if (Regex.IsMatch(path, @"Signature\[\d+\]")
                || Regex.IsMatch(path, @"Date\[\d+\]/#text\[0\]$")
                || Regex.IsMatch(path, @"AppVersion\[\d+\]/#text\[0\]$"))
            {
                return;
            }

            // Compare values for text and CDATA nodes
            if (node1 is XmlText || node1 is XmlCDataSection)
            {
                node1.Value.Should().Be(node2.Value, $"Node value mismatch at {path}{fileInfoMessage}");
            }

            // Compare attributes, ignoring namespace declarations
            if (node1.Attributes != null && node2.Attributes != null)
            {
                // Filter out namespace declaration attributes and schemaVersion
                var attrs1 = node1.Attributes.Cast<XmlAttribute>()
                    .Where(a => !a.Name.StartsWith("xmlns", StringComparison.OrdinalIgnoreCase)
                             && !a.LocalName.Equals("schemaVersion", StringComparison.OrdinalIgnoreCase)
                             && !a.LocalName.Equals("schemaLocation", StringComparison.OrdinalIgnoreCase))
                    .ToList();

                var attrs2 = node2.Attributes.Cast<XmlAttribute>()
                    .Where(a => !a.Name.StartsWith("xmlns", StringComparison.OrdinalIgnoreCase)
                             && !a.LocalName.Equals("schemaVersion", StringComparison.OrdinalIgnoreCase)
                             && !a.LocalName.Equals("schemaLocation", StringComparison.OrdinalIgnoreCase))
                    .ToList();

                if (attrs1.Count != attrs2.Count)
                {
                    var typeNode1 = attrs1.FirstOrDefault(c => c.Name == "xsi:type");
                    var typeNode2 = attrs2.FirstOrDefault(c => c.Name == "xsi:type");
                    if (typeNode1 != null && typeNode2 == null)
                        attrs1.Remove(typeNode1);
                    if (typeNode1 == null && typeNode2 != null)
                        attrs2.Remove(typeNode2);
                }

                attrs1.Count.Should().Be(attrs2.Count, $"Attribute count mismatch at {path}{fileInfoMessage}");

                foreach (var attr1 in attrs1)
                {
                    var attr2 = attrs2.FirstOrDefault(a => a.LocalName == attr1.LocalName);
                    attr2.Should().NotBeNull($"Missing attribute '{attr1.Name}' at {path}{fileInfoMessage}");

                    var val1 = attr1.Value.Split(":").Last();
                    var val2 = attr2!.Value.Split(":").Last();

                    // Ignore differences in 'id' attributes
                    if (attr1.Name != "id")
                        val1.Should().Be(val2, $"Attribute value mismatch for '{attr1.Name}' at {path}{fileInfoMessage}");
                }
            }

            // Compare child nodes (excluding comments and whitespace)
            var children1 = node1.ChildNodes.Cast<XmlNode>()
                .Where(n => n.NodeType != XmlNodeType.Comment && n.NodeType != XmlNodeType.Whitespace).ToList();
            var children2 = node2.ChildNodes.Cast<XmlNode>()
                .Where(n => n.NodeType != XmlNodeType.Comment && n.NodeType != XmlNodeType.Whitespace).ToList();

            children1.Count.Should().Be(children2.Count, $"Child node count mismatch at {path}{fileInfoMessage}");

            for (int i = 0; i < children1.Count; i++)
            {
                CompareNodesRecursive(children1[i], children2[i], $"{path}/{children1[i].Name}[{i}]");
            }
        }

        // Initiate comparison
        CompareNodesRecursive(doc1.DocumentElement!, doc2.DocumentElement!, doc1.DocumentElement!.Name);

        void RemoveMonitoringDataNodes(XmlDocument document)
        {
            var monitoringNodes = document.GetElementsByTagName("MonitoringData");

            // Convert to array to avoid modification during enumeration
            var nodesToRemove = monitoringNodes.Cast<XmlNode>().ToArray();

            foreach (var node in nodesToRemove)
            {
                node.ParentNode?.RemoveChild(node);
            }
        }
    }

    public static void AssertFileSizes(string[] files, uint tolerance, Dictionary<string, int> expectedFileSizes)
    {
        foreach (var jsonFile in files)
        {
            var fileName = Path.GetFileName(jsonFile);
            var expectedSize = expectedFileSizes[fileName];

            var fileInfo = new FileInfo(jsonFile);
            var actualSize = (int)fileInfo.Length;

            // Allow for variance in file size
            actualSize.Should().BeCloseTo(expectedSize, tolerance,
                $"File {fileName} should be approximately {expectedSize} bytes but was {actualSize} bytes");
        }
    }

    public static IComponentDeclaration LoadComponentFromXml(string filePath)
    {
        using var fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read);
        if (!VectoXml.TryGetSerializer(fileStream, out var serializer, out _, out var error))
            throw new InvalidOperationException(error);

        var item = serializer.Deserialize(fileStream)!;

        foreach (var prop in item.GetType().GetProperties())
        {
            if (prop.PropertyType.IsClass && prop.PropertyType != typeof(string))
            {
                var value = prop.GetValue(item);
                if (value is IComponentDeclaration comp)
                    return comp;
            }
        }

        throw new InvalidOperationException("No component declaration found.");
    }

    public static void InitGeneration() => InitGenerationInternal();

    static partial void InitGenerationInternal();

    private static string GetSourceDirectory([CallerFilePath] string callerFilePath = "")
    {
        return Path.GetDirectoryName(callerFilePath)!;
    }
}