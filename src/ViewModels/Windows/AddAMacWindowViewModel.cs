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

using System.ComponentModel;
using System.ComponentModel.Composition;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using Ptm.Controls;
using Ptm.Interfaces;

namespace Ptm.ViewModels.Windows;

/// <summary>
///     ViewModel for Adding a Mac window.
/// </summary>
[Export(typeof(AddAMacWindowViewModel))]
[PartCreationPolicy(CreationPolicy.NonShared)]
[method: ImportingConstructor]
public class AddAMacWindowViewModel
    : INotifyPropertyChanged
{
    private bool _isAddButtonEnabled;
    private string? _macHostNameOrIp;

    /// <summary>
    ///     Gets or sets a value indicating whether the Add button is enabled.
    /// </summary>
    public bool IsAddButtonEnabled
    {
        get => _isAddButtonEnabled;
        set
        {
            _isAddButtonEnabled = value;
            OnPropertyChanged();
        }
    }

    /// <summary>
    ///     Gets or sets the Mac host name or IP address.
    /// </summary>
    public string? MacHostNameOrIp
    {
        get => _macHostNameOrIp;
        set
        {
            _macHostNameOrIp = value;
            OnPropertyChanged();
            IsAddButtonEnabled = !string.IsNullOrWhiteSpace(_macHostNameOrIp);
        }
    }

    /// <summary>
    ///     Gets the command to add a Mac.
    /// </summary>
    public RelayCommand<IClosable> AddAMacCommand => new(AddMac);

    /// <summary>
    ///     Gets the command to close the window.
    /// </summary>
    public RelayCommand<IClosable> CloseCommand => new(Close);

    /// <summary>
    ///     Gets the command to handle hyperlink requests.
    /// </summary>
    public RelayCommand<string> HyperlinkCommand => new(HyperlinkRequest);

    /// <summary>
    ///     Occurs when a property value changes.
    /// </summary>
    public event PropertyChangedEventHandler? PropertyChanged;

    /// <summary>
    ///     Raises the <see cref="PropertyChanged" /> event.
    /// </summary>
    /// <param name="propertyName">Name of the property that changed.</param>
    protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    /// <summary>
    ///     Adds a Mac and closes the window.
    /// </summary>
    /// <param name="window">The window to close.</param>
    private void AddMac(IClosable? window)
    {
        if (window is null) return;

        window.CloseResult = true;
        window.Result = MacHostNameOrIp;
        window.Close();
    }

    /// <summary>
    ///     Closes the window.
    /// </summary>
    /// <param name="window">The window to close.</param>
    private static void Close(IClosable? window)
    {
        if (window is null) return;

        window.CloseResult = false;
        window.Close();
    }

    /// <summary>
    ///     Handles hyperlink requests.
    /// </summary>
    /// <param name="uri">The URI to open.</param>
    private static void HyperlinkRequest(string? uri)
    {
        if (uri is not null)
            Process.Start(new ProcessStartInfo(uri) { UseShellExecute = true });
    }
}