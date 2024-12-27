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

using System.Collections.Generic;
using System.Linq;

namespace Ptm.Models.Dotnet;

/// <summary>
///     Represents a .NET workload with its associated properties.
/// </summary>
public class DotnetWorkload
{
    public string Id { get; set; }

    public string ManifestVersion { get; set; }

    public string InstallationSource { get; set; }

    /// <summary>
    ///     Determines if the MAUI workloads match between macOS and Windows.
    /// </summary>
    /// <param name="macWorkloads">The list of workloads for macOS.</param>
    /// <param name="winWorkloads">The list of workloads for Windows.</param>
    /// <returns>True if there is a matching MAUI workload version; otherwise, false.</returns>
    public static bool MatchMauiWorkLoads(List<DotnetWorkload>? macWorkloads, List<DotnetWorkload>? winWorkloads)
    {
        if (macWorkloads is null || winWorkloads is null) return false;

        var remoteMauiVersions = macWorkloads
            .Where(x => x.Id == "maui")
            .Select(x => x.ManifestVersion)
            .ToList();

        var localMauiVersions = winWorkloads
            .Where(x => x.Id == "maui-windows")
            .Select(x => x.ManifestVersion)
            .ToList();

        if (!remoteMauiVersions.Any() || !localMauiVersions.Any()) return false;

        return remoteMauiVersions.Intersect(localMauiVersions).Any();
    }
}