// Copyright (c) Pond Hawk Technologies Inc. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using CommunityToolkit.Diagnostics;

namespace Pondhawk.Mediator;

/// <summary>
/// Tracks batch execution state via AsyncLocal.
/// Used to detect when commands execute as part of a batch operation
/// (e.g., for logging verbosity control).
/// </summary>
public static class BatchExecutionContext
{
    private static readonly AsyncLocal<BatchInfo?> _current = new();

    /// <summary>
    /// Returns true if currently executing within a batch context.
    /// </summary>
    public static bool IsInBatch => _current.Value != null;

    /// <summary>
    /// Returns the current batch nesting depth (0 if not in a batch).
    /// </summary>
    public static int Depth => _current.Value?.Depth ?? 0;

    /// <summary>
    /// Returns the current batch ID, or null if not in a batch.
    /// </summary>
    public static string? BatchId => _current.Value?.BatchId;

    /// <summary>
    /// Begins a new batch context. Dispose the returned object to exit the batch.
    /// </summary>
    /// <param name="batchId">A unique identifier for the batch operation.</param>
    /// <returns>An IDisposable that restores the previous context when disposed.</returns>
    public static IDisposable BeginBatch(string batchId)
    {
        Guard.IsNotNullOrWhiteSpace(batchId);

        var previous = _current.Value;
        var newDepth = (previous?.Depth ?? 0) + 1;
        _current.Value = new BatchInfo(batchId, newDepth);
        return new BatchScope(previous);
    }

    private sealed class BatchScope(BatchInfo? previous) : IDisposable
    {
        public void Dispose()
        {
            _current.Value = previous;
        }
    }

    private sealed record BatchInfo(string BatchId, int Depth);
}
