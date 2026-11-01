// Copyright (c) Pond Hawk Technologies Inc. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

namespace Pondhawk.Exceptions;

/// <summary>
/// Classifies errors by kind (e.g. NotFound, Predicate, System, Functional).
/// </summary>
public enum ErrorKind
{
    /// <summary>The error kind is unknown or unspecified.</summary>
    Unknown,
    /// <summary>A predicate or validation constraint was not satisfied.</summary>
    Predicate,
    /// <summary>The request was malformed or invalid.</summary>
    BadRequest,
    /// <summary>A business-logic or functional error occurred.</summary>
    Functional,
    /// <summary>The requested operation is not implemented.</summary>
    NotImplemented,
    /// <summary>The requested resource was not found.</summary>
    NotFound,
    /// <summary>A concurrency conflict occurred.</summary>
    Concurrency,
    /// <summary>A system-level or infrastructure error occurred.</summary>
    System,
    /// <summary>No error; the operation succeeded.</summary>
    None,
    /// <summary>Authentication is required to perform the operation.</summary>
    AuthenticationRequired,
    /// <summary>The caller is not authorized to perform the operation.</summary>
    NotAuthorized,
    /// <summary>A conflict with the current state of the resource was detected.</summary>
    Conflict,
    /// <summary>An error occurred in a remote service or dependency.</summary>
    Remote
}
