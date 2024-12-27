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
using System.ComponentModel;
using System.ComponentModel.Composition;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Imaging;
using Microsoft.VisualStudio.Imaging;
using Microsoft.VisualStudio.Shell;
using Ptm.Controls;
using Ptm.Enums;
using Ptm.Helpers;
using Ptm.Interfaces;
using Ptm.Models.Mac;
using StorageConstants = Ptm.Constants.ExtensionConstants.SettingsStorage;

namespace Ptm.ViewModels.Windows;

/// <summary>
///     ViewModel for the Connect to Mac Window.
///     Handles the logic for connecting to a Mac machine via SSH.
/// </summary>
[Export(typeof(ConnectToMacWindowViewModel))]
[PartCreationPolicy(CreationPolicy.NonShared)]
public class ConnectToMacWindowViewModel : INotifyPropertyChanged
{
    private readonly IMacBridgeService _macBridgeService;
    private readonly IOutputService _outputService;
    private readonly ISecureStorageService _secureStorageService;
    private readonly ISecureTransferService _secureTransferService;
    private readonly IStatusPanelService _statusPanelService;

    private bool _isSshConnectionAttempted;
    private bool _isSshSigninButtonEnabled;
    private bool _isTestingConnection;
    private string _macSshFootprint;
    private string _macSshHostname;
    private string _macSshPassword;
    private string _macSshUsername;
    private FontWeight _sshFootprintTextBlockFontWeight = FontWeights.Regular;
    private string _sshFootPrintTextBlockText;
    private Visibility _sshProgressControlVisibility = Visibility.Collapsed;
    private Visibility _sshStatusImageVisibility = Visibility.Collapsed;
    private BitmapSource _sshSuccessImage;

    /// <summary>
    ///     Initializes a new instance of the <see cref="ConnectToMacWindowViewModel" /> class.
    /// </summary>
    /// <param name="outputService">The output service.</param>
    /// <param name="macBridgeService">The Mac bridge service.</param>
    /// <param name="secureStorageService">The secure storage service.</param>
    /// <param name="secureTransferService">The secure transfer service.</param>
    /// <param name="statusPanelService">The status panel service.</param>
    [ImportingConstructor]
    public ConnectToMacWindowViewModel(
        IOutputService outputService,
        IMacBridgeService macBridgeService,
        ISecureStorageService secureStorageService,
        ISecureTransferService secureTransferService,
        IStatusPanelService statusPanelService)
    {
        _outputService = outputService;
        _macBridgeService = macBridgeService;
        _statusPanelService = statusPanelService;
        _secureTransferService = secureTransferService;
        _secureStorageService = secureStorageService;
    }

    /// <summary>
    ///     Gets or sets a value indicating whether the SSH sign-in button is enabled.
    /// </summary>
    public bool IsSshSigninButtonEnabled
    {
        get => _isSshSigninButtonEnabled;
        set
        {
            if (_isSshSigninButtonEnabled == value) return;
            _isSshSigninButtonEnabled = value;
            OnPropertyChanged();
        }
    }

    /// <summary>
    ///     Gets or sets the SSH hostname of the Mac.
    /// </summary>
    public string MacSshHostname
    {
        get => _macSshHostname;
        set
        {
            if (_macSshHostname == value) return;
            _macSshHostname = value;
            OnPropertyChanged();
        }
    }

    /// <summary>
    ///     Gets or sets the SSH password of the Mac.
    /// </summary>
    public string MacSshPassword
    {
        get => _macSshPassword;
        set
        {
            if (_macSshPassword == value) return;
            _macSshPassword = value;
            OnPropertyChanged();
            EvaluateIfSigninButtonIsEnabled();
        }
    }

    /// <summary>
    ///     Gets or sets the SSH username of the Mac.
    /// </summary>
    public string MacSshUsername
    {
        get => _macSshUsername;
        set
        {
            if (_macSshUsername == value) return;
            _macSshUsername = value;
            OnPropertyChanged();
            EvaluateIfSigninButtonIsEnabled();
        }
    }

