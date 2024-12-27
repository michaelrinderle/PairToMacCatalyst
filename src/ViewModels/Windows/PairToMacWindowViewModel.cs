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
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.ComponentModel.Composition;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using Microsoft.VisualStudio.Imaging;
using Microsoft.VisualStudio.PlatformUI;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Threading;
using Ptm.Constants;
using Ptm.Controls;
using Ptm.Enums;
using Ptm.Helpers;
using Ptm.Interfaces;
using Ptm.Models.Dotnet;
using Ptm.Models.Mac;
using Ptm.Views.Dialogs;
using Ptm.Views.Windows;
using StorageConstants = Ptm.Constants.ExtensionConstants.SettingsStorage;

namespace Ptm.ViewModels.Windows;

[Export(typeof(PairToMacWindowViewModel))]
[PartCreationPolicy(CreationPolicy.Shared)]
public class PairToMacWindowViewModel
    : INotifyPropertyChanged
{
    private readonly ILocalService _localService;
    private readonly IMacBridgeService _macBridgeService;
    private readonly IOutputService _outputService;
    private readonly ISecureStorageService _secureStorageService;

    private BitmapSource _computerImage;
    private bool _isPairActionButtonEnabled;
    private bool _isConnecting = false;
    private BitmapSource? _linkImage;
    private ObservableCollection<MacConnectionDetail> _macConnectionDetails = new();
    private string _pairButtonText;
    private MacConnectionDetail? _selectedMacConnectionDetail;
    private StackPanel _statusPanel = new() { Margin = new Thickness(0, 10, 0, 15) };
    private string _statusPanelMessage;
    private ProgressBar _statusPanelProgressBar = new();
    private string _statusPanelTitle;
    private Visibility _statusPanelVisibility = Visibility.Hidden;

    /// <summary>
    /// Initializes a new instance of the <see cref="PairToMacWindowViewModel"/> class.
    /// </summary>
    /// <param name="localService">The local service instance.</param>
    /// <param name="macBridgeService">The Mac bridge service instance.</param>
    /// <param name="outputService">The output service instance.</param>
    /// <param name="secureStorageService">The secure storage service instance.</param>
    [ImportingConstructor]
    public PairToMacWindowViewModel(
        ILocalService localService,
        IMacBridgeService macBridgeService,
        IOutputService outputService,
        ISecureStorageService secureStorageService)
    {
        _localService = localService;
        _macBridgeService = macBridgeService;
        _outputService = outputService;
        _secureStorageService = secureStorageService;
    }

    /// <summary>
    /// Gets or sets the computer image.
    /// </summary>
    public BitmapSource? ComputerImage
    {
        get => _computerImage;
        set
        {
            if (_computerImage == value) return;
            _computerImage = value;
            OnPropertyChanged();
        }
    }

    /// <summary>
    /// Gets or sets a value indicating whether the pair action button is enabled.
    /// </summary>
    public bool IsPairActionButtonEnabled
    {
        get => _isPairActionButtonEnabled;
        set
        {
            if (_isPairActionButtonEnabled == value) return;
            _isPairActionButtonEnabled = value;
            OnPropertyChanged();
        }
    }

    /// <summary>
    /// Gets or sets the link image.
    /// </summary>
    public BitmapSource? LinkImage
    {
        get => _linkImage;
        set
        {
            if (_linkImage == value) return;
            _linkImage = value;
            OnPropertyChanged();
        }
    }

    /// <summary>
    /// Gets or sets the pair action button text.
    /// </summary>
    public string PairActionButtonText
    {
        get => _pairButtonText;
        set
        {
            if (_pairButtonText == value) return;
            _pairButtonText = value;
            OnPropertyChanged();
        }
    }

    /// <summary>
    /// Gets or sets the collection of Mac connection details.
    /// </summary>
    public ObservableCollection<MacConnectionDetail> MacConnectionDetails
    {
        get => _macConnectionDetails;
        set
        {
            if (_macConnectionDetails == value) return;
            _macConnectionDetails = value;
            OnPropertyChanged();
        }
    }

    /// <summary>
    /// Gets or sets the selected Mac connection detail.
    /// </summary>
    public MacConnectionDetail? SelectedMacConnectionDetail
    {
        get => _selectedMacConnectionDetail;
        set
        {
            if (_selectedMacConnectionDetail == value) return;
            _selectedMacConnectionDetail = value;
            EvaluateIfIsPairActionButtonIsEnabled();
            OnPropertyChanged();
        }
    }

    /// <summary>
    /// Gets or sets the status panel.
    /// </summary>
    public StackPanel StatusPanel
    {
        get => _statusPanel;
        set
        {
            if (_statusPanel == value) return;
            _statusPanel = value;
            OnPropertyChanged();
        }
    }

    /// <summary>
    /// Gets or sets the status panel message.
    /// </summary>
    public string StatusPanelMessage
    {
        get => _statusPanelMessage;
        set
        {
            if (_statusPanelMessage == value) return;
            _statusPanelMessage = value;
            OnPropertyChanged();
        }
    }

    /// <summary>
    /// Gets or sets the status panel progress bar.
    /// </summary>
    public ProgressBar StatusPanelProgressBar
    {
        get => _statusPanelProgressBar;
        set
        {
            if (_statusPanelProgressBar == value) return;
            _statusPanelProgressBar = value;
            OnPropertyChanged();
        }
    }

    /// <summary>
    /// Gets or sets the status panel title.
    /// </summary>
    public string StatusPanelTitle
    {
        get => _statusPanelTitle;
        set
        {
            if (_statusPanelTitle == value) return;
            _statusPanelTitle = value;
            OnPropertyChanged();
        }
    }

    /// <summary>
    /// Gets or sets the visibility of the status panel.
    /// </summary>
    public Visibility StatusPanelVisibility
    {
        get => _statusPanelVisibility;
        set
        {
            if (_statusPanelVisibility == value) return;
            _statusPanelVisibility = value;
            OnPropertyChanged();
        }
    }

    /// <summary>
    /// Gets the command to add a Mac.
    /// </summary>
    public RelayCommand AddAMacCommand => new(x => AddAMac());

    /// <summary>
    /// Gets the command to close the window.
    /// </summary>
    public RelayCommand<IClosable> CloseCommand => new(Close);

    /// <summary>
    /// Gets the command for the pair action button.
    /// </summary>
    public RelayCommand PairActionButtonCommand => new(async x => await PairActionAsync());

    /// <summary>
    /// Gets the command for the selected pair action.
    /// </summary>
    public RelayCommand<MacConnectionDetail> SelectedPairActionCommand => new(SelectedPairActionAsync);

    /// <summary>
    /// Gets the command to forget a Mac.
    /// </summary>
    public RelayCommand<MacConnectionDetail> ForgetThisMacCommand => new(ForgetThisMac);

    /// <summary>
    /// Gets the command for hyperlink requests.
    /// </summary>
    public RelayCommand<string> HyperlinkCommand => new(HyperlinkRequest);

    /// <summary>
    /// Occurs when a property value changes.
    /// </summary>
    public event PropertyChangedEventHandler? PropertyChanged;

    /// <summary>
    /// Raises the <see cref="PropertyChanged"/> event.
    /// </summary>
    /// <param name="propertyName">The name of the property that changed.</param>
    protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    /// <summary>
    /// Loads the view model asynchronously.
    /// </summary>
    /// <returns>A task that represents the asynchronous load operation.</returns>
    public async Task LoadViewModelAsync()
    {
        if (LinkImage is null || ComputerImage is null)
        {
            ComputerImage = MonikerHelper.GetMonikerImageSource(KnownMonikers.Computer, 32, 32, 0xFF696969);
            LinkImage = MonikerHelper.GetMonikerImageSource(KnownMonikers.Link, 24, 24);
        }

        SetStatusPanelClearAsync(this);

        await RetrieveMacConnectionsAsync();

        if (_macBridgeService.IsConnected)
            SelectedMacConnectionDetail = MacConnectionDetails.FirstOrDefault(x =>
                x.SshFingerprint == _macBridgeService.MacConnection?.SshFingerprint);

        EvaluatePairActionText();
    }

    /// <summary>
    /// Handles the MouseLeave event for the PairToMacWindow.
    /// </summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">The event data.</param>
    public void PairToMacWindow_MouseLeave(object sender, MouseEventArgs e)
    {
        SelectedMacConnectionDetail = null;
        EvaluateIfIsPairActionButtonIsEnabled();
    }

    /// <summary>
    /// Adds a new Mac to the list of available connections.
    /// </summary>
    /// <remarks>
    /// This method opens a modal window to add a new Mac. If the Mac is successfully added,
    /// it then opens another modal window to connect to the newly added Mac. Finally, it retrieves
    /// the updated list of Mac connections.
    /// </remarks>
    private async void AddAMac()
    {
        var addAMac = PtmPackage.Instance.GetMefComposition<AddAMacWindow>();

        addAMac.ShowModal();

        if (!addAMac.CloseResult.HasValue || !addAMac.CloseResult.Value) return;

        var mac = addAMac.Result as string;

        var connectToMac = PtmPackage.Instance.GetMefComposition<ConnectToMacWindow>();
        connectToMac.Load([mac!]);
        connectToMac.ShowModal();
        if (!connectToMac.CloseResult.HasValue || !connectToMac.CloseResult.Value) return;

        await RetrieveMacConnectionsAsync();
    }

    /// <summary>
    /// Evaluates whether the pair action button should be enabled based on the selected Mac connection detail.
    /// </summary>
    private void EvaluateIfIsPairActionButtonIsEnabled()
    {
        IsPairActionButtonEnabled = SelectedMacConnectionDetail is not null;
        EvaluatePairActionText();
    }

    /// <summary>
    /// Evaluates the text to be displayed on the pair action button.
    /// </summary>
    /// <remarks>
    /// This method sets the text of the pair action button based on the current connection status.
    /// If the Mac is connected and the SSH fingerprint matches the selected Mac connection detail,
    /// the button text will be "Disconnect". Otherwise, it will be "Connect".
    /// </remarks>
    private void EvaluatePairActionText()
    {
        PairActionButtonText =
            _macBridgeService.IsConnected &&
            _macBridgeService.MacConnection?.SshFingerprint == SelectedMacConnectionDetail?.SshFingerprint
                ? "Disconnect"
                : "Connect";
    }

    /// <summary>
    /// Forgets the specified Mac connection detail.
    /// </summary>
    /// <param name="macConnectionDetail">The Mac connection detail to forget.</param>
    /// <remarks>
    /// This method displays a confirmation dialog to the user. If the user confirms, it removes the specified Mac connection
    /// detail from the secure storage and updates the list of Mac connection details in the view model.
    /// </remarks>
    private async void ForgetThisMac(MacConnectionDetail macConnectionDetail)
    {
        var dialog = new ForgetMacDialog(macConnectionDetail.Hostname, macConnectionDetail.IpAddress);
        var confirm = dialog.ShowModal();

        if (!confirm.HasValue || !confirm.Value) return;

        var macConnectionList = await _secureStorageService
            .GetDataAsync<List<MacConnection>>(StorageConstants.MacConnectionListKey);

        macConnectionList = macConnectionList!.Where(x => x.Hostname != macConnectionDetail.Hostname).ToList();

        await _secureStorageService.SaveDataAsync(StorageConstants.MacConnectionListKey, macConnectionList);

        MacConnectionDetails =
            new ObservableCollection<MacConnectionDetail>(MacConnectionDetails.Where(x =>
                x.Hostname != macConnectionDetail.Hostname));

        SelectedMacConnectionDetail = null;

        EvaluateIfIsPairActionButtonIsEnabled();
    }

    /// <summary>
    /// Handles the pair action asynchronously.
    /// </summary>
    /// <remarks>
    /// This method connects or disconnects from a selected Mac connection detail.
    /// If the connection is successful, it updates the connection status and UI accordingly.
    /// </remarks>
    private async Task PairActionAsync()
    {
        try
        {
            if (SelectedMacConnectionDetail is null) return;

            var detail = MacConnectionDetails.FirstOrDefault(x =>
                x.Hostname == SelectedMacConnectionDetail.Hostname);

            if (detail is null) return;

            await UiThreadHelper.EnsureUiThreadAsync(() =>
            {
                detail.IsProcessing = true;
                detail.IsConnected = false;

                OnPropertyChanged(nameof(detail));
            });

            // TODO: PairToMacViewModel.cs, want to force user disconnection first?

            var isConnected = _macBridgeService.IsConnected;
            var currentFingerPrint = _macBridgeService.MacConnection?.SshFingerprint;

            if (!isConnected || detail.SshFingerprint != currentFingerPrint)
            {
                detail.IsConnected = await RunMacConnectionAsync(detail);

                if (detail.IsConnected && isConnected)
                {
                    var currentConnection = MacConnectionDetails
                        .FirstOrDefault(x => x.SshFingerprint == currentFingerPrint);

                    if (currentConnection is not null)
                    {
                        await UiThreadHelper.EnsureUiThreadAsync(() =>
                        {
                            currentConnection.IsConnected = false;
                            OnPropertyChanged(nameof(currentConnection));
                        });
                    }
                }
            }
            else
            {
                detail.IsConnected = !await RunMacDisconnectionAsync();

                await SetStatusPanelClearAsync(this);
            }

            await UiThreadHelper.EnsureUiThreadAsync(() =>
            {
                detail.IsProcessing = false;
                OnPropertyChanged(nameof(detail));
            });
        }
        catch (Exception ex)
        {
            Debug.WriteLine(ex.ToString());
        }
        finally
        {
            EvaluatePairActionText();
        }
    }

    /// <summary>
    /// Retrieves the list of Mac connections asynchronously.
    /// </summary>
    /// <remarks>
    /// This method fetches the list of Mac connections from secure storage and updates the
    /// <see cref="MacConnectionDetails"/> collection. If no connections are found, the collection remains unchanged.
    /// </remarks>
    /// <returns>A task that represents the asynchronous retrieve operation.</returns>
    private async Task RetrieveMacConnectionsAsync()
    {
        var macConnectionList = await _secureStorageService
            .GetDataAsync<List<MacConnection>>(StorageConstants.MacConnectionListKey);

        if (macConnectionList is null || macConnectionList.Count <= 0) return;

        MacConnectionDetails = new ObservableCollection<MacConnectionDetail>(macConnectionList
            .Select(x => new MacConnectionDetail(x, _macBridgeService?.MacConnection?.SshFingerprint)));
    }

    /// <summary>
    /// Establishes a connection to a Mac machine asynchronously.
    /// </summary>
    /// <param name="detail">The details of the Mac connection.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains a boolean value indicating whether the connection was successful.</returns>
    private async Task<bool> RunMacConnectionAsync(MacConnectionDetail detail)
    {
        try
        {
            // Connecting
            //      -> Initializing environment...                                                                       ✔
            //      -> Checking SSH Configuration...                                                                     ✔
            //      -> Verifying dotnet runtime information...                                                           ✔
            //      -> Checking Mono installation...                                                [Obsolete]
            //      -> Checking Xamarin iOS installation...[yes/no]                                 [Obsolete]
            //      -> check maui workload & mismatch versions [yes/no]                             [New]                ✔
            //      -> starting connection to 'hostname'...                                                              ✔
            //      -> starting IDB 17.12.0.152...
            //      -> Starting registered agents: ...
            //      -> The Agents have been started successfully
            // Running Server validation...
            //      -> Initializing IDB environment...
            //      -> Validating Xcode license state...                                                                 ✔
            //      -> Validating installed Xcode tools...                                                               ✔
            //      -> Checking Xcode tool package 'MobileDeviceDevelopment.pkg' installation...                         ✔
            //      -> Checking Xcode tool package 'CoreTypes.pkg' installation...                                       ✔
            //      -> Checking Xcode tool package 'MobileDevice.pkg' installation...                                    ✔
            //      -> Xcode tool package 'XcodeSystemResources.pkg' is up to date                                       ✔
            //      -> Checking installed iOS runtimes...                                                                ✔
            // Connected                                                                                                 ✔
            //      -> Connection to 'hostname' completed successfully                                                   ✔

            if (_isConnecting) return true;

            _isConnecting = true;

            _macBridgeService.MacBuildEnv!.ResetEnvironment();

            await SetStatusPanelClearAsync(this);
            await SetStatusPanelAsync(this, StatusPanelType.Operation, "Connecting...", "Initializing environment...");

            await SetStatusPanelMessageAsync(this, "Checking SSH Configuration...");

            await TaskScheduler.Default;

            if (!await _macBridgeService.BridgeConnectAsync(detail.Hostname))
            {
                SetStatusPanelAsync(this, StatusPanelType.Warning, "Alert",
                    $"\'{detail.Hostname}\' connection could not be established");

                return false;
            }

            _macBridgeService.MacBuildEnv.MacOsVersion = await _macBridgeService.GetMacOsVersionAsync();

            await SetStatusPanelMessageAsync(this, "Verifying dotnet runtime information...");

            _macBridgeService.MacBuildEnv.DotnetRuntimes = await _macBridgeService.VerifyMacDotnetRuntimesAsync();
            _macBridgeService.MacBuildEnv.DotnetSdks = await _macBridgeService.VerifyMacDotnetSdksAsync();

            if (_macBridgeService.MacBuildEnv.DotnetRuntimes?.Count == 0 ||
                _macBridgeService.MacBuildEnv.DotnetSdks?.Count == 0)
            {
                await SetStatusPanelAsync(this, StatusPanelType.Warning, "Alert",
                    "Failed to verify remote dotnet installation");

                return false;
            }

            await SetStatusPanelMessageAsync(this, "Checking Maui workloads...");

            _macBridgeService.MacBuildEnv.DotnetMauiWorkloads =
                await _macBridgeService.VerifyMacDotnetMauiWorkloadAsync();

            var localMauiWorkloads = await _localService.VerifyLocalDotnetWorkloadsAsync();

            if (!DotnetWorkload.MatchMauiWorkLoads(
                    _macBridgeService.MacBuildEnv.DotnetMauiWorkloads,
                    localMauiWorkloads))
            {
                // TODO: PairToMacViewModel.cs, add a dialog to install missing workloads

                await SetStatusPanelAsync(this, StatusPanelType.Warning, "Alert",
                    "Maui workload mismatch detected, please install missing workloads");

                return false;
            }

            await SetStatusPanelMessageAsync(this, $"Starting connection to '{detail.Hostname}'...");

            await TaskScheduler.Default;

            if (!await _macBridgeService.VerifyVsDebuggerExistsAsync())
            {
                await SetStatusPanelAsync(this, StatusPanelType.Warning, "Alert",
                    "Failed to verify remote Vs debugger exists on remote machine");

                return false;
            }

            var vsDbgVer = await _macBridgeService.GetVsDebuggerVersionAsync();

            await SetStatusPanelMessageAsync(this, $"Starting VsDbg {vsDbgVer!.ToString()}");

            if (!await _macBridgeService.StartVsDebuggerAsync())
            {
                await SetStatusPanelAsync(this, StatusPanelType.Warning, "Alert",
                    "Failed to start VS debugger on remote machine");

                return false;
            }

            //      -> Starting registered agents: ...
            //      -> The Agents have been started successfully

            await SetStatusPanelTitleAsync(this, "Running Server Validation...");

            //      -> Initializing IDB environment...

            await SetStatusPanelMessageAsync(this, "Validating Xcode license state...");

            if (!await _macBridgeService.VerifyMacXcodeVersionAsync(_macBridgeService.MacBuildEnv) ||
                !await _macBridgeService.VerifyMacXcodeLicenseStatusAsync())
            {
                SetStatusPanelAsync(this, StatusPanelType.Warning, "Alert",
                    "Failed to verify Xcode license status");

                return false;
            }

            await SetStatusPanelMessageAsync(this, "Validating installed Xcode tools...");

            if (!await _macBridgeService.VerifyMacXcodeBuildToolsAsync())
            {
                await SetStatusPanelAsync(this, StatusPanelType.Warning, "Alert",
                    "Failed to verify Xcode build tools");

                return false;
            }

            await SetStatusPanelMessageAsync(this,
                $"Checking Xcode tool package '{MacConstants.MobileDeviceDevelopmentPkgName}' installation...");

            if (!await _macBridgeService.VerifyMacXcodePkgExistsAsync(MacConstants.MobileDeviceDevelopmentPkgFullName))
            {
                await SetStatusPanelAsync(this, StatusPanelType.Warning, "Alert",
                    $"Failed to verify {MacConstants.MobileDeviceDevelopmentPkgName}");

                return false;
            }

            await SetStatusPanelMessageAsync(this,
                $"Checking Xcode tool package '{MacConstants.CoreTypesPkgName}' installation...");

            if (!await _macBridgeService.VerifyMacXcodePkgExistsAsync(MacConstants.CoreTypesPkgFullName))
            {
                await SetStatusPanelAsync(this, StatusPanelType.Warning, "Alert",
                    $"Failed to verify {MacConstants.CoreTypesPkgName}");

                return false;
            }

            await SetStatusPanelMessageAsync(this,
                $"Checking Xcode tool package '{MacConstants.MobileDevicePkgName}' installation...");

            if (!await _macBridgeService.VerifyMacXcodePkgExistsAsync(MacConstants.MobileDevicePkgFullName))
            {
                await SetStatusPanelAsync(this, StatusPanelType.Warning, "Alert",
                    $"Failed to verify {MacConstants.MobileDevicePkgName}");

                return false;
            }

            var xcodeSystemResourcesVersion =
                await _macBridgeService.GetMacXcodePkgVersionAsync(MacConstants.XcodeSystemResourcesPkgFullName);

            if (xcodeSystemResourcesVersion is null ||
                xcodeSystemResourcesVersion <= MacConstants.MinimumXcodeSystemResourcesVersion)
            {
                await SetStatusPanelAsync(this, StatusPanelType.Warning, "Alert",
                    $"Failed to verify {MacConstants.XcodeSystemResourcesPkgName}");

                return false;
            }

            await SetStatusPanelMessageAsync(this, "Xcode tool package 'XcodeSystemResources.pkg' is up to date");

            await SetStatusPanelMessageAsync(this, "Checking installed MacOS runtimes...");

            var localDotnetRuntimes = await _localService.VerifyLocalDotnetRuntimesAsync();

            if (!DotnetRuntime.MatchRuntimes(_macBridgeService.MacBuildEnv.DotnetRuntimes, localDotnetRuntimes))
            {
                await SetStatusPanelAsync(this, StatusPanelType.Warning, "Alert",
                    $"Dotnet runtime mismatch detected, please install missing runtimes");

                return false;
            }

            await SetStatusPanelAsync(this, StatusPanelType.Information, "Connected",
                $"Connection to '{detail.Hostname}' completed successfully");

            _macBridgeService.MacBuildEnv.IsEnvironmentVerified = true;

            return true;
        }
        catch (Exception ex)
        {
            await SetStatusPanelAsync(this, StatusPanelType.Warning, "Alert",
                "Could not connect to Mac, check debug output pane for more information");

            await _outputService.WriteToOutputAsync(ex.Message, OutputPaneType.Debug);
        }
        finally
        {
            if (!_macBridgeService.MacBuildEnv!.IsEnvironmentVerified)
            {
                await _macBridgeService.BridgeDisconnectAsync();
            }

            _isConnecting = false;
        }

        return false;
    }

    /// <summary>
    /// Asynchronously disconnects from the Mac machine.
    /// </summary>
    /// <returns>
    /// A task that represents the asynchronous operation. The task result contains a boolean value indicating whether
    /// the disconnection was successful.
    /// </returns>
    private async Task<bool> RunMacDisconnectionAsync()
    {
        try
        {
            return await _macBridgeService.BridgeDisconnectAsync();
        }
        catch (Exception ex)
        {
            await _outputService.WriteToOutputAsync(ex.Message, OutputPaneType.Debug);
        }

        return true;
    }

    /// <summary>
    /// Handles the selected pair action asynchronously.
    /// </summary>
    /// <param name="macConnectionDetail">The details of the selected Mac connection.</param>
    /// <remarks>
    /// This method sets the selected Mac connection detail and initiates the pairing action asynchronously.
    /// </remarks>
    private async void SelectedPairActionAsync(MacConnectionDetail macConnectionDetail)
    {
        SelectedMacConnectionDetail = macConnectionDetail;

        await PairActionAsync();
    }

    /// <summary>
    /// Closes the specified window.
    /// </summary>
    /// <param name="window">The window to close.</param>
    /// <remarks>
    /// This method checks if the window is null or if a connection is in progress before attempting to close the window.
    /// </remarks>
    private void Close(IClosable? window)
    {
        if (window is null) return;

        if (_isConnecting) return;

        window.Close();
    }

    /// <summary>
    /// Opens the specified URI in the default web browser.
    /// </summary>
    /// <param name="uri">The URI to open.</param>
    private static void HyperlinkRequest(string? uri)
    {
        if (uri is not null)
            Process.Start(new ProcessStartInfo(uri) { UseShellExecute = true });
    }

    /// <summary>
    /// Sets the status panel with the specified type, title, and message.
    /// </summary>
    /// <param name="viewModel">The view model instance.</param>
    /// <param name="statusType">The type of the status panel.</param>
    /// <param name="title">The title of the status panel.</param>
    /// <param name="message">The message to display in the status panel.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    public static async Task SetStatusPanelAsync(PairToMacWindowViewModel? viewModel, StatusPanelType statusType, string title,
        string message)
    {
        await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();

        viewModel ??= PtmPackage.Instance.GetMefComposition<PairToMacWindowViewModel>();

        viewModel!.StatusPanel.Children.Clear();
        viewModel.StatusPanelTitle = title;
        viewModel.StatusPanelMessage = message;

        var statusIcon = new StackPanel { Margin = new Thickness(5, 5, 0, 0) };
        var statusProgressBar = new StackPanel { VerticalAlignment = VerticalAlignment.Bottom };

        var titleLabel = new Label
        {
            FontWeight = FontWeights.Bold,
            Margin = new Thickness(5, 0, 0, 0),
            Padding = new Thickness(5, 0, 0, 5)
        };

        var titleBinding = new Binding(nameof(StatusPanelTitle))
        {
            Mode = BindingMode.OneWay,
            Source = viewModel
        };

        titleLabel.SetBinding(Label.ContentProperty, titleBinding);

        var messageLabel = new Label
        {
            FontWeight = FontWeights.Bold,
            Margin = new Thickness(5, 0, 0, 0),
            Padding = new Thickness(5, 0, 0, 5)
        };

        var messageBinding = new Binding(nameof(StatusPanelMessage))
        {
            Mode = BindingMode.OneWay,
            Source = viewModel
        };

        messageLabel.SetBinding(Label.ContentProperty, messageBinding);

        var statusPanel = new StackPanel
        {
            Children =
            {
                new StackPanel
                {
                    Children =
                    {
                        statusIcon,
                        new StackPanel
                        {
                            Children =
                            {
                                new StackPanel
                                {
                                    Children =
                                    {
                                        titleLabel,
                                        messageLabel
                                    }
                                }
                            },
                            Orientation = Orientation.Vertical
                        }
                    },
                    Orientation = Orientation.Horizontal
                },
                statusProgressBar
            },
            Margin = new Thickness(5, 5, 0, 0),
            Orientation = Orientation.Vertical
        };

        switch (statusType)
        {
            case StatusPanelType.Information:
                statusIcon.Children.Add(new Image
                {
                    Source = MonikerHelper.GetMonikerImageSource(KnownMonikers.StatusInformation, 18, 18),
                    Margin = new Thickness(0, 0, 5, 0)
                });
                break;

            case StatusPanelType.Operation:
                statusIcon.Children.Add(new ProgressControl());
                viewModel.StatusPanelProgressBar = new ProgressBar();
                viewModel.StatusPanelProgressBar.Value = 1;
                statusProgressBar.Children.Add(viewModel.StatusPanelProgressBar);
                break;

            case StatusPanelType.Warning:
                statusIcon.Children.Add(new Image
                {
                    Source = MonikerHelper.GetMonikerImageSource(KnownMonikers.StatusWarning, 18, 18),
                    Margin = new Thickness(0, 0, 5, 0)
                });
                break;
        }

        viewModel.StatusPanel = statusPanel;
        viewModel.StatusPanelVisibility = Visibility.Visible;

        await Task.Delay(100); // allow UI to update for fast switching

        await TaskScheduler.Default;
    }

    /// <summary>
    /// Clears the status panel asynchronously.
    /// </summary>
    /// <param name="viewModel">The view model instance.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    public static async Task SetStatusPanelClearAsync(PairToMacWindowViewModel? viewModel)
    {
        await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();

        viewModel ??= PtmPackage.Instance.GetMefComposition<PairToMacWindowViewModel>();

        viewModel!.StatusPanel.Children.Clear();
        viewModel.StatusPanelVisibility = Visibility.Hidden;

        await Task.Delay(100); // allow UI to update for fast switching

        await TaskScheduler.Default;
    }

    /// <summary>
    /// Sets the status panel message asynchronously.
    /// </summary>
    /// <param name="viewModel">The view model instance.</param>
    /// <param name="message">The message to display in the status panel.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    public static async Task SetStatusPanelMessageAsync(PairToMacWindowViewModel? viewModel, string message)
    {
        await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();

        viewModel ??= PtmPackage.Instance.GetMefComposition<PairToMacWindowViewModel>();

        viewModel!.StatusPanelMessage = message;

        await Task.Delay(100); // allow UI to update for fast switching

        await TaskScheduler.Default;
    }

    /// <summary>
    /// Sets the progress value of the status panel's progress bar asynchronously.
    /// </summary>
    /// <param name="viewModel">The view model instance.</param>
    /// <param name="progress">The progress value to set.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    public static async Task SetStatusPanelProgressAsync(PairToMacWindowViewModel? viewModel, int progress)
    {
        await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();

        viewModel ??= PtmPackage.Instance.GetMefComposition<PairToMacWindowViewModel>();

        if (viewModel?.StatusPanelProgressBar is null) return;

        viewModel.StatusPanelProgressBar.Value = progress;

        await Task.Delay(100); // allow UI to update for fast switching

        await TaskScheduler.Default;
    }

    /// <summary>
    /// Sets the title of the status panel asynchronously.
    /// </summary>
    /// <param name="viewModel">The view model instance.</param>
    /// <param name="title">The title to set for the status panel.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    public static async Task SetStatusPanelTitleAsync(PairToMacWindowViewModel? viewModel, string title)
    {
        await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();

        viewModel ??= PtmPackage.Instance.GetMefComposition<PairToMacWindowViewModel>();

        viewModel!.StatusPanelTitle = title;

        await Task.Delay(100); // allow UI to update for fast switching

        await TaskScheduler.Default;
    }
}