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
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.Settings;
using Microsoft.VisualStudio.Shell.Settings;
using Newtonsoft.Json;
using Ptm.Enums;
using Ptm.Interfaces;
using StorageConstants = Ptm.Constants.ExtensionConstants.SettingsStorage;

namespace Ptm.Services;

/// <inheritdoc />
[Export(typeof(ISecureStorageService))]
[PartCreationPolicy(CreationPolicy.Shared)]
public class SecureStorageService
    : ISecureStorageService
{
    private readonly WritableSettingsStore _extensionSettingsStore;
    private readonly IOutputService _outputService;

    [ImportingConstructor]
    public SecureStorageService(IOutputService outputService)
    {
        _outputService = outputService;

        var settingsManager = new ShellSettingsManager(PtmPackage.Instance);
        _extensionSettingsStore = settingsManager.GetWritableSettingsStore(SettingsScope.UserSettings);

        if (!_extensionSettingsStore.CollectionExists(StorageConstants.CollectionName))
            _extensionSettingsStore.CreateCollection(StorageConstants.CollectionName);
    }

    /// <inheritdoc />
    public async Task<bool> SaveDataAsync<T>(string key, T value)
    {
        try
        {
            var json = JsonConvert.SerializeObject(value);
            var jsonBytes = Encoding.UTF8.GetBytes(json);
            var encryptedData = ProtectedData.Protect(
                jsonBytes,
                Encoding.Unicode.GetBytes(StorageConstants.PdEntropy),
                DataProtectionScope.CurrentUser);
            var encryptedDataBase64 = Convert.ToBase64String(encryptedData);

            _extensionSettingsStore.SetString(
                StorageConstants.CollectionName,
                key,
                encryptedDataBase64);

            return true;
        }
        catch (Exception ex)
        {
            await _outputService.WriteToOutputAsync("Error saving to settings storage", OutputPaneType.Debug);
        }

        return false;
    }

    /// <inheritdoc />
    public async Task<T?> GetDataAsync<T>(string key)
    {
        try
        {
            var encryptedDataBase64 = _extensionSettingsStore.GetString(StorageConstants.CollectionName, key);

            if (string.IsNullOrEmpty(encryptedDataBase64)) return default;

            var encryptedData = Convert.FromBase64String(encryptedDataBase64);

            var decryptedData = ProtectedData.Unprotect(
                encryptedData,
                Encoding.Unicode.GetBytes(StorageConstants.PdEntropy),
                DataProtectionScope.CurrentUser);

            var json = Encoding.UTF8.GetString(decryptedData);

            return JsonConvert.DeserializeObject<T>(json);
        }
        catch (Exception ex)
        {
            await _outputService.WriteToOutputAsync("Error getting from settings storage", OutputPaneType.Debug);
        }

        return default;
    }

    /// <inheritdoc />
    public async Task<bool> RemoveDataAsync(string key)
    {
        try
        {
            return _extensionSettingsStore.DeleteProperty(
                StorageConstants.CollectionName,
                key);
        }
        catch (Exception ex)
        {
            await _outputService.WriteToOutputAsync("Error removing property from settings storage",
                OutputPaneType.Debug);
        }

        return false;
    }
}