// Copyright (c) Pond Hawk Technologies Inc. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

namespace Pondhawk.Exceptions;

/// <summary>
/// Exception thrown when an operation conflicts with the current state of a resource. Fixes
/// <see cref="ErrorKind.Conflict"/>. This is sugar over <c>WithKind</c>; mapping is always
/// keyed on <see cref="ExternalException.Kind"/>, never on the concrete type.
/// </summary>
public sealed class ConflictException : FluentException<ConflictException>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ConflictException"/> class with the specified message.
    /// </summary>
    /// <param name="message">The message describing the conflict.</param>
    public ConflictException(string message)
        : base(message)
    {
        WithKind(ErrorKind.Conflict);
        WithErrorCode("Conflict");
    }
}
