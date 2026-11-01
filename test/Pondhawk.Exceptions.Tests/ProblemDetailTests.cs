// Copyright (c) Pond Hawk Technologies Inc. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using Pondhawk.Exceptions;
using Shouldly;
using Xunit;

namespace Pondhawk.Exceptions.Tests;

public class ProblemDetailTests
{

    [Fact]
    public void DefaultProperties_AreEmptyStrings_StatusCodeZero()
    {
        var pd = new ProblemDetail();

        pd.Type.ShouldBe(string.Empty);
        pd.Title.ShouldBe(string.Empty);
        pd.StatusCode.ShouldBe(0);
        pd.Detail.ShouldBe(string.Empty);
        pd.Instance.ShouldBe(string.Empty);
        pd.CorrelationId.ShouldBe(string.Empty);
        pd.Segments.ShouldBeEmpty();
    }

    [Fact]
    public void Properties_AreSettable()
    {
        var pd = new ProblemDetail
        {
            Type = "https://example.com/error",
            Title = "Bad Request",
            StatusCode = 400,
            Detail = "Validation failed",
            Instance = "/api/orders/123",
            CorrelationId = "corr-456"
        };

        pd.Type.ShouldBe("https://example.com/error");
        pd.Title.ShouldBe("Bad Request");
        pd.StatusCode.ShouldBe(400);
        pd.Detail.ShouldBe("Validation failed");
        pd.Instance.ShouldBe("/api/orders/123");
        pd.CorrelationId.ShouldBe("corr-456");
    }

    [Fact]
    public void Segments_IsMutableList()
    {
        var pd = new ProblemDetail();

        pd.Segments.Add(EventDetail.Build().WithExplanation("seg1"));
        pd.Segments.Add(EventDetail.Build().WithExplanation("seg2"));

        pd.Segments.Count.ShouldBe(2);
        pd.Segments[0].Explanation.ShouldBe("seg1");
    }

}
