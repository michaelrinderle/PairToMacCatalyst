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

// TODO: ConnectToMacWindow.xaml.cs, find greyish color for SSH Fingerprint label

using System.ComponentModel.Composition;
using Microsoft.VisualStudio.PlatformUI;
using Ptm.Extensions;
using Ptm.Interfaces;
using Ptm.ViewModels.Windows;

namespace Ptm.Views.Windows;

/// <summary>
///     Represents a window for connecting to a Mac machine via SSH.
/// </summary>
/// <remarks>
///     This window allows the user to input SSH credentials and connect to a Mac.
/// </remarks>
[Export(typeof(ConnectToMacWindow))]
[PartCreationPolicy(CreationPolicy.NonShared)]
public partial class ConnectToMacWindow
    : DialogWindow, IClosable
{
    private readonly ConnectToMacWindowViewModel _viewModel;

    [ImportingConstructor]
    public ConnectToMacWindow(ConnectToMacWindowViewModel viewModel)
    {
        DataContext = _viewModel = viewModel;

        InitializeComponent();

        Loaded += (s, e) =>
        {
            MacSshUsernameTextBox!.AddPlaceHolderText("Username");
            MacSshPasswordPasswordBox!.AddPlaceHolderText("Password");
        };
    }

    /// <summary>
    ///     Gets or sets the result of the close operation.
    /// </summary>
    public bool? CloseResult { get; set; }

    /// <summary>
    ///     Gets or sets the result object.
    /// </summary>
    public object Result { get; set; }

    /// <summary>
    ///     Loads the specified objects into the window.
    /// </summary>
    /// <param name="objects">The objects to load.</param>
    public void Load(object[] objects)
    {
        _viewModel.Load(objects);
    }
}