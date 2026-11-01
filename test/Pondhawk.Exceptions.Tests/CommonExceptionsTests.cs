// Copyright (c) Pond Hawk Technologies Inc. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using Pondhawk.Exceptions;
using Shouldly;
using Xunit;

namespace Pondhawk.Exceptions.Tests;

public class CommonExceptionsTests
{

    [Fact]
    public void NotFoundException_SetsKindAndCode()
    {
        var ex = new NotFoundException("Order", 42);

        ex.Kind.ShouldBe(ErrorKind.NotFound);
        ex.ErrorCode.ShouldBe("NotFound");
        ex.Message.ShouldContain("Order");
        ex.Message.ShouldContain("42");
    }

    [Fact]
    public void ConflictException_SetsKindAndCode()
    {
        var ex = new ConflictException("Already exists");

        ex.Kind.ShouldBe(ErrorKind.Conflict);
        ex.ErrorCode.ShouldBe("Conflict");
        ex.Message.ShouldBe("Already exists");
    }

    [Fact]
    public void NotAuthorizedException_SetsKindAndCode()
    {
        var ex = new NotAuthorizedException("No access");

        ex.Kind.ShouldBe(ErrorKind.NotAuthorized);
        ex.ErrorCode.ShouldBe("NotAuthorized");
        ex.Message.ShouldBe("No access");
    }

    [Fact]
    public void ErrorInfo_From_CopiesKindCodeExplanationAndDetails()
    {
        var ex = new NotFoundException("Order", 7);

        var info = ErrorInfo.From(ex);

        info.Kind.ShouldBe(ErrorKind.NotFound);
        info.ErrorCode.ShouldBe("NotFound");
        info.Explanation.ShouldBe(ex.Explanation);
        info.Details.Count.ShouldBe(ex.Details.Count);
    }

    [Fact]
    public void ErrorInfo_From_CarriesValidationViolations()
    {
        var ex = new FailedValidationException(
        [
            EventDetail.Build().WithRuleName("R1").WithExplanation("bad"),
        ]);

        var info = ErrorInfo.From(ex);

        info.Kind.ShouldBe(ErrorKind.Predicate);
        info.Details.ShouldHaveSingleItem();
        info.Details[0].Explanation.ShouldBe("bad");
    }

    [Fact]
    public void ErrorInfo_System_UsesSystemKindAndMessage()
    {
        var info = ErrorInfo.System(new InvalidOperationException("boom"));

        info.Kind.ShouldBe(ErrorKind.System);
        info.ErrorCode.ShouldBe("System");
        info.Explanation.ShouldBe("boom");
    }

}
