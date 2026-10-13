using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Schema;
using Newtonsoft.Json.Schema.Generation;
using VectoInputManager.Core;

namespace VectoInputManager.Tests.CodeGeneration;

/// <summary>
/// Utility class for generating JSON schemas from .NET types with support for polymorphic types and
/// custom generation providers.
/// </summary>
public class JsonSchemaGenerator
{
    public static string GenerateSchema(Type type)
    {
        TestUtils.InitGeneration();
        JSchemaGenerator generator = new()
        {
            SchemaReferenceHandling = SchemaReferenceHandling.None,
            SchemaIdGenerationHandling = SchemaIdGenerationHandling.TypeName,
            SchemaLocationHandling = SchemaLocationHandling.Inline
        };

        generator.GenerationProviders.Add(new StringEnumGenerationProvider());
        generator.GenerationProviders.Add(new PolymorphicTypeGenerationProvider());
        generator.GenerationProviders.Add(new ArrayTypeGenerationProvider());

        JSchema schema = generator.Generate(type);

        RemovePropertiesByName(schema, "Signature");
        RemovePropertiesByName(schema, "MonitoringData");
        RemoveConditionalValueProperties(schema);
        RemoveSpecifiedProperties(schema);

        return schema.ToString();
    }

    /// <summary>
    /// Custom generation provider for array types that creates schemas with custom $type and
    /// $values properties.
    /// </summary>
    public class ArrayTypeGenerationProvider : JSchemaGenerationProvider
    {
        public override bool CanGenerateSchema(JSchemaTypeGenerationContext context)
        {
            var type = context.ObjectType;
            // Check if this is an array or generic list/collection
            return type.IsArray ||
                   (type.IsGenericType &&
                    (type.GetGenericTypeDefinition() == typeof(List<>) ||
                     type.GetGenericTypeDefinition() == typeof(IList<>) ||
                     type.GetGenericTypeDefinition() == typeof(ICollection<>) ||
                     type.GetGenericTypeDefinition() == typeof(IEnumerable<>)));
        }

        public override JSchema GetSchema(JSchemaTypeGenerationContext context)
        {
            var type = context.ObjectType;

            // Get the element type
            Type elementType;
            if (type.IsArray)
            {
                elementType = type.GetElementType()!;
            }
            else
            {
                elementType = type.GetGenericArguments()[0];
            }

            // Create the schema for the array wrapper object
            var schema = new JSchema
            {
                Type = JSchemaType.Object
            };

            // Add $type property with array type notation
            schema.Properties["$type"] = new JSchema
            {
                Type = JSchemaType.String,
                Const = $"[{elementType.Name}]"
            };
            schema.Required.Add("$type");

            // Add $values property as an array of element type
            var valuesSchema = new JSchema
            {
                Type = JSchemaType.Array
            };

            // Generate schema for the element type
            var elementSchema = context.Generator.Generate(elementType);
            valuesSchema.Items.Add(elementSchema);

            schema.Properties["$values"] = valuesSchema;
            schema.Required.Add("$values");

            return schema;
        }
    }

    /// <summary>
    /// Custom generation provider for polymorphic types that creates schemas with oneOf for all
    /// concrete descendants.
    /// </summary>
    public class PolymorphicTypeGenerationProvider : JSchemaGenerationProvider
    {
        public override bool CanGenerateSchema(JSchemaTypeGenerationContext context)
        {
            // Check if this is an abstract type or has descendants that might need polymorphic handling
            var type = context.ObjectType;
            return type.IsAbstract || HasConcreteDescendants(type);
        }

        public override JSchema GetSchema(JSchemaTypeGenerationContext context)
        {
            var type = context.ObjectType;
            var descendants = TypeUtils.GetSelfAndDescendantTypes(type)
                .OrderBy(t => t.Name)   
                .ToList();

            var schema = new JSchema
            {
                Type = JSchemaType.Object
            };

            // Add oneOf with all possible concrete descendants
            foreach (var descendant in descendants)
            {
                var descendantSchema = context.Generator.Generate(descendant);

                // Add our custom discriminator property (the type name without namespace)
                AddDiscriminatorProperty(descendantSchema, descendant);

                schema.OneOf.Add(descendantSchema);
            }

            return schema;
        }

        private static void AddDiscriminatorProperty(JSchema schema, Type type)
        {
            schema.Properties["$type"] = new JSchema
            {
                Type = JSchemaType.String,
                Enum = { type.Name }
            };

            if (!schema.Required.Contains("$type"))
                schema.Required.Add("$type");
        }

