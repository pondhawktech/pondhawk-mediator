// Copyright (c) Pond Hawk Technologies Inc. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Reflection;
using CommunityToolkit.Diagnostics;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace Pondhawk.Mediator;

/// <summary>
/// Extension methods for registering mediator services with the DI container.
/// </summary>
public static partial class ServiceCollectionExtensions
{
    /// <summary>
    /// Registers the mediator and all handlers from the specified assemblies.
    /// </summary>
    /// <param name="services">The service collection to register with.</param>
    /// <param name="assemblies">The assemblies to scan for handlers.</param>
    /// <returns>The same service collection, for chaining.</returns>
    public static IServiceCollection AddMediator(
        this IServiceCollection services,
        params Assembly[] assemblies)
        => services.AddMediator(loggerFactory: null, assemblies);

    /// <summary>
    /// Registers the mediator and all handlers from the specified assemblies, logging any types that
    /// fail to load during discovery.
    /// </summary>
    /// <param name="services">The service collection to register with.</param>
    /// <param name="loggerFactory">
    /// The factory used to report unloadable types encountered while scanning. When
    /// <see langword="null"/>, falls back to <see cref="NullLoggerFactory"/> and such warnings are
    /// discarded — pass a bootstrap factory to surface them.
    /// </param>
    /// <param name="assemblies">The assemblies to scan for handlers.</param>
    /// <returns>The same service collection, for chaining.</returns>
    public static IServiceCollection AddMediator(
        this IServiceCollection services,
        ILoggerFactory? loggerFactory,
        params Assembly[] assemblies)
    {
        Guard.IsNotNull(services);
        Guard.IsNotNull(assemblies);
        Guard.HasSizeGreaterThan(assemblies, 0);

        var logger = (loggerFactory ?? NullLoggerFactory.Instance)
            .CreateLogger("Pondhawk.Mediator.HandlerDiscovery");

        // Register mediator
        services.AddScoped<IMediator, Mediator>();

        // Register all handlers from assemblies
        foreach (var assembly in assemblies)
        {
            var handlerTypes = GetLoadableTypes(assembly, logger)
                .Where(t => !t.IsAbstract && !t.IsInterface)
                .Where(t => t.GetInterfaces().Any(i =>
                    i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IRequestHandler<,>)));

            foreach (var handlerType in handlerTypes)
            {
                // A single class may handle more than one request type: register every
                // IRequestHandler<,> it implements, not just the first-discovered one.
                var handlerInterfaces = handlerType.GetInterfaces()
                    .Where(i => i.IsGenericType &&
                           i.GetGenericTypeDefinition() == typeof(IRequestHandler<,>));

                foreach (var handlerInterface in handlerInterfaces)
                {
                    services.AddScoped(handlerInterface, handlerType);
                }
            }
        }

        return services;
    }

    /// <summary>
    /// Returns the types in <paramref name="assembly"/>, salvaging the loadable ones when a
    /// <see cref="ReflectionTypeLoadException"/> is thrown: a single unloadable type (a missing
    /// optional dependency, a version mismatch) must not abort discovery of every other handler.
    /// The dropped types are reported as a single warning.
    /// </summary>
    private static Type[] GetLoadableTypes(Assembly assembly, ILogger logger)
    {
        try
        {
            return assembly.GetTypes();
        }
        catch (ReflectionTypeLoadException ex)
        {
            var loaded = ex.Types.OfType<Type>().ToArray();
            var skipped = ex.Types.Length - loaded.Length;
            LogSkippedTypes(logger, skipped, assembly.GetName().Name ?? "unknown", ex.Message);
            return loaded;
        }
    }

    [LoggerMessage(
        Level = LogLevel.Warning,
        Message = "Handler discovery skipped {SkippedCount} unloadable type(s) in assembly {Assembly}: {LoaderMessage}")]
    private static partial void LogSkippedTypes(ILogger logger, int skippedCount, string assembly, string loaderMessage);

    /// <summary>
    /// Registers an open generic pipeline behavior for all request types.
    /// Order matters: first registered = outermost (executes first).
    /// </summary>
    public static IServiceCollection AddPipelineBehavior(
        this IServiceCollection services,
        Type behaviorType)
    {
        Guard.IsNotNull(services);
        Guard.IsNotNull(behaviorType);

        services.AddScoped(typeof(IPipelineBehavior<,>), behaviorType);
        return services;
    }
}
