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
using System.ComponentModel.Composition;
using System.Linq;
using System.Threading.Tasks;
using EnvDTE;
using EnvDTE80;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using Ptm.Enums;
using Ptm.Interfaces;
using Ptm.Mappers;

namespace Ptm.Services;

/// <inheritdoc />
[Export(typeof(IOutputService))]
[PartCreationPolicy(CreationPolicy.Shared)]
public class OutputService
    : IOutputService
{
    private static readonly object Lock = new();

    private readonly IVsOutputWindow _outputWindow;
    private readonly IVsStatusbar _statusBar;

    private IVsOutputWindowPane _buildOutputPane;
    private IVsOutputWindowPane _debugOutputPane;

    [ImportingConstructor]
    public OutputService()
    {
        _outputWindow = ServiceProvider.GlobalProvider.GetService(typeof(SVsOutputWindow)) as IVsOutputWindow;
        if (_outputWindow == null) throw new InvalidOperationException("Cannot get SVsOutputWindow service.");

        _statusBar = ServiceProvider.GlobalProvider.GetService(typeof(SVsStatusbar)) as IVsStatusbar;
        if (_statusBar == null) throw new InvalidOperationException("Cannot get SVsStatusbar service.");
    }

    /// <inheritdoc />
    public async Task WriteToOutputAsync(string message, OutputPaneType outputType)
    {
        await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();

        var outputPane = await GetOutputPaneAsync(outputType);

        outputPane?.OutputStringThreadSafe(message);
        outputPane?.Activate();
    }

    /// <inheritdoc />
    public async Task WriteToStatusBarAsync(string message)
    {
        await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();

        _statusBar.SetText(message);
    }

    /// <inheritdoc />
    public async Task ClearOutputAsync(OutputPaneType outputType)
    {
        await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();

        var outputPane = await GetOutputPaneAsync(outputType);
        outputPane?.Clear();
        outputPane?.Activate();
    }

    /// <inheritdoc />
    public async Task ClearStatusBarAsync()
    {
        await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();

        _statusBar.Clear();
    }

    /// <inheritdoc />
    public async Task<string> GetOutputTextAsync(OutputPaneType outputType)
    {
        await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();

        var painName = outputType == OutputPaneType.Build ? "Build" : "Debug";

        var dte = Package.GetGlobalService(typeof(DTE)) as DTE2;

        var outputWindow = dte.ToolWindows.OutputWindow;

        var buildPane = outputWindow.OutputWindowPanes
            .Cast<OutputWindowPane>()
            .FirstOrDefault(pane => pane.Name == painName);

        if (buildPane == null) return string.Empty;

        var doc = buildPane.TextDocument;
        var editPoint = doc.StartPoint.CreateEditPoint();

        return editPoint.GetText(doc.EndPoint);
    }

    /// <summary>
    ///     Retrieves the output pane asynchronously based on the specified output type.
    /// </summary>
    /// <param name="outputType">The type of output (Build or Debug).</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the output window pane.</returns>
    private async Task<IVsOutputWindowPane> GetOutputPaneAsync(OutputPaneType outputType)
    {
        await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();

        var paneGuid = OutputTypeMapper.GetGuid(outputType);
        IVsOutputWindowPane outputPane = null;

        if (outputType == OutputPaneType.Build)
        {
            if (_buildOutputPane == null)
                lock (Lock)
                {
                    if (_buildOutputPane == null) _outputWindow.GetPane(ref paneGuid, out _buildOutputPane);
                }

            outputPane = _buildOutputPane;
        }
        else if (outputType == OutputPaneType.Debug)
        {
            if (_debugOutputPane == null)
                lock (Lock)
                {
                    if (_debugOutputPane == null) _outputWindow.GetPane(ref paneGuid, out _debugOutputPane);
                }

            outputPane = _debugOutputPane;
        }

        return outputPane;
    }
}