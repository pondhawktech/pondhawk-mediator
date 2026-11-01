// Copyright (c) Pond Hawk Technologies Inc. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using Pondhawk.Exceptions;
using Pondhawk.Mediator;
using Shouldly;
using Xunit;

namespace Pondhawk.Mediator.Tests;

public class ResponseTests
{

    [Fact]
    public void ImplicitConversion_FromValue_ProducesSuccess()
    {
        Response<int> response = 42;

        response.Ok.ShouldBeTrue();
        response.Value.ShouldBe(42);
        response.Error.ShouldBeNull();
    }

    [Fact]
    public void ImplicitConversion_IsEquivalentToSuccess()
    {
        Response<string> implicitly = "hello";
        var explicitly = Response<string>.Success("hello");

        implicitly.ShouldBe(explicitly);
    }

    [Fact]
    public void Failure_SetsErrorAndNotOk()
    {
        var error = new ErrorInfo { Kind = ErrorKind.NotFound, ErrorCode = "NotFound", Explanation = "gone" };

        var response = Response<string>.Failure(error);

        response.Ok.ShouldBeFalse();
        response.Value.ShouldBeNull();
        response.Error.ShouldBeSameAs(error);
    }

    [Fact]
    public void Match_RoutesByOutcome()
    {
        Response<int> ok = 7;
        var failed = Response<int>.Failure(new ErrorInfo { Kind = ErrorKind.System, ErrorCode = "System", Explanation = "boom" });

        ok.Match(v => $"ok:{v}", e => $"err:{e.Kind}").ShouldBe("ok:7");
        failed.Match(v => $"ok:{v}", e => $"err:{e.Kind}").ShouldBe("err:System");
    }

    [Fact]
    public void GetValueOrThrow_OnFailure_Throws()
    {
        var failed = Response<int>.Failure(new ErrorInfo { Kind = ErrorKind.Conflict, ErrorCode = "Conflict", Explanation = "nope" });

        Should.Throw<InvalidOperationException>(() => failed.GetValueOrThrow());
    }

    [Fact]
    public void Receipt_One_AffectsOne()
    {
        Receipt.One.Affected.ShouldBe(1);
    }

    [Fact]
    public void Receipt_Of_CarriesTally()
    {
        Receipt.Of(5).Affected.ShouldBe(5);
    }

    [Fact]
    public void IsError_IsInverseOfOk()
    {
        Response<int> ok = 1;
        var failed = Response<int>.Failure(new ErrorInfo { Kind = ErrorKind.System, ErrorCode = "System", Explanation = "boom" });

        ok.IsError.ShouldBe(!ok.Ok);
        ok.IsError.ShouldBeFalse();
        failed.IsError.ShouldBe(!failed.Ok);
        failed.IsError.ShouldBeTrue();
    }

    [Fact]
    public void AsEntity_OnSuccess_ReturnsValue()
    {
        Response<int> ok = 42;

        ok.AsEntity.ShouldBe(42);
    }

    [Fact]
    public void AsEntity_OnFailure_Throws()
    {
        var failed = Response<int>.Failure(new ErrorInfo { Kind = ErrorKind.NotFound, ErrorCode = "NotFound", Explanation = "gone" });

        Should.Throw<InvalidOperationException>(() => failed.AsEntity);
    }

    [Fact]
    public void AsError_OnFailure_ReturnsErrorInfo()
    {
        var error = new ErrorInfo { Kind = ErrorKind.Conflict, ErrorCode = "Conflict", Explanation = "nope" };
        var failed = Response<int>.Failure(error);

        failed.AsError.ShouldBeSameAs(error);
    }

    [Fact]
    public void AsError_OnSuccess_Throws()
    {
        Response<int> ok = 1;

        Should.Throw<InvalidOperationException>(() => ok.AsError);
    }

