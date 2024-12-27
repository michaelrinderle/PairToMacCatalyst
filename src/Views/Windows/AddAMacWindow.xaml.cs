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
using System.ComponentModel.Composition;
using Microsoft.VisualStudio.PlatformUI;
using Ptm.Extensions;
using Ptm.Interfaces;
using Ptm.ViewModels.Windows;

namespace Ptm.Views.Windows;

/// <summary>
///     Represents a window for adding a Mac address.
/// </summary>
[Export(typeof(AddAMacWindow))]
[PartCreationPolicy(CreationPolicy.NonShared)]
public partial class AddAMacWindow
    : DialogWindow, IClosable
{
    private readonly AddAMacWindowViewModel _viewModel;

    /// <summary>
    ///     Initializes a new instance of the <see cref="AddAMacWindow" /> class.
    /// </summary>
    /// <param name="viewModel">The view model for the window.</param>
    [ImportingConstructor]
    public AddAMacWindow(AddAMacWindowViewModel viewModel)
    {
        DataContext = _viewModel = viewModel;

        InitializeComponent();

        Loaded += (s, e) => { MacAddressTextBox.AddPlaceHolderText("0.0.0.0"); };
    }

    /// <summary>
    ///     Gets or sets the result of the close operation.
    /// </summary>
    public bool? CloseResult { get; set; }

    /// <summary>
    ///     Gets or sets the result object.
    /// </summary>
    public object? Result { get; set; }

    /// <summary>
    ///     Loads the specified objects into the window.
    /// </summary>
    /// <param name="objects">The objects to load.</param>
    public void Load(object[] objects)
    {
        throw new NotImplementedException();
    }
}