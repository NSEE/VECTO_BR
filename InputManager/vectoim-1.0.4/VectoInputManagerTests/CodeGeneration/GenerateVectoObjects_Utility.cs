using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using XmlSchemaClassGenerator;

namespace VectoInputManager.Tests.CodeGeneration;

[Explicit("Utility to generate C# XML classes from Vecto XSDs")]
public partial class GenerateVectoObjects_Utility
{
    private string _tempXsdFolder = string.Empty;

    /// <summary>
    /// Copies every XSD into a single flat temp folder before each test.
    /// The schemas in the Engineering sub-folder reference their dependencies by
    /// bare file name (as if they were siblings), even though those files live in
    /// the parent XSD folder.
    /// </summary>
    [SetUp]
    public void CopyXsdsToTempFolder()
    {
        _tempXsdFolder = Path.Combine(Path.GetTempPath(), "VectoXsd_" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(_tempXsdFolder);
        foreach (var xsd in Directory.EnumerateFiles(TestUtils.XSDPath, "*.xsd", SearchOption.AllDirectories))
        {
            File.Copy(xsd, Path.Combine(_tempXsdFolder, Path.GetFileName(xsd)), overwrite: true);
        }
        foreach (var xsd in Directory.EnumerateFiles(TestUtils.XSDTrailerPath, "*.xsd", SearchOption.AllDirectories))
        {
            // File.Copy(xsd, Path.Combine(_tempXsdFolder, Path.GetFileName(xsd)), overwrite: true);
        }
    }

    [Test]
    public void GenerateDeclarationObjects()
    {
        var disambiguationMap = new Dictionary<string, string>
        {
            { "Trailer", "Trailer" },
        };
        var disambiguateTypeNames = new List<string>
        {
            "VectoInputDeclaration",
            "VehicleDeclarationType"
        };
        GenerateObjects(
            "VectoObjects",
            namespaceDisambiguationMap: disambiguationMap,
            disambiguateTypeNames: disambiguateTypeNames,
            "VectoDeclarationJob.xsd",
            "VectoDeclarationComponent.xsd");
    }

    [Test]
    public void GenerateEngineeringObjects()
    {
        var disambiguationMap = new Dictionary<string, string>
        {
            { "EngineeringDefinitions", "Engineering" },
            { "DeclarationDefinitions", "Declaration" },
        };
        var disambiguateTypeNames = new List<string>
        {
            "PredictiveCruiseControlType",
            "VehicleComponentsType",
        };
        GenerateObjects("VectoEngineeringObjects",
            disambiguationMap,
            disambiguateTypeNames,
            "VectoEngineeringJob.xsd",
            "VectoEngineeringComponent.xsd");
    }

    [Test]
    public void GenerateCustomerObjects()
    {
        GenerateObjects(
            "VectoCustomerObjects",
            namespaceDisambiguationMap: null,
            disambiguateTypeNames: null,
            "VectoOutputCustomer.xsd");
    }

    [Test]
    public void GenerateMonitoringObjects()
    {
        GenerateObjects(
            "VectoMonitoringObjects",
            namespaceDisambiguationMap: null,
            disambiguateTypeNames: null,
            "VectoMonitoring.xsd");
    }

    [Test]
    public void GenerateVtpReportObjects()
    {
        // VectoVTPReport.xsd (legacy) is intentionally omitted
        GenerateObjects(
            "VectoVtpReportObjects",
            namespaceDisambiguationMap: null,
            disambiguateTypeNames: null,
            "VTPReport.xsd");
    }

    [Test]
    public void GenerateMultistepObjects()
    {
        GenerateObjects(
            "VectoMultistepObjects",
            namespaceDisambiguationMap: null,
            disambiguateTypeNames: null,
            "VectoOutputMultistep.xsd");
    }

    [Test]
    public void GeneratePrimaryBusObjects()
    {
        // VectoOutputPrimaryBus.xsd imports DEV schemas (2.3_DEV / 2.6_Buses) that are missing from the repository.
        GenerateObjects(
            "VectoPrimaryBusObjects",
            namespaceDisambiguationMap: null,
            disambiguateTypeNames: null,
            "Engineering/VectoOutputPrimaryBus.xsd");
    }

    private void GenerateObjects(string rootNamespace,
        IDictionary<string, string>? namespaceDisambiguationMap,
        ICollection<string>? disambiguateTypeNames,
        params string[] schemaRelativePaths)
    {
        var outputFolder = TestUtils.OutputPath;

        var generator = new Generator
        {
            // Namespace mapping: -n <rootNamespace>
            NamespaceProvider = new List<KeyValuePair<NamespaceKey, string>>
            {
                new KeyValuePair<NamespaceKey, string>(new NamespaceKey(), rootNamespace)
            }.ToNamespaceProvider(key =>
            {
                var xmlns = key.XmlSchemaNamespace;
                var name = CodeUtilities.GenerateNamespace(xmlns, "");
                return name;
            }),

            // -0 (nullable)
            GenerateNullables = true,

            // -namingScheme=Direct
            NamingScheme = NamingScheme.Direct,

            // --uniqueTypeNames
            UniqueTypeNamesAcrossNamespaces = true,

            // Per-test disambiguation: for each name in DisambiguateTypeNames the label mapped
            // from the namespace's second-to-last segment is inserted before the version suffix,
            // e.g. "PredictiveCruiseControlType_Engineering_v1_0". This separates types that share
            // a name AND a version segment across different schema families.
            NamespaceDisambiguationMap = namespaceDisambiguationMap ?? new Dictionary<string, string>(),
            DisambiguateTypeNames = disambiguateTypeNames ?? new List<string>(),

            // --collectionType=System.Collections.Generic.List`1
            CollectionType = typeof(List<>),

            // --interface- (disable interfaces)
            GenerateInterfaces = false,

            // Default settings that match the console application
            EnableDataBinding = false,
            EmitOrder = false,
            EntityFramework = false,
            AssemblyVisible = false,
            CollectionImplementationType = null,
            CodeTypeReferenceOptions = default,
            TextValuePropertyName = "Value",
            GenerateDebuggerStepThroughAttribute = true,
            DisableComments = false,
            GenerateDescriptionAttribute = true,
            PrivateMemberPrefix = "_",
            EnableUpaCheck = true,
            GenerateComplexTypesForCollections = true,
            UseShouldSerializePattern = false,
            SeparateClasses = false,
            CollectionSettersMode = CollectionSettersMode.Private,
            DoNotForceIsNullable = false,
            SeparateSubstitutes = false,
            CompactTypeNames = false,
            CreateGeneratedCodeAttributeVersion = true,
            NetCoreSpecificCode = false,
            EnableNullableReferenceAttributes = false,
            GenerateCommandLineArgumentsComment = false,
            UseArrayItemAttribute = true,
            EnumAsString = false,
            MergeRestrictionsWithBase = true,
            MapUnionToWidestCommonType = false,
            SeparateNamespaceHierarchy = false,
            SerializeEmptyCollections = false,
            AllowDtdParse = false,
            OmitXmlIncludeAttribute = false,
            ForceUriScheme = "none"
        };

        // Set comment languages
        generator.CommentLanguages.UnionWith(["en"]);

        // Resolve each requested schema from the flattened temp folder (see SetUp).
        var schemaFiles = schemaRelativePaths
            .Select(path => Path.Combine(_tempXsdFolder, Path.GetFileName(path)))
            .ToList();

        // Collect schema compile/validation messages, which the generator otherwise
        // only reports through this callback (it does not throw on a broken schema set).
        var log = new List<string>();
        generator.Log = msg => log.Add(msg);

        // Generate the classes
        Directory.SetCurrentDirectory(outputFolder);
        generator.Generate(schemaFiles);

        // A broken schema set yields zero namespaces and silently writes no file, so
        // assert the expected output exists and surface the validation log if it doesn't.
        var expectedFile = Path.Combine(outputFolder, rootNamespace + ".cs");
        Assert.That(File.Exists(expectedFile), Is.True,
            $"No classes were generated for '{rootNamespace}'. Schema validation messages:{Environment.NewLine}"
            + string.Join(Environment.NewLine, log));

        // Verify the generated C# actually compiles (Roslyn), and report any errors.
        var compileErrors = GetCompileErrors(expectedFile);
        if (compileErrors.Count > 0)
        {
            File.WriteAllLines(Path.Combine(outputFolder, $"_compileerrors_{rootNamespace}.txt"), compileErrors);
        }
        Assert.That(compileErrors, Is.Empty,
            $"Generated file '{rootNamespace}.cs' has {compileErrors.Count} C# compile error(s):{Environment.NewLine}"
            + string.Join(Environment.NewLine, compileErrors.Take(50)));
    }

    /// <summary>
    /// Compiles the generated C# file in-memory with Roslyn and returns the compile
    /// errors (empty when it compiles cleanly). Each generated file is self-contained
    /// (all types in a single namespace), so it can be compiled on its own against the
    /// running runtime's reference assemblies.
    /// </summary>
    private static List<string> GetCompileErrors(string csFilePath)
    {
        var tree = CSharpSyntaxTree.ParseText(
            File.ReadAllText(csFilePath),
            new CSharpParseOptions(LanguageVersion.Latest),
            path: csFilePath);

        // Reference the assemblies of the currently running runtime.
        var trustedAssemblies = (string)AppContext.GetData("TRUSTED_PLATFORM_ASSEMBLIES")!;
        var references = trustedAssemblies
            .Split(Path.PathSeparator, StringSplitOptions.RemoveEmptyEntries)
            .Where(p => p.EndsWith(".dll", StringComparison.OrdinalIgnoreCase))
            .Select(p => (MetadataReference)MetadataReference.CreateFromFile(p))
            .ToList();

        var compilation = CSharpCompilation.Create(
            assemblyName: "GeneratedCheck_" + Path.GetFileNameWithoutExtension(csFilePath),
            syntaxTrees: [tree],
            references: references,
            options: new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary)
                .WithNullableContextOptions(NullableContextOptions.Enable));

        using var ms = new MemoryStream();
        var result = compilation.Emit(ms);

        return result.Diagnostics
            .Where(d => d.Severity == DiagnosticSeverity.Error)
            .Select(d => d.ToString())
            .ToList();
    }
}

/*
[System.CodeDom.Compiler.GeneratedCodeAttribute("XmlSchemaClassGenerator", "1.0.0.0")]
[System.SerializableAttribute()]
[System.Xml.Serialization.XmlTypeAttribute("PredictiveCruiseControlType", Namespace="urn:tugraz:ivt:VectoAPI:EngineeringDefinitions:v1.0")]
[System.Xml.Serialization.XmlRootAttribute("PredictiveCruiseControlType", Namespace="urn:tugraz:ivt:VectoAPI:EngineeringDefinitions:v1.0")]
public enum PredictiveCruiseControlType_v1_0
...

[System.CodeDom.Compiler.GeneratedCodeAttribute("XmlSchemaClassGenerator", "1.0.0.0")]
[System.SerializableAttribute()]
[System.Xml.Serialization.XmlTypeAttribute("PredictiveCruiseControlType", Namespace="urn:tugraz:ivt:VectoAPI:DeclarationDefinitions:v1.0")]
[System.Xml.Serialization.XmlRootAttribute("PredictiveCruiseControlType", Namespace="urn:tugraz:ivt:VectoAPI:DeclarationDefinitions:v1.0")]
public enum PredictiveCruiseControlType_v1_0
{
*/
