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

// TODO: DotnetRuntime.cs, match all current runtimes between macOS and Windows

#nullable enable

using System.Collections.Generic;
using System.Linq;

namespace Ptm.Models.Dotnet;

/// <summary>
/// Represents a .NET runtime environment.
/// </summary>
/// <remarks>
/// This class is used to store information about a specific .NET runtime, including its name, version, and installation path.
/// </remarks>
public class DotnetRuntime
{
    public string Runtime { get; set; }

    public string Version { get; set; }

    public string Path { get; set; }

    /// <summary>
    ///     Matches the .NET Core runtimes between MacOS and Windows environments.
    /// </summary>
    /// <param name="macRuntimes">List of .NET Core runtimes on macOS.</param>
    /// <param name="winRuntimes">List of .NET Core runtimes on Windows.</param>
    /// <returns>True if there is any matching runtime version between macOS and Windows; otherwise, false.</returns>
    public static bool MatchRuntimes(List<DotnetRuntime>? macRuntimes, List<DotnetRuntime>? winRuntimes)
    {
        if (macRuntimes is null || winRuntimes is null) return false;

        var remoteRuntimes = macRuntimes
            .Where(x => x.Runtime == "Microsoft.NETCore.App")
            .Select(x => x.Version)
            .ToList();

        var localRuntimes = winRuntimes
            .Where(x => x.Runtime == "Microsoft.NETCore.App")
            .Select(x => x.Version)
            .ToList();

        if (!remoteRuntimes.Any() || !localRuntimes.Any()) return false;

        return remoteRuntimes.Intersect(localRuntimes).Any();
    }
}