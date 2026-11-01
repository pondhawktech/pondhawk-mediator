// Copyright (c) Pond Hawk Technologies Inc. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

namespace Pondhawk.Exceptions;

/// <summary>
/// Exception thrown when validation produces violation events, carrying the violation details.
/// </summary>
public class FailedValidationException : FluentException<FailedValidationException>
{

    /// <summary>
    /// Initializes a new instance of the <see cref="FailedValidationException"/> class with the specified violations.
    /// </summary>
    /// <param name="violations">The validation violation details.</param>
    public FailedValidationException(IEnumerable<EventDetail> violations) : base("Violation events occurred during validation.")
    {
        WithKind(ErrorKind.Predicate);
        WithDetails(violations);
    }

}
