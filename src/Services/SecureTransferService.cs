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
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Threading;
using Ptm.Constants;
using Ptm.Enums;
using Ptm.Interfaces;
using Renci.SshNet;
using Renci.SshNet.Common;
using Path = System.IO.Path;

namespace Ptm.Services;

/// <inheritdoc />
[Export(typeof(ISecureTransferService))]
[PartCreationPolicy(CreationPolicy.Shared)]
[method: ImportingConstructor]
public class SecureTransferService(
    IOutputService outputService)
    : ISecureTransferService
{
    private readonly JoinableTaskFactory _jtf = ThreadHelper.JoinableTaskFactory;

    private readonly IOutputService _outputService = outputService;

    private SftpClient? _sftpClient;

    /// <inheritdoc />
    public async Task<bool> DirectoryCreateAsync(SshClient sshClient, string remoteFolderPath)
    {
        try
        {
            if (sshClient is { IsConnected: false })
                return false;

            // Remove colons from the remote folder path
            remoteFolderPath = remoteFolderPath.Replace(":", "");

            if (remoteFolderPath.Contains("~"))
                remoteFolderPath = await DirectoryExpandHomePathAsync(sshClient, remoteFolderPath) ?? remoteFolderPath;

            if (!SetSftpClient(sshClient)) return false;

            if (!await DirectoryExistsAsync(sshClient, remoteFolderPath))
            {
                var directories = remoteFolderPath.Split('/');
                var currentPath = "";

                foreach (var dir in directories)
                {
                    if (string.IsNullOrEmpty(dir))
                        continue;

                    var sanitizedDir = dir.Replace(":", "");
                    currentPath += "/" + sanitizedDir;

                    if (!await DirectoryExistsAsync(sshClient, currentPath))
                        await _sftpClient!.CreateDirectoryAsync(currentPath);
                }
            }

            return true;
        }
        catch (Exception ex)
        {
            await _jtf.SwitchToMainThreadAsync();
            await _outputService.WriteToOutputAsync(ex.Message, OutputPaneType.Debug);
            await TaskScheduler.Default;
        }

        return false;
    }

    /// <inheritdoc />
    public async Task<string?> DirectoryExpandHomePathAsync(SshClient sshClient, string remoteFolderPath)
    {
        try
        {
            if (sshClient is { IsConnected: false }) return null;

            remoteFolderPath = remoteFolderPath.Replace(":", ""); // remove drive letter if present

            if (!remoteFolderPath.StartsWith("~")) return remoteFolderPath;

            var homeDirectory = await ExecuteSshCommandAsync(sshClient, ShellCommandConstants.GetHomeDirectory);

            return !string.IsNullOrEmpty(homeDirectory)
                ? remoteFolderPath.Replace("~", homeDirectory!.Trim())
                : null;
        }
        catch (Exception ex)
        {
            await _jtf.SwitchToMainThreadAsync();
            await _outputService.WriteToOutputAsync(ex.Message, OutputPaneType.Debug);
            await TaskScheduler.Default;
        }

        return null;
    }

    /// <inheritdoc />
    public async Task<bool> DirectoryExistsAsync(SshClient sshClient, string remoteFolderPath)
    {
        try
        {
            if (sshClient is { IsConnected: false }) return false;

            if (remoteFolderPath.Contains("~")) // sanity home directory check for full path for ssh command
                remoteFolderPath =
                    await DirectoryExpandHomePathAsync(sshClient, remoteFolderPath) ??
                    remoteFolderPath; // try unexpanded path as fallback

            if (!SetSftpClient(sshClient)) return false;

            var attributes = await Task.Run(() => _sftpClient!.GetAttributes(remoteFolderPath));

            return attributes.IsDirectory;
        }
        catch (SftpPathNotFoundException ex)
        {
            // no directory found throws exception, suppressing exception output
            Debug.WriteLine(ex.Message);
        }

        return false;
    }

    /// <inheritdoc />
    public async Task<bool> DirectoryTransferAsync(SshClient sshClient, string localFolderPath, string remoteBasePath,
        List<string>? ignoreFolders = null, bool preserveWriteAttributes = true, CancellationToken? token = null)
    {
        const int maxDegreeOfParallelism = 10;
        using var semaphore = new SemaphoreSlim(maxDegreeOfParallelism);

        try
        {
            if (sshClient is { IsConnected: false })
                return false;

            if (remoteBasePath.Contains("~"))
                remoteBasePath = await DirectoryExpandHomePathAsync(sshClient, remoteBasePath) ?? remoteBasePath;

            if (!SetSftpClient(sshClient))
                return false;

            await DirectoryCreateAsync(sshClient, remoteBasePath);

            var directoryInfo = new DirectoryInfo(localFolderPath);

            var tasks = directoryInfo.GetFiles("*", SearchOption.AllDirectories)
                .Where(file =>
                    ignoreFolders == null || !ignoreFolders.Any(folder => file.DirectoryName.Contains(folder)))
                .Select(async file =>
                {
                    await semaphore.WaitAsync();

                    try
                    {
                        var newFilePath = file.Directory!.FullName.Replace("\\", "/").Replace(":", "");
                        var remoteFolderPath = remoteBasePath + newFilePath;
                        var remoteFilePath = remoteFolderPath + "/" + file.Name.Replace(":", "");

                        await DirectoryCreateAsync(sshClient, remoteFolderPath);

                        if (preserveWriteAttributes)
                        {
                            if (await FileExistsAsync(sshClient, remoteFilePath))
                                await FileCreateAsync(sshClient, remoteFilePath, file.FullName);
                        }
                        else
                        {
                            await FileCreateAsync(sshClient, remoteFilePath, file.FullName);
                        }

                        Debug.WriteLine($"Transferring file {file.FullName}");

                        await _outputService.WriteToOutputAsync($"{file.FullName}\n", OutputPaneType.Build);
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine($"Error transferring file {file.FullName}: {ex.Message}");
                    }
                    finally
                    {
                        semaphore.Release();

                        token?.ThrowIfCancellationRequested();
                    }
                })
                .ToList();

            await Task.WhenAll(tasks);

            return true;
        }
        catch (OperationCanceledException)
        {
        }
        catch (Exception ex)
        {
            await _outputService.WriteToOutputAsync(ex.Message, OutputPaneType.Debug);
        }

        return false;
    }

    /// <inheritdoc />
    public async Task<string?> ExecuteSshCommandAsync(SshClient sshClient, string command)
    {
        try
        {
            var fullCommand = $"/bin/bash -c \"{ShellCommandConstants.ShellProfileSources} {command}\"";
            var cmd = sshClient.CreateCommand(fullCommand);
            var result = await Task.Run(() => cmd.Execute());

            return result.Trim();
        }
        catch (Exception ex)
        {
            await _outputService.WriteToOutputAsync(ex.Message, OutputPaneType.Debug);
        }

        return null;
    }

    /// <inheritdoc />
    public async Task<bool> ExecuteSshCommandStreamAsync(SshClient sshClient, string command,
        OutputPaneType? logOutput = null,
        string[]? outputMatch = null)
    {
        try
        {
            var matchFound = false;

            using var shellStream = sshClient.CreateShellStream(
                "shell", // terminal
                80, // columns
                24, // rows
                800, // width
                600, // height
                1024, // buffer
                new Dictionary<TerminalModes, uint>());

            shellStream.WriteLine("exec -a -l /bin/sh --noprofile --norc");
            ClearShellStream(shellStream);

            var fullCommand = $"/bin/bash -c \"{ShellCommandConstants.ShellProfileSources} {command}\"";

            var outputBuilder = new StringBuilder();
            var prompt = "$ ";

            shellStream.WriteLine(fullCommand);
            await shellStream.FlushAsync();

            while (true)
            {
                var output = shellStream.ReadLine(TimeSpan.FromSeconds(.5));
                if (output != null)
                {
                    outputBuilder.AppendLine(output + "\n");

                    if (logOutput != null)
                        await _outputService.WriteToOutputAsync(output + "\n", (OutputPaneType)logOutput);

                    if (outputMatch != null)
                        if (!matchFound)
                            foreach (var match in outputMatch)
                                matchFound = output.IndexOf(match, StringComparison.InvariantCultureIgnoreCase) > 0;

                    if (output.Trim().EndsWith(prompt)) break;
                }
                else
                {
                    break;
                }
            }

            // waiting for launch for now... check ps on build before closing...
            await Task.Delay(TimeSpan.FromSeconds(5));

            var outputString = outputBuilder.ToString();

            return outputMatch == null || matchFound;
        }
        catch (Exception ex)
        {
            await _outputService.WriteToOutputAsync(ex.Message, OutputPaneType.Debug);
        }

        return false;
    }

    /// <inheritdoc />
    public async Task<bool> FileCreateAsync(SshClient sshClient, string remoteFilePath, string fileName,
        bool preserveWriteAttributes = true)
    {
        try
        {
            if (sshClient is { IsConnected: false }) return false;

            if (remoteFilePath.Contains("~")) // sanity home directory check for full path for ssh command
                remoteFilePath =
                    await DirectoryExpandHomePathAsync(sshClient, remoteFilePath) ??
                    remoteFilePath; // try unexpanded path as fallback

            if (!SetSftpClient(sshClient)) return false;

            if (await FileExistsAsync(sshClient, remoteFilePath)) return true;

            using var fileStream = File.OpenRead(fileName);
            var lastWriteTime = File.GetLastWriteTime(fileName);

            await Task.Run(() => _sftpClient!.UploadFile(fileStream, remoteFilePath));

            if (!preserveWriteAttributes) return true;

            var attributes = _sftpClient!.GetAttributes(remoteFilePath);
            attributes.LastWriteTime = lastWriteTime;
            _sftpClient.SetAttributes(remoteFilePath, attributes);

            return true;
        }
        catch (Exception ex)
        {
            await _outputService.WriteToOutputAsync(ex.Message, OutputPaneType.Debug);
        }

        return false;
    }

    /// <inheritdoc />
    public async Task<bool> FileExistsAsync(SshClient sshClient, string remoteFilePath)
    {
        try
        {
            if (sshClient is { IsConnected: false }) return false;

            if (remoteFilePath.Contains("~")) // sanity home directory check for full path for ssh command
                remoteFilePath =
                    await DirectoryExpandHomePathAsync(sshClient, remoteFilePath) ??
                    remoteFilePath; // try unexpanded path as fallback

            if (!SetSftpClient(sshClient)) return false;

            var attributes = await Task.Run(() => _sftpClient!.GetAttributes(remoteFilePath));

            return attributes.IsRegularFile;
        }
        catch (SftpPathNotFoundException ex)
        {
            // no file found throws exception, suppressing exception output
            Debug.WriteLine(ex.Message);
        }

        return false;
    }

    /// <inheritdoc />
    public async Task<bool> FileExistsNewerAsync(SshClient sshClient, string remoteFilePath, DateTime modifiedDate)
    {
        try
        {
            if (!sshClient.IsConnected) return false;

            if (!SetSftpClient(sshClient)) return false;

            var attributes = await Task.Run(() => _sftpClient!.GetAttributes(remoteFilePath));

            if (attributes.IsRegularFile) return attributes.LastWriteTimeUtc == modifiedDate.ToUniversalTime();
        }
        catch (SftpPathNotFoundException ex)
        {
            // no file found throws exception, suppressing exception output
            Debug.WriteLine(ex.Message);
        }

        return false;
    }

    /// <inheritdoc />
    public async Task<SshClient?> GetSshClientAsync(string host, string username, string password)
    {
        try
        {
            if (string.IsNullOrEmpty(host) ||
                string.IsNullOrEmpty(username) ||
                string.IsNullOrEmpty(password))
            {
                await _outputService.WriteToOutputAsync("Mac connection options are not configured",
                    OutputPaneType.Debug);

                return null;
            }

            var connectionInfo = new ConnectionInfo(host, username,
                new PasswordAuthenticationMethod(username, password));

            return new SshClient(connectionInfo);
        }
        catch (Exception ex)
        {
            await _outputService.WriteToOutputAsync(ex.Message, OutputPaneType.Debug);
        }

        return null;
    }

    /// <inheritdoc />
    public async Task<string> GetSshFingerprintAsync(string host)
    {
        var tempPublicKeyFile = string.Empty;

        try
        {
            var assemblyLocation = PtmPackage.Instance.GetExecutingAssemblyLocation();
            var vsixDirectory = Path.GetDirectoryName(assemblyLocation);

            if (string.IsNullOrEmpty(vsixDirectory))
            {
                await _jtf.SwitchToMainThreadAsync();
                await _outputService.WriteToOutputAsync("Unable to determine the location of the VSIX extension.",
                    OutputPaneType.Debug);
                await TaskScheduler.Default;

                return string.Empty;
            }

            var sshKeyScanExe = Path.Combine(vsixDirectory, "binaries", "SSH", "ssh-keyscan.exe");
            var sshKeyGenExe = Path.Combine(vsixDirectory, "binaries", "SSH", "ssh-keygen.exe");

            if (!File.Exists(sshKeyScanExe))
            {
                await _outputService.WriteToOutputAsync("ssh-keyscan.exe cannot be located", OutputPaneType.Debug);

                return string.Empty;
            }

            if (!File.Exists(sshKeyGenExe))
            {
                await _outputService.WriteToOutputAsync("ssh-keygen.exe cannot be located", OutputPaneType.Debug);

                return string.Empty;
            }

            using var sshKeyScanProc = new Process();
            sshKeyScanProc.StartInfo = new ProcessStartInfo
            {
                FileName = sshKeyScanExe,
                Arguments = $"-t rsa {host}",
                RedirectStandardOutput = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            sshKeyScanProc.Start();
            var publicKey = await sshKeyScanProc.StandardOutput.ReadToEndAsync();
            await sshKeyScanProc.WaitForExitAsync();

            if (string.IsNullOrEmpty(publicKey))
            {
                await _outputService.WriteToOutputAsync("Failed to retrieve the public key from the remote host.",
                    OutputPaneType.Debug);

                return string.Empty;
            }

            tempPublicKeyFile = Path.GetTempFileName();
            File.WriteAllText(tempPublicKeyFile, publicKey);

            var sshKeyGenProc = new Process();
            sshKeyGenProc.StartInfo = new ProcessStartInfo
            {
                FileName = sshKeyGenExe,
                Arguments = $"-l -E md5 -f {tempPublicKeyFile}", // new format:  $"-lf -"
                RedirectStandardOutput = true,
                // RedirectStandardInput = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            sshKeyGenProc.Start();
            var footPrintResult = await sshKeyGenProc.StandardOutput.ReadToEndAsync();
            await sshKeyGenProc.WaitForExitAsync();

            if (string.IsNullOrEmpty(footPrintResult))
            {
                await _outputService.WriteToOutputAsync("Failed to retrieve the public key from the remote host.",
                    OutputPaneType.Debug);

                return string.Empty;
            }

            // 3072 MD5:84:d1:43:0c:32:15:a2:20:c2:03:3f:d4:4d:9d:84:a7 192.168.1.1 (RSA)
            return footPrintResult.Split(' ')[1].Replace("MD5:", "").ToUpper();
        }
        catch (Exception ex)
        {
            await _outputService.WriteToOutputAsync(ex.Message, OutputPaneType.Debug);
        }
        finally
        {
            if (!string.IsNullOrEmpty(tempPublicKeyFile))
                if (File.Exists(tempPublicKeyFile))
                    File.Delete(tempPublicKeyFile);
        }

        return string.Empty;
    }

    /// <inheritdoc />
    public string SanitizeMacPath(string remotePath)
    {
        return remotePath.Replace("\\", "/").Replace(":", "");
    }

    /// <summary>
    ///     Sets the SFTP client for the given SSH client.
    /// </summary>
    /// <param name="sshClient">The SSH client.</param>
    /// <returns>A boolean indicating whether the SFTP client was successfully set.</returns>
    private bool SetSftpClient(SshClient sshClient)
    {
        if (_sftpClient is null || !_sftpClient.IsConnected || _sftpClient.ConnectionInfo != sshClient.ConnectionInfo)
        {
            _sftpClient = new SftpClient(sshClient.ConnectionInfo);
            _sftpClient.Connect();

            if (!_sftpClient.IsConnected) return false;
        }

        return true;
    }

    /// <summary>
    ///     Clears the shell stream.
    /// </summary>
    /// <param name="shellStream">The shell stream to clear.</param>
    private static void ClearShellStream(ShellStream shellStream)
    {
        while (true)
        {
            var output = shellStream!.ReadLine(TimeSpan.FromSeconds(.5));
            if (output == null) break;
        }
    }
}