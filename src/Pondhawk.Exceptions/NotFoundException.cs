// Copyright (c) Pond Hawk Technologies Inc. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

namespace Pondhawk.Exceptions;

/// <summary>
/// Exception thrown when a requested resource could not be found. Fixes
/// <see cref="ErrorKind.NotFound"/>. This is sugar over <c>WithKind</c>; mapping is always
/// keyed on <see cref="ExternalException.Kind"/>, never on the concrete type.
/// </summary>
public sealed class NotFoundException : FluentException<NotFoundException>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="NotFoundException"/> class for the
    /// given resource and key.
    /// </summary>
    /// <param name="resource">The kind of resource that was being looked up (e.g. "Order").</param>
    /// <param name="key">The key that was not found.</param>
    public NotFoundException(string resource, object key)
        : base($"{resource} '{key}' was not found.")
    {
        WithKind(ErrorKind.NotFound);
        WithErrorCode("NotFound");
    }
}
