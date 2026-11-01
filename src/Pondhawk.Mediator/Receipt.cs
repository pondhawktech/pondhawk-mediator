// Copyright (c) Pond Hawk Technologies Inc. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

namespace Pondhawk.Mediator;

/// <summary>
/// The payload a mutation returns when it has no entity to hand back — proof it ran, plus a tally.
/// A command with no entity to return uses <c>Response&lt;Receipt&gt;</c>.
/// </summary>
public sealed record Receipt
{
    /// <summary>
    /// Gets the number of entities affected. 1 for a single command, the real count for a bulk one,
    /// 0 = nothing changed (the canonical no-op signal).
    /// </summary>
    public int Affected { get; init; } = 1;

    /// <summary>
    /// Gets a receipt for a single affected entity (<see cref="Affected"/> = 1).
    /// </summary>
    public static Receipt One { get; } = new();

    /// <summary>
    /// Creates a receipt for the given tally.
    /// </summary>
    /// <param name="affected">The number of entities affected.</param>
    /// <returns>A receipt carrying the tally.</returns>
    public static Receipt Of(int affected) => new() { Affected = affected };
}
