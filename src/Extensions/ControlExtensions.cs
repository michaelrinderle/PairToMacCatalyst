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

using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;
using Microsoft.VisualStudio.PlatformUI;
using Ptm.Controls;

namespace Ptm.Extensions;

/// <summary>
///     Provides extension methods for WPF controls.
/// </summary>
public static class ControlExtensions
{
    /// <summary>
    ///     Adds placeholder text to a TextBox control.
    /// </summary>
    /// <param name="textBox">The TextBox control to which the placeholder text will be added.</param>
    /// <param name="placeholderText">The placeholder text to display in the TextBox.</param>
    /// <param name="brush">The Brush to use for the placeholder text. If null, a default brush will be used.</param>
    public static void AddPlaceHolderText(this TextBox textBox, string placeholderText, Brush? brush = null)
    {
        brush ??= (Brush)Application.Current.Resources[EnvironmentColors.SystemMenuTextBrushKey];

        var adornerLayer = AdornerLayer.GetAdornerLayer(textBox);
        if (adornerLayer != null)
        {
            var placeholderAdorner = new PlaceholderAdorner(textBox, placeholderText, brush);

            adornerLayer.Add(placeholderAdorner);
        }

        textBox.TextChanged += (sender, args) =>
        {
            if (adornerLayer != null) adornerLayer.Update();
        };
    }

    /// <summary>
    ///     Adds placeholder text to a PasswordBox control.
    /// </summary>
    /// <param name="passwordBox">The PasswordBox control to which the placeholder text will be added.</param>
    /// <param name="placeholderText">The placeholder text to display in the PasswordBox.</param>
    /// <param name="brush">The Brush to use for the placeholder text. If null, a default brush will be used.</param>
    public static void AddPlaceHolderText(this PasswordBox passwordBox, string placeholderText, Brush? brush = null)
    {
        brush ??= (Brush)Application.Current.Resources[EnvironmentColors.SystemMenuTextBrushKey];

        var adornerLayer = AdornerLayer.GetAdornerLayer(passwordBox);
        if (adornerLayer != null)
        {
            var placeholderAdorner = new PlaceholderAdorner(passwordBox, placeholderText, brush);

            adornerLayer.Add(placeholderAdorner);
        }

        passwordBox.PasswordChanged += (sender, args) =>
        {
            if (adornerLayer != null) adornerLayer.Update();
        };
    }
}