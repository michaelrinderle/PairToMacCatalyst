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
using Renci.SshNet;
using System.Threading.Tasks;
using System.Windows.Documents;
using Ptm.Models.Mac;
using Ptm.Enums;

namespace Ptm.Interfaces;

/// <summary>
/// Provides methods for SSH operations.
/// </summary>
public interface ISecureTransferService
{
    /// <summary>
    /// Creates a directory asynchronously.
    /// </summary>
    /// <param name="sshClient">The SSH client.</param>
    /// <param name="remoteFolderPath">The remote folder path where the directory will be created.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains a boolean indicating whether the directory was successfully created.</returns>
    Task<bool> DirectoryCreateAsync(SshClient sshClient, string remoteFolderPath);

    /// <summary>
    /// Asynchronously retrieves the home directory path for a given path.
    /// </summary>
    /// <param name="sshClient">The SSH client.</param>
    /// <param name="remoteFolderPath">The path to retrieve the home directory for.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the home directory path as a string.</returns>
    Task<string?> DirectoryExpandHomePathAsync(SshClient sshClient, string remoteFolderPath);

    /// <summary>
    /// Checks if a directory exists asynchronously.
    /// </summary>
    /// <param name="sshClient">The SSH client.</param>
    /// <param name="remoteFolderPath">The path to check for the directory.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains a boolean indicating whether the directory exists.</returns>
    Task<bool> DirectoryExistsAsync(SshClient sshClient, string remoteFolderPath);

    /// <summary>
    /// Transfers a directory asynchronously.
    /// </summary>
    /// <param name="sshClient">The SSH client.</param>
    /// <param name="localFolderPath">The local folder path to transfer.</param>
    /// <param name="remoteBasePath">The remote base path where the directory will be transferred.</param>
    /// <param name="ignoreFolders">A list of folders to ignore during the transfer. Default is null.</param>
    /// <param name="preserveWriteAttributes">Indicates whether to preserve file attributes. Default is true.</param>
    /// <param name="token">A cancellation token to observe while waiting for the task to complete. Default is null.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains a boolean indicating whether the directory was successfully transferred.</returns>
    Task<bool> DirectoryTransferAsync(SshClient sshClient, string localFolderPath, string remoteBasePath,
        List<string>? ignoreFolders = null, bool preserveWriteAttributes = true, CancellationToken? token = null);

    /// <summary>
    /// Executes an SSH command asynchronously.
    /// </summary>
    /// <param name="sshClient">The SSH client.</param>
    /// <param name="command">The command to execute.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the command output as a string.</returns>
    Task<string?> ExecuteSshCommandAsync(SshClient sshClient, string command);

    /// <summary>
    /// Executes an SSH command asynchronously and streams the output.
    /// </summary>
    /// <param name="sshClient">The SSH client.</param>
    /// <param name="command">The command to execute.</param>
    /// <param name="logOutput">The type of output pane to log the output. Default is null.</param>
    /// <param name="outputMatch">An array of strings to match in the output. Default is null.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains a boolean indicating whether the command was successfully executed.</returns>
    Task<bool> ExecuteSshCommandStreamAsync(SshClient sshClient, string command, OutputPaneType? logOutput = null, string[]? outputMatch = null);

    /// <summary>
    /// Creates a file asynchronously.
    /// </summary>
    /// <param name="sshClient">The SSH client.</param>
    /// <param name="remoteFilePath">The remote file path where the file will be created.</param>
    /// <param name="fileName">The name of the file to create.</param>
    /// <param name="preserveWriteAttributes">Indicates whether to preserve file attributes. Default is true.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains a boolean indicating whether the file was successfully created.</returns>
    Task<bool> FileCreateAsync(SshClient sshClient, string remoteFilePath, string fileName, bool preserveWriteAttributes = true);

    /// <summary>
    /// Checks if a file exists asynchronously.
    /// </summary>
    /// <param name="sshClient">The SSH client.</param>
    /// <param name="remoteFilePath">The remote file path to check for the file.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains a boolean indicating whether the file exists.</returns>
    Task<bool> FileExistsAsync(SshClient sshClient, string remoteFilePath);

    /// <summary>
    /// Checks if a file exists and is newer than the specified date asynchronously.
    /// </summary>
    /// <param name="sshClient">The SSH client.</param>
    /// <param name="remoteFilePath">The remote file path to check for the file.</param>
    /// <param name="modifiedDate">The date to compare the file's modified date against.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains a boolean indicating whether the file exists and is newer than the specified date.</returns>
    Task<bool> FileExistsNewerAsync(SshClient sshClient, string remoteFilePath, DateTime modifiedDate);

    /// <summary>
    /// Gets the SSH client asynchronously.
    /// </summary>
    /// <param name="host">The SSH host.</param>
    /// <param name="username">The SSH username.</param>
    /// <param name="password">The SSH password.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the SSH client.</returns>
    Task<SshClient?> GetSshClientAsync(string host, string username, string password);

    /// <summary>
    /// Scans the SSH fingerprint asynchronously.
    /// </summary>
    /// <param name="host">The SSH host.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the SSH fingerprint as a string.</returns>
    Task<string> GetSshFingerprintAsync(string host);

    /// <summary>
    /// Sanitizes the given remote path for Mac.
    /// </summary>
    /// <param name="remotePath">The remote path to sanitize.</param>
    /// <returns>The sanitized remote path as a string.</returns>
    string SanitizeMacPath(string remotePath);
}