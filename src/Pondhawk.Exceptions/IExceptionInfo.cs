// Copyright (c) Pond Hawk Technologies Inc. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

namespace Pondhawk.Exceptions;

/// <summary>
/// Provides structured error information (kind, code, explanation, details) for exception types.
/// </summary>
public interface IExceptionInfo
{

    /// <summary>
    /// Gets the classification of the error.
    /// </summary>
    ErrorKind Kind { get; }

    /// <summary>
    /// Gets the error code identifying the type of error.
    /// </summary>
    string ErrorCode { get; }

    /// <summary>
    /// Gets the human-readable explanation of the error.
    /// </summary>
    string Explanation { get; }

    /// <summary>
    /// Gets the list of event details associated with this error.
    /// </summary>
    IList<EventDetail> Details { get; }

}
