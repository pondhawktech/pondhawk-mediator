// Copyright (c) Pond Hawk Technologies Inc. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using CommunityToolkit.Diagnostics;

namespace Pondhawk.Exceptions;

/// <summary>
/// Base exception with a fluent builder API for setting kind, error code, explanation, and details.
/// </summary>
public abstract class FluentException<TDescendant> : ExternalException where TDescendant : FluentException<TDescendant>
{

    /// <summary>
    /// Initializes a new instance of the <see cref="FluentException{TDescendant}"/> class with the specified message.
    /// </summary>
    /// <param name="message">The error message.</param>
    protected FluentException(string message) : base(message)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="FluentException{TDescendant}"/> class with the specified message and inner exception.
    /// </summary>
    /// <param name="message">The error message.</param>
    /// <param name="inner">The inner exception that caused this exception.</param>
    protected FluentException(string message, Exception inner) : base(message, inner)
    {
    }


    /// <summary>
    /// Sets the <see cref="ExternalException.Kind"/> and returns this instance for fluent chaining.
    /// </summary>
    /// <param name="kind">The error kind to set.</param>
    /// <returns>This exception instance.</returns>
    public TDescendant WithKind(ErrorKind kind)
    {
        Kind = kind;
        return (TDescendant)this;
    }

    /// <summary>
    /// Sets the <see cref="ExternalException.ErrorCode"/> and returns this instance for fluent chaining.
    /// </summary>
    /// <param name="code">The error code to set.</param>
    /// <returns>This exception instance.</returns>
    public TDescendant WithErrorCode(string code)
    {
        Guard.IsNotNull(code);
        ErrorCode = code;
        return (TDescendant)this;
    }

    /// <summary>
    /// Sets the <see cref="ExternalException.Explanation"/> and returns this instance for fluent chaining.
    /// </summary>
    /// <param name="explanation">The explanation text to set.</param>
    /// <returns>This exception instance.</returns>
    public TDescendant WithExplanation(string explanation)
    {
        Guard.IsNotNull(explanation);
        Explanation = explanation;
        return (TDescendant)this;

    }

    /// <summary>
    /// Sets the <see cref="ExternalException.CorrelationId"/> and returns this instance for fluent chaining.
    /// </summary>
    /// <param name="correlationId">The correlation identifier to set.</param>
    /// <returns>This exception instance.</returns>
    public TDescendant WithCorrelationId(string correlationId)
    {
        Guard.IsNotNull(correlationId);
        CorrelationId = correlationId;
        return (TDescendant)this;

    }

    /// <summary>
    /// Adds a single <see cref="EventDetail"/> to this exception and returns this instance for fluent chaining.
    /// </summary>
    /// <param name="detail">The event detail to add.</param>
    /// <returns>This exception instance.</returns>
    public TDescendant WithDetail(EventDetail detail)
    {

        Guard.IsNotNull(detail);

        Details.Add(detail);
        return (TDescendant)this;

    }

    /// <summary>
    /// Adds multiple <see cref="EventDetail"/> instances to this exception and returns this instance for fluent chaining.
    /// </summary>
    /// <param name="details">The event details to add.</param>
    /// <returns>This exception instance.</returns>
    public TDescendant WithDetails(IEnumerable<EventDetail> details)
    {
        Guard.IsNotNull(details);

        foreach (var d in details)
            Details.Add(d);

        return (TDescendant)this;

    }

    /// <summary>
    /// Populates this exception from an <see cref="IExceptionInfo"/> and returns this instance for fluent chaining.
    /// </summary>
    /// <param name="info">The exception info to copy kind, error code, explanation, and details from.</param>
    /// <returns>This exception instance.</returns>
    public TDescendant With(IExceptionInfo info)
    {

        Guard.IsNotNull(info);

        WithKind(info.Kind);
        WithErrorCode(info.ErrorCode);
        WithExplanation(info.Explanation);
        WithDetails(info.Details);


        return (TDescendant)this;

    }



}