    /// <summary>
    ///     Gets or sets the font weight of the SSH footprint text block.
    /// </summary>
    public FontWeight SshFootprintTextBlockFontWeight
    {
        get => _sshFootprintTextBlockFontWeight;
        set
        {
            if (_sshFootprintTextBlockFontWeight == value) return;
            _sshFootprintTextBlockFontWeight = value;
            OnPropertyChanged();
        }
    }

    /// <summary>
    ///     Gets or sets the text of the SSH footprint text block.
    /// </summary>
    public string SshFootPrintTextBlockText
    {
        get => _sshFootPrintTextBlockText;
        set
        {
            if (_sshFootPrintTextBlockText == value) return;
            _sshFootPrintTextBlockText = value;
            OnPropertyChanged();
        }
    }

    /// <summary>
    ///     Gets or sets the visibility of the SSH progress control.
    /// </summary>
    public Visibility SshProgressControlVisibility
    {
        get => _sshProgressControlVisibility;
        set
        {
            if (_sshProgressControlVisibility == value) return;
            _sshProgressControlVisibility = value;
            OnPropertyChanged();
        }
    }

    /// <summary>
    ///     Gets or sets the visibility of the SSH status image.
    /// </summary>
    public Visibility SshStatusImageVisibility
    {
        get => _sshStatusImageVisibility;
        set
        {
            if (_sshStatusImageVisibility == value) return;
            _sshStatusImageVisibility = value;
            OnPropertyChanged();
        }
    }

    /// <summary>
    ///     Gets or sets the SSH success image.
    /// </summary>
    public BitmapSource SshSuccessImage
    {
        get => _sshSuccessImage;
        set
        {
            _sshSuccessImage = value;
            OnPropertyChanged();
        }
    }

    /// <summary>
    ///     Gets the command to close the window.
    /// </summary>
    public RelayCommand<IClosable> CloseCommand => new(Close);

    /// <summary>
    ///     Gets the command to sign in to the Mac.
    /// </summary>
    public RelayCommand<IClosable> SignInCommand => new(Signin);

    /// <summary>
    ///     Occurs when a property value changes.
    /// </summary>
    public event PropertyChangedEventHandler PropertyChanged;

    /// <summary>
    ///     Raises the <see cref="PropertyChanged" /> event.
    /// </summary>
    /// <param name="propertyName">Name of the property that changed.</param>
    protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    /// <summary>
    ///     Loads the specified objects.
    /// </summary>
    /// <param name="objects">The objects to load.</param>
    public void Load(object[] objects)
    {
        MacSshHostname = objects[0] as string;

        new Action(async () => { await RunSshFootprintCheckAsync(); })();
    }

    /// <summary>
    ///     Signs in to the Mac using the provided credentials.
    /// </summary>
    /// <param name="window">The window to close upon successful sign-in.</param>
    private async void Signin(IClosable? window)
    {
        if (window is null) return;
        if (_isTestingConnection) return;
        if (string.IsNullOrEmpty(MacSshHostname) ||
            string.IsNullOrEmpty(MacSshUsername) ||
            string.IsNullOrEmpty(MacSshPassword)) return;

        Task.Run(async () =>
        {
            try
            {
                await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();

                var macConnectionList = await _secureStorageService
                    .GetDataAsync<List<MacConnection>>(StorageConstants.MacConnectionListKey);

                if (macConnectionList is { Count: > 1 })
                    if (macConnectionList.Any(x => x.Hostname == MacSshHostname))
                        // set alert
                        return;

                await _statusPanelService.SetStatusPanelMessageAsync("Connection to host...");

                var connectionInfo =
                    await _macBridgeService.VerifyMacCredentialsAsync(MacSshHostname, MacSshUsername, MacSshPassword);
                if (connectionInfo != null)
                {
                    await _statusPanelService.SetStatusPanelMessageAsync("Connection succeeded...");

                    var macConnection = new MacConnection
                    {
                        DateCreated = DateTime.Now,
                        Hostname = connectionInfo.Hostname,
                        IpAddress = connectionInfo.IpAddress,
                        Password = MacSshPassword,
                        SshFingerprint = _macSshFootprint,
                        Username = MacSshUsername
                    };

                    macConnectionList ??= [];
                    macConnectionList.Add(macConnection);

                    var stored =
                        await _secureStorageService.SaveDataAsync(
                            StorageConstants.MacConnectionListKey,
                            macConnectionList);

                    if (!stored) return;

                    await _statusPanelService.SetStatusPanelClearAsync();

                    window.CloseResult = true;
                    window.Close();
                }

                await _statusPanelService.SetStatusPanelMessageAsync("Connection failed...");
            }
            catch (Exception ex)
            {
                await _statusPanelService.SetStatusPanelMessageAsync("Error trying to connect...");

                await UiThreadHelper.EnsureUiThreadAsync(async () =>
                {
                    await _outputService.WriteToOutputAsync(ex.Message, OutputPaneType.Debug);
                });
            }
            finally
            {
                _isTestingConnection = false;
            }
        });
    }

