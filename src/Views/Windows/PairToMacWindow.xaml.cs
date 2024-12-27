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
using Ptm.Interfaces;
using Ptm.ViewModels.Windows;

namespace Ptm.Views.Windows;


/// <summary>
/// Represents a window that allows pairing to a Mac.
/// </summary>
/// <remarks>
/// This window is part of the MEF (Managed Extensibility Framework) composition and is created with non-shared part creation policy.
/// </remarks>
[Export(typeof(PairToMacWindow))]
[PartCreationPolicy(CreationPolicy.NonShared)]
public partial class PairToMacWindow
    : DialogWindow, IClosable
{
    private readonly PairToMacWindowViewModel _viewModel;

    [ImportingConstructor]
    public PairToMacWindow(PairToMacWindowViewModel viewModel)
    {
        DataContext = _viewModel = viewModel;
        InitializeComponent();

        MouseLeave += _viewModel.PairToMacWindow_MouseLeave;

        new Action(async () =>
        {
            // mef composition is shared, need to reload on window creation
            await _viewModel.LoadViewModelAsync();
        })();
    }

    /// <summary>
    /// Finalizes an instance of the <see cref="PairToMacWindow"/> class.
    /// </summary>
    ~PairToMacWindow()
    {
        MouseLeave -= _viewModel.PairToMacWindow_MouseLeave;
    }

    /// <inheritdoc/>
    public bool? CloseResult { get; set; }

    /// <inheritdoc/>
    public object? Result { get; set; }

    /// <inheritdoc/>
    public void Load(object[] objects)
    {
        throw new NotImplementedException();
    }
}