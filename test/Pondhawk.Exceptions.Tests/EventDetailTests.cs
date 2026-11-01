// Copyright (c) Pond Hawk Technologies Inc. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using Pondhawk.Exceptions;
using Shouldly;
using Xunit;
using static Pondhawk.Exceptions.EventDetail;

namespace Pondhawk.Exceptions.Tests;

public class EventDetailTests
{

    [Fact]
    public void Build_ReturnsNewInstance_WithDefaults()
    {
        var detail = EventDetail.Build();

        detail.ShouldNotBeNull();
        detail.Category.ShouldBe(EventCategory.Error);
        detail.RuleName.ShouldBe("");
        detail.Group.ShouldBe("");
        detail.Source.ShouldBe("");
        detail.Explanation.ShouldBe("");
    }

    [Fact]
    public void WithCategory_SetsCategory()
    {
        var detail = EventDetail.Build().WithCategory(EventCategory.Warning);

        detail.Category.ShouldBe(EventCategory.Warning);
    }

    [Fact]
    public void WithRuleName_SetsRuleName()
    {
        var detail = EventDetail.Build().WithRuleName("AgeCheck");

        detail.RuleName.ShouldBe("AgeCheck");
    }

    [Fact]
    public void WithRuleName_Null_Throws()
    {
        Should.Throw<ArgumentNullException>(() =>
            EventDetail.Build().WithRuleName(null));
    }

    [Fact]
    public void WithGroup_SetsGroup()
    {
        var detail = EventDetail.Build().WithGroup("Validation");

        detail.Group.ShouldBe("Validation");
    }

    [Fact]
    public void WithGroup_Null_Throws()
    {
        Should.Throw<ArgumentNullException>(() =>
            EventDetail.Build().WithGroup(null));
    }

    [Fact]
    public void WithSource_SetsSourceFromObjectToString()
    {
        var detail = EventDetail.Build().WithSource(42);

        detail.Source.ShouldBe("42");
    }

    [Fact]
    public void WithSource_Null_Throws()
    {
        Should.Throw<ArgumentNullException>(() =>
            EventDetail.Build().WithSource(null));
    }

    [Fact]
    public void WithExplanation_SetsExplanation()
    {
        var detail = EventDetail.Build().WithExplanation("Name is required");

        detail.Explanation.ShouldBe("Name is required");
    }

    [Fact]
    public void WithExplanation_Null_Throws()
    {
        Should.Throw<ArgumentNullException>(() =>
            EventDetail.Build().WithExplanation(null));
    }

    [Fact]
    public void FluentChain_ReturnsSameInstance()
    {
        var detail = EventDetail.Build();

        var result = detail
            .WithCategory(EventCategory.Info)
            .WithRuleName("Rule1")
            .WithGroup("Group1")
            .WithSource("Source1")
            .WithExplanation("Explanation1");

        result.ShouldBeSameAs(detail);
    }

    [Fact]
    public void Comparer_SameFields_AreEqual()
    {
        var a = EventDetail.Build()
            .WithCategory(EventCategory.Error)
            .WithRuleName("Rule1")
            .WithGroup("Group1")
            .WithExplanation("Explain");

        var b = EventDetail.Build()
            .WithCategory(EventCategory.Error)
            .WithRuleName("Rule1")
            .WithGroup("Group1")
            .WithExplanation("Explain");

        var comparer = new EventDetail.Comparer();
        comparer.Equals(a, b).ShouldBeTrue();
    }

    [Fact]
    public void Comparer_DifferentCategory_NotEqual()
    {
        var a = EventDetail.Build().WithCategory(EventCategory.Error).WithExplanation("x");
        var b = EventDetail.Build().WithCategory(EventCategory.Warning).WithExplanation("x");

        var comparer = new EventDetail.Comparer();
        comparer.Equals(a, b).ShouldBeFalse();
    }

    [Fact]
    public void Comparer_DifferentExplanation_NotEqual()
    {
        var a = EventDetail.Build().WithExplanation("a");
        var b = EventDetail.Build().WithExplanation("b");

        var comparer = new EventDetail.Comparer();
        comparer.Equals(a, b).ShouldBeFalse();
    }

    [Fact]
    public void Comparer_IgnoresSource()
    {
        var a = EventDetail.Build().WithSource("SourceA").WithExplanation("x");
        var b = EventDetail.Build().WithSource("SourceB").WithExplanation("x");

        var comparer = new EventDetail.Comparer();
        comparer.Equals(a, b).ShouldBeTrue();
    }

    [Fact]
    public void Comparer_NullX_ReturnsFalse()
    {
        var b = EventDetail.Build();

        var comparer = new EventDetail.Comparer();
        comparer.Equals(null, b).ShouldBeFalse();
    }

    [Fact]
    public void Comparer_NullY_ReturnsFalse()
    {
        var a = EventDetail.Build();

        var comparer = new EventDetail.Comparer();
        comparer.Equals(a, null).ShouldBeFalse();
    }

    [Fact]
    public void Comparer_GetHashCode_EqualObjects_SameHash()
    {
        var a = EventDetail.Build()
            .WithCategory(EventCategory.Violation)
            .WithRuleName("Rule1")
            .WithGroup("Group1")
            .WithExplanation("msg");

        var b = EventDetail.Build()
            .WithCategory(EventCategory.Violation)
            .WithRuleName("Rule1")
            .WithGroup("Group1")
            .WithExplanation("msg");

        var comparer = new EventDetail.Comparer();
        comparer.GetHashCode(a).ShouldBe(comparer.GetHashCode(b));
    }

    [Fact]
    public void DeDup_RemovesDuplicates()
    {
        var d1 = EventDetail.Build().WithRuleName("R1").WithExplanation("E1");
        var d2 = EventDetail.Build().WithRuleName("R1").WithExplanation("E1");
        var d3 = EventDetail.Build().WithRuleName("R2").WithExplanation("E2");

        var result = EventDetail.DeDup([d1, d2, d3]).ToList();

        result.Count.ShouldBe(2);
    }

    [Fact]
    public void Merge_TwoSets_DeduplicatesOverlap()
    {
        var shared = EventDetail.Build().WithRuleName("R1").WithExplanation("E1");
        var unique1 = EventDetail.Build().WithRuleName("R2").WithExplanation("E2");
        var unique2 = EventDetail.Build().WithRuleName("R3").WithExplanation("E3");
        var sharedDup = EventDetail.Build().WithRuleName("R1").WithExplanation("E1");

        var set1 = new List<EventDetail> { shared, unique1 };
        var set2 = new List<EventDetail> { sharedDup, unique2 };

        var result = EventDetail.Merge(set1, set2).ToList();

        result.Count.ShouldBe(3);
    }

}
