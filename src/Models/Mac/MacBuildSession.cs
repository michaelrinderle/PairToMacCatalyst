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

using System;
using System.Collections.Generic;
using EnvDTE;
using Project = Community.VisualStudio.Toolkit.Project;

namespace Ptm.Models.Mac;

/// <summary>
/// Represents a build session for a Mac project, including details about the build process,
/// such as start and end times, success status, and project references.
/// </summary>
public class MacBuildSession
{
    public DateTime? BuildEnded { get; set; }

    public DateTime? BuildStarted { get; set; }

    public bool BuildSuccessful = false;

    public bool IsActive { get; set; }

    public string MauiProjectPath { get; set; }

    public string MacCatalystVersion { get; set; }

    public List<string> ReferencesPaths { get; set; } = new();

    public Guid SessionId { get; set; }

    /// <summary>
    /// Resets the current build session, initializing all properties to their default values.
    /// </summary>
    public void ResetSession()
    {
        BuildStarted = DateTime.Now;
        BuildEnded = null;
        BuildSuccessful = false;
        IsActive = false;
        ReferencesPaths.Clear();
        SessionId = Guid.NewGuid();
    }
}