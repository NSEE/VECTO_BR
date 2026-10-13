using System;
using Spectre.Console.Cli;

namespace VectoInputManager.Commands;

/// <summary>
/// Provides type resolution services for the Spectre.Console.Cli framework by wrapping the .NET
/// Core dependency injection service provider. This class acts as an adapter that enables
/// Spectre.Console command classes to receive dependencies through constructor injection by
/// delegating resolution requests to the underlying IServiceProvider instance.
/// </summary>
public sealed class TypeResolver(IServiceProvider provider) : ITypeResolver
{
    private readonly IServiceProvider _provider = provider 
        ?? throw new ArgumentNullException(nameof(provider));

    public object? Resolve(Type? type)
    {
        return _provider.GetService(type!);
    }
}
