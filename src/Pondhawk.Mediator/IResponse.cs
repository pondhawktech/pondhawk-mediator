// Copyright (c) Pond Hawk Technologies Inc. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Diagnostics.CodeAnalysis;
using Pondhawk.Exceptions;

namespace Pondhawk.Mediator;

/// <summary>
/// Non-generic view of a <see cref="Response{T}"/>, for code that must inspect an outcome without
/// knowing the payload type — e.g. an ASP.NET endpoint filter or exception handler mapping a boxed
/// response to an HTTP result. <see cref="Response{T}"/> implements this; the strongly-typed surface
/// (<see cref="Response{T}.Value"/>, <c>AsEntity</c>, <c>Match</c>, …) remains the primary API.
/// </summary>
public interface IResponse
{
    /// <summary>Gets a value indicating whether the request succeeded.</summary>
    bool Ok { get; }

    /// <summary>
    /// Gets the structured error. Non-<see langword="null"/> if and only if <see cref="Ok"/> is
    /// <see langword="false"/> (the same invariant as <see cref="Response{T}.Error"/>).
    /// </summary>
    [SuppressMessage("Naming", "CA1716:Identifiers should not match keywords",
        Justification = "Mirrors Response<T>.Error, the property this non-generic view abstracts.")]
    ErrorInfo? Error { get; }

    /// <summary>
    /// Gets the success value boxed as <see cref="object"/> — <see langword="null"/> on failure or
    /// when the payload itself is null.
    /// </summary>
    object? Value { get; }
}
