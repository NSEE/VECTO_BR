using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NLog.Extensions.Logging;
using VectoInputManager.Core;

namespace VectoInputManager.Commands;

/// <summary>
/// Extension methods for configuring services and logging used by the CLI application. Registers
/// component calculators, builders, and logging providers.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Registers the required services for the CLI application, including component standard value
    /// calculators, builders, and NLog logging.
    /// </summary>
    /// <param name="services">The service collection to configure.</param>
    /// <returns>The same service collection, for chaining.</returns>
    public static IServiceCollection ConfigureServices(this IServiceCollection services)
    {
        // Register all calculator types used for standard values by component type
        foreach (var type in Components.ComponentInformation.Select(c => c.Calc))
            services.AddTransient(type);

        services.AddSingleton<IStandardValuesFactory, StandardValuesFactory>();
        services.AddTransient<VehicleBuilder>();
        services.AddTransient<StandardValuesBuilder>();
        services.AddTransient<ComponentBuilder>();
        services.AddTransient<CorpusProcessor>();
        services.AddTransient<ComponentProcessor>();

        // Configure logging with NLog
        services.AddLogging(builder =>
        {
            builder.ClearProviders();
            builder.SetMinimumLevel(LogLevel.Debug);
            builder.AddNLog();
        });

        return services;
    }
}

/// <summary>
/// Default implementation of <see cref="IStandardValuesFactory"/> that resolves the appropriate
/// calculator from the dependency injection container.
/// </summary>
public class StandardValuesFactory(IServiceProvider serviceProvider) : IStandardValuesFactory
{
    /// <summary>
    /// Creates a standard values calculator for the specified component type.
    /// </summary>
    /// <param name="componentType">The component type to create the calculator for.</param>
    /// <returns>The resolved <see cref="BaseStandardValues"/> implementation.</returns>
    /// <exception cref="InvalidOperationException">
    /// Thrown when no calculator is registered for the specified component type.
    /// </exception>
    public BaseStandardValues Create(ComponentType componentType)
    {
        if (Components.ComponentInformation.TryGetValue(componentType, out var kv))
            return (BaseStandardValues)serviceProvider.GetRequiredService(kv.Calc);

        throw new InvalidOperationException(
            $"No standard values class registered for {componentType}.");
    }
}
