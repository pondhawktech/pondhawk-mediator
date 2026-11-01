// Copyright (c) Pond Hawk Technologies Inc. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

namespace Pondhawk.Exceptions;

/// <summary>
/// Exception thrown when the caller is not authorized to perform an operation. Fixes
/// <see cref="ErrorKind.NotAuthorized"/>. This is sugar over <c>WithKind</c>; mapping is always
/// keyed on <see cref="ExternalException.Kind"/>, never on the concrete type.
/// </summary>
public sealed class NotAuthorizedException : FluentException<NotAuthorizedException>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="NotAuthorizedException"/> class with the specified message.
    /// </summary>
    /// <param name="message">The message describing the authorization failure.</param>
    public NotAuthorizedException(string message)
        : base(message)
    {
        WithKind(ErrorKind.NotAuthorized);
        WithErrorCode("NotAuthorized");
    }
}
