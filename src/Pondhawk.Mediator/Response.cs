// Copyright (c) Pond Hawk Technologies Inc. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Diagnostics.CodeAnalysis;
using Pondhawk.Exceptions;

namespace Pondhawk.Mediator;

/// <summary>
/// Success/failure envelope returned by <see cref="IMediator.SendAsync{TResponse}"/>. Carries
/// either the handler's value or a structured <see cref="ErrorInfo"/> (including the
/// <see cref="ErrorKind"/>), so callers that have no exception-mapping pipeline — queue consumers
/// and batch — can branch on the outcome without catching.
/// </summary>
/// <remarks>
/// The envelope is internal to the process. Edges (an ASP.NET result filter, a queue consumer)
/// adapt it to their transport; it is not itself a wire contract.
/// </remarks>
/// <typeparam name="T">The response value type.</typeparam>
[SuppressMessage("Design", "CA1000:Do not declare static members on generic types", Justification = "Static factory methods are the intended public construction API for the envelope (Result<T>.Ok-style).")]
public readonly record struct Response<T> : IResponse
{
    /// <summary>
    /// The failure carried by <c>default(Response&lt;T&gt;)</c> and by any failure state whose backing
    /// error is null. A <see cref="Response{T}"/> is a value type, so <c>default</c> — an uninitialized
    /// field, an unpopulated array slot, a dictionary miss — yields <c>Ok = false</c> with no backing
    /// error. Rather than let that violate the "error non-null iff not Ok" invariant (and NRE the
    /// accessors), it reads as this coherent System failure.
    /// </summary>
    private static readonly ErrorInfo Uninitialized = new()
    {
        Kind = ErrorKind.System,
        ErrorCode = "System",
        Explanation = "Response was not initialized.",
    };

    private readonly bool _ok;
    private readonly ErrorInfo? _error;

    private Response(bool ok, T? value, ErrorInfo? error)
    {
        _ok = ok;
        Value = value;
        _error = error;
    }

    /// <summary>
    /// Gets a value indicating whether the request succeeded.
    /// </summary>
    public bool Ok => _ok;

    /// <summary>
    /// Gets the response value. Meaningful only when <see cref="Ok"/> is <see langword="true"/>.
    /// </summary>
    public T? Value { get; }

    /// <summary>
    /// Non-generic access to <see cref="Value"/> boxed as <see cref="object"/>, for
    /// <see cref="IResponse"/> consumers that do not know <typeparamref name="T"/>.
    /// </summary>
    object? IResponse.Value => Value;

    /// <summary>
    /// Gets the structured error. Non-<see langword="null"/> if and only if <see cref="Ok"/> is
    /// <see langword="false"/> — a failure with no backing error (e.g. <c>default</c>) reads as
    /// <see cref="Uninitialized"/> so the invariant holds on read.
    /// </summary>
    public ErrorInfo? Error => _ok ? null : (_error ?? Uninitialized);

    /// <summary>
    /// Gets a value indicating whether the request failed. The convenience inverse of <see cref="Ok"/>.
    /// </summary>
    public bool IsError => !Ok;

    /// <summary>
    /// Gets the response value on success, or throws on failure. The framework's name for the
    /// success-value accessor; an alias of <see cref="GetValueOrThrow"/>.
    /// </summary>
    /// <exception cref="InvalidOperationException">Thrown when <see cref="Ok"/> is <see langword="false"/>.</exception>
    public T AsEntity => GetValueOrThrow();

    /// <summary>
    /// Gets the failure to propagate, or throws on success. Combined with the implicit
    /// <see cref="ErrorInfo"/> conversion, this lets a failed <see cref="Response{T}"/> be re-raised
    /// into a differently-typed <see cref="Response{T}"/> via <c>return source.AsError;</c>.
    /// </summary>
    /// <exception cref="InvalidOperationException">Thrown when <see cref="Ok"/> is <see langword="true"/>.</exception>
    public ErrorInfo AsError =>
        Error ?? throw new InvalidOperationException("Response is not an error.");

    /// <summary>
    /// Creates a successful response carrying the given value.
    /// </summary>
    /// <param name="value">The handler's response value.</param>
    /// <returns>A successful envelope.</returns>
    public static Response<T> Success(T value) => new(true, value, null);

    /// <summary>
    /// Creates a failed response carrying the given error.
    /// </summary>
    /// <param name="error">The structured error.</param>
    /// <returns>A failed envelope.</returns>
    public static Response<T> Failure(ErrorInfo error)
    {
        CommunityToolkit.Diagnostics.Guard.IsNotNull(error);
        return new Response<T>(false, default, error);
    }

    /// <summary>
    /// Implicitly wraps a value in a successful envelope, so a value can be returned where a
    /// <see cref="Response{T}"/> is expected. The named alternative is <see cref="Success(T)"/>.
    /// There is deliberately no conversion in the reverse direction — unwrapping must go through
    /// <see cref="Value"/>, <see cref="GetValueOrThrow"/>, or <see cref="Match{TResult}"/> so a
    /// failure can never be silently treated as a value.
    /// </summary>
    /// <param name="value">The value to wrap.</param>
    public static implicit operator Response<T>(T value) => Success(value);

    /// <summary>
    /// Implicitly wraps an <see cref="ErrorInfo"/> in a failed envelope. This enables cross-type
    /// error propagation: a failure read off a <see cref="Response{T}"/> of one payload type (via
    /// <see cref="AsError"/>) converts to a failed <see cref="Response{T}"/> of any other payload
    /// type, carrying the same error. Unambiguous against the value conversion because payload
    /// types <typeparamref name="T"/> are never <see cref="ErrorInfo"/>.
    /// </summary>
    /// <param name="error">The error to wrap.</param>
    public static implicit operator Response<T>(ErrorInfo error) => Failure(error);

    /// <summary>
    /// Projects the envelope to a single value, invoking <paramref name="onSuccess"/> when
    /// <see cref="Ok"/> is <see langword="true"/> and <paramref name="onFailure"/> otherwise.
    /// </summary>
    /// <typeparam name="TResult">The projection result type.</typeparam>
    /// <param name="onSuccess">Invoked with the value on success.</param>
    /// <param name="onFailure">Invoked with the error on failure.</param>
    /// <returns>The projected result.</returns>
    public TResult Match<TResult>(Func<T, TResult> onSuccess, Func<ErrorInfo, TResult> onFailure)
    {
        CommunityToolkit.Diagnostics.Guard.IsNotNull(onSuccess);
        CommunityToolkit.Diagnostics.Guard.IsNotNull(onFailure);

        return Ok ? onSuccess(Value!) : onFailure(Error!);
    }

    /// <summary>
    /// Returns the value on success, or throws an <see cref="InvalidOperationException"/> describing
    /// the error on failure. For call sites that genuinely want exception flow.
    /// </summary>
    /// <returns>The response value.</returns>
    /// <exception cref="InvalidOperationException">Thrown when <see cref="Ok"/> is <see langword="false"/>.</exception>
    public T GetValueOrThrow()
    {
        if (!Ok)
            throw new InvalidOperationException($"Response failed ({Error!.Kind}/{Error.ErrorCode}): {Error.Explanation}");

        return Value!;
    }
}
