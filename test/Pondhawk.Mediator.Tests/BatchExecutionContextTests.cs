// Copyright (c) Pond Hawk Technologies Inc. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using Pondhawk.Mediator;
using Shouldly;
using Xunit;

namespace Pondhawk.Mediator.Tests;

public class BatchExecutionContextTests
{

    // ── Default state ──

    [Fact]
    public void Default_IsNotInBatch()
    {
        BatchExecutionContext.IsInBatch.ShouldBeFalse();
    }

    [Fact]
    public void Default_DepthIsZero()
    {
        BatchExecutionContext.Depth.ShouldBe(0);
    }

    [Fact]
    public void Default_BatchIdIsNull()
    {
        BatchExecutionContext.BatchId.ShouldBeNull();
    }

    // ── BeginBatch ──

    [Fact]
    public void BeginBatch_SetsIsInBatch()
    {
        using var scope = BatchExecutionContext.BeginBatch("batch-1");

        BatchExecutionContext.IsInBatch.ShouldBeTrue();
    }

    [Fact]
    public void BeginBatch_SetsBatchId()
    {
        using var scope = BatchExecutionContext.BeginBatch("batch-1");

        BatchExecutionContext.BatchId.ShouldBe("batch-1");
    }

    [Fact]
    public void BeginBatch_SetsDepthToOne()
    {
        using var scope = BatchExecutionContext.BeginBatch("batch-1");

        BatchExecutionContext.Depth.ShouldBe(1);
    }

    [Fact]
    public void BeginBatch_NullBatchId_Throws()
    {
        Should.Throw<ArgumentNullException>(() => BatchExecutionContext.BeginBatch(null));
    }

    [Fact]
    public void BeginBatch_EmptyBatchId_Throws()
    {
        Should.Throw<ArgumentException>(() => BatchExecutionContext.BeginBatch(""));
    }

    [Fact]
    public void BeginBatch_WhitespaceBatchId_Throws()
    {
        Should.Throw<ArgumentException>(() => BatchExecutionContext.BeginBatch("   "));
    }

    // ── Dispose restores state ──

    [Fact]
    public void Dispose_RestoresNotInBatch()
    {
        var scope = BatchExecutionContext.BeginBatch("batch-1");
        scope.Dispose();

        BatchExecutionContext.IsInBatch.ShouldBeFalse();
        BatchExecutionContext.BatchId.ShouldBeNull();
        BatchExecutionContext.Depth.ShouldBe(0);
    }

    // ── Nesting ──

    [Fact]
    public void NestedBatch_IncrementsDepth()
    {
        using var outer = BatchExecutionContext.BeginBatch("outer");
        BatchExecutionContext.Depth.ShouldBe(1);

        using var inner = BatchExecutionContext.BeginBatch("inner");
        BatchExecutionContext.Depth.ShouldBe(2);
        BatchExecutionContext.BatchId.ShouldBe("inner");
    }

    [Fact]
    public void NestedBatch_DisposeInner_RestoresOuter()
    {
        using var outer = BatchExecutionContext.BeginBatch("outer");

        var inner = BatchExecutionContext.BeginBatch("inner");
        inner.Dispose();

        BatchExecutionContext.IsInBatch.ShouldBeTrue();
        BatchExecutionContext.Depth.ShouldBe(1);
        BatchExecutionContext.BatchId.ShouldBe("outer");
    }

    [Fact]
    public void TripleNesting_RestoresCorrectly()
    {
        using var level1 = BatchExecutionContext.BeginBatch("L1");
        BatchExecutionContext.Depth.ShouldBe(1);

        var level2 = BatchExecutionContext.BeginBatch("L2");
        BatchExecutionContext.Depth.ShouldBe(2);

        var level3 = BatchExecutionContext.BeginBatch("L3");
        BatchExecutionContext.Depth.ShouldBe(3);
        BatchExecutionContext.BatchId.ShouldBe("L3");

        level3.Dispose();
        BatchExecutionContext.Depth.ShouldBe(2);
        BatchExecutionContext.BatchId.ShouldBe("L2");

        level2.Dispose();
        BatchExecutionContext.Depth.ShouldBe(1);
        BatchExecutionContext.BatchId.ShouldBe("L1");
    }

    // ── AsyncLocal isolation ──

    [Fact]
    public async Task AsyncLocal_DoesNotLeakAcrossTasks()
    {
        using var scope = BatchExecutionContext.BeginBatch("parent");

        var childSawBatch = false;
        await Task.Run(() =>
        {
            childSawBatch = BatchExecutionContext.IsInBatch;
        });

        // AsyncLocal flows into child tasks
        childSawBatch.ShouldBeTrue();

        // But child modifications do not flow back
        var childModifiedDepth = 0;
        await Task.Run(() =>
        {
            using var childScope = BatchExecutionContext.BeginBatch("child");
            childModifiedDepth = BatchExecutionContext.Depth;
        });

        childModifiedDepth.ShouldBe(2);
        BatchExecutionContext.Depth.ShouldBe(1); // Parent unaffected
    }

}
