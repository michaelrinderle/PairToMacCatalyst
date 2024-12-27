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

using System.Threading.Tasks;

namespace Ptm.Interfaces;

/// <summary>
///     Interface for secure data storage services.
/// </summary>
/// <remarks>
///     This interface provides methods to save, retrieve, and remove data securely.
/// </remarks>
public interface ISecureStorageService
{
    /// <summary>
    ///     Saves data asynchronously.
    /// </summary>
    /// <typeparam name="T">The type of the data to save.</typeparam>
    /// <param name="key">The key under which the data will be saved.</param>
    /// <param name="value">The data to save.</param>
    /// <returns>
    ///     A task that represents the asynchronous save operation. The task result contains a boolean indicating whether
    ///     the save was successful.
    /// </returns>
    Task<bool> SaveDataAsync<T>(string key, T value);

    /// <summary>
    ///     Retrieves data asynchronously.
    /// </summary>
    /// <typeparam name="T">The type of the data to retrieve.</typeparam>
    /// <param name="key">The key under which the data is saved.</param>
    /// <returns>
    ///     A task that represents the asynchronous retrieve operation. The task result contains the retrieved data or
    ///     null if the data does not exist.
    /// </returns>
    Task<T?> GetDataAsync<T>(string key);

    /// <summary>
    ///     Removes data asynchronously.
    /// </summary>
    /// <param name="key">The key under which the data is saved.</param>
    /// <returns>
    ///     A task that represents the asynchronous remove operation. The task result contains a boolean indicating
    ///     whether the remove was successful.
    /// </returns>
    Task<bool> RemoveDataAsync(string key);
}