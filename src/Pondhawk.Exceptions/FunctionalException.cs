// Copyright (c) Pond Hawk Technologies Inc. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

namespace Pondhawk.Exceptions;

/// <summary>
/// Exception representing a functional/business-logic error.
/// </summary>
public class FunctionalException : FluentException<FunctionalException>
{


    /// <summary>
    /// Initializes a new instance of the <see cref="FunctionalException"/> class with the specified message.
    /// </summary>
    /// <param name="message">The error message describing the functional error.</param>
    public FunctionalException(string message) : base(message)
    {

        Kind = ErrorKind.Functional;

    }

    /// <summary>
    /// Initializes a new instance of the <see cref="FunctionalException"/> class with the specified message and inner exception.
    /// </summary>
    /// <param name="message">The error message describing the functional error.</param>
    /// <param name="inner">The inner exception that caused this exception.</param>
    public FunctionalException(string message, Exception inner) : base(message, inner)
    {

        Kind = ErrorKind.Functional;

    }


}
