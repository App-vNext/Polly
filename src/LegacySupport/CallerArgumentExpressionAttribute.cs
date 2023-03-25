// Copyright (c) Microsoft Corporation. All Rights Reserved.

#pragma warning disable IDE0079
#pragma warning disable SA1101
#pragma warning disable SA1512

// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Diagnostics.CodeAnalysis;

namespace System.Runtime.CompilerServices;

/// <summary>
/// Tags parameter that should be filled with specific caller name.
/// </summary>
[AttributeUsage(AttributeTargets.Parameter, AllowMultiple = false, Inherited = false)]
[ExcludeFromCodeCoverage]
internal sealed class CallerArgumentExpressionAttribute : Attribute
{
    /// <summary>
    /// Initializes a new instance of the <see cref="CallerArgumentExpressionAttribute"/> class.
    /// </summary>
    /// <param name="parameterName">Function parameter to take the name from.</param>
    public CallerArgumentExpressionAttribute(string parameterName) => ParameterName = parameterName;

    /// <summary>
    /// Gets name of the function parameter that name should be taken from.
    /// </summary>
    public string ParameterName { get; }
}