        private static bool HasConcreteDescendants(Type type)
        {
            return TypeUtils.GetSelfAndDescendantTypes(type).Count() > 1;
        }
    }

    /// <summary>
    /// Recursively removes properties ending with "Specified" when a corresponding base property 
    /// exists (e.g., removes "DefaultValueSpecified" when "DefaultValue" exists). 
    /// Also removes the base property from the required list.
    /// </summary>
    private static void RemoveSpecifiedProperties(JSchema schema)
    {
        // Find all properties that end with "Specified" and have a corresponding base property
        var specifiedProperties = schema.Properties.Keys
            .Where(key => key.EndsWith("Specified") &&
                         !key.EndsWith("ValueSpecified")) // Skip ValueSpecified (handled by RemoveConditionalValueProperties)
            .ToList();

        foreach (var specifiedProperty in specifiedProperties)
        {
            // Get the base property name by removing "Specified" suffix
            var baseProperty = specifiedProperty[..^9]; // "Specified" is 9 characters

            // Only remove if the base property exists
            if (schema.Properties.ContainsKey(baseProperty))
            {
                // Remove the Specified property
                schema.Properties.Remove(specifiedProperty);
                schema.Required.Remove(specifiedProperty);

                // Also remove the base property from the required list (but keep the property itself)
                schema.Required.Remove(baseProperty);
            }
        }

        // Recursively process nested schemas
        foreach (var prop in schema.Properties.Values)
        {
            RemoveSpecifiedProperties(prop);
        }

        if (schema.OneOf != null)
        {
            foreach (var oneOfSchema in schema.OneOf)
            {
                RemoveSpecifiedProperties(oneOfSchema);
            }
        }

        if (schema.AnyOf != null)
        {
            foreach (var anyOfSchema in schema.AnyOf)
            {
                RemoveSpecifiedProperties(anyOfSchema);
            }
        }

        if (schema.AllOf != null)
        {
            foreach (var allOfSchema in schema.AllOf)
            {
                RemoveSpecifiedProperties(allOfSchema);
            }
        }

        if (schema.Not != null)
        {
            RemoveSpecifiedProperties(schema.Not);
        }

        // Process items schema (for arrays)
        if (schema.Items != null)
        {
            foreach (var itemSchema in schema.Items)
            {
                RemoveSpecifiedProperties(itemSchema);
            }
        }

        // Process additional properties schema
        if (schema.AdditionalProperties != null)
        {
            RemoveSpecifiedProperties(schema.AdditionalProperties);
        }

        // Process definitions in extension data
        if (schema.ExtensionData?.ContainsKey("definitions") == true)
        {
            var definitions = schema.ExtensionData["definitions"] as JObject;

            foreach (var def in definitions!.Properties().ToList())
            {
                if (def.Value is JObject defObj)
                {
                    var defSchema = defObj.ToObject<JSchema>();
                    if (defSchema != null)
                    {
                        RemoveSpecifiedProperties(defSchema);
                        definitions[def.Name] = JToken.FromObject(defSchema);
                    }
                }
            }
        }
    }

