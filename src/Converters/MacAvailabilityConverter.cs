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

using System;
using System.Globalization;
using System.Windows.Data;

namespace Ptm.Converters;

/// <summary>
///     Converts the availability status and IP address to a displayable string.
/// </summary>
public class MacAvailabilityConverter
    : IMultiValueConverter
{
    /// <summary>
    ///     Converts the availability status and IP address to a displayable string.
    /// </summary>
    /// <param name="values">An array of values that contains the availability status and IP address.</param>
    /// <param name="targetType">The type of the binding target property.</param>
    /// <param name="parameter">The converter parameter to use.</param>
    /// <param name="culture">The culture to use in the converter.</param>
    /// <returns>A string representing the IP address if available, otherwise "Host Unreachable".</returns>
    public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
    {
        if (values.Length != 3)
            return "Host Unreachable";

        var isAvailable = (bool)values[0];
        var ipAddress = (string)values[1];
        var isProcessing = (bool)values[2];

        if (isProcessing)
            return "Checking Host...";

        return isAvailable ? ipAddress : "Host Unreachable";
    }

    /// <summary>
    ///     Not implemented. Throws a <see cref="NotImplementedException" />.
    /// </summary>
    /// <param name="value">The value that is produced by the binding target.</param>
    /// <param name="targetTypes">The array of types to convert to.</param>
    /// <param name="parameter">The converter parameter to use.</param>
    /// <param name="culture">The culture to use in the converter.</param>
    /// <returns>Throws a <see cref="NotImplementedException" />.</returns>
    /// <exception cref="NotImplementedException">Always thrown.</exception>
    public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}