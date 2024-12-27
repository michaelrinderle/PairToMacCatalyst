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

namespace Ptm.Interfaces;

/// <summary>
///     Defines methods and properties for a closable dialog window.
/// </summary>
public interface IClosable
{
    /// <summary>
    ///     Gets or sets the result of the close operation.
    /// </summary>
    bool? CloseResult { get; set; }

    /// <summary>
    ///     Gets or sets the result object.
    /// </summary>
    object? Result { get; set; }

    /// <summary>
    ///     Closes the object.
    /// </summary>
    void Close();

    /// <summary>
    ///     Loads the specified objects into the closable object.
    /// </summary>
    /// <param name="objects">The objects to load.</param>
    void Load(object[] objects);
}