    /// <summary>
    ///     Closes the window.
    /// </summary>
    /// <param name="window">The window to close.</param>
    private void Close(IClosable? window)
    {
        if (window is null) return;

        if (string.IsNullOrEmpty(_macSshFootprint) || _isSshConnectionAttempted)
            _statusPanelService.SetStatusPanelAsync(
                StatusPanelType.Warning,
                "Warning",
                $"Host \'{MacSshHostname}\' couldn't be configured because the user authentication has been canceled");

        window.CloseResult = false;
        window.Close();
    }

    /// <summary>
    ///     Evaluates if the sign-in button should be enabled.
    /// </summary>
    private void EvaluateIfSigninButtonIsEnabled()
    {
        if (string.IsNullOrEmpty(_macSshFootprint) ||
            string.IsNullOrEmpty(MacSshHostname) ||
            string.IsNullOrEmpty(MacSshUsername) ||
            string.IsNullOrEmpty(MacSshPassword))
            IsSshSigninButtonEnabled = false;
        else
            IsSshSigninButtonEnabled = true;
    }

    /// <summary>
    ///     Runs the SSH footprint check asynchronously.
    /// </summary>
    private async Task RunSshFootprintCheckAsync()
    {
        try
        {
            await _statusPanelService.SetStatusPanelAsync(
                StatusPanelType.Operation,
                "Connecting",
                "Checking SSH configuration...");

            _isSshConnectionAttempted = true;

            SshProgressControlVisibility = Visibility.Visible;
            SshStatusImageVisibility = Visibility.Collapsed;
            SshFootPrintTextBlockText = "Retrieving SSH Fingerprint...";
            SshFootprintTextBlockFontWeight = FontWeights.Regular;

            _macSshFootprint = await _secureTransferService.GetSshFingerprintAsync(MacSshHostname);

            if (string.IsNullOrEmpty(_macSshFootprint))
            {
                SshStatusImageVisibility = Visibility.Visible;
                SshSuccessImage = MonikerHelper.GetMonikerImageSource(KnownMonikers.StatusError);
                SshFootPrintTextBlockText = "Couldn't retrieve SSH fingerprint. Please ensure the host" +
                                            " is reachable and Remote Login is enabled";
                SshFootprintTextBlockFontWeight = FontWeights.Bold;
            }
            else
            {
                SshFootPrintTextBlockText = _macSshFootprint;
                SshFootprintTextBlockFontWeight = FontWeights.Regular;
            }

            SshProgressControlVisibility = Visibility.Collapsed;
        }
        catch (Exception ex)
        {
            SshStatusImageVisibility = Visibility.Visible;
            SshSuccessImage = MonikerHelper.GetMonikerImageSource(KnownMonikers.StatusError);
            SshFootPrintTextBlockText = "Couldn't retrieve SSH fingerprint. Please ensure the host" +
                                        " is reachable and Remote Login is enabled";
        }
    }
}