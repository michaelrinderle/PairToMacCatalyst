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
/// Contains constants for various shell commands used in the application.
/// </summary>
public static class ShellCommandConstants
{
    // chmod commands
    public const string ChmodVsDbg = "chmod +x ~/vsdbg/vsdbg";

    // get commands
    public const string GetActivePublicIp = "\"ifconfig | grep 'inet ' | awk '{print $2}' | grep -v 127.0.0.1\"";
    public const string GetAppleDeveloperCert = "security find-identity -v -p codesigning | grep \"Mac Developer\"";
    public const string GetAppleProvisioningProfiles = @"ls -1 ~/Library/MobileDevice/'Provisioning Profiles'/*.provisionprofile";
    public const string GetDotnetRuntimes = "dotnet --list-runtimes";
    public const string GetDotnetSdks = "dotnet --list-sdks";
    public const string GetDotnetVersion = "dotnet --version";
    public const string GetDotnetWorkloads = "dotnet workload list";
    public const string GetHomeDirectory = "echo $HOME";
    public const string GetHostname = "hostname";
    public const string GetMacOsVersion = "sw_vers -productVersion";
    public const string GetPkgList = "pkgutil --pkgs";
    public const string GetVsDbgPid = "ps -A | grep './vsdbg --interpreter=vscode' | grep -v grep | awk '{print $1}'";
    public const string GetXcodeSelectCli = "xcode-select -p";
    public const string GetXcodeVersion = "xcodebuild -version";

    // install commands
    public const string InstallVsDbg = "curl -sSL https://aka.ms/getvsdbgsh | bash /dev/stdin -v latest -l "; // ~/.ptm/vsdbg";

    // read commands
    public const string ReadAppleProvisionProfiles = "security cms -D -i";
    public const string ReadXcodeLicenseStatus = "defaults read /Library/Preferences/com.apple.dt.Xcode";

    // run commands
    public const string StartVsDbg = "--interpreter=vscode --pauseEngineForDebugger > /Users/michael/.ptm/vsdbg/vsdbg.log 2>&1 &";

    // shell commands
    public const string ShellProfileSources = "source /etc/profile; source /etc/bashrc; [ -f ~/.bashrc ] && source ~/.bashrc; [ -f ~/.bash_profile ] && source ~/.bash_profile; [ -f ~/.profile ] && source ~/.profile;";
}