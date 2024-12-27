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

using System.Runtime.InteropServices;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Microsoft.VisualStudio.Imaging.Interop;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;

namespace Ptm.Helpers;

/// <summary>
///     Provides helper methods for working with image monikers.
/// </summary>
public static class MonikerHelper
{
    /// <summary>
    ///     Retrieves a <see cref="BitmapSource" /> for the specified <see cref="ImageMoniker" />.
    /// </summary>
    /// <param name="moniker">The image moniker to retrieve the image for.</param>
    /// <param name="height">The height of the image. Default is 16.</param>
    /// <param name="width">The width of the image. Default is 16.</param>
    /// <returns>A <see cref="BitmapSource" /> representing the image moniker.</returns>
    public static BitmapSource GetMonikerImageSource(ImageMoniker moniker, int height = 16, int width = 16,
        uint color = 0xFFFFFFFF)
    {
        ThreadHelper.ThrowIfNotOnUIThread();

        var imageService = Package.GetGlobalService(typeof(SVsImageService)) as IVsImageService2;
        var imageAttributes = new ImageAttributes
        {
            StructSize = Marshal.SizeOf(typeof(ImageAttributes)), // Ensure StructSize is set correctly
            ImageType = (uint)_UIImageType.IT_Bitmap,
            Format = (uint)_UIDataFormat.DF_WPF,
            LogicalWidth = height,
            LogicalHeight = width,
            Flags = (uint)_ImageAttributesFlags.IAF_RequiredFlags,
            Background = color // Set the background color
        };

        var image = imageService.GetImage(moniker, imageAttributes);
        image.get_Data(out var data);

        var bitmapSource = data as BitmapSource;
        if (bitmapSource != null)
        {
            var colorConvertedBitmap = new FormatConvertedBitmap();
            colorConvertedBitmap.BeginInit();
            colorConvertedBitmap.Source = bitmapSource;
            colorConvertedBitmap.DestinationFormat = PixelFormats.Bgra32;
            colorConvertedBitmap.EndInit();

            return colorConvertedBitmap;
        }

        return null;
    }
}