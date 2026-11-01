// Copyright (c) Pond Hawk Technologies Inc. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

namespace Pondhawk.Mediator;

/// <summary>
/// Mediator interface for sending requests through the pipeline to handlers.
/// </summary>
public interface IMediator
{
    /// <summary>
    /// Sends a request through the pipeline to its handler and returns a <see cref="Response{T}"/>
    /// envelope. The handler still throws on error; the mediator is the single seam that converts a
    /// throw into a structured failure (preserving the error <c>Kind</c>). Argument validation and
    /// missing-handler configuration errors still throw; <see cref="OperationCanceledException"/> is
    /// never enveloped.
    /// </summary>
    /// <typeparam name="TResponse">The response value type.</typeparam>
    /// <param name="request">The request to dispatch.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>A success or failure envelope.</returns>
    Task<Response<TResponse>> SendAsync<TResponse>(
        IRequest<TResponse> request,
        CancellationToken cancellationToken = default);
}
