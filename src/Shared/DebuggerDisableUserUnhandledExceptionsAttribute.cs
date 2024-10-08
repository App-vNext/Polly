#pragma warning disable
// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

// Adapted from https://github.com/dotnet/runtime/blob/bffe34e0c7ff8a05e79d884ed8447426aae17bfb/src/libraries/System.Private.CoreLib/src/System/Diagnostics/DebuggerDisableUserUnhandledExceptionsAttribute.cs
// See the following links for more information and context:
// https://github.com/dotnet/runtime/issues/103105,
// https://github.com/dotnet/runtime/pull/104813
// https://github.com/dotnet/aspnetcore/issues/57085

namespace System.Diagnostics;

/// <summary>
/// If a .NET Debugger is attached which supports the Debugger.BreakForUserUnhandledException(Exception) API,
/// this attribute will prevent the debugger from breaking on user-unhandled exceptions when the
/// exception is caught by a method with this attribute, unless BreakForUserUnhandledException is called.
/// </summary>
[AttributeUsage(AttributeTargets.Method)]
internal sealed class DebuggerDisableUserUnhandledExceptionsAttribute : Attribute
{
}
