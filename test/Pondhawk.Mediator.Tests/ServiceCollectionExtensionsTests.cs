// Copyright (c) Pond Hawk Technologies Inc. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Pondhawk.Mediator;
using Shouldly;
using Xunit;

namespace Pondhawk.Mediator.Tests;

public class ServiceCollectionExtensionsTests
{

    // ── Test doubles ──

    public record TestRequest(string Value) : IRequest<string>;

    public class TestHandler : IRequestHandler<TestRequest, string>
    {
        public Task<string> HandleAsync(TestRequest request, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(request.Value);
        }
    }

    public record AnotherRequest : IRequest<int>;

    public class AnotherHandler : IRequestHandler<AnotherRequest, int>
    {
        public Task<int> HandleAsync(AnotherRequest request, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(99);
        }
    }

    public record FirstRequest : IRequest<string>;

    public record SecondRequest : IRequest<int>;

    // A single class that handles more than one request type — every IRequestHandler<,> it
    // implements must be registered, not just the first one discovered by reflection.
    public class MultiHandler :
        IRequestHandler<FirstRequest, string>,
        IRequestHandler<SecondRequest, int>
    {
        public Task<string> HandleAsync(FirstRequest request, CancellationToken cancellationToken = default)
            => Task.FromResult("first");

        public Task<int> HandleAsync(SecondRequest request, CancellationToken cancellationToken = default)
            => Task.FromResult(2);
    }

    public class TestBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
        where TRequest : IRequest<TResponse>
    {
        public Task<TResponse> HandleAsync(
            TRequest request,
            RequestHandlerDelegate<TResponse> next,
            CancellationToken cancellationToken = default)
        {
            return next();
        }
    }

    // An assembly whose GetTypes() throws ReflectionTypeLoadException — the loadable types survive
    // (as a real type array with a null slot for the one that failed), mirroring a missing optional
    // dependency or a version mismatch at scan time.
    private sealed class FaultyAssembly(params Type[] loadable) : Assembly
    {
        public override AssemblyName GetName() => new("FaultyAssembly");

        public override Type[] GetTypes()
        {
            var types = new Type[loadable.Length + 1];
            Array.Copy(loadable, types, loadable.Length);
            types[loadable.Length] = null; // the type that failed to load

            var loaderErrors = new Exception[types.Length];
            loaderErrors[loadable.Length] = new TypeLoadException("simulated load failure");

            throw new ReflectionTypeLoadException(types, loaderErrors);
        }
    }

    // Minimal ILoggerFactory that records emitted entries so a warning can be asserted.
    private sealed class CapturingLoggerFactory : ILoggerFactory
    {
        public List<(LogLevel Level, string Message)> Entries { get; } = [];

        public void AddProvider(ILoggerProvider provider) { }

        public ILogger CreateLogger(string categoryName) => new CapturingLogger(Entries);

        public void Dispose() { }

        private sealed class CapturingLogger(List<(LogLevel, string)> entries) : ILogger
        {
            public IDisposable BeginScope<TState>(TState state) => NullScope.Instance;

            public bool IsEnabled(LogLevel logLevel) => true;

            public void Log<TState>(
                LogLevel logLevel,
                EventId eventId,
                TState state,
                Exception exception,
                Func<TState, Exception, string> formatter)
                => entries.Add((logLevel, formatter(state, exception)));

            private sealed class NullScope : IDisposable
            {
                public static readonly NullScope Instance = new();
                public void Dispose() { }
            }
        }
    }

    // ── AddMediator ──

    [Fact]
    public void AddMediator_RegistersMediator()
    {
        var services = new ServiceCollection();

        services.AddMediator(typeof(TestHandler).Assembly);

        var provider = services.BuildServiceProvider();
        var mediator = provider.GetRequiredService<IMediator>();
        mediator.ShouldBeOfType<Pondhawk.Mediator.Mediator>();
    }

    [Fact]
    public void AddMediator_DiscoversHandlersFromAssembly()
    {
        var services = new ServiceCollection();

        services.AddMediator(typeof(TestHandler).Assembly);

        var provider = services.BuildServiceProvider();
        var handler = provider.GetService<IRequestHandler<TestRequest, string>>();
        handler.ShouldNotBeNull();
        handler.ShouldBeOfType<TestHandler>();
    }

