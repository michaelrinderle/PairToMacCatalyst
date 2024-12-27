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

using EnvDTE80;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.Threading;
using Ptm.Abstractions;
using Ptm.Enums;
using Ptm.Interfaces;
using Ptm.Models.Mac;
using System;
using System.ComponentModel.Composition;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Ptm.Services;

/// <inheritdoc cref="ISolutionsService" />
[Export(typeof(ISolutionsService))]
[PartCreationPolicy(CreationPolicy.Shared)]
public class SolutionsService
    : SolutionsServiceBase, ISolutionsService, IVsUpdateSolutionEvents2, IVsDebuggerEvents
{
    private readonly IMacBridgeService _macBridgeService;
    private readonly IOutputService _outputService;

    private readonly IVsDebugger? _debugger;
    private readonly IVsSolutionBuildManager? _solutionBuildManager;

    private DTE2? _dte;
    private uint _debuggerEventCookie;
    private uint _solutionBuildManagerCookie;

    private bool _isMauiProjectInSolution = false;

    /// <summary>
    /// Initializes a new instance of the <see cref="SolutionsService"/> class.
    /// </summary>
    /// <param name="macBridgeService">The Mac bridge service.</param>
    /// <param name="outputService">The output service.</param>
    [ImportingConstructor]
    public SolutionsService(
        IMacBridgeService macBridgeService,
        IOutputService outputService)
    {
        _macBridgeService = macBridgeService;
        _outputService = outputService;

        ThreadHelper.ThrowIfNotOnUIThread();

        _solutionBuildManager = ServiceProvider.GlobalProvider
            .GetService(typeof(SVsSolutionBuildManager)) as IVsSolutionBuildManager2;
        _solutionBuildManager?.AdviseUpdateSolutionEvents(this, out _solutionBuildManagerCookie);

        _debugger = ServiceProvider.GlobalProvider.GetService(typeof(SVsShellDebugger)) as IVsDebugger;
        _debugger?.AdviseDebuggerEvents(this, out _debuggerEventCookie);
    }

    /// <summary>
    /// Disposes the service and unadvises the solution and debugger events.
    /// </summary>
    public new void Dispose()
    {
        ThreadHelper.ThrowIfNotOnUIThread();

        _debugger?.UnadviseDebuggerEvents(_debuggerEventCookie);
        _solutionBuildManager?.UnadviseUpdateSolutionEvents(_solutionBuildManagerCookie);

        _debuggerEventCookie = 0;
        _solutionBuildManagerCookie = 0;

        base.Dispose();
    }

    /// <inheritdoc />
    public async Task InitializeAsync(DTE2 dte)
    {
        await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();

        _dte = dte;

        _isMauiProjectInSolution = await IsMauiMacCatalystProjectInSolutionAsync();

        if (_isMauiProjectInSolution)
            await RegisterSolutionEventsAsync(_dte);
    }

    /// <summary>
    /// Retrieves the Mac Catalyst version from the specified project file asynchronously.
    /// </summary>
    /// <param name="projectPath">The path to the project file.</param>
    /// <returns>
    /// A task that represents the asynchronous operation. The task result contains the Mac Catalyst version
    /// if found; otherwise, null.
    /// </returns>
    private async Task<string?> GetMacCatalystVersionAsync(string projectPath)
    {
        try
        {
            if (!File.Exists(projectPath)) return null;

            using var stream = File.OpenRead(projectPath);

            var csproj = XDocument.Load(stream);
            var propertyGroups = csproj.Root?.Elements("PropertyGroup");

            if (propertyGroups is null) return null;

            // Maui attributes should always be in the first property group
            // but checking all property groups for pertinent elements
            foreach (var propertyGroup in propertyGroups)
            {
                var targetFrameworkElement = propertyGroup.Element("TargetFramework");
                var targetFrameworksElement = propertyGroup.Element("TargetFrameworks");

                // check if <TargetFramework> includes net*-maccatalyst
                if (targetFrameworkElement is not null)
                {
                    if (targetFrameworkElement.Value.Contains("maccatalyst"))
                        return targetFrameworkElement.Value;
                }

                // check if <TargetFrameworks> includes net*-maccatalyst
                if (targetFrameworksElement is not null)
                {
                    if (targetFrameworksElement.Value.Contains("maccatalyst"))
                    {
                        var targets = targetFrameworksElement.Value.Split(';');

                        return targets.FirstOrDefault(x => x.Contains("maccatalyst"));
                    }
                }

                return null;
            }
        }
        catch (Exception ex)
        {
            await _outputService.WriteToOutputAsync(ex.Message, OutputPaneType.Build);
        }

        return null;
    }

    /// <summary>
    /// Initializes the Mac build session asynchronously.
    /// </summary>
    /// <returns>
    /// A task that represents the asynchronous operation. The task result contains a boolean value indicating whether
    /// the session was initialized successfully.
    /// </returns>
    private async Task<bool> InitializeSessionAsync()
    {
        try
        {
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();

            _macBridgeService.MacBuildSession!.IsActive = true;

            var startupProjects = (Array)_dte.Solution.SolutionBuild.StartupProjects;
            if (startupProjects.Length == 0) return false;

            var project = _dte.Solution.Projects
                .Cast<EnvDTE.Project>()
                .FirstOrDefault(x => x.UniqueName == startupProjects.GetValue(0));

            if (project!.Object is not VSLangProj.VSProject vsProject) return true;

            _macBridgeService.MacBuildSession.MauiProjectPath = vsProject.Project.FileName;
            _macBridgeService.MacBuildSession.MacCatalystVersion = await GetMacCatalystVersionAsync(vsProject.Project.FileName);

            foreach (VSLangProj.Reference reference in vsProject.References)
                if (reference.SourceProject != null)
                    _macBridgeService.MacBuildSession.ReferencesPaths.Add(reference.SourceProject.FileName);

            return true;
        }
        catch (Exception ex)
        {
            _macBridgeService.MacBuildSession!.IsActive = false;
            await _outputService.WriteToOutputAsync(ex.Message, OutputPaneType.Debug);
        }

        return false;
    }

    /// <summary>
    /// Determines if the active debug profile is set to "Mac Catalyst" and if the target frameworks include "maccatalyst".
    /// </summary>
    /// <returns>
    /// A task that represents the asynchronous operation. The task result contains a boolean value indicating whether
    /// the active debug profile is "Mac Catalyst" and the target frameworks include "maccatalyst".
    /// </returns>
    private async Task<bool> IsMauiMacCatalystActiveDebugProfileAsync()
    {
        try
        {
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();

            if (_dte?.Solution.SolutionBuild.StartupProjects is not Array { Length: > 0 } startupProjects) return false;

            var project = _dte.Solution.Projects
                .Cast<EnvDTE.Project>()
                .FirstOrDefault(x => x.UniqueName == startupProjects.GetValue(0).ToString());

            if (project is null) return false;

            var debugProfile = project.Properties.Item("ActiveDebugProfile")?.Value?.ToString();
            var targetFrameworks = project.Properties.Item("TargetFrameworks")?.Value?.ToString();

            if (string.IsNullOrEmpty(debugProfile) || string.IsNullOrEmpty(targetFrameworks)) return false;

            return debugProfile!.Equals("Mac Catalyst", StringComparison.OrdinalIgnoreCase) &&
                   targetFrameworks!.IndexOf("maccatalyst", StringComparison.OrdinalIgnoreCase) >= 0;
        }
        catch (Exception ex)
        {
            await _outputService.WriteToOutputAsync(ex.Message, OutputPaneType.Build);
        }

        return false;
    }

    /// <summary>
    /// Determines if the specified project is a MAUI Mac Catalyst project.
    /// </summary>
    /// <param name="projectPath">The path to the project file.</param>
    /// <returns>
    /// A task that represents the asynchronous operation. The task result contains a boolean value indicating whether
    /// the project is a MAUI Mac Catalyst project.
    /// </returns>
    private async Task<bool> IsMauiMacCatalystProjectAsync(string projectPath)
    {
        try
        {
            if (!File.Exists(projectPath)) return false;

            bool useMauiFlag = false, outputTypeFlag = false, maccatalyst = false;

            using var stream = File.OpenRead(projectPath);

            var csproj = XDocument.Load(stream);
            var propertyGroups = csproj.Root?.Elements("PropertyGroup");

            if (propertyGroups is null) return false;

            // Maui attributes should always be in the first property group
            // but checking all property groups for pertinent elements
            foreach (var propertyGroup in propertyGroups)
            {
                var useMauiElement = propertyGroup.Element("UseMaui");
                var outputTypeElement = propertyGroup.Element("OutputType");
                var targetFrameworkElement = propertyGroup.Element("TargetFramework");
                var targetFrameworksElement = propertyGroup.Element("TargetFrameworks");

                // check is <UseMaui> is true
                if (useMauiElement is not null)
                    if (!useMauiFlag)
                        useMauiFlag = useMauiElement.Value.ToLower() == "true";

                // check if <OutputType> is Exe
                if (outputTypeElement is not null)
                    if (!outputTypeFlag)
                        outputTypeFlag = outputTypeElement.Value == "Exe";

                // check if <TargetFramework> includes net*-maccatalyst
                if (targetFrameworkElement is not null)
                    if (!maccatalyst)
                        maccatalyst = targetFrameworkElement.Value.Contains("maccatalyst");

                // check if <TargetFrameworks> includes net*-maccatalyst
                if (targetFrameworksElement is not null)
                    if (!maccatalyst)
                        maccatalyst = targetFrameworksElement.Value.Contains("maccatalyst");
            }

            if (useMauiFlag && outputTypeFlag && maccatalyst) return true;
        }
        catch (Exception ex)
        {
            await _outputService.WriteToOutputAsync(ex.Message, OutputPaneType.Build);
        }

        return false;
    }

    /// <summary>
    /// Determines if there is a MAUI Mac Catalyst project in the solution.
    /// </summary>
    /// <returns>
    /// A task that represents the asynchronous operation. The task result contains a boolean value indicating whether
    /// there is a MAUI Mac Catalyst project in the solution.
    /// </returns>
    private async Task<bool> IsMauiMacCatalystProjectInSolutionAsync()
    {
        try
        {
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();

            var projects = _dte.Solution.Projects;

            if (projects is null || projects.Count == 0) return false;

            foreach (EnvDTE.Project project in projects)
                if (await IsMauiMacCatalystProjectAsync(project.FullName)) return true;
        }
        catch (Exception ex)
        {
            await _outputService.WriteToOutputAsync(ex.Message, OutputPaneType.Build);
        }

        return false;
    }

    #region Base

    /// <inheritdoc />
    protected override void CancelEvent_BeforeExecute(string Guid, int ID, object CustomIn, object CustomOut, ref bool CancelDefault)
    {
        if (_macBridgeService.OperationTokenSource is not null &&
           !_macBridgeService.OperationTokenSource.Token.IsCancellationRequested)
            _macBridgeService.OperationTokenSource.Cancel();
    }

    /// <inheritdoc />
    protected override void DebugEvent_BeforeExecute(string Guid, int ID, object CustomIn, object CustomOut, ref bool CancelDefault)
    {
        var cancel = CancelDefault;

        ThreadHelper.JoinableTaskFactory.RunAsync(async () =>
        {
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();

            if (await IsMauiMacCatalystActiveDebugProfileAsync())
            {
                // cancelling VS22 default debug 'Mac Catalyst' \ 'MACCATLYST' command

                CommandEvent = CommandEventType.Debug;
                cancel = true;

                ThreadHelper.JoinableTaskFactory.RunAsync(async () =>
                {
                    await Task.Delay(TimeSpan.FromSeconds(1.5));

                    // since debug is cancelled, start build and pick up on launch flag in UpdateProjectCfg_Begin
                    _dte.Solution.SolutionBuild.Build(true);

                }).FireAndForget();
            }
        }).GetAwaiter().GetResult();

        CancelDefault = cancel;
    }

    #endregion Base

    #region IVsUpdateSolutionEvents

    /// <inheritdoc />
    int IVsUpdateSolutionEvents.UpdateSolution_Begin(ref int pfCancelUpdate)
    {
        if (!_isMauiProjectInSolution) return VSConstants.S_OK;

        ThreadHelper.JoinableTaskFactory.RunAsync(async () =>
        {
            _macBridgeService.MacBuildSession.ResetSession();

            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();

            if (await IsMauiMacCatalystActiveDebugProfileAsync())
            {
                if (_macBridgeService.IsConnected &&
                    _macBridgeService.MacBuildEnv!.IsEnvironmentVerified)
                {
                    _macBridgeService.OperationTokenSource = new();
                    await InitializeSessionAsync();
                }
            }
        }).GetAwaiter().GetResult();

        return VSConstants.S_OK;
    }

    /// <inheritdoc />
    int IVsUpdateSolutionEvents2.UpdateSolution_Done(int fSucceeded, int fModified, int fCancelCommand)
    {
        CommandEvent = null;

        return VSConstants.S_OK;
    }

    /// <inheritdoc />
    int IVsUpdateSolutionEvents2.UpdateSolution_StartUpdate(ref int pfCancelUpdate) => VSConstants.S_OK;

    /// <inheritdoc />
    int IVsUpdateSolutionEvents2.UpdateSolution_Cancel() => VSConstants.S_OK;

    /// <inheritdoc />
    int IVsUpdateSolutionEvents2.OnActiveProjectCfgChange(IVsHierarchy pIVsHierarchy) => VSConstants.S_OK;

    /// <inheritdoc />
    public int UpdateProjectCfg_Begin(IVsHierarchy pHierProj, IVsCfg pCfgProj, IVsCfg pCfgSln, uint dwAction, ref int pfCancel)
    {
        if (!_isMauiProjectInSolution) return VSConstants.S_OK;

        var cancel = pfCancel;

        Task.Run(async () =>
        {
            try
            {
                if (_macBridgeService.MacBuildSession.IsActive) //_currentSession.IsActive)
                {
                    if (pHierProj is IVsProject vsProject)
                    {
                        vsProject.GetMkDocument(VSConstants.VSITEMID_ROOT, out string projectPath);

                        if (_macBridgeService.MacBuildSession.MauiProjectPath.Equals(projectPath))
                        {
                            bool operationResult = CommandEvent switch
                            {
                                CommandEventType.Build =>
                                    await _macBridgeService.StartBuildOperationAsync(),
                                CommandEventType.Clean =>
                                    await _macBridgeService.StartCleanOperationAsync(),
                                CommandEventType.Debug =>
                                    await _macBridgeService.StartDebugOperationAsync(),
                                // CommandEventType.Deploy => await _macBridgeService.StartBuildAsync(),
                                CommandEventType.Rebuild =>
                                    await _macBridgeService.StartRebuildOperationAsync(),
                                _ => false
                            };

                            cancel = operationResult ? 0 : 1;
                        }
                    }
                }
            }
            catch (OperationCanceledException)
            {
                System.Diagnostics.Debug.WriteLine("Operation was cancelled");

                cancel = 1;
            }
        }, _macBridgeService.OperationTokenSource!.Token);

        pfCancel = cancel;

        return VSConstants.S_FALSE;
    }

    /// <inheritdoc />
    public int UpdateProjectCfg_Done(IVsHierarchy pHierProj, IVsCfg pCfgProj, IVsCfg pCfgSln, uint dwAction, int fSuccess,
        int fCancel)
    {
        return _macBridgeService.MacBuildSession.IsActive ? VSConstants.S_FALSE : VSConstants.S_OK;
    }

    /// <inheritdoc />
    int IVsUpdateSolutionEvents2.UpdateSolution_Begin(ref int pfCancelUpdate) => VSConstants.S_OK;

    /// <inheritdoc />
    int IVsUpdateSolutionEvents.UpdateSolution_Done(int fSucceeded, int fModified, int fCancelCommand)
    {
        int updateCode = VSConstants.S_OK;

        if (_macBridgeService.MacBuildSession.IsActive)
        {
            CommandEvent = null;

            _macBridgeService.MacBuildSession.IsActive = false;

            if (_macBridgeService.OperationTokenSource != null &&
                _macBridgeService.OperationTokenSource.Token.IsCancellationRequested)
            {
                fCancelCommand = 1; // sanity check for canceled operation
            }

            updateCode = VSConstants.S_FALSE;
        }

        return updateCode;
    }

    /// <inheritdoc />
    int IVsUpdateSolutionEvents.UpdateSolution_StartUpdate(ref int pfCancelUpdate) => VSConstants.S_OK;

    /// <inheritdoc />
    int IVsUpdateSolutionEvents.UpdateSolution_Cancel()
    {
        CommandEvent = null;

        return VSConstants.S_OK;
    }

    /// <inheritdoc />
    int IVsUpdateSolutionEvents.OnActiveProjectCfgChange(IVsHierarchy pIVsHierarchy) => VSConstants.S_OK;

    #endregion IVsUpdateSolutionEvents

    #region IVsDebuggerEvents

    /// <inheritdoc />
    public int OnModeChange(DBGMODE dbgmodeNew)
    {
        switch (dbgmodeNew)
        {
            case DBGMODE.DBGMODE_Run:
                // Debugger has started
                _outputService.WriteToOutputAsync("Debugger started.", OutputPaneType.Debug).FireAndForget();
                break;

            case DBGMODE.DBGMODE_Break:
                // Debugger is in break mode
                _outputService.WriteToOutputAsync("Debugger is in break mode.", OutputPaneType.Debug).FireAndForget();
                break;

            case DBGMODE.DBGMODE_Design:
                // Debugger has stopped
                _outputService.WriteToOutputAsync("Debugger stopped.", OutputPaneType.Debug).FireAndForget();
                break;
        }

        return VSConstants.S_OK;
    }

    #endregion IVsDebuggerEvents
}