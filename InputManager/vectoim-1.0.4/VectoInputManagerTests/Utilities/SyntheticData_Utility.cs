using Fasterflect;
using System.Reflection;
using System.Xml.Serialization;
using VectoInputManager.Core;
using VectoObjects;
using VectoObjects.Helpers;

namespace VectoInputManager.Tests.Utilities;

public class SyntheticData_Utility
{
    [Test]
    [Explicit("Run manually to generate synthetic XML data.")]
    public void Execute_SyntheticData()
    {
        var inputDir = TestUtils.XMLReaderDeclarationTestFilesPath;
        var outputDir = TestUtils.CombinePath(TestUtils.OutputPath, "SyntheticData");
        using StreamWriter writer = new(Path.Combine(outputDir, "report.txt"));

        List<VectoDeclarationJobType_v2_7> corpus = [];
        List<object> corpusObjects = [];

        // Load all files.

        foreach (var xmlFile in Directory.EnumerateFiles(inputDir, "*.xml", SearchOption.AllDirectories))
        {
            if (File.ReadAllText(xmlFile).Contains(":VehicleInterimFile", StringComparison.OrdinalIgnoreCase))
                continue;

            using var fs = File.OpenRead(xmlFile);
            VectoXml.TryGetSerializer(fs, out var serializer, out var rootType, out var error);
            var jobObj = serializer.Deserialize(fs);

            if (jobObj is not VectoDeclarationJobType_v2_7 job)
                continue;

            writer.WriteLine($"Processing: {Path.GetFileName(xmlFile)} as {rootType}");

            corpus.Add(job);
            EnumerateObjects(job);

            //break;
        }

        // Enumerate all vehicle types
        foreach (var vehicleType in TypeUtils.GetAllVehicleTypes())
        {
            if (!vehicleType.Name.Contains("v2_7"))
                continue;

            // Create a synthetic job for each vehicle type
            var vehicleInstance = vehicleType.CreateInstance() as IVehicleDeclaration;
            var job = VectoXml.CreateVehicleJobDeclaration(vehicleInstance) as VectoDeclarationJobType_v2_7;
            CorpusData(job!);

            // Serialize the synthetic job to XML
            var serializer = new XmlSerializer(typeof(VectoDeclarationJobType_v2_7));
            var outputFile = Path.Combine(outputDir, $"{vehicleType.Name}_Synthetic.xml");
            using var stream = File.Create(outputFile);
            serializer.Serialize(stream, job);
        }

        // Internal metehods

        void CorpusData(object robj)
        {
            var type = robj.GetType();

            foreach (var prop in type.GetProperties())
            {
                if (!prop.CanWrite) continue;

                var propType = prop.PropertyType;

                if (typeof(System.Collections.IEnumerable).IsAssignableFrom(propType))
                    continue;

                if (propType.IsClass && propType != typeof(string))
                {
                    var obj = prop.GetValue(robj);

                    if (obj == null)
                    {
                        var descendands = TypeUtils.GetSelfAndDescendantTypes(propType);
                        obj = descendands.FirstOrDefault()?.CreateInstance();
                    }

                    if (obj != null)
                    {
                        var fobj = corpusObjects.FirstOrDefault(x => x.GetType() == obj.GetType());
                        if (fobj != null)
                            obj = fobj;
                    }

                    obj ??= CreateInstance(propType)!;

                    prop.SetValue(robj, obj);
                    CorpusData(obj);
                }
            }
        }

        object? CreateInstance(Type type)
        {
            ArgumentNullException.ThrowIfNull(type);

            if (type == typeof(string))
            {
                return "SampleString";
            }

            else if (type == typeof(byte))
            {
                return (byte)1;
            }

            else if (type == typeof(DateTime))
            {
                return new DateTime(2025, 1, 1);
            }

            else if (typeof(System.Collections.IEnumerable).IsAssignableFrom(type))
            {
                var elemType = type.GetElementType();
                if (elemType == null)
                    return null;

                var arr = Array.CreateInstance(elemType, 1);

                var elem = CreateInstance(elemType);
                arr.SetValue(elem, 0);
                return arr;
            }

            else if (type.IsClass)
            {
                var descendands = TypeUtils.GetSelfAndDescendantTypes(type);
                var obj = descendands.FirstOrDefault()?.CreateInstance();
                return obj;
            }

            return null;
        }


        void MockData(object robj)
        {
            var type = robj.GetType();

            foreach (var prop in type.GetProperties())
            {
                var propType = prop.PropertyType;
                var obj = prop.GetValue(robj);

                obj ??= CreateInstance(propType);

                propType = obj!.GetType();

                if (obj == null)
                    continue;

                if (prop.CanWrite)
                    prop.SetValue(robj, obj);

                if (typeof(System.Collections.IEnumerable).IsAssignableFrom(propType))
                {
                    var enumerable = obj as System.Collections.IEnumerable;
                }

                else if (propType.IsClass && propType != typeof(string))
                    MockData(obj);
            }
        }

        void EnumerateObjects(object obj)
        {
            var type = obj.GetType();

            // Ignore primitive, string, enum, or value types
            if (type.IsPrimitive || type.IsEnum || type == typeof(string) || type.IsValueType)
                return;

            //// Get all public instance properties
            var properties = type.GetProperties(BindingFlags.Public | BindingFlags.Instance);

            // Check if this object has any complex object properties
            bool hasComplex = false;
            foreach (var prop in properties)
            {
                var propType = prop.PropertyType;
                if (propType == typeof(string) || propType.IsPrimitive || propType.IsEnum || propType.IsValueType)
                    continue;
                if (typeof(System.Collections.IEnumerable).IsAssignableFrom(propType) && propType != typeof(string))
                {
                    var value = prop.GetValue(obj);
                    if (value != null)
                    {
                        foreach (var item in (System.Collections.IEnumerable)value)
                            EnumerateObjects(item);
                    }
                    continue;
                }
                // Found a complex property
                hasComplex = true;
                var valueObj = prop.GetValue(obj);
                if (valueObj != null)
                    EnumerateObjects(valueObj);
            }

            // If no complex properties, add to corpusObjects
            if (!hasComplex)
                corpusObjects.Add(obj);
        }
    }
}
