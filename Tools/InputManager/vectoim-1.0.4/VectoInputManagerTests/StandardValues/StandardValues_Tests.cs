using AwesomeAssertions;
using TUGraz.VectoHashing;
using VectoInputManager.Core;

namespace VectoInputManager.Tests.StandardValues;

[TestFixture]
public class StandardValues_Tests
{
    [Test]
    public void InputOutput_Tests()
    {
        var componentTypes = Components.ComponentInformation
            .Select(ct => ct.ComponentType.ToString()).ToArray();

        foreach (var componentType in Components.ComponentInformation)
        {
            TestComponent(componentType);
        }
    }

    private static void TestComponent(Components.ComponentTypeInfo componentType)
    {
        var inputFolder = TestUtils.CombinePath(TestUtils.InputPath,
            "StandardValues", componentType.ComponentType.ToString());

        var outputFolder = TestUtils.CombinePath(TestUtils.OutputPath, nameof(InputOutput_Tests));

        var calcType = componentType.Calc;

        // Get all JSON files in the input folder. Each JSON file represents a test case,
        // containing input data and the expected output XML file.
        var jsonFiles = Directory.GetFiles(inputFolder, "*.json", SearchOption.AllDirectories);

        foreach (var jsonFile in jsonFiles)
        {
            var jsonContent = File.ReadAllText(jsonFile);
            var inputData = JsonInputUtils.Deserialize(jsonContent);

            var calculator = (BaseStandardValues)Activator.CreateInstance(calcType)!;
            var calculatedComponent = calculator.Calculate(inputData);

            // Create the output XML file path
            var xmlFileName = Path.GetFileNameWithoutExtension(jsonFile) + ".xml";
            var calculatedXmlFile = Path.Combine(outputFolder, xmlFileName);

            // Generate the XML with the calculated component
            var vInputDeclaration = TypeUtils.CreateVectoInputDeclaration();
            calculator.SetInput(vInputDeclaration, calculatedComponent);

            using (var fs = File.Create(calculatedXmlFile))
                XmlUtils.HashAndSerialize(fs, vInputDeclaration, inputData);

            var expectedXmlFile = Path.Combine(inputFolder, xmlFileName);

            // Compare the calculated XML with the expected XML
            TestUtils.AssertXmlFilesAreEquivalent(calculatedXmlFile, expectedXmlFile);

            // Validate the generated XML

            // Todo: IEPC is not implemented yet, so skip it for now
            if (componentType.ComponentType != ComponentType.IEPC)
                TestUtils.AssertValidateComponentXML(calculatedXmlFile);

            // Validate the hash
            VectoHash.Load(calculatedXmlFile).ValidateHash().Should().BeTrue();
        }
    }
}
