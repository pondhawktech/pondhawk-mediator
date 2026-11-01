// Copyright (c) Pond Hawk Technologies Inc. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using CommunityToolkit.Diagnostics;
using Pondhawk.Exceptions;

namespace Pondhawk.Mediator;

/// <summary>
/// Type-erased result wrapper for batch command responses.
/// Allows heterogeneous command results to be handled uniformly.
/// </summary>
public record BatchCommandResult
{
    /// <summary>
    /// Whether the command succeeded.
    /// </summary>
    public required bool Success { get; init; }

    /// <summary>
    /// The response object from the command (type-erased).
    /// </summary>
    public object? Response { get; init; }

    /// <summary>
    /// The type of command that was executed.
    /// </summary>
    public required string CommandType { get; init; }

    /// <summary>
    /// The UID of the entity affected by the command.
    /// </summary>
    public string? EntityUid { get; init; }

    /// <summary>
    /// The structured error if the command failed; otherwise <see langword="null"/>. This is the
    /// authoritative failure field — it preserves the error <see cref="ErrorKind"/> so a batch can
    /// route retry/dead-letter per command.
    /// </summary>
    public ErrorInfo? Error { get; init; }

    /// <summary>
    /// Human-readable error message if the command failed. Derived from
    /// <see cref="ErrorInfo.Explanation"/>; the authoritative field is <see cref="Error"/>.
    /// </summary>
    public string? ErrorMessage => Error?.Explanation;

    /// <summary>
    /// Creates a successful result from a command response.
    /// </summary>
    /// <typeparam name="T">The response type.</typeparam>
    /// <param name="response">The command response value.</param>
    /// <param name="entityUid">The UID of the affected entity, if any.</param>
    /// <returns>A successful batch result.</returns>
    public static BatchCommandResult Succeeded<T>(T response, string? entityUid = null)
    {
        return new BatchCommandResult
        {
            Success = true,
            Response = response,
            CommandType = typeof(T).Name.Replace("Response", "", StringComparison.Ordinal),
            EntityUid = entityUid
        };
    }

    /// <summary>
    /// Creates a failed result carrying the structured error (including its <see cref="ErrorKind"/>).
    /// </summary>
    /// <param name="commandType">The type of command that failed.</param>
    /// <param name="entityUid">The UID of the affected entity, if any.</param>
    /// <param name="error">The structured error describing the failure.</param>
    /// <returns>A failed batch result.</returns>
    public static BatchCommandResult Failed(string commandType, string? entityUid, ErrorInfo error)
    {
        Guard.IsNotNullOrWhiteSpace(commandType);
        Guard.IsNotNull(error);

        return new BatchCommandResult
        {
            Success = false,
            CommandType = commandType,
            EntityUid = entityUid,
            Error = error
        };
    }

    /// <summary>
    /// Attempts to get the response as a specific type.
    /// </summary>
    /// <typeparam name="T">The expected response type.</typeparam>
    /// <returns>The typed response, or <see langword="null"/> if the cast fails.</returns>
    public T? GetResponse<T>() where T : class
    {
        return Response as T;
    }
}
