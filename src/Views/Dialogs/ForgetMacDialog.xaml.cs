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

using System.Windows;
using Microsoft.VisualStudio.PlatformUI;

namespace Ptm.Views.Dialogs;

/// <summary>
///     Interaction logic for ForgetMacDialog.xaml
/// </summary>
public partial class ForgetMacDialog
    : DialogWindow
{
    public ForgetMacDialog(string hostname, string ipAddress)
    {
        InitializeComponent();

        MacAddress.Content = $"{hostname} ({ipAddress})";
    }

    /// <summary>
    ///     Handles the click event for the Close button.
    ///     Sets the DialogResult to false and closes the dialog.
    /// </summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">The event data.</param>
    private void Close_OnClick(object sender, RoutedEventArgs e)
    {
        DialogResult = false;
        Close();
    }

    /// <summary>
    ///     Handles the click event for the Confirm button.
    ///     Sets the DialogResult to true and closes the dialog.
    /// </summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">The event data.</param>
    private void Confirm_OnClick(object sender, RoutedEventArgs e)
    {
        DialogResult = true;
        Close();
    }
}