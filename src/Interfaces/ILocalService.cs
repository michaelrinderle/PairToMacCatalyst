﻿/*
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

using System.Collections.Generic;
using System.Threading.Tasks;
using Ptm.Models.Dotnet;

namespace Ptm.Interfaces;

/// <summary>
///     Interface for local service operations.
/// </summary>
public interface ILocalService
{
    /// <summary>
    ///     Executes a shell command asynchronously.
    /// </summary>
    /// <param name="command"></param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the output of the command.</returns>
    Task<string?> ExecuteShellCommandAsync(string command);

    /// <summary>
    ///     Verifies the local .NET runtime asynchronously.
    /// </summary>
    /// <returns>A task that represents the asynchronous operation. The task result contains a list of .NET runtimes.</returns>
    Task<List<DotnetRuntime>?> VerifyLocalDotnetRuntimesAsync();

    /// <summary>
    ///     Verifies the local .NET runtime asynchronously.
    /// </summary>
    /// <returns>A task that represents the asynchronous operation. The task result contains a list of .NET workloads.</returns>
    Task<List<DotnetWorkload>?> VerifyLocalDotnetWorkloadsAsync();
}