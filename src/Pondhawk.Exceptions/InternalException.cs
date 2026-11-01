// Copyright (c) Pond Hawk Technologies Inc. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

namespace Pondhawk.Exceptions;

/// <summary>
/// Base exception for internal/system errors with an explanation and optional event details.
/// </summary>
public class InternalException : Exception
{

    /// <summary>
    /// Initializes a new instance of the <see cref="InternalException"/> class with the specified message.
    /// </summary>
    /// <param name="message">The error message describing the internal error.</param>
    public InternalException(string message) : base(message)
    {
        Explanation = message;
    }


    /// <summary>
    /// Gets the human-readable explanation of the internal error.
    /// </summary>
    public string Explanation { get; protected set; }

    /// <summary>
    /// Gets the list of event details associated with this internal exception.
    /// </summary>
    public IList<EventDetail> Details { get; protected set; } = new List<EventDetail>();

}
