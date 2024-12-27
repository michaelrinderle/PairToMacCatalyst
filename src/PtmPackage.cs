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

using Community.VisualStudio.Toolkit;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using Ptm.Models.Options;
using Ptm.Views.Options;
using System;
using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Hosting;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using EnvDTE80;
using Microsoft.VisualStudio.RpcContracts.Solution;
using Microsoft.VisualStudio;
using Ptm.Interfaces;

namespace Ptm;

/// <summary>
///     Represents the main package for the Pair To MacCatalyst extension.
///     This package is responsible for initializing solution events, MEF composition container,
///     and registering commands. It also provides methods to get MEF compositions and options.
/// </summary>
[Guid(PackageGuids.PtmString)]
[InstalledProductRegistration(Vsix.Name, Vsix.Description, Vsix.Version)]
[PackageRegistration(UseManagedResourcesOnly = true, AllowsBackgroundLoading = true)]
[ProvideAutoLoad(UIContextGuids80.NoSolution, PackageAutoLoadFlags.BackgroundLoad)]
[ProvideMenuResource("Menus.ctmenu", 1)]
[ProvideOptionPage(typeof(PtmOptionsPage), "Pair To MacCatalyst", "Connection", 0, 0, true)]
public sealed class PtmPackage
    : AsyncPackage, IVsSolutionEvents
{
    private CompositionContainer _mefCompositionContainer;

    private IVsSolution? _solution;

    private ISolutionsService? _solutionService;

    private uint _solutionEventsCookie;

    public static PtmPackage Instance { get; private set; }

    protected override async Task InitializeAsync(CancellationToken cancellationToken,
        IProgress<ServiceProgressData> progress)
    {
        await JoinableTaskFactory.SwitchToMainThreadAsync(cancellationToken);

        Instance = this;

        // initialize solution events
        _solution = await GetServiceAsync(typeof(SVsSolution)) as IVsSolution;
        _solution?.AdviseSolutionEvents(this, out _solutionEventsCookie);

        // initialize MEF composition container
        this.InitializeMefCompositionContainer();

        // register commands
        await this.RegisterCommandsAsync();
    }

    /// <inheritdoc/>
    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            if (_solution is not null && _solutionEventsCookie != 0)
                _solution.UnadviseSolutionEvents(_solutionEventsCookie);
        }

        base.Dispose(disposing);
    }

    /// <summary>
    /// Gets the location of the executing assembly.
    /// </summary>
    /// <returns>A string representing the location of the executing assembly.</returns>
    public string GetExecutingAssemblyLocation()
    {
        return GetType().Assembly.Location;
    }

    /// <summary>
    /// Retrieves an exported value of the specified type from the MEF composition container.
    /// </summary>
    /// <typeparam name="T">The type of the exported value to retrieve.</typeparam>
    /// <returns>An instance of the specified type from the MEF composition container.</returns>
    /// <exception cref="InvalidOperationException">Thrown if the method is not called on the UI thread.</exception>
    public T GetMefComposition<T>() where T : class
    {
        ThreadHelper.ThrowIfNotOnUIThread();

        return _mefCompositionContainer.GetExportedValue<T>();
    }

    /// <summary>
    /// Refreshes the MEF (Managed Extensibility Framework) composition with the specified part.
    /// This method creates a new composition batch, adds the provided part to the batch,
    /// and composes the batch into the MEF composition container.
    /// </summary>
    /// <typeparam name="T">The type of the part to refresh in the MEF composition.</typeparam>
    /// <param name="part">The part to refresh in the MEF composition.</param>
    public void RefreshMefComposition<T>(T part)
    {
        var batch = new CompositionBatch();
        batch.AddPart(part);

        _mefCompositionContainer.Compose(batch);
    }

    /// <summary>
    /// Retrieves the Pair To MacCatalyst options.
    /// </summary>
    /// <returns>A <see cref="PtmOptions"/> object containing the current options.</returns>
    public PtmOptions GetPtmOptions()
    {
        ThreadHelper.ThrowIfNotOnUIThread();

        var page = (PtmOptionsPage)GetDialogPage(typeof(PtmOptionsPage));

        return new PtmOptions
        {
            PairToKnownAvailableMacHosts = page.PairToKnownAvailableMacHosts,
        };
    }

    /// <summary>
    /// Initializes the MEF (Managed Extensibility Framework) composition container.
    /// This method creates an aggregate catalog, adds the assembly catalog of the current assembly,
    /// and composes the parts to the MEF composition container.
    /// </summary>
    private void InitializeMefCompositionContainer()
    {
        var catalog = new AggregateCatalog();
        catalog.Catalogs.Add(new AssemblyCatalog(typeof(PtmPackage).Assembly));
        _mefCompositionContainer = new CompositionContainer(catalog);
        _mefCompositionContainer.ComposeParts(this);
    }

    #region IVsSolutionEvents

    /// <inheritdoc/>
    public int OnAfterOpenProject(IVsHierarchy pHierarchy, int fAdded) => VSConstants.S_OK;

    /// <inheritdoc/>
    public int OnQueryCloseProject(IVsHierarchy pHierarchy, int fRemoving, ref int pfCancel) => VSConstants.S_OK;

    /// <inheritdoc/>
    public int OnBeforeCloseProject(IVsHierarchy pHierarchy, int fRemoved) => VSConstants.S_OK;

    /// <inheritdoc/>
    public int OnAfterLoadProject(IVsHierarchy pStubHierarchy, IVsHierarchy pRealHierarchy) => VSConstants.S_OK;

    /// <inheritdoc/>
    public int OnQueryUnloadProject(IVsHierarchy pRealHierarchy, ref int pfCancel) => VSConstants.S_OK;

    /// <inheritdoc/>
    public int OnBeforeUnloadProject(IVsHierarchy pRealHierarchy, IVsHierarchy pStubHierarchy) => VSConstants.S_OK;

    /// <inheritdoc/>
    public int OnAfterOpenSolution(object pUnkReserved, int fNewSolution)
    {
        JoinableTaskFactory.Run(async () =>
        {
            await JoinableTaskFactory.SwitchToMainThreadAsync();

            if (_solutionService is null)
            {
                if (await GetServiceAsync(typeof(EnvDTE.DTE)) is DTE2 dte)
                {
                    _solutionService = _mefCompositionContainer.GetExportedValue<ISolutionsService>();
                    await _solutionService.InitializeAsync(dte);
                }
            }
        });

        return VSConstants.S_OK;
    }

    /// <inheritdoc/>
    public int OnQueryCloseSolution(object pUnkReserved, ref int pfCancel) => VSConstants.S_OK;

    /// <inheritdoc/>
    public int OnBeforeCloseSolution(object pUnkReserved) => VSConstants.S_OK;

    /// <inheritdoc/>
    public int OnAfterCloseSolution(object pUnkReserved)
    {
        _solutionService?.Dispose();
        _solutionService = null;

        return VSConstants.S_OK;
    }

    #endregion IVsSolutionEvents
}