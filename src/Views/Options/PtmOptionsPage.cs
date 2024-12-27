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

// TODO: PtmOptionsPage.cs, add pair to known available Mac hosts

using System.ComponentModel;
using System.Runtime.InteropServices;
using Microsoft.VisualStudio.Shell;
using Ptm.Models.Options;

namespace Ptm.Views.Options;

/// <summary>
///     Represents the options page for the Ptm extension.
/// </summary>
/// <remarks>
///     This class provides a dialog page for configuring options related to Mac connection.
/// </remarks>
[ComVisible(true)]
public class PtmOptionsPage
    : DialogPage
{
    private PtmOptions _options = new();

    [Category("Mac Connection")]
    [DisplayName("Pair To Known Available Mac Hosts")]
    [Description("Automatically pair to known available Mac hosts")]
    public bool PairToKnownAvailableMacHosts
    {
        get => _options.PairToKnownAvailableMacHosts;
        set => _options.PairToKnownAvailableMacHosts = value;
    }
}