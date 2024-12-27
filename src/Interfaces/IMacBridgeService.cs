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
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.VisualStudio.Debugger.Interop;
using Ptm.Models.Dotnet;
using Ptm.Models.Mac;

namespace Ptm.Interfaces;

/// <summary>
///     Interface for managing the connection bridge to a Mac machine.
/// </summary>
/// <remarks>
///     This interface provides methods to connect and disconnect from a Mac machine,
///     as well as properties to check the connection status and details.
/// </remarks>
public interface IMacBridgeService
{
    /// <summary>
    ///     Gets or sets a value indicating whether the bridge is connected.
    /// </summary>
    bool IsConnected { get; set; }

    /// <summary>
    ///     Gets or sets the host of the Mac connection.
    /// </summary>
    MacConnection? MacConnection { get; set; }

    /// <summary>
    ///     Gets or sets the Mac build environment.
    /// </summary>
    MacBuildEnvironment? MacBuildEnv { get; set; }

    /// <summary>
    ///     Gets or sets the Mac build session.
    /// </summary>
    MacBuildSession MacBuildSession { get; set; }

    /// <summary>
    ///     Gets or sets the cancellation token source for the Mac build.
    /// </summary>
    CancellationTokenSource? OperationTokenSource { get; set; }

    /// <summary>
    ///     Gets or sets the Visual Studio debug server.
    /// </summary>
    IDebugCoreServer3? DebugServer { get; set; }

    /// <summary>
    ///     Asynchronously attaches to the Visual Studio debugger.
    /// </summary>
    /// <returns>
    ///     A task that represents the asynchronous operation. The task result contains a boolean value indicating whether
    ///     the attachment was successful.
    /// </returns>
    Task<bool> AttachToVsDebuggerAsync();

    /// <summary>
    ///     Asynchronously connects to the Mac machine using the specified connection details.
    /// </summary>
    /// <param name="hostname">The hostname of the Mac connection.</param>
    /// <returns>
    ///     A task that represents the asynchronous operation. The task result contains a boolean value indicating whether
    ///     the connection was successful.
    /// </returns>
    Task<bool> BridgeConnectAsync(string hostname);

    /// <summary>
    ///     Asynchronously disconnects from the Mac machine.
    /// </summary>
    /// <returns>
    ///     A task that represents the asynchronous operation. The task result contains a boolean value indicating whether
    ///     the disconnection was successful.
    /// </returns>
    Task<bool> BridgeDisconnectAsync();

    /// <summary>
    ///     Gets the build session path for the specified project.
    /// </summary>
    /// <param name="projectName">The name of the project.</param>
    /// <returns>The build session path for the specified project.</returns>
    string GetBuildSessionPath(string projectName);

    /// <summary>
    ///     Generates a unique build session ID for the specified project.
    /// </summary>
    /// <param name="projectName">The name of the project.</param>
    /// <returns>A unique build session ID for the specified project.</returns>
    string GenerateBuildSessionId(string projectName);

    /// <summary>
    ///     Generates a command for the build session's .csproj file asynchronously.
    /// </summary>
    /// <param name="command">The command to be executed.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the generated command string.</returns>
    Task<string?> GenerateBuildSessionCsprojCmdAsync(string command);

    /// <summary>
    ///     Gets the macOS version asynchronously.
    /// </summary>
    /// <returns>A task that represents the asynchronous operation. The task result contains the macOS version.</returns>
    Task<Version?> GetMacOsVersionAsync();

    /// <summary>
    ///     Gets the Mac signing identity asynchronously.
    /// </summary>
    /// <returns>A task that represents the asynchronous operation. The task result contains the Mac signing identity.</returns>
    Task<MacSigningIdentity?> GetMacSigningIdentityAsync(string bundleId);

    /// <summary>
    ///     Gets the version of a specific Xcode package on the Mac machine asynchronously.
    /// </summary>
    /// <param name="packageId">The ID of the Xcode package.</param>
    /// <returns>
    ///     A task that represents the asynchronous operation. The task result contains the version information of the
    ///     Xcode package.
    /// </returns>
    Task<MacPkgVersion?> GetMacXcodePkgVersionAsync(string packageId);

    /// <summary>
    ///     Gets the version of the Visual Studio debugger asynchronously.
    /// </summary>
    /// <returns>
    ///     A task that represents the asynchronous operation. The task result contains the version of the Visual Studio
    ///     debugger.
    /// </returns>
    Task<Version?> GetVsDebuggerVersionAsync();

    /// <summary>
    ///     Starts the build process asynchronously for the specified Mac build session.
    /// </summary>
    /// <returns>
    ///     A task that represents the asynchronous operation. The task result contains a boolean value indicating whether
    ///     the build was started successfully.
    /// </returns>
    Task<bool> StartBuildOperationAsync();

    /// <summary>
    ///     Starts the clean process asynchronously for the specified Mac build session.
    /// </summary>
    /// <returns>
    ///     A task that represents the asynchronous operation. The task result contains a boolean value indicating whether
    ///     the clean was started successfully.
    /// </returns>
    Task<bool> StartCleanOperationAsync();

