// Copyright (c) Pond Hawk Technologies Inc. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using Pondhawk.Exceptions;
using Pondhawk.Mediator;
using Shouldly;
using Xunit;

namespace Pondhawk.Mediator.Tests;

public class BatchCommandResultTests
{

    // ── Test doubles ──

    public class OrderResponse
    {
        public int OrderId { get; init; }
    }

    public class InvoiceResponse
    {
        public string InvoiceNumber { get; init; }
    }

    private static ErrorInfo Error(string explanation, ErrorKind kind = ErrorKind.Functional)
        => new() { Kind = kind, ErrorCode = kind.ToString(), Explanation = explanation };

    // ── Succeeded ──

    [Fact]
    public void Succeeded_SetsSuccessTrue()
    {
        var result = BatchCommandResult.Succeeded("CreateOrder", new OrderResponse { OrderId = 1 });

        result.Success.ShouldBeTrue();
    }

    [Fact]
    public void Succeeded_StoresResponse()
    {
        var response = new OrderResponse { OrderId = 42 };

        var result = BatchCommandResult.Succeeded("CreateOrder", response);

        result.Response.ShouldBeSameAs(response);
    }

    [Fact]
    public void Succeeded_SetsCommandType()
    {
        var result = BatchCommandResult.Succeeded("DeleteOrder", new Receipt());

        result.CommandType.ShouldBe("DeleteOrder");
    }

    [Fact]
    public void Succeeded_NullCommandType_Throws()
    {
        Should.Throw<ArgumentNullException>(
            () => BatchCommandResult.Succeeded(null, new OrderResponse()));
    }

    [Fact]
    public void Succeeded_EmptyCommandType_Throws()
    {
        Should.Throw<ArgumentException>(
            () => BatchCommandResult.Succeeded("", new OrderResponse()));
    }

    [Fact]
    public void Succeeded_WithEntityUid_SetsEntityUid()
    {
        var result = BatchCommandResult.Succeeded("CreateOrder", new OrderResponse(), "order-123");

        result.EntityUid.ShouldBe("order-123");
    }

    [Fact]
    public void Succeeded_WithoutEntityUid_EntityUidIsNull()
    {
        var result = BatchCommandResult.Succeeded("CreateOrder", new OrderResponse());

        result.EntityUid.ShouldBeNull();
    }

    [Fact]
    public void Succeeded_ErrorMessageIsNull()
    {
        var result = BatchCommandResult.Succeeded("CreateOrder", new OrderResponse());

        result.ErrorMessage.ShouldBeNull();
    }

    // ── Failed ──

    [Fact]
    public void Failed_SetsSuccessFalse()
    {
        var result = BatchCommandResult.Failed("CreateOrder", "order-1", Error("Validation failed", ErrorKind.Predicate));

        result.Success.ShouldBeFalse();
    }

    [Fact]
    public void Failed_SetsCommandType()
    {
        var result = BatchCommandResult.Failed("CreateOrder", "order-1", Error("Error"));

        result.CommandType.ShouldBe("CreateOrder");
    }

    [Fact]
    public void Failed_SetsEntityUid()
    {
        var result = BatchCommandResult.Failed("CreateOrder", "order-1", Error("Error"));

        result.EntityUid.ShouldBe("order-1");
    }

    [Fact]
    public void Failed_NullEntityUid_Allowed()
    {
        var result = BatchCommandResult.Failed("CreateOrder", null, Error("Error"));

        result.EntityUid.ShouldBeNull();
    }

    [Fact]
    public void Failed_SetsError()
    {
        var error = Error("Validation failed", ErrorKind.Predicate);

        var result = BatchCommandResult.Failed("CreateOrder", null, error);

        result.Error.ShouldBeSameAs(error);
    }

    [Fact]
    public void Failed_DerivesErrorMessageFromExplanation()
    {
        var result = BatchCommandResult.Failed("CreateOrder", null, Error("Something broke"));

        result.ErrorMessage.ShouldBe("Something broke");
    }

    [Fact]
    public void Failed_PreservesKind()
    {
        var result = BatchCommandResult.Failed("CreateOrder", null, Error("Gone", ErrorKind.NotFound));

        result.Error!.Kind.ShouldBe(ErrorKind.NotFound);
    }

    [Fact]
    public void Failed_ResponseIsNull()
    {
        var result = BatchCommandResult.Failed("CreateOrder", null, Error("Error"));

        result.Response.ShouldBeNull();
    }

    [Fact]
    public void Failed_NullCommandType_Throws()
    {
        Should.Throw<ArgumentNullException>(
            () => BatchCommandResult.Failed(null, null, Error("Error")));
    }

    [Fact]
    public void Failed_EmptyCommandType_Throws()
    {
        Should.Throw<ArgumentException>(
            () => BatchCommandResult.Failed("", null, Error("Error")));
    }

    [Fact]
    public void Failed_NullError_Throws()
    {
        Should.Throw<ArgumentNullException>(
            () => BatchCommandResult.Failed("CreateOrder", null, null!));
    }

    // ── GetResponse ──

    [Fact]
    public void GetResponse_CorrectType_ReturnsTypedResponse()
    {
        var response = new OrderResponse { OrderId = 42 };
        var result = BatchCommandResult.Succeeded("CreateOrder", response);

        var typed = result.GetResponse<OrderResponse>();

        typed.ShouldNotBeNull();
        typed.OrderId.ShouldBe(42);
    }

    [Fact]
    public void GetResponse_WrongType_ReturnsNull()
    {
        var result = BatchCommandResult.Succeeded("CreateOrder", new OrderResponse());

        var typed = result.GetResponse<InvoiceResponse>();

        typed.ShouldBeNull();
    }

    [Fact]
    public void GetResponse_NullResponse_ReturnsNull()
    {
        var result = BatchCommandResult.Failed("Test", null, Error("Error"));

        var typed = result.GetResponse<OrderResponse>();

        typed.ShouldBeNull();
    }

    // ── Batch ──

    [Fact]
    public void Batch_MixedSuccessFailure_PreservesEachKind()
    {
        var results = new[]
        {
            BatchCommandResult.Succeeded("CreateOrder", new OrderResponse { OrderId = 1 }, "order-1"),
            BatchCommandResult.Failed("CreateOrder", "order-2", Error("Gone", ErrorKind.NotFound)),
            BatchCommandResult.Failed("CreateOrder", "order-3", Error("State conflict", ErrorKind.Conflict)),
        };

        results[0].Success.ShouldBeTrue();
        results[0].Error.ShouldBeNull();
        results[1].Error!.Kind.ShouldBe(ErrorKind.NotFound);
        results[2].Error!.Kind.ShouldBe(ErrorKind.Conflict);
    }

    // ── Record equality ──

    [Fact]
    public void RecordEquality_SameValues_AreEqual()
    {
        var a = new BatchCommandResult { Success = true, CommandType = "Test", EntityUid = "1" };
        var b = new BatchCommandResult { Success = true, CommandType = "Test", EntityUid = "1" };

        a.ShouldBe(b);
    }

    [Fact]
    public void RecordEquality_DifferentValues_AreNotEqual()
    {
        var a = new BatchCommandResult { Success = true, CommandType = "Test" };
        var b = new BatchCommandResult { Success = false, CommandType = "Test" };

        a.ShouldNotBe(b);
    }

}
