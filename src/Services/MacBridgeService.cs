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
using System.ComponentModel.Composition;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.VisualStudio.Debugger.Interop;
using Ptm.Constants;
using Ptm.Enums;
using Ptm.Interfaces;
using Ptm.Models.Dotnet;
using Ptm.Models.Mac;
using Renci.SshNet;
using VSLangProj;
using StorageConstants = Ptm.Constants.ExtensionConstants.SettingsStorage;

namespace Ptm.Services;

/// <inheritdoc />
[Export(typeof(IMacBridgeService))]
[PartCreationPolicy(CreationPolicy.Shared)]
[method: ImportingConstructor]
public class MacBridgeService(
    IOutputService outputService,
    ISecureStorageService secureStorageService,
    ISecureTransferService secureTransferService)
    : IMacBridgeService
{
    private readonly IOutputService _outputService = outputService;
    private readonly ISecureStorageService _secureStorageService = secureStorageService;
    private readonly ISecureTransferService _secureTransferService = secureTransferService;

    private SshClient? SshClient { get; set; }

    public bool IsConnected { get; set; }

    public MacConnection? MacConnection { get; set; }

    public MacBuildEnvironment MacBuildEnv { get; set; } = new();

    public MacBuildSession MacBuildSession { get; set; } = new();

    public CancellationTokenSource? OperationTokenSource { get; set; } = new();

    public IDebugCoreServer3 DebugServer { get; set; }

    /// <inheritdoc />
    public Task<bool> AttachToVsDebuggerAsync()
    {
        throw new NotImplementedException();
    }

    /// <inheritdoc />
    public async Task<bool> BridgeConnectAsync(string hostname)
    {
        // TODO: MacBridgeService.cs, add logic to reconnect SshClient if disconnected

        try
        {
            if (IsConnected && MacConnection?.Hostname == hostname) return true;

            var macConnections = await _secureStorageService
                .GetDataAsync<List<MacConnection>>(StorageConstants.MacConnectionListKey);

            var macConnection = macConnections?
                .FirstOrDefault(x => x.Hostname == hostname);

            if (macConnection is null) return false;

            SshClient = await _secureTransferService.GetSshClientAsync(
                macConnection.Hostname,
                macConnection.Username,
                macConnection.Password);

            SshClient?.Connect();

            if (SshClient?.IsConnected == true)
            {
                macConnection.Password = string.Empty;
                MacConnection = macConnection;
                IsConnected = true;

                return true;
            }

            MacConnection = null;
            IsConnected = false;

            return false;
        }
        catch (Exception ex)
        {
            await _outputService.WriteToOutputAsync("Failed to disconnect Mac bridge", OutputPaneType.Debug);
        }

        return false;
    }

    /// <inheritdoc />
    public async Task<bool> BridgeDisconnectAsync()
    {
        try
        {
            if (SshClient is not null && SshClient.IsConnected)
                await StopVsDebuggerAsync();

            MacConnection = null;
            IsConnected = false;

            SshClient?.Disconnect();
            SshClient = null;

            return true;
        }
        catch (Exception ex)
        {
            await _outputService.WriteToOutputAsync("Failed to disconnect Mac bridge", OutputPaneType.Debug);
        }

        return false;
    }

    /// <inheritdoc />
    public string GetBuildSessionPath(string projectName)
    {
        return MacConstants.MacBuildPath + "/" + projectName + "/" + GenerateBuildSessionId(projectName) + "/";
    }

    /// <inheritdoc />
    public string GenerateBuildSessionId(string projectName)
    {
        using var sha256 = SHA256.Create();
        var hashBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(projectName));
        var hashString = new StringBuilder(64);

        foreach (var b in hashBytes)
            hashString.Append(b.ToString("x2"));

        return hashString.ToString();
    }

    public async Task<string?> GenerateBuildSessionCsprojCmdAsync(string command)
    {
        try
        {
            var projectPath = Path.GetDirectoryName(MacBuildSession.MauiProjectPath);
            var projectName = Path.GetFileNameWithoutExtension(projectPath);

            var sessionPath = GetBuildSessionPath(projectName);

            sessionPath = await _secureTransferService.DirectoryExpandHomePathAsync(SshClient!, sessionPath);

            string? dotnetLocation;
            var dotnetSymlink =
                await _secureTransferService.ExecuteSshCommandAsync(SshClient!, "ls -l $(which dotnet)");

            if (!string.IsNullOrEmpty(dotnetSymlink))
            {
                var match = Regex.Match(dotnetSymlink, @"->\s+(.*)");

                if (match.Success)
                    dotnetLocation = match.Groups[1].Value;
                else
                    return string.Empty;
            }
            else
            {
                dotnetLocation = await _secureTransferService.ExecuteSshCommandAsync(SshClient!, "which dotnet");
            }

            if (string.IsNullOrEmpty(dotnetLocation)) return string.Empty;

            var csproj = _secureTransferService.SanitizeMacPath(sessionPath + MacBuildSession.MauiProjectPath);

            if (!await _secureTransferService.FileExistsAsync(SshClient!, csproj)) return null;

            return $"{dotnetLocation} {command} {csproj}";
        }
        catch (Exception ex)
        {
            await _outputService.WriteToOutputAsync("Failed to generate csproj path", OutputPaneType.Debug);
        }

        return null;
    }

    /// <inheritdoc />
    public async Task<Version?> GetMacOsVersionAsync()
    {
        try
        {
            if (SshClient is { IsConnected: false }) return null;

            var output = await _secureTransferService
                .ExecuteSshCommandAsync(SshClient!, ShellCommandConstants.GetMacOsVersion);

            if (string.IsNullOrEmpty(output)) return null;

            var parsed = Version.TryParse(output!.Trim(), out var version);

            return parsed ? version : null;
        }
        catch (Exception ex)
        {
            await _outputService.WriteToOutputAsync("Failed to get MacOS version", OutputPaneType.Debug);
        }

        return null;
    }

    /// <inheritdoc />
    public async Task<MacSigningIdentity?> GetMacSigningIdentityAsync(string bundleId)
    {
        MacSigningIdentity identity = new();

        try
        {
            if (SshClient is { IsConnected: false }) return null;

            // get Mac Developer Cert
            var certOutput = await _secureTransferService
                .ExecuteSshCommandAsync(SshClient!, ShellCommandConstants.GetAppleDeveloperCert);

            if (!string.IsNullOrEmpty(certOutput))
            {
                identity.CertificateName = certOutput?.Split('"')[1];
                identity.TeamId = certOutput?.Split('(')[1].Split(')')[0];
            }

            // Get Provisioning Profile
            var profileOutput = await _secureTransferService
                .ExecuteSshCommandAsync(SshClient!, ShellCommandConstants.GetAppleProvisioningProfiles);

            if (string.IsNullOrEmpty(profileOutput)) return null;

            foreach (var profile in profileOutput!.Split('\n'))
            {
                if (string.IsNullOrEmpty(profile)) continue;

                var readProfileOutput = await _secureTransferService
                    .ExecuteSshCommandAsync(SshClient!,
                        ShellCommandConstants.ReadAppleProvisionProfiles + " " + profile);

                if (string.IsNullOrEmpty(readProfileOutput)) return null;

                if (!readProfileOutput!.Contains(bundleId)) continue;

                identity.ProvisioningProfile = Path.GetFileNameWithoutExtension(profile);
                identity.ProvisioningProfileId = profile.Split('/').Last();
                identity.BundleId = bundleId;
                identity.AppId = $"{identity.TeamId}.{identity.BundleId}";

                break;
            }

            return identity;
        }
        catch (Exception ex)
        {
            await _outputService.WriteToOutputAsync("Failed to get Apple signing identity", OutputPaneType.Debug);
        }

        return null;
    }

    /// <inheritdoc />
    public async Task<MacPkgVersion?> GetMacXcodePkgVersionAsync(string packageId)
    {
        try
        {
            if (SshClient is { IsConnected: false }) return null;

            var command = $"pkgutil --pkg-info={packageId}";
            var output = await _secureTransferService.ExecuteSshCommandAsync(SshClient!, command);

            if (string.IsNullOrEmpty(output) || output!.Contains("No receipt for")) return null;

            var versionLine = output.Split(['\r', '\n'], StringSplitOptions.RemoveEmptyEntries)
                .FirstOrDefault(line => line.StartsWith("version:"));

            return !string.IsNullOrEmpty(versionLine)
                ? new MacPkgVersion(versionLine.Split(':')[1].Trim())
                : null;
        }
        catch (Exception ex)
        {
            await _outputService.WriteToOutputAsync($"Failed to get package version for {packageId}: {ex.Message}",
                OutputPaneType.Debug);
        }

        return null;
    }

    /// <inheritdoc />
    public async Task<Version?> GetVsDebuggerVersionAsync()
    {
        try
        {
            if (SshClient is { IsConnected: false }) return null;

            var cmd = $"cat {MacConstants.MacVsDebuggerDir}/version.txt";

            var output = await _secureTransferService.ExecuteSshCommandAsync(SshClient!, cmd);

            return !string.IsNullOrEmpty(output)
                ? Version.Parse(output!.Split(' ')[0].Trim())
                : null;
        }
        catch (Exception ex)
        {
            await _outputService.WriteToOutputAsync("Failed to get Vs Debugger version",
                OutputPaneType.Debug);
        }

        return null;
    }

    /// <inheritdoc />
    public async Task<bool> StartBuildOperationAsync()
    {
        try
        {
            if (SshClient is { IsConnected: false }) return false;

            if (!MacBuildSession.IsActive) return false;

            OperationTokenSource?.Token.ThrowIfCancellationRequested();

            if (!await TransferProjectAsync()) return false;

            OperationTokenSource?.Token.ThrowIfCancellationRequested();

            var buildCmd = await GenerateBuildSessionCsprojCmdAsync("build");

            OperationTokenSource?.Token.ThrowIfCancellationRequested();

            if (!await _secureTransferService
                    .ExecuteSshCommandStreamAsync(SshClient!, buildCmd, OutputPaneType.Build, ["succeeded"]))
                return false;

            OperationTokenSource?.Token.ThrowIfCancellationRequested();

            return true;
        }
        catch (OperationCanceledException)
        {
        }
        catch (Exception ex)
        {
            await _outputService.WriteToOutputAsync("Failed to execute build command", OutputPaneType.Debug);
        }

        return false;
    }

    /// <inheritdoc />
    public async Task<bool> StartCleanOperationAsync()
    {
        try
        {
            if (SshClient is { IsConnected: false }) return false;

            if (!MacBuildSession.IsActive) return false;

            OperationTokenSource?.Token.ThrowIfCancellationRequested();

            var buildCmd = await GenerateBuildSessionCsprojCmdAsync("clean");

            if (string.IsNullOrEmpty(buildCmd)) return false;

            OperationTokenSource?.Token.ThrowIfCancellationRequested();

            if (!await _secureTransferService
                    .ExecuteSshCommandStreamAsync(SshClient!, buildCmd, OutputPaneType.Build, ["succeeded"]))
                return false;

            return true;
        }
        catch (OperationCanceledException)
        {
        }
        catch (Exception ex)
        {
            await _outputService.WriteToOutputAsync("Failed to execute clean command", OutputPaneType.Debug);
        }

        return false;
    }

    /// <inheritdoc />
    public async Task<bool> StartDebugOperationAsync()
    {
        try
        {
            if (SshClient is { IsConnected: false }) return false;

            if (!MacBuildSession.IsActive) return false;

            OperationTokenSource?.Token.ThrowIfCancellationRequested();

            if (!await TransferProjectAsync()) return false;

            OperationTokenSource?.Token.ThrowIfCancellationRequested();

            var buildCmd = await GenerateBuildSessionCsprojCmdAsync(
                $"run -f {MacBuildSession.MacCatalystVersion} --project");

            OperationTokenSource?.Token.ThrowIfCancellationRequested();

            if (!await _secureTransferService
                    .ExecuteSshCommandStreamAsync(
                        SshClient!, buildCmd!, OutputPaneType.Build, ["succeeded"])) return false;

            return true;
        }
        catch (OperationCanceledException)
        {
        }
        catch (Exception ex)
        {
            await _outputService.WriteToOutputAsync("Failed to execute debug command", OutputPaneType.Debug);
        }

        return false;
    }

    /// <inheritdoc />
    public async Task<bool> StartRebuildOperationAsync()
    {
        try
        {
            if (SshClient is { IsConnected: false }) return false;

            OperationTokenSource?.Token.ThrowIfCancellationRequested();

            if (!await TransferProjectAsync()) return false;

            OperationTokenSource?.Token.ThrowIfCancellationRequested();

            var buildCmd = await GenerateBuildSessionCsprojCmdAsync("build --no-incremental");

            OperationTokenSource?.Token.ThrowIfCancellationRequested();

            if (!await _secureTransferService
                    .ExecuteSshCommandStreamAsync(SshClient!, buildCmd, OutputPaneType.Build, ["succeeded"]))
                return false;

            OperationTokenSource?.Token.ThrowIfCancellationRequested();

            return true;
        }
        catch (OperationCanceledException)
        {
        }
        catch (Exception ex)
        {
            await _outputService.WriteToOutputAsync("Failed to execute rebuild command", OutputPaneType.Debug);
        }

        return false;
    }

    /// <inheritdoc />
    public async Task<bool> StartVsDebuggerAsync()
    {
        try
        {
            if (SshClient is { IsConnected: false }) return false;

            // check if vsdbg is already running

            var vsDbgRunning = await _secureTransferService
                .ExecuteSshCommandAsync(SshClient!, ShellCommandConstants.GetVsDbgPid);

            if (!string.IsNullOrEmpty(vsDbgRunning)) return true;

            // start vsdbg

            var vsDbgPath = await _secureTransferService
                .DirectoryExpandHomePathAsync(SshClient!, MacConstants.MacVsDebuggerDir);

            var vsDbgStartCmd = vsDbgPath +
                                $"/vsdbg --interpreter=vscode --pauseEngineForDebugger > {vsDbgPath}/vsdbg.log 2>&1 &";

            var startOutput = await _secureTransferService.ExecuteSshCommandAsync(SshClient!, vsDbgStartCmd);

            await Task.Delay(2000);

            // confirm vsdbg is running by pid

            vsDbgRunning = await _secureTransferService
                .ExecuteSshCommandAsync(SshClient!, ShellCommandConstants.GetVsDbgPid);

            return !string.IsNullOrEmpty(vsDbgRunning);
        }
        catch (Exception ex)
        {
            await _outputService.WriteToOutputAsync("Failed to start Vs Debugger version", OutputPaneType.Debug);
        }

        return false;
    }

    /// <inheritdoc />
    public async Task<bool> StopVsDebuggerAsync()
    {
        try
        {
            if (SshClient is { IsConnected: false }) return false;

            // check if vsdbg is already running

            var vsDbgPid = await _secureTransferService
                .ExecuteSshCommandAsync(SshClient!, ShellCommandConstants.GetVsDbgPid);

            if (string.IsNullOrEmpty(vsDbgPid)) return true;

            // kill vsdbg process by pid

            var killVsDgb = await _secureTransferService
                .ExecuteSshCommandAsync(SshClient!, $"kill -9 {vsDbgPid}");

            return !string.IsNullOrEmpty(killVsDgb) && killVsDgb!.Contains("killed");
        }
        catch (Exception ex)
        {
            await _outputService.WriteToOutputAsync("Failed to start Vs Debugger version",
                OutputPaneType.Debug);
        }

        return false;
    }

    /// <inheritdoc />
    public async Task<bool> TransferProjectAsync()
    {
        try
        {
            if (SshClient is { IsConnected: false }) return false;

            var projectPath = Path.GetDirectoryName(MacBuildSession.MauiProjectPath);
            var projectName = Path.GetFileNameWithoutExtension(projectPath);

            if (string.IsNullOrEmpty(projectPath) || string.IsNullOrEmpty(projectName)) return false;

            List<string> excludeFolders = ["bin", "obj", ".vs", ".git", ".github", "packages", "node_modules"];

            OperationTokenSource?.Token.ThrowIfCancellationRequested();

            var sessionPath = GetBuildSessionPath(projectName);

            await _outputService.WriteToOutputAsync($"Transferring project to {MacConnection!.Hostname}\n",
                OutputPaneType.Build);

            OperationTokenSource?.Token.ThrowIfCancellationRequested();

            if (!await _secureTransferService
                    .DirectoryTransferAsync(SshClient!, projectPath, sessionPath, excludeFolders)) return false;

            OperationTokenSource?.Token.ThrowIfCancellationRequested();

            foreach (var reference in MacBuildSession.ReferencesPaths)
            {
                OperationTokenSource?.Token.ThrowIfCancellationRequested();

                var referencePath = Path.GetDirectoryName(reference);

                if (string.IsNullOrEmpty(referencePath)) return false;

                if (!await _secureTransferService
                        .DirectoryTransferAsync(SshClient!, referencePath!, sessionPath, excludeFolders,
                            token: OperationTokenSource!.Token))
                    return false;
            }

            return true;
        }
        catch (OperationCanceledException)
        {
        }
        catch (Exception ex)
        {
            await _outputService.WriteToOutputAsync("Failed to transfer project files to Mac host",
                OutputPaneType.Debug);
        }

        return false;
    }

    /// <inheritdoc />
    public async Task<MacNetworkInfo?> VerifyMacCredentialsAsync(string host, string username, string password)
    {
        SshClient? sshClient = null;
        MacNetworkInfo networkInfo = new();

        if (string.IsNullOrEmpty(host) ||
            string.IsNullOrEmpty(username) ||
            string.IsNullOrEmpty(password)) return null;

        try
        {
            sshClient = await _secureTransferService.GetSshClientAsync(host, username, password);
            sshClient?.Connect();

            if (sshClient is null || !sshClient.IsConnected) return null;

            networkInfo.Hostname = (await _secureTransferService
                    .ExecuteSshCommandAsync(sshClient, ShellCommandConstants.GetHostname))
                ?.Replace(".local", "");

            networkInfo.IpAddress = await _secureTransferService
                .ExecuteSshCommandAsync(sshClient, ShellCommandConstants.GetActivePublicIp);

            if (string.IsNullOrEmpty(networkInfo.Hostname) ||
                string.IsNullOrEmpty(networkInfo.IpAddress)) return null;

            return networkInfo;
        }
        catch (Exception ex)
        {
            await _outputService.WriteToOutputAsync("Failed to verify Mac credentials", OutputPaneType.Debug);
        }
        finally
        {
            if (sshClient is { IsConnected: true })
                sshClient.Disconnect();
        }

        return null;
    }

    /// <inheritdoc />
    public async Task<List<DotnetWorkload>?> VerifyMacDotnetMauiWorkloadAsync()
    {
        List<DotnetWorkload> workloads = [];

        try
        {
            if (SshClient is { IsConnected: false }) return null;

            var output = await _secureTransferService
                .ExecuteSshCommandAsync(SshClient!, ShellCommandConstants.GetDotnetWorkloads);

            var lines = output?.Split(['\r', '\n'], StringSplitOptions.RemoveEmptyEntries);

            foreach (var line in lines)
            {
                if (!line.Contains("maui")) continue;

                // macos needs `maui` workload

                Regex regex = new(@"\s{2,}");
                var parts = regex.Split(line!, 3);

                workloads.Add(new DotnetWorkload
                {
                    Id = parts![0].Trim(),
                    ManifestVersion = parts[1].Trim(),
                    InstallationSource = parts[2].Trim()
                });
            }

            return workloads;
        }
        catch (Exception ex)
        {
            await _outputService.WriteToOutputAsync("Failed to verify remote dotnet maui workload",
                OutputPaneType.Debug);
        }

        return null;
    }

    /// <inheritdoc />
    public async Task<List<DotnetRuntime>?> VerifyMacDotnetRuntimesAsync()
    {
        List<DotnetRuntime>? runtimes = [];

        try
        {
            if (SshClient is { IsConnected: false }) return null;

            var output = await _secureTransferService
                .ExecuteSshCommandAsync(SshClient!, ShellCommandConstants.GetDotnetRuntimes);

            var lines = output?.Split(['\r', '\n'], StringSplitOptions.RemoveEmptyEntries);

            foreach (var line in lines)
            {
                // disregard aspnetcore runtime
                if (line.Contains("Microsoft.AspNetCore.App")) continue;
                if (!line.Contains("dotnet")) continue;

                var parts = line?.Split(' ');

                runtimes.Add(new DotnetRuntime
                {
                    Runtime = parts![0].Trim(),
                    Version = parts[1].Trim(),
                    Path = parts[2].Replace("[", " ").Replace("]", "").Trim()
                });
            }

            return runtimes;
        }
        catch (Exception ex)
        {
            await _outputService.WriteToOutputAsync("Failed to verify remote dotnet installation",
                OutputPaneType.Debug);
        }

        return null;
    }

    /// <inheritdoc />
    public async Task<List<DotnetSdk>?> VerifyMacDotnetSdksAsync()
    {
        List<DotnetSdk>? sdks = [];

        try
        {
            if (SshClient is { IsConnected: false }) return null;

            var output = await _secureTransferService
                .ExecuteSshCommandAsync(SshClient!, ShellCommandConstants.GetDotnetSdks);

            var lines = output?.Split(['\r', '\n'], StringSplitOptions.RemoveEmptyEntries);

            foreach (var line in lines)
            {
                if (!line.Contains("dotnet")) continue;

                var parts = line?.Split(' ');

                sdks.Add(new DotnetSdk
                {
                    Version = Version.Parse(parts![0].Trim()),
                    Path = parts[1].Replace("[", " ").Replace("]", "").Trim()
                });
            }

            return sdks;
        }
        catch (Exception ex)
        {
            await _outputService.WriteToOutputAsync("Failed to verify remote dotnet installation",
                OutputPaneType.Debug);
        }

        return null;
    }

    /// <inheritdoc />
    public async Task<bool> VerifyMacXcodeBuildToolsAsync()
    {
        try
        {
            if (SshClient is { IsConnected: false }) return false;

            var output = await _secureTransferService
                .ExecuteSshCommandAsync(SshClient!, ShellCommandConstants.GetXcodeSelectCli);

            if (output is null || string.IsNullOrEmpty(output)) return false;

            // /Applications/Xcode.app/Contents/Developer

            return output.Contains("Xcode.app");
        }
        catch (Exception ex)
        {
            await _outputService.WriteToOutputAsync($"Failed to verify Xcode license status: {ex.Message}",
                OutputPaneType.Debug);
        }

        return false;
    }

    /// <inheritdoc />
    public async Task<bool> VerifyMacXcodeLicenseStatusAsync()
    {
        try
        {
            if (SshClient is { IsConnected: false }) return false;

            var output = await _secureTransferService
                .ExecuteSshCommandAsync(SshClient!, ShellCommandConstants.ReadXcodeLicenseStatus);

            var lines = output?.Split(['\r', '\n'], StringSplitOptions.RemoveEmptyEntries);

            if (lines is null || !lines.Any()) return false;

            // {
            //     IDELastGMLicenseAgreedTo = EA1879;
            //     IDELastPTRLicenseAgreedTo = EA1879;
            //     IDEXcodeVersionForAgreedToGMLicense = "16.0";
            //     IDEXcodeVersionForAgreedToPTRLicense = "16.0";
            // }

            var gmlLicenseValue = lines
                .FirstOrDefault(x => x.Contains("IDELastGMLicenseAgreedTo"))
                ?.Split('=')[1]
                .Trim();

            var ptrLicenseValue = lines
                .FirstOrDefault(x => x.Contains("IDELastPTRLicenseAgreedTo"))
                ?.Split('=')[1]
                .Trim();

            return !string.IsNullOrEmpty(gmlLicenseValue) &&
                   !string.IsNullOrEmpty(ptrLicenseValue);
        }
        catch (Exception ex)
        {
            await _outputService.WriteToOutputAsync($"Failed to verify Xcode license status: {ex.Message}",
                OutputPaneType.Debug);
        }

        return false;
    }

    /// <inheritdoc />
    public async Task<bool> VerifyMacXcodePkgExistsAsync(string packageId)
    {
        try
        {
            if (SshClient is { IsConnected: false }) return false;

            var output = await _secureTransferService
                .ExecuteSshCommandAsync(SshClient!, ShellCommandConstants.GetPkgList);

            if (output is null || string.IsNullOrEmpty(output)) return false;

            return output.Contains(packageId);
        }
        catch (Exception ex)
        {
            await _outputService.WriteToOutputAsync($"Failed to verify package id: {packageId}",
                OutputPaneType.Debug);
        }

        return false;
    }

    /// <inheritdoc />
    public async Task<bool> VerifyMacXcodeVersionAsync(MacBuildEnvironment environment)
    {
        try
        {
            if (SshClient is { IsConnected: false }) return false;

            var output = await _secureTransferService
                .ExecuteSshCommandAsync(SshClient!, ShellCommandConstants.GetXcodeVersion);

            var lines = output?.Split(['\r', '\n'], StringSplitOptions.RemoveEmptyEntries);

            if (lines is null || !lines.Any()) return false;

            // Xcode 16.2
            // Build version 16C5032a

            environment.XcodeVersion = Version.Parse(lines[0]?.Split(' ')[1]!);
            environment.XcodeBuildVersion = lines[1]?.Split(' ')[2];

            return true;
        }
        catch (Exception ex)
        {
            await _outputService.WriteToOutputAsync($"Failed to verify Xcode license status: {ex.Message}",
                OutputPaneType.Debug);
        }

        return false;
    }

    /// <inheritdoc />
    public async Task<bool> VerifyVsDebuggerExistsAsync(bool installIfMissing = true)
    {
        try
        {
            if (SshClient is { IsConnected: false }) return false;

            var vsDbgPath = await _secureTransferService
                .DirectoryExpandHomePathAsync(SshClient!, MacConstants.MacVsDebuggerDir);

            var vsDbgBin = vsDbgPath + "/vsdbg";

            //check if vsdbg exists
            var vdDbgBinExists = await _secureTransferService
                .FileExistsAsync(SshClient!, vsDbgBin);

            if (vdDbgBinExists) return true;

            if (!installIfMissing) return false;

            // TODO: MacBridgeService.cs, add logic to check versions and upgrade if necessary

            // install vsdbg
            var output = await _secureTransferService.ExecuteSshCommandAsync(SshClient!,
                ShellCommandConstants.InstallVsDbg + vsDbgPath);

            // Info: Previous installation at '/Users/{username}/.ptm/vsdbg' not found
            // Info: Using vsdbg version '17.12.11216.3'
            // Using arguments
            // Version: 'latest'
            // Location: '/Users/{username}/.ptm/vsdbg'
            // SkipDownloads: 'false'
            // LaunchVsDbgAfter: 'false'
            // RemoveExistingOnUpgrade: 'false'
            // Info: Using Runtime ID 'osx-arm64'
            // Downloading https://vsdebugger-cyg0dxb6czfafzaz.b01.azurefd.net/vsdbg-17-12-11216-3/vsdbg-osx-arm64.tar.gz
            // Info: Successfully installed vsdbg at '/Users/{username}/.ptm/vsdbg'

            return output?.Contains("Success") ?? false;
        }
        catch (Exception ex)
        {
            await _outputService.WriteToOutputAsync($"Failed to verify remote VS Debugger: {ex.Message}",
                OutputPaneType.Debug);
        }

        return false;
    }
}