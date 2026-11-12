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

    // ── Success ──

    [Fact]
    public void Success_SetsOkTrue()
    {
        var result = BatchCommandResult.Success("CreateOrder", new OrderResponse { OrderId = 1 });

        result.Ok.ShouldBeTrue();
    }

    [Fact]
    public void Success_StoresResponse()
    {
        var response = new OrderResponse { OrderId = 42 };

        var result = BatchCommandResult.Success("CreateOrder", response);

        result.Response.ShouldBeSameAs(response);
    }

    [Fact]
    public void Success_SetsCommandType()
    {
        var result = BatchCommandResult.Success("DeleteOrder", new Receipt());

        result.CommandType.ShouldBe("DeleteOrder");
    }

    [Fact]
    public void Success_NullCommandType_Throws()
    {
        Should.Throw<ArgumentNullException>(
            () => BatchCommandResult.Success(null, new OrderResponse()));
    }

    [Fact]
    public void Success_EmptyCommandType_Throws()
    {
        Should.Throw<ArgumentException>(
            () => BatchCommandResult.Success("", new OrderResponse()));
    }

    [Fact]
    public void Success_WithEntityUid_SetsEntityUid()
    {
        var result = BatchCommandResult.Success("CreateOrder", new OrderResponse(), "order-123");

        result.EntityUid.ShouldBe("order-123");
    }

    [Fact]
    public void Success_WithoutEntityUid_EntityUidIsNull()
    {
        var result = BatchCommandResult.Success("CreateOrder", new OrderResponse());

        result.EntityUid.ShouldBeNull();
    }

    [Fact]
    public void Success_ErrorMessageIsNull()
    {
        var result = BatchCommandResult.Success("CreateOrder", new OrderResponse());

        result.ErrorMessage.ShouldBeNull();
    }

    // ── Failure ──

    [Fact]
    public void Failure_SetsOkFalse()
    {
        var result = BatchCommandResult.Failure("CreateOrder", "order-1", Error("Validation failed", ErrorKind.Predicate));

        result.Ok.ShouldBeFalse();
    }

    [Fact]
    public void Failure_SetsCommandType()
    {
        var result = BatchCommandResult.Failure("CreateOrder", "order-1", Error("Error"));

        result.CommandType.ShouldBe("CreateOrder");
    }

    [Fact]
    public void Failure_SetsEntityUid()
    {
        var result = BatchCommandResult.Failure("CreateOrder", "order-1", Error("Error"));

        result.EntityUid.ShouldBe("order-1");
    }

    [Fact]
    public void Failure_NullEntityUid_Allowed()
    {
        var result = BatchCommandResult.Failure("CreateOrder", null, Error("Error"));

        result.EntityUid.ShouldBeNull();
    }

    [Fact]
    public void Failure_SetsError()
    {
        var error = Error("Validation failed", ErrorKind.Predicate);

        var result = BatchCommandResult.Failure("CreateOrder", null, error);

        result.Error.ShouldBeSameAs(error);
    }

    [Fact]
    public void Failure_DerivesErrorMessageFromExplanation()
    {
        var result = BatchCommandResult.Failure("CreateOrder", null, Error("Something broke"));

        result.ErrorMessage.ShouldBe("Something broke");
    }

    [Fact]
    public void Failure_PreservesKind()
    {
        var result = BatchCommandResult.Failure("CreateOrder", null, Error("Gone", ErrorKind.NotFound));

        result.Error!.Kind.ShouldBe(ErrorKind.NotFound);
    }

    [Fact]
    public void Failure_ResponseIsNull()
    {
        var result = BatchCommandResult.Failure("CreateOrder", null, Error("Error"));

        result.Response.ShouldBeNull();
    }

    [Fact]
    public void Failure_NullCommandType_Throws()
    {
        Should.Throw<ArgumentNullException>(
            () => BatchCommandResult.Failure(null, null, Error("Error")));
    }

    [Fact]
    public void Failure_EmptyCommandType_Throws()
    {
        Should.Throw<ArgumentException>(
            () => BatchCommandResult.Failure("", null, Error("Error")));
    }

    [Fact]
    public void Failure_NullError_Throws()
    {
        Should.Throw<ArgumentNullException>(
            () => BatchCommandResult.Failure("CreateOrder", null, null!));
    }

    // ── GetResponse ──

    [Fact]
    public void GetResponse_CorrectType_ReturnsTypedResponse()
    {
        var response = new OrderResponse { OrderId = 42 };
        var result = BatchCommandResult.Success("CreateOrder", response);

        var typed = result.GetResponse<OrderResponse>();

        typed.ShouldNotBeNull();
        typed.OrderId.ShouldBe(42);
    }

    [Fact]
    public void GetResponse_WrongType_ReturnsNull()
    {
        var result = BatchCommandResult.Success("CreateOrder", new OrderResponse());

        var typed = result.GetResponse<InvoiceResponse>();

        typed.ShouldBeNull();
    }

    [Fact]
    public void GetResponse_NullResponse_ReturnsNull()
    {
        var result = BatchCommandResult.Failure("Test", null, Error("Error"));

        var typed = result.GetResponse<OrderResponse>();

        typed.ShouldBeNull();
    }

    // ── Batch ──

    [Fact]
    public void Batch_MixedSuccessFailure_PreservesEachKind()
    {
        var results = new[]
        {
            BatchCommandResult.Success("CreateOrder", new OrderResponse { OrderId = 1 }, "order-1"),
            BatchCommandResult.Failure("CreateOrder", "order-2", Error("Gone", ErrorKind.NotFound)),
            BatchCommandResult.Failure("CreateOrder", "order-3", Error("State conflict", ErrorKind.Conflict)),
        };

        results[0].Ok.ShouldBeTrue();
        results[0].Error.ShouldBeNull();
        results[1].Error!.Kind.ShouldBe(ErrorKind.NotFound);
        results[2].Error!.Kind.ShouldBe(ErrorKind.Conflict);
    }

    // ── Record equality ──

    [Fact]
    public void RecordEquality_SameValues_AreEqual()
    {
        var a = new BatchCommandResult { Ok = true, CommandType = "Test", EntityUid = "1" };
        var b = new BatchCommandResult { Ok = true, CommandType = "Test", EntityUid = "1" };

        a.ShouldBe(b);
    }

    [Fact]
    public void RecordEquality_DifferentValues_AreNotEqual()
    {
        var a = new BatchCommandResult { Ok = true, CommandType = "Test" };
        var b = new BatchCommandResult { Ok = false, CommandType = "Test" };

        a.ShouldNotBe(b);
    }

}