    /// <summary>
    /// Recursively removes properties ending with "Value" and "ValueSpecified" when a corresponding
    /// base property exists (e.g., removes "DeltaCdxA_CFDValue" and "DeltaCdxA_CFDValueSpecified" 
    /// when "DeltaCdxA_CFD" exists). Also removes the base property from the required list.
    /// </summary>
    private static void RemoveConditionalValueProperties(JSchema schema)
    {
        // Find all properties that could be base properties for Value/ValueSpecified patterns
        var baseProperties = schema.Properties.Keys
            .Where(key => schema.Properties.ContainsKey(key + "Value") ||
                         schema.Properties.ContainsKey(key + "ValueSpecified"))
            .ToList();

        foreach (var baseProperty in baseProperties)
        {
            var valueProperty = baseProperty + "Value";
            var valueSpecifiedProperty = baseProperty + "ValueSpecified";

            // Remove the Value and ValueSpecified properties if they exist
            if (schema.Properties.ContainsKey(valueProperty))
            {
                schema.Properties.Remove(valueProperty);
                schema.Required.Remove(valueProperty);
            }

            if (schema.Properties.ContainsKey(valueSpecifiedProperty))
            {
                schema.Properties.Remove(valueSpecifiedProperty);
                schema.Required.Remove(valueSpecifiedProperty);
            }

            // Also remove the base property from the required list (but keep the property itself)
            schema.Required.Remove(baseProperty);
        }

        // Recursively process nested schemas
        foreach (var prop in schema.Properties.Values)
        {
            RemoveConditionalValueProperties(prop);
        }

        if (schema.OneOf != null)
        {
            foreach (var oneOfSchema in schema.OneOf)
            {
                RemoveConditionalValueProperties(oneOfSchema);
            }
        }

        if (schema.AnyOf != null)
        {
            foreach (var anyOfSchema in schema.AnyOf)
            {
                RemoveConditionalValueProperties(anyOfSchema);
            }
        }

        if (schema.AllOf != null)
        {
            foreach (var allOfSchema in schema.AllOf)
            {
                RemoveConditionalValueProperties(allOfSchema);
            }
        }

        if (schema.Not != null)
        {
            RemoveConditionalValueProperties(schema.Not);
        }

        // Process items schema (for arrays)
        if (schema.Items != null)
        {
            foreach (var itemSchema in schema.Items)
            {
                RemoveConditionalValueProperties(itemSchema);
            }
        }

        // Process additional properties schema
        if (schema.AdditionalProperties != null)
        {
            RemoveConditionalValueProperties(schema.AdditionalProperties);
        }

        // Process definitions in extension data
        if (schema.ExtensionData?.ContainsKey("definitions") == true)
        {
            var definitions = schema.ExtensionData["definitions"] as JObject;

            foreach (var def in definitions!.Properties().ToList())
            {
                if (def.Value is JObject defObj)
                {
                    var defSchema = defObj.ToObject<JSchema>();
                    if (defSchema != null)
                    {
                        RemoveConditionalValueProperties(defSchema);
                        definitions[def.Name] = JToken.FromObject(defSchema);
                    }
                }
            }
        }
    }

    /// <summary>
    /// Recursively removes all properties with the specified name from the schema and its nested schemas.
    /// Also removes leftover object definitions that match the property name pattern.
    /// </summary>
    private static void RemovePropertiesByName(JSchema schema, string propertyName)
    {
        schema.Properties.Remove(propertyName);
        schema.Required.Remove(propertyName);

        foreach (var prop in schema.Properties.Values)
        {
            RemovePropertiesByName(prop, propertyName);
        }

        if (schema.OneOf != null)
        {
            foreach (var oneOfSchema in schema.OneOf)
            {
                RemovePropertiesByName(oneOfSchema, propertyName);
            }
        }

        if (schema.AnyOf != null)
        {
            foreach (var anyOfSchema in schema.AnyOf)
            {
                RemovePropertiesByName(anyOfSchema, propertyName);
            }
        }

        if (schema.AllOf != null)
        {
            foreach (var allOfSchema in schema.AllOf)
            {
                RemovePropertiesByName(allOfSchema, propertyName);
            }
        }

        if (schema.Not != null)
        {
            RemovePropertiesByName(schema.Not, propertyName);
        }

        // Process items schema (for arrays)
        if (schema.Items != null)
        {
            foreach (var itemSchema in schema.Items)
            {
                RemovePropertiesByName(itemSchema, propertyName);
            }
        }

        // Process additional properties schema
        if (schema.AdditionalProperties != null)
        {
            RemovePropertiesByName(schema.AdditionalProperties, propertyName);
        }

        // Process definitions in extension data
        if (schema.ExtensionData?.ContainsKey("definitions") == true)
        {
            var definitions = schema.ExtensionData["definitions"] as JObject;

            // Remove well-known type definitions
            var relatedTypeNames = new[]
            {
                    "DigestMethodType",
                    "ReferenceType",
                    "TransformType",
                    "SignatureType_v2_0",
                    "SignatureType_v1_0",
                };

            // First, recursively process each definition
            foreach (var def in definitions!.Properties().ToList())
            {
                if (def.Value is JObject defObj)
                {
                    var defSchema = defObj.ToObject<JSchema>();
                    if (defSchema != null)
                    {
                        RemovePropertiesByName(defSchema, propertyName);
                        definitions[def.Name] = JToken.FromObject(defSchema);
                    }
                }
            }

            // Then, remove definitions that match the property name pattern
            var definitionsToRemove = definitions.Properties()
                .Where(def => relatedTypeNames.Contains(def.Name))
                .Select(def => def.Name)
                .ToList();

            foreach (var defName in definitionsToRemove)
            {
                definitions.Remove(defName);
            }
        }
    }
}
