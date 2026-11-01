// Copyright (c) Pond Hawk Technologies Inc. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

namespace Pondhawk.Mediator;

/// <summary>
/// The payload a list/query returns when results are paged — the page of items plus the paging
/// metadata a caller needs to navigate. A query that returns a page uses
/// <c>Response&lt;PagedResponse&lt;T&gt;&gt;</c>. Sibling of <see cref="Receipt"/> (the mutation payload).
/// Implements <see cref="IPagedResult"/> so the HTTP edge can lift the paging metadata into the
/// response envelope's <c>meta</c>.
/// </summary>
/// <typeparam name="T">The item (response) type.</typeparam>
/// <param name="Items">The items on this page.</param>
/// <param name="Page">The 1-based page number these items came from.</param>
/// <param name="PageSize">The maximum number of items per page.</param>
/// <param name="TotalCount">The total number of items across all pages.</param>
public sealed record PagedResponse<T>(IReadOnlyList<T> Items, int Page, int PageSize, int TotalCount) : IPagedResult
{
    /// <summary>Gets the total number of pages implied by <see cref="TotalCount"/> and <see cref="PageSize"/>.</summary>
    public int TotalPages
    {
        get
        {
            if (PageSize <= 0)
                return 0;

            var totalPages = (int)Math.Ceiling((double)TotalCount / PageSize);
            return totalPages;
        }
    }

    /// <inheritdoc />
    object IPagedResult.Items => Items;
}
