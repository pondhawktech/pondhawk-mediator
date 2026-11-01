// Copyright (c) Pond Hawk Technologies Inc. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

namespace Pondhawk.Exceptions;

/// <summary>
/// Canonical, transport-agnostic retry policy for <see cref="ErrorKind"/> values so that
/// every queue consumer routes <c>retry vs. dead-letter</c> identically by default.
/// </summary>
/// <remarks>
/// Consumers may override this policy, but it is the shared default. Note that the HTTP
/// status-code mapping is deliberately NOT defined here — that belongs to the ASP.NET layer.
/// </remarks>
public static class ErrorKindPolicy
{
    /// <summary>
    /// Indicates whether an error of the given kind is transient and therefore worth
    /// retrying / requeuing. Everything else is treated as permanent (dead-letter).
    /// </summary>
    /// <param name="kind">The error kind to classify.</param>
    /// <returns><see langword="true"/> if the kind is transient; otherwise <see langword="false"/>.</returns>
    public static bool IsTransient(this ErrorKind kind) =>
        kind is ErrorKind.System or ErrorKind.Remote or ErrorKind.Concurrency;
}
