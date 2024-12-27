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
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.VisualStudio.Threading;
using Ptm.Constants;
using Ptm.Enums;
using Ptm.Interfaces;
using Ptm.Models.Dotnet;

namespace Ptm.Services;

/// <inheritdoc />
[Export(typeof(ILocalService))]
[PartCreationPolicy(CreationPolicy.Shared)]
[method: ImportingConstructor]
public class LocalService(
    IOutputService outputService)
    : ILocalService
{
    private readonly IOutputService _outputService = outputService;

    /// <inheritdoc />
    public async Task<string?> ExecuteShellCommandAsync(string command)
    {
        try
        {
            var procStartInfo = new ProcessStartInfo
            {
                FileName = "powershell.exe",
                Arguments = $"-Command \"{command}\"",
                RedirectStandardOutput = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            using var proc = Process.Start(procStartInfo);
            if (proc is null) return null;

            var output = await proc.StandardOutput.ReadToEndAsync();
            await proc.WaitForExitAsync();

            return output;
        }
        catch (Exception ex)
        {
            await _outputService.WriteToOutputAsync(ex.Message, OutputPaneType.Debug);
        }

        return null;
    }

    /// <inheritdoc />
    public async Task<List<DotnetRuntime>?> VerifyLocalDotnetRuntimesAsync()
    {
        List<DotnetRuntime> runtimes = [];

        try
        {
            var output = await ExecuteShellCommandAsync(ShellCommandConstants.GetDotnetRuntimes);

            var lines = output?.Split(['\r', '\n'], StringSplitOptions.RemoveEmptyEntries);

            foreach (var line in lines)
            {
                // disregard aspnetcore runtime
                if (line.Contains("Microsoft.AspNetCore.App")) continue;
                // disregard windows desktop runtime
                if (line.Contains("Microsoft.WindowsDesktop.App")) continue;
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
            await _outputService.WriteToOutputAsync(ex.Message, OutputPaneType.Debug);
        }

        return null;
    }

    /// <inheritdoc />
    public async Task<List<DotnetWorkload>?> VerifyLocalDotnetWorkloadsAsync()
    {
        List<DotnetWorkload> workloads = [];

        try
        {
            var output = await ExecuteShellCommandAsync(ShellCommandConstants.GetDotnetWorkloads);

            var lines = output?.Split(['\r', '\n'], StringSplitOptions.RemoveEmptyEntries);

            foreach (var line in lines)
            {
                if (!line.Contains("maui")) continue;

                // windows needs 'maui-windows', 'android', 'ios', & 'maccatalyst'
                // will match 'maui-windows' manifest version for mismatch

                Regex regex = new(@"\s{2,}");
                var parts = regex.Split(line!, 3);

                workloads.Add(new DotnetWorkload
                {
                    Id = parts![0],
                    ManifestVersion = parts[1],
                    InstallationSource = parts[2]
                });
            }

            return workloads;
        }
        catch (Exception ex)
        {
            await _outputService.WriteToOutputAsync(ex.Message, OutputPaneType.Debug);
        }

        return null;
    }
}