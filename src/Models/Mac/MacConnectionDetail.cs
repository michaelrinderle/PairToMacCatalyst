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
using System.ComponentModel;
using System.Net.NetworkInformation;
using System.Runtime.CompilerServices;

namespace Ptm.Models.Mac;

/// <summary>
/// Represents the details of a connection to a Mac machine.
/// </summary>
/// <remarks>
/// This class provides the hostname and IP address of the Mac machine and checks the availability of the connection.
/// </remarks>
public sealed class MacConnectionDetail
    : INotifyPropertyChanged
{
    private string _hostname;
    private string _ipAddress;
    private bool _isAvailable;
    private bool _isConnected;
    private bool _isProcessing;
    private string _sshFingerprint;

    /// <summary>
    /// Gets or sets the hostname of the Mac machine.
    /// </summary>
    public string Hostname
    {
        get => _hostname;
        set
        {
            if (_hostname == value) return;
            _hostname = value;
            OnPropertyChanged();
        }
    }

    /// <summary>
    /// Gets or sets the IP address of the Mac machine.
    /// </summary>
    public string IpAddress
    {
        get => _ipAddress;
        set
        {
            if (_ipAddress == value) return;
            _ipAddress = value;
            OnPropertyChanged();
        }
    }

    /// <summary>
    /// Gets or sets a value indicating whether the Mac machine is available.
    /// </summary>
    public bool IsAvailable
    {
        get => _isAvailable;
        set
        {
            if (_isAvailable == value) return;
            _isAvailable = value;
            OnPropertyChanged();
        }
    }

    /// <summary>
    /// Gets or sets a value indicating whether the Mac machine is connected.
    /// </summary>
    public bool IsConnected
    {
        get => _isConnected;
        set
        {
            if (_isConnected == value) return;
            _isConnected = value;
            OnPropertyChanged();
        }
    }

    /// <summary>
    /// Gets or sets a value indicating whether the Mac machine is currently being scanned.
    /// </summary>
    public bool IsProcessing
    {
        get => _isProcessing;
        set
        {
            if (_isProcessing == value) return;
            _isProcessing = value;
            OnPropertyChanged();
        }
    }

    /// <summary>
    /// Gets or sets the SSH fingerprint of the Mac machine.
    /// </summary>
    public string SshFingerprint
    {
        get => _sshFingerprint;
        set
        {
            if (_sshFingerprint == value) return;
            _sshFingerprint = value;
            OnPropertyChanged();
        }
    }

    public event PropertyChangedEventHandler PropertyChanged;

    /// <summary>
    /// Initializes a new instance of the <see cref="MacConnectionDetail"/> class.
    /// </summary>
    /// <param name="connection">The Mac connection details.</param>
    /// <param name="connectedHostSshFingerprint"></param>
    public MacConnectionDetail(MacConnection connection, string? connectedHostSshFingerprint = null)
    {
        Hostname = connection.Hostname;
        IpAddress = connection.IpAddress;
        SshFingerprint = connection.SshFingerprint;

        if (!string.IsNullOrEmpty(connectedHostSshFingerprint))
        {
            if (connection.SshFingerprint == connectedHostSshFingerprint)
            {
                IsAvailable = true;
                IsConnected = true;
            }
        }
        else
            CheckAvailability();
    }

    private void OnPropertyChanged([CallerMemberName] string propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    /// <summary>
    /// Checks if the Mac machine is available by pinging its IP address.
    /// </summary>
    public void CheckAvailability()
    {
        try
        {
            var ping = new Ping();
            var reply = ping.Send(IpAddress);
            IsAvailable = reply?.Status == IPStatus.Success;
        }
        catch (Exception ex)
        {
            IsAvailable = false;
            IpAddress = "Host Unreachable";
        }
    }
}