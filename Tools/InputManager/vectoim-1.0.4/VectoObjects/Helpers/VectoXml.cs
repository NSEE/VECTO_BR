using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Xml;
using System.Xml.Serialization;

namespace VectoObjects.Helpers
{
    /// <summary>
    /// This class provides methods to serialize and deserialize Vecto vehicle job declarations to
    /// and from XML format, applying necessary schema normalization such as decimal scaling and UTC
    /// timestamp formatting.
    /// </summary>
    public static class VectoXml
    {
        /// <summary>
        /// Serializes a <see cref="IVehicleJobDeclaration"/> to the specified output stream as XML,
        /// applying schema normalization (e.g., decimal scale, UTC timestamps) before serialization.
        /// </summary>
        public static void SerializeJob(Stream output, IVehicleJobDeclaration vectoInputDeclaration)
        {
            vectoInputDeclaration.schemaVersion = ForceScale(vectoInputDeclaration.SchemaVersionInternal, 1);

            var inputDeclarationType = vectoInputDeclaration.GetType();
            var jobSerializer = new XmlSerializer(inputDeclarationType);

            using (var stream = new MemoryStream())
            {
                jobSerializer.SerializeFixed(stream, vectoInputDeclaration);
                stream.Seek(0, SeekOrigin.Begin);
                stream.CopyTo(output);
            }
        }

        /// <summary>
        /// Creates the vehicle job declaration.
        /// </summary>
        /// <param name="vehicle">The vehicle.</param>
        /// <returns></returns>
        /// <exception cref="System.InvalidOperationException">Unsupported job type: {vehicleType.FullName}</exception>
        public static IVehicleJobDeclaration CreateVehicleJobDeclaration(IVehicleDeclaration vehicle)
        {
            Type vehicleType = vehicle.GetType();
            Type jobType;

            // DeclarationInput:v1.0
            if (vehicleType == typeof(VehicleDeclarationType_v1_0))
            {
                jobType = typeof(VectoDeclarationJobType_v1_0);
            }

            // DeclarationInput:v2.0
            else if (vehicleType == typeof(VehicleDeclarationType_v2_0)
                    || vehicleType == typeof(VehicleDeclarationType_v2_1)
                    || vehicleType == typeof(ExemptedVehicleDeclarationType_v2_1)
                    || vehicleType == typeof(ExemptedVehicleDeclarationType_v2_2_1)
                    || vehicleType.Name.EndsWith("v2_4"))
            {
                jobType = typeof(VectoDeclarationJobType_v2_0);
            }

            // DeclarationInput:v3.0
            else if (vehicleType.Name.EndsWith("v2_7"))
            {
                jobType = typeof(VectoDeclarationJobType_v2_7);
            }

            // Unrecognized type
            else
            {
                throw new InvalidOperationException(
                    $"Unsupported job type: {vehicleType.FullName}");
            }

            var inputDeclaration = (IVehicleJobDeclaration)Activator.CreateInstance(jobType);

            inputDeclaration.Vehicle = vehicle;

            return inputDeclaration;
        }

        /// <summary>
        /// Serializes a <see cref="IVehicleJobDeclaration"/> to the specified output file,
        /// applying schema normalization (e.g., decimal scale, UTC timestamps) before serialization.
        /// </summary>
        public static void SerializeJob(string filePath, IVehicleJobDeclaration vectoInputDeclaration)
        {
            using (var fileStream = new FileStream(filePath, FileMode.Create, FileAccess.Write))
            {
                SerializeJob(fileStream, vectoInputDeclaration);
            }
        }

        /// <summary>
        /// Creates an <see cref="XmlSerializer"/> for the root element of the specified XML stream. This
        /// method inspects the root element of the specified XML stream to determine its type and
        /// namespace. If the root element and namespace are recognized, it returns a serializer
        /// configured for that type. Otherwise, it returns an error indicating the unknown root element
        /// or namespace.
        /// </summary>
        public static bool TryGetSerializer(Stream xmlStream,
            out XmlSerializer serializer, out Type rootType, out string error)
        {
            using (var reader = XmlReader.Create(xmlStream))
            {
                var result = TryGetSerializerFromReader(reader, out serializer, out rootType, out error);
                xmlStream.Seek(0, SeekOrigin.Begin);
                return result;
            }
        }

        /// <summary>
        /// Creates an <see cref="XmlSerializer"/> for the root element of the specified XML file. This
        /// method inspects the root element of the specified XML file to determine its type and
        /// namespace. If the root element and namespace are recognized, it returns a serializer
        /// configured for that type. Otherwise, it returns an error indicating the unknown root element
        /// or namespace.
        /// </summary>
        public static bool TryGetSerializer(string xmlFilePath,
            out XmlSerializer serializer, out Type rootType, out string error)
        {
            using (var reader = XmlReader.Create(xmlFilePath))
                return TryGetSerializerFromReader(reader, out serializer, out rootType, out error);
        }