    /// <summary>
    ///     Starts the debug process asynchronously for the specified Mac build session.
    /// </summary>
    /// <returns>
    ///     A task that represents the asynchronous operation. The task result contains a boolean value indicating whether
    ///     the debug was started successfully.
    /// </returns>
    Task<bool> StartDebugOperationAsync();

    /// <summary>
    ///     Starts the rebuild process asynchronously for the specified Mac build session.
    /// </summary>
    /// <returns>
    ///     A task that represents the asynchronous operation. The task result contains a boolean value indicating whether
    ///     the rebuild was started successfully.
    /// </returns>
    Task<bool> StartRebuildOperationAsync();

    /// <summary>
    ///     Starts the Visual Studio debugger asynchronously.
    /// </summary>
    /// <returns>
    ///     A task that represents the asynchronous operation. The task result contains a boolean value indicating whether
    ///     the debugger was started successfully.
    /// </returns>
    Task<bool> StartVsDebuggerAsync();

    /// <summary>
    ///     Stops the Visual Studio debugger asynchronously.
    /// </summary>
    /// <returns>
    ///     A task that represents the asynchronous operation. The task result contains a boolean value indicating whether
    ///     the debugger was stopped successfully.
    /// </returns>
    Task<bool> StopVsDebuggerAsync();

    /// <summary>
    ///     Transfers the project asynchronously.
    /// </summary>
    /// <returns>
    ///     A task that represents the asynchronous operation. The task result contains a boolean value indicating whether
    ///     the project transfer was successful.
    /// </returns>
    Task<bool> TransferProjectAsync();

    /// <summary>
    ///     Tests the SSH credentials asynchronously.
    /// </summary>
    /// <param name="host">The SSH host.</param>
    /// <param name="username">The SSH username.</param>
    /// <param name="password">The SSH password.</param>
    /// <returns>
    ///     A task that represents the asynchronous operation. The task result contains a boolean indicating whether the
    ///     credentials are valid.
    /// </returns>
    Task<MacNetworkInfo?> VerifyMacCredentialsAsync(string host, string username, string password);

    /// <summary>
    ///     Verifies the .NET MAUI workloads installed on the Mac machine asynchronously.
    /// </summary>
    /// <returns>
    ///     A task that represents the asynchronous operation. The task result contains a list of .NET MAUI workloads
    ///     installed on the Mac machine.
    /// </returns>
    Task<List<DotnetWorkload>?> VerifyMacDotnetMauiWorkloadAsync();

    /// <summary>
    ///     Verifies the .NET runtimes installed on the Mac machine asynchronously.
    /// </summary>
    /// <returns>
    ///     A task that represents the asynchronous operation. The task result contains a list of .NET runtimes installed
    ///     on the Mac machine.
    /// </returns>
    Task<List<DotnetRuntime>?> VerifyMacDotnetRuntimesAsync();

    /// <summary>
    ///     Verifies the .NET SDKs installed on the Mac machine asynchronously.
    /// </summary>
    /// <returns>
    ///     A task that represents the asynchronous operation. The task result contains a list of .NET SDKs installed on
    ///     the Mac machine.
    /// </returns>
    Task<List<DotnetSdk>?> VerifyMacDotnetSdksAsync();

    /// <summary>
    ///     Verifies the Xcode build tools installed on the Mac machine asynchronously.
    /// </summary>
    /// <returns>
    ///     A task that represents the asynchronous operation. The task result contains a boolean value indicating whether
    ///     the Xcode build tools are installed.
    /// </returns>
    Task<bool> VerifyMacXcodeBuildToolsAsync();

    /// <summary>
    ///     Verifies the Xcode license status on the Mac machine asynchronously.
    /// </summary>
    /// <returns>
    ///     A task that represents the asynchronous operation. The task result contains a boolean value indicating whether
    ///     the Xcode license is accepted.
    /// </returns>
    Task<bool> VerifyMacXcodeLicenseStatusAsync();

    /// <summary>
    ///     Verifies if a specific Xcode package exists on the Mac machine asynchronously.
    /// </summary>
    /// <param name="packageId">The ID of the Xcode package.</param>
    /// <returns>
    ///     A task that represents the asynchronous operation. The task result contains a boolean value indicating whether
    ///     the Xcode package exists.
    /// </returns>
    Task<bool> VerifyMacXcodePkgExistsAsync(string packageId);

    /// <summary>
    ///     Verifies the Xcode version on the Mac machine asynchronously.
    /// </summary>
    /// <param name="environment">The build environment details.</param>
    /// <returns>
    ///     A task that represents the asynchronous operation. The task result contains a boolean value indicating whether
    ///     the Xcode version is valid.
    /// </returns>
    Task<bool> VerifyMacXcodeVersionAsync(MacBuildEnvironment environment);

    /// <summary>
    ///     Verifies if the Visual Studio debugger exists on the Mac machine asynchronously.
    /// </summary>
    /// <param name="installIfMissing">A boolean value indicating whether to install the debugger if it is missing.</param>
    /// <returns>
    ///     A task that represents the asynchronous operation. The task result contains a boolean value indicating whether
    ///     the Visual Studio debugger exists.
    /// </returns>
    Task<bool> VerifyVsDebuggerExistsAsync(bool installIfMissing = true);
}