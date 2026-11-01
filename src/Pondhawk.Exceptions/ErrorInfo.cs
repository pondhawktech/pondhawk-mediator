// Copyright (c) Pond Hawk Technologies Inc. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using CommunityToolkit.Diagnostics;

namespace Pondhawk.Exceptions;

/// <summary>
/// One transport-agnostic error shape, shared by exceptions and the mediator
/// <c>Response</c> envelope so the two cannot drift. Carries the error
/// <see cref="ErrorKind"/> (the signal queue consumers route on), a stable
/// machine-readable code, a human-readable explanation, and any structured details.
/// </summary>
public sealed record ErrorInfo
{
    /// <summary>
    /// Gets the classification of the error. This is the canonical signal callers
    /// (HTTP edges, queue consumers, batch) adapt to their transport.
    /// </summary>
    public required ErrorKind Kind { get; init; }

    /// <summary>
    /// Gets the stable, machine-readable error code.
    /// </summary>
    public required string ErrorCode { get; init; }

    /// <summary>
    /// Gets the human-readable explanation of the error.
    /// </summary>
    public required string Explanation { get; init; }

    /// <summary>
    /// Gets the structured details associated with the error (e.g. validation violations).
    /// </summary>
    public IReadOnlyList<EventDetail> Details { get; init; } = [];

    /// <summary>
    /// Creates an <see cref="ErrorInfo"/> from an <see cref="ExternalException"/>, copying its
    /// kind, error code, explanation, and details.
    /// </summary>
    /// <param name="ex">The application-level exception to convert.</param>
    /// <returns>An error shape mirroring the exception.</returns>
    public static ErrorInfo From(ExternalException ex)
    {
        Guard.IsNotNull(ex);

        return new ErrorInfo
        {
            Kind = ex.Kind,
            ErrorCode = ex.ErrorCode,
            Explanation = ex.Explanation,
            Details = [.. ex.Details],
        };
    }

    /// <summary>
    /// Creates a <see cref="ErrorKind.System"/> <see cref="ErrorInfo"/> from an unexpected exception.
    /// Used for bugs/infrastructure faults that are not modeled as an <see cref="ExternalException"/>.
    /// </summary>
    /// <param name="ex">The unexpected exception.</param>
    /// <returns>A system-kind error shape carrying the exception message.</returns>
    public static ErrorInfo System(Exception ex)
    {
        Guard.IsNotNull(ex);

        return new ErrorInfo
        {
            Kind = ErrorKind.System,
            ErrorCode = "System",
            Explanation = ex.Message,
        };
    }
}
