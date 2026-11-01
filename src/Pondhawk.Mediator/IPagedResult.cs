// Copyright (c) Pond Hawk Technologies Inc. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

namespace Pondhawk.Mediator;

/// <summary>
/// Marker for a paged query result that the HTTP edge recognizes, so it can lift the page metadata into
/// the response envelope's <c>meta.pagination</c> and place the items alone in <c>data</c>.
/// <see cref="PagedResponse{T}"/> implements this; handlers keep returning the strongly-typed
/// <see cref="PagedResponse{T}"/> and never touch the envelope.
/// </summary>
public interface IPagedResult
{
    /// <summary>Gets the items on the current page, boxed — the envelope serializes them as <c>data</c>.</summary>
    object Items { get; }

    /// <summary>Gets the 1-based page number.</summary>
    int Page { get; }

    /// <summary>Gets the page size.</summary>
    int PageSize { get; }

    /// <summary>Gets the total item count across all pages.</summary>
    int TotalCount { get; }

    /// <summary>Gets the total number of pages.</summary>
    int TotalPages { get; }
}
