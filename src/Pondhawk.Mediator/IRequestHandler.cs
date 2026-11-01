// Copyright (c) Pond Hawk Technologies Inc. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Diagnostics.CodeAnalysis;

namespace Pondhawk.Mediator;

/// <summary>
/// Handler interface for processing requests.
/// </summary>
public interface IRequestHandler<in TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    /// <summary>
    /// Handles the request and returns a response.
    /// </summary>
    Task<TResponse> HandleAsync(TRequest request, CancellationToken cancellationToken = default);
}

/// <summary>
/// Convenience alias for command handlers.
/// </summary>
[SuppressMessage("Design", "MA0048:File name must match type name", Justification = "ICommandHandler and IQueryHandler are convenience aliases tightly coupled with IRequestHandler")]
public interface ICommandHandler<in TCommand, TResponse> : IRequestHandler<TCommand, TResponse>
    where TCommand : ICommand<TResponse> { }

/// <summary>
/// Convenience alias for query handlers.
/// </summary>
[SuppressMessage("Design", "MA0048:File name must match type name", Justification = "ICommandHandler and IQueryHandler are convenience aliases tightly coupled with IRequestHandler")]
public interface IQueryHandler<in TQuery, TResponse> : IRequestHandler<TQuery, TResponse>
    where TQuery : IQuery<TResponse> { }