        private static bool TryGetSerializerFromReader(XmlReader reader,
            out XmlSerializer serializer, out Type rootType, out string error)
        {
            reader.MoveToContent();
            var rootName = reader.LocalName;
            var rootNs = reader.NamespaceURI;

            if (KnownTypes.TryGetValue(rootNs, out var rt))
            {
                serializer = new XmlSerializer(rt);
                rootType = rt;
                error = null;
                return true;
            }

            error = $"Unknown root element '{rootName}' or namespace '{rootNs}'.";
            serializer = null;
            rootType = null;
            return false;
        }

        private static readonly Dictionary<string, Type> KnownTypes = new Dictionary<string, Type>()
        {
            { "urn:tugraz:ivt:VectoAPI:DeclarationInput:v1.0", typeof(VectoDeclarationJobType_v1_0) },
            { "urn:tugraz:ivt:VectoAPI:DeclarationInput:v2.0", typeof(VectoDeclarationJobType_v2_0) },
            { "urn:tugraz:ivt:VectoAPI:DeclarationInput:v3.0", typeof(VectoDeclarationJobType_v2_7) },

            { "urn:tugraz:ivt:VectoAPI:DeclarationComponent:v1.0", typeof(VectoInputDeclaration_v1_0) },
            { "urn:tugraz:ivt:VectoAPI:DeclarationComponent:v2.0", typeof(VectoInputDeclaration_v2_0) },
            { "urn:tugraz:ivt:VectoAPI:DeclarationComponent:v2.1", typeof(VectoInputDeclaration_v2_1) },
            { "urn:tugraz:ivt:VectoAPI:DeclarationComponent:v2.2", typeof(VectoInputDeclaration_v2_2) },
        };

        /// <summary>
        /// Recursively normalizes an object graph before XML serialization to produce
        /// schema-friendly output.
        /// </summary>
        public static void SerializeFixed(this XmlSerializer serializer, Stream stream, object obj)
        {
            ProcessObject(obj);
            serializer.Serialize(stream, obj);

            // Internal recursive method

            void ProcessObject(object current)
            {
                if (current == null) return;
                var type = current.GetType();

                //// If it's an IEnumerable, walk its items
                if (current is IEnumerable en && !(current is string))
                {
                    foreach (var item in en)
                    {
                        if (item == null) continue;
                        var itemType = item.GetType();
                        if (itemType.IsClass && !(item is string))
                            ProcessObject(item);
                    }
                }
                else if (type.IsClass && type != typeof(string))
                {
                    // Walk properties
                    foreach (var prop in type.GetProperties(BindingFlags.Instance | BindingFlags.Public))
                    {
                        if (!prop.CanRead) continue;

                        var propType = prop.PropertyType;
                        object value = prop.GetValue(current);
                        if (value == null) continue;

                        // Handle datetime / nullable datetime
                        if (propType == typeof(DateTime) || propType == typeof(DateTime?))
                        {
                            if (!prop.CanWrite) continue;

                            // Don't want any milliseconds or local times.
                            var dt = (DateTime)value;
                            var newDt = new DateTime(
                                dt.Year, dt.Month, dt.Day,
                                dt.Hour, dt.Minute, dt.Second, DateTimeKind.Utc);
                            prop.SetValue(current, newDt);

                            continue;
                        }

                        // Handle decimal / nullable decimal
                        else if (propType == typeof(decimal) || propType == typeof(decimal?))
                        {
                            if (!prop.CanWrite) continue;

                            // Force scale if specified
                            var scale = GetScaleFromProperty(prop);
                            if (scale is null) continue;

                            var newVal = ForceScale((decimal)value, (byte)scale.Value);
                            prop.SetValue(current, newVal);

                            continue;
                        }

                        ProcessObject(value);
                    }
                }
            }
        }

        private static decimal ForceScale(decimal value, byte scale)
        {
            decimal rounded = Math.Round(value, scale, MidpointRounding.AwayFromZero);

            // Ensure internal representation actually has the requested scale (even if trailing zeros).
            // Yes, 0.10m and 0.1m are equal, but C# decimal saves the scale information that is then
            // used during serialization.

            string valueScaled = rounded.ToString($"F{scale}", CultureInfo.InvariantCulture);

            // Could be optimized by avoiding string conversion.
            return decimal.Parse(valueScaled, CultureInfo.InvariantCulture);
        }

        private static int? GetScaleFromProperty(PropertyInfo prop)
        {
            var ra = prop.GetCustomAttribute<RegularExpressionAttribute>();
            if (ra != null)
            {
                // Look for a fragment like \.\[0-9\]{d} - we are interested in the d
                // which indicates the required number of decimal places.
                var m = Regex.Match(ra.Pattern, @"\\\.\[0\-9\]\{(?<d>\d+)\}");
                if (m.Success && int.TryParse(m.Groups["d"].Value, out var d))
                    return d;
            }
            return null;
        }
    }
}