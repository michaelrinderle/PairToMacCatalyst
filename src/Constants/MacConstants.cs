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

// TODO: MacConstants.cs, add logic for checking minimum version requirements for Xcode\MacCatalyst\MacOS

using Ptm.Models.Mac;

namespace Ptm.Constants;

/// <summary>
/// Class containing constants related to Mac package names, minimum version requirements, and build environment paths.
/// </summary>
/// <remarks>
/// This class includes constants for Mac package names, minimum version requirements for MacOS, Xcode, and Xcode System Resources,
/// as well as paths related to the Mac build environment.
/// </remarks>
public class MacConstants
{
    // Mac package names
    public const string CoreTypesPkgFullName = "com.apple.pkg.CoreTypes";
    public const string CoreTypesPkgName = "CoreTypes.pkg";
    public const string MobileDeviceDevelopmentPkgFullName = "com.apple.pkg.MobileDeviceDevelopment";
    public const string MobileDeviceDevelopmentPkgName = "MobileDeviceDevelopment.pkg";
    public const string MobileDevicePkgFullName = "com.apple.pkg.MobileDevice";
    public const string MobileDevicePkgName = "MobileDevice.pkg";
    public const string XcodeSystemResourcesPkgFullName = "com.apple.pkg.XcodeSystemResources";
    public const string XcodeSystemResourcesPkgName = "XcodeSystemResources.pkg";

    // Mac minimum version requirements
    public static readonly MacPkgVersion MinimumMacOsVersion = new MacPkgVersion("10.15.0"); // for .NET 7
    public static readonly MacPkgVersion MinimumXcodeSystemResourcesVersion = new MacPkgVersion("16.0.0");
    public static readonly MacPkgVersion MinimumXcodeVersion = new MacPkgVersion("13.0.0");

    // Mac Build Environment
    public const string MacBuildPath = "~/Library/Caches/Maui/builds";
    public const string MacDotnetBinDefault = "/usr/local/share/dotnet/dotnet";
    public const string MacHomeBuildToolDir = "~/.ptm";
    public const string MacVsDebuggerDir = "~/.ptm/vsdbg";

    // dotnet build -t:Run -f net9.0-maccatalyst
}