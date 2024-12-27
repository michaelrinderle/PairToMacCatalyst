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

namespace Ptm.Constants;

/// <summary>
///     Contains constant values used for extension settings storage.
/// </summary>
public static class ExtensionConstants
{
    /// <summary>
    ///     Contains constant values related to settings storage.
    /// </summary>
    public static class SettingsStorage
    {
        /// <summary>
        ///     The name of the collection used for storing settings.
        /// </summary>
        public const string CollectionName = "Ptm_C2FA2398-A043-487E-88DC-6712BF767942";

        /// <summary>
        ///     The entropy value used for encryption.
        /// </summary>
        public const string PdEntropy = "A9DC0C0A-0EE5-4FFC-967D-10FB9BA8098F";

        /// <summary>
        ///     The key used for storing the list of Mac connections.
        /// </summary>
        public const string MacConnectionListKey = "MacConnectionList";
    }
}