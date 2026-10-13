using Castle.Core.Internal;
using System.ComponentModel;
using System.Reflection;
using TUGraz.VectoCore.Utils;
using VectoInputManager.Core;
using VectoObjects;

namespace VectoInputManager.Tests.Utilities;

public static class RandomVehicleGeneratorExtensions
{
    private static readonly Random _rnd = new();

    public static void SetExampleValues(this IVehicleDeclaration vehicle)
    {
        var type = vehicle.GetType();
        int id = 1;
        var schema = XMLValidator.GetXMLSchema(XmlDocumentType.DeclarationJobData);

        FillRandom(vehicle);

        void FillRandom(object obj)
        {
            var objType = obj.GetType();
            foreach (var prop in objType.GetProperties(BindingFlags.Public | BindingFlags.Instance))
            {
                if (!prop.CanWrite
                    || prop.GetAttribute<EditorBrowsableAttribute>()?.State == EditorBrowsableState.Never
                    || prop.Name == "Reference")
                    continue;

                var propSpecified = objType.GetProperty($"{prop.Name}Specified");
                var optional = propSpecified != null;

                object? value = null;

                if (prop.Name == "id")
                {
                    value = "id-" + id++;
                }
                else if (prop.PropertyType == typeof(string))
                {
                    value = $"String_{_rnd.Next(1000, 9999)}";
                }

                else if (prop.PropertyType == typeof(int))
                {
                    value = _rnd.Next(1, 10000);
                }
                else if (prop.PropertyType == typeof(int?))
                {
                    value = _rnd.Next(1, 10000);
                }

                else if (prop.PropertyType == typeof(DateTime))
                {
                    value = DateTime.UtcNow.Date.AddDays(_rnd.Next(-1000, -100))
                        .AddHours(_rnd.Next(8, 17));
                }

                else if (prop.PropertyType == typeof(decimal))
                {
                    value = (decimal)(_rnd.NextDouble() * 10000);
                }
                else if (prop.PropertyType == typeof(decimal?))
                {
                    value = (decimal)(_rnd.NextDouble() * 10000);
                }

                else if (prop.PropertyType == typeof(double))
                {
                    value = (double)(_rnd.NextDouble() * 10000);
                }

                else if (prop.PropertyType == typeof(bool))
                {
                    value = true;
                }
                else if (prop.PropertyType == typeof(bool?))
                {
                    value = true;
                }

                else if (prop.PropertyType.IsEnum)
                {
                    var names = Enum.GetNames(prop.PropertyType);
                    value = Enum.Parse(prop.PropertyType, names[_rnd.Next(names.Length)]);
                }
                else if (Nullable.GetUnderlyingType(prop.PropertyType)?.IsEnum == true)
                {
                    var enumType = Nullable.GetUnderlyingType(prop.PropertyType)!;
                    var names = Enum.GetNames(enumType);
                    value = Enum.Parse(enumType, names[_rnd.Next(names.Length)]);
                }

                else if (prop.PropertyType.IsGenericType &&
                    prop.PropertyType.GetGenericTypeDefinition() == typeof(List<>))
                {
                    var itemType = prop.PropertyType.GetGenericArguments()[0];
                    var collection = prop.GetValue(obj)! ?? Activator.CreateInstance(prop.PropertyType)!;
                    prop.SetValue(obj, collection);

                    var addMethod = collection.GetType().GetMethod("Add");
                    int count = _rnd.Next(1, 4); // 1-3 items
                    for (int i = 0; i < count; i++)
                    {
                        var descendands = TypeUtils.GetSelfAndDescendantTypes(itemType).ToList();
                        Type chosenType = descendands.Count > 0
                            ? descendands[_rnd.Next(descendands.Count)]
                            : itemType;

                        // Supported: string and class items.
                        var instance = chosenType == typeof(string)
                            ? $"Item_{_rnd.Next(1000, 9999)}"
                            : Activator.CreateInstance(chosenType);

                        FillRandom(instance!);
                        addMethod!.Invoke(collection, [instance]);
                    }
                    continue;
                }

                else if (prop.PropertyType.IsClass && prop.PropertyType != typeof(string))
                {
                    var descendands = TypeUtils.GetSelfAndDescendantTypes(prop.PropertyType).ToList();
                    Type chosenType = descendands.Count > 0
                        ? descendands[_rnd.Next(descendands.Count)]
                        : prop.PropertyType;
                    var instance = Activator.CreateInstance(chosenType);
                    prop.SetValue(obj, instance);
                    FillRandom(instance!);
                    continue;
                }
                else
                {
                    throw new NotSupportedException(
                        $"Unsupported property type: {prop.PropertyType.FullName} for property {prop.Name}");
                }

                if (optional && propSpecified != null && propSpecified.CanWrite)
                    propSpecified!.SetValue(obj, true);

                prop.SetValue(obj, value);
            }
        }
    }
}
