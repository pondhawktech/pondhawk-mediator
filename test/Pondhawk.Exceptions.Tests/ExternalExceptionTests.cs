// Copyright (c) Pond Hawk Technologies Inc. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using Pondhawk.Exceptions;
using Shouldly;
using Xunit;

namespace Pondhawk.Exceptions.Tests;

public class ExternalExceptionTests
{

    // Concrete test subclass since ExternalException is abstract
    private class TestException : ExternalException
    {
        public TestException(string message) : base(message) { }
        public TestException(string message, Exception inner) : base(message, inner) { }
        public TestException(IExceptionInfo info) : base(info) { }
    }

    private class TestExceptionInfo : IExceptionInfo
    {
        public ErrorKind Kind { get; set; }
        public string ErrorCode { get; set; } = "";
        public string Explanation { get; set; } = "";
        public IList<EventDetail> Details { get; set; } = new List<EventDetail>();
    }

    // ── Constructor with message ──

    [Fact]
    public void Constructor_SetsExplanationFromMessage()
    {
        var ex = new TestException("Something failed");

        ex.Explanation.ShouldBe("Something failed");
    }

    [Fact]
    public void Constructor_DeriveErrorCodeFromTypeName()
    {
        var ex = new TestException("test");

        ex.ErrorCode.ShouldBe("Test");
    }

    [Fact]
    public void Constructor_DefaultKindIsSystem()
    {
        var ex = new TestException("test");

        ex.Kind.ShouldBe(ErrorKind.System);
    }

    // ── Constructor with IExceptionInfo ──

    [Fact]
    public void Constructor_FromExceptionInfo_SetsKind()
    {
        var info = new TestExceptionInfo
        {
            Kind = ErrorKind.BadRequest,
            Explanation = "Bad request"
        };

        var ex = new TestException(info);

        ex.Kind.ShouldBe(ErrorKind.BadRequest);
    }

    [Fact]
    public void Constructor_FromExceptionInfo_SetsExplanation()
    {
        var info = new TestExceptionInfo { Explanation = "Detailed error" };

        var ex = new TestException(info);

        ex.Explanation.ShouldBe("Detailed error");
        ex.InnerExplanation.ShouldBe("Detailed error");
    }

    [Fact]
    public void Constructor_FromExceptionInfo_CopiesDetails()
    {
        var info = new TestExceptionInfo
        {
            Explanation = "error",
            Details = new List<EventDetail>
            {
                EventDetail.Build().WithExplanation("detail1"),
                EventDetail.Build().WithExplanation("detail2")
            }
        };

        var ex = new TestException(info);

        ex.Details.Count.ShouldBe(2);
        ex.Details[0].Explanation.ShouldBe("detail1");
    }

    [Fact]
    public void Constructor_FromExceptionInfo_SetsMessageFromExplanation()
    {
        var info = new TestExceptionInfo { Explanation = "the message" };

        var ex = new TestException(info);

        ex.Message.ShouldBe("the message");
    }

    // ── FluentException With(IExceptionInfo) ──

    [Fact]
    public void FluentException_With_PopulatesFromInfo()
    {
        var info = new TestExceptionInfo
        {
            Kind = ErrorKind.NotFound,
            ErrorCode = "CUSTOM",
            Explanation = "not found",
            Details = new List<EventDetail>
            {
                EventDetail.Build().WithExplanation("missing")
            }
        };

        var ex = new FunctionalException("initial").With(info);

        ex.Kind.ShouldBe(ErrorKind.NotFound);
        ex.Explanation.ShouldBe("not found");
        ex.Details.Count.ShouldBe(1);
    }

    [Fact]
    public void FluentException_With_Null_Throws()
    {
        var ex = new FunctionalException("test");

        Should.Throw<ArgumentNullException>(() => ex.With(null));
    }

}
