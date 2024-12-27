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
using System.Windows.Controls;

namespace Ptm.Controls;

/// <summary>
///     Provides attached properties for binding a PasswordBox's password.
/// </summary>
public static class PasswordBoxHelper
{
    private static bool _isUpdating;

    /// <summary>
    ///     Identifies the BoundPassword attached dependency property.
    /// </summary>
    public static readonly DependencyProperty BoundPassword =
        DependencyProperty.RegisterAttached(
            "BoundPassword",
            typeof(string),
            typeof(PasswordBoxHelper),
            new PropertyMetadata(string.Empty, OnBoundPasswordChanged));

    /// <summary>
    ///     Gets the bound password.
    /// </summary>
    /// <param name="dp">The dependency object.</param>
    /// <returns>The bound password.</returns>
    public static string GetBoundPassword(DependencyObject dp)
    {
        return (string)dp.GetValue(BoundPassword);
    }

    /// <summary>
    ///     Sets the bound password.
    /// </summary>
    /// <param name="dp">The dependency object.</param>
    /// <param name="value">The password value.</param>
    public static void SetBoundPassword(DependencyObject dp, string value)
    {
        dp.SetValue(BoundPassword, value);
    }

    private static void OnBoundPasswordChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is PasswordBox passwordBox)
        {
            passwordBox.PasswordChanged -= PasswordBox_PasswordChanged;

            if (!_isUpdating) passwordBox.Password = (string)e.NewValue;

            passwordBox.PasswordChanged += PasswordBox_PasswordChanged;
        }
    }

    private static void PasswordBox_PasswordChanged(object sender, RoutedEventArgs e)
    {
        if (sender is PasswordBox passwordBox)
        {
            _isUpdating = true;
            SetBoundPassword(passwordBox, passwordBox.Password);
            _isUpdating = false;
        }
    }
}