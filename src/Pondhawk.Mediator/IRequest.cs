// Copyright (c) Pond Hawk Technologies Inc. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Diagnostics.CodeAnalysis;

namespace Pondhawk.Mediator;

/// <summary>
/// Unified marker interface for requests with a response type.
/// Works for both commands and queries.
/// </summary>
public interface IRequest<TResponse> { }

/// <summary>
/// Convenience alias for commands to preserve semantic intent.
/// </summary>
[SuppressMessage("Design", "MA0048:File name must match type name", Justification = "ICommand and IQuery are convenience aliases tightly coupled with IRequest")]
public interface ICommand<TResponse> : IRequest<TResponse> { }

/// <summary>
/// Convenience alias for queries to preserve semantic intent.
/// </summary>
[SuppressMessage("Design", "MA0048:File name must match type name", Justification = "ICommand and IQuery are convenience aliases tightly coupled with IRequest")]
public interface IQuery<TResponse> : IRequest<TResponse> { }
