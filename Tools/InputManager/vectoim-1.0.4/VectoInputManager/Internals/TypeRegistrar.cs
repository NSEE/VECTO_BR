using System;
using Microsoft.Extensions.DependencyInjection;
using Spectre.Console.Cli;

namespace VectoInputManager.Commands;

/// <summary>
/// Provides a bridge between Microsoft.Extensions.DependencyInjection and Spectre.Console.Cli
/// frameworks. This class implements the ITypeRegistrar interface to enable dependency injection
/// within the Spectre.Console command-line application infrastructure. It wraps the .NET Core
/// service collection to manage service registrations and builds a service provider that can
/// resolve dependencies for command classes.
/// </summary>
public sealed class TypeRegistrar : ITypeRegistrar
{
    private readonly IServiceCollection _builder;

    public TypeRegistrar(IServiceCollection builder)
    {
        _builder = builder;
    }

    public ITypeResolver Build()
    {
        return new TypeResolver(_builder.BuildServiceProvider());
    }

    public void Register(Type service, Type implementation)
    {
        _builder.AddSingleton(service, implementation);
    }

    public void RegisterInstance(Type service, object implementation)
    {
        _builder.AddSingleton(service, implementation);
    }

    public void RegisterLazy(Type service, Func<object> func)
    {
        ArgumentNullException.ThrowIfNull(func);
        _builder.AddSingleton(service, _ => func());
    }
}