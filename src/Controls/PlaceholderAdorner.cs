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

using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;
using Ptm.Extensions;

namespace Ptm.Controls;

/// <summary>
///     Represents an adorner that displays a placeholder text on a TextBox when it is empty.
/// </summary>
public class PlaceholderAdorner
    : Adorner
{
    private readonly TextBlock _placeholderTextBlock;

    /// <summary>
    ///     Initializes a new instance of the <see cref="PlaceholderAdorner" /> class.
    /// </summary>
    /// <param name="adornedElement">The element to bind the adorner to.</param>
    /// <param name="placeholderText">The placeholder text to display.</param>
    /// <param name="foregroundColor">The color of the placeholder text. If null, the default color is DimGray.</param>
    public PlaceholderAdorner(UIElement adornedElement, string placeholderText, Brush? foregroundColor = null)
        : base(adornedElement)
    {
        _placeholderTextBlock = new TextBlock
        {
            FontWeight = FontWeights.Regular,
            Text = placeholderText,
            Foreground = foregroundColor ?? Brushes.DimGray,
            Margin = new Thickness(5, 0, 0, 0),
            VerticalAlignment = VerticalAlignment.Center
        };

        IsHitTestVisible = false;
    }

    /// <summary>
    ///     Renders the placeholder text if the adorned element is a TextBox and is empty.
    /// </summary>
    /// <param name="drawingContext">
    ///     The drawing instructions for a specific element. This context is provided to the layout
    ///     system.
    /// </param>
    protected override void OnRender(DrawingContext drawingContext)
    {
        if (AdornedElement is TextBox textBox && textBox.Text != string.Empty) return;
        if (AdornedElement is PasswordBox passwordBox && passwordBox.Password != string.Empty) return;

        var adornedElementRect = new Rect(AdornedElement.RenderSize);
        var text = new FormattedText(
            _placeholderTextBlock.Text,
            CultureInfo.CurrentCulture,
            FlowDirection.LeftToRight,
            new Typeface(_placeholderTextBlock.FontFamily, _placeholderTextBlock.FontStyle,
                _placeholderTextBlock.FontWeight, _placeholderTextBlock.FontStretch),
            _placeholderTextBlock.FontSize,
            _placeholderTextBlock.Foreground);

        var textLocation = new Point(adornedElementRect.Left + _placeholderTextBlock.Margin.Left,
            adornedElementRect.Top + (adornedElementRect.Height - text.Height) / 2);

        drawingContext.DrawText(text, textLocation);
    }

    /// <summary>
    ///     Adds a placeholder text to a TextBox.
    /// </summary>
    /// <param name="textBox">The TextBox to add the placeholder to.</param>
    /// <param name="placeHolder">The placeholder text to display.</param>
    public static void AddPlaceHolder(TextBox textBox, string placeHolder)
    {
        textBox.AddPlaceHolderText(placeHolder);
    }

    /// <summary>
    ///     Adds a placeholder text to a PasswordBox.
    /// </summary>
    /// <param name="passwordBox">The PasswordBox to add the placeholder to.</param>
    /// <param name="placeHolder">The placeholder text to display.</param>
    public static void AddPlaceHolder(PasswordBox passwordBox, string placeHolder)
    {
        passwordBox.AddPlaceHolderText(placeHolder);
    }
}