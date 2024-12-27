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

using System.ComponentModel.Composition;
using System.Threading.Tasks;
using Ptm.Enums;
using Ptm.Interfaces;
using Ptm.ViewModels.Windows;

namespace Ptm.Services;

/// <inheritdoc />
[Export(typeof(IStatusPanelService))]
[PartCreationPolicy(CreationPolicy.Shared)]
public class StatusPanelService
    : IStatusPanelService
{
    /// <inheritdoc />
    public async Task SetStatusPanelClearAsync()
    {
        await PairToMacWindowViewModel.SetStatusPanelClearAsync(null);
    }

    /// <inheritdoc />
    public async Task SetStatusPanelAsync(StatusPanelType statusType, string title, string message)
    {
        await PairToMacWindowViewModel.SetStatusPanelAsync(null, statusType, title, message);
    }

    /// <inheritdoc />
    public async Task SetStatusPanelMessageAsync(string message)
    {
        await PairToMacWindowViewModel.SetStatusPanelMessageAsync(null, message);
    }

    /// <inheritdoc />
    public async Task SetStatusPanelProgressAsync(int progress)
    {
        await PairToMacWindowViewModel.SetStatusPanelProgressAsync(null, progress);
    }

    /// <inheritdoc />
    public async Task SetStatusPanelTitleAsync(string title)
    {
        await PairToMacWindowViewModel.SetStatusPanelTitleAsync(null, title);
    }
}