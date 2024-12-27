/*
    MIT License

    michael rinderle 2024
    written by michael rinderle <michael@sofdigital.net>

    Permission is hereby granted, free of charge, to any person obtaining a copy
    of this software and associated documentation files (the "Software"), to deal
    in the Software without restriction, including without limitation the rights
    to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
    copies of the Software, and to permit persons to whom the Software is
    furnished to do so, subject to the following conditions:

    The above copyright notice and this permission notice shall be included in all
    copies or substantial portions of the Software.

    THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
    IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
    FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
    AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
    LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
    OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
    SOFTWARE.

*/

#nullable enable

using System;
using Ptm.Models.Dotnet;
using System.Collections.Generic;

namespace Ptm.Models.Mac;

/// <summary>
/// Represents the build environment for macOS, including .NET runtimes, SDKs, and other relevant tools.
/// </summary>
public class MacBuildEnvironment
{
    public bool IsEnvironmentVerified { get; set; }

    public List<DotnetRuntime>? DotnetRuntimes { get; set; }

    public List<DotnetSdk>? DotnetSdks { get; set; }

    public List<DotnetWorkload>? DotnetMauiWorkloads { get; set; }

    public Version? MacOsVersion { get; set; }

    public Version? XcodeVersion { get; set; }

    public string? XcodeBuildVersion { get; set; }

    /// <summary>
    /// Resets the build environment to its default state.
    /// </summary>
    public void ResetEnvironment()
    {
        IsEnvironmentVerified = false;
        DotnetRuntimes = null;
        DotnetSdks = null;
        DotnetMauiWorkloads = null;
        XcodeVersion = null;
        XcodeBuildVersion = null;
    }
}