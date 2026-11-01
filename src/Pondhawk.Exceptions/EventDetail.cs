// Copyright (c) Pond Hawk Technologies Inc. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.ComponentModel;
using System.Text.Json.Serialization;
using CommunityToolkit.Diagnostics;

// ReSharper disable UnusedMember.Global

namespace Pondhawk.Exceptions;

/// <summary>
/// A structured event detail with category, group, rule name, source, and explanation, used for validation and error reporting.
/// </summary>
public class EventDetail
{

    /// <summary>
    /// Categorizes event details by severity.
    /// </summary>
    public enum EventCategory
    {
        /// <summary>Informational event.</summary>
        Info,
        /// <summary>Warning event that may require attention.</summary>
        Warning,
        /// <summary>Violation event indicating a rule or validation failure.</summary>
        Violation,
        /// <summary>Error event indicating a fault.</summary>
        Error
    };

    /// <summary>
    /// Equality comparer for <see cref="EventDetail"/> based on category, rule name, group, and explanation.
    /// </summary>
    public class Comparer : IEqualityComparer<EventDetail>
    {

        /// <summary>
        /// Determines whether two <see cref="EventDetail"/> instances are equal by comparing category, rule name, group, and explanation.
        /// </summary>
        /// <param name="x">The first event detail to compare.</param>
        /// <param name="y">The second event detail to compare.</param>
        /// <returns><see langword="true"/> if the instances are considered equal; otherwise, <see langword="false"/>.</returns>
        public bool Equals(EventDetail? x, EventDetail? y)
        {

            if (x is null || y is null)
                return false;

            var eq = (x.Category == y.Category) && string.Equals(x.RuleName, y.RuleName, StringComparison.Ordinal) && string.Equals(x.Group, y.Group, StringComparison.Ordinal) && string.Equals(x.Explanation, y.Explanation, StringComparison.Ordinal);
            return eq;

        }

        /// <summary>
        /// Returns a hash code for the specified <see cref="EventDetail"/> based on its category, rule name, group, and explanation.
        /// </summary>
        /// <param name="obj">The event detail for which to get the hash code.</param>
        /// <returns>A hash code for the specified event detail.</returns>
        public int GetHashCode(EventDetail obj)
        {
            return HashCode.Combine(obj.Category, obj.RuleName, obj.Group, obj.Explanation);
        }

    }

    /// <summary>
    /// Removes duplicate event details from the given collection using the <see cref="Comparer"/>.
    /// </summary>
    /// <param name="source">The collection of event details to deduplicate.</param>
    /// <returns>A deduplicated set of event details.</returns>
    public static IEnumerable<EventDetail> DeDup(IEnumerable<EventDetail> source)
    {
        var set = new HashSet<EventDetail>(new Comparer());

        set.UnionWith(source);

        return set;

    }

    /// <summary>
    /// Merges two collections of event details into a single deduplicated set.
    /// </summary>
    /// <param name="source1">The first collection of event details.</param>
    /// <param name="source2">The second collection of event details.</param>
    /// <returns>A merged and deduplicated set of event details.</returns>
    public static IEnumerable<EventDetail> Merge(IEnumerable<EventDetail> source1, IEnumerable<EventDetail> source2)
    {

        var set = new HashSet<EventDetail>(new Comparer());
        set.UnionWith(source1);
        set.UnionWith(source2);

        return set;

    }



    /// <summary>
    /// Creates a new <see cref="EventDetail"/> instance with default values.
    /// </summary>
    /// <returns>A new <see cref="EventDetail"/> instance.</returns>
    public static EventDetail Build()
    {
        return new EventDetail();
    }

    /// <summary>
    /// Gets or sets the category of this event detail.
    /// </summary>
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public EventCategory Category { get; set; } = EventCategory.Error;

    /// <summary>
    /// Gets or sets the name of the rule that produced this event.
    /// </summary>
    [DefaultValue("")]
    public string RuleName { get; set; } = "";

    /// <summary>
    /// Gets or sets the logical group this event belongs to.
    /// </summary>
    [DefaultValue("")]
    public string Group { get; set; } = "";

    /// <summary>
    /// Gets or sets the source that originated this event.
    /// </summary>
    [DefaultValue("")]
    public string Source { get; set; } = "";

    /// <summary>
    /// Gets or sets the human-readable explanation for this event.
    /// </summary>
    [DefaultValue("")]
    public string Explanation { get; set; } = "";


    /// <summary>
    /// Sets the <see cref="Category"/> and returns this instance for fluent chaining.
    /// </summary>
    /// <param name="category">The event category to set.</param>
    /// <returns>This <see cref="EventDetail"/> instance.</returns>
    public EventDetail WithCategory(EventCategory category)
    {
        Category = category;
        return this;
    }

    /// <summary>
    /// Sets the <see cref="RuleName"/> and returns this instance for fluent chaining.
    /// </summary>
    /// <param name="ruleName">The rule name to set.</param>
    /// <returns>This <see cref="EventDetail"/> instance.</returns>
    public EventDetail WithRuleName(string ruleName)
    {
        Guard.IsNotNull(ruleName);
        RuleName = ruleName;
        return this;
    }

    /// <summary>
    /// Sets the <see cref="Group"/> and returns this instance for fluent chaining.
    /// </summary>
    /// <param name="group">The group name to set.</param>
    /// <returns>This <see cref="EventDetail"/> instance.</returns>
    public EventDetail WithGroup(string group)
    {
        Guard.IsNotNull(group);
        Group = group;
        return this;
    }

    /// <summary>
    /// Sets the <see cref="Source"/> from the object's string representation and returns this instance for fluent chaining.
    /// </summary>
    /// <param name="source">The source object whose string representation will be used.</param>
    /// <returns>This <see cref="EventDetail"/> instance.</returns>
    public EventDetail WithSource(object source)
    {
        Guard.IsNotNull(source);

        Source = source.ToString() ?? "";

        return this;
    }

    /// <summary>
    /// Sets the <see cref="Explanation"/> and returns this instance for fluent chaining.
    /// </summary>
    /// <param name="message">The explanation message to set.</param>
    /// <returns>This <see cref="EventDetail"/> instance.</returns>
    public EventDetail WithExplanation(string message)
    {
        Guard.IsNotNull(message);
        Explanation = message;
        return this;
    }


}