    [Fact]
    public void AddMediator_DiscoversMultipleHandlers()
    {
        var services = new ServiceCollection();

        services.AddMediator(typeof(TestHandler).Assembly);

        var provider = services.BuildServiceProvider();
        provider.GetService<IRequestHandler<TestRequest, string>>().ShouldNotBeNull();
        provider.GetService<IRequestHandler<AnotherRequest, int>>().ShouldNotBeNull();
    }

    [Fact]
    public async Task AddMediator_EndToEnd_HandlerResolvesAndExecutes()
    {
        var services = new ServiceCollection();
        services.AddMediator(typeof(TestHandler).Assembly);
        var provider = services.BuildServiceProvider();
        var mediator = provider.GetRequiredService<IMediator>();

        var result = await mediator.SendAsync(new TestRequest("hello"));

        result.Value.ShouldBe("hello");
    }

    [Fact]
    public void AddMediator_NullServices_Throws()
    {
        IServiceCollection services = null;

        Should.Throw<ArgumentNullException>(
            () => services.AddMediator(typeof(TestHandler).Assembly));
    }

    [Fact]
    public void AddMediator_NullAssemblies_Throws()
    {
        var services = new ServiceCollection();

        Should.Throw<ArgumentNullException>(
            () => services.AddMediator((Assembly[])null));
    }

    [Fact]
    public void AddMediator_EmptyAssemblies_Throws()
    {
        var services = new ServiceCollection();

        Should.Throw<ArgumentException>(
            () => services.AddMediator(Array.Empty<Assembly>()));
    }

    [Fact]
    public void AddMediator_ClassHandlingMultipleRequests_RegistersEveryHandlerInterface()
    {
        var services = new ServiceCollection();

        services.AddMediator(typeof(MultiHandler).Assembly);

        var provider = services.BuildServiceProvider();
        provider.GetService<IRequestHandler<FirstRequest, string>>().ShouldBeOfType<MultiHandler>();
        provider.GetService<IRequestHandler<SecondRequest, int>>().ShouldBeOfType<MultiHandler>();
    }

    [Fact]
    public async Task AddMediator_ClassHandlingMultipleRequests_BothRoutesDispatch()
    {
        var services = new ServiceCollection();
        services.AddMediator(typeof(MultiHandler).Assembly);
        var provider = services.BuildServiceProvider();
        var mediator = provider.GetRequiredService<IMediator>();

        (await mediator.SendAsync(new FirstRequest())).Value.ShouldBe("first");
        (await mediator.SendAsync(new SecondRequest())).Value.ShouldBe(2);
    }

    [Fact]
    public void AddMediator_UnloadableTypes_StillRegistersLoadableHandlers()
    {
        var services = new ServiceCollection();
        var faulty = new FaultyAssembly(typeof(TestHandler));

        // A single unloadable type must not abort discovery of the rest.
        services.AddMediator(new CapturingLoggerFactory(), faulty);

        var provider = services.BuildServiceProvider();
        provider.GetService<IRequestHandler<TestRequest, string>>().ShouldBeOfType<TestHandler>();
    }

    [Fact]
    public void AddMediator_UnloadableTypes_LogsWarning()
    {
        var services = new ServiceCollection();
        var loggerFactory = new CapturingLoggerFactory();
        var faulty = new FaultyAssembly(typeof(TestHandler));

        services.AddMediator(loggerFactory, faulty);

        loggerFactory.Entries.ShouldContain(e =>
            e.Level == LogLevel.Warning && e.Message.Contains("skipped", StringComparison.Ordinal));
    }

    // ── AddPipelineBehavior ──

    [Fact]
    public void AddPipelineBehavior_RegistersOpenGenericBehavior()
    {
        var services = new ServiceCollection();
        services.AddMediator(typeof(TestHandler).Assembly);

        services.AddPipelineBehavior(typeof(TestBehavior<,>));

        var provider = services.BuildServiceProvider();
        var behaviors = provider.GetServices<IPipelineBehavior<TestRequest, string>>();
        behaviors.ShouldNotBeEmpty();
    }

    [Fact]
    public void AddPipelineBehavior_NullServices_Throws()
    {
        IServiceCollection services = null;

        Should.Throw<ArgumentNullException>(
            () => services.AddPipelineBehavior(typeof(TestBehavior<,>)));
    }

    [Fact]
    public void AddPipelineBehavior_NullBehaviorType_Throws()
    {
        var services = new ServiceCollection();

        Should.Throw<ArgumentNullException>(
            () => services.AddPipelineBehavior(null));
    }

}