    [Fact]
    public void ImplicitConversion_FromErrorInfo_ProducesFailure()
    {
        var error = new ErrorInfo { Kind = ErrorKind.NotFound, ErrorCode = "NotFound", Explanation = "gone" };

        Response<string> response = error;

        response.Ok.ShouldBeFalse();
        response.IsError.ShouldBeTrue();
        response.Error.ShouldBeSameAs(error);
    }

    [Fact]
    public void AsError_PropagatesAcrossPayloadTypes()
    {
        var error = new ErrorInfo { Kind = ErrorKind.NotFound, ErrorCode = "NotFound", Explanation = "gone" };
        var failed = Response<int>.Failure(error);

        static Response<string> Propagate(Response<int> source) => source.AsError;

        var propagated = Propagate(failed);

        propagated.IsError.ShouldBeTrue();
        propagated.Error.ShouldBeSameAs(error);
    }

    [Fact]
    public void Default_IsAFailureNotASuccess()
    {
        // A Response<T> is a value type; default (uninitialized field, array slot, dictionary miss)
        // must read as a coherent failure, never as a success carrying a null value.
        Response<string> uninitialized = default;

        uninitialized.Ok.ShouldBeFalse();
        uninitialized.IsError.ShouldBeTrue();
    }

    [Fact]
    public void Default_ErrorIsNonNullSystemFailure()
    {
        // The invariant "Error non-null iff not Ok" must hold on read even for default.
        Response<string> uninitialized = default;

        uninitialized.Error.ShouldNotBeNull();
        uninitialized.Error!.Kind.ShouldBe(ErrorKind.System);
    }

    [Fact]
    public void Default_Match_RoutesToFailureWithoutNullReference()
    {
        Response<int> uninitialized = default;

        // onFailure must receive a non-null ErrorInfo — no NRE from a null Error.
        var result = uninitialized.Match(v => $"ok:{v}", e => $"err:{e.Kind}");

        result.ShouldBe("err:System");
    }

    [Fact]
    public void Default_AsError_ReturnsSystemFailure()
    {
        Response<int> uninitialized = default;

        // AsError must not throw "not an error" while IsError is true.
        uninitialized.AsError.Kind.ShouldBe(ErrorKind.System);
    }

    [Fact]
    public void Default_GetValueOrThrow_Throws()
    {
        Response<int> uninitialized = default;

        Should.Throw<InvalidOperationException>(() => uninitialized.GetValueOrThrow());
    }

    [Fact]
    public void Default_AsEntity_Throws()
    {
        Response<int> uninitialized = default;

        Should.Throw<InvalidOperationException>(() => uninitialized.AsEntity);
    }

    [Fact]
    public void Failure_NullError_Throws()
    {
        Should.Throw<ArgumentNullException>(() => Response<int>.Failure(null!));
    }

    [Fact]
    public void Match_NullOnSuccess_Throws()
    {
        Response<string> ok = "x";

        Should.Throw<ArgumentNullException>(() => ok.Match(null!, e => e.Explanation));
    }

    [Fact]
    public void Match_NullOnFailure_Throws()
    {
        Response<string> ok = "x";

        Should.Throw<ArgumentNullException>(() => ok.Match(v => v, null!));
    }

    // ── IResponse (non-generic view) ──

    [Fact]
    public void IResponse_Success_ExposesBoxedValue()
    {
        IResponse r = Response<int>.Success(42);

        r.Ok.ShouldBeTrue();
        r.Error.ShouldBeNull();
        r.Value.ShouldBe(42);
    }

    [Fact]
    public void IResponse_Failure_ExposesError_AndNullValue()
    {
        var error = new ErrorInfo { Kind = ErrorKind.NotFound, ErrorCode = "NotFound", Explanation = "gone" };

        IResponse r = Response<string>.Failure(error);

        r.Ok.ShouldBeFalse();
        r.Error.ShouldBeSameAs(error);
        r.Value.ShouldBeNull();
    }

    [Fact]
    public void IResponse_Default_ReadsAsSystemFailure()
    {
        IResponse r = default(Response<int>);

        r.Ok.ShouldBeFalse();
        r.Error!.Kind.ShouldBe(ErrorKind.System);
    }

}
