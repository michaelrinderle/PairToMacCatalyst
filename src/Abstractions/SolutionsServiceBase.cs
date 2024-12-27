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

using EnvDTE;
using EnvDTE80;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell;
using Ptm.Enums;
using System;
using System.Threading.Tasks;

namespace Ptm.Abstractions;

/// <summary>
///     Base class for handling solution events in Visual Studio.
/// </summary>
/// <remarks>
///     This abstract class provides the foundation for managing various solution events such as build, clean, debug,
///     deploy, and rebuild.
///     It implements the <see cref="IDisposable" /> interface to ensure proper cleanup of event handlers.
/// </remarks>
public abstract class SolutionsServiceBase
    : IDisposable
{
    private Events2? _dteEvents;

    private CommandEvents? _buildSelEvents;
    private CommandEvents? _buildSlnEvents;
    private CommandEvents? _cancelBuildEvents;
    private CommandEvents? _cancelEvents;
    private CommandEvents? _cleanSelEvents;
    private CommandEvents? _cleanSlnEvents;
    private CommandEvents? _debugEvents;
    private CommandEvents? _deploySelEvents;
    private CommandEvents? _deploySlnEvents;
    private CommandEvents? _rebuildSelEvents;
    private CommandEvents? _rebuildSlnEvents;

    /// <summary>
    ///     Gets or sets the type of the command event.
    /// </summary>
    public CommandEventType? CommandEvent;

    /// <summary>
    ///     Disposes the resources used by the <see cref="SolutionsServiceBase" /> class.
    /// </summary>
    public void Dispose()
    {
        ThreadHelper.ThrowIfNotOnUIThread();

        if (_buildSelEvents is not null)
        {
            _buildSelEvents.BeforeExecute -= BuildEvent_BeforeExecute;
            _buildSelEvents = null;
        }

        if (_buildSlnEvents is not null)
        {
            _buildSlnEvents.BeforeExecute -= BuildEvent_BeforeExecute;
            _buildSlnEvents = null;
        }

        if (_cancelEvents is not null)
        {
            _cancelEvents.BeforeExecute -= CancelEvent_BeforeExecute;
            _cancelEvents = null;
        }

        if (_cancelBuildEvents is not null)
        {
            _cancelBuildEvents.BeforeExecute -= CancelEvent_BeforeExecute;
            _cancelBuildEvents = null;
        }

        if (_cleanSelEvents is not null)
        {
            _cleanSelEvents.BeforeExecute -= CleanEvent_BeforeExecute;
            _cleanSelEvents = null;
        }

        if (_cleanSlnEvents is not null)
        {
            _cleanSlnEvents.BeforeExecute -= CleanEvent_BeforeExecute;
            _cleanSlnEvents = null;
        }

        if (_debugEvents is not null)
        {
            _debugEvents.BeforeExecute -= CleanEvent_BeforeExecute;
            _debugEvents = null;
        }

        if (_deploySelEvents is not null)
        {
            _deploySelEvents.BeforeExecute -= DeployEvent_BeforeExecute;
            _debugEvents = null;
        }

        if (_deploySlnEvents is not null)
        {
            _deploySlnEvents.BeforeExecute -= DeployEvent_BeforeExecute;
            _deploySlnEvents = null;
        }

        if (_rebuildSelEvents != null)
        {
            _rebuildSelEvents.BeforeExecute -= RebuildEvent_BeforeExecute;
            _rebuildSelEvents = null;
        }

        if (_rebuildSlnEvents != null)
        {
            _rebuildSlnEvents.BeforeExecute -= RebuildEvent_BeforeExecute;
            _rebuildSlnEvents = null;
        }

        _dteEvents = null;
    }

    /// <summary>
    ///     Registers solution events asynchronously.
    /// </summary>
    /// <param name="dte">The DTE2 object representing the Visual Studio environment.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    /// <remarks>
    ///     This method registers various solution events such as build, clean, debug, deploy, and rebuild.
    ///     It ensures that the event handlers are properly set up to handle these events.
    /// </remarks>
    protected async Task RegisterSolutionEventsAsync(DTE2 dte)
    {
        await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();

        _dteEvents = dte.Events as Events2;

        _buildSelEvents = _dteEvents!.CommandEvents[
            VSConstants.GUID_VSStandardCommandSet97.ToString("B"),
            (int)VSConstants.VSStd97CmdID.BuildSel];

        _buildSelEvents.BeforeExecute += BuildEvent_BeforeExecute;

        _buildSlnEvents = _dteEvents!.CommandEvents[
            VSConstants.GUID_VSStandardCommandSet97.ToString("B"),
            (int)VSConstants.VSStd97CmdID.BuildSln];

        _buildSlnEvents.BeforeExecute += BuildEvent_BeforeExecute;

        _cancelEvents = _dteEvents!.CommandEvents[
            VSConstants.GUID_VSStandardCommandSet97.ToString("B"),
            (int)VSConstants.VSStd97CmdID.CancelBuild];

        _cancelEvents.BeforeExecute += CancelEvent_BeforeExecute;

        _cancelBuildEvents = _dteEvents!.CommandEvents[
            VSConstants.GUID_VSStandardCommandSet97.ToString("B"),
            (int)VSConstants.VSStd97CmdID.CancelBuild];

        _cancelBuildEvents.BeforeExecute += CancelEvent_BeforeExecute;

        _cleanSelEvents = _dteEvents!.CommandEvents[
            VSConstants.GUID_VSStandardCommandSet97.ToString("B"),
            (int)VSConstants.VSStd97CmdID.CleanSel];

        _cleanSelEvents.BeforeExecute += CleanEvent_BeforeExecute;

        _cleanSlnEvents = _dteEvents!.CommandEvents[
            VSConstants.GUID_VSStandardCommandSet97.ToString("B"),
            (int)VSConstants.VSStd97CmdID.CleanSln];

        _cleanSlnEvents.BeforeExecute += CleanEvent_BeforeExecute;

        _debugEvents = _dteEvents!.CommandEvents[
            VSConstants.GUID_VSStandardCommandSet97.ToString("B"),
            (int)VSConstants.VSStd97CmdID.Start];

        _debugEvents.BeforeExecute += DebugEvent_BeforeExecute;

        _deploySelEvents = _dteEvents!.CommandEvents[
            VSConstants.GUID_VSStandardCommandSet97.ToString("B"),
            (int)VSConstants.VSStd97CmdID.DeploySel];

        _deploySelEvents.BeforeExecute += DebugEvent_BeforeExecute;

        _deploySlnEvents = _dteEvents!.CommandEvents[
            VSConstants.GUID_VSStandardCommandSet97.ToString("B"),
            (int)VSConstants.VSStd97CmdID.DeploySln];

        _deploySlnEvents.BeforeExecute += DeployEvent_BeforeExecute;

        _rebuildSelEvents = _dteEvents!.CommandEvents[
            VSConstants.GUID_VSStandardCommandSet97.ToString("B"),
            (int)VSConstants.VSStd97CmdID.RebuildSel];

        _rebuildSelEvents.BeforeExecute += RebuildEvent_BeforeExecute;

        _rebuildSlnEvents = _dteEvents!.CommandEvents[
            VSConstants.GUID_VSStandardCommandSet97.ToString("B"),
            (int)VSConstants.VSStd97CmdID.RebuildSln];

        _rebuildSlnEvents.BeforeExecute += RebuildEvent_BeforeExecute;
    }

    /// <summary>
    ///     Handles the event that occurs before a build command is executed.
    /// </summary>
    /// <param name="Guid">The GUID of the command group.</param>
    /// <param name="ID">The ID of the command.</param>
    /// <param name="CustomIn">Custom input parameter for the command.</param>
    /// <param name="CustomOut">Custom output parameter for the command.</param>
    /// <param name="CancelDefault">A boolean value indicating whether to cancel the default execution of the command.</param>
    private void BuildEvent_BeforeExecute(string Guid, int ID, object CustomIn, object CustomOut,
        ref bool CancelDefault)
    {
        CommandEvent = CommandEventType.Build;
    }

    /// <summary>
    ///     Handles the event that occurs before a cancel command is executed.
    /// </summary>
    /// <param name="Guid">The GUID of the command group.</param>
    /// <param name="ID">The ID of the command.</param>
    /// <param name="CustomIn">Custom input parameter for the command.</param>
    /// <param name="CustomOut">Custom output parameter for the command.</param>
    /// <param name="CancelDefault">A boolean value indicating whether to cancel the default execution of the command.</param>
    protected abstract void CancelEvent_BeforeExecute(string Guid, int ID, object CustomIn, object CustomOut,
        ref bool CancelDefault);

    /// <summary>
    ///     Handles the event that occurs before a clean command is executed.
    /// </summary>
    /// <param name="Guid">The GUID of the command group.</param>
    /// <param name="ID">The ID of the command.</param>
    /// <param name="CustomIn">Custom input parameter for the command.</param>
    /// <param name="CustomOut">Custom output parameter for the command.</param>
    /// <param name="CancelDefault">A boolean value indicating whether to cancel the default execution of the command.</param>
    private void CleanEvent_BeforeExecute(string Guid, int ID, object CustomIn, object CustomOut,
        ref bool CancelDefault)
    {
        CommandEvent = CommandEventType.Clean;
    }

    /// <summary>
    ///     Handles the event that occurs before a debug command is executed.
    /// </summary>
    /// <param name="Guid">The GUID of the command group.</param>
    /// <param name="ID">The ID of the command.</param>
    /// <param name="CustomIn">Custom input parameter for the command.</param>
    /// <param name="CustomOut">Custom output parameter for the command.</param>
    /// <param name="CancelDefault">A boolean value indicating whether to cancel the default execution of the command.</param>
    protected abstract void DebugEvent_BeforeExecute(string Guid, int ID, object CustomIn, object CustomOut,
        ref bool CancelDefault);

    /// <summary>
    ///     Handles the event that occurs before a deploy command is executed.
    /// </summary>
    /// <param name="Guid">The GUID of the command group.</param>
    /// <param name="ID">The ID of the command.</param>
    /// <param name="CustomIn">Custom input parameter for the command.</param>
    /// <param name="CustomOut">Custom output parameter for the command.</param>
    /// <param name="CancelDefault">A boolean value indicating whether to cancel the default execution of the command.</param>
    private void DeployEvent_BeforeExecute(string Guid, int ID, object CustomIn, object CustomOut,
        ref bool CancelDefault)
    {
        CommandEvent = CommandEventType.Deploy;
    }

    /// <summary>
    ///     Handles the event that occurs before a rebuild command is executed.
    /// </summary>
    /// <param name="Guid">The GUID of the command group.</param>
    /// <param name="ID">The ID of the command.</param>
    /// <param name="CustomIn">Custom input parameter for the command.</param>
    /// <param name="CustomOut">Custom output parameter for the command.</param>
    /// <param name="CancelDefault">A boolean value indicating whether to cancel the default execution of the command.</param>
    private void RebuildEvent_BeforeExecute(string Guid, int ID, object CustomIn, object CustomOut,
        ref bool CancelDefault)
    {
        CommandEvent = CommandEventType.Rebuild;
    }
}