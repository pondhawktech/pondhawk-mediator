// Copyright (c) Pond Hawk Technologies Inc. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using Pondhawk.Exceptions;
using Shouldly;
using Xunit;

namespace Pondhawk.Exceptions.Tests;

public class FluentExceptionTests
{

    [Fact]
    public void Constructor_SetsMessageKindAndErrorCode()
    {
        var ex = new FunctionalException("Something failed");

        ex.Kind.ShouldBe(ErrorKind.Functional);
        ex.ErrorCode.ShouldBe("Functional");
    }

    [Fact]
    public void Constructor_SetsExplanationFromMessage()
    {
        var ex = new FunctionalException("Something failed");

        ex.Explanation.ShouldBe("Something failed");
    }

    [Fact]
    public void Constructor_WithInner_SetsInnerException()
    {
        var inner = new InvalidOperationException("root cause");
        var ex = new FunctionalException("wrapper", inner);

        ex.InnerException.ShouldBeSameAs(inner);
    }

    [Fact]
    public void Constructor_WithInternalExceptionInner_CopiesDetailsAndInnerExplanation()
    {
        var internalEx = new InternalException("internal issue");
        internalEx.Details.Add(EventDetail.Build().WithExplanation("detail1"));

        var ex = new FunctionalException("wrapper", internalEx);

        ex.InnerExplanation.ShouldBe("internal issue");
        ex.Details.Count.ShouldBe(1);
        ex.Details[0].Explanation.ShouldBe("detail1");
    }

    [Fact]
    public void Constructor_WithNonInternalInner_DoesNotCopyDetails()
    {
        var inner = new InvalidOperationException("plain");

        var ex = new FunctionalException("wrapper", inner);

        ex.InnerExplanation.ShouldBe("");
        ex.Details.ShouldBeEmpty();
    }

    [Fact]
    public void WithKind_SetsKind_ReturnsSelf()
    {
        var ex = new FunctionalException("test");

        var result = ex.WithKind(ErrorKind.BadRequest);

        result.ShouldBeSameAs(ex);
        result.Kind.ShouldBe(ErrorKind.BadRequest);
    }

    [Fact]
    public void WithErrorCode_SetsErrorCode()
    {
        var ex = new FunctionalException("test");

        ex.WithErrorCode("CUSTOM_CODE");

        ex.ErrorCode.ShouldBe("CUSTOM_CODE");
    }

    [Fact]
    public void WithErrorCode_Null_Throws()
    {
        var ex = new FunctionalException("test");

        Should.Throw<ArgumentNullException>(() => ex.WithErrorCode(null));
    }

    [Fact]
    public void WithExplanation_SetsExplanation()
    {
        var ex = new FunctionalException("test");

        ex.WithExplanation("detailed explanation");

        ex.Explanation.ShouldBe("detailed explanation");
    }

    [Fact]
    public void WithExplanation_Null_Throws()
    {
        var ex = new FunctionalException("test");

        Should.Throw<ArgumentNullException>(() => ex.WithExplanation(null));
    }

    [Fact]
    public void WithCorrelationId_SetsCorrelationId()
    {
        var ex = new FunctionalException("test");

        ex.WithCorrelationId("corr-123");

        ex.CorrelationId.ShouldBe("corr-123");
    }

    [Fact]
    public void WithCorrelationId_Null_Throws()
    {
        var ex = new FunctionalException("test");

        Should.Throw<ArgumentNullException>(() => ex.WithCorrelationId(null));
    }

    [Fact]
    public void WithDetail_AddsToDetailsList()
    {
        var ex = new FunctionalException("test");
        var detail = EventDetail.Build().WithExplanation("violation");

        ex.WithDetail(detail);

        ex.Details.Count.ShouldBe(1);
        ex.Details[0].Explanation.ShouldBe("violation");
    }

    [Fact]
    public void WithDetail_Null_Throws()
    {
        var ex = new FunctionalException("test");

        Should.Throw<ArgumentNullException>(() => ex.WithDetail(null));
    }

    [Fact]
    public void WithDetails_AddsMultiple()
    {
        var ex = new FunctionalException("test");
        var details = new List<EventDetail>
        {
            EventDetail.Build().WithExplanation("v1"),
            EventDetail.Build().WithExplanation("v2")
        };

        ex.WithDetails(details);

        ex.Details.Count.ShouldBe(2);
    }

    [Fact]
    public void FailedValidation_SetsPredicateKind_CopiesViolations()
    {
        var violations = new List<EventDetail>
        {
            EventDetail.Build().WithExplanation("Name required"),
            EventDetail.Build().WithExplanation("Age invalid")
        };

        var ex = new FailedValidationException(violations);

        ex.Kind.ShouldBe(ErrorKind.Predicate);
        ex.Details.Count.ShouldBe(2);
        ex.Message.ShouldBe("Violation events occurred during validation.");
    }

}
