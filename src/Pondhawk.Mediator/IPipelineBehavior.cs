// Copyright (c) Pond Hawk Technologies Inc. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Diagnostics.CodeAnalysis;

namespace Pondhawk.Mediator;

/// <summary>
/// Delegate representing the next step in the pipeline (another behavior or the handler).
/// </summary>
[SuppressMessage("Naming", "CA1711:Identifiers should not have incorrect suffix", Justification = "Delegate suffix is semantically correct for a delegate type")]
[SuppressMessage("Design", "MA0048:File name must match type name", Justification = "RequestHandlerDelegate is used exclusively by IPipelineBehavior")]
public delegate Task<TResponse> RequestHandlerDelegate<TResponse>();

/// <summary>
/// Pipeline behavior for cross-cutting concerns (logging, validation, etc.).
/// Behaviors wrap handler execution using a delegate chain pattern.
/// </summary>
public interface IPipelineBehavior<in TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    /// <summary>
    /// Handles the request, optionally calling the next step in the pipeline.
    /// </summary>
    [SuppressMessage("Naming", "CA1716:Identifiers should not match keywords", Justification = "next is the established parameter name for pipeline delegate chains")]
    Task<TResponse> HandleAsync(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken = default);
}
