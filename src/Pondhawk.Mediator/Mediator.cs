// Copyright (c) Pond Hawk Technologies Inc. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;
using CommunityToolkit.Diagnostics;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Pondhawk.Exceptions;

namespace Pondhawk.Mediator;

/// <summary>
/// Default mediator implementation that routes requests through pipeline behaviors to handlers.
/// Uses cached handler wrappers to avoid reflection on every request. Dispatch is the single seam
/// that converts a thrown error into a <see cref="Response{T}"/> failure envelope.
/// </summary>
[SuppressMessage("Design", "MA0049:Type name should not match containing namespace", Justification = "Mediator is the canonical name for this type")]
public partial class Mediator : IMediator
{
    private static readonly ConcurrentDictionary<Type, RequestHandlerBase> HandlerWrappers = new();

    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<Mediator> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="Mediator"/> class.
    /// </summary>
    /// <param name="serviceProvider">The service provider used to resolve handlers and behaviors.</param>
    /// <param name="logger">
    /// The logger used to record dispatch failures by kind. Falls back to a no-op logger when not
    /// supplied (e.g. when <c>AddLogging()</c> has not been called).
    /// </param>
    public Mediator(IServiceProvider serviceProvider, ILogger<Mediator>? logger = null)
    {
        Guard.IsNotNull(serviceProvider);

        _serviceProvider = serviceProvider;
        _logger = logger ?? NullLogger<Mediator>.Instance;
    }

    /// <inheritdoc />
    public async Task<Response<TResponse>> SendAsync<TResponse>(
        IRequest<TResponse> request,
        CancellationToken cancellationToken = default)
    {
        Guard.IsNotNull(request);

        var requestType = request.GetType();

        var wrapper = HandlerWrappers.GetOrAdd(
            requestType,
            static type =>
            {
                var requestInterface = type.GetInterfaces()
                    .FirstOrDefault(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IRequest<>))
                    ?? throw new InvalidOperationException($"Type {type.Name} does not implement IRequest<TResponse>");

                var responseType = requestInterface.GetGenericArguments()[0];

                var wrapperType = typeof(RequestHandlerWrapper<,>).MakeGenericType(type, responseType);
                return (RequestHandlerBase)Activator.CreateInstance(wrapperType)!;
            });

        // Resolve the handler and build the pipeline OUTSIDE the try: a missing handler is a
        // configuration/programming error and must keep throwing, not be laundered into an envelope.
        var pipeline = wrapper.BuildPipeline<TResponse>(request, _serviceProvider, cancellationToken);

        try
        {
            var value = await pipeline().ConfigureAwait(false);
            return Response<TResponse>.Success(value);
        }
        catch (OperationCanceledException)
        {
            // Never swallow cancellation.
            throw;
        }
        catch (ExternalException ex)
        {
            // Expected, app-level outcome (incl. validation): preserve the kind on the envelope.
            LogByKind(ex, requestType.Name);
            return Response<TResponse>.Failure(ErrorInfo.From(ex));
        }
        catch (Exception ex)
        {
            // Unexpected = a bug. Keep it loud (Error) but do not let it abort the caller.
            LogUnhandled(ex, requestType.Name);
            return Response<TResponse>.Failure(ErrorInfo.System(ex));
        }
    }

    /// <summary>
    /// Logs an expected, application-level failure at a level appropriate to its kind: the
    /// 4xx-family kinds are logged at Information/Warning (they are normal outcomes, not faults),
    /// while the 5xx family is logged at Error.
    /// </summary>
    private void LogByKind(ExternalException ex, string request)
    {
        var level = ex.Kind switch
        {
            ErrorKind.NotFound
                or ErrorKind.Predicate
                or ErrorKind.BadRequest
                or ErrorKind.Functional
                or ErrorKind.NotImplemented => LogLevel.Information,
            ErrorKind.NotAuthorized
                or ErrorKind.AuthenticationRequired
                or ErrorKind.Conflict
                or ErrorKind.Concurrency => LogLevel.Warning,
            _ => LogLevel.Error,
        };

        LogDispatchFailure(level, request, ex.Kind, ex.ErrorCode, ex.Explanation);
    }

    [LoggerMessage(Message = "Dispatch of {Request} failed with {Kind} ({ErrorCode}): {Explanation}")]
    private partial void LogDispatchFailure(LogLevel level, string request, ErrorKind kind, string errorCode, string explanation);

    [LoggerMessage(Level = LogLevel.Error, Message = "Unhandled error dispatching {Request}")]
    private partial void LogUnhandled(Exception exception, string request);
}

/// <summary>
/// Base class for handler wrappers - enables caching without knowing generic types.
/// </summary>
[SuppressMessage("Design", "MA0048:File name must match type name", Justification = "Internal implementation details of the Mediator class")]
internal abstract class RequestHandlerBase
{
    public abstract RequestHandlerDelegate<TResponse> BuildPipeline<TResponse>(
        object request,
        IServiceProvider serviceProvider,
        CancellationToken cancellationToken);
}

/// <summary>
/// Typed wrapper that handles pipeline construction without reflection.
/// One instance is cached per request type.
/// </summary>
[SuppressMessage("Design", "MA0048:File name must match type name", Justification = "Internal implementation details of the Mediator class")]
internal sealed class RequestHandlerWrapper<TRequest, TResponse> : RequestHandlerBase
    where TRequest : IRequest<TResponse>
{
    public override RequestHandlerDelegate<TResult> BuildPipeline<TResult>(
        object request,
        IServiceProvider serviceProvider,
        CancellationToken cancellationToken)
    {
        return (RequestHandlerDelegate<TResult>)(object)BuildPipeline((TRequest)request, serviceProvider, cancellationToken);
    }

    private static RequestHandlerDelegate<TResponse> BuildPipeline(
        TRequest request,
        IServiceProvider serviceProvider,
        CancellationToken cancellationToken)
    {
        var handler = serviceProvider.GetService<IRequestHandler<TRequest, TResponse>>()
            ?? throw new InvalidOperationException($"No handler registered for {typeof(TRequest).Name}");

        var behaviors = serviceProvider.GetServices<IPipelineBehavior<TRequest, TResponse>>();

        // Build pipeline: behaviors wrap handler
        RequestHandlerDelegate<TResponse> pipeline = () => handler.HandleAsync(request, cancellationToken);

        // Wrap behaviors from innermost to outermost
        foreach (var behavior in behaviors.Reverse())
        {
            var next = pipeline;
            pipeline = () => behavior.HandleAsync(request, next, cancellationToken);
        }

        return pipeline;
    }
}
