// Copyright (c) Pond Hawk Technologies Inc. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

namespace Pondhawk.Exceptions;

/// <summary>
/// RFC 7807-style problem detail for HTTP API error responses, with correlation ID and event segments.
/// </summary>
public class ProblemDetail
{

    /// <summary>
    /// Gets or sets the URI reference that identifies the problem type.
    /// </summary>
    public string Type { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets a short, human-readable summary of the problem.
    /// </summary>
    public string Title { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the HTTP status code for this problem.
    /// </summary>
    public int StatusCode { get; set; }

    /// <summary>
    /// Gets or sets a human-readable explanation specific to this occurrence of the problem.
    /// </summary>
    public string Detail { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets a URI reference that identifies the specific occurrence of the problem.
    /// </summary>
    public string Instance { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the correlation identifier for tracing this problem across systems.
    /// </summary>
    public string CorrelationId { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the event detail segments associated with this problem.
    /// </summary>
    public IList<EventDetail> Segments { get; set; } = [];


}
