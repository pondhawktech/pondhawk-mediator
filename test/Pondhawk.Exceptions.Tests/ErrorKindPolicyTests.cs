// Copyright (c) Pond Hawk Technologies Inc. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using Pondhawk.Exceptions;
using Shouldly;
using Xunit;

namespace Pondhawk.Exceptions.Tests;

public class ErrorKindPolicyTests
{

    [Theory]
    [InlineData(ErrorKind.System)]
    [InlineData(ErrorKind.Remote)]
    [InlineData(ErrorKind.Concurrency)]
    public void IsTransient_TransientKinds_ReturnTrue(ErrorKind kind)
    {
        kind.IsTransient().ShouldBeTrue();
    }

    [Theory]
    [InlineData(ErrorKind.Unknown)]
    [InlineData(ErrorKind.Predicate)]
    [InlineData(ErrorKind.BadRequest)]
    [InlineData(ErrorKind.Functional)]
    [InlineData(ErrorKind.NotImplemented)]
    [InlineData(ErrorKind.NotFound)]
    [InlineData(ErrorKind.None)]
    [InlineData(ErrorKind.AuthenticationRequired)]
    [InlineData(ErrorKind.NotAuthorized)]
    [InlineData(ErrorKind.Conflict)]
    public void IsTransient_PermanentKinds_ReturnFalse(ErrorKind kind)
    {
        kind.IsTransient().ShouldBeFalse();
    }

}
