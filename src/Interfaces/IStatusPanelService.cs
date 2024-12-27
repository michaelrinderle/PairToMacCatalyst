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

using Ptm.Enums;
using System.Threading.Tasks;

namespace Ptm.Interfaces;

/// <summary>
/// Interface for managing the pair to mac status panel in the application.
/// Provides methods to clear the status panel, set status with type, title, and message,
/// update the status message, update the progress, and update the title.
/// </summary>
public interface IStatusPanelService
{
    /// <summary>
    /// Clears the status panel.
    /// </summary>
    Task SetStatusPanelClearAsync();

    /// <summary>
    /// Sets the status panel with the specified status type, title, and message.
    /// </summary>
    /// <param name="statusType">The type of the status.</param>
    /// <param name="title">The title of the status.</param>
    /// <param name="message">The message of the status.</param>
    Task SetStatusPanelAsync(StatusPanelType statusType, string title, string message);

    /// <summary>
    /// Updates the status panel message.
    /// </summary>
    /// <param name="message">The new message to display.</param>
    Task SetStatusPanelMessageAsync(string message);

    /// <summary>
    /// Updates the progress on the status panel.
    /// </summary>
    /// <param name="progress">The progress value to set.</param>
    Task SetStatusPanelProgressAsync(int progress);

    /// <summary>
    /// Updates the title of the status panel.
    /// </summary>
    /// <param name="title">The new title to display.</param>
    Task SetStatusPanelTitleAsync(string title);
}