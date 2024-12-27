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

using System.Threading.Tasks;
using Ptm.Enums;

namespace Ptm.Interfaces;

/// <summary>
///     Defines methods for writing to, clearing, and retrieving output text.
/// </summary>
public interface IOutputService
{
    /// <summary>
    ///     Clears the output asynchronously.
    /// </summary>
    /// <param name="outputType">The type of output to clear.</param>
    /// <returns>A task that represents the asynchronous clear operation.</returns>
    Task ClearOutputAsync(OutputPaneType outputType);

    /// <summary>
    ///     Clears the status bar asynchronously.
    /// </summary>
    /// <returns>A task that represents the asynchronous clear operation.</returns>
    Task ClearStatusBarAsync();

    /// <summary>
    ///     Retrieves the output text asynchronously.
    /// </summary>
    /// <param name="outputType">The type of output to retrieve.</param>
    /// <returns>A task that represents the asynchronous retrieval operation. The task result contains the output text.</returns>
    Task<string> GetOutputTextAsync(OutputPaneType outputType);

    /// <summary>
    ///     Writes a message to the output asynchronously.
    /// </summary>
    /// <param name="message">The message to write.</param>
    /// <param name="outputType">The type of output.</param>
    /// <returns>A task that represents the asynchronous write operation.</returns>
    Task WriteToOutputAsync(string message, OutputPaneType outputType);

    /// <summary>
    ///     Writes a message to the status bar asynchronously.
    /// </summary>
    /// <param name="message">The message to write to the status bar.</param>
    /// <returns>A task that represents the asynchronous write operation.</returns>
    Task WriteToStatusBarAsync(string message);
